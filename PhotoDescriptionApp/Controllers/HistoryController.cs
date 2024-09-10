using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PhotoDescriptionApp.Models;
using PhotoDescriptionApp.Services;

namespace PhotoDescriptionApp.Controllers
{
    public class HistoryController : Controller
    {
        private readonly VisionService _visionService;
        private readonly string _blobConnectionString = "DefaultEndpointsProtocol=https;AccountName=appdev3blob;AccountKey=h5uNrDgTGn2H6R+yg/cn67hAPgfV+uiwvoEZ5nC8DlJr1D9qUwI7gJnDAsB1BbyqQiA+0ebCzRrJ+AStmYaDjw==;EndpointSuffix=core.windows.net";
        private readonly string _containerName = "images";

        public HistoryController()
        {
            _visionService = new VisionService();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var metadata = await GetImageMetadataFromBlobAsync();
            return View(metadata);
        }

        [HttpPost]
        public async Task<IActionResult> Search(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return RedirectToAction("Index");
            }

            var imageStream = await DownloadImageFromBlobAsync(imageUrl);
            if (imageStream == null)
            {
                return RedirectToAction("Index");
            }

            var description = await _visionService.DescribeImageAsync(imageStream, Path.GetFileName(imageUrl));
            return RedirectToAction("ImageDetails", "ImageUpload", new { imageUrl = imageUrl, description = description });
        }

        private async Task<Stream> DownloadImageFromBlobAsync(string imageUrl)
        {
            try
            {
                var blobServiceClient = new BlobServiceClient(_blobConnectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(Path.GetFileName(imageUrl));

                var response = await blobClient.DownloadAsync();
                return response.Value.Content;
            }
            catch
            {
                return null;
            }
        }

        private async Task<IEnumerable<ImageMetadata>> GetImageMetadataFromBlobAsync()
        {
            var metadataList = new List<ImageMetadata>();

            try
            {
                var blobServiceClient = new BlobServiceClient(_blobConnectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

                await foreach (var blobItem in containerClient.GetBlobsAsync())
                {
                    // Assume each blob's name is its metadata; adjust if using different metadata storage
                    metadataList.Add(new ImageMetadata
                    {
                        ImageUrl = containerClient.GetBlobClient(blobItem.Name).Uri.ToString(),
                        UploadDate = blobItem.Properties.CreatedOn?.DateTime ?? DateTime.MinValue
                    });
                }

                // Sort the metadata list by UploadDate in descending order
                metadataList = metadataList.OrderByDescending(m => m.UploadDate).ToList();
            }
            catch
            {
                // Handle errors as needed
            }

            return metadataList;
        }
    }
}
