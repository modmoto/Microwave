using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microwave.Application.Exceptions;

namespace Adapters.Framework.WebApi
{
    public class ConcurrencyViolatedFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var concurrencyViolatedException = context.Exception as ConcurrencyViolatedException;
            if (concurrencyViolatedException != null)
            {
                var error = new { Description = "ConcurrencyViolation", Detail = concurrencyViolatedException.Message };
                var conflictResut = new OkObjectResult(error);
                conflictResut.StatusCode = 409;
                context.Result = conflictResut;
            }
        }
    }
}