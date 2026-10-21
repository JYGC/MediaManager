using System.Collections.Generic;
using System.Linq;
using Bunit;
using MediaManager.Dtos.Application;
using MediaManagerUI.Shared;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.RazorComponents
{
    public class DownloadQualityPickerTests : MudBlazorComponentTestContext
    {
        private const string StyleOfAPickerThatHoldsAnOverride = "--mud-palette-lines-inputs: var(--mud-palette-primary); --mud-palette-text-secondary: var(--mud-palette-primary)";

        private static readonly List<DownloadQualityPickerEntry<ApplicationMaximumVideoResolutionChoice>> EntriesOfTheResolutionPicker =
            DownloadQualityPickerEntries.ForMaximumVideoResolutionOverride(ApplicationMaximumVideoResolutionChoice.NoLimit);

        private readonly List<DownloadQualityPickerEntry<ApplicationMaximumVideoResolutionChoice>> _entriesChosen = new List<DownloadQualityPickerEntry<ApplicationMaximumVideoResolutionChoice>>();

        private readonly IRenderedComponent<MudPopoverProvider> _popoverProvider;

        public DownloadQualityPickerTests()
        {
            _popoverProvider = RenderComponent<MudPopoverProvider>();
        }

        private IRenderedComponent<_DownloadQualityPicker<ApplicationMaximumVideoResolutionChoice>> RenderPicker(
            DownloadQualityPickerEntry<ApplicationMaximumVideoResolutionChoice> selectedEntry = null,
            string errorTextOfFailedSave = null,
            bool showsAnOverride = false)
        {
            return RenderComponent<_DownloadQualityPicker<ApplicationMaximumVideoResolutionChoice>>(parameters => parameters
                .Add(picker => picker.Label, "Maximum resolution")
                .Add(picker => picker.Entries, EntriesOfTheResolutionPicker)
                .Add(picker => picker.SelectedEntry, selectedEntry ?? EntriesOfTheResolutionPicker[0])
                .Add(picker => picker.OnEntryChosen, EventCallback.Factory.Create<DownloadQualityPickerEntry<ApplicationMaximumVideoResolutionChoice>>(this, _entriesChosen.Add))
                .Add(picker => picker.ErrorTextOfFailedSave, errorTextOfFailedSave)
                .Add(picker => picker.ShowsAnOverride, showsAnOverride));
        }

        [Fact]
        public void ThePickerIsOutlinedAndFullWidthWithItsLabelAndTheLabelOfTheSelectedEntry()
        {
            var renderedPicker = RenderPicker(EntriesOfTheResolutionPicker.Single(entry => entry.Choice == ApplicationMaximumVideoResolutionChoice.UpTo720p));

            Assert.NotEmpty(renderedPicker.FindAll(".mud-input-outlined"));
            Assert.NotEmpty(renderedPicker.FindAll(".mud-input-control-full-width"));
            Assert.Equal("Maximum resolution", renderedPicker.Find("label").TextContent);
            Assert.Equal("720p", renderedPicker.Find("input").GetAttribute("value"));
        }

        [Fact]
        public void ThePickerShowsTheSameAsSettingsLabelWhenThatEntryIsSelected()
        {
            var renderedPicker = RenderPicker(EntriesOfTheResolutionPicker[0]);

            Assert.Equal("Same as Settings (No limit)", renderedPicker.Find("input").GetAttribute("value"));
        }

        [Fact]
        public void ChoosingAnEntryRaisesTheCallbackWithThatEntry()
        {
            var renderedPicker = RenderPicker();

            renderedPicker.Find(".mud-input-control").PointerDown();
            _popoverProvider.WaitForAssertion(() => Assert.Contains(_popoverProvider.FindAll(".mud-list-item"), item => item.TextContent.Trim() == "720p"));
            _popoverProvider.FindAll(".mud-list-item").Single(item => item.TextContent.Trim() == "720p").Click();

            renderedPicker.WaitForAssertion(() => Assert.Single(_entriesChosen));
            var entryChosen = Assert.Single(_entriesChosen);
            Assert.Equal(ApplicationMaximumVideoResolutionChoice.UpTo720p, entryChosen.Choice);
        }

        [Fact]
        public void AnErrorTextOfAFailedSaveIsShownWithTheErrorState()
        {
            var renderedPicker = RenderPicker(errorTextOfFailedSave: "Couldn't save to config.json: access denied");

            Assert.Contains("Couldn't save to config.json: access denied", renderedPicker.Markup);
            Assert.NotEmpty(renderedPicker.FindAll(".mud-input-error"));
        }

        [Fact]
        public void WithNoErrorTextThereIsNoErrorState()
        {
            var renderedPicker = RenderPicker();

            Assert.Empty(renderedPicker.FindAll(".mud-input-error"));
        }

        [Fact]
        public void ShowingAnOverrideAddsTheAccentStyleToThePickerOnly()
        {
            var renderedPicker = RenderPicker(showsAnOverride: true);

            Assert.Equal(StyleOfAPickerThatHoldsAnOverride, renderedPicker.Find(".mud-input-control").GetAttribute("style"));
        }

        [Fact]
        public void ShowingNoOverrideAddsNoAccentStyle()
        {
            var renderedPicker = RenderPicker(showsAnOverride: false);

            Assert.DoesNotContain("--mud-palette-lines-inputs", renderedPicker.Markup);
            Assert.DoesNotContain("--mud-palette-text-secondary", renderedPicker.Markup);
        }
    }
}
