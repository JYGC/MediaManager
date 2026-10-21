namespace MediaManager.Dtos.Repositories;

public class RepositoryLogEntryDto
{
    public string Id { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public RepositoryLogEntryType LogType { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ExceptionText { get; set; }
    public string VariablesAsIndentedJson { get; set; } = string.Empty;
}
