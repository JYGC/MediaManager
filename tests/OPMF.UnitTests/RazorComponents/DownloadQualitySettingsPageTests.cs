using System;
using System.IO;
using System.Linq;
using Bunit;
using MediaManager.Dtos.Application;
using MediaManager.Initializations;
using MediaManagerUI.Pages.Settings;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.RazorComponents
{
    public class DownloadQualitySettingsPageTests : MudBlazorComponentTestContext
    {
        private readonly FakeDownloadQualitySettingsUseCases _fakeDownloadQualitySettingsUseCases;
        private readonly IRenderedComponent<MudPopoverProvider> _popoverProvider;

        public DownloadQualitySettingsPageTests()
        {
            _fakeDownloadQualitySettingsUseCases = DownloadQualitySettingsUseCases;
            _popoverProvider = RenderComponent<MudPopoverProvider>();
        }

        private IRenderedComponent<DownloadQualitySettingsPage> RenderPage() => RenderComponent<DownloadQualitySettingsPage>();

        private static string[] ValuesShownByThePickers(IRenderedComponent<DownloadQualitySettingsPage> renderedPage) =>
            renderedPage.FindAll("input").Select(input => input.GetAttribute("value")).ToArray();

        private void ChooseEntryOfPicker(IRenderedComponent<DownloadQualitySettingsPage> renderedPage, int indexOfThePicker, string labelOfTheEntry)
        {
            renderedPage.FindAll(".mud-input-control")[indexOfThePicker].PointerDown();
            _popoverProvider.WaitForAssertion(() => Assert.Contains(_popoverProvider.FindAll(".mud-list-item"), item => item.TextContent.Trim() == labelOfTheEntry));
            _popoverProvider.FindAll(".mud-list-item").Single(item => item.TextContent.Trim() == labelOfTheEntry).Click();
        }

        [Fact]
        public void ThePageShowsItsHeadingAndTheLineSayingWhenTheChoicesApply()
        {
            var renderedPage = RenderPage();

            Assert.Equal("Download quality", renderedPage.Find("h5").TextContent);
            Assert.Contains("Used for every download, unless a video has its own choice.", renderedPage.Markup);
        }

        [Fact]
        public void ThePageShowsThreePickersWithTheirLabels()
        {
            var renderedPage = RenderPage();

            Assert.Equal(new[] { "Video codec", "Maximum resolution", "Audio" }, renderedPage.FindAll("label").Select(label => label.TextContent));
        }

        [Fact]
        public void ThePickersHoldTheValuesReadThroughTheUseCases()
        {
            _fakeDownloadQualitySettingsUseCases.AppWideDownloadQualityToReturn = new ApplicationDownloadQualityDto
            {
                VideoCodecToPreferWhenDownloading = ApplicationVideoCodecChoice.AnyCodec,
                MaximumVideoResolutionOnTheShorterSide = ApplicationMaximumVideoResolutionChoice.UpTo1080p,
                AudioQualityToDownload = ApplicationAudioQualityChoice.SmallestAac,
            };

            var renderedPage = RenderPage();

            Assert.Equal(new[] { "Any codec (AV1, VP9 or H.264)", "1080p", "Smallest AAC (about 50 kbps)" }, ValuesShownByThePickers(renderedPage));
            Assert.Equal(new[] { "GetAppWideDownloadQuality" }, _fakeDownloadQualitySettingsUseCases.CallsMade);
        }

        [Fact]
        public void ThePickersHoldTheDefaultsWhenTheDefaultsAreRead()
        {
            var renderedPage = RenderPage();

            Assert.Equal(new[] { "H.264 (plays everywhere)", "No limit", "Best AAC (about 128 kbps)" }, ValuesShownByThePickers(renderedPage));
        }

        [Fact]
        public void ChoosingAResolutionSavesItAsTheAppWideMaximumVideoResolution()
        {
            var renderedPage = RenderPage();

            ChooseEntryOfPicker(renderedPage, 1, "720p");

            renderedPage.WaitForAssertion(() => Assert.Contains("SaveAppWideMaximumVideoResolutionChoice(UpTo720p)", _fakeDownloadQualitySettingsUseCases.CallsMade));
            renderedPage.WaitForAssertion(() => Assert.Equal("720p", ValuesShownByThePickers(renderedPage)[1]));
        }

        [Fact]
        public void ChoosingACodecSavesItAsTheAppWideVideoCodec()
        {
            var renderedPage = RenderPage();

            ChooseEntryOfPicker(renderedPage, 0, "Any codec (AV1, VP9 or H.264)");

            renderedPage.WaitForAssertion(() => Assert.Contains("SaveAppWideVideoCodecChoice(AnyCodec)", _fakeDownloadQualitySettingsUseCases.CallsMade));
            renderedPage.WaitForAssertion(() => Assert.Equal("Any codec (AV1, VP9 or H.264)", ValuesShownByThePickers(renderedPage)[0]));
        }

        [Fact]
        public void ChoosingAnAudioQualitySavesItAsTheAppWideAudioQuality()
        {
            var renderedPage = RenderPage();

            ChooseEntryOfPicker(renderedPage, 2, "Smallest AAC (about 50 kbps)");

            renderedPage.WaitForAssertion(() => Assert.Contains("SaveAppWideAudioQualityChoice(SmallestAac)", _fakeDownloadQualitySettingsUseCases.CallsMade));
            renderedPage.WaitForAssertion(() => Assert.Equal("Smallest AAC (about 50 kbps)", ValuesShownByThePickers(renderedPage)[2]));
        }

        [Fact]
        public void ASaveThatSucceedsShowsNoError()
        {
            var renderedPage = RenderPage();

            ChooseEntryOfPicker(renderedPage, 1, "720p");

            renderedPage.WaitForAssertion(() => Assert.Contains("SaveAppWideMaximumVideoResolutionChoice(UpTo720p)", _fakeDownloadQualitySettingsUseCases.CallsMade));
            Assert.DoesNotContain("Couldn't save", renderedPage.Markup);
            Assert.Empty(renderedPage.FindAll(".mud-input-error"));
        }

        [Fact]
        public void ASaveThatFailsKeepsThePreviousEntryAndShowsTheErrorUnderThatPickerOnly()
        {
            _fakeDownloadQualitySettingsUseCases.OnAnySave = () => throw new IOException("access to config.json was denied");
            var renderedPage = RenderPage();

            ChooseEntryOfPicker(renderedPage, 1, "720p");

            renderedPage.WaitForAssertion(() => Assert.Contains("Couldn't save to config.json: access to config.json was denied", renderedPage.Markup));
            Assert.Equal("No limit", ValuesShownByThePickers(renderedPage)[1]);
            Assert.Single(renderedPage.FindAll(".mud-input-error").Where(element => element.ClassList.Contains("mud-input")));
        }

        [Fact]
        public void ALaterSuccessfulSaveOfThatPickerClearsTheError()
        {
            _fakeDownloadQualitySettingsUseCases.OnAnySave = () => throw new IOException("access to config.json was denied");
            var renderedPage = RenderPage();
            ChooseEntryOfPicker(renderedPage, 1, "720p");
            renderedPage.WaitForAssertion(() => Assert.Contains("Couldn't save to config.json", renderedPage.Markup));
            _fakeDownloadQualitySettingsUseCases.OnAnySave = () => { };

            ChooseEntryOfPicker(renderedPage, 1, "480p");

            renderedPage.WaitForAssertion(() => Assert.DoesNotContain("Couldn't save", renderedPage.Markup));
            renderedPage.WaitForAssertion(() => Assert.Equal("480p", ValuesShownByThePickers(renderedPage)[1]));
        }

        [Fact]
        public void AFailureToReadTheValuesReachesTheErrorBoundaryInsteadOfBeingSwallowed()
        {
            _fakeDownloadQualitySettingsUseCases.ExceptionToThrowWhenReading = new InvalidOperationException("config.json is not valid JSON");

            var exception = Assert.Throws<InvalidOperationException>(() => RenderComponent<DownloadQualitySettingsPage>());

            Assert.Equal("config.json is not valid JSON", exception.Message);
        }
    }
}
