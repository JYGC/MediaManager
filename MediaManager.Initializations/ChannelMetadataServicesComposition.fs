namespace MediaManager.Initializations

open OPMF.Entities
open MediaManager.Services
open OPMF.SiteAdapter.Youtube

module ChannelMetadataServicesComposition =
    let getVideoByUrl videoUrl =
        let getDatabaseConnection = DatabaseContextComposition.createGetDatabaseConnection()
        ChannelMetadataServices.getVideoByUrl
            (MetadataServices.getBySiteId getDatabaseConnection DatabaseContextComposition.getMetadataCollection)
            (ChannelServices.getBySiteId getDatabaseConnection DatabaseContextComposition.getChannelCollection)
            (MetadataServices.insertNew getDatabaseConnection DatabaseContextComposition.getMetadataCollection)
            (ChannelServices.insertOrUpdate getDatabaseConnection DatabaseContextComposition.getChannelCollection)
            (new YoutubeVideoMetadataGetter())
            videoUrl
        |> LogServicesComposition.passResultLogError getDatabaseConnection
    let GetVideoByUrl videoUrl = getVideoByUrl videoUrl
