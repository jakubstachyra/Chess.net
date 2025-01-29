using Infrastructure.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Infrastructure
{
    public class DataContextFactory : IDesignTimeDbContextFactory<DomainDataContext>
    {
        public DomainDataContext CreateDbContext(string[] args)
        {
            //var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../Chess.net");
            //var configuration = new ConfigurationBuilder()
            //    .SetBasePath(basePath)
            //    .AddJsonFile("appsettings.json")
            //    .Build();


            //var connectionString = configuration.GetConnectionString("DefaultConnection");

            var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");
            Console.WriteLine($"Connection String z factory: {connectionString}");

            var optionsBuilder = new DbContextOptionsBuilder<DomainDataContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new DomainDataContext(optionsBuilder.Options);
        }
    }
}
