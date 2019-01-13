using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microwave.Domain;

namespace Microwave.WebApi.ApiFormatting.Identity
{

    public class IdentityModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(Domain.Identity) || context.Metadata.ModelType == typeof(StringIdentity) || context.Metadata.ModelType == typeof(GuidIdentity))
                return new IdentityModelBinder();

            return null;
        }
    }

    public class IdentityModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var values = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (values.Length > 1)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            var bindingContextModel = Domain.Identity.Create(values.SingleOrDefault());
            bindingContext.Result = ModelBindingResult.Success(bindingContextModel);

            return Task.CompletedTask;
        }
    }
}