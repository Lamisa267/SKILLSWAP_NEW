using Microsoft.EntityFrameworkCore;
using SkillSwap.Models;

namespace SkillSwap.Data
{
    public class SkillSwapDbContext : DbContext
    {
        public SkillSwapDbContext(DbContextOptions<SkillSwapDbContext> options)
            : base(options)
        {
        }

        // =========================
        // Database Tables
        // =========================

        public DbSet<User> Users { get; set; }

        public DbSet<Skill> Skills { get; set; }

        public DbSet<UserSkill> UserSkills { get; set; }

        public DbSet<SwapRequest> SwapRequests { get; set; }

        // Members table
        public DbSet<Member> Members { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // =========================
            // User → UserSkill
            // =========================

            modelBuilder.Entity<UserSkill>()
                .HasOne(us => us.User)
                .WithMany(u => u.UserSkills)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // =========================
            // Skill → UserSkill
            // =========================

            modelBuilder.Entity<UserSkill>()
                .HasOne(us => us.Skill)
                .WithMany(s => s.UserSkills)
                .HasForeignKey(us => us.SkillId)
                .OnDelete(DeleteBehavior.Cascade);


            // =========================
            // SwapRequest → Sender
            // =========================

            modelBuilder.Entity<SwapRequest>()
                .HasOne(sr => sr.Sender)
                .WithMany(u => u.SentSwapRequests)
                .HasForeignKey(sr => sr.SenderId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================
            // SwapRequest → Receiver
            // =========================

            modelBuilder.Entity<SwapRequest>()
                .HasOne(sr => sr.Receiver)
                .WithMany(u => u.ReceivedSwapRequests)
                .HasForeignKey(sr => sr.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================
            // SwapRequest → Offered Skill
            // =========================

            modelBuilder.Entity<SwapRequest>()
                .HasOne(sr => sr.OfferedSkill)
                .WithMany()
                .HasForeignKey(sr => sr.OfferedSkillId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================
            // SwapRequest → Requested Skill
            // =========================

            modelBuilder.Entity<SwapRequest>()
                .HasOne(sr => sr.RequestedSkill)
                .WithMany()
                .HasForeignKey(sr => sr.RequestedSkillId)
                .OnDelete(DeleteBehavior.Restrict);


            // =========================
            // User Email must be unique
            // =========================

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();


            // =========================
            // Member Email must be unique
            // =========================

            modelBuilder.Entity<Member>()
                .HasIndex(m => m.Email)
                .IsUnique();
        }
    }
}