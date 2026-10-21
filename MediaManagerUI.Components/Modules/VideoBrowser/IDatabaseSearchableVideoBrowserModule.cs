namespace MediaManagerUI.Modules.VideoBrowser
{
    public interface IDatabaseSearchableVideoBrowserModule
    {
        string? SearchVideoTitle { get; set; }
        string? SearchChannelName { get; set; }
    }
}
