namespace MediaManager.Dtos.Application;

public class ApplicationLogEntryDto
{
    public string Id { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ApplicationLogEntryType LogType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ExceptionText { get; set; }
    public string VariablesAsIndentedJson { get; set; } = string.Empty;
}
