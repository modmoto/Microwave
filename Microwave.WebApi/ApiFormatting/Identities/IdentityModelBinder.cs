using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microwave.Domain.Identities;

namespace Microwave.WebApi.ApiFormatting.Identities
{

    public class IdentityModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(Identity) || context.Metadata.ModelType == typeof(StringIdentity) || context.Metadata.ModelType == typeof(GuidIdentity))
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

            var bindingContextModel = Identity.Create(values.SingleOrDefault());

            if (bindingContext.ModelType == typeof(Identity))
            {
                bindingContext.Result = ModelBindingResult.Success(bindingContextModel);
            }
            else if (bindingContext.ModelType == typeof(StringIdentity))
            {
                bindingContext.Result = ModelBindingResult.Success(StringIdentity.Create(bindingContextModel.Id));
            }
            else if (bindingContext.ModelType == typeof(GuidIdentity) && bindingContextModel is GuidIdentity)
            {
                bindingContext.Result = ModelBindingResult.Success(bindingContextModel);
            }
            else
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }

            return Task.CompletedTask;
        }
    }
}