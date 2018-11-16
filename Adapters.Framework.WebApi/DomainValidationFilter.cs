using Domain.Framework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Adapters.Framework.WebApi
{
    public class DomainValidationFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var domainValidationException = context.Exception as DomainValidationException;
            if (domainValidationException != null)
            {
                var error = new { Description = "Domain Validation Failed", Errors = domainValidationException.DomainErrors };
                var badRequestResult = new BadRequestObjectResult(error);
                context.Result = badRequestResult;
            }
        }
    }
}