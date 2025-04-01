using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using MoviesAPI.Helpers;
using System.Net;

namespace MoviesAPI.Middlewares
{
    public class LimitRequestsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOptionsMonitor<LimitRequestsDTO> _optionsMonitorLimitRequests;

        public LimitRequestsMiddleware(RequestDelegate next, IOptionsMonitor<LimitRequestsDTO> optionsMonitorLimitRequests)
        {
            _next = next;
            _optionsMonitorLimitRequests = optionsMonitorLimitRequests;
        }

        public async Task InvokeAsync(HttpContext httpContext, ApplicationDbContext context)
        {
            Endpoint endpoint = httpContext.GetEndpoint();

            if (endpoint is null)
            {
                await _next.Invoke(httpContext);
                return;
            }

            ControllerActionDescriptor actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();

            if (actionDescriptor is not null)
            {
                bool actionHasDisableLimitRequestsAttribute = actionDescriptor.MethodInfo
                    .GetCustomAttributes(typeof(DisableLimitRequestsAttribute), inherit: true)
                    .Any();

                bool controllerHasDisableLimitRequestsAttribute = actionDescriptor.ControllerTypeInfo
                    .GetCustomAttributes(typeof(DisableLimitRequestsAttribute), inherit: true)
                    .Any();

                if (actionHasDisableLimitRequestsAttribute || controllerHasDisableLimitRequestsAttribute)
                {
                    await _next.Invoke(httpContext);
                    return;
                }
            }

            LimitRequestsDTO dto = _optionsMonitorLimitRequests.CurrentValue;

            StringValues keyStringValues = httpContext.Request.Headers["X-Api-Key"];

            if (keyStringValues.Count == 0)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsync("Debe proveer api key en la cabecera X-Api-Key");
                return;
            }

            if (keyStringValues.Count > 1)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsync("Solo una api key debe estar presente");
                return;
            }

            string key = keyStringValues[0];

            APIKey? keyDb = await context.APIKeys.Include(k => k.DomainRestrictions)
                                                 .Include(k => k.IPRestrictions)
                                                 .Include(k => k.User)
                                                 .FirstOrDefaultAsync(k => k.Key == key);

            if (keyDb is null)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsync("La llave no existe");
                return;
            }

            if (!keyDb.Active)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsync("La llave se encuentra inactiva");
                return;
            }

            bool overcomeRestrictions = RequestExceedsAnyRestriction(keyDb, httpContext);

            if (!overcomeRestrictions)
            {
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            if (keyDb.KeyType == KeyType.Free)
            {
                DateTime today = DateTime.UtcNow.Date;
                int requestsQuentityToday = await context.APIRequests.CountAsync(r => r.APIKeyId == keyDb.Id 
                                                                                   && r.RequestDate >= today);

                if (dto.RequestsPerDayFree <= requestsQuentityToday)

                httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await httpContext.Response.WriteAsync("Ha excedido el minimo de peticiones por dia. Si desea realizar más peticiones actualice su plan a capa profesional");
                return;
            }
            else if (keyDb.User.DefaultingDebtor)
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsync("No ha pagado su factura");
                return;
            }

            IPAddress remoteIpAddres = httpContext.Connection.RemoteIpAddress;

            APIRequest request = new APIRequest
            {
                APIKeyId = keyDb.Id,
                RequestDate = DateTime.UtcNow,
                RemoteIp = remoteIpAddres.ToString(),
            };

            await context.APIRequests.AddAsync(request);
            await context.SaveChangesAsync();

            await _next.Invoke(httpContext);            
        }
    
        private bool RequestExceedsAnyRestriction(APIKey apiKey, HttpContext httpContext)
        {
            bool thereAreRestrictions = apiKey.DomainRestrictions.Any() || apiKey.IPRestrictions.Any();

            if (!thereAreRestrictions)
            {
                return true;
            }

            bool requestExceedsDomainRestriction = RequestExceedsDomainRestriction(apiKey.DomainRestrictions, httpContext);
            bool requestExceedsIpRestriction = RequestExceedsIPRestriction(apiKey.IPRestrictions, httpContext);

            return requestExceedsDomainRestriction || requestExceedsIpRestriction;
        }

        private bool RequestExceedsDomainRestriction(List<DomainRestriction> restrictions, HttpContext httpContext)
        {
            if (restrictions is null || restrictions.Count == 0)
            {
                return false;
            }

            string referer = httpContext.Request.Headers["referer"].ToString();

            if (referer == string.Empty)
            {
                return false;
            }

            Uri myURI = new Uri(referer);
            string domain = myURI.Host;

            bool exceedsRestriction = restrictions.Any(r => r.Domain == domain);

            return exceedsRestriction;
        }

        private bool RequestExceedsIPRestriction(List<IPRestriction> restrictions, HttpContext httpContext)
        {
            if (restrictions is null || restrictions.Count == 0)
            {
                return false;
            }

            IPAddress remoteIpAddres = httpContext.Connection.RemoteIpAddress;

            if (remoteIpAddres is null)
            {
                return false;
            }

            string Ip = remoteIpAddres.ToString();

            if (Ip == string.Empty)
            {
                return false;
            }

            bool exceedsRestriction = restrictions.Any(r => r.IP == Ip);

            return exceedsRestriction;
        }
    }

    public static class LimitRequestsMiddlewareExtensions
    {
        public static IApplicationBuilder UseLimitRequestsMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LimitRequestsMiddleware>();
        }
    }
}
