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

    let getBySiteId siteId =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        MetadataServices.getBySiteId
            getDatabaseConnection
            DatabaseContextComposition.getMetadataCollection
            siteId
        |> LogServicesComposition.passResultLogError
            getDatabaseConnection

    let getToDownload() =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        MetadataServices.getToDownload
            getDatabaseConnection
            DatabaseContextComposition.getMetadataCollection
        |> LogServicesComposition.passResultLogError
            getDatabaseConnection

    let getToDownloadAndWait skip pageSize =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        MetadataServices.getToDownloadAndWait
            getDatabaseConnection
            DatabaseContextComposition.getMetadataCollection
            skip
            pageSize
        |> LogServicesComposition.passResultLogError
            getDatabaseConnection

    let getNew skip pageSize =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        MetadataServices.getNew
            getDatabaseConnection
            DatabaseContextComposition.getMetadataCollection
            skip
            pageSize
        |> LogServicesComposition.passResultLogError
            getDatabaseConnection

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

    let insertNew newMetadatas =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        MetadataServices.insertNew
            getDatabaseConnection
            DatabaseContextComposition.getMetadataCollection
            newMetadatas
        |> LogServicesComposition.passResultLogError
            getDatabaseConnection