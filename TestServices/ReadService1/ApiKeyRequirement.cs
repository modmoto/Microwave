using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ReadService1
{
    public class ApiKeyRequirement : AuthorizationHandler<ApiKeyRequirement>, IAuthorizationRequirement
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiKeyRequirement requirement)
        {
            if (context.Resource is AuthorizationFilterContext authorizationHandlerContext)
            {
                var headerDictionary = authorizationHandlerContext.HttpContext.Request.Headers;
                var apiKey = headerDictionary["Authorization"].ToString();
                if (apiKey == "123")
                {
                    context.Succeed(requirement);
                }
                else
                {
                    authorizationHandlerContext.Result = new UnauthorizedResult();
                    context.Succeed(requirement);
                }
                return Task.CompletedTask;
            }

            context.Fail();
            return Task.CompletedTask;
        }
    }
}