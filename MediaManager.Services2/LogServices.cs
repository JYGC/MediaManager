using MediaManager.Dtos.Repositories;
using MediaManager.Dtos.Services;
using MediaManager.Repositories;

namespace MediaManager.Services2
{
    public interface ILogServices
    {
        List<ServiceLogEntryDto> GetWarningsLoggedBetween(DateTime startDateTime, DateTime endDateTime);
        List<ServiceLogEntryDto> GetErrorsLoggedBetween(DateTime startDateTime, DateTime endDateTime);
        void RecordError(string errorMessage, string fullExceptionText);
    }
    public class LogServices(ILogRepository logRepository) : ILogServices
    {
        public List<ServiceLogEntryDto> GetWarningsLoggedBetween(DateTime startDateTime, DateTime endDateTime)
        {
            return logRepository.GetWarningsLoggedBetween(startDateTime, endDateTime).Select(RepositoryDtoAndServiceDtoConverter.ConvertLogEntryToServiceDto).ToList();
        }

        public List<ServiceLogEntryDto> GetErrorsLoggedBetween(DateTime startDateTime, DateTime endDateTime)
        {
            return logRepository.GetErrorsLoggedBetween(startDateTime, endDateTime).Select(RepositoryDtoAndServiceDtoConverter.ConvertLogEntryToServiceDto).ToList();
        }

        public void RecordError(string errorMessage, string fullExceptionText)
        {
            logRepository.RecordErrorLogEntry(new RepositoryErrorLogEntryToRecordDto { Message = errorMessage, ExceptionText = fullExceptionText });
        }
    }
}
