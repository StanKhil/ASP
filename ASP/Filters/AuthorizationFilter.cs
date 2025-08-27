using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ASP.Filters
{
    public class AuthorizationFilter : ActionFilterAttribute
    {
        override public void OnActionExecuting(ActionExecutingContext context)
        {
            Console.WriteLine("User is authenticated");
            if (context.HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                
                base.OnActionExecuting(context);
            }
            else
            {
                context.Result = new JsonResult(new
                {
                    status = "401",
                    message = "Unauthorized"
                });
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Do something after the action executes.
        }
    }
}
