using Domain.AuthModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Services.Interfaces
{
    public interface IAccountService
    {
        Task<(bool Success, IEnumerable<IdentityError> Errors)> RegisterUser(RegisterModel model);
        Task<(bool Success, string? Token, IEnumerable<IdentityError> Errors)> LoginUser(LoginModel model);
        Task<(string Email, string Username)> GetUserInfo(ClaimsPrincipal user);
    }
}
