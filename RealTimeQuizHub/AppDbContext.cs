using Microsoft.EntityFrameworkCore;
using RealTimeQuizHub.Models;

namespace RealTimeQuizHub
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Question> Questions { get; set; } = null!;
        public DbSet<Answer> Answers { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Room> Rooms { get; set; } = null!;
        public DbSet<Quiz> Quizzes { get; set; } = null!;
        public DbSet<QuizQuestion> QuizQuestions { get; set; } = null!;
        public DbSet<UserScore> UserScores { get; set; } = null!;
        public DbSet<UserStats> UserStats { get; set; } = null!;
        public DbSet<Badge> Badges { get; set; } = null!;
        public DbSet<UserBadge> UserBadges { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Question>()
                .HasMany(q => q.Answers)
                .WithOne()
                .HasForeignKey(a => a.QuestionId);

            modelBuilder.Entity<Answer>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Text)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(a => a.IsCorrect)
                    .IsRequired();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Name)
                    .IsRequired()
                    .HasMaxLength(30);
                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(u => u.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(u => u.IsAdmin)
                    .IsRequired()
                    .HasDefaultValue(false);
            });

            modelBuilder.Entity<Quiz>(entity =>
            {
                entity.HasKey(q => q.Id);
                entity.Property(q => q.Title)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(q => q.Description)
                    .HasMaxLength(500);
                entity.Property(q => q.TimerSecondsPerQuestion)
                    .HasDefaultValue(30);

                entity.HasMany(q => q.QuizQuestions)
                    .WithOne(qq => qq.Quiz!)
                    .HasForeignKey(qq => qq.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<QuizQuestion>(entity =>
            {
                entity.HasKey(qq => qq.Id);
                entity.Property(qq => qq.OrderIndex)
                    .IsRequired();

                entity.HasOne(qq => qq.Question)
                    .WithMany()
                    .HasForeignKey(qq => qq.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(qq => new { qq.QuizId, qq.QuestionId });
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(r => r.Description)
                    .HasMaxLength(500);
                entity.Property(r => r.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);
                entity.Property(r => r.TimerSecondsPerQuestion)
                    .HasDefaultValue(30);

                entity.HasOne(r => r.Quiz)
                    .WithMany()
                    .HasForeignKey(r => r.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserScore>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Score).IsRequired();

                entity.HasOne(s => s.User)
                    .WithMany()
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Room)
                    .WithMany()
                    .HasForeignKey(s => s.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(s => s.RoomId);
                entity.HasIndex(s => new { s.UserId, s.RoomId });
            });

            modelBuilder.Entity<UserStats>(entity =>
            {
                // One row per user: the user id is the primary key.
                entity.HasKey(s => s.UserId);
                entity.Property(s => s.UserId).ValueGeneratedNever();
                entity.Property(s => s.TotalScore).IsRequired();
                entity.Property(s => s.QuizzesCompleted).IsRequired();
                entity.Property(s => s.Wins).IsRequired();

                entity.HasOne(s => s.User)
                    .WithMany()
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Ignore(s => s.Level);
            });

            modelBuilder.Entity<Badge>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Name).IsRequired().HasMaxLength(100);
                entity.Property(b => b.Description).IsRequired().HasMaxLength(255);
                entity.Property(b => b.IconEmoji).IsRequired().HasMaxLength(16);
                entity.HasIndex(b => b.Name).IsUnique();

                entity.HasData(BadgeCatalog.Seed);
            });

            modelBuilder.Entity<UserBadge>(entity =>
            {
                entity.HasKey(ub => new { ub.UserId, ub.BadgeId });

                entity.HasOne(ub => ub.User)
                    .WithMany()
                    .HasForeignKey(ub => ub.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ub => ub.Badge)
                    .WithMany()
                    .HasForeignKey(ub => ub.BadgeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
