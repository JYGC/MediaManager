namespace MediaManager.Services

open System
open OPMF.Entities
open MediaManager.Types.DatabaseContextTypes

module ChannelServices =
    let getAll
      (getDbConnection: unit -> Result<TDatabaseConnection, exn>)
      (getChannelCollection: TDatabaseConnection -> TChannelCollection)
      : Result<ResizeArray<Channel>, exn> =
        match getDbConnection() with
        | Ok dbConnection ->
            try Ok (getChannelCollection(dbConnection).FindAll() |> ResizeArray<Channel>)
            with e -> Error e
        | Error ex -> Error ex

    let getBySiteId
      (getDbConnection: unit -> Result<TDatabaseConnection, exn>)
      (getChannelCollection: TDatabaseConnection -> TChannelCollection)
      (siteId: string)
      : Result<Channel option, exn> =
        match getDbConnection() with
        | Ok dbConnection ->
            try
                let channels = getChannelCollection(dbConnection).Query().Where(fun m ->
                    m.SiteId = siteId).ToList()
                match channels with
                | value when value.Count = 0 -> Ok None
                | _ -> Ok (Some channels[0])
            with e -> Error e
        | Error ex -> Error ex

    let getManyBySiteIds
      (getDbConnection: unit -> Result<TDatabaseConnection, exn>)
      (getChannelCollection: TDatabaseConnection -> TChannelCollection)
      (siteIds: string seq)
      : Result<ResizeArray<Channel>, exn> =
        match getDbConnection() with
        | Ok dbConnection ->
            try
                let siteIdList = siteIds |> ResizeArray
                getChannelCollection(dbConnection).Query().Where(fun m ->
                    siteIdList.Contains(m.SiteId)).ToList()
                |> Ok
            with e -> Error e
        | Error ex -> Error ex

    let getNotBacklisted
      (getDbConnection: unit -> Result<TDatabaseConnection, exn>)
      (getChannelCollection: TDatabaseConnection -> TChannelCollection)
      : Result<ResizeArray<Channel>, exn> =
        match getDbConnection() with
        | Ok dbConnection ->
            try
                Ok (getChannelCollection(dbConnection).Query().Where(fun c ->
                    c.Blacklisted = false).ToList() |> ResizeArray<Channel>)
            with e -> Error e
        | Error ex -> Error ex

    let getManyByWordInName
      (getDbConnection: unit -> Result<TDatabaseConnection, exn>)
      (getChannelCollection: TDatabaseConnection -> TChannelCollection)
      (wordInChannelName: string)
      : Result<ResizeArray<Channel>, exn> =
        match getDbConnection() with
        | Ok dbConnection ->
            try
                Ok (getChannelCollection(dbConnection).Query().Where(fun c ->
                    c.Name.Contains(wordInChannelName)).ToList() |> ResizeArray<Channel>)
            with e -> Error e
        | Error ex -> Error ex

    let private _updateExistingChannelsAndReturnThem
      (updateFunction: Channel -> Channel -> unit)
      (channelCollection: TChannelCollection)
      (inboundChannels: Channel seq)
      : ResizeArray<Channel> * int =
        let siteIdChannelFromUiMap =
            inboundChannels |> Seq.map(fun c -> (c.SiteId, c)) |> Map.ofSeq
        let channelsFromUiSiteIds = siteIdChannelFromUiMap |> Map.keys |> ResizeArray
        let channelsToUpdate = channelCollection.Query().Where(fun c ->
            channelsFromUiSiteIds.Contains(c.SiteId)).ToList()
        channelsToUpdate
        |> Seq.iter(fun channelFromDb ->
            let inboundChannel = Map.find channelFromDb.SiteId siteIdChannelFromUiMap
            updateFunction channelFromDb inboundChannel)
        let updateNumber = channelCollection.Update(channelsToUpdate)
        (channelsToUpdate, updateNumber)

    let private _insertOrUpdate
      (channelCollection: TChannelCollection)
      (updateExistingChannelsAndReturnThem:
        (Channel -> Channel -> unit)
        -> TChannelCollection
        -> Channel seq
        -> ResizeArray<Channel> * int)
      (inboundChannels: Channel seq)
      : int * int =
        let updateFunction (channelFromDb: Channel) (inboundChannel: Channel): unit =
            channelFromDb.Name <- inboundChannel.Name
            if not (String.IsNullOrWhiteSpace(inboundChannel.Description)) then
                channelFromDb.Description <- inboundChannel.Description
            channelFromDb.Thumbnail.Url <- inboundChannel.Thumbnail.Url
            channelFromDb.Thumbnail.Width <- inboundChannel.Thumbnail.Width
            channelFromDb.Thumbnail.Height <- inboundChannel.Thumbnail.Height
            channelFromDb.Url <- inboundChannel.Url

        let (channelsToUpdate, updateNumber) =
            updateExistingChannelsAndReturnThem
                updateFunction
                channelCollection
                inboundChannels

        let inboundChannelsSiteIds =
            inboundChannels |> Seq.map(fun c -> c.SiteId) |> ResizeArray
        let channelsToUpdateSiteIds =
            channelsToUpdate |> Seq.map(fun c -> c.SiteId) |> ResizeArray
        let newChannelSiteIds =
            List.except channelsToUpdateSiteIds (inboundChannelsSiteIds |> Seq.toList)
            |> ResizeArray
        let newChannels =
            inboundChannels |> Seq.filter(fun c -> newChannelSiteIds.Contains(c.SiteId))
        let insertNumber = channelCollection.InsertBulk(newChannels)

        (insertNumber, updateNumber)

    let insertOrUpdate
      (getDbConnection: unit -> Result<TDatabaseConnection, exn>)
      (getChannelCollection: TDatabaseConnection -> TChannelCollection)
      (inboundChannels: Channel seq)
      : Result<int * int, exn> =
        match getDbConnection() with
        | Ok dbConnection ->
            try
                let channelCollection = getChannelCollection(dbConnection)
                let (insertNumber, updateNumber) =
                    _insertOrUpdate
                        channelCollection
                        _updateExistingChannelsAndReturnThem
                        inboundChannels
                dbConnection.Commit() |> ignore
                Ok (insertNumber, updateNumber)
            with e ->
                dbConnection.Rollback() |> ignore
                Error e
        | Error ex -> Error ex

    let private _updateLastCheckedOutAndActivity
      (channelCollection: TChannelCollection)
      (updateExistingChannelsAndReturnThem:
        (Channel -> Channel -> unit)
        -> TChannelCollection
        -> Channel seq
        -> ResizeArray<Channel> * int)
      (inboundChannels: Channel seq)
      : int =
        let updateFunction (channelFromDb: Channel) (inboundChannel: Channel): unit =
            channelFromDb.LastCheckedOut <- inboundChannel.LastCheckedOut
            channelFromDb.LastActivityDate <- inboundChannel.LastActivityDate

        let (_, updateNumber) =
            updateExistingChannelsAndReturnThem
                updateFunction
                channelCollection
                inboundChannels

        updateNumber

    let updateLastCheckedOutAndActivity
      (getDbConnection: unit -> Result<TDatabaseConnection, exn>)
      (getChannelCollection: TDatabaseConnection -> TChannelCollection)
      (inboundChannels: Channel seq)
      : Result<int, exn> =
        match getDbConnection() with
        | Ok dbConnection ->
            try
                let channelCollection = getChannelCollection(dbConnection)
                let updateNumber =
                    _updateLastCheckedOutAndActivity
                        channelCollection
                        _updateExistingChannelsAndReturnThem
                        inboundChannels
                dbConnection.Commit() |> ignore
                Ok updateNumber
            with e ->
                dbConnection.Rollback() |> ignore
                Error e
        | Error ex -> Error ex

    let _updateBlackListStatus
      (channelCollection: TChannelCollection)
      (updateExistingChannelsAndReturnThem:
        (Channel -> Channel -> unit)
        -> TChannelCollection
        -> Channel seq
        -> ResizeArray<Channel> * int)
      (inboundChannels: Channel seq)
      : int =
        let updateFunction (channelFromDb: Channel) (inboundChannel: Channel): unit =
            channelFromDb.Blacklisted <- inboundChannel.Blacklisted

        let (_, updateNumber) =
            updateExistingChannelsAndReturnThem
                updateFunction
                channelCollection
                inboundChannels

        updateNumber

    let updateBlackListStatus
      (getDbConnection: unit -> Result<TDatabaseConnection, exn>)
      (getChannelCollection: TDatabaseConnection -> TChannelCollection)
      (inboundChannels: Channel seq)
      : Result<int, exn> =
        match getDbConnection() with
        | Ok dbConnection ->
            try
                let channelCollection = getChannelCollection(dbConnection)
                let updateNumber =
                    _updateBlackListStatus
                        channelCollection
                        _updateExistingChannelsAndReturnThem
                        inboundChannels
                dbConnection.Commit() |> ignore
                Ok updateNumber
            with e ->
                dbConnection.Rollback() |> ignore
                Error e
        | Error ex -> Error ex