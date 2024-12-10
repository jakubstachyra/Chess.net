using Domain.AuthModels;
using Domain.Users;
using Logic.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Logic.Services
{
    public class AccountService(UserManager<User> userManager,
        SignInManager<User> signInManager, IConfiguration configuration): IAccountService
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly SignInManager<User> _signInManager = signInManager;
        private readonly IConfiguration _configuration = configuration;

        public async Task<(bool, IEnumerable<string>)> RegisterUser(RegisterModel model)
        {
            var user = new User { UserName = model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return (false, result.Errors.Select(x => x.Description));
            }

            await _userManager.AddToRoleAsync(user, "USER");

            return (true, Enumerable.Empty<string>());
        }
    }
}
