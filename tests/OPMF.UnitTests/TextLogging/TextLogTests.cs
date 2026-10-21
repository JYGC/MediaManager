using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Time.Testing;
using OPMF.TextLogging;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.TextLogging
{
    public class TextLogTests
    {
        private static readonly DateTimeOffset InstantTheFakeClockStartsAt = new DateTimeOffset(2026, 10, 18, 9, 30, 15, TimeSpan.Zero);

        private static string ReadWholeFileWhileTheTextLogStillHasItOpen(string textLogFilePath)
        {
            using var fileStream = new FileStream(textLogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var streamReader = new StreamReader(fileStream, Encoding.UTF8);
            return streamReader.ReadToEnd();
        }

        private static string FormatEntryTheWayTheTextLogDoes(DateTime entryTime, string text)
        {
            return $"\r\nDATETIME: {entryTime.ToLongTimeString()} {entryTime.ToLongDateString()}" + Environment.NewLine
                + "ERROR:" + Environment.NewLine
                + text + Environment.NewLine
                + "-------------------------------" + Environment.NewLine;
        }

        [Fact]
        public void LogEntryWritesTheClocksDateTimeThenErrorThenTheTextThenTheDashedLine()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var textLogFilePath = Path.Combine(temporaryFolder.FolderPath, "log.txt");
            var fakeTimeProvider = new FakeTimeProvider(InstantTheFakeClockStartsAt);
            var textLog = new TextLog(textLogFilePath, fakeTimeProvider);

            textLog.LogEntry("something failed");

            var entryTime = fakeTimeProvider.GetLocalNow().LocalDateTime;
            Assert.Equal(FormatEntryTheWayTheTextLogDoes(entryTime, "something failed"), ReadWholeFileWhileTheTextLogStillHasItOpen(textLogFilePath));
        }

        [Fact]
        public void NoFileExistsBeforeTheFirstEntry()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var textLogFilePath = Path.Combine(temporaryFolder.FolderPath, "log.txt");

            _ = new TextLog(textLogFilePath, new FakeTimeProvider(InstantTheFakeClockStartsAt));

            Assert.False(File.Exists(textLogFilePath));
        }

        [Fact]
        public void EntriesAreAppendedToAnExistingFile()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var textLogFilePath = Path.Combine(temporaryFolder.FolderPath, "log.txt");
            File.WriteAllText(textLogFilePath, "entry written earlier" + Environment.NewLine);
            var fakeTimeProvider = new FakeTimeProvider(InstantTheFakeClockStartsAt);
            var textLog = new TextLog(textLogFilePath, fakeTimeProvider);

            textLog.LogEntry("something failed");

            var entryTime = fakeTimeProvider.GetLocalNow().LocalDateTime;
            Assert.Equal(
                "entry written earlier" + Environment.NewLine + FormatEntryTheWayTheTextLogDoes(entryTime, "something failed"),
                ReadWholeFileWhileTheTextLogStillHasItOpen(textLogFilePath));
        }

        [Fact]
        public void ASecondEntryIsWrittenAfterTheFirstEntryWithTheClocksLaterTime()
        {
            using var temporaryFolder = new TemporaryFolderForOneTest();
            var textLogFilePath = Path.Combine(temporaryFolder.FolderPath, "log.txt");
            var fakeTimeProvider = new FakeTimeProvider(InstantTheFakeClockStartsAt);
            var textLog = new TextLog(textLogFilePath, fakeTimeProvider);
            textLog.LogEntry("first failure");
            var firstEntryTime = fakeTimeProvider.GetLocalNow().LocalDateTime;
            fakeTimeProvider.Advance(TimeSpan.FromMinutes(5));

            textLog.LogEntry("second failure");

            var secondEntryTime = fakeTimeProvider.GetLocalNow().LocalDateTime;
            Assert.Equal(
                FormatEntryTheWayTheTextLogDoes(firstEntryTime, "first failure") + FormatEntryTheWayTheTextLogDoes(secondEntryTime, "second failure"),
                ReadWholeFileWhileTheTextLogStillHasItOpen(textLogFilePath));
        }
    }
}
