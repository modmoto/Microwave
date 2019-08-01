using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microwave.Domain.Validation;

namespace Microwave.WebApi.Filters
{
    internal class DomainValidationFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (!(context.Exception is DomainValidationException domainValidationException)) return;

            var error = new ProblemDocument(
                "DomainValidationFailed",
                "A Domain Validation failed, see details",
                domainValidationException.DomainErrors);
            var badRequestResult = new BadRequestObjectResult(error);
            context.Result = badRequestResult;
        }
    }
}