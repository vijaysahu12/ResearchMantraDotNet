using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RM.API.Helpers
{
    public class GdriveImageUpload
    {
        private readonly string PathToServiceAccountKeyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gdriveCredentials.json");
        private readonly string DirectoryId = "15qmjhzMWwUHyqSAxR3n3VxH2scOPdyHQ";

        private static string GenerateFileName(IFormFile image)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string extension = Path.GetExtension(image.FileName);
            return $"{timestamp}{extension}";
        }

        public async Task<string?> UploadImageToGDriveAsync(IFormFile image)
        {
            try
            {
                // Generate the filename with timestamp and extension
                string uploadFileName = GenerateFileName(image);

                // Save the image in assets folder first to upload into GDrive
                string assetsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "CallPerformanceImage");
                string filePath = Path.Combine(assetsFolderPath, uploadFileName);

                // Ensure the assets folder exists
                if (!Directory.Exists(assetsFolderPath))
                {
                    Directory.CreateDirectory(assetsFolderPath);
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }




                // Load the service account credentials
                GoogleCredential credential;
                using (var stream = new FileStream(PathToServiceAccountKeyFile, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped(DriveService.ScopeConstants.Drive);
                }

                // Create the Drive API service
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "RM.API"
                });

                // Create file metadata for the image upload
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = uploadFileName,
                    Parents = new[] { DirectoryId }
                };

                // Upload the image file
                string uploadedFileId;
                using (var fsSource = image.OpenReadStream())
                {
                    var request = service.Files.Create(fileMetadata, fsSource, image.ContentType);
                    request.Fields = "*";
                    var results = await request.UploadAsync(CancellationToken.None);

                    if (results.Status == UploadStatus.Completed)
                    {
                        uploadedFileId = request.ResponseBody?.Id;
                    }
                    else
                    {
                        return null;
                    }
                }

                // Update file permissions to make it accessible to anyone with the link
                var permission = new Google.Apis.Drive.v3.Data.Permission()
                {
                    Type = "anyone",
                    Role = "reader"
                };

                var permissionRequest = service.Permissions.Create(permission, uploadedFileId);
                await permissionRequest.ExecuteAsync();

                // Get file metadata to retrieve the URL
                var getFileRequest = service.Files.Get(uploadedFileId);
                getFileRequest.Fields = "id, name, webViewLink, webContentLink";
                var file = await getFileRequest.ExecuteAsync();

                if (file != null)
                {
                    // Delete the file after uploading to the drive 
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    return file.WebViewLink;
                }
                else
                {
                    Console.WriteLine("File not found.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }
    }
}

