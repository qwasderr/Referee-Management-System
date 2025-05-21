namespace SportSystem2.Services
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;

    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const string BasePath = "images";

        public ImageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string?> SaveImageAsync(IFormFile image, string folderPath)
        {
            if (image == null || image.Length == 0)
                return null;

            string relativeFolderPath = Path.Combine(BasePath, folderPath);
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, relativeFolderPath);
            Directory.CreateDirectory(uploadsFolder);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return $"/{relativeFolderPath}/{fileName}".Replace("\\", "/");
        }

        public void DeleteImage(string relativePath, string folderPath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;

            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}