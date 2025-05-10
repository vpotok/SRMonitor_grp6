using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using SRMCore.Services;

namespace SRMCore.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, AuthService authService)
        {
            var token = context.Request.Cookies["auth_token"];
            if (!string.IsNullOrEmpty(token))
            {
                var isValid = await authService.ValidateToken(token);
                if (!isValid)
                {
                    var refreshToken = context.Request.Cookies["refresh_token"];
                    if (!string.IsNullOrEmpty(refreshToken))
                    {
                        var newTokens = await authService.RotateRefreshToken(refreshToken);
                        if (newTokens != null)
                        {
                            context.Response.Cookies.Append("auth_token", newTokens.Value.NewAccessToken, new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Path = "/",
                                Expires = DateTime.UtcNow.AddMinutes(15)
                            });

                            context.Response.Cookies.Append("refresh_token", newTokens.Value.NewRefreshToken, new CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = SameSiteMode.Strict,
                                Path = "/",
                                Expires = DateTime.UtcNow.AddDays(7)
                            });

                            context.Items["IsTokenValid"] = true;
                            await _next(context);
                            return;
                        }
                    }
                }

                context.Items["IsTokenValid"] = isValid;
            }
            else
            {
                context.Items["IsTokenValid"] = false;
            }

            await _next(context);
        }
    }
}