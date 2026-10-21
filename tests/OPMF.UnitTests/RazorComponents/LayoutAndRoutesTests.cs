using System;
using System.Linq;
using Bunit;
using Bunit.TestDoubles;
using MediaManagerUI;
using MediaManagerUI.Layout;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using MudBlazor;
using OPMF.UnitTests.Support;
using Xunit;
using MediaManager.Initializations;

namespace OPMF.UnitTests.RazorComponents
{
    public class ThrowOnFirstRenderComponent : ComponentBase
    {
        public static int TimesRendered;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            TimesRendered++;
            if (TimesRendered == 1)
            {
                throw new InvalidOperationException("the page failed to render");
            }
            builder.AddMarkupContent(0, "<span>the page rendered</span>");
        }
    }

    public class MainLayoutTests : MudBlazorComponentTestContext
    {
        public MainLayoutTests()
        {
            ThrowOnFirstRenderComponent.TimesRendered = 0;
        }

        private static RenderFragment Body(string markup) => builder => builder.AddMarkupContent(0, markup);

        [Fact]
        public void TheLayoutShowsTheAppBarMenusAndTheBody()
        {
            var renderedLayout = RenderComponent<MainLayout>(parameters => parameters.Add(layout => layout.Body, Body("<span>the body</span>")));

            Assert.Contains("the body", renderedLayout.Markup);
            Assert.Contains("Videos", renderedLayout.Markup);
            Assert.Contains("Channels", renderedLayout.Markup);
            Assert.Contains("MediaManager", renderedLayout.Markup);
        }

        [Fact]
        public void ABodyThatThrowsShowsTheErrorMessageAndTheStackTraceInsteadOfTheLayout()
        {
            var renderedLayout = RenderComponent<MainLayout>(parameters => parameters.Add(layout => layout.Body, builder =>
            {
                builder.OpenComponent<ThrowOnFirstRenderComponent>(0);
                builder.CloseComponent();
            }));

            Assert.Contains("Error: ", renderedLayout.Markup);
            Assert.Contains("the page failed to render", renderedLayout.Markup);
            Assert.Contains("StackTrace", renderedLayout.Markup);
            Assert.Empty(renderedLayout.FindAll(".mud-appbar"));
        }

        [Fact]
        public void ClickingContinueRecoversAndShowsTheBodyAgain()
        {
            var renderedLayout = RenderComponent<MainLayout>(parameters => parameters.Add(layout => layout.Body, builder =>
            {
                builder.OpenComponent<ThrowOnFirstRenderComponent>(0);
                builder.CloseComponent();
            }));

            renderedLayout.FindAll("button").Single(button => button.TextContent.Trim() == "Continue").Click();

            renderedLayout.WaitForAssertion(() => Assert.Contains("the page rendered", renderedLayout.Markup));
            Assert.DoesNotContain("Error: ", renderedLayout.Markup);
        }
    }

    public class AppBarMenusTests : MudBlazorComponentTestContext
    {
        [Fact]
        public void TheSettingsItemOfTheSettingsMenuIsEnabledAndLinksToTheSettingsPage()
        {
            var renderedPopovers = RenderComponent<MudPopoverProvider>();
            var renderedMenus = RenderComponent<AppBarMenus>();

            renderedMenus.FindAll("button").Single(button => button.TextContent.Trim() == "Settings").Click();

            var settingsItem = renderedPopovers.FindAll("a.mud-menu-item").Single(item => item.TextContent.Trim() == "Settings");
            Assert.Equal("/settings", settingsItem.GetAttribute("href"));
            Assert.DoesNotContain("mud-disabled", settingsItem.ClassList);
        }
    }

    public class RoutesTests : MudBlazorComponentTestContext
    {
        public RoutesTests()
        {
            Services.AddSingleton<IVideoBrowsingUseCases>(new FakeVideoBrowsingUseCases());
            Services.AddSingleton<IChannelBrowsingUseCases>(new FakeChannelBrowsingUseCases());
            Services.AddSingleton<ILogBrowsingUseCases>(new FakeLogBrowsingUseCases());
            Services.AddSingleton<TimeProvider>(new FakeTimeProvider(new DateTimeOffset(2026, 10, 18, 9, 30, 15, TimeSpan.Zero)));
        }

        [Theory]
        [InlineData("/", "New Videos")]
        [InlineData("/videos/new", "New Videos")]
        [InlineData("/videos/todownload", "Download Queue")]
        [InlineData("/videos/find", "Find Videos from Database")]
        [InlineData("/videos/getsingle", "Get Single Video from Internet")]
        [InlineData("/channels/all", "All Channels")]
        [InlineData("/channels/add", "AddChannels")]
        [InlineData("/logs/errors", "Range")]
        [InlineData("/logs/warnings", "Range")]
        [InlineData("/settings", "Download quality")]
        public void EveryRouteShowsItsPageInsideTheMainLayoutWithoutTheErrorContent(string route, string textOnThePage)
        {
            var navigationManager = Services.GetRequiredService<FakeNavigationManager>();
            var renderedRoutes = RenderComponent<Routes>();

            navigationManager.NavigateTo(route);

            renderedRoutes.WaitForAssertion(() => Assert.Contains(textOnThePage, renderedRoutes.Markup));
            Assert.NotEmpty(renderedRoutes.FindAll(".mud-appbar"));
            Assert.DoesNotContain("Error: ", renderedRoutes.Markup);
        }
    }
}
