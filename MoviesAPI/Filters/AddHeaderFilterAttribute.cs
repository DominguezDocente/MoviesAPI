using Microsoft.AspNetCore.Mvc.Filters;

namespace MoviesAPI.Filters
{
    public class AddHeaderFilterAttribute : ActionFilterAttribute
    {
        private readonly string _name;
        private readonly string _value;

        public AddHeaderFilterAttribute(string name, string value)
        {
            _name = name;
            _value = value;
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            // Antes ejecución acción
            context.HttpContext.Response.Headers.Append(_name, _value);

            base.OnResultExecuting(context);
            
            // Despues de ejecución
        }
    }
}
