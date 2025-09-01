using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace RM.BlobStorage
{
    public interface IAzureBlobStorageService
    {
        public string Upload(FileStream uploadFileStream, string fileName);
        public string Upload(string uploadFileStream, string fileName);
        Task<string?> UploadImage(IFormFile file);
        Task<bool> DeleteImage(string fileName);
    }

    public class AzureBlobStorageService : IAzureBlobStorageService
    {
        readonly string _connectionString;
        readonly string _containerName;

        public AzureBlobStorageService(IOptions<BlobStorageConfigModel> azure)
        {
            //"DefaultEndpointsProtocol=https;AccountName=communitypostdata;AccountKey=dwV604p4JEQP0yfrdz20IIz0r/Cc/tS1KIES7cDC6p+fbXwaoDgiKQfPWlsF16ObHLEKuZ10Gscv+ASt6E5hMQ==;EndpointSuffix=core.windows.net";
            _containerName = azure.Value.ContainerNameFirst;
            _connectionString = azure.Value.StorageConnection;
        }

        public string Upload(string localFilePath, string fileName)
        {
            //localFilePath = "D:\\temp\\dashboard.jpg";
            //fileName = "dashboard.jpg";
            try
            {
                using FileStream uploadFileStream = File.OpenRead(localFilePath);
                BlobServiceClient blobServiceClient = new(_connectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                BlobClient blobClient = containerClient.GetBlobClient(fileName);
                blobClient.Upload(uploadFileStream, true);
                uploadFileStream.Close();
                Console.WriteLine($"File uploaded to Blob Storage: {blobClient.Uri}");
                return blobClient.Uri.ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public string Upload(FileStream uploadFileStream, string fileName)
        {
            if (uploadFileStream != null)
            {
                BlobServiceClient blobServiceClient = new(_connectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                BlobClient blobClient = containerClient.GetBlobClient(fileName);
                blobClient.Upload(uploadFileStream, true);
                uploadFileStream.Close();
                Console.WriteLine($"File uploaded to Blob Storage: {blobClient.Uri}");
                return blobClient.Uri.ToString();
            }

            return null;
        }

        /// <summary>
        /// Use this to upload an image to the blob storage
        /// </summary>
        /// <returns></returns>
        public async Task<string?> UploadImage(IFormFile file)
        {
            if (file is not { Length: > 0 }) return null;
            // Create blob service client
            BlobServiceClient blobServiceClient = new(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            // Generate unique filename to avoid overwrites
            var uniqueFileName = $"{DateTime.Now:yyyyMMddHHmmssfff}{Path.GetExtension(file.FileName)}";
            var blobClient = containerClient.GetBlobClient(uniqueFileName);

            // Upload the file
            await using var stream = file.OpenReadStream();
            var dd = await blobClient.UploadAsync(stream, true);

            // return blobClient.Uri.ToString();
            return uniqueFileName;
        }

        public async Task DeleteFileAsync(string blobName)
        {
            var blobClient = new BlobClient(_connectionString, _containerName, blobName);
            await blobClient.DeleteIfExistsAsync();
        }

        /// <summary>
        /// Use this method to delete an image from the blob storage based on the filename
        /// </summary>
        public async Task<bool> DeleteImage(string fileName)
        {
            try
            {
                BlobServiceClient blobServiceClient = new(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(fileName);
                await blobClient.DeleteIfExistsAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}