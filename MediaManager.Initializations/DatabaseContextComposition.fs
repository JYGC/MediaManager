namespace MediaManager.Initializations

open OPMF.Settings
open MediaManager.Definitions.DatabaseContextDefinitions
open MediaManager.Database

module DatabaseContextComposition =
    let connectionType = "shared"
    let databasePath = ConfigHelper.ReadonlySettings.GetDatabasePath()
    let getDatabaseConnection: unit -> Result<TDatabaseConnection, exn> =
        fun _ -> DatabaseContext.getDatabaseConnection databasePath connectionType

    let channelCollectionName = "YoutubeChannel"
    let getChannelCollection: TDatabaseConnection -> TChannelCollection =
        DatabaseContext.getChannelCollection channelCollectionName

    let metadataCollectionName = "YoutubeMetadata"
    let getMetadataCollection: TDatabaseConnection -> TMetadataCollection =
        DatabaseContext.getMetadataCollection metadataCollectionName