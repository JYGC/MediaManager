namespace MediaManager.Services

open System

open OPMF.Entities
open MediaManager.Types.DatabaseContextTypes
open OPMF.SiteAdapter


module ChannelMetadataServices =
    let private _createChannelMetadata channel metadata =
        let newChannelMetadata = new ChannelMetadata()
        newChannelMetadata.Channel <- channel
        newChannelMetadata.Metadata <- metadata
        newChannelMetadata

    let getChannelMetadataFromMetadata
      (getCollection: unit -> Result<TMetadataCollection, exn>)
      (metadatas: Metadata seq)
      : ResizeArray<ChannelMetadata> = [] |> ResizeArray

    let getByChannelAndTitleContainingWord
      (wordInChannelName: string)
      (wordInMetadataTitle: string)
      (skip: int)
      (pageSize: int)
      : ResizeArray<ChannelMetadata> = [] |> ResizeArray

    let getByTitleContainingWord
      (wordInMetadataTitle: string)
      (skip: int)
      (pageSize: int)
      : ResizeArray<ChannelMetadata> = [] |> ResizeArray

    let getNew (skip: int) (pageSize: int): ResizeArray<ChannelMetadata> = [] |> ResizeArray

    let getToDownloadAndWait (skip: int) (pageSize: int): ResizeArray<ChannelMetadata> = [] |> ResizeArray

    let getVideoByUrl
      (getMetadataBySiteId: string -> Result<Metadata option, exn>)
      (getChannelBySiteId: string -> Result<Channel option, exn>)
      (insertNewMetadata: Metadata seq -> Result<int, exn>)
      (insertOrUpdateNewChannel: Channel seq -> Result<int * int, exn>)
      (mediaProvider: ISiteVideoMetadataGetter)
      (videoUrl: string)
      : Result<ResizeArray<ChannelMetadata>, exn> =
        try
            let siteId = mediaProvider.GetSiteIdFromURL videoUrl
            match getMetadataBySiteId siteId with
            | Ok (Some metadata) ->
                match getChannelBySiteId metadata.ChannelSiteId with
                | Ok (Some channel) ->
                    Ok ([_createChannelMetadata channel metadata] |> ResizeArray)
                | Ok (None) -> raise (
                    new Exception($"Cannot find channel with id: {metadata.ChannelSiteId}"))
                | Error es -> Error es
            | Ok None ->
                let struct (metadataFromProvider, channelFromProvider) =
                    mediaProvider.GetVideoByURL siteId
                match insertOrUpdateNewChannel [channelFromProvider] with
                | Ok values -> values |> ignore
                | Error eiuc -> raise eiuc
                match insertNewMetadata [metadataFromProvider] with
                | Ok values -> values |> ignore
                | Error eim -> raise eim
                Ok (
                    [_createChannelMetadata channelFromProvider metadataFromProvider]
                    |> ResizeArray
                )
            | Error es -> Error es
        with e ->
            Error e