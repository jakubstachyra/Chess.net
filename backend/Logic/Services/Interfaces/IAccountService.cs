using Domain.AuthModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Services.Interfaces
{
    public interface IAccountService
    {
        Task<(bool Success, IEnumerable<string> Errors)> RegisterUser(RegisterModel model);
    }
}
