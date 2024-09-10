using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using PhotoDescriptionApp.Models;
using PhotoDescriptionApp.Services;
using System.Threading.Tasks;

namespace PhotoDescriptionApp.Controllers
{
    public class ImageUploadController : Controller
    {
        private readonly VisionService _visionService;

        public ImageUploadController()
        {
            _visionService = new VisionService();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(ImageUploadModel model)
        {
            if (model.ImageFile != null)
            {
                var fileName = model.ImageFile.FileName;
                var fileExtension = Path.GetExtension(fileName).ToLower();
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                if (allowedExtensions.Contains(fileExtension))
                {
                    using (var stream = model.ImageFile.OpenReadStream())
                    {
                        // Upload the image to Blob Storage and get the URL
                        var imageUrl = await _visionService.UploadImageToBlobAsync(stream, fileName);

                        // Reset the stream to the beginning for further processing
                        stream.Position = 0;

                        // Get the description of the image from the Computer Vision API
                        var description = await _visionService.DescribeImageAsync(stream, fileName);

                        // Redirect to the ImageDetails view with the image URL and description
                        return RedirectToAction("ImageDetails", new { imageUrl = imageUrl, description = description });
                    }
                }
                else
                {
                    ModelState.AddModelError("ImageFile", "The uploaded file is not a valid image.");
                }
            }
            else
            {
                ModelState.AddModelError("ImageFile", "Please upload an image file.");
            }

            return View(model);
        }




        [HttpGet]
        public IActionResult ImageDetails(string imageUrl, string description)
        {
            // Pass the image file name and description to the view
            var model = new ImageDetailsModel
            {
                ImageUrl = imageUrl,
                Description = description
            };

            return View(model);
        }
    }
}
