using System.Collections.Generic;
using System.IO;
using System.Text;
using MediaManager.Services2;
using OPMF.Filesystem;
using OPMF.Settings;

namespace OPMF.UnitTests.Support
{
    public record FilesMove(string SourceFolderPath, string DestinationFolderPath, List<string> FileExtensions);

    public class RecordingMissingFolderCreator : IMissingFolderCreator
    {
        private readonly CallRecorder _callRecorder;

        public RecordingMissingFolderCreator(CallRecorder callRecorder = null)
        {
            _callRecorder = callRecorder ?? new CallRecorder();
        }

        public List<string> FoldersCreated { get; } = new List<string>();

        public void CreateAppFolderAndItsDownloadsDatabasesAndBinFoldersWhenMissing(IReadonlySettings readonlySettings)
        {
            _callRecorder.Calls.Add("CreateAppFolderAndItsDownloadsDatabasesAndBinFoldersWhenMissing");
        }

        public void CreateFolderWhenMissing(string folderPath)
        {
            _callRecorder.Calls.Add("CreateFolderWhenMissing");
            FoldersCreated.Add(folderPath);
        }
    }

    public class RecordingDownloadedVideoFileMover : IDownloadedVideoFileMover
    {
        private readonly CallRecorder _callRecorder;

        public RecordingDownloadedVideoFileMover(CallRecorder callRecorder = null)
        {
            _callRecorder = callRecorder ?? new CallRecorder();
        }

        public List<FilesMove> Moves { get; } = new List<FilesMove>();

        public void MoveFilesWithExtensionsFromFolderToFolderSkippingNamesThatAlreadyExist(string sourceFolderPath, string destinationFolderPath, IEnumerable<string> fileExtensionsToMove)
        {
            _callRecorder.Calls.Add("MoveFilesWithExtensionsFromFolderToFolderSkippingNamesThatAlreadyExist");
            Moves.Add(new FilesMove(sourceFolderPath, destinationFolderPath, new List<string>(fileExtensionsToMove)));
        }
    }

    public class CallRecordingStandardOutput : TextWriter
    {
        private readonly CallRecorder _callRecorder;

        public CallRecordingStandardOutput(CallRecorder callRecorder)
        {
            _callRecorder = callRecorder;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine(string value)
        {
            _callRecorder.Calls.Add("print: " + value);
        }
    }
}
