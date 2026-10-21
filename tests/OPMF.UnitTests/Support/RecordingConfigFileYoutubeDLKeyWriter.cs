using System;
using System.Collections.Generic;
using OPMF.Settings;

namespace OPMF.UnitTests.Support
{
    public record YoutubeDLKeyWritten(string ConfigFilePath, string YoutubeDLKeyName, object Value);

    public class RecordingConfigFileYoutubeDLKeyWriter : IConfigFileYoutubeDLKeyWriter
    {
        public List<YoutubeDLKeyWritten> KeysWritten { get; } = new List<YoutubeDLKeyWritten>();
        public Exception ExceptionToThrowWhenWriting { get; set; }

        public void WriteYoutubeDLKeyKeepingEveryOtherKeyAsItIs(string configFilePath, string youtubeDLKeyName, object value)
        {
            if (ExceptionToThrowWhenWriting != null)
            {
                throw ExceptionToThrowWhenWriting;
            }
            KeysWritten.Add(new YoutubeDLKeyWritten(configFilePath, youtubeDLKeyName, value));
        }
    }
}
