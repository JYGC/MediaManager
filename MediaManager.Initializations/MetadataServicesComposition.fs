namespace MediaManager.Initializations

open OPMF.Entities
open MediaManager.Services

module MetadataServicesComposition =
    let getAll: unit -> Result<ResizeArray<Metadata>, exn> =
        fun _ ->
            MetadataServices.getAll
                DatabaseContextComposition.getDatabaseConnection
                DatabaseContextComposition.getMetadataCollection
    let GetAll() = getAll()

    let getBySiteId: string -> Result<Metadata, exn> =
        MetadataServices.getBySiteId
            DatabaseContextComposition.getDatabaseConnection
            DatabaseContextComposition.getMetadataCollection
    let GetBySiteId siteId = getBySiteId siteId

    let getToDownload: unit -> Result<ResizeArray<Metadata>, exn> =
        fun _ ->
            MetadataServices.getToDownload
                DatabaseContextComposition.getDatabaseConnection
                DatabaseContextComposition.getMetadataCollection
    let GetToDownload() = getToDownload()

    let getToDownloadAndWait: int -> int -> Result<ResizeArray<Metadata>, exn> =
        MetadataServices.getToDownloadAndWait
            DatabaseContextComposition.getDatabaseConnection
            DatabaseContextComposition.getMetadataCollection
    let GetToDownloadAndWait skip pageSize = getToDownloadAndWait skip pageSize

    let getNew: int -> int -> Result<ResizeArray<Metadata>, exn> =
        MetadataServices.getNew
            DatabaseContextComposition.getDatabaseConnection
            DatabaseContextComposition.getMetadataCollection
    let GetNew skip pageSize = getNew skip pageSize

    let getManyByWordInTitle: string -> int -> int -> Result<ResizeArray<Metadata>, exn> =
        MetadataServices.getManyByWordInTitle
            DatabaseContextComposition.getDatabaseConnection
            DatabaseContextComposition.getMetadataCollection
    let GetManyByWordInTitle wordInMetadataTitle skip pageSize =
        getManyByWordInTitle wordInMetadataTitle skip pageSize

    let getManyByChannelSiteIdAndWordInTitle
      : string seq -> string -> int -> int -> Result<ResizeArray<Metadata>, exn> =
        MetadataServices.getManyByChannelSiteIdAndWordInTitle
            DatabaseContextComposition.getDatabaseConnection
            DatabaseContextComposition.getMetadataCollection
    let GetManyByChannelSiteIdAndWordInTitle channelSiteIds wordInMetadataTitle skip pageSize =
        getManyByChannelSiteIdAndWordInTitle channelSiteIds wordInMetadataTitle skip pageSize

    let insertNew: Metadata seq -> Result<int, exn> =
        MetadataServices.insertNew
            DatabaseContextComposition.getDatabaseConnection
            DatabaseContextComposition.getMetadataCollection
    let InsertNew newMetadatas = insertNew newMetadatas