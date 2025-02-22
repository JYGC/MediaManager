namespace MediaManager.Initializations

open MediaManager.Services

module MetadataServicesComposition =
    let getAll() =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        MetadataServices.getAll
            getDatabaseConnection
            DatabaseContextComposition.getMetadataCollection
        |> LogServicesComposition.passResultLogError
            getDatabaseConnection
    let GetAll() = getAll()

    let getBySiteId siteId =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        MetadataServices.getBySiteId
            getDatabaseConnection
            DatabaseContextComposition.getMetadataCollection
            siteId
        |> LogServicesComposition.passResultLogError
            getDatabaseConnection
    let GetBySiteId siteId = getBySiteId siteId

    let getToDownload() =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        MetadataServices.getToDownload
            getDatabaseConnection
            DatabaseContextComposition.getMetadataCollection
        |> LogServicesComposition.passResultLogError
            getDatabaseConnection
    let GetToDownload() = getToDownload()

    let getToDownloadAndWait skip pageSize =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        MetadataServices.getToDownloadAndWait
            getDatabaseConnection
            DatabaseContextComposition.getMetadataCollection
            skip
            pageSize
        |> LogServicesComposition.passResultLogError
            getDatabaseConnection
    let GetToDownloadAndWait skip pageSize = getToDownloadAndWait skip pageSize

    let getNew skip pageSize =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        MetadataServices.getNew
            getDatabaseConnection
            DatabaseContextComposition.getMetadataCollection
            skip
            pageSize
        |> LogServicesComposition.passResultLogError
            getDatabaseConnection
    let GetNew skip pageSize = getNew skip pageSize

    let getManyByWordInTitle wordInMetadataTitle skip pageSize =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        MetadataServices.getManyByWordInTitle
            getDatabaseConnection
            DatabaseContextComposition.getMetadataCollection
            wordInMetadataTitle
            skip
            pageSize
        |> LogServicesComposition.passResultLogError
            getDatabaseConnection
    let GetManyByWordInTitle wordInMetadataTitle skip pageSize =
        getManyByWordInTitle wordInMetadataTitle skip pageSize

    let getManyByChannelSiteIdAndWordInTitle channelSiteIds wordInMetadataTitle skip pageSize =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        MetadataServices.getManyByChannelSiteIdAndWordInTitle
            getDatabaseConnection
            DatabaseContextComposition.getMetadataCollection
            channelSiteIds
            wordInMetadataTitle
            skip
            pageSize
        |> LogServicesComposition.passResultLogError
            getDatabaseConnection
    let GetManyByChannelSiteIdAndWordInTitle channelSiteIds wordInMetadataTitle skip pageSize =
        getManyByChannelSiteIdAndWordInTitle channelSiteIds wordInMetadataTitle skip pageSize

    let insertNew newMetadatas =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        MetadataServices.insertNew
            getDatabaseConnection
            DatabaseContextComposition.getMetadataCollection
            newMetadatas
        |> LogServicesComposition.passResultLogError
            getDatabaseConnection
    let InsertNew newMetadatas = insertNew newMetadatas