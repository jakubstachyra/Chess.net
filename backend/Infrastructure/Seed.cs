using Domain.Common;
using Domain.Users;
using Infrastructure.DataRepositories;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class Seed
    {

        public static async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var dataRepository = scope.ServiceProvider.GetRequiredService<IDataRepository>();

            await SeedGameModesAsync(dataRepository);
            await SeedComputerPlayerAsync(dataRepository,userManager);

        }

        public static async Task SeedGameModesAsync(IDataRepository dataRepository)
        {
            var gameModes = new List<GameMode>
    {
        new GameMode { Description = "Standard" },
                new GameMode { Description = "Bullet" },

        new GameMode { Description = "Blitz" },
        new GameMode { Description = "Rapid" },
        new GameMode { Description = "Chess960" },
                new GameMode { Description = "Brain-hand" },
        new GameMode { Description = "The king is dead, long live the king!" },
                new GameMode { Description = "Computer" }


    };
            using var transaction = await dataRepository.BeginTransactionAsync();
            try
            {
                var existingMode = dataRepository.GameModeRepository.GetAllAsync().Result;
                if (existingMode.Count() ==0)
                {
                    foreach (var mode in gameModes)
                    {
                            await dataRepository.GameModeRepository.AddAsync(mode);
                        
                    }
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine(ex.Message);
            }
        }

        public static async Task SeedComputerPlayerAsync(IDataRepository dataRepository, UserManager<User> _userManager)
        {
            var User = new User { UserName = "Computer" };
            using var transaction = await dataRepository.BeginTransactionAsync();
            try
            {

                
                var computerPlayer = await _userManager.FindByNameAsync("Computer");
                if (computerPlayer == null)
                {
                    var computerUser = new User { UserName = "Computer", Email = "computer@chess.com" };
                    var computerResult = await _userManager.CreateAsync(computerUser, "SecurePassword123!");

                    if (computerResult.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(computerUser, "COMPUTER");

                    }
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine(ex.Message);
            }
        }

    }
}
