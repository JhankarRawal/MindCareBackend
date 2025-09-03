using MentalHealthApis.Models;
using MentalHealthApis.Models.Blog;
using Microsoft.EntityFrameworkCore;

namespace MentalHealthApis.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }

        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<BlogCategory> BlogCategories { get; set; }
        public DbSet<BlogTag> BlogTags { get; set; }
        public DbSet<BlogComment> BlogComments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure unique email for User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Relationships
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Or Cascade if user deletion means appointment deletion

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DoctorAvailability>()
                .HasOne(da => da.Doctor)
                .WithMany(d => d.Availabilities)
                .HasForeignKey(da => da.DoctorId);

            // Seed an Admin User (password should be hashed properly in a real app)
            // For simplicity, using a plain password here which AuthService would hash
            modelBuilder.Entity<User>().HasData(

    new User
    {
        Id = 1, // Manually set ID for seeding
        Name = "Admin User",
        Email = "admin@app.com",
        PasswordHash = "$2a$12$Cz3TQXWv5kMOJ2LF5pnz/eQU7jsffTQOjhsYQkR0w7O7PRlC/X5Y6", // Example static hashed password
        PhoneNumber = "1234567890",
        Role = UserRole.Admin
    });
            modelBuilder.Entity<BlogCategory>()
                .HasIndex(c => c.Slug)
                .IsUnique();

            modelBuilder.Entity<BlogPost>()
                .HasIndex(p => p.Slug)
                .IsUnique();

            modelBuilder.Entity<BlogPost>()
                .HasMany(p => p.Tags)
                .WithMany(t => t.BlogPosts)
                .UsingEntity(j => j.ToTable("BlogPostTags"));

            modelBuilder.Entity<BlogTag>()
                .HasIndex(t => t.Slug)
                .IsUnique();

          
        }
    }
}