using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace MoviesAPI.Helpers
{
    public class CustomTypeBinder<T> : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            string propertyName = bindingContext.ModelName;
            ValueProviderResult valuesProvider = bindingContext.ValueProvider.GetValue(propertyName);

            if (valuesProvider == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            try
            {
                T unserializedValue = JsonConvert.DeserializeObject<T>(valuesProvider.FirstValue);
                bindingContext.Result = ModelBindingResult.Success(unserializedValue);
            }
            catch (Exception ex)
            {
                bindingContext.ModelState.TryAddModelError(propertyName, "Valor inválido para tipo T");
            }

            return Task.CompletedTask;
        }
    }
}
