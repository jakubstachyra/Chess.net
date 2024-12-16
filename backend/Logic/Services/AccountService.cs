using Domain.AuthModels;
using Domain.Users;
using Logic.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Logic.Services
{
    public class AccountService(UserManager<User> userManager,
        SignInManager<User> signInManager, IConfiguration configuration): IAccountService
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly SignInManager<User> _signInManager = signInManager;
        private readonly IConfiguration _configuration = configuration;

        public async Task<(bool Success, IEnumerable<IdentityError> Errors)> RegisterUser(RegisterModel model)
        {
            var user = new User { UserName = model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return (false, result.Errors);
            }

            await _userManager.AddToRoleAsync(user, "USER");

            return (true, Enumerable.Empty<IdentityError>());
        }
        public async Task<(bool Success, string? Token, IEnumerable<IdentityError> Errors)> LoginUser(LoginModel model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                var errors = new List<IdentityError>
                {
                    new IdentityError { Code = "UserNotFound", Description = "User does not exist." }
                };
                
                return (false, null!, errors);
            }

            var result = await  _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if(!result.Succeeded)
            {
                var errors = new List<IdentityError>
                {
                    new IdentityError { Code = "InvalidPassword", Description = "The password is incorrect" }
                };

                return (false, null!, errors);
            }

            var token = await GenerateJwtToken(user);

            return (true, token, Enumerable.Empty<IdentityError>());   
        }
        public Task<(string Email, string Username)> GetUserInfo(ClaimsPrincipal user)
        {
            var email = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var username = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
           
            return Task.FromResult((email!, username!));
        }

        private async Task<string> GenerateJwtToken(User user)
        {
            // Pobierz role przypisane do użytkownika
            var roles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email!), 
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), 
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };


            authClaims.AddRange(roles.Select(role => new Claim("roles", role)));


            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddHours(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
