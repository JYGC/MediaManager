namespace MediaManager.Services

open System.Collections.Generic

open OPMF.Entities
open MediaManager.Types.DatabaseContextTypes

module LogServices =
    let private _makeNewLog (logType: OPMFLogType) (variableOptions: Dictionary<string, obj> option) (ex: exn) =
        let newLog = new OPMFLog()
        newLog.Message <- ex.Message
        newLog.Type <- logType
        newLog.Variables <-
            match variableOptions with
            | Some variables -> variables
            | None -> new Dictionary<string, obj>()
        newLog.ExceptionObject <- ex.ToString()
        newLog

    let passResultLogError<'T>
      (writeLog: string -> unit)
      (getDbConnection: unit -> Result<TDatabaseConnection, exn>)
      (getLogCollection: TDatabaseConnection -> TLogCollection)
      (inboundResult: Result<'T, exn>)
      : Result<'T, exn> =
        match inboundResult with
        | Ok value -> value |> ignore
        | Error re ->
            try
                let newLog = _makeNewLog OPMFLogType.Error None re
                match getDbConnection() with
                | Ok dbConnection ->
                    try
                        getLogCollection(dbConnection).Insert([newLog]) |> ignore
                        dbConnection.Commit() |> ignore
                    with e ->
                        dbConnection.Rollback() |> ignore
                        raise e
                | Error ed -> raise ed
            with ex ->
                writeLog(ex.ToString())
                writeLog(re.ToString())
        inboundResult