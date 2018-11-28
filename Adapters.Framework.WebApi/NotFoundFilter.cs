using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microwave.Application.Exceptions;

namespace Adapters.Framework.WebApi
{
    public class NotFoundFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var notFoundException = context.Exception as NotFoundException;
            if (notFoundException != null)
            {
                var error = new { Description = "Entity not found", Detail = notFoundException.Message };
                var badRequestResult = new NotFoundObjectResult(error);
                context.Result = badRequestResult;
            }
        }
    }
}