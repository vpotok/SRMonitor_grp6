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
                // Validate the token via SRMAuth
                var isValid = await authService.ValidateToken(token);
                if (isValid)
                {
                    // Optionally, retrieve user information from the token
                    var username = await authService.GetUserFromToken(token);
                    context.Items["User"] = username;
                }
            }

            await _next(context);
        }
    }
}