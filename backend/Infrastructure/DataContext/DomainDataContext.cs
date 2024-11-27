﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain.Users;
using Domain.Common;

namespace Infrastructure.DataContext
{
    public class DomainDataContext : IdentityDbContext<User>
    {

        public DbSet<Friend> Friends { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Move> Moves { get; set; }


        public DomainDataContext(DbContextOptions<DomainDataContext> options) : base(options) { }
    }

}
