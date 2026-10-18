using System.IO;
using MediaManager.Initializations;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;

namespace MediaManagerUI
{
    internal class Program
    {
        [STAThread]
        static void Main()
        {
            var services = new ServiceCollection();
            services.AddSingleton(TimeProvider.System);
            services.AddSingleton<TextWriter>(Console.Out);
            services.AddMediaManagerApplicationLayer();
            services.AddMudServices();
            services.AddWpfBlazorWebView();

            using ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetRequiredService<IApplicationEntryPointUseCase>().RunStartupThenRunApplicationThenRecordAndPrintAnyUnhandledError(() =>
            {
                var app = new App(serviceProvider);
                app.InitializeComponent();
                app.Run();
            });
        }
    }
}
