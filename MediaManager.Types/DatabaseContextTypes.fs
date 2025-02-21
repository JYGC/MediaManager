namespace MediaManager.Types

open LiteDB
open OPMF.Entities

module DatabaseContextTypes =
    type TDatabaseConnection = LiteDatabase
    type TChannelCollection = ILiteCollection<Channel>
    type TMetadataCollection = ILiteCollection<Metadata>
