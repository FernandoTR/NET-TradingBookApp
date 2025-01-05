
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Data;

public partial class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccessMenu> AccessMenus { get; set; }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<ApplicationRole> ApplicationRoles { get; set; }  

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<ErrorLog> ErrorLogs { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<PasswordHistory> PasswordHistories { get; set; }

    public virtual DbSet<StatusEmployee> StatusEmployees { get; set; }

    public virtual DbSet<UserType> UserTypes { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AccessMenu>(entity =>
        {
            entity.HasOne(d => d.Menu).WithMany(p => p.AccessMenus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AccessMenu_Menus");

            entity.HasOne(d => d.Rol).WithMany(p => p.AccessMenus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AccessMenu_AspNetRoles");
        });

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Logs");

            entity.Property(e => e.ApplicationId).HasDefaultValue(1);

            entity.HasOne(d => d.Application).WithMany(p => p.ActivityLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Logs_Applications");

            entity.HasOne(d => d.User).WithMany(p => p.ActivityLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Logs_AspNetUsers");
        });

        modelBuilder.Entity<ApplicationRole>(entity =>
        {
            entity.HasOne(d => d.Application).WithMany(p => p.ApplicationRoles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApplicationRoles_Applications");

            entity.HasOne(d => d.Rol).WithMany(p => p.ApplicationRoles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApplicationRoles_Roles");
        });       

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasOne(d => d.AlterAuthor).WithMany(p => p.EmployeeAlterAuthors).HasConstraintName("FK_Employees_AspNetUsers1");

            entity.HasOne(d => d.Author).WithMany(p => p.EmployeeAuthors)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employees_AspNetUsers");

            entity.HasOne(d => d.StatusEmployee).WithMany(p => p.Employees)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employees_StatusEmployee");
        });

        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_LogError");

            entity.Property(e => e.ApplicationId).HasDefaultValue(1);

            entity.HasOne(d => d.Application).WithMany(p => p.ErrorLogs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LogError_Applications");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.Property(e => e.ApplicationId).HasDefaultValue(1);

            entity.HasOne(d => d.Application).WithMany(p => p.Menus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Menus_Applications");
        });

        modelBuilder.Entity<PasswordHistory>(entity =>
        {
            entity.HasOne(d => d.User).WithMany(p => p.PasswordHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PasswordHistory_AspNetUsers");
        });

        modelBuilder.Entity<UserType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TypeUser");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
