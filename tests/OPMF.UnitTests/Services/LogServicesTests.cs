using System;
using System.Linq;
using MediaManager.Dtos.Repositories;
using MediaManager.Dtos.Services;
using MediaManager.Services2;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Services
{
    public class LogServicesTests
    {
        private static readonly DateTime StartOfRange = new DateTime(2026, 9, 1);
        private static readonly DateTime EndOfRange = new DateTime(2026, 10, 1);

        private readonly FakeLogRepository _fakeLogRepository = new FakeLogRepository();
        private readonly LogServices _logServices;

        public LogServicesTests()
        {
            _logServices = new LogServices(_fakeLogRepository);
        }

        [Fact]
        public void GetWarningsLoggedBetweenAsksTheRepositoryForTheRangeAndReturnsServiceDtos()
        {
            _fakeLogRepository.Warnings.Add(new RepositoryLogEntryDto { Id = "w1", Message = "a warning", LogType = RepositoryLogEntryType.Warning });

            var warnings = _logServices.GetWarningsLoggedBetween(StartOfRange, EndOfRange);

            Assert.Equal((StartOfRange, EndOfRange), Assert.Single(_fakeLogRepository.RangesRequested));
            var warning = Assert.Single(warnings);
            Assert.Equal("a warning", warning.Message);
            Assert.Equal(ServiceLogEntryType.Warning, warning.LogType);
        }

        [Fact]
        public void GetErrorsLoggedBetweenAsksTheRepositoryForTheRangeAndReturnsServiceDtos()
        {
            _fakeLogRepository.Errors.Add(new RepositoryLogEntryDto { Id = "e1", Message = "an error", LogType = RepositoryLogEntryType.Error });

            var errors = _logServices.GetErrorsLoggedBetween(StartOfRange, EndOfRange);

            Assert.Equal((StartOfRange, EndOfRange), Assert.Single(_fakeLogRepository.RangesRequested));
            var error = Assert.Single(errors);
            Assert.Equal("an error", error.Message);
            Assert.Equal(ServiceLogEntryType.Error, error.LogType);
        }

        [Fact]
        public void RecordErrorPassesTheMessageAndTheExceptionTextToTheRepository()
        {
            _logServices.RecordError("it failed", "the full exception text");

            var recordedError = Assert.Single(_fakeLogRepository.RecordedErrors);
            Assert.Equal("it failed", recordedError.Message);
            Assert.Equal("the full exception text", recordedError.ExceptionText);
            Assert.Empty(recordedError.VariablesAsJsonText);
        }
    }
}
