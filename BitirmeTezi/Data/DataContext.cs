using Microsoft.EntityFrameworkCore;
using BitirmeTezi.Entities;

namespace BitirmeTezi.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<WritingQuestion> WritingQuestions { get; set; }
        public DbSet<StudentEnrollment> StudentEnrollments { get; set; }
        public DbSet<StudentProgress> StudentProgress { get; set; }
        public DbSet<WritingQuestionTranslation> WritingQuestionTranslations { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Admin> Admins { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WritingQuestionTranslation>()
                .HasIndex(t => new { t.WritingQuestionId, t.LanguageCode })
                .IsUnique();

            modelBuilder.Entity<Course>().HasQueryFilter(c => c.IsActive);
            modelBuilder.Entity<Lesson>().HasQueryFilter(l => l.IsActive);
            modelBuilder.Entity<WritingQuestion>().HasQueryFilter(q => q.IsActive);
            modelBuilder.Entity<WritingQuestionTranslation>().HasQueryFilter(t => t.IsActive);
            modelBuilder.Entity<Student>().HasQueryFilter(s => s.IsActive);
            modelBuilder.Entity<StudentProgress>().HasQueryFilter(p => p.IsActive);

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }


        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedDate = DateTime.Now;
                        entry.Entity.IsActive = true; 
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedDate = DateTime.Now;
                        break;

                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Entity.IsActive = false;
                        entry.Entity.UpdatedDate = DateTime.Now;
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }

    }
}
