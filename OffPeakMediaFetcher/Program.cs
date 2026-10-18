using System;
using System.IO;
using MediaManager.Initializations;
using Microsoft.Extensions.DependencyInjection;

namespace OffPeakMediaFetcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddSingleton(TimeProvider.System);
            services.AddSingleton<TextWriter>(Console.Out);
            services.AddMediaManagerApplicationLayer();

            using ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetRequiredService<IApplicationEntryPointUseCase>().RunStartupThenRunApplicationThenRecordAndPrintAnyUnhandledError(
                () => new CommandLineArgumentsDispatcher(serviceProvider.GetRequiredService<ICommandLineTaskUseCases>()).DispatchCommandLineArgumentsToUseCase(args));
        }
    }
}
