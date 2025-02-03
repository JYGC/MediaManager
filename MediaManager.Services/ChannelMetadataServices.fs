namespace MediaManager.Services

open OPMF.Entities
open MediaManager.Definitions.DatabaseContextDefinitions
open OPMF.SiteAdapter
open OPMF.SiteAdapter.Youtube


module ChannelMetadataServices =
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
      (mediaProvider: ISiteVideoMetadataGetter)
      (getMetadataBySiteId: string -> Result<Metadata, exn>)
      (insertNewMetadata: Metadata seq -> Result<int, exn>)
      (insertOrUpdateNewChannel: Channel seq -> Result<int * int, exn>)
      (videoUrl: string)
      (skip: int)
      (pageSize: int)
      : ResizeArray<ChannelMetadata> = [] |> ResizeArray
