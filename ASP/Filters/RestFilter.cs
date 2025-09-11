using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using ASP.Models.Rest;
using System.Reflection;

namespace ASP.Filters
{
    public class RestFilter(String? Name = null) : ActionFilterAttribute
    {
        private readonly String? _Name = Name;
        override public void OnActionExecuting(ActionExecutingContext context)
        {
            RestResponse response = new();
            response.Meta.Method = context.HttpContext.Request.Method;
            if (_Name != null)
                response.Meta.ResourceName = _Name;
            var field = context.Controller.GetType()
                .GetField("response", BindingFlags.NonPublic | BindingFlags.Instance);

            field?.SetValue(context.Controller, response);
            
        }

        override public  void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
        }
    }
}
