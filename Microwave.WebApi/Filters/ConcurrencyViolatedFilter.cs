using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microwave.Application.Exceptions;

namespace Microwave.WebApi.Filters
{
    public class ConcurrencyViolatedFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is ConcurrencyViolatedException concurrencyViolatedException)
            {
                var error = new ProblemDocument("ConcurrencyViolation", concurrencyViolatedException.Message);
                var conflictResut = new OkObjectResult(error);
                conflictResut.StatusCode = 409;
                context.Result = conflictResut;
            }
        }
    }
}