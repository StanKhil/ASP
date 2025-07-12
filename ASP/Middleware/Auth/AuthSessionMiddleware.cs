using ASP.Data.Entities;
using System.Globalization;
using System.Text.Json;

namespace ASP.Middleware.Auth
{
    public class AuthSessionMiddleware
    {
        private readonly RequestDelegate _next;
        public AuthSessionMiddleware(RequestDelegate next)
        {
            _next = next; 
        }
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Session.Keys.Contains("userAccess"))
            {
                context.Items["userAccess"] =
                    JsonSerializer.Deserialize<UserAccess>(
                        context.Session.GetString("userAccess")!);
            }
            await _next(context);
        }
    }
}
