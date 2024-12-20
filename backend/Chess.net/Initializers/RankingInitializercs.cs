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
                new Ranking { Name = "1min", Description = "One minute bullet game." },
                new Ranking { Name = "3min", Description = "Three minutes bullet game." },
                new Ranking { Name = "5min", Description = "Five minutes blitz game." },
                new Ranking { Name = "10min", Description = "Ten minutes blitz game." },
                new Ranking { Name = "15min", Description = "Fithteen minutes rapid game." },
            };

            context.Rankings.AddRange(rankings);
            context.SaveChanges();
        }
    }
}

