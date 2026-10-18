using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using OPMF.TextLogging;

namespace MediaManagerUI
{
    public partial class MainWindow : Window
    {
        public MainWindow(IServiceProvider serviceProvider)
        {
            var mainWindowConstructionFailureReporter = new MainWindowConstructionFailureReporter(
                serviceProvider.GetRequiredService<TextLog>().LogEntry,
                (text, caption) => MessageBox.Show(text, caption));

            mainWindowConstructionFailureReporter.RunMainWindowConstructionAndReportAnyFailure(() =>
            {
                InitializeComponent();
                Resources.Add("services", serviceProvider);
            });
        }
    }
}
