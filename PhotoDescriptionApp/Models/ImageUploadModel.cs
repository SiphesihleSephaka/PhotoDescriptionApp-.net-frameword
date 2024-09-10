using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PhotoDescriptionApp.Models
{
    public class ImageUploadModel
    {
        [Required]
        public IFormFile ImageFile { get; set; }
    }
}
