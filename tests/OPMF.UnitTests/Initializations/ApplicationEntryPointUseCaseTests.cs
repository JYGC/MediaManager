using System;
using System.Collections.Generic;
using System.IO;
using MediaManager.Initializations;
using OPMF.UnitTests.Support;
using Xunit;

namespace OPMF.UnitTests.Initializations
{
    public class ApplicationEntryPointUseCaseTests
    {
        private class FakeApplicationStartupUseCase : IApplicationStartupUseCase
        {
            private readonly CallRecorder _callRecorder;

            public FakeApplicationStartupUseCase(CallRecorder callRecorder)
            {
                _callRecorder = callRecorder;
            }

            public Exception ExceptionToThrow { get; set; }

            public void InitializeReadonlySettingsThenEstablishAppFoldersThenEstablishConfig()
            {
                _callRecorder.Calls.Add("startup");
                if (ExceptionToThrow != null)
                {
                    throw ExceptionToThrow;
                }
            }
        }

        private class FakeUnhandledErrorRecordingUseCase : IUnhandledErrorRecordingUseCase
        {
            private readonly CallRecorder _callRecorder;

            public FakeUnhandledErrorRecordingUseCase(CallRecorder callRecorder)
            {
                _callRecorder = callRecorder;
            }

            public List<Exception> ExceptionsRecorded { get; } = new List<Exception>();
            public Exception ExceptionToThrow { get; set; }

            public void RecordUnhandledError(Exception unhandledException)
            {
                _callRecorder.Calls.Add("record");
                ExceptionsRecorded.Add(unhandledException);
                if (ExceptionToThrow != null)
                {
                    throw ExceptionToThrow;
                }
            }
        }

        private readonly CallRecorder _callRecorder = new CallRecorder();
        private readonly FakeApplicationStartupUseCase _fakeApplicationStartupUseCase;
        private readonly FakeUnhandledErrorRecordingUseCase _fakeUnhandledErrorRecordingUseCase;
        private readonly StringWriter _standardOutput = new StringWriter();
        private int _timesTheRecorderWasCreated;
        private readonly ApplicationEntryPointUseCase _applicationEntryPointUseCase;

        public ApplicationEntryPointUseCaseTests()
        {
            _fakeApplicationStartupUseCase = new FakeApplicationStartupUseCase(_callRecorder);
            _fakeUnhandledErrorRecordingUseCase = new FakeUnhandledErrorRecordingUseCase(_callRecorder);
            _applicationEntryPointUseCase = new ApplicationEntryPointUseCase(
                _fakeApplicationStartupUseCase,
                new Lazy<IUnhandledErrorRecordingUseCase>(() =>
                {
                    _timesTheRecorderWasCreated++;
                    return _fakeUnhandledErrorRecordingUseCase;
                }),
                _standardOutput);
        }

        [Fact]
        public void StartupRunsBeforeTheApplication()
        {
            _applicationEntryPointUseCase.RunStartupThenRunApplicationThenRecordAndPrintAnyUnhandledError(() => _callRecorder.Calls.Add("application"));

            Assert.Equal(new[] { "startup", "application" }, _callRecorder.Calls);
        }

        [Fact]
        public void WhenStartupFailsTheApplicationDoesNotRun()
        {
            _fakeApplicationStartupUseCase.ExceptionToThrow = new InvalidOperationException("config.json is broken");

            _applicationEntryPointUseCase.RunStartupThenRunApplicationThenRecordAndPrintAnyUnhandledError(() => _callRecorder.Calls.Add("application"));

            Assert.DoesNotContain("application", _callRecorder.Calls);
        }

        [Fact]
        public void AnExceptionFromStartupIsRecordedAndItsMessageIsWrittenToStandardOutput()
        {
            var exceptionOfStartup = new InvalidOperationException("config.json is broken");
            _fakeApplicationStartupUseCase.ExceptionToThrow = exceptionOfStartup;

            _applicationEntryPointUseCase.RunStartupThenRunApplicationThenRecordAndPrintAnyUnhandledError(() => { });

            Assert.Same(exceptionOfStartup, Assert.Single(_fakeUnhandledErrorRecordingUseCase.ExceptionsRecorded));
            Assert.Equal("config.json is broken" + Environment.NewLine, _standardOutput.ToString());
        }

        [Fact]
        public void AnExceptionFromTheApplicationIsRecordedAndItsMessageIsWrittenToStandardOutput()
        {
            var exceptionOfTheApplication = new Exception("Invalid argument.");

            _applicationEntryPointUseCase.RunStartupThenRunApplicationThenRecordAndPrintAnyUnhandledError(() => throw exceptionOfTheApplication);

            Assert.Same(exceptionOfTheApplication, Assert.Single(_fakeUnhandledErrorRecordingUseCase.ExceptionsRecorded));
            Assert.Equal("Invalid argument." + Environment.NewLine, _standardOutput.ToString());
        }

        [Fact]
        public void TheRecorderIsNotCreatedWhenStartupAndTheApplicationSucceed()
        {
            _applicationEntryPointUseCase.RunStartupThenRunApplicationThenRecordAndPrintAnyUnhandledError(() => { });

            Assert.Equal(0, _timesTheRecorderWasCreated);
        }

        [Fact]
        public void TheRecorderIsCreatedAfterStartupRanAndOnlyWhenAnErrorMustBeRecorded()
        {
            _applicationEntryPointUseCase.RunStartupThenRunApplicationThenRecordAndPrintAnyUnhandledError(() => throw new Exception("the application failed"));

            Assert.Equal(1, _timesTheRecorderWasCreated);
            Assert.Equal(new[] { "startup", "record" }, _callRecorder.Calls);
        }

        [Fact]
        public void NothingIsRecordedOrWrittenWhenStartupAndTheApplicationSucceed()
        {
            _applicationEntryPointUseCase.RunStartupThenRunApplicationThenRecordAndPrintAnyUnhandledError(() => { });

            Assert.Empty(_fakeUnhandledErrorRecordingUseCase.ExceptionsRecorded);
            Assert.Equal(string.Empty, _standardOutput.ToString());
        }

        [Fact]
        public void AFailureWhileRecordingPropagatesAndNothingIsWritten()
        {
            var exceptionWhileRecording = new InvalidOperationException("the read-only settings were never kept");
            _fakeUnhandledErrorRecordingUseCase.ExceptionToThrow = exceptionWhileRecording;

            var exceptionThatPropagated = Assert.Throws<InvalidOperationException>(
                () => _applicationEntryPointUseCase.RunStartupThenRunApplicationThenRecordAndPrintAnyUnhandledError(() => throw new Exception("the application failed")));

            Assert.Same(exceptionWhileRecording, exceptionThatPropagated);
            Assert.Equal(string.Empty, _standardOutput.ToString());
        }
    }
}
