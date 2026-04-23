
namespace Project_X_Data.Services.Storage
{
    public class DiskStorageService : IStorageService
    {
        private const String path = "C:/StorageASP32/";
        private static readonly String[] allowedExtentions = [".jpg", ".png", ".jpeg"];
        public byte[] Load(string filename)
        {
            String fullName = Path.Combine(path, filename);
            if (File.Exists(fullName)) {
                return File.ReadAllBytes(fullName);
            }
            return null;

        }

        public string Save(IFormFile file)
        {
            
            int dotIndex = file.FileName.LastIndexOf('.');
            if (dotIndex == -1) {
                throw new ArgumentException("File name must have an extention.");
            }
            String ext = file.FileName[dotIndex..].ToLower();

            if (!allowedExtentions.Contains(ext)) {
                throw new ArgumentException($"File extention '{ext}' not supported");
            }
            String filename = Guid.NewGuid().ToString() + ext;

            using FileStream fileStream = new(Path.Combine(path, filename),
                FileMode.Create);

            file.CopyTo(fileStream);

            return filename;
        }
    }
}
