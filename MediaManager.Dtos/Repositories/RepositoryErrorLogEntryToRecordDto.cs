namespace MediaManager.Dtos.Repositories;

public class RepositoryErrorLogEntryToRecordDto
{
    public string Message { get; set; } = string.Empty;
    public string ExceptionText { get; set; } = string.Empty;
    public Dictionary<string, string> VariablesAsJsonText { get; set; } = new();
}
