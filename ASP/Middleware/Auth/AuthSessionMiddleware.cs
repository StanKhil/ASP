using ASP.Data.Entities;
using System.Globalization;
using System.Text.Json;
using System.Security.Claims;
using ASP.Middleware.Auth;
using Microsoft.Extensions.Logging;

namespace ASP.Middleware.Auth
{
    public class AuthSessionMiddleware
    {
        private readonly RequestDelegate _next;
        public AuthSessionMiddleware(RequestDelegate next)
        {
            _next = next; 
        }
        public async Task InvokeAsync(HttpContext context, ILogger<AuthSessionMiddleware> logger)
        {
            if (context.Request.Query.ContainsKey("logout"))
            {
                context.Session.Remove("userAccess");
                context.Response.Redirect(context.Request.Path);
                return;
            }
            else if (context.Session.Keys.Contains("userAccess"))
            {
                var ua = JsonSerializer
                    .Deserialize<UserAccess>(
                        context.Session.GetString("userAccess")!);
                //logger.LogInformation("login: {login}", ua.Login);
                //context.Items["userAccess"] = ua;
                context.User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new Claim[]
                        {
                            new (ClaimTypes.Name, ua!.UserData.Name),
                            new (ClaimTypes.Email, ua!.UserData.Email),
                            new (ClaimTypes.Sid, ua.Login),
                        },
                        nameof(AuthSessionMiddleware)
                    )
                );
            }
            await _next(context);
        }
    }

    public static class AuthTokenMiddlewareExtension
    {
        public static IApplicationBuilder UseAuthToken(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthTokenMiddleware>();
        }

    }
}


