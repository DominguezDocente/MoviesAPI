using MoviesAPI.Entities;

namespace MoviesAPI.Services
{
    public interface IKeysService
    {
        Task<APIKey> CreateKey(string userId, KeyType keyType);
        string GenerateKey();
    }

    public class KeysService : IKeysService
    {
        private readonly ApplicationDbContext _context;

        public KeysService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<APIKey> CreateKey(string userId, KeyType keyType)
        {
            string key = GenerateKey();

            APIKey apiKey = new APIKey
            {
                Active  = true,
                Key = key,
                KeyType = keyType,
                UserId = userId
            };

            await _context.AddAsync(apiKey);
            await _context.SaveChangesAsync();

            return apiKey;
        }

        public string GenerateKey() => Guid.NewGuid().ToString().Replace("-","");
    }
}
