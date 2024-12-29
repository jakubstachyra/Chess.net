using Domain.AuthModels;
using Domain.Users;
using Logic.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

[Route("/[controller]")]
[ApiController]
public class AccountController(IAccountService accountService) : ControllerBase
{
    private readonly IAccountService _accountService = accountService;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (succes, errors) = await _accountService.RegisterUser(model);

        if(succes)
        {
            return Ok(new { Message = "User registered successfully" });
        }

        return BadRequest(errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _accountService.LoginUser(model);
            
        if(!result.Success)
        {
            return Unauthorized(result.Errors);
        }

        Response.Cookies.Append("authToken", result.Token!, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.None,
            Secure = true,
            Expires = DateTimeOffset.UtcNow.AddHours(1)
        }) ;

        return Ok(new LoginResponse { Token = result.Token! });

    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("authToken", new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.None,
            Secure = true,
            Expires = DateTimeOffset.UtcNow.AddHours(1)
        });

        return Ok(new LogOutResponse { Message = "Logged out successfully." });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetUserInfo()
    {
        var (email, username, userID, isAdmin) = await _accountService.GetUserInfo(User);
        return Ok(new MeResponse
        {
            Email = email,
            Username = username,
            UserID = userID,
            IsAdmin = isAdmin // Dodane pole IsAdmin
        });
    }


    [HttpGet("check-auth")]
    [Authorize]
    public IActionResult CheckAuth()
    {
        return Ok(new { message = "User is logged in" });
    }
    public class LoginResponse
    {
        public required string Token { get; set; }
    }
    public class LogOutResponse
    {
        public required string Message { get; set; }
    }
    public class MeResponse
    {
        public required string Email { get; set; }
        public required string Username { get; set; }
        public required string UserID { get; set; }
        public required bool IsAdmin { get; set; }  
    }
}
