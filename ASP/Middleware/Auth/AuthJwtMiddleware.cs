using ASP.Services.JWT;
using System.Security.Claims;
using System.Text.Json;

namespace ASP.Middleware.Auth
{
    public class AuthJwtMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthJwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context,
            IJwtService jwtService,
            ILogger<AuthJwtMiddleware> logger)
        {
            logger.LogInformation("Authorization header: " + context.Request.Headers.Authorization.FirstOrDefault());
            if (context
                .Request
                .Headers
                .Authorization
                .FirstOrDefault(h => h?.StartsWith("Bearer ") ?? false)
                is String authHeader)
            {
                String jwt = authHeader[7..];
                try
                {
                    var (header, payload) = jwtService.DecodeJwt(jwt);
                    var payloadElement = (JsonElement)payload;
                    context.User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new Claim[]
                            {
                                new(ClaimTypes.Name, payloadElement.GetProperty("Name").GetString()!),
                                new(ClaimTypes.Email, payloadElement.GetProperty("Email").GetString()!)
                            },
                            nameof(AuthTokenMiddleware)
                        )
                    );
                }
                catch (Exception ex)
                {
                    logger.LogError("JWT decode error {ex}", ex.Message);
                }
            }
            await _next(context);
        }
    }

    public static class AuthJwtMiddlewareExtension
    {
        public static IApplicationBuilder UseAuthJwt(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthJwtMiddleware>();
        }
    }
}

