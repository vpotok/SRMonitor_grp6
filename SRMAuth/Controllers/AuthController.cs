using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SRMAuth.Models;
using SRMAuth.Services;

namespace SRMAuth.Controllers;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly TokenService _tokenService;
    private readonly IConfiguration _config;

    public AuthController(TokenService tokenService, IConfiguration config)
    {
        _tokenService = tokenService;
        _config = config;
    }
}
