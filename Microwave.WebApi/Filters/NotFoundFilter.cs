using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microwave.Domain.Exceptions;

namespace Microwave.WebApi.Filters
{
    public class NotFoundFilter : IExceptionFilter
    {
     
        public void OnException(ExceptionContext context)
        {
            if (!(context.Exception is NotFoundException notFoundException)) return;

            var error = new ProblemDocument("Entity not found", notFoundException.Message);
            var badRequestResult = new NotFoundObjectResult(error);
            context.Result = badRequestResult;
        }
    }
}