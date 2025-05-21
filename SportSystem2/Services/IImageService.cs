namespace SportSystem2.Services
{
    public interface IImageService
    {
        Task<string?> SaveImageAsync(IFormFile image, string folderPath);
        void DeleteImage(string relativePath, string folderPath);
    }

}
