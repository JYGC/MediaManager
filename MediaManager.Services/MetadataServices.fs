namespace MediaManager.Services

open System.Collections.Generic

open OPMF.Entities
open MediaManager.Types.DatabaseContextTypes

module MetadataServices =
    let getAll
      (getDbConnection: unit -> Result<TDatabaseConnection, exn>)
      (getMetadataCollection: TDatabaseConnection -> TMetadataCollection)
      : Result<ResizeArray<Metadata>, exn> =
        match getDbConnection() with
        | Ok dbConnection ->
            try getMetadataCollection(dbConnection).FindAll() |> ResizeArray<Metadata> |> Ok
            with e -> Error e
        | Error ex -> Error ex

    let getBySiteId
      (getDbConnection: unit -> Result<TDatabaseConnection, exn>)
      (getMetadataCollection: TDatabaseConnection -> TMetadataCollection)
      (siteId: string)
      : Result<Metadata option, exn> =
        match getDbConnection() with
        | Ok dbConnection ->
            try
                let metadatas = getMetadataCollection(dbConnection).Query().Where(fun m ->
                    m.SiteId = siteId).ToList()
                match metadatas with
                | value when value.Count = 0 -> None
                | _ -> Some metadatas[0]
                |> Ok
            with e -> Error e
        | Error ex -> Error ex

    let getToDownload
      (getDbConnection: unit -> Result<TDatabaseConnection, exn>)
      (getMetadataCollection: TDatabaseConnection -> TMetadataCollection)
      : Result<ResizeArray<Metadata>, exn> =
        match getDbConnection() with
        | Ok dbConnection ->
            try
                getMetadataCollection(dbConnection).Query().Where(fun m ->
                    m.Status = MetadataStatus.ToDownload).ToList()
                |> Ok
            with e -> Error e
        | Error ex -> Error ex

    let getToDownloadAndWait
      (getDbConnection: unit -> Result<TDatabaseConnection, exn>)
      (getMetadataCollection: TDatabaseConnection -> TMetadataCollection)
      (skip: int)
      (pageSize: int)
      : Result<ResizeArray<Metadata>, exn> =
        match getDbConnection() with
        | Ok dbConnection ->
            try
                let waitToDownloadMetadatas =
                    [MetadataStatus.ToDownload; MetadataStatus.Wait] |> List<MetadataStatus>
                getMetadataCollection(dbConnection).Query().Where(fun m ->
                    waitToDownloadMetadatas.Contains(m.Status)
                ).Skip(skip).Limit(pageSize).ToList()
                |> Ok
            with e -> Error e
        | Error ex -> Error ex

    let getNew
      (getDbConnection: unit -> Result<TDatabaseConnection, exn>)
      (getMetadataCollection: TDatabaseConnection -> TMetadataCollection)
      (skip: int)
      (pageSize: int)
      : Result<ResizeArray<Metadata>, exn> =
        match getDbConnection() with
        | Ok dbConnection ->
            try
                getMetadataCollection(dbConnection).Query().Where(fun m ->
                    m.Status = MetadataStatus.New).Skip(skip).Limit(pageSize).ToList()
                |> Ok
            with e -> Error e
        | Error ex -> Error ex

    let getManyByWordInTitle
      (getDbConnection: unit -> Result<TDatabaseConnection, exn>)
      (getMetadataCollection: TDatabaseConnection -> TMetadataCollection)
      (wordInMetadataTitle: string)
      (skip: int)
      (pageSize: int)
      : Result<ResizeArray<Metadata>, exn> =
        match getDbConnection() with
        | Ok dbConnection ->
            try
                getMetadataCollection(dbConnection).Query().Where(fun m ->
                    m.Title.Contains(wordInMetadataTitle)
                ).OrderByDescending(fun i -> i.PublishedAt).Skip(skip).Limit(pageSize).ToList()
                |> Ok
            with e -> Error e
        | Error ex -> Error ex

    let getManyByChannelSiteIdAndWordInTitle
      (getDbConnection: unit -> Result<TDatabaseConnection, exn>)
      (getMetadataCollection: TDatabaseConnection -> TMetadataCollection)
      (channelSiteIds: string seq)
      (wordInMetadataTitle: string)
      (skip: int)
      (pageSize: int)
      : Result<ResizeArray<Metadata>, exn> =
        match getDbConnection() with
        | Ok dbConnection ->
            try
                let channelSiteIdList = channelSiteIds |> ResizeArray |> List<string>
                getMetadataCollection(dbConnection).Query().Where(fun m ->
                    channelSiteIdList.Contains(m.ChannelSiteId) &&
                    m.Title.Contains(wordInMetadataTitle)).Skip(skip).Limit(pageSize).ToList()
                |> Ok
            with e -> Error e
        | Error ex -> Error ex

    let insertNew
      (getDbConnection: unit -> Result<TDatabaseConnection, exn>)
      (getMetadataCollection: TDatabaseConnection -> TMetadataCollection)
      (inboundMetadata: Metadata seq)
      : Result<int, exn> =
        match getDbConnection() with
        | Ok dbConnection ->
            try
                let inboundMetadataSiteIds =
                    inboundMetadata |> Seq.map(fun m -> m.SiteId) |> List<string>
                let metadataCollection = getMetadataCollection(dbConnection)
                let existingMetadataSiteIds = metadataCollection.Query().Where(fun m ->
                    inboundMetadataSiteIds.Contains(m.SiteId)).Select(fun m -> m.SiteId).ToList()
                let newMetadataSiteIds =
                    Array.except existingMetadataSiteIds (inboundMetadataSiteIds |> Seq.toArray)
                    |> List<string>
                let newMetadata = inboundMetadata |> Seq.filter(fun m ->
                    newMetadataSiteIds.Contains(m.SiteId))
                let insertNumber = metadataCollection.InsertBulk(newMetadata)
                dbConnection.Commit() |> ignore
                Ok insertNumber
            with e ->
                dbConnection.Rollback() |> ignore
                Error e
        | Error ex -> Error ex

    let private _updateExistingMetadatasAndReturnThem
      (updateFunction: Metadata -> Metadata -> unit)
      (metadataCollection: TMetadataCollection)
      (inboundMetadata: Metadata seq)
      : List<Metadata> * int =
        let siteIdToInboundMetadataMap =
            inboundMetadata |> Seq.map(fun m -> (m.SiteId, m)) |> Map.ofSeq
        let inboundMetadataSiteIds: List<string> = siteIdToInboundMetadataMap |> Map.keys |> ResizeArray
        let metadatasToUpdate = metadataCollection.Query().Where(fun m ->
            inboundMetadataSiteIds.Contains(m.SiteId)).ToList()
        metadatasToUpdate
        |> Seq.iter(fun metadataFromDb ->
            let inboundMetadata = Map.find metadataFromDb.SiteId siteIdToInboundMetadataMap
            updateFunction metadataFromDb inboundMetadata)
        let updateNumber = metadataCollection.Update(metadatasToUpdate)
        (metadatasToUpdate, updateNumber)

    let private _updateStatus
      (metadataCollection: TMetadataCollection)
      (updateExistingChannelsAndReturnThem:
        (Metadata -> Metadata -> unit)
        -> TMetadataCollection
        -> Metadata seq
        -> List<Metadata> * int)
      (inboundMetadata: Metadata seq)
      : int =
        let updateFunction (metadataFromDb: Metadata) (inboundMetadata: Metadata): unit =
            metadataFromDb.Status <- inboundMetadata.Status

        let (_, updateNumber) =
            updateExistingChannelsAndReturnThem
                updateFunction
                metadataCollection
                inboundMetadata

        updateNumber

    let updateStatus
      (getDbConnection: unit -> Result<TDatabaseConnection, exn>)
      (getMetadataCollection: TDatabaseConnection -> TMetadataCollection)
      (inboundMetadata: Metadata seq)
      : Result<int, exn> =
        match getDbConnection() with
        | Ok dbConnection ->
            try
                let metadataCollection = getMetadataCollection(dbConnection)
                let updateNumber =
                    _updateStatus
                        metadataCollection
                        _updateExistingMetadatasAndReturnThem
                        inboundMetadata
                dbConnection.Commit() |> ignore
                Ok updateNumber
            with e ->
                dbConnection.Rollback() |> ignore
                Error e
        | Error ex -> Error ex

    let _updateIsBeingProcessed
      (metadataCollection: TMetadataCollection)
      (updateExistingChannelsAndReturnThem:
        (Metadata -> Metadata -> unit)
        -> TMetadataCollection
        -> Metadata seq
        -> List<Metadata> * int)
      (inboundMetadata: Metadata seq)
      (isBeingProcessed: bool option)
      : int =
        let updateFunction (metadataFromDb: Metadata) (inboundMetadata: Metadata): unit =
            metadataFromDb.IsBeingDownloaded <-
                match isBeingProcessed with
                | None -> inboundMetadata.IsBeingDownloaded
                | Some isBeingProcessedValue -> isBeingProcessedValue

        let (_, updateNumber) =
            updateExistingChannelsAndReturnThem
                updateFunction
                metadataCollection
                inboundMetadata

        updateNumber

    let updateIsBeingProcessed
      (getDbConnection: unit -> Result<TDatabaseConnection, exn>)
      (getMetadataCollection: TDatabaseConnection -> TMetadataCollection)
      (inboundMetadata: Metadata seq)
      (isBeingProcessed: bool option)
      : Result<int, exn> =
        match getDbConnection() with
        | Ok dbConnection ->
            try
                let metadataCollection = getMetadataCollection(dbConnection)
                let updateNumber =
                    _updateIsBeingProcessed
                        metadataCollection
                        _updateExistingMetadatasAndReturnThem
                        inboundMetadata
                        isBeingProcessed
                dbConnection.Commit() |> ignore
                Ok updateNumber
            with e ->
                dbConnection.Rollback() |> ignore
                Error e
        | Error ex -> Error ex
