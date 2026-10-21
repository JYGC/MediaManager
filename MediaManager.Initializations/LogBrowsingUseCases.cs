using MediaManager.Dtos.Application;
using MediaManager.Services2;

namespace MediaManager.Initializations;

public interface ILogBrowsingUseCases
{
    List<ApplicationLogEntryDto> GetWarningsLoggedBetween(DateTime startDateTime, DateTime endDateTime);
    List<ApplicationLogEntryDto> GetErrorsLoggedBetween(DateTime startDateTime, DateTime endDateTime);
}

public class LogBrowsingUseCases(ILogServices logServices) : ILogBrowsingUseCases
{
    public List<ApplicationLogEntryDto> GetWarningsLoggedBetween(DateTime startDateTime, DateTime endDateTime)
    {
        return logServices.GetWarningsLoggedBetween(startDateTime, endDateTime).Select(ServiceDtoAndApplicationDtoConverter.ConvertLogEntryToApplicationDto).ToList();
    }

    public List<ApplicationLogEntryDto> GetErrorsLoggedBetween(DateTime startDateTime, DateTime endDateTime)
    {
        return logServices.GetErrorsLoggedBetween(startDateTime, endDateTime).Select(ServiceDtoAndApplicationDtoConverter.ConvertLogEntryToApplicationDto).ToList();
    }
}
