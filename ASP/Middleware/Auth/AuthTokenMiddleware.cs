using ASP.Data;
using ASP.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace ASP.Middleware.Auth
{
    public class AuthTokenMiddleware
    {
        private readonly RequestDelegate _next;
        public AuthTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        
        public async Task InvokeAsync(HttpContext context, DataContext dataContext)
        {
            var authHeader = context
                .Request
                .Headers
                .Authorization
                .FirstOrDefault(h => h?.StartsWith("Bearer ") ?? false);

            if (authHeader is not null)
            {
                var jti = authHeader[7..];
                var accessToken = dataContext.AccessTokens
                    .AsNoTracking()
                    .Include(at => at.UserAccess)
                        .ThenInclude(ua => ua.UserData)
                    .FirstOrDefault(at => at.Jti == jti);

                if (accessToken != null)
                {
                    //Console.WriteLine($"[AuthMiddleware] JTI: {accessToken.Jti}");
                    //Console.WriteLine($"[AuthMiddleware] Exp: {accessToken.Exp}");

                    var expDate = ParseUnixTime(accessToken.Exp);

                    if (expDate.HasValue)
                    {
                        //Console.WriteLine($"[AuthMiddleware] Exp (parsed): {expDate.Value:O}");

                        if (expDate.Value > DateTime.UtcNow)
                        {
                            //Console.WriteLine($"[AuthMiddleware] Token is valid. Access granted.");

                            context.User = new ClaimsPrincipal(
                                new ClaimsIdentity(
                                    new[]
                                    {
                                new Claim(ClaimTypes.Name, accessToken.UserAccess.UserData.Name),
                                new Claim(ClaimTypes.Email, accessToken.UserAccess.UserData.Email),
                                    },
                                    nameof(AuthTokenMiddleware)
                                )
                            );
                        }
                        //else
                        //{
                        //    Console.WriteLine($"[AuthMiddleware] Token expired at {expDate.Value:O}");
                        //}
                    }
                    //else
                    //{
                    //    Console.WriteLine($"[AuthMiddleware] Failed to parse Exp or Exp is out of range.");
                    //}
                }
                //else
                //{
                //    Console.WriteLine($"[AuthMiddleware] No access token found for JTI: {jti}");
                //}
            }

            await _next(context);
        }

        private static DateTime? ParseUnixTime(string? unixTimeStr)
        {

            if (long.TryParse(unixTimeStr, out var timestamp))
            {
                timestamp = timestamp / 1000;
                return DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
            }
            

            return null;
        }



    }
}
