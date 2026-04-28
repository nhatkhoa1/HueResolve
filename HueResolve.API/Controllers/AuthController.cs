using HueResolve.Business.Interfaces;
using HueResolve.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace HueResolve.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);

        if (result == null)
            return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu" });

        return Ok(result);
    }
    [HttpGet("hash/{password}")]
    public IActionResult GetHash(string password)
    {
        return Ok(BCrypt.Net.BCrypt.HashPassword(password));
    }
}