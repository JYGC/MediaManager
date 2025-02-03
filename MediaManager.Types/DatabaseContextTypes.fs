namespace MediaManager.Types

open LiteDB
open OPMF.Entities

module DatabaseContextTypes =
    type TDatabaseConnection = LiteDatabase
    type TLogCollection = ILiteCollection<OPMFLog>
    type TChannelCollection = ILiteCollection<Channel>
    type TMetadataCollection = ILiteCollection<Metadata>
