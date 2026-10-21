namespace MediaManager.Dtos.Services;

public class ServiceLogEntryDto
{
    public string Id { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ServiceLogEntryType LogType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ExceptionText { get; set; }
    public string VariablesAsIndentedJson { get; set; } = string.Empty;
}
