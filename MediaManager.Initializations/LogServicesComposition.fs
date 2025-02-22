namespace MediaManager.Initializations

open MediaManager.Types.DatabaseContextTypes
open MediaManager.Services

module LogServicesComposition =
    let passResultLogError
      (getDbConnection: unit -> Result<TDatabaseConnection, exn>)
      (inboundResult: Result<'T, exn>)
      : Result<'T, exn> =
        LogServices.passResultLogError
            TextLogComposition.writeLog
            getDbConnection
            DatabaseContextComposition.getLogCollection
            inboundResult
