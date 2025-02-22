namespace MediaManager.Initializations

open MediaManager.Services

module ChannelServicesComposition =
    let getAll() =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        ChannelServices.getAll
            getDatabaseConnection
            DatabaseContextComposition.getChannelCollection
        |> LogServicesComposition.passResultLogError getDatabaseConnection
    let GetAll() = getAll()

    let getBySiteId siteId =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        ChannelServices.getBySiteId
            getDatabaseConnection
            DatabaseContextComposition.getChannelCollection
            siteId
        |> LogServicesComposition.passResultLogError getDatabaseConnection
    let GetBySiteId siteId = getBySiteId siteId

    let getNotBacklisted() =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        ChannelServices.getNotBacklisted
            getDatabaseConnection
            DatabaseContextComposition.getChannelCollection
        |> LogServicesComposition.passResultLogError getDatabaseConnection
    let GetNotBacklisted() = getNotBacklisted()

    let getManyByWordInName wordInChannelName =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        ChannelServices.getManyByWordInName
            getDatabaseConnection
            DatabaseContextComposition.getChannelCollection
            wordInChannelName
        |> LogServicesComposition.passResultLogError getDatabaseConnection
    let GetManyByWordInName wordInChannelName = getManyByWordInName wordInChannelName

    let insertOrUpdate channels =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        ChannelServices.insertOrUpdate
            getDatabaseConnection
            DatabaseContextComposition.getChannelCollection
            channels
        |> LogServicesComposition.passResultLogError getDatabaseConnection
    let InsertOrUpdate channels = insertOrUpdate channels

    let updateLastCheckedOutAndActivity channels =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        ChannelServices.updateLastCheckedOutAndActivity
            getDatabaseConnection
            DatabaseContextComposition.getChannelCollection
            channels
        |> LogServicesComposition.passResultLogError getDatabaseConnection
    let UpdateLastCheckedOutAndActivity channels = updateLastCheckedOutAndActivity channels

    let updateBlackListStatus channels =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        ChannelServices.updateBlackListStatus
            (DatabaseContextComposition.createGetDatabaseConnection())
            DatabaseContextComposition.getChannelCollection
            channels
        |> LogServicesComposition.passResultLogError getDatabaseConnection
    let UpdateBlackListStatus channels = updateBlackListStatus channels