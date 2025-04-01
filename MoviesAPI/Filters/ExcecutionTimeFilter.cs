using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace MoviesAPI.Filters
{
    public class ExcecutionTimeFilter : IAsyncActionFilter
    {
        private readonly ILogger<ExcecutionTimeFilter> _logger;

        public ExcecutionTimeFilter(ILogger<ExcecutionTimeFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Antes ejecución
            Stopwatch stopWatch = Stopwatch.StartNew();
            _logger.LogInformation($"INICIO Acción: {context.ActionDescriptor.DisplayName}");

            await next();

            // Despues ejecución
            stopWatch.Stop();
            _logger.LogInformation($"FIN Acción: {context.ActionDescriptor.DisplayName} - Tiempo: {stopWatch.ElapsedMilliseconds} ms");
        }
    }
}
