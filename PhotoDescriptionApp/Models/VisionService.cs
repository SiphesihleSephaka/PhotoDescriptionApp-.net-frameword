using Azure.Storage.Blobs;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.IO;
using System.Threading.Tasks;

namespace PhotoDescriptionApp.Services
{
    public class VisionService
    {
        private readonly string subscriptionKey = "72dfce98434a471ebc9ac7888111c90c";
        private readonly string endpoint = "https://universityoffuturevilleass2.cognitiveservices.azure.com/";

        // Blob storage details
        private readonly string blobConnectionString = "DefaultEndpointsProtocol=https;AccountName=appdev3blob;AccountKey=h5uNrDgTGn2H6R+yg/cn67hAPgfV+uiwvoEZ5nC8DlJr1D9qUwI7gJnDAsB1BbyqQiA+0ebCzRrJ+AStmYaDjw==;EndpointSuffix=core.windows.net";
        private readonly string containerName = "images";

        private ComputerVisionClient Authenticate()
        {
            return new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey))
            {
                Endpoint = endpoint
            };
        }

        public async Task<string> DescribeImageAsync(Stream imageStream, string fileName)
        {
            if (imageStream == null)
            {
                throw new ArgumentNullException(nameof(imageStream), "Image stream cannot be null");
            }

            var client = Authenticate();

            // Describe the image
            var descriptionResult = await client.DescribeImageInStreamAsync(imageStream);

            // Extract the description
            if (descriptionResult.Captions.Count > 0)
            {
                return descriptionResult.Captions[0].Text;
            }
            else
            {
                return "No description available.";
            }
        }


        public async Task<string> UploadImageToBlobAsync(Stream imageStream, string fileName)
        {
            // Create a BlobServiceClient
            var blobServiceClient = new BlobServiceClient(blobConnectionString);

            // Get a reference to the container
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure the container exists
            await containerClient.CreateIfNotExistsAsync();

            // Get a reference to the blob (file)
            var blobClient = containerClient.GetBlobClient(fileName);

            // Upload the file to the blob
            await blobClient.UploadAsync(imageStream, overwrite: true);

            // Return the URL of the uploaded blob
            return blobClient.Uri.ToString();
        }

        public async Task<Stream> GetImageStreamAsync(string imageUrl)
        {
            var blobServiceClient = new BlobServiceClient(blobConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(new Uri(imageUrl).AbsolutePath.TrimStart('/'));

            if (await blobClient.ExistsAsync())
            {
                var blobDownloadInfo = await blobClient.DownloadAsync();
                return blobDownloadInfo.Value.Content;
            }

            return null;
        }
    }
}
