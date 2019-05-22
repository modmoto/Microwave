using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microwave.Domain.Validation;

namespace Microwave.WebApi.Filters
{
    public class DomainValidationFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is DomainValidationException domainValidationException)
            {
                var error = new ProblemDocument("DomainError", "Domain Validation Failed", domainValidationException.DomainErrors);
                var badRequestResult = new BadRequestObjectResult(error);
                context.Result = badRequestResult;
            }
        }
    }
}