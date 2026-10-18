using System;
using System.Collections.Generic;
using MediaManagerUI;
using Xunit;

namespace OPMF.UnitTests.RazorComponents
{
    public class MainWindowConstructionFailureReporterTests
    {
        private readonly List<string> _textLogEntries = new List<string>();
        private readonly List<(string Text, string Caption)> _messageBoxes = new List<(string, string)>();
        private readonly MainWindowConstructionFailureReporter _mainWindowConstructionFailureReporter;

        public MainWindowConstructionFailureReporterTests()
        {
            _mainWindowConstructionFailureReporter = new MainWindowConstructionFailureReporter(
                _textLogEntries.Add,
                (text, caption) => _messageBoxes.Add((text, caption)));
        }

        [Fact]
        public void TheConstructionIsRun()
        {
            var timesRun = 0;

            _mainWindowConstructionFailureReporter.RunMainWindowConstructionAndReportAnyFailure(() => timesRun++);

            Assert.Equal(1, timesRun);
        }

        [Fact]
        public void NothingIsReportedWhenTheConstructionSucceeds()
        {
            _mainWindowConstructionFailureReporter.RunMainWindowConstructionAndReportAnyFailure(() => { });

            Assert.Empty(_textLogEntries);
            Assert.Empty(_messageBoxes);
        }

        [Fact]
        public void AFailureIsWrittenToTheTextLogAsItsFullText()
        {
            var failure = new InvalidOperationException("the window could not be built");

            _mainWindowConstructionFailureReporter.RunMainWindowConstructionAndReportAnyFailure(() => throw failure);

            var textLogEntry = Assert.Single(_textLogEntries);
            Assert.StartsWith("System.InvalidOperationException: the window could not be built", textLogEntry);
        }

        [Fact]
        public void AFailureIsShownInAMessageBoxWithItsMessageAndTheCaptionError()
        {
            _mainWindowConstructionFailureReporter.RunMainWindowConstructionAndReportAnyFailure(() => throw new InvalidOperationException("the window could not be built"));

            Assert.Equal(("the window could not be built", "Error"), Assert.Single(_messageBoxes));
        }

        [Fact]
        public void AFailureIsNotRethrown()
        {
            var exception = Record.Exception(() =>
                _mainWindowConstructionFailureReporter.RunMainWindowConstructionAndReportAnyFailure(() => throw new InvalidOperationException("the window could not be built")));

            Assert.Null(exception);
        }
    }
}
