namespace MoviesAPI.Middlewares
{
    public class ForbiddenPathMiddleware
    {
        private readonly RequestDelegate _next;

        public ForbiddenPathMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext.Request.Path == "/forbbiden-path")
            {
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                await httpContext.Response.WriteAsync("Acceso denegado");
            }
            else
            {
                await _next.Invoke(httpContext);
            }
        }
    }

    public static class ForbiddenPathMiddlewareExtensions
    {
        public static IApplicationBuilder UseForbbidenPathMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ForbiddenPathMiddleware>();
        }
    }
}
