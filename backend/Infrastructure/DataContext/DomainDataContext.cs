﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain.Users;
using Domain.Common;

namespace Infrastructure.DataContext
{
    public class DomainDataContext : IdentityDbContext<User>
    {

        public virtual DbSet<Friend> Friends { get; set; }
        public virtual DbSet<Game> Games { get; set; }
        public virtual DbSet<Move> Moves { get; set; }

        public DomainDataContext() { }
        public DomainDataContext(DbContextOptions<DomainDataContext> options) : base(options) { }


    }

}
