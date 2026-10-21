using MediaManager.Dtos.Repositories;
using OPMF.Database;
using OPMF.Entities;
using OPMF.Logging;

namespace MediaManager.Repositories;

public interface ILogRepository
{
    List<RepositoryLogEntryDto> GetWarningsLoggedBetween(DateTime startDateTime, DateTime endDateTime);
    List<RepositoryLogEntryDto> GetErrorsLoggedBetween(DateTime startDateTime, DateTime endDateTime);
    void RecordErrorLogEntry(RepositoryErrorLogEntryToRecordDto errorLogEntry); // saved as OPMFError via OPMF.Logging
}

public class LogRepository(IDatabaseConnectionOpener databaseConnectionOpener, Logger logger, TimeProvider timeProvider) : ILogRepository
{
    public List<RepositoryLogEntryDto> GetWarningsLoggedBetween(DateTime startDateTime, DateTime endDateTime)
    {
        return ReadLogEntriesAsDtos(logCollection => logCollection.GetWarnings(startDateTime, endDateTime));
    }

    public List<RepositoryLogEntryDto> GetErrorsLoggedBetween(DateTime startDateTime, DateTime endDateTime)
    {
        return ReadLogEntriesAsDtos(logCollection => logCollection.GetErrors(startDateTime, endDateTime));
    }

    public void RecordErrorLogEntry(RepositoryErrorLogEntryToRecordDto errorLogEntry)
    {
        DateTime timeOfRecording = timeProvider.GetLocalNow().LocalDateTime;
        logger.LogEntry(DatabaseModelAndRepositoryDtoConverter.ConvertRepositoryDtoToOPMFError(errorLogEntry, timeOfRecording));
    }

    private List<RepositoryLogEntryDto> ReadLogEntriesAsDtos(Func<OPMFLogDbCollection, IEnumerable<OPMFLog>> readLogEntries)
    {
        List<RepositoryLogEntryDto> logEntryDtos = [];
        databaseConnectionOpener.RunWithDatabaseConnectionOpenForThisCallOnly(databaseAdapter =>
            logEntryDtos = readLogEntries(databaseAdapter.OPMFLogDbCollection)
                .Select(DatabaseModelAndRepositoryDtoConverter.ConvertLogToRepositoryDto)
                .ToList());
        return logEntryDtos;
    }
}
