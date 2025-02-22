namespace MediaManager.Database

open LiteDB

open OPMF.Entities
open MediaManager.Types.DatabaseContextTypes

module DatabaseContext =
    let createGetDatabaseConnection databasePath connectionType : unit -> Result<TDatabaseConnection, exn> =
        let dbConnection =
            try Ok (new LiteDatabase($"Filename={databasePath};connection={connectionType}"))
            with e -> Error e
        let getDbConnection() = dbConnection
        getDbConnection

    let getLogCollection
      (collectionName: string)
      (dbConnection: TDatabaseConnection)
      : TLogCollection =
        dbConnection.GetCollection<OPMFLog>(collectionName)

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