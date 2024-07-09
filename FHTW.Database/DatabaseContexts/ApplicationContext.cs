using FHTW.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace FHTW.Database.DatabaseContexts;

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
    public DbSet<DiscordServerRole> RequestableRoles { get; set; }
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
            .HasIndex(u => u.UID)
            .IsUnique();
        
        modelBuilder.Entity<DiscordUser>()
            .HasOne(d => d.Student)
            .WithOne(s => s.DiscordUser)
            .HasForeignKey<DiscordUser>(d => d.StudentUid)
            .HasPrincipalKey<Student>(s => s.UID);
        
        modelBuilder.Entity<DiscordUserRole>()
            .HasKey(dur => new { dur.DiscordUserId, dur.RoleId });

        modelBuilder.Entity<DiscordUserRole>()
            .HasOne(dur => dur.DiscordUser)
            .WithMany(du => du.DiscordUserRoles)
            .HasForeignKey(dur => dur.DiscordUserId);

        modelBuilder.Entity<DiscordUserRole>()
            .HasOne(dur => dur.DiscordServerRole)
            .WithMany(rr => rr.DiscordUserRoles)
            .HasForeignKey(dur => dur.RoleId);
    }
}