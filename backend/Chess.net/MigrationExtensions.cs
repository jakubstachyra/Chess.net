using Infrastructure.DataContext;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Intrinsics.Arm;

namespace Chess.net
{
    public static class MigrationExtensions
    {
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            using DomainDataContext _domainDataContext = scope.ServiceProvider.GetService<DomainDataContext>();
            _domainDataContext.Database.Migrate();
        }
    }
}
