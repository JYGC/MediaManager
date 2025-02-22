namespace MediaManager.Services

open System
open System.Collections.Generic

open OPMF.Entities
open MediaManager.Types.DatabaseContextTypes

module LogServices =
    let getLogs
      (getDbConnection: unit -> Result<TDatabaseConnection, exn>)
      (getLogCollection: TDatabaseConnection -> TLogCollection)
      (startDateTime: DateTime)
      (endDateTime: DateTime)
      (logType: OPMFLogType)
      : Result<ResizeArray<OPMFLog>, exn> =
        match getDbConnection() with
        | Ok dbConnection ->
            try Ok (
                getLogCollection(dbConnection).Query().Where(fun i ->
                    i.Type = logType &&
                    i.DateCreated > startDateTime &&
                    i.DateCreated < endDateTime).ToList())
            with e -> Error e
        | Error ex -> Error ex

    let private _makeNewLog
      (logType: OPMFLogType)
      (overrideMessageOption: string option)
      (variableOptions: Dictionary<string, obj> option)
      (exOption: exn option)
      : OPMFLog =
        let newLog = new OPMFLog()
        match exOption with
        | Some ex ->
            newLog.Message <- ex.Message
            newLog.ExceptionObject <- ex.ToString()
        | None -> ()
        match overrideMessageOption with
        | Some overrideMessage -> newLog.Message <- overrideMessage
        | None -> ()
        newLog.Type <- logType
        newLog.Variables <-
            match variableOptions with
            | Some variables -> variables
            | None -> new Dictionary<string, obj>()
        newLog

    let insertNewLog
      (writeLog: string -> unit)
      (getDbConnection: unit -> Result<TDatabaseConnection, exn>)
      (getLogCollection: TDatabaseConnection -> TLogCollection)
      (logType: OPMFLogType)
      (message: string)
      (variables: Dictionary<string, obj>)
      (exOption: exn option)
      : unit =
        try
            let newLog = _makeNewLog logType (Some message) (Some variables) exOption
            match getDbConnection() with
            | Ok dbConnection ->
                try
                    (getLogCollection dbConnection).Insert([newLog]) |> ignore
                    dbConnection.Commit() |> ignore
                with e ->
                    dbConnection.Rollback() |> ignore
                    raise e
            | Error ed -> raise ed
        with ex ->
            writeLog (ex.ToString())

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
                let newLog = _makeNewLog OPMFLogType.Error None None (Some re)
                match getDbConnection() with
                | Ok dbConnection ->
                    try
                        (getLogCollection dbConnection).Insert([newLog]) |> ignore
                        dbConnection.Commit() |> ignore
                    with e ->
                        dbConnection.Rollback() |> ignore
                        raise e
                | Error ed -> raise ed
            with ex ->
                writeLog (ex.ToString())
                writeLog (re.ToString())
        inboundResult