using System;
using System.IO;

namespace OPMF.UnitTests.Support
{
    public class TemporaryFolderForOneTest : IDisposable
    {
        public string FolderPath { get; }

        public TemporaryFolderForOneTest()
        {
            FolderPath = Path.Combine(Path.GetTempPath(), "OPMFUnitTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(FolderPath);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(FolderPath))
                {
                    Directory.Delete(FolderPath, true);
                }
            }
            catch (IOException)
            {
                // a file that a test left open is not worth failing the test for
            }
        }
    }
}
