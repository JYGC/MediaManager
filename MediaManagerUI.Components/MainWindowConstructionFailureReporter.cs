namespace MediaManagerUI
{
    public class MainWindowConstructionFailureReporter
    {
        private readonly Action<string> _appendEntryToTextLog;
        private readonly Action<string, string> _showMessageBoxWithTextAndCaption;

        public MainWindowConstructionFailureReporter(Action<string> appendEntryToTextLog, Action<string, string> showMessageBoxWithTextAndCaption)
        {
            _appendEntryToTextLog = appendEntryToTextLog;
            _showMessageBoxWithTextAndCaption = showMessageBoxWithTextAndCaption;
        }

        public void RunMainWindowConstructionAndReportAnyFailure(Action constructMainWindowContents)
        {
            try
            {
                constructMainWindowContents();
            }
            catch (Exception exception)
            {
                _appendEntryToTextLog(exception.ToString());
                _showMessageBoxWithTextAndCaption(exception.Message, "Error");
            }
        }
    }
}
