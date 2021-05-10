using HackerRank.Models.Transactions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HackerRank.Models.Groups;
using HackerRank.Models.Achievements;
using HackerRank.Models.Users;
using HackerRank.Models.Projects;
using HackerRank.Models;

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
        public DbSet<Level> Levels { get; set; }
        public DbSet<UserLevel> UserLevels { get; set; }
        public DbSet<GroupStats> GroupStats { get; set; }

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
            modelBuilder.Entity<Group>().HasOne(t => t.GroupStats).WithOne(x => x.Group).HasForeignKey<GroupStats>(k => k.GroupStatsId);

            modelBuilder.Entity<Level>().HasData(
                new Level() { LevelId = 1, LevelName = "Newbie", XpNeeded = 0 },
                new Level() { LevelId = 2, LevelName = "Apprentice", XpNeeded = 10 },
                new Level() { LevelId = 3, LevelName = "Novice", XpNeeded = 20 },
                new Level() { LevelId = 4, LevelName = "Bronze", XpNeeded = 30 },
                new Level() { LevelId = 5, LevelName = "Herald", XpNeeded = 40 },
                new Level() { LevelId = 6, LevelName = "Combatant", XpNeeded = 50 },
                new Level() { LevelId = 7, LevelName = "Pippin", XpNeeded = 60 },
                new Level() { LevelId = 8, LevelName = "Silver", XpNeeded = 70 },
                new Level() { LevelId = 9, LevelName = "Guardian", XpNeeded = 80 },
                new Level() { LevelId = 10, LevelName = "Frodo", XpNeeded = 90 },
                new Level() { LevelId = 11, LevelName = "Challenger", XpNeeded = 100 },
                new Level() { LevelId = 12, LevelName = "Gold", XpNeeded = 110 },
                new Level() { LevelId = 13, LevelName = "Crusader", XpNeeded = 120 },
                new Level() { LevelId = 14, LevelName = "DoubleAk", XpNeeded = 130 },
                new Level() { LevelId = 15, LevelName = "Rival", XpNeeded = 140 },
                new Level() { LevelId = 16, LevelName = "Platinum", XpNeeded = 150 },
                new Level() { LevelId = 17, LevelName = "Archon", XpNeeded = 160 },
                new Level() { LevelId = 18, LevelName = "LegendaryEagle", XpNeeded = 170 },
                new Level() { LevelId = 19, LevelName = "Gandalf", XpNeeded = 180 },
                new Level() { LevelId = 20, LevelName = "Duelist", XpNeeded = 190 },
                new Level() { LevelId = 21, LevelName = "Diamond", XpNeeded = 200 },
                new Level() { LevelId = 22, LevelName = "Legend", XpNeeded = 210 },
                new Level() { LevelId = 23, LevelName = "GlobalElite", XpNeeded = 220 },
                new Level() { LevelId = 24, LevelName = "Gladiator", XpNeeded = 230 },
                new Level() { LevelId = 25, LevelName = "Mythic", XpNeeded = 240 },
                new Level() { LevelId = 26, LevelName = "Ancient", XpNeeded = 250 },
                new Level() { LevelId = 27, LevelName = "Samwise", XpNeeded = 260 },
                new Level() { LevelId = 28, LevelName = "Planeswalker", XpNeeded = 270 },
                new Level() { LevelId = 29, LevelName = "Divine", XpNeeded = 280 },
                new Level() { LevelId = 30, LevelName = "AlmostGaben", XpNeeded = 290 },
                new Level() { LevelId = 31, LevelName = "Gaben", XpNeeded = 300 });
        }
    }
}
