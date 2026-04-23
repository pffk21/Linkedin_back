using Microsoft.EntityFrameworkCore;
using Project_X_Data.Data.Entities;
using Project_X_Data.Models.LogInOut;

namespace Project_X_Data.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Comment> Comments { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<ContactInformation> ContactInformations { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<UserPostReaction> UserPostReactions { get; set; }
        public DbSet<UserCommentReaction> UserCommentReactions { get; set; }
        public DbSet<UserSkill> UserSkills { get; set; }
        public DbSet<Expirience> Expiriences { get; set; }
        public DbSet<Education> Educations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserAccess> UserAccesses { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserSkill>()
                .HasKey(us => new { us.UserId, us.SkillId });

            modelBuilder.Entity<UserSkill>()
                .HasOne(us => us.User)
                .WithMany(u => u.UserSkills)
                .HasForeignKey(us => us.UserId);

            modelBuilder.Entity<UserSkill>()
                .HasOne(us => us.Skill)
                .WithMany(s => s.UserSkills)
                .HasForeignKey(us => us.SkillId);

            modelBuilder.Entity<UserPostReaction>()
                .HasKey(upr => new { upr.UserId, upr.PostId });

            modelBuilder.Entity<UserPostReaction>()
                .HasOne(upr => upr.User)
                .WithMany(u => u.PostReactions)
                .HasForeignKey(upr => upr.UserId);

            modelBuilder.Entity<UserPostReaction>()
                .HasOne(upr => upr.Post)
                .WithMany(p => p.PostReactions)
                .HasForeignKey(upr => upr.PostId);

            modelBuilder.Entity<UserCommentReaction>()
                .HasKey(ucr => new { ucr.UserId, ucr.CommentId });

            modelBuilder.Entity<UserCommentReaction>()
                .HasOne(ucr => ucr.User)
                .WithMany(u => u.CommentReactions)
                .HasForeignKey(ucr => ucr.UserId);

            modelBuilder.Entity<UserCommentReaction>()
                .HasOne(ucr => ucr.Comment)
                .WithMany(c => c.CommentReactions)
                .HasForeignKey(ucr => ucr.CommentId);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ContactInformation>()
                .HasOne(c => c.User)
                .WithMany(ci => ci.ContactInformations)
                .HasForeignKey(c => c.UserId);
        }

    }
}
