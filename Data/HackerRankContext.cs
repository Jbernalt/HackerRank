using System;
using System.Collections.Generic;
using System.Text;

using HackerRank.Models;
using HackerRank.Models.Achivements;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HackerRank.Data
{
    public class HackerRankContext : IdentityDbContext
    {
        public HackerRankContext(DbContextOptions<HackerRankContext> options)
            : base(options)
        {
        }

        DbSet<Achivement> Achivement { get; set; }
        DbSet<UserAchivement> UserAchivement { get; set; }
    }
}
