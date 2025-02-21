namespace MediaManager.Initializations

open OPMF.Entities
open MediaManager.Services
open OPMF.SiteAdapter.Youtube

module ChannelMetadataServicesComposition =
    let getVideoByUrl: string -> Result<ResizeArray<ChannelMetadata>, exn> =
        ChannelMetadataServices.getVideoByUrl
            MetadataServicesComposition.getBySiteId
            ChannelServicesComposition.getBySiteId
            MetadataServicesComposition.insertNew
            ChannelServicesComposition.insertOrUpdate
            (new YoutubeVideoMetadataGetter())
    let GetVideoByUrl videoUrl = getVideoByUrl videoUrl
