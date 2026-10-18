using System;
using System.Collections.Generic;
using System.Linq;
using Bunit;
using MediaManager.Dtos.Application;
using MediaManagerUI.Modules.ChannelBrowser;
using MediaManagerUI.Modules.LogBrowser;
using MediaManagerUI.Modules.VideoBrowser;
using MediaManagerUI.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.RazorComponents
{
    public class VideoBrowserComponentTests : MudBlazorComponentTestContext
    {
        private readonly FakeVideoBrowserModule _fakeVideoBrowserModule = new FakeVideoBrowserModule();

        private static ApplicationVideoWithChannelDto Video(
            string siteId,
            ApplicationVideoDownloadStatus status = ApplicationVideoDownloadStatus.New,
            bool isBeingDownloaded = false,
            TimeSpan? duration = null) => new ApplicationVideoWithChannelDto
            {
                Video = new ApplicationVideoMetadataDto
                {
                    SiteId = siteId,
                    Title = "Title of " + siteId,
                    Description = "Description of " + siteId,
                    DownloadStatus = status,
                    IsBeingDownloaded = isBeingDownloaded,
                    Duration = duration ?? new TimeSpan(0, 5, 0),
                    PublishedAt = new DateTime(2026, 9, 1),
                },
                Channel = new ApplicationChannelDto { SiteId = "c1", Name = "Channel of " + siteId, Description = "Description of the channel" },
            };

        private IRenderedComponent<_VideoBrowser> RenderBrowser(params ApplicationVideoWithChannelDto[] videos)
        {
            _fakeVideoBrowserModule.Results = videos.ToList();
            return RenderComponent<_VideoBrowser>(parameters => parameters.Add(component => component.VideoBrowserModule, _fakeVideoBrowserModule));
        }

        [Fact]
        public void WithoutAModuleTheBrowserThrowsPleaseSetGetResultsToAMethod()
        {
            var exception = Assert.Throws<Exception>(() => RenderComponent<_VideoBrowser>());

            Assert.Equal("Please set GetResults to a method", exception.Message);
        }

        [Fact]
        public void EveryVideoOfTheModuleIsShownWithItsTitleAndChannel()
        {
            var renderedBrowser = RenderBrowser(Video("v1"), Video("v2"), Video("v3"));

            Assert.Contains("Title of v1", renderedBrowser.Markup);
            Assert.Contains("Title of v2", renderedBrowser.Markup);
            Assert.Contains("Title of v3", renderedBrowser.Markup);
            Assert.Contains("Channel of v1", renderedBrowser.Markup);
        }

        [Fact]
        public void AVideoThatIsBeingDownloadedShowsThatInsteadOfTheStatusButtons()
        {
            var renderedBrowser = RenderBrowser(Video("v1", isBeingDownloaded: true));

            Assert.Contains("Video is being downloaded...", renderedBrowser.Markup);
            Assert.DoesNotContain("ToDownload", renderedBrowser.Markup);
        }

        [Fact]
        public void AVideoThatIsNotBeingDownloadedShowsOneButtonPerStatus()
        {
            var renderedBrowser = RenderBrowser(Video("v1"));

            var statusButtonLabels = renderedBrowser.FindAll("td[data-label='Status'] button").Select(button => button.TextContent.Trim());
            Assert.Equal(new[] { "New", "ToDownload", "Wait", "Ignore", "Downloaded" }, statusButtonLabels);
        }

        [Fact]
        public void TheStatusButtonsTheModuleDoesNotAllowAreDisabled()
        {
            _fakeVideoBrowserModule.UnselectableMetadataStatuses = new[] { ApplicationVideoDownloadStatus.New, ApplicationVideoDownloadStatus.Downloaded };
            var renderedBrowser = RenderBrowser(Video("v1"));

            var disabledStatusButtons = renderedBrowser.FindAll("td[data-label='Status'] button")
                .Where(button => button.HasAttribute("disabled"))
                .Select(button => button.TextContent.Trim());
            Assert.Equal(new[] { "New", "Downloaded" }, disabledStatusButtons);
        }

        [Fact]
        public void TheStatusButtonOfTheVideosStatusIsTheSecondaryColour()
        {
            var renderedBrowser = RenderBrowser(Video("v1", status: ApplicationVideoDownloadStatus.Wait));

            var buttonOfTheStatus = renderedBrowser.FindAll("td[data-label='Status'] button").Single(button => button.TextContent.Trim() == "Wait");
            var buttonOfAnotherStatus = renderedBrowser.FindAll("td[data-label='Status'] button").Single(button => button.TextContent.Trim() == "Ignore");
            Assert.Contains("mud-button-filled-secondary", buttonOfTheStatus.ClassName);
            Assert.DoesNotContain("secondary", buttonOfAnotherStatus.ClassName);
        }

        [Fact]
        public void SelectableStatusButtonsAreFilledAndTheSelectedUnselectableOneIsOutlinedAndTheOthersAreText()
        {
            _fakeVideoBrowserModule.UnselectableMetadataStatuses = new[] { ApplicationVideoDownloadStatus.New, ApplicationVideoDownloadStatus.Downloaded };
            var renderedBrowser = RenderBrowser(Video("v1", status: ApplicationVideoDownloadStatus.New));
            var buttons = renderedBrowser.FindAll("td[data-label='Status'] button");

            Assert.Contains("mud-button-outlined", buttons.Single(button => button.TextContent.Trim() == "New").ClassName);
            Assert.Contains("mud-button-text", buttons.Single(button => button.TextContent.Trim() == "Downloaded").ClassName);
            Assert.Contains("mud-button-filled", buttons.Single(button => button.TextContent.Trim() == "ToDownload").ClassName);
        }

        [Fact]
        public void ClickingAStatusButtonSchedulesTheStatusChangeOnTheModule()
        {
            var renderedBrowser = RenderBrowser(Video("v1"));

            renderedBrowser.FindAll("td[data-label='Status'] button").Single(button => button.TextContent.Trim() == "ToDownload").Click();

            Assert.Equal(new[] { (ApplicationVideoDownloadStatus.ToDownload, "v1") }, _fakeVideoBrowserModule.ScheduledStatusUpdates);
        }

        [Fact]
        public void AZeroDurationShowsNoData()
        {
            var renderedBrowser = RenderBrowser(Video("v1", duration: TimeSpan.Zero));

            Assert.Contains("No data", renderedBrowser.Markup);
        }

        [Fact]
        public void ADurationIsShown()
        {
            var renderedBrowser = RenderBrowser(Video("v1", duration: new TimeSpan(1, 2, 3)));

            Assert.Contains("01:02:03", renderedBrowser.Markup);
            Assert.DoesNotContain("No data", renderedBrowser.Markup);
        }

        [Fact]
        public void AVideoWithNoChannelStillRenders()
        {
            var videoWithoutChannel = Video("v1");
            videoWithoutChannel.Channel = null;

            var renderedBrowser = RenderBrowser(videoWithoutChannel);

            Assert.Contains("Title of v1", renderedBrowser.Markup);
        }

        [Fact]
        public void ClickingTheTitleOpensTheVideoDetailsDialogOfThatRow()
        {
            var dialogProvider = RenderComponent<MudDialogProvider>();
            var renderedBrowser = RenderBrowser(Video("v1"), Video("v2"));

            renderedBrowser.FindAll("td[data-label='Title'] button").Single(button => button.TextContent.Trim() == "Title of v2").Click();

            dialogProvider.WaitForAssertion(() => Assert.Contains("Title: ", dialogProvider.Markup));
            Assert.Contains("Title of v2", dialogProvider.Markup);
        }

        [Fact]
        public void ClickingTheChannelNameOpensTheChannelDetailsDialogOfThatRow()
        {
            var dialogProvider = RenderComponent<MudDialogProvider>();
            var renderedBrowser = RenderBrowser(Video("v1"), Video("v2"));

            renderedBrowser.FindAll("td[data-label='Channel'] button").Single(button => button.TextContent.Trim() == "Channel of v2").Click();

            dialogProvider.WaitForAssertion(() => Assert.Contains("Channel Name: ", dialogProvider.Markup));
            Assert.Contains("Channel of v2", dialogProvider.Markup);
        }

        [Fact]
        public void TheChangedCallbackIsRaisedOnlyWhenTheModuleChanges()
        {
            var modulesReported = new List<IVideoBrowserModule>();
            var renderedBrowser = RenderComponent<_VideoBrowser>(parameters => parameters
                .Add(component => component.VideoBrowserModuleChanged, EventCallback.Factory.Create<IVideoBrowserModule>(this, modulesReported.Add))
                .Add(component => component.VideoBrowserModule, _fakeVideoBrowserModule));

            renderedBrowser.SetParametersAndRender(parameters => parameters.Add(component => component.VideoBrowserModule, _fakeVideoBrowserModule));
            Assert.Single(modulesReported);

            var anotherModule = new FakeVideoBrowserModule();
            renderedBrowser.SetParametersAndRender(parameters => parameters.Add(component => component.VideoBrowserModule, anotherModule));
            Assert.Equal(2, modulesReported.Count);
        }

        [Fact]
        public void TheToolBarContentIsShown()
        {
            var renderedBrowser = RenderComponent<_VideoBrowser>(parameters => parameters
                .Add(component => component.VideoBrowserModule, _fakeVideoBrowserModule)
                .Add(component => component.TableToolBarContent, builder => builder.AddMarkupContent(0, "<span>the tool bar</span>")));

            Assert.Contains("the tool bar", renderedBrowser.Markup);
        }
    }

    public class ChannelBrowserComponentTests : MudBlazorComponentTestContext
    {
        private readonly FakeChannelBrowserModule _fakeChannelBrowserModule = new FakeChannelBrowserModule();

        private static ApplicationChannelDto ChannelNamed(string siteId, bool isBlacklisted = false) => new ApplicationChannelDto
        {
            SiteId = siteId,
            Name = "Name of " + siteId,
            Description = "Description of " + siteId,
            IsBlacklisted = isBlacklisted,
            LatestVideoPublishedAt = new DateTime(2026, 9, 1),
        };

        private IRenderedComponent<_ChannelBrowser> RenderBrowser(params ApplicationChannelDto[] channels)
        {
            _fakeChannelBrowserModule.Results = channels.ToList();
            return RenderComponent<_ChannelBrowser>(parameters => parameters.Add(component => component.ChannelBrowserModule, _fakeChannelBrowserModule));
        }

        [Fact]
        public void WithoutAModuleTheBrowserThrowsPleaseSetGetResultsToAMethod()
        {
            var exception = Assert.Throws<Exception>(() => RenderComponent<_ChannelBrowser>());

            Assert.Equal("Please set GetResults to a method", exception.Message);
        }

        [Fact]
        public void EveryChannelOfTheModuleIsShownWithItsName()
        {
            var renderedBrowser = RenderBrowser(ChannelNamed("c1"), ChannelNamed("c2"));

            Assert.Contains("Name of c1", renderedBrowser.Markup);
            Assert.Contains("Name of c2", renderedBrowser.Markup);
        }

        [Fact]
        public void TheBlacklistSwitchSchedulesTheChangeOnTheModuleAndFlipsTheChannel()
        {
            var channel = ChannelNamed("c1", isBlacklisted: false);
            var renderedBrowser = RenderBrowser(channel);

            renderedBrowser.Find("td[data-label='Not blacklisted'] input").Change(true);

            Assert.True(channel.IsBlacklisted);
            Assert.Equal(new[] { "c1" }, _fakeChannelBrowserModule.SiteIdsOfChannelsWhoseBlacklistChangeWasScheduled);
        }

        [Fact]
        public void ClickingTheNameOpensTheChannelDetailsDialogOfThatRow()
        {
            var dialogProvider = RenderComponent<MudDialogProvider>();
            var renderedBrowser = RenderBrowser(ChannelNamed("c1"), ChannelNamed("c2"));

            renderedBrowser.FindAll("td[data-label='Name'] button").Single(button => button.TextContent.Trim() == "Name of c2").Click();

            dialogProvider.WaitForAssertion(() => Assert.Contains("Channel Name: ", dialogProvider.Markup));
            Assert.Contains("Name of c2", dialogProvider.Markup);
        }

        [Fact]
        public void TheChangedCallbackIsRaisedOnlyWhenTheModuleChanges()
        {
            var modulesReported = new List<IChannelBrowserModule>();
            var renderedBrowser = RenderComponent<_ChannelBrowser>(parameters => parameters
                .Add(component => component.ChannelBrowserModuleChanged, EventCallback.Factory.Create<IChannelBrowserModule>(this, modulesReported.Add))
                .Add(component => component.ChannelBrowserModule, _fakeChannelBrowserModule));

            renderedBrowser.SetParametersAndRender(parameters => parameters.Add(component => component.ChannelBrowserModule, _fakeChannelBrowserModule));

            Assert.Single(modulesReported);
        }
    }

    public class LogBrowserComponentTests : MudBlazorComponentTestContext
    {
        private readonly FakeLogBrowserModule _fakeLogBrowserModule = new FakeLogBrowserModule();

        private IRenderedComponent<_LogBrowser> RenderBrowser(params ApplicationLogEntryDto[] logs)
        {
            _fakeLogBrowserModule.Results = logs.ToList();
            return RenderComponent<_LogBrowser>(parameters => parameters.Add(component => component.LogBrowserModule, _fakeLogBrowserModule));
        }

        [Fact]
        public void WithoutAModuleTheBrowserThrowsPleaseSetGetResultsToAMethod()
        {
            var exception = Assert.Throws<Exception>(() => RenderComponent<_LogBrowser>());

            Assert.Equal("Please set GetResults to a method", exception.Message);
        }

        [Fact]
        public void EveryLogOfTheModuleIsShownWithItsMessage()
        {
            var renderedBrowser = RenderBrowser(
                new ApplicationLogEntryDto { Message = "first message", CreatedAt = new DateTime(2026, 9, 1) },
                new ApplicationLogEntryDto { Message = "second message", CreatedAt = new DateTime(2026, 9, 2) });

            Assert.Contains("first message", renderedBrowser.Markup);
            Assert.Contains("second message", renderedBrowser.Markup);
        }

        [Fact]
        public void ClickingDetailsOpensTheLogDetailsDialogOfThatRow()
        {
            var dialogProvider = RenderComponent<MudDialogProvider>();
            var renderedBrowser = RenderBrowser(
                new ApplicationLogEntryDto { Message = "first message", VariablesAsIndentedJson = "{}" },
                new ApplicationLogEntryDto { Message = "second message", VariablesAsIndentedJson = "{}" });

            renderedBrowser.FindAll("button").Where(button => button.TextContent.Trim() == "Details").Last().Click();

            dialogProvider.WaitForAssertion(() => Assert.Contains("Logged date: ", dialogProvider.Markup));
            Assert.Contains("second message", dialogProvider.Markup);
        }

        [Fact]
        public void TheChangedCallbackIsRaisedOnlyWhenTheModuleChanges()
        {
            var modulesReported = new List<ILogBrowserModule>();
            var renderedBrowser = RenderComponent<_LogBrowser>(parameters => parameters
                .Add(component => component.LogBrowserModuleChanged, EventCallback.Factory.Create<ILogBrowserModule>(this, modulesReported.Add))
                .Add(component => component.LogBrowserModule, _fakeLogBrowserModule));

            renderedBrowser.SetParametersAndRender(parameters => parameters.Add(component => component.LogBrowserModule, _fakeLogBrowserModule));

            Assert.Single(modulesReported);
        }
    }
}
