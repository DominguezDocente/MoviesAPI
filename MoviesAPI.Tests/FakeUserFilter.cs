using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace MoviesAPI.Tests
{
    public class FakeUserFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {            
            string defaultUserId = "898fad99-d01e-43f6-b004-3055f1de6e3a"; 
            string defaultUserEmail = "test@test.com"; 

            context.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.Name, defaultUserEmail),
                new Claim(ClaimTypes.Email, defaultUserId),
                new Claim(ClaimTypes.NameIdentifier, defaultUserId),
            }));

            await next();
        }
    }
}
