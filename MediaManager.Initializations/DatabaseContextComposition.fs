namespace MediaManager.Initializations

open OPMF.Settings
open MediaManager.Types.DatabaseContextTypes
open MediaManager.Database

module DatabaseContextComposition =
    let connectionType = "shared"
    let databasePath = ConfigHelper.ReadonlySettings.GetDatabasePath()
    let createGetDatabaseConnection: unit -> (unit -> Result<TDatabaseConnection, exn>) =
        fun () -> DatabaseContext.createGetDatabaseConnection databasePath connectionType

    let logCollectionName = "OPMFLog"
    let getLogCollection: TDatabaseConnection -> TLogCollection =
        DatabaseContext.getLogCollection logCollectionName

    let channelCollectionName = "YoutubeChannel"
    let getChannelCollection: TDatabaseConnection -> TChannelCollection =
        DatabaseContext.getChannelCollection channelCollectionName

    let metadataCollectionName = "YoutubeMetadata"
    let getMetadataCollection: TDatabaseConnection -> TMetadataCollection =
        DatabaseContext.getMetadataCollection metadataCollectionName