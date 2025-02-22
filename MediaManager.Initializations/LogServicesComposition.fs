namespace MediaManager.Initializations

open MediaManager.Services

module LogServicesComposition =
    let getLogs startDateTime endDateTime logType =
        LogServices.getLogs startDateTime endDateTime logType

    let insertNewLog getDbConnection logType message variables exOption =
        LogServices.insertNewLog
            TextLogComposition.writeLog
            getDbConnection
            DatabaseContextComposition.getLogCollection
            logType
            message
            variables
            exOption

    let passResultLogError getDbConnection inboundResult =
        LogServices.passResultLogError
            TextLogComposition.writeLog
            getDbConnection
            DatabaseContextComposition.getLogCollection
            inboundResult
