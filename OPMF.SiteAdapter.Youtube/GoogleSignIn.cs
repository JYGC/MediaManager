using System.IO;
using System.Threading;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

namespace OPMF.SiteAdapter.Youtube
{
    public class GoogleSignIn
    {
        private readonly string _clientSecretFilePath;
        private readonly string _credentialFolderPath;
        private readonly TextWriter _standardOutput;

        public GoogleSignIn(string clientSecretFilePath, string credentialFolderPath, TextWriter standardOutput)
        {
            _clientSecretFilePath = clientSecretFilePath;
            _credentialFolderPath = credentialFolderPath;
            _standardOutput = standardOutput;
        }

        public UserCredential SignInAndGetCredentialForScopes(string[] scopes)
        {
            using (FileStream stream = new FileStream(_clientSecretFilePath, FileMode.Open, FileAccess.Read))
            {
                _standardOutput.WriteLine("authenticate with google");
                UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(_credentialFolderPath, true)).Result;
                _standardOutput.WriteLine("credential file saved to: " + _credentialFolderPath);
                return credential;
            }
        }
    }
}
