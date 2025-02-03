namespace MediaManager.Database

open LiteDB

open OPMF.Entities
open MediaManager.Definitions.DatabaseContextDefinitions

module DatabaseContext =
    let getDatabaseConnection databasePath connectionType : Result<TDatabaseConnection, exn> =
        try Ok (new LiteDatabase($"Filename={databasePath};connection={connectionType}"))
        with e -> Error e

    let getChannelCollection
      (collectionName: string)
      (dbConnection: TDatabaseConnection)
      : TChannelCollection =
        dbConnection.GetCollection<Channel>(collectionName)

    let getMetadataCollection
      (collectionName: string)
      (dbConnection: TDatabaseConnection)
      : TMetadataCollection =
        dbConnection.GetCollection<Metadata>(collectionName)