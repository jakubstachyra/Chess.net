using Domain.Common;
using Humanizer.Localisation;
using Infrastructure.DataContext; 
using Microsoft.EntityFrameworkCore;
using System;

public static class RankingInitializer
{
    public static void Initialize(DomainDataContext context)
    {
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate(); 
        }

        if (!context.Rankings.Any())
        {
            var rankings = new[]
            {
                new Ranking { Name = "Bullet", Description = "1 - 3 minute games." },
                new Ranking { Name = "Blitz", Description = "3 - 10 minutes games." },
                new Ranking { Name = "Rapid", Description = "10 - 30 minutes games." },
            };

            context.Rankings.AddRange(rankings);
            context.SaveChanges();
        }
    }
}

