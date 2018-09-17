
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using File = Google.Apis.Drive.v3.Data.File;

namespace Proxy.GoogleDriveAPI
{
    class GDrive
    {
        private string[] scopes = {DriveService.Scope.Drive};
        private string applicationName = "Proxy";

        private UserCredential credential;
        private DriveService driveService;


        public Action<Google.Apis.Download.IDownloadProgress> DownloadProgres;
        public event Action<string> ErrorMessage;

        public GDrive()
        {
           Authentication();
        }
        private void Authentication()
        {
            try
            {
                credential = GetUserCredentional();
                driveService = GetDriveService(credential);
            }
            catch (Exception e)
            {
                ErrorMessage.Invoke(e.Message);
            }
        }

        public Dictionary<string, string> GetUserFiles()
        {
            try
            {
                IList<File> files = driveService.Files.List().Execute().Files;
                Dictionary<string, string> result = new Dictionary<string, string>();
                foreach (var item in files)
                {
                    if (!result.ContainsKey(item.Name))
                    {
                        result.Add(item.Name, item.Id);
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                ErrorMessage.Invoke(e.Message);
                return null;
            }
        }

        public async void DownloadFileFromDrive(string fileId, string filePath)
        {
            try
            {
                var request = driveService.Files.Get(fileId);

                using (var memoryStream = new MemoryStream())
                {
                    request.MediaDownloader.ProgressChanged += DownloadProgres;

                    await request.DownloadAsync(memoryStream);

                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        await fileStream.WriteAsync(memoryStream.GetBuffer(), 0, memoryStream.GetBuffer().Length);
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorMessage.Invoke(exception.Message);
            }
            
        }


        private UserCredential GetUserCredentional()
        {
            string currentFile = $"{Environment.CurrentDirectory}\\client_secret.json";
            using (var stream = new FileStream(currentFile, FileMode.Open, FileAccess.Read))
            {
                string creadPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                creadPath = Path.Combine(creadPath, "driveApiCredentials", "drive-credentinals.json");

                return GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "User",
                    CancellationToken.None,
                    new FileDataStore(creadPath, true)).Result;
            }
        }

        private DriveService GetDriveService(UserCredential userCredential)
        {
            return  new DriveService(
                new BaseClientService.Initializer
                {
                    HttpClientInitializer =  userCredential,
                    ApplicationName = applicationName
                });
        }
    }
}
