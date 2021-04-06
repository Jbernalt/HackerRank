using HackerRank.Models.Transactions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HackerRank.Models.Groups;
using HackerRank.Models.Achievements;
using HackerRank.Models.Users;
using HackerRank.Models.Projects;

namespace HackerRank.Data
{
    public class HackerRankContext : IdentityDbContext<User>
    {
        public HackerRankContext(DbContextOptions<HackerRankContext> options)
            : base(options)
        {

        }

        public DbSet<Achievement> Achievement { get; set; }
        public DbSet<UserAchievement> UserAchievement { get; set; }
        public DbSet<Group> Group { get; set; }
        public DbSet<UserTransaction> UserTransaction { get; set; }
        public DbSet<Transaction> Transaction { get; set; }
        public DbSet<UserStats> UserStats { get; set; }
        public DbSet<Project> Project { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Project>().Property(p => p.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<UserTransaction>().HasKey(t => new { t.UserId, t.TransactionId, t.FetchDate });
            modelBuilder.Entity<Transaction>().HasData(
                new Transaction() { TransactionId = 1, Description = "Commits", Points = 0.15 },
                new Transaction() { TransactionId = 2, Description = "Issues opened", Points = 0.15 },
                new Transaction() { TransactionId = 3, Description = "Issues solved", Points = 0.3 },
                new Transaction() { TransactionId = 4, Description = "Merge requests", Points = 0.35 },
                new Transaction() { TransactionId = 5, Description = "Comments", Points = 0.05 });
            modelBuilder.Entity<User>().HasOne(t => t.UserStats).WithOne(x => x.User).HasForeignKey<UserStats>(k => k.UserStatsId);
        }
    }
}
