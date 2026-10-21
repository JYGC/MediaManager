using System;
using System.Collections.Generic;
using System.Linq;
using Bunit;
using MediaManager.Dtos.Application;
using MediaManager.Initializations;
using MediaManagerUI.Pages.Channels;
using MediaManagerUI.Pages.Logs;
using MediaManagerUI.Pages.Videos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using MudBlazor;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.RazorComponents
{
    public abstract class PageComponentTestsBase : MudBlazorComponentTestContext
    {
        protected static readonly DateTimeOffset InstantTheFakeClockStartsAt = new DateTimeOffset(2026, 10, 18, 9, 30, 15, TimeSpan.Zero);

        protected readonly FakeVideoBrowsingUseCases VideoBrowsingUseCases = new FakeVideoBrowsingUseCases();
        protected readonly FakeChannelBrowsingUseCases ChannelBrowsingUseCases = new FakeChannelBrowsingUseCases();
        protected readonly FakeLogBrowsingUseCases LogBrowsingUseCases = new FakeLogBrowsingUseCases();
        protected readonly FakeTimeProvider FakeTimeProvider = new FakeTimeProvider(InstantTheFakeClockStartsAt);

        protected PageComponentTestsBase()
        {
            Services.AddSingleton<IVideoBrowsingUseCases>(VideoBrowsingUseCases);
            Services.AddSingleton<IChannelBrowsingUseCases>(ChannelBrowsingUseCases);
            Services.AddSingleton<ILogBrowsingUseCases>(LogBrowsingUseCases);
            Services.AddSingleton<TimeProvider>(FakeTimeProvider);
        }

        protected static List<ApplicationVideoWithChannelDto> VideosWithChannels(params string[] titles) =>
            titles.Select((title, number) => new ApplicationVideoWithChannelDto
            {
                Video = new ApplicationVideoMetadataDto { SiteId = "v" + number, Title = title, PublishedAt = new DateTime(2026, 9, 1) },
                Channel = new ApplicationChannelDto { SiteId = "c1", Name = "The channel" },
            }).ToList();
    }

    public class VideoListPageTests : PageComponentTestsBase
    {
        [Fact]
        public void TheNewVideosPageLoadsTheNewVideosAndShowsThem()
        {
            VideoBrowsingUseCases.OnGetNewVideosWithChannels = (_, _) => VideosWithChannels("A new video");

            var renderedPage = RenderComponent<NewVideos>();

            renderedPage.WaitForAssertion(() => Assert.Contains("A new video", renderedPage.Markup));
            Assert.Equal(new[] { "GetNewVideosWithChannels(0, 10000)" }, VideoBrowsingUseCases.CallsMade);
        }

        [Fact]
        public void TheNewVideosPageShowsItsTitleInTheToolBar()
        {
            var renderedPage = RenderComponent<NewVideos>();

            renderedPage.WaitForAssertion(() => Assert.Contains("New Videos", renderedPage.Markup));
        }

        [Fact]
        public void TheDownloadQueuePageLoadsTheQueuedAndWaitingVideosAndShowsThem()
        {
            VideoBrowsingUseCases.OnGetVideosWithChannelsQueuedToDownloadOrWaiting = (_, _) => VideosWithChannels("A queued video");

            var renderedPage = RenderComponent<DownloadQueue>();

            renderedPage.WaitForAssertion(() => Assert.Contains("A queued video", renderedPage.Markup));
            Assert.Equal(new[] { "GetVideosWithChannelsQueuedToDownloadOrWaiting(0, 10000)" }, VideoBrowsingUseCases.CallsMade);
        }

        [Fact]
        public void TheFindVideosPageLoadsNothingUntilFindIsClicked()
        {
            RenderComponent<FindVideos>();

            Assert.Empty(VideoBrowsingUseCases.CallsMade);
        }

        [Fact]
        public void ClickingFindSearchesByTheTitleTypedIn()
        {
            VideoBrowsingUseCases.OnGetVideosWithChannelsWhereVideoTitleContainsWord = (_, _, _) => VideosWithChannels("A found video");
            var renderedPage = RenderComponent<FindVideos>();
            renderedPage.FindAll("input")[0].Change("keyword");

            renderedPage.FindAll("button").Single(button => button.TextContent.Trim() == "Find").Click();

            renderedPage.WaitForAssertion(() => Assert.Contains("A found video", renderedPage.Markup));
            Assert.Equal(new[] { "GetVideosWithChannelsWhereVideoTitleContainsWord(keyword, 0, 10000)" }, VideoBrowsingUseCases.CallsMade);
        }

        [Fact]
        public void ClickingFindSearchesByTheChannelNameAndTheTitleTypedIn()
        {
            var renderedPage = RenderComponent<FindVideos>();
            renderedPage.FindAll("input")[0].Change("titleWord");
            renderedPage.FindAll("input")[1].Change("channelWord");

            renderedPage.FindAll("button").Single(button => button.TextContent.Trim() == "Find").Click();

            renderedPage.WaitForAssertion(() =>
                Assert.Equal(new[] { "GetVideosWithChannelsWhereChannelNameAndVideoTitleContainWords(channelWord, titleWord, 0, 10000)" }, VideoBrowsingUseCases.CallsMade));
        }

        [Fact]
        public void TheGetSingleVideoPageLoadsNothingUntilFindIsClicked()
        {
            RenderComponent<GetSingleVideo>();

            Assert.Empty(VideoBrowsingUseCases.CallsMade);
        }

        [Fact]
        public void ClickingFindOnTheGetSingleVideoPageGetsTheVideoByTheUrlTypedIn()
        {
            VideoBrowsingUseCases.OnGetVideoWithChannelByUrl = _ => VideosWithChannels("A single video").Single();
            var renderedPage = RenderComponent<GetSingleVideo>();
            renderedPage.Find("input").Change("https://www.youtube.com/watch?v=v0");

            renderedPage.FindAll("button").Single(button => button.TextContent.Trim() == "Find").Click();

            renderedPage.WaitForAssertion(() => Assert.Contains("A single video", renderedPage.Markup));
            Assert.Equal(new[] { "GetVideoWithChannelByUrlFetchingFromVideoSiteWhenNotStored(https://www.youtube.com/watch?v=v0)" }, VideoBrowsingUseCases.CallsMade);
        }
    }

    public class ChannelPageTests : PageComponentTestsBase
    {
        [Fact]
        public void TheAllChannelsPageLoadsAllChannelsAndShowsThem()
        {
            ChannelBrowsingUseCases.ChannelsToReturn.Add(new ApplicationChannelDto { SiteId = "c1", Name = "First channel", LatestVideoPublishedAt = new DateTime(2026, 9, 1) });
            ChannelBrowsingUseCases.ChannelsToReturn.Add(new ApplicationChannelDto { SiteId = "c2", Name = "Second channel", LatestVideoPublishedAt = new DateTime(2026, 9, 2) });

            var renderedPage = RenderComponent<AllChannels>();

            renderedPage.WaitForAssertion(() =>
            {
                Assert.Contains("First channel", renderedPage.Markup);
                Assert.Contains("Second channel", renderedPage.Markup);
            });
        }

        [Fact]
        public void TheAddChannelsPageShowsItsHeading()
        {
            var renderedPage = RenderComponent<AddChannels>();

            Assert.Contains("AddChannels", renderedPage.Markup);
        }
    }

    public class LogPageTests : PageComponentTestsBase
    {
        private DateTime Now => FakeTimeProvider.GetLocalNow().LocalDateTime;

        [Fact]
        public void TheErrorsPageLoadsTheErrorsOfTheLastMonthByDefault()
        {
            RenderComponent<MudPopoverProvider>();
            var renderedPage = RenderComponent<Errors>();

            renderedPage.WaitForAssertion(() => Assert.Equal(new[] { $"GetErrorsLoggedBetween({Now.AddMonths(-1):O}, {Now:O})" }, LogBrowsingUseCases.CallsMade));
        }

        [Fact]
        public void TheWarningsPageLoadsTheWarningsOfTheLastMonthByDefault()
        {
            RenderComponent<MudPopoverProvider>();
            var renderedPage = RenderComponent<Warnings>();

            renderedPage.WaitForAssertion(() => Assert.Equal(new[] { $"GetWarningsLoggedBetween({Now.AddMonths(-1):O}, {Now:O})" }, LogBrowsingUseCases.CallsMade));
        }

        [Fact]
        public void TheErrorsPageShowsTheErrorsItLoaded()
        {
            LogBrowsingUseCases.ErrorsToReturn.Add(new ApplicationLogEntryDto { Message = "Disk is full", LogType = ApplicationLogEntryType.Error, CreatedAt = Now.AddDays(-2) });
            RenderComponent<MudPopoverProvider>();

            var renderedPage = RenderComponent<Errors>();

            renderedPage.WaitForAssertion(() => Assert.Contains("Disk is full", renderedPage.Markup));
        }

        [Fact]
        public void TheWarningsPageShowsTheWarningsItLoaded()
        {
            LogBrowsingUseCases.WarningsToReturn.Add(new ApplicationLogEntryDto { Message = "Slow download", LogType = ApplicationLogEntryType.Warning, CreatedAt = Now.AddDays(-2) });
            RenderComponent<MudPopoverProvider>();

            var renderedPage = RenderComponent<Warnings>();

            renderedPage.WaitForAssertion(() => Assert.Contains("Slow download", renderedPage.Markup));
        }

        [Fact]
        public void ClosingTheDateRangePickerLoadsTheLogsAgain()
        {
            RenderComponent<MudPopoverProvider>();
            var renderedPage = RenderComponent<Errors>();
            renderedPage.WaitForAssertion(() => Assert.Single(LogBrowsingUseCases.CallsMade));

            var picker = renderedPage.FindComponent<MudDateRangePicker>();
            renderedPage.InvokeAsync(() => picker.Instance.PickerClosed.InvokeAsync(null));

            renderedPage.WaitForAssertion(() => Assert.Equal(2, LogBrowsingUseCases.CallsMade.Count));
        }

        [Fact]
        public void ClosingTheDateRangePickerWhileTheRangeIsIncompleteDoesNotLoadTheLogsAgain()
        {
            RenderComponent<MudPopoverProvider>();
            var renderedPage = RenderComponent<Errors>();
            renderedPage.WaitForAssertion(() => Assert.Single(LogBrowsingUseCases.CallsMade));
            var picker = renderedPage.FindComponent<MudDateRangePicker>();
            renderedPage.InvokeAsync(() => picker.Instance.DateRangeChanged.InvokeAsync(new DateRange(Now.AddDays(-3), null)));

            renderedPage.InvokeAsync(() => picker.Instance.PickerClosed.InvokeAsync(null));

            Assert.Single(LogBrowsingUseCases.CallsMade);
        }
    }
}
