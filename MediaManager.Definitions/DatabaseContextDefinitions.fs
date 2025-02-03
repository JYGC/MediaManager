namespace MediaManager.Definitions

open LiteDB
open OPMF.Entities

module DatabaseContextDefinitions =
    type TDatabaseConnection = LiteDatabase
    type TChannelCollection = ILiteCollection<Channel>
    type TMetadataCollection = ILiteCollection<Metadata>
