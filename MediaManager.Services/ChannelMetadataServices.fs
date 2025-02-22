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
            let siteIdsToChannelsMap =
                (match getManyChannelsByWordInName wordInChannelName with
                | Ok channels -> channels
                | Error ec -> raise ec)
                |> Seq.map(fun c -> (c.SiteId, c))
                |> dict
            (match getManyMetadataByChannelSiteIdAndWordInTitle
                siteIdsToChannelsMap.Keys
                wordInMetadataTitle
                skip
                pageSize with
            | Ok metadatas -> metadatas
            | Error et -> raise et)
            |> Seq.map(fun m -> _createChannelMetadata siteIdsToChannelsMap[m.ChannelSiteId] m)
            |> ResizeArray
            |> Ok
        with ex -> Error ex

    let private _getChannelMetadataFromMetadata
      (getManyChannelsBySiteIds: ResizeArray<string> -> Result<ResizeArray<Channel>, exn>)
      (getMetadatasResult: Result<ResizeArray<Metadata>, exn>)
      : ResizeArray<ChannelMetadata> =
        match getMetadatasResult with
        | Ok metadatas ->
            let channelSiteIds =
                metadatas
                |> Seq.map(fun m -> m.ChannelSiteId)
                |> Seq.distinct
                |> ResizeArray
            let siteIdsToChannelsMap =
                (match getManyChannelsBySiteIds channelSiteIds with
                | Ok channels -> channels
                | Error ec -> raise ec)
                |> Seq.map(fun c -> (c.SiteId, c))
                |> dict
            metadatas
            |> Seq.map(fun m -> _createChannelMetadata siteIdsToChannelsMap[m.ChannelSiteId] m)
            |> ResizeArray
        | Error em -> raise em

    let getByTitleContainingWord
      (getManyMetadatasByWordInTitle: string -> int -> int -> Result<ResizeArray<Metadata>, exn>)
      (getManyChannelsBySiteIds: ResizeArray<string> -> Result<ResizeArray<Channel>, exn>)
      (wordInMetadataTitle: string)
      (skip: int)
      (pageSize: int)
      : Result<ResizeArray<ChannelMetadata>, exn> =
        try
            getManyMetadatasByWordInTitle wordInMetadataTitle skip pageSize
            |> _getChannelMetadataFromMetadata getManyChannelsBySiteIds
            |> Ok
        with ex -> Error ex

    let getNew
      (getNewMetadatas: int -> int -> Result<ResizeArray<Metadata>, exn>)
      (getManyChannelsBySiteIds: ResizeArray<string> -> Result<ResizeArray<Channel>, exn>)
      (skip: int)
      (pageSize: int)
      : Result<ResizeArray<ChannelMetadata>, exn> =
        try
            getNewMetadatas skip pageSize
            |> _getChannelMetadataFromMetadata getManyChannelsBySiteIds
            |> Ok
        with ex -> Error ex

    let getToDownloadAndWait
      (getToDownloadAndWaitMetadatas: int -> int -> Result<ResizeArray<Metadata>, exn>)
      (getManyChannelsBySiteIds: ResizeArray<string> -> Result<ResizeArray<Channel>, exn>)
      (skip: int)
      (pageSize: int)
      : Result<ResizeArray<ChannelMetadata>, exn> =
        try
            getToDownloadAndWaitMetadatas skip pageSize
            |> _getChannelMetadataFromMetadata getManyChannelsBySiteIds
            |> Ok
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
                | Ok (None) -> raise (
                    new Exception($"Cannot find channel with id: {metadata.ChannelSiteId}"))
                | Error es -> Error es
            | Ok None ->
                let struct (metadataFromProvider, channelFromProvider) =
                    mediaProvider.GetVideoByURL siteId
                match insertOrUpdateNewChannel [channelFromProvider] with
                | Ok _ -> ()
                | Error eiuc -> raise eiuc
                match insertNewMetadata [metadataFromProvider] with
                | Ok _ -> ()
                | Error eim -> raise eim
                [_createChannelMetadata channelFromProvider metadataFromProvider]
                |> ResizeArray
                |> Ok
            | Error es -> Error es
        with ex -> Error ex