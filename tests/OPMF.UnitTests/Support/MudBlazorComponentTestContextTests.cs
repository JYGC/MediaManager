using MudBlazor;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace OPMF.UnitTests.Support
{
    public class MudBlazorComponentTestContextTests : MudBlazorComponentTestContext
    {
        [Fact]
        public void TheMudBlazorServicesAreRegisteredInTheTestContainer()
        {
            Assert.NotNull(Services.GetService<IDialogService>());
            Assert.NotNull(Services.GetService<ISnackbar>());
        }

        [Fact]
        public void JavaScriptInteropRunsInLooseModeSoComponentsThatCallJavaScriptStillRender()
        {
            var renderedButton = RenderComponent<MudButton>(parameters => parameters.AddChildContent("Press me"));

            Assert.Contains("Press me", renderedButton.Markup);
        }
    }
}
