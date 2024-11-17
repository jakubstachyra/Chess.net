using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain.User;

namespace Infrastructure.DataContext
{
    public class DomainDataContext : IdentityDbContext<User>
    {
        public DomainDataContext(DbContextOptions<DomainDataContext> options) : base(options) { }
    }

}
