using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microwave.Domain.Exceptions;

namespace Microwave.WebApi.Filters
{
    internal class ConcurrencyViolatedFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (!(context.Exception is ConcurrencyViolatedException concurrencyViolatedException)) return;

            var error = new ProblemDocument(
                "ConcurrencyViolation",
                "Concurrency was violated, can not update entity",
                HttpStatusCode.Conflict,
                concurrencyViolatedException.Message);
            var conflictResult = new OkObjectResult(error);
            conflictResult.StatusCode = 409;
            context.Result = conflictResult;
        }
    }
}