namespace MediaManager.Initializations

open OPMF.Entities
open MediaManager.Services

module ChannelServicesComposition =
    let getAll: unit -> Result<ResizeArray<Channel>, exn> =
        fun _ ->
            ChannelServices.getAll
                DatabaseContextComposition.getDatabaseConnection
                DatabaseContextComposition.getChannelCollection
    let GetAll() = getAll()

    let getBySiteId: string -> Result<Channel, exn> =
        ChannelServices.getBySiteId
            DatabaseContextComposition.getDatabaseConnection
            DatabaseContextComposition.getChannelCollection
    let GetBySiteId siteId = getBySiteId siteId

    let getNotBacklisted: unit -> Result<ResizeArray<Channel>, exn> =
        fun _ ->
            ChannelServices.getNotBacklisted
                DatabaseContextComposition.getDatabaseConnection
                DatabaseContextComposition.getChannelCollection
    let GetNotBacklisted() = getNotBacklisted()

    let getManyByWordInName: string -> Result<ResizeArray<Channel>, exn> =
        ChannelServices.getManyByWordInName
            DatabaseContextComposition.getDatabaseConnection
            DatabaseContextComposition.getChannelCollection
    let GetManyByWordInName wordInChannelName = getManyByWordInName wordInChannelName

    let insertOrUpdate: Channel seq -> Result<int * int, exn> =
        ChannelServices.insertOrUpdate
            DatabaseContextComposition.getDatabaseConnection
            DatabaseContextComposition.getChannelCollection
    let InsertOrUpdate channels = insertOrUpdate channels

    let updateLastCheckedOutAndActivity: Channel seq -> Result<int, exn> =
        ChannelServices.updateLastCheckedOutAndActivity
            DatabaseContextComposition.getDatabaseConnection
            DatabaseContextComposition.getChannelCollection
    let UpdateLastCheckedOutAndActivity channels = updateLastCheckedOutAndActivity channels

    let updateBlackListStatus: Channel seq -> Result<int, exn> =
        ChannelServices.updateBlackListStatus
            DatabaseContextComposition.getDatabaseConnection
            DatabaseContextComposition.getChannelCollection
    let UpdateBlackListStatus channels = updateBlackListStatus channels