using Domain.AuthModels;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Interfaces
{
    public interface IAccountService
    {
        Task<(bool Success, IEnumerable<IdentityError> Errors)> RegisterUser(RegisterModel model);
        Task<(bool Success, string? Token, IEnumerable<IdentityError> Errors)> LoginUser(LoginModel model);
        Task<(string Email, string Username, string UserID, bool IsAdmin)> GetUserInfo(ClaimsPrincipal user);
    }
}
