using System;
using System.Collections.Generic;
using MediaManager.Dtos.Services;
using MediaManager.Services2;
using OPMF.Settings;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Services
{
    public class DownloadQualitySettingsServicesTests
    {
        private readonly TemporaryFolderReadonlySettings _readonlySettings = new TemporaryFolderReadonlySettings(@"C:\NoSuchFolder");
        private readonly RecordingConfigFileYoutubeDLKeyWriter _recordingConfigFileYoutubeDLKeyWriter = new RecordingConfigFileYoutubeDLKeyWriter();
        private readonly FakeLogRepository _fakeLogRepository = new FakeLogRepository();
        private readonly List<string> _configFilePathsRead = new List<string>();
        private AppConfig _appConfigInTheConfigFile = new AppConfig();

        private DownloadQualitySettingsServices CreateServices()
        {
            return new DownloadQualitySettingsServices(
                _readonlySettings,
                configFilePath =>
                {
                    _configFilePathsRead.Add(configFilePath);
                    return _appConfigInTheConfigFile;
                },
                _recordingConfigFileYoutubeDLKeyWriter,
                _fakeLogRepository);
        }

        [Fact]
        public void TheAppWideDownloadQualityIsReadFromTheConfigFileOfTheApp()
        {
            CreateServices().GetAppWideDownloadQualityFromConfigFile();

            Assert.Equal(new[] { _readonlySettings.GetConfigFilePath() }, _configFilePathsRead);
        }

        [Fact]
        public void TheDefaultConfigGivesH264NoLimitAndBestAac()
        {
            var downloadQuality = CreateServices().GetAppWideDownloadQualityFromConfigFile();

            Assert.Equal(ServiceVideoCodecChoice.H264, downloadQuality.VideoCodecToPreferWhenDownloading);
            Assert.Equal(ServiceMaximumVideoResolutionChoice.NoLimit, downloadQuality.MaximumVideoResolutionOnTheShorterSide);
            Assert.Equal(ServiceAudioQualityChoice.BestAac, downloadQuality.AudioQualityToDownload);
        }

        [Fact]
        public void TheThreeValuesReadFromTheConfigFileAreMapped()
        {
            _appConfigInTheConfigFile.YoutubeDL.VideoCodecToPreferWhenDownloading = VideoCodecChoice.AnyCodec;
            _appConfigInTheConfigFile.YoutubeDL.MaximumVideoResolutionOnTheShorterSide = MaximumVideoResolutionChoice.UpTo1440p;
            _appConfigInTheConfigFile.YoutubeDL.AudioQualityToDownload = AudioQualityChoice.SmallestAac;

            var downloadQuality = CreateServices().GetAppWideDownloadQualityFromConfigFile();

            Assert.Equal(ServiceVideoCodecChoice.AnyCodec, downloadQuality.VideoCodecToPreferWhenDownloading);
            Assert.Equal(ServiceMaximumVideoResolutionChoice.UpTo1440p, downloadQuality.MaximumVideoResolutionOnTheShorterSide);
            Assert.Equal(ServiceAudioQualityChoice.SmallestAac, downloadQuality.AudioQualityToDownload);
        }

        [Theory]
        [InlineData(ServiceVideoCodecChoice.H264, VideoCodecChoice.H264)]
        [InlineData(ServiceVideoCodecChoice.AnyCodec, VideoCodecChoice.AnyCodec)]
        public void SavingTheVideoCodecWritesOnlyItsKeyToTheConfigFile(ServiceVideoCodecChoice choiceToSave, VideoCodecChoice expectedValueWritten)
        {
            CreateServices().SaveAppWideVideoCodecChoice(choiceToSave);

            var keyWritten = Assert.Single(_recordingConfigFileYoutubeDLKeyWriter.KeysWritten);
            Assert.Equal(_readonlySettings.GetConfigFilePath(), keyWritten.ConfigFilePath);
            Assert.Equal(nameof(YoutubeDLConfig.VideoCodecToPreferWhenDownloading), keyWritten.YoutubeDLKeyName);
            Assert.Equal(expectedValueWritten, keyWritten.Value);
        }

        [Theory]
        [InlineData(ServiceMaximumVideoResolutionChoice.NoLimit, MaximumVideoResolutionChoice.NoLimit)]
        [InlineData(ServiceMaximumVideoResolutionChoice.UpTo2160p, MaximumVideoResolutionChoice.UpTo2160p)]
        [InlineData(ServiceMaximumVideoResolutionChoice.UpTo1440p, MaximumVideoResolutionChoice.UpTo1440p)]
        [InlineData(ServiceMaximumVideoResolutionChoice.UpTo1080p, MaximumVideoResolutionChoice.UpTo1080p)]
        [InlineData(ServiceMaximumVideoResolutionChoice.UpTo720p, MaximumVideoResolutionChoice.UpTo720p)]
        [InlineData(ServiceMaximumVideoResolutionChoice.UpTo480p, MaximumVideoResolutionChoice.UpTo480p)]
        [InlineData(ServiceMaximumVideoResolutionChoice.UpTo360p, MaximumVideoResolutionChoice.UpTo360p)]
        public void SavingTheMaximumVideoResolutionWritesOnlyItsKeyToTheConfigFile(ServiceMaximumVideoResolutionChoice choiceToSave, MaximumVideoResolutionChoice expectedValueWritten)
        {
            CreateServices().SaveAppWideMaximumVideoResolutionChoice(choiceToSave);

            var keyWritten = Assert.Single(_recordingConfigFileYoutubeDLKeyWriter.KeysWritten);
            Assert.Equal(_readonlySettings.GetConfigFilePath(), keyWritten.ConfigFilePath);
            Assert.Equal(nameof(YoutubeDLConfig.MaximumVideoResolutionOnTheShorterSide), keyWritten.YoutubeDLKeyName);
            Assert.Equal(expectedValueWritten, keyWritten.Value);
        }

        [Theory]
        [InlineData(ServiceAudioQualityChoice.BestAac, AudioQualityChoice.BestAac)]
        [InlineData(ServiceAudioQualityChoice.BestOfAnyCodec, AudioQualityChoice.BestOfAnyCodec)]
        [InlineData(ServiceAudioQualityChoice.SmallestAac, AudioQualityChoice.SmallestAac)]
        public void SavingTheAudioQualityWritesOnlyItsKeyToTheConfigFile(ServiceAudioQualityChoice choiceToSave, AudioQualityChoice expectedValueWritten)
        {
            CreateServices().SaveAppWideAudioQualityChoice(choiceToSave);

            var keyWritten = Assert.Single(_recordingConfigFileYoutubeDLKeyWriter.KeysWritten);
            Assert.Equal(_readonlySettings.GetConfigFilePath(), keyWritten.ConfigFilePath);
            Assert.Equal(nameof(YoutubeDLConfig.AudioQualityToDownload), keyWritten.YoutubeDLKeyName);
            Assert.Equal(expectedValueWritten, keyWritten.Value);
        }

        [Fact]
        public void ASuccessfulSaveRecordsNoError()
        {
            CreateServices().SaveAppWideMaximumVideoResolutionChoice(ServiceMaximumVideoResolutionChoice.UpTo720p);

            Assert.Empty(_fakeLogRepository.RecordedErrors);
        }

        [Fact]
        public void AFailingWriteRecordsAnErrorWithTheKeyAndTheValueAndRethrowsTheSameException()
        {
            var exceptionOfTheWrite = new UnauthorizedAccessException("config.json is read-only");
            _recordingConfigFileYoutubeDLKeyWriter.ExceptionToThrowWhenWriting = exceptionOfTheWrite;

            var exceptionThrown = Assert.Throws<UnauthorizedAccessException>(() => CreateServices().SaveAppWideMaximumVideoResolutionChoice(ServiceMaximumVideoResolutionChoice.UpTo720p));

            Assert.Same(exceptionOfTheWrite, exceptionThrown);
            var recordedError = Assert.Single(_fakeLogRepository.RecordedErrors);
            Assert.Equal("config.json is read-only", recordedError.Message);
            Assert.Contains("UnauthorizedAccessException", recordedError.ExceptionText);
            Assert.Equal("\"MaximumVideoResolutionOnTheShorterSide\"", recordedError.VariablesAsJsonText["youtubeDLKeyName"]);
            Assert.Equal("\"UpTo720p\"", recordedError.VariablesAsJsonText["value"]);
        }

        [Fact]
        public void AFailingWriteOfTheVideoCodecAndOfTheAudioQualityIsRecordedWithTheirOwnKeys()
        {
            _recordingConfigFileYoutubeDLKeyWriter.ExceptionToThrowWhenWriting = new InvalidOperationException("no");
            var services = CreateServices();

            Assert.Throws<InvalidOperationException>(() => services.SaveAppWideVideoCodecChoice(ServiceVideoCodecChoice.AnyCodec));
            Assert.Throws<InvalidOperationException>(() => services.SaveAppWideAudioQualityChoice(ServiceAudioQualityChoice.SmallestAac));

            Assert.Equal(2, _fakeLogRepository.RecordedErrors.Count);
            Assert.Equal("\"VideoCodecToPreferWhenDownloading\"", _fakeLogRepository.RecordedErrors[0].VariablesAsJsonText["youtubeDLKeyName"]);
            Assert.Equal("\"AnyCodec\"", _fakeLogRepository.RecordedErrors[0].VariablesAsJsonText["value"]);
            Assert.Equal("\"AudioQualityToDownload\"", _fakeLogRepository.RecordedErrors[1].VariablesAsJsonText["youtubeDLKeyName"]);
            Assert.Equal("\"SmallestAac\"", _fakeLogRepository.RecordedErrors[1].VariablesAsJsonText["value"]);
        }
    }
}
