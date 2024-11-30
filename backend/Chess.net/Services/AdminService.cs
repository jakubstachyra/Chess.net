using Chess.net.Services.Interfaces;
using Domain.Users;
using Infrastructure.DataContext;

namespace Chess.net.Services
{
    public class AdminService(DomainDataContext context)
    {
        private readonly DomainDataContext _repository = context;
        
    }
}
