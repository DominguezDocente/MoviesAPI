using Microsoft.AspNetCore.Mvc.Filters;

namespace MoviesAPI.Filters
{
    public class MyActionFilter : IActionFilter
    {
        private readonly ILogger<MyActionFilter> _logger;

        public MyActionFilter(ILogger<MyActionFilter> logger)
        {
            _logger = logger;
        }

        // Antes de acción
        public void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.LogCritical("Ejecutando filtro...");
        }

        // Despues de acción
        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogCritical("Filtro ejecutado");
        }
    }
}
