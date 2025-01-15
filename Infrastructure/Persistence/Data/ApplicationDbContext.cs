using System;
using System.Collections.Generic;
using Infrastructure;
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

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AccountBalance> AccountBalances { get; set; }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<ApplicationRole> ApplicationRoles { get; set; }
   
    public virtual DbSet<CatAccountType> CatAccountTypes { get; set; }

    public virtual DbSet<CatCategory> CatCategories { get; set; }

    public virtual DbSet<CatDay> CatDays { get; set; }

    public virtual DbSet<CatDirection> CatDirections { get; set; }

    public virtual DbSet<CatFigure> CatFigures { get; set; }

    public virtual DbSet<CatFrame> CatFrames { get; set; }

    public virtual DbSet<CatInstrument> CatInstruments { get; set; }

    public virtual DbSet<CatScenery> CatSceneries { get; set; }

    public virtual DbSet<CatStage> CatStages { get; set; }

    public virtual DbSet<CatStatus> CatStatuses { get; set; }

    public virtual DbSet<CatTrigger> CatTriggers { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<ErrorLog> ErrorLogs { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<PasswordHistory> PasswordHistories { get; set; }

    public virtual DbSet<RiskManagementRule> RiskManagementRules { get; set; }

    public virtual DbSet<StatusEmployee> StatusEmployees { get; set; }

    public virtual DbSet<Trade> Trades { get; set; }

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

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasOne(d => d.CatAccountType).WithMany(p => p.Accounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Accounts_Cat_AccountType");

            entity.HasOne(d => d.User).WithMany(p => p.Accounts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Accounts_AspNetUsers");
        });

        modelBuilder.Entity<AccountBalance>(entity =>
        {
            entity.HasOne(d => d.Account).WithMany(p => p.AccountBalances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AccountBalances_Accounts");

            entity.HasOne(d => d.Order).WithMany(p => p.AccountBalances).HasConstraintName("FK_AccountBalances_Orders");
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

        

        modelBuilder.Entity<CatAccountType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_AccountType");
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

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasOne(d => d.Account).WithMany(p => p.Orders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Accounts");

            entity.HasOne(d => d.Author).WithMany(p => p.Orders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_AspNetUsers");

            entity.HasOne(d => d.CatCategory).WithMany(p => p.Orders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Cat_Category");

            entity.HasOne(d => d.CatDay).WithMany(p => p.Orders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Cat_Day");

            entity.HasOne(d => d.CatDirection).WithMany(p => p.Orders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Cat_Direction");

            entity.HasOne(d => d.CatFigure).WithMany(p => p.Orders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Cat_Figure");

            entity.HasOne(d => d.CatFrame).WithMany(p => p.Orders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Cat_Frame");

            entity.HasOne(d => d.CatInstrument).WithMany(p => p.Orders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Cat_Instruments");

            entity.HasOne(d => d.CatScenery).WithMany(p => p.Orders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Cat_Scenery");

            entity.HasOne(d => d.CatStage).WithMany(p => p.Orders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Cat_Stage");

            entity.HasOne(d => d.CatStatus).WithMany(p => p.Orders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Cat_Status");

            entity.HasOne(d => d.CatTrigger).WithMany(p => p.Orders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Cat_Trigger");
        });

        modelBuilder.Entity<PasswordHistory>(entity =>
        {
            entity.HasOne(d => d.User).WithMany(p => p.PasswordHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PasswordHistory_AspNetUsers");
        });

        modelBuilder.Entity<RiskManagementRule>(entity =>
        {
            entity.HasOne(d => d.Account).WithMany(p => p.RiskManagementRules)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RiskManagementRules_Accounts");
        });

        modelBuilder.Entity<Trade>(entity =>
        {
            entity.HasOne(d => d.Order).WithMany(p => p.Trades)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Trades_Orders");
        });

        modelBuilder.Entity<UserType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_TypeUser");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
