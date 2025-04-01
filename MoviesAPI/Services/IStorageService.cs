namespace MoviesAPI.Services
{
    public interface IStorageService
    {
        public Task DeleteFile(string path, string container);
        public Task<string> SaveFileAsync(byte[] content, string extension, string container, string contentType);
        public Task<string> UpdateFile(byte[] content, string extension, string container, string path, string contentType);
    }

    public class LocalStorageService : IStorageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LocalStorageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task DeleteFile(string path, string container)
        {
            if (path is not null)
            {
                string fileName = Path.GetFileName(path);
                string fileDirectory = Path.Combine(_env.WebRootPath, container, fileName);

                if (File.Exists(fileDirectory))
                {
                    File.Delete(fileDirectory);
                }
            }

            return Task.FromResult(0);
        }

        public async Task<string> SaveFileAsync(byte[] content, string extension, string container, string contentType)
        {
            string fileName = $"{Guid.NewGuid()}{extension}";
            string folder = Path.Combine(_env.WebRootPath, container);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string path = Path.Combine(folder, fileName);
            await File.WriteAllBytesAsync(path, content);

            string currentUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";

            string dbUrl = Path.Combine(currentUrl, container, fileName).Replace("\\","/");

            return dbUrl;
        }

        public async Task<string> UpdateFile(byte[] content, string extension, string container, string path, string contentType)
        {
            await DeleteFile(path, container);
            return await SaveFileAsync(content, extension, container, contentType);
        }
    }

    public class AzureStorageService : IStorageService
    {
        public Task DeleteFile(string path, string container)
        {
            throw new NotImplementedException();
        }

        public Task<string> SaveFileAsync(byte[] content, string extension, string container, string contentType)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpdateFile(byte[] content, string extension, string container, string path, string contentType)
        {
            throw new NotImplementedException();
        }
    }
}
