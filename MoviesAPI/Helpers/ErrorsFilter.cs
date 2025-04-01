using Microsoft.AspNetCore.Mvc.Filters;

namespace MoviesAPI.Helpers
{
    public class ErrorsFilter : ExceptionFilterAttribute
    {
        private readonly ILogger<ErrorsFilter> _logger;

        public ErrorsFilter(ILogger<ErrorsFilter> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, context.Exception.Message);

            base.OnException(context);
        }
    }
}
