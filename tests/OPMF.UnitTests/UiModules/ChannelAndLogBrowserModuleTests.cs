using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaManager.Dtos.Application;
using MediaManagerUI.Modules.ChannelBrowser;
using MediaManagerUI.Modules.LogBrowser;
using Microsoft.Extensions.Time.Testing;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.UiModules
{
    public class AllChannelsModuleTests
    {
        private readonly FakeChannelBrowsingUseCases _channelBrowsingUseCases = new FakeChannelBrowsingUseCases();
        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(2026, 10, 18, 9, 30, 15, TimeSpan.Zero));
        private readonly AllChannelsModule _allChannelsModule;

        public AllChannelsModuleTests()
        {
            _allChannelsModule = new AllChannelsModule(_channelBrowsingUseCases, _fakeTimeProvider);
        }

        [Fact]
        public async Task TheResultsAreAllTheChannelsOfTheUseCase()
        {
            _channelBrowsingUseCases.ChannelsToReturn.Add(new ApplicationChannelDto { SiteId = "c1", Name = "One" });
            _channelBrowsingUseCases.ChannelsToReturn.Add(new ApplicationChannelDto { SiteId = "c2", Name = "Two" });

            await _allChannelsModule.GetResultsAsync();

            Assert.Equal(new[] { "One", "Two" }, _allChannelsModule.Results.Select(channel => channel.Name));
        }

        [Fact]
        public async Task LoadingAgainReplacesTheEarlierResults()
        {
            _channelBrowsingUseCases.ChannelsToReturn.Add(new ApplicationChannelDto { SiteId = "c1", Name = "One" });
            await _allChannelsModule.GetResultsAsync();
            _channelBrowsingUseCases.ChannelsToReturn.Clear();
            _channelBrowsingUseCases.ChannelsToReturn.Add(new ApplicationChannelDto { SiteId = "c2", Name = "Two" });

            await _allChannelsModule.GetResultsAsync();

            Assert.Equal("Two", Assert.Single(_allChannelsModule.Results).Name);
        }

        [Fact]
        public async Task IsLoadingIsTrueWhileLoadingAndFalseAfterwards()
        {
            using var theUseCaseWasAsked = new ManualResetEventSlim();
            using var theTestChecked = new ManualResetEventSlim();
            _channelBrowsingUseCases.OnGetAllChannels = () =>
            {
                theUseCaseWasAsked.Set();
                theTestChecked.Wait(TimeSpan.FromSeconds(10));
                return new List<ApplicationChannelDto>();
            };

            var loading = _allChannelsModule.GetResultsAsync();
            Assert.True(theUseCaseWasAsked.Wait(TimeSpan.FromSeconds(10)));
            var isLoadingWhileTheUseCaseWasWorking = _allChannelsModule.IsLoading;
            theTestChecked.Set();
            await loading;

            Assert.True(isLoadingWhileTheUseCaseWasWorking);
            Assert.False(_allChannelsModule.IsLoading);
        }

        [Fact]
        public async Task IsLoadingIsFalseAgainAfterTheUseCaseFailedAndTheExceptionPropagates()
        {
            var exceptionOfTheUseCase = new InvalidOperationException("the database is locked");
            _channelBrowsingUseCases.OnGetAllChannels = () => throw exceptionOfTheUseCase;

            var exceptionThatPropagated = await Assert.ThrowsAsync<InvalidOperationException>(_allChannelsModule.GetResultsAsync);

            Assert.Same(exceptionOfTheUseCase, exceptionThatPropagated);
            Assert.False(_allChannelsModule.IsLoading);
        }

        [Fact]
        public void AfterOneSecondEveryScheduledBlacklistChangeIsSentInOneCall()
        {
            var firstChannel = new ApplicationChannelDto { SiteId = "c1" };
            var secondChannel = new ApplicationChannelDto { SiteId = "c2" };
            _allChannelsModule.ScheduleChannelBacklistChange(firstChannel);
            _allChannelsModule.ScheduleChannelBacklistChange(secondChannel);
            Assert.Empty(_channelBrowsingUseCases.BlacklistUpdates);

            _fakeTimeProvider.Advance(TimeSpan.FromSeconds(1));

            var oneCall = Assert.Single(_channelBrowsingUseCases.BlacklistUpdates);
            Assert.Equal(new[] { "c1", "c2" }, oneCall.Select(channel => channel.SiteId));
        }

        [Fact]
        public void ScheduledBlacklistChangesAreSentOnlyOnce()
        {
            _allChannelsModule.ScheduleChannelBacklistChange(new ApplicationChannelDto { SiteId = "c1" });

            _fakeTimeProvider.Advance(TimeSpan.FromSeconds(3));

            Assert.Single(_channelBrowsingUseCases.BlacklistUpdates);
        }

        [Fact]
        public void NothingIsSentWhenNoBlacklistChangeIsScheduled()
        {
            _fakeTimeProvider.Advance(TimeSpan.FromSeconds(5));

            Assert.Empty(_channelBrowsingUseCases.BlacklistUpdates);
        }
    }

    public class LogBrowserModuleTests
    {
        private static readonly DateTime StartOfRange = new DateTime(2026, 9, 1, 8, 0, 0);
        private static readonly DateTime EndOfRange = new DateTime(2026, 10, 1, 8, 0, 0);

        private readonly FakeLogBrowsingUseCases _logBrowsingUseCases = new FakeLogBrowsingUseCases();

        [Fact]
        public async Task TheWarningModuleAsksTheUseCaseForTheWarningsOfTheRange()
        {
            _logBrowsingUseCases.WarningsToReturn.Add(new ApplicationLogEntryDto { Message = "a warning", LogType = ApplicationLogEntryType.Warning });
            var module = new WarningLogBrowserModule(_logBrowsingUseCases);

            await module.GetResultsAsync(StartOfRange, EndOfRange);

            Assert.Equal(new[] { $"GetWarningsLoggedBetween({StartOfRange:O}, {EndOfRange:O})" }, _logBrowsingUseCases.CallsMade);
            Assert.Equal("a warning", Assert.Single(module.Results).Message);
        }

        [Fact]
        public async Task TheErrorModuleAsksTheUseCaseForTheErrorsOfTheRange()
        {
            _logBrowsingUseCases.ErrorsToReturn.Add(new ApplicationLogEntryDto { Message = "an error", LogType = ApplicationLogEntryType.Error });
            var module = new ErrorLogBrowserModule(_logBrowsingUseCases);

            await module.GetResultsAsync(StartOfRange, EndOfRange);

            Assert.Equal(new[] { $"GetErrorsLoggedBetween({StartOfRange:O}, {EndOfRange:O})" }, _logBrowsingUseCases.CallsMade);
            Assert.Equal("an error", Assert.Single(module.Results).Message);
        }

        [Fact]
        public async Task LoadingAgainReplacesTheEarlierResults()
        {
            _logBrowsingUseCases.ErrorsToReturn.Add(new ApplicationLogEntryDto { Message = "first" });
            var module = new ErrorLogBrowserModule(_logBrowsingUseCases);
            await module.GetResultsAsync(StartOfRange, EndOfRange);
            _logBrowsingUseCases.ErrorsToReturn.Clear();
            _logBrowsingUseCases.ErrorsToReturn.Add(new ApplicationLogEntryDto { Message = "second" });

            await module.GetResultsAsync(StartOfRange, EndOfRange);

            Assert.Equal("second", Assert.Single(module.Results).Message);
        }

        [Fact]
        public async Task IsLoadingIsFalseAgainAfterTheUseCaseFailedAndTheExceptionPropagates()
        {
            _logBrowsingUseCases.OnGetErrors = () => throw new InvalidOperationException("the database is locked");
            var module = new ErrorLogBrowserModule(_logBrowsingUseCases);

            await Assert.ThrowsAsync<InvalidOperationException>(() => module.GetResultsAsync(StartOfRange, EndOfRange));

            Assert.False(module.IsLoading);
        }

        [Fact]
        public async Task IsLoadingIsTrueWhileLoadingAndFalseAfterwards()
        {
            using var theUseCaseWasAsked = new ManualResetEventSlim();
            using var theTestChecked = new ManualResetEventSlim();
            _logBrowsingUseCases.OnGetErrors = () =>
            {
                theUseCaseWasAsked.Set();
                theTestChecked.Wait(TimeSpan.FromSeconds(10));
                return new List<ApplicationLogEntryDto>();
            };
            var module = new ErrorLogBrowserModule(_logBrowsingUseCases);

            var loading = module.GetResultsAsync(StartOfRange, EndOfRange);
            Assert.True(theUseCaseWasAsked.Wait(TimeSpan.FromSeconds(10)));
            var isLoadingWhileTheUseCaseWasWorking = module.IsLoading;
            theTestChecked.Set();
            await loading;

            Assert.True(isLoadingWhileTheUseCaseWasWorking);
            Assert.False(module.IsLoading);
        }
    }
}
