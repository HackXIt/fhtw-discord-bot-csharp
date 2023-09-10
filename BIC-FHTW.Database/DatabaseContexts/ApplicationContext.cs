using BIC_FHTW.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace BIC_FHTW.Database.DatabaseContexts;

public class ApplicationContext : DbContext
{
    public ApplicationContext()
    {
    }

    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {
    }
    
    public DbSet<DiscordUser> Users { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<RequestableRole> RequestableRoles { get; set; }
    public DbSet<DiscordUserRole> DiscordUserRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=users.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DiscordUser>()
            .HasIndex(u => u.DiscordUserId)
            .IsUnique();

        modelBuilder.Entity<Student>()
            .HasIndex(u => u.EmailString)
            .IsUnique();
        
        modelBuilder.Entity<DiscordUser>()
            .HasOne(d => d.Student)
            .WithOne(s => s.DiscordUser)
            .HasForeignKey<DiscordUser>(d => d.StudentMail)
            .HasPrincipalKey<Student>(s => s.EmailString);
        
        modelBuilder.Entity<DiscordUserRole>()
            .HasKey(dur => new { dur.DiscordUserId, dur.RoleId });

        modelBuilder.Entity<DiscordUserRole>()
            .HasOne(dur => dur.DiscordUser)
            .WithMany(du => du.DiscordUserRoles)
            .HasForeignKey(dur => dur.DiscordUserId);

        modelBuilder.Entity<DiscordUserRole>()
            .HasOne(dur => dur.RequestableRole)
            .WithMany(rr => rr.DiscordUserRoles)
            .HasForeignKey(dur => dur.RoleId);
    }
}