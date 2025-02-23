namespace MediaManager.Initializations

open MediaManager.Services

module ChannelServicesComposition =
    let getAll() =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        ChannelServices.getAll
            getDatabaseConnection
            DatabaseContextComposition.getChannelCollection
        |> LogServicesComposition.passResultLogError getDatabaseConnection

    let getBySiteId siteId =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        ChannelServices.getBySiteId
            getDatabaseConnection
            DatabaseContextComposition.getChannelCollection
            siteId
        |> LogServicesComposition.passResultLogError getDatabaseConnection

    let getManyBySiteIds siteIds =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        ChannelServices.getManyBySiteIds
            getDatabaseConnection
            DatabaseContextComposition.getChannelCollection
            siteIds
        |> LogServicesComposition.passResultLogError getDatabaseConnection

    let getNotBacklisted() =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        ChannelServices.getNotBacklisted
            getDatabaseConnection
            DatabaseContextComposition.getChannelCollection
        |> LogServicesComposition.passResultLogError getDatabaseConnection

    let getManyByWordInName wordInChannelName =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        ChannelServices.getManyByWordInName
            getDatabaseConnection
            DatabaseContextComposition.getChannelCollection
            wordInChannelName
        |> LogServicesComposition.passResultLogError getDatabaseConnection

    let insertOrUpdate channels =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        ChannelServices.insertOrUpdate
            getDatabaseConnection
            DatabaseContextComposition.getChannelCollection
            channels
        |> LogServicesComposition.passResultLogError getDatabaseConnection

    let updateLastCheckedOutAndActivity channels =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        ChannelServices.updateLastCheckedOutAndActivity
            getDatabaseConnection
            DatabaseContextComposition.getChannelCollection
            channels
        |> LogServicesComposition.passResultLogError getDatabaseConnection

    let updateBlackListStatus channels =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        ChannelServices.updateBlackListStatus
            (DatabaseContextComposition.createGetDatabaseConnection())
            DatabaseContextComposition.getChannelCollection
            channels
        |> LogServicesComposition.passResultLogError getDatabaseConnection