namespace MediaManager.Services

open System

open OPMF.Entities
open OPMF.SiteAdapter

module ChannelMetadataServices =
    let private _createChannelMetadata channel metadata =
        let newChannelMetadata = new ChannelMetadata()
        newChannelMetadata.Channel <- channel
        newChannelMetadata.Metadata <- metadata
        newChannelMetadata

    let getByChannelAndTitleContainingWord
      (getManyChannelsByWordInName: string -> Result<ResizeArray<Channel>, exn>)
      (getManyMetadataByChannelSiteIdAndWordInTitle:
        string seq -> string -> int -> int -> Result<ResizeArray<Metadata>, exn>)
      (wordInChannelName: string)
      (wordInMetadataTitle: string)
      (skip: int)
      (pageSize: int)
      : Result<ResizeArray<ChannelMetadata>, exn> =
        try
            let siteIdsToChannelsMapResult =
                getManyChannelsByWordInName wordInChannelName
                |> Result.bind (fun ca ->
                    ca
                    |> Seq.map (fun c -> (c.SiteId, c))
                    |> dict
                    |> Ok
                )

            siteIdsToChannelsMapResult
            |> Result.bind (fun siteIdsToChannelsMap ->
                getManyMetadataByChannelSiteIdAndWordInTitle
                    siteIdsToChannelsMap.Keys
                    wordInMetadataTitle
                    skip
                    pageSize
                |> Result.bind (fun metadatas ->
                    Ok (metadatas, siteIdsToChannelsMap)
                )
            )
            |> Result.bind (fun (metadatas, siteIdsToChannelsMap) ->
                metadatas
                |> Seq.map(fun m -> _createChannelMetadata siteIdsToChannelsMap[m.ChannelSiteId] m)
                |> ResizeArray
                |> Ok
            )
        with ex -> Error ex

    let private _getChannelMetadataFromMetadata
      (getManyChannelsBySiteIds: ResizeArray<string> -> Result<ResizeArray<Channel>, exn>)
      (metadatas: ResizeArray<Metadata>)
      : Result<ResizeArray<ChannelMetadata>, exn> =
        let channelSiteIds =
            metadatas
            |> Seq.map (fun m -> m.ChannelSiteId)
            |> Seq.distinct
            |> ResizeArray
        let siteIdsToChannelsMapResult =
            getManyChannelsBySiteIds channelSiteIds
            |> Result.bind (fun channels ->
                channels
                |> Seq.map (fun c -> (c.SiteId, c))
                |> dict
                |> Ok
            )
        siteIdsToChannelsMapResult
        |> Result.bind (fun siteIdsToChannelsMap ->
            metadatas
            |> Seq.map (fun m -> _createChannelMetadata siteIdsToChannelsMap[m.ChannelSiteId] m)
            |> ResizeArray
            |> Ok
        )

    let getByTitleContainingWord
      (getManyMetadatasByWordInTitle: string -> int -> int -> Result<ResizeArray<Metadata>, exn>)
      (getManyChannelsBySiteIds: ResizeArray<string> -> Result<ResizeArray<Channel>, exn>)
      (wordInMetadataTitle: string)
      (skip: int)
      (pageSize: int)
      : Result<ResizeArray<ChannelMetadata>, exn> =
        try
            getManyMetadatasByWordInTitle wordInMetadataTitle skip pageSize
            |> Result.bind (_getChannelMetadataFromMetadata getManyChannelsBySiteIds)
        with ex -> Error ex

    let getNew
      (getNewMetadatas: int -> int -> Result<ResizeArray<Metadata>, exn>)
      (getManyChannelsBySiteIds: ResizeArray<string> -> Result<ResizeArray<Channel>, exn>)
      (skip: int)
      (pageSize: int)
      : Result<ResizeArray<ChannelMetadata>, exn> =
        try
            getNewMetadatas skip pageSize
            |> Result.bind (_getChannelMetadataFromMetadata getManyChannelsBySiteIds)
        with ex -> Error ex

    let getToDownloadAndWait
      (getToDownloadAndWaitMetadatas: int -> int -> Result<ResizeArray<Metadata>, exn>)
      (getManyChannelsBySiteIds: ResizeArray<string> -> Result<ResizeArray<Channel>, exn>)
      (skip: int)
      (pageSize: int)
      : Result<ResizeArray<ChannelMetadata>, exn> =
        try
            getToDownloadAndWaitMetadatas skip pageSize
            |> Result.bind (_getChannelMetadataFromMetadata getManyChannelsBySiteIds)
        with ex -> Error ex

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
                    [_createChannelMetadata channel metadata] |> ResizeArray |> Ok
                | Ok None ->
                    Error (new Exception($"Cannot find channel with id: {metadata.ChannelSiteId}"))
                | Error es -> Error es
            | Ok None ->
                let struct (metadataFromProvider, channelFromProvider) =
                    mediaProvider.GetVideoByURL siteId

                insertOrUpdateNewChannel [channelFromProvider]
                |> Result.bind (fun _ -> insertNewMetadata [metadataFromProvider])
                |> Result.bind (fun _ ->
                    [_createChannelMetadata channelFromProvider metadataFromProvider]
                    |> ResizeArray
                    |> Ok
                )
            | Error es -> Error es
        with ex -> Error ex