using Project_X_Data.Services.Storage;
using Microsoft.AspNetCore.Mvc;

namespace Project_X_Data.Controllers
{
    public class StorageController(IStorageService storageService) : Controller
    {
        private readonly IStorageService _storageService = storageService;
        public IActionResult Item(String id)
        {
            String ext = Path.GetExtension(id);
            String mimeType = ext switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/jpeg",
                _ => "application/octet-stream"
            };
            var bytes = _storageService.Load(id);

            return  bytes == null ? NotFound() : File(bytes, mimeType);
        }
    }
}
