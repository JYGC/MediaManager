using OPMF.SiteAdapter.Youtube;
using Xunit;

namespace OPMF.UnitTests.SiteAdapter
{
    public class GoogleClientSecretFileLocationTests
    {
        [Fact]
        public void TheClientSecretFileIsInTheFolderOfTheEntryExecutable()
        {
            var clientSecretFilePath = GoogleClientSecretFileLocation.GetClientSecretFilePathInFolderOfEntryExecutable(
                "file:///C:/Apps/OffPeak/OffPeakMediaFetcher.dll",
                "gmail-python-quickstart.json");

            Assert.Equal(@"C:\Apps\OffPeak\gmail-python-quickstart.json", clientSecretFilePath);
        }

        [Fact]
        public void ASecretPathWithASubfolderIsJoinedOntoTheFolderOfTheEntryExecutable()
        {
            var clientSecretFilePath = GoogleClientSecretFileLocation.GetClientSecretFilePathInFolderOfEntryExecutable(
                "file:///C:/Apps/OffPeak/OffPeakMediaFetcher.dll",
                @"secrets\client.json");

            Assert.Equal(@"C:\Apps\OffPeak\secrets\client.json", clientSecretFilePath);
        }
    }
}
