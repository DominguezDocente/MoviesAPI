using Microsoft.AspNetCore.Identity;
using MoviesAPI.Entities;
using System.Security.Claims;

namespace MoviesAPI.Services
{
    public interface IUsersService
    {
        public string? GetUserId();
    }
    public class UsersService : IUsersService
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsersService(UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetUserId()
        {
            Claim claimId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "userId");

            if (claimId is null)
            {
                return null;
            }

            return claimId.Value;
        }
    }
}
