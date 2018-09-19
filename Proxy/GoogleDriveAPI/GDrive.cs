
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using AngleSharp.Html;
using AngleSharp.Network;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Proxy.EnumAtributes;
using File = Google.Apis.Drive.v3.Data.File;

namespace Proxy.GoogleDriveAPI
{
    public enum ContentType
    {
        [BaseAttribute.GoogleDriveType("application/vnd.google-apps.audio")]
        audio,
        [BaseAttribute.GoogleDriveType("application/vnd.google-apps.document")]
        document,
        [BaseAttribute.GoogleDriveType("application/vnd.google-apps.drawing")]
        drawing,
        [BaseAttribute.GoogleDriveType("application/vnd.google-apps.file")]
        file,
        [BaseAttribute.GoogleDriveType("application/vnd.google-apps.folder")]
        folder,
        [BaseAttribute.GoogleDriveType("application/vnd.google-apps.form")]
        form,
        [BaseAttribute.GoogleDriveType("application/vnd.google-apps.fusiontable")]
        fusiontable,
        [BaseAttribute.GoogleDriveType("application/vnd.google-apps.map")]
        map,
        [BaseAttribute.GoogleDriveType("application/vnd.google-apps.photo")]
        photo,
        [BaseAttribute.GoogleDriveType("application/vnd.google-apps.presentation")]
        presentation,
        [BaseAttribute.GoogleDriveType("application/vnd.google-apps.script")]
        script,
        [BaseAttribute.GoogleDriveType("application/vnd.google-apps.site")]
        site,
        [BaseAttribute.GoogleDriveType("application/vnd.ms-excel")]
        spreadsheet,
        [BaseAttribute.GoogleDriveType("application/vnd.google-apps.unknown")]
        unknown,
        [BaseAttribute.GoogleDriveType("application/vnd.google-apps.video")]
        video,
        [BaseAttribute.GoogleDriveType("application/vnd.google-apps.drive-sdk")]
        driveSdk
    }
    class GDrive
    {
        private string[] scopes = { DriveService.Scope.Drive };
        private string applicationName = "Proxy";

        private UserCredential credential;
        private DriveService driveService;


        public event Action<string> DownloadProgres;
        public event Action<string> ErrorMessage;
        public event Action<string, string> UploadCompleted;
        public event Action<string> UpdateCompleted;


        public GDrive(string jsonFileFullPath)
        {
            Authentication(jsonFileFullPath);
        }

        public GDrive()
        {
            Authentication();
        }
        private void Authentication(string jsonFileFullPath)
        {
            try
            {
                credential = GetUserCredentional(jsonFileFullPath);
                driveService = GetDriveService(credential);
            }
            catch (Exception e)
            {
                ErrorMessage.Invoke(e.Message);
            }
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
        /// <summary>
        /// Завантаження файлу з gDrive
        /// </summary>
        /// <param name="fileId">id файлу</param>
        /// <param name="filePath">куди зберігати</param>
        public async void DownloadFileFromDrive(string fileId, string filePath)
        {
            try
            {
                var request = driveService.Files.Get(fileId);

                using (var memoryStream = new MemoryStream())
                {
                    await request.DownloadAsync(memoryStream);

                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        await fileStream.WriteAsync(memoryStream.GetBuffer(), 0, memoryStream.GetBuffer().Length);
                    }
                    DownloadProgres?.Invoke(filePath);
                }
            }
            catch (Exception exception)
            {
                ErrorMessage.Invoke(exception.Message);
            }
        }

        /// <summary>
        /// Відпрака файлу на gDrive
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="filePath"></param>
        /// <param name="connentType"></param>
        public void UploadFileToDrive(string fileName, string filePath, ContentType connentType)
        {
            try
            {
                var fileMetadata = new File();
                fileMetadata.Name = fileName;

                FilesResource.CreateMediaUpload request;

                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    string type = connentType.GetGoogleDriveType();
                    request = driveService.Files.Create(fileMetadata, stream, type);
                    request.Upload();

                    var file = request.ResponseBody;
                    UploadCompleted.Invoke(file.Id, file.Name);
                    stream.Close();
                }
            }
            catch (Exception exception)
            {
                ErrorMessage.Invoke(exception.Message);
            }

        }
        public void UploadFileToDrive(string fileName, string filePath, string connentType)
        {
            try
            {
                var fileMetadata = new File();
                fileMetadata.Name = fileName;

                FilesResource.CreateMediaUpload request;

                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    request = driveService.Files.Create(fileMetadata, stream, connentType); request.Upload();
                    var file = request.ResponseBody;
                    UploadCompleted?.Invoke(file.Id, file.Name);
                    stream.Close();
                }

            }
            catch (Exception exception)
            {
                ErrorMessage.Invoke(exception.Message);
            }

        }
        /// <summary>
        /// Обновить файл
        /// </summary>
        /// <param name="fileId"> id файла</param>
        /// <param name="filePath"> путь к файлу</param>
        /// <param name="connentType">тип файла </param>
        public void UpdateFileAtDrive(string fileId, string filePath, string connentType)
        {
            try
            {
                var fileMetadata = new File();

                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    var request = driveService.Files.Update(fileMetadata, fileId, stream, connentType);
                    request.Upload();
                    UpdateCompleted?.Invoke(request.FileId);
                    stream.Close();
                }

            }
            catch (Exception exception)
            {
                ErrorMessage.Invoke(exception.Message);
            }
        }
        /// <summary>
        /// Обновить файл
        /// </summary>
        /// <param name="fileId"> id файла</param>
        /// <param name="filePath"> путь к файлу</param>
        /// <param name="connentType">тип файла </param>
        public void UpdateFileAtDrive(string fileId, string filePath, ContentType connentType)
        {
            try
            {
                var fileMetadata = new File();

                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    string content = connentType.GetGoogleDriveType();
                    var request = driveService.Files.Update(fileMetadata, fileId, stream, content);
                    request.Upload();
                    UpdateCompleted?.Invoke(request.FileId);
                    stream.Close();
                }

            }
            catch (Exception exception)
            {
                ErrorMessage.Invoke(exception.Message);
            }
        }

        private UserCredential GetUserCredentional(string proxySettingsFullPath)
        {

            using (var stream = new FileStream(proxySettingsFullPath, FileMode.Open, FileAccess.Read))
            {
                string creadPath = Environment.CurrentDirectory;
                creadPath = Path.Combine(creadPath, "gDriveUpload", "drive-credentinals.json");

                return GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "User",
                    CancellationToken.None,
                    new FileDataStore(creadPath, true)).Result;
            }
        }
        private UserCredential GetUserCredentional()
        {

            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string creadPath = Path.Combine(userDocumentsPath, "Proxy Master", "gDriveUpload", "drive-credentinals.json");

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
            return new DriveService(
                new BaseClientService.Initializer
                {
                    HttpClientInitializer = userCredential,
                    ApplicationName = applicationName
                });
        }
    }
}
