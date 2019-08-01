using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microwave.WebApi.ApiFormatting.DateTimeOffsets
{

    internal class DateTimeOffsetBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(System.DateTimeOffset))
                return new DateTimeOffsetBinder();

            return null;
        }
    }

    internal class DateTimeOffsetBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var values = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (values.Length > 1)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            var timeString = values.FirstOrDefault();

            if (timeString == null) return Task.CompletedTask;

            if (timeString.Contains(" ")) timeString = timeString.Replace(" ", "+");

            if (System.DateTimeOffset.TryParseExact(timeString, "o", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var result))
            {
                bindingContext.Result = ModelBindingResult.Success(result);
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}