namespace MediaManager.Initializations

open MediaManager.Services
open OPMF.SiteAdapter.Youtube

module ChannelMetadataServicesComposition =
    let getByChannelAndTitleContainingWord wordInChannelName wordInMetadataTitle skip pageSize =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        ChannelMetadataServices.getByChannelAndTitleContainingWord
            (ChannelServices.getManyByWordInName
                getDatabaseConnection
                DatabaseContextComposition.getChannelCollection)
            (MetadataServices.getManyByChannelSiteIdAndWordInTitle
                getDatabaseConnection
                DatabaseContextComposition.getMetadataCollection)
            wordInChannelName
            wordInMetadataTitle
            skip
            pageSize
        |> LogServicesComposition.passResultLogError getDatabaseConnection

    let getByTitleContainingWord wordInMetadataTitle skip pageSize =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        ChannelMetadataServices.getByTitleContainingWord
            (MetadataServices.getManyByWordInTitle
                getDatabaseConnection
                DatabaseContextComposition.getMetadataCollection)
            (ChannelServices.getManyBySiteIds
                getDatabaseConnection
                DatabaseContextComposition.getChannelCollection)
            wordInMetadataTitle
            skip
            pageSize
        |> LogServicesComposition.passResultLogError getDatabaseConnection

    let getNew skip pageSize =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        ChannelMetadataServices.getNew
            (MetadataServices.getNew
                getDatabaseConnection
                DatabaseContextComposition.getMetadataCollection)
            (ChannelServices.getManyBySiteIds
                getDatabaseConnection
                DatabaseContextComposition.getChannelCollection)
            skip
            pageSize
        |> LogServicesComposition.passResultLogError getDatabaseConnection

    let getToDownloadAndWait skip pageSize =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        ChannelMetadataServices.getToDownloadAndWait
            (MetadataServices.getToDownloadAndWait
                getDatabaseConnection
                DatabaseContextComposition.getMetadataCollection)
            (ChannelServices.getManyBySiteIds
                getDatabaseConnection
                DatabaseContextComposition.getChannelCollection)
            skip
            pageSize
        |> LogServicesComposition.passResultLogError getDatabaseConnection

    let getVideoByUrl videoUrl =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        ChannelMetadataServices.getVideoByUrl
            (MetadataServices.getBySiteId
                getDatabaseConnection
                DatabaseContextComposition.getMetadataCollection)
            (ChannelServices.getBySiteId
                getDatabaseConnection
                DatabaseContextComposition.getChannelCollection)
            (MetadataServices.insertNew
                getDatabaseConnection
                DatabaseContextComposition.getMetadataCollection)
            (ChannelServices.insertOrUpdate
                getDatabaseConnection
                DatabaseContextComposition.getChannelCollection)
            (new YoutubeVideoMetadataGetter())
            videoUrl
        |> LogServicesComposition.passResultLogError getDatabaseConnection
    let GetVideoByUrl videoUrl = getVideoByUrl videoUrl
