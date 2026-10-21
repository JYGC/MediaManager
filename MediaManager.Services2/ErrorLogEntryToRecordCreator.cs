using System.Text.Json;
using MediaManager.Dtos.Repositories;

namespace MediaManager.Services2;

internal static class ErrorLogEntryToRecordCreator
{
    public static RepositoryErrorLogEntryToRecordDto CreateWithVariablesSerializedAsJsonText(Exception exception, IReadOnlyDictionary<string, object?> variables)
    {
        return new RepositoryErrorLogEntryToRecordDto
        {
            Message = exception.Message,
            ExceptionText = exception.ToString(),
            VariablesAsJsonText = variables.ToDictionary(variable => variable.Key, variable => JsonSerializer.Serialize(variable.Value)),
        };
    }
}
