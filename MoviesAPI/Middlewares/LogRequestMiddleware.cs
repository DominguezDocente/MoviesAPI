namespace MoviesAPI.Middlewares
{
    //public class LogRequestMiddleware : IMiddleware
    public class LogRequestMiddleware
    {
        private readonly RequestDelegate _next;

        public LogRequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            // Viene petición
            ILogger<StartUp> logger = httpContext.RequestServices.GetRequiredService<ILogger<StartUp>>();

            logger.LogInformation($"Petición: {httpContext.Request.Method} {httpContext.Request.Path}");

            await _next.Invoke(httpContext);

            // Se va la respuesta
            logger.LogInformation($"Respuesta: {httpContext.Response.StatusCode}");
        }

        //public Task InvokeAsync(HttpContext context, RequestDelegate next)
        //{
        //    throw new NotImplementedException();
        //}
    }

    public static class LogRequestMiddlewareExtensions
    {
        public static IApplicationBuilder UseLogRequestMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LogRequestMiddleware>();
        }
    }
}
