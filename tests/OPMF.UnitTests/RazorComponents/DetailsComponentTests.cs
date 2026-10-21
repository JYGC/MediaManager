using System;
using System.Collections.Generic;
using System.Linq;
using Bunit;
using MediaManager.Dtos.Application;
using MediaManagerUI.Modules.VideoBrowser;
using MediaManagerUI.Shared;
using MudBlazor;
using Microsoft.AspNetCore.Components;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.RazorComponents
{
    public class MultilineTextAsParagraphsTests : MudBlazorComponentTestContext
    {
        [Fact]
        public void EachLineOfTheTextIsOneParagraph()
        {
            var renderedText = RenderComponent<_MultilineTextAsParagraphs>(parameters => parameters.Add(component => component.Text, "first line\nsecond line\nthird line"));

            Assert.Equal(new[] { "first line", "second line", "third line" }, renderedText.FindAll("p").Select(paragraph => paragraph.TextContent));
        }

        [Fact]
        public void ATextWithoutLineBreaksIsOneParagraph()
        {
            var renderedText = RenderComponent<_MultilineTextAsParagraphs>(parameters => parameters.Add(component => component.Text, "just one line"));

            Assert.Equal("just one line", Assert.Single(renderedText.FindAll("p")).TextContent);
        }
    }

    public class ChannelDetailsTests : MudBlazorComponentTestContext
    {
        private static ApplicationChannelDto ChannelNamed(string name, bool isNotFound = false, string description = "line one\nline two") => new ApplicationChannelDto
        {
            SiteId = "c1",
            Name = name,
            Url = "https://www.youtube.com/channel/c1",
            Description = description,
            IsNotFoundOnVideoSite = isNotFound,
            Thumbnail = new ApplicationThumbnailDto { Url = "https://thumbnails/c1", WidthInPixels = 120, HeightInPixels = 90 },
        };

        [Fact]
        public void TheDetailsShowTheNameAndTheUrlAndTheDescriptionLines()
        {
            var renderedDetails = RenderComponent<_ChannelDetails>(parameters => parameters.Add(component => component.Channel, ChannelNamed("The channel")));

            Assert.Contains("The channel", renderedDetails.Markup);
            Assert.Contains("https://www.youtube.com/channel/c1", renderedDetails.Markup);
            Assert.Contains("line one", renderedDetails.Markup);
            Assert.Contains("line two", renderedDetails.Markup);
        }

        [Fact]
        public void AChannelWithNoDescriptionStillRenders()
        {
            var renderedDetails = RenderComponent<_ChannelDetails>(parameters => parameters.Add(component => component.Channel, ChannelNamed("The channel", description: null)));

            Assert.Contains("The channel", renderedDetails.Markup);
        }

        [Fact]
        public void AChannelThatExistsShowsChannelExists()
        {
            var renderedDetails = RenderComponent<_ChannelDetails>(parameters => parameters.Add(component => component.Channel, ChannelNamed("The channel", isNotFound: false)));

            Assert.Contains("Channel Exists", renderedDetails.Markup);
            Assert.DoesNotContain("Channel Not Found", renderedDetails.Markup);
        }

        [Fact]
        public void AChannelThatWasNotFoundShowsChannelNotFound()
        {
            var renderedDetails = RenderComponent<_ChannelDetails>(parameters => parameters.Add(component => component.Channel, ChannelNamed("The channel", isNotFound: true)));

            Assert.Contains("Channel Not Found", renderedDetails.Markup);
        }

        [Fact]
        public void WithNoChannelNothingIsShown()
        {
            var renderedDetails = RenderComponent<_ChannelDetails>();

            Assert.Equal(string.Empty, renderedDetails.Markup.Trim());
        }

        [Fact]
        public void TheChangedCallbackIsRaisedOnlyWhenTheChannelChanges()
        {
            var channelsReported = new List<ApplicationChannelDto>();
            var firstChannel = ChannelNamed("First");
            var renderedDetails = RenderComponent<_ChannelDetails>(parameters => parameters
                .Add(component => component.ChannelChanged, EventCallback.Factory.Create<ApplicationChannelDto>(this, channelsReported.Add))
                .Add(component => component.Channel, firstChannel));

            renderedDetails.SetParametersAndRender(parameters => parameters.Add(component => component.Channel, firstChannel));
            Assert.Single(channelsReported);

            var secondChannel = ChannelNamed("Second");
            renderedDetails.SetParametersAndRender(parameters => parameters.Add(component => component.Channel, secondChannel));
            Assert.Equal(new[] { firstChannel, secondChannel }, channelsReported);
        }
    }

    public class LogDetailsTests : MudBlazorComponentTestContext
    {
        private static ApplicationLogEntryDto LogWith(string exceptionText) => new ApplicationLogEntryDto
        {
            Message = "something happened",
            LogType = ApplicationLogEntryType.Warning,
            CreatedAt = new DateTime(2026, 9, 1, 10, 30, 0),
            ExceptionText = exceptionText,
            VariablesAsIndentedJson = "{\n  \"metadata.Title\": \"The video\"\n}",
        };

        [Fact]
        public void TheDetailsShowTheMessageTheTypeAndTheVariablesAsJson()
        {
            var renderedDetails = RenderComponent<_LogDetails>(parameters => parameters.Add(component => component.Log, LogWith(null)));

            Assert.Contains("something happened", renderedDetails.Markup);
            Assert.Contains("Warning", renderedDetails.Markup);
            Assert.Contains("\"metadata.Title\": \"The video\"", renderedDetails.Markup);
        }

        [Fact]
        public void EachLineOfTheVariablesIsOneParagraph()
        {
            var renderedDetails = RenderComponent<_LogDetails>(parameters => parameters.Add(component => component.Log, LogWith(null)));

            var paragraphs = renderedDetails.FindAll("p").Select(paragraph => paragraph.TextContent).ToList();
            Assert.Contains("{", paragraphs);
            Assert.Contains("  \"metadata.Title\": \"The video\"", paragraphs);
            Assert.Contains("}", paragraphs);
        }

        [Fact]
        public void TheExceptionSectionIsShownOnlyWhenTheLogHasAnException()
        {
            var renderedWithException = RenderComponent<_LogDetails>(parameters => parameters.Add(component => component.Log, LogWith("System.Exception: boom")));
            var renderedWithoutException = RenderComponent<_LogDetails>(parameters => parameters.Add(component => component.Log, LogWith("  ")));

            Assert.Contains("Exception: ", renderedWithException.Markup);
            Assert.Contains("System.Exception: boom", renderedWithException.Markup);
            Assert.DoesNotContain("Exception: ", renderedWithoutException.Markup);
        }

        [Fact]
        public void TheChangedCallbackIsRaisedOnlyWhenTheLogChanges()
        {
            var logsReported = new List<ApplicationLogEntryDto>();
            var firstLog = LogWith(null);
            var renderedDetails = RenderComponent<_LogDetails>(parameters => parameters
                .Add(component => component.LogChanged, EventCallback.Factory.Create<ApplicationLogEntryDto>(this, logsReported.Add))
                .Add(component => component.Log, firstLog));

            renderedDetails.SetParametersAndRender(parameters => parameters.Add(component => component.Log, firstLog));
            Assert.Single(logsReported);

            var secondLog = LogWith("System.Exception: boom");
            renderedDetails.SetParametersAndRender(parameters => parameters.Add(component => component.Log, secondLog));
            Assert.Equal(new[] { firstLog, secondLog }, logsReported);
        }
    }

    public class VideoDetailsTests : MudBlazorComponentTestContext
    {
        private readonly FakeVideoBrowserModule _fakeVideoBrowserModule = new FakeVideoBrowserModule();
        private readonly IRenderedComponent<MudPopoverProvider> _popoverProvider;

        public VideoDetailsTests()
        {
            _popoverProvider = RenderComponent<MudPopoverProvider>();
        }

        private static ApplicationVideoWithChannelDto VideoWithChannel(bool isBeingDownloaded, TimeSpan duration, ApplicationChannelDto channel = null, string description = "About the video") =>
            new ApplicationVideoWithChannelDto
            {
                Video = new ApplicationVideoMetadataDto
                {
                    SiteId = "v1",
                    Title = "The video",
                    Url = "https://www.youtube.com/watch?v=v1",
                    Description = description,
                    IsBeingDownloaded = isBeingDownloaded,
                    Duration = duration,
                    Thumbnail = new ApplicationThumbnailDto { Url = "https://thumbnails/v1", WidthInPixels = 320, HeightInPixels = 180 },
                },
                Channel = channel,
            };

        private IRenderedComponent<_VideoDetails> RenderDetails(ApplicationVideoWithChannelDto videoWithChannel, bool withModule = true)
        {
            return RenderComponent<_VideoDetails>(parameters =>
            {
                parameters.Add(component => component.VideoWithChannel, videoWithChannel);
                if (withModule)
                {
                    parameters.Add(component => component.VideoBrowserModule, _fakeVideoBrowserModule);
                }
            });
        }

        [Fact]
        public void AVideoThatIsNotBeingDownloadedShowsTheDownloadNowButton()
        {
            var renderedDetails = RenderDetails(VideoWithChannel(isBeingDownloaded: false, new TimeSpan(0, 5, 0)));

            Assert.Contains("Download Now", renderedDetails.Markup);
            Assert.DoesNotContain("Remove \"being downloaded\" block", renderedDetails.Markup);
        }

        [Fact]
        public void ClickingDownloadNowAsksTheModuleToDownloadTheVideo()
        {
            var renderedDetails = RenderDetails(VideoWithChannel(isBeingDownloaded: false, new TimeSpan(0, 5, 0)));

            renderedDetails.Find("button").Click();

            Assert.Equal(new[] { "v1" }, _fakeVideoBrowserModule.SiteIdsDownloadedNow);
        }

        [Fact]
        public void AVideoThatIsBeingDownloadedShowsTheRemoveBlockButtonInsteadOfDownloadNow()
        {
            var renderedDetails = RenderDetails(VideoWithChannel(isBeingDownloaded: true, new TimeSpan(0, 5, 0)));

            Assert.Contains("Video is being downloaded...", renderedDetails.Markup);
            Assert.Contains("Remove \"being downloaded\" block", renderedDetails.Markup);
            Assert.DoesNotContain("Download Now", renderedDetails.Markup);
        }

        [Fact]
        public void ClickingTheRemoveBlockButtonAsksTheModuleToRemoveTheBeingDownloadedStatus()
        {
            var renderedDetails = RenderDetails(VideoWithChannel(isBeingDownloaded: true, new TimeSpan(0, 5, 0)));

            renderedDetails.Find("button").Click();

            Assert.Equal(new[] { "v1" }, _fakeVideoBrowserModule.SiteIdsWhoseBeingDownloadedStatusWasRemoved);
        }

        [Fact]
        public void WithNoModuleThereAreNoActions()
        {
            var renderedDetails = RenderDetails(VideoWithChannel(isBeingDownloaded: false, new TimeSpan(0, 5, 0)), withModule: false);

            Assert.DoesNotContain("Actions: ", renderedDetails.Markup);
            Assert.Empty(renderedDetails.FindAll("button"));
        }

        [Fact]
        public void AZeroDurationShowsNoData()
        {
            var renderedDetails = RenderDetails(VideoWithChannel(isBeingDownloaded: false, TimeSpan.Zero));

            Assert.Contains("No data", renderedDetails.Markup);
        }

        [Fact]
        public void ADurationIsShown()
        {
            var renderedDetails = RenderDetails(VideoWithChannel(isBeingDownloaded: false, new TimeSpan(1, 2, 3)));

            Assert.Contains("01:02:03", renderedDetails.Markup);
            Assert.DoesNotContain("No data", renderedDetails.Markup);
        }

        [Fact]
        public void AVideoWithNoDescriptionStillRenders()
        {
            var renderedDetails = RenderDetails(VideoWithChannel(isBeingDownloaded: false, new TimeSpan(0, 5, 0), description: null));

            Assert.Contains("The video", renderedDetails.Markup);
        }

        [Fact]
        public void AVideoWithNoChannelShowsUnknownAsItsChannel()
        {
            var renderedDetails = RenderDetails(VideoWithChannel(isBeingDownloaded: false, new TimeSpan(0, 5, 0), channel: null));

            Assert.Contains("Unknown", renderedDetails.Markup);
        }

        [Fact]
        public void AVideoWithAChannelShowsTheChannelName()
        {
            var renderedDetails = RenderDetails(VideoWithChannel(isBeingDownloaded: false, new TimeSpan(0, 5, 0), new ApplicationChannelDto { SiteId = "c1", Name = "The channel" }));

            Assert.Contains("The channel", renderedDetails.Markup);
        }

        [Fact]
        public void TheChangedCallbacksAreRaisedOnlyWhenTheValuesChange()
        {
            var modulesReported = new List<IVideoBrowserModule>();
            var videosReported = new List<ApplicationVideoWithChannelDto>();
            var firstVideo = VideoWithChannel(isBeingDownloaded: false, TimeSpan.Zero);
            var renderedDetails = RenderComponent<_VideoDetails>(parameters => parameters
                .Add(component => component.VideoBrowserModuleChanged, EventCallback.Factory.Create<IVideoBrowserModule>(this, modulesReported.Add))
                .Add(component => component.VideoWithChannelChanged, EventCallback.Factory.Create<ApplicationVideoWithChannelDto>(this, videosReported.Add))
                .Add(component => component.VideoBrowserModule, _fakeVideoBrowserModule)
                .Add(component => component.VideoWithChannel, firstVideo));

            renderedDetails.SetParametersAndRender(parameters => parameters
                .Add(component => component.VideoBrowserModule, _fakeVideoBrowserModule)
                .Add(component => component.VideoWithChannel, firstVideo));

            Assert.Single(modulesReported);
            Assert.Single(videosReported);
        }

        private const string AccentStyleOfAPickerThatHoldsAnOverride = "--mud-palette-lines-inputs: var(--mud-palette-primary)";

        private static string[] ValuesShownByThePickers(IRenderedComponent<_VideoDetails> renderedDetails) =>
            renderedDetails.FindAll("input").Select(input => input.GetAttribute("value")).ToArray();

        private void ChooseEntryOfPicker(IRenderedComponent<_VideoDetails> renderedDetails, int indexOfThePicker, string labelOfTheEntry)
        {
            renderedDetails.FindAll(".mud-input-control")[indexOfThePicker].PointerDown();
            _popoverProvider.WaitForAssertion(() => Assert.Contains(_popoverProvider.FindAll(".mud-list-item"), item => item.TextContent.Trim() == labelOfTheEntry));
            _popoverProvider.FindAll(".mud-list-item").Single(item => item.TextContent.Trim() == labelOfTheEntry).Click();
        }

        [Fact]
        public void TheBoxesAppearInTodaysOrderThenTheDownloadQualityBoxThenTheActions()
        {
            var renderedDetails = RenderDetails(VideoWithChannel(isBeingDownloaded: false, new TimeSpan(0, 5, 0)));

            var positionsOfTheLabels = new[] { "Title: ", "Thumbnail: ", "Channel: ", "Url: ", "Duration: ", "Description: ", "Download quality: ", "Actions: " }
                .Select(label => renderedDetails.Markup.IndexOf("<b>" + label + "</b>", StringComparison.Ordinal))
                .ToList();
            Assert.All(positionsOfTheLabels, position => Assert.True(position >= 0));
            Assert.Equal(positionsOfTheLabels.OrderBy(position => position), positionsOfTheLabels);
        }

        [Fact]
        public void WithNoModuleNeitherTheDownloadQualityBoxNorTheActionsAppear()
        {
            var renderedDetails = RenderDetails(VideoWithChannel(isBeingDownloaded: false, new TimeSpan(0, 5, 0)), withModule: false);

            Assert.DoesNotContain("Download quality: ", renderedDetails.Markup);
            Assert.DoesNotContain("Actions: ", renderedDetails.Markup);
            Assert.Empty(renderedDetails.FindAll("input"));
            Assert.DoesNotContain("GetAppWideDownloadQuality", DownloadQualitySettingsUseCases.CallsMade);
        }

        [Fact]
        public void TheBoxHasThreePickersLabelledVideoCodecMaximumResolutionAndAudio()
        {
            var renderedDetails = RenderDetails(VideoWithChannel(isBeingDownloaded: false, new TimeSpan(0, 5, 0)));

            Assert.Equal(new[] { "Video codec", "Maximum resolution", "Audio" }, renderedDetails.FindAll("label").Select(label => label.TextContent));
        }

        [Fact]
        public void WithNoOverridesEachPickerShowsSameAsSettingsNamingTheSettingsValueWithNoAccentStyle()
        {
            var renderedDetails = RenderDetails(VideoWithChannel(isBeingDownloaded: false, new TimeSpan(0, 5, 0)));

            Assert.Equal(new[] { "Same as Settings (H.264)", "Same as Settings (No limit)", "Same as Settings (Best AAC)" }, ValuesShownByThePickers(renderedDetails));
            Assert.DoesNotContain("--mud-palette-lines-inputs", renderedDetails.Markup);
        }

        [Fact]
        public void TheSameAsSettingsLabelsFollowTheValuesOfTheSettings()
        {
            DownloadQualitySettingsUseCases.AppWideDownloadQualityToReturn = new ApplicationDownloadQualityDto
            {
                VideoCodecToPreferWhenDownloading = ApplicationVideoCodecChoice.AnyCodec,
                MaximumVideoResolutionOnTheShorterSide = ApplicationMaximumVideoResolutionChoice.UpTo1080p,
                AudioQualityToDownload = ApplicationAudioQualityChoice.SmallestAac,
            };

            var renderedDetails = RenderDetails(VideoWithChannel(isBeingDownloaded: false, new TimeSpan(0, 5, 0)));

            Assert.Equal(new[] { "Same as Settings (Any codec)", "Same as Settings (1080p)", "Same as Settings (Smallest AAC)" }, ValuesShownByThePickers(renderedDetails));
        }

        [Fact]
        public void AVideoWithAnOverrideShowsItAndOnlyThatPickerHasTheAccentStyle()
        {
            var videoWithChannel = VideoWithChannel(isBeingDownloaded: false, new TimeSpan(0, 5, 0));
            videoWithChannel.Video.MaximumVideoResolutionChoiceOverride = ApplicationMaximumVideoResolutionChoice.UpTo720p;

            var renderedDetails = RenderDetails(videoWithChannel);

            Assert.Equal(new[] { "Same as Settings (H.264)", "720p", "Same as Settings (Best AAC)" }, ValuesShownByThePickers(renderedDetails));
            var pickersWithTheAccentStyle = renderedDetails.FindAll(".mud-input-control").Where(picker => (picker.GetAttribute("style") ?? string.Empty).Contains(AccentStyleOfAPickerThatHoldsAnOverride)).ToList();
            var pickerWithTheAccentStyle = Assert.Single(pickersWithTheAccentStyle);
            Assert.Equal("Maximum resolution", pickerWithTheAccentStyle.QuerySelector("label").TextContent);
        }

        [Fact]
        public void ChoosingAnEntrySetsThatFieldOfTheVideoAndSavesThroughTheModuleOnce()
        {
            var videoWithChannel = VideoWithChannel(isBeingDownloaded: false, new TimeSpan(0, 5, 0));
            var renderedDetails = RenderDetails(videoWithChannel);

            ChooseEntryOfPicker(renderedDetails, 1, "720p");

            renderedDetails.WaitForAssertion(() => Assert.Single(_fakeVideoBrowserModule.VideosWhoseDownloadQualityChoiceOverridesWereSaved));
            Assert.Same(videoWithChannel.Video, _fakeVideoBrowserModule.VideosWhoseDownloadQualityChoiceOverridesWereSaved[0]);
            Assert.Equal(ApplicationMaximumVideoResolutionChoice.UpTo720p, videoWithChannel.Video.MaximumVideoResolutionChoiceOverride);
            Assert.Null(videoWithChannel.Video.VideoCodecChoiceOverride);
            Assert.Null(videoWithChannel.Video.AudioQualityChoiceOverride);
            renderedDetails.WaitForAssertion(() => Assert.Equal("720p", ValuesShownByThePickers(renderedDetails)[1]));
        }

        [Fact]
        public void ChoosingTheCodecAndTheAudioQualitySetsTheirOwnFields()
        {
            var videoWithChannel = VideoWithChannel(isBeingDownloaded: false, new TimeSpan(0, 5, 0));
            var renderedDetails = RenderDetails(videoWithChannel);

            ChooseEntryOfPicker(renderedDetails, 0, "Any codec (AV1, VP9 or H.264)");
            renderedDetails.WaitForAssertion(() => Assert.Single(_fakeVideoBrowserModule.VideosWhoseDownloadQualityChoiceOverridesWereSaved));
            ChooseEntryOfPicker(renderedDetails, 2, "Smallest AAC (about 50 kbps)");
            renderedDetails.WaitForAssertion(() => Assert.Equal(2, _fakeVideoBrowserModule.VideosWhoseDownloadQualityChoiceOverridesWereSaved.Count));

            Assert.Equal(ApplicationVideoCodecChoice.AnyCodec, videoWithChannel.Video.VideoCodecChoiceOverride);
            Assert.Equal(ApplicationAudioQualityChoice.SmallestAac, videoWithChannel.Video.AudioQualityChoiceOverride);
            Assert.Null(videoWithChannel.Video.MaximumVideoResolutionChoiceOverride);
        }

        [Fact]
        public void ChoosingSameAsSettingsRemovesTheOverrideAndSavesIt()
        {
            var videoWithChannel = VideoWithChannel(isBeingDownloaded: false, new TimeSpan(0, 5, 0));
            videoWithChannel.Video.MaximumVideoResolutionChoiceOverride = ApplicationMaximumVideoResolutionChoice.UpTo720p;
            var renderedDetails = RenderDetails(videoWithChannel);

            ChooseEntryOfPicker(renderedDetails, 1, "Same as Settings (No limit)");

            renderedDetails.WaitForAssertion(() => Assert.Single(_fakeVideoBrowserModule.VideosWhoseDownloadQualityChoiceOverridesWereSaved));
            Assert.Null(videoWithChannel.Video.MaximumVideoResolutionChoiceOverride);
            renderedDetails.WaitForAssertion(() => Assert.DoesNotContain("--mud-palette-lines-inputs", renderedDetails.Markup));
        }

        [Fact]
        public void ASaveThatFailsSetsTheFieldBackAndShowsTheErrorUnderThatPicker()
        {
            _fakeVideoBrowserModule.ExceptionToThrowWhenSavingDownloadQualityChoiceOverrides = new InvalidOperationException("the database is locked");
            var videoWithChannel = VideoWithChannel(isBeingDownloaded: false, new TimeSpan(0, 5, 0));
            var renderedDetails = RenderDetails(videoWithChannel);

            ChooseEntryOfPicker(renderedDetails, 1, "720p");

            renderedDetails.WaitForAssertion(() => Assert.Contains("Couldn't save this video's choice: the database is locked", renderedDetails.Markup));
            Assert.Null(videoWithChannel.Video.MaximumVideoResolutionChoiceOverride);
            Assert.Equal("Same as Settings (No limit)", ValuesShownByThePickers(renderedDetails)[1]);
            Assert.DoesNotContain("--mud-palette-lines-inputs", renderedDetails.Markup);
        }

        [Fact]
        public void ASaveThatFailsRestoresAnOverrideTheVideoHadBefore()
        {
            _fakeVideoBrowserModule.ExceptionToThrowWhenSavingDownloadQualityChoiceOverrides = new InvalidOperationException("the database is locked");
            var videoWithChannel = VideoWithChannel(isBeingDownloaded: false, new TimeSpan(0, 5, 0));
            videoWithChannel.Video.MaximumVideoResolutionChoiceOverride = ApplicationMaximumVideoResolutionChoice.UpTo480p;
            var renderedDetails = RenderDetails(videoWithChannel);

            ChooseEntryOfPicker(renderedDetails, 1, "720p");

            renderedDetails.WaitForAssertion(() => Assert.Contains("Couldn't save this video's choice", renderedDetails.Markup));
            Assert.Equal(ApplicationMaximumVideoResolutionChoice.UpTo480p, videoWithChannel.Video.MaximumVideoResolutionChoiceOverride);
            Assert.Equal("480p", ValuesShownByThePickers(renderedDetails)[1]);
        }

        [Fact]
        public void ALaterSuccessfulSaveOfThatPickerClearsTheError()
        {
            _fakeVideoBrowserModule.ExceptionToThrowWhenSavingDownloadQualityChoiceOverrides = new InvalidOperationException("the database is locked");
            var renderedDetails = RenderDetails(VideoWithChannel(isBeingDownloaded: false, new TimeSpan(0, 5, 0)));
            ChooseEntryOfPicker(renderedDetails, 1, "720p");
            renderedDetails.WaitForAssertion(() => Assert.Contains("Couldn't save this video's choice", renderedDetails.Markup));
            _fakeVideoBrowserModule.ExceptionToThrowWhenSavingDownloadQualityChoiceOverrides = null;

            ChooseEntryOfPicker(renderedDetails, 1, "480p");

            renderedDetails.WaitForAssertion(() => Assert.DoesNotContain("Couldn't save", renderedDetails.Markup));
            renderedDetails.WaitForAssertion(() => Assert.Equal("480p", ValuesShownByThePickers(renderedDetails)[1]));
        }

        [Fact]
        public void WhileTheVideoIsBeingDownloadedThePickersStayEnabledAndCanSave()
        {
            var videoWithChannel = VideoWithChannel(isBeingDownloaded: true, new TimeSpan(0, 5, 0));
            var renderedDetails = RenderDetails(videoWithChannel);

            Assert.Equal(3, renderedDetails.FindAll("input").Count);
            Assert.Empty(renderedDetails.FindAll(".mud-input-control.mud-disabled"));
            ChooseEntryOfPicker(renderedDetails, 1, "720p");

            renderedDetails.WaitForAssertion(() => Assert.Single(_fakeVideoBrowserModule.VideosWhoseDownloadQualityChoiceOverridesWereSaved));
        }

        [Fact]
        public void TheSettingsAreReadAgainWhenTheDialogGetsADifferentVideo()
        {
            var renderedDetails = RenderDetails(VideoWithChannel(isBeingDownloaded: false, new TimeSpan(0, 5, 0)));
            DownloadQualitySettingsUseCases.AppWideDownloadQualityToReturn = new ApplicationDownloadQualityDto
            {
                VideoCodecToPreferWhenDownloading = ApplicationVideoCodecChoice.AnyCodec,
                MaximumVideoResolutionOnTheShorterSide = ApplicationMaximumVideoResolutionChoice.UpTo480p,
                AudioQualityToDownload = ApplicationAudioQualityChoice.BestOfAnyCodec,
            };

            renderedDetails.SetParametersAndRender(parameters => parameters
                .Add(component => component.VideoBrowserModule, _fakeVideoBrowserModule)
                .Add(component => component.VideoWithChannel, VideoWithChannel(isBeingDownloaded: false, new TimeSpan(0, 5, 0))));

            Assert.Equal(new[] { "Same as Settings (Any codec)", "Same as Settings (480p)", "Same as Settings (Best of any codec)" }, ValuesShownByThePickers(renderedDetails));
        }

        [Fact]
        public void AFailureToReadTheSettingsReachesTheErrorBoundaryInsteadOfBeingSwallowed()
        {
            DownloadQualitySettingsUseCases.ExceptionToThrowWhenReading = new InvalidOperationException("config.json is not valid JSON");

            var exception = Assert.Throws<InvalidOperationException>(() => RenderDetails(VideoWithChannel(isBeingDownloaded: false, new TimeSpan(0, 5, 0))));

            Assert.Equal("config.json is not valid JSON", exception.Message);
        }
    }
}
