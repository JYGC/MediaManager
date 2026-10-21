using Bunit;
using MediaManager.Initializations;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace OPMF.UnitTests.Support
{
    public abstract class MudBlazorComponentTestContext : TestContext
    {
        protected FakeDownloadQualitySettingsUseCases DownloadQualitySettingsUseCases { get; } = new FakeDownloadQualitySettingsUseCases();

        protected MudBlazorComponentTestContext()
        {
            Services.AddMudServices();
            Services.AddSingleton<IDownloadQualitySettingsUseCases>(DownloadQualitySettingsUseCases);
            JSInterop.Mode = JSRuntimeMode.Loose;
        }
    }
}
