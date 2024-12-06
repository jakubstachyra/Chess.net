using Domain.AuthModels;
using Domain.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[Route("/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IConfiguration _configuration;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = new User { UserName = model.Username, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            // Przypisanie domyślnej roli "User"
            await _userManager.AddToRoleAsync(user, "User");
            return Ok(new { Message = "User registered successfully." });
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return Unauthorized(new { Message = "Invalid email or password." });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        if (!result.Succeeded)
        {
            return Unauthorized(new { Message = "Invalid email or password." });
        }

        var token = await GenerateJwtToken(user);

        Response.Cookies.Append("authToken", token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.None,
            Secure = true,
            Expires = DateTimeOffset.UtcNow.AddHours(1)
        }) ;
        return Ok(new { Token = token });
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

        return Ok(new { Message = "Logged out successfully." });
    }
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetUserInfo()
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value; 
        return Ok(new { email, username });
    }
    [HttpGet("check-auth")]
    [Authorize]
    public IActionResult CheckAuth()
    {
        return Ok(new { message = "User is logged in" });
    }


    private async Task<string> GenerateJwtToken(User user)
    {
        // Pobierz role przypisane do użytkownika
        var roles = await _userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
    {
        new Claim(ClaimTypes.Email, user.Email), // E-mail użytkownika jako nazwa
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Id użytkownika jako subject
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unikalny ID tokena
    };

        // Dodaj role do listy claimów z nazwą "roles"
        authClaims.AddRange(roles.Select(role => new Claim("roles", role)));

        // Klucz podpisujący z konfiguracji
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            expires: DateTime.Now.AddHours(1), 
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        // Zwróć wygenerowany token w formie ciągu znaków
        return new JwtSecurityTokenHandler().WriteToken(token);
    }



}
