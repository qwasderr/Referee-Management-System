namespace SportSystem2.Helpers
{
    namespace SportSystem2.Helpers
    {
        public static class ImageValidator
        {
            private static readonly string[] AllowedTypes = { "image/jpeg", "image/png", "image/gif" };

            public static bool IsValidContentType(string? contentType)
            {
                if (string.IsNullOrWhiteSpace(contentType))
                    return false;

                return AllowedTypes.Contains(contentType.ToLower());
            }

            public static bool FileIsNull(IFormFile? file)
            {
                return file == null || file.Length == 0;
            }
            public static string[] GetAllowedTypes() => AllowedTypes;
        }
    }

}
