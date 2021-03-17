using System;
using System.Collections.Generic;
using System.Text;
using HackerRank.Models.Groups;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HackerRank.Models.Transactions;

namespace HackerRank.Data
{
    public class HackerRankContext : IdentityDbContext
    {
        public HackerRankContext(DbContextOptions<HackerRankContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<GroupTransaction>().HasKey(t => new { t.GroupId, t.TransactionId, t.FetchDate });
            modelBuilder.Entity<Group>().HasData(
                new Transaction() { TransactionId = 1, Description = "Commits", Points = 0.15 },
                new Transaction() { TransactionId = 2, Description = "Issues opened", Points = 0.15 },
                new Transaction() { TransactionId = 3, Description = "Issues solved", Points = 0.3 },
                new Transaction() { TransactionId = 4, Description = "Merge requests", Points = 0.35 },
                new Transaction() { TransactionId = 5, Description = "Comments", Points = 0.05 });
        }
    }
}
