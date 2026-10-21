using System;
using System.IO;

namespace OPMF.TextLogging
{
    public class TextLog
    {
        private readonly string _textLogFilePath;
        private readonly TimeProvider _timeProvider;
        private readonly object _fileOpeningAndWritingLock = new object();
        private StreamWriter _fileStream;

        public TextLog(string textLogFilePath, TimeProvider timeProvider)
        {
            _textLogFilePath = textLogFilePath;
            _timeProvider = timeProvider;
        }

        // The fallback for errors that could not be written to the database
        public void LogEntry(string error)
        {
            lock (_fileOpeningAndWritingLock)
            {
                _fileStream ??= File.Exists(_textLogFilePath) ? File.AppendText(_textLogFilePath) : File.CreateText(_textLogFilePath);
                var entryTime = _timeProvider.GetLocalNow().LocalDateTime;
                _fileStream.WriteLine($"\r\nDATETIME: {entryTime.ToLongTimeString()} {entryTime.ToLongDateString()}");
                _fileStream.WriteLine("ERROR:");
                _fileStream.WriteLine($"{error}");
                _fileStream.WriteLine("-------------------------------");
                _fileStream.Flush();
            }
        }
    }
}
