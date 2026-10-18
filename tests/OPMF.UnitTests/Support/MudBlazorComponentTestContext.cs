using Bunit;
using MudBlazor.Services;

namespace OPMF.UnitTests.Support
{
    public abstract class MudBlazorComponentTestContext : TestContext
    {
        protected MudBlazorComponentTestContext()
        {
            Services.AddMudServices();
            JSInterop.Mode = JSRuntimeMode.Loose;
        }
    }
}
