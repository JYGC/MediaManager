namespace OPMF.Entities
{
    // Member names are stored by LiteDB: never rename them.
    public enum EntityMaximumVideoResolutionChoice
    {
        NoLimit,
        UpTo2160p,
        UpTo1440p,
        UpTo1080p,
        UpTo720p,
        UpTo480p,
        UpTo360p,
    }
}
