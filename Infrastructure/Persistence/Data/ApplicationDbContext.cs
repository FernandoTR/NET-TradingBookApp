
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

    public virtual DbSet<ViewOrder> ViewOrders { get; set; }

    public virtual DbSet<AiTradeValidation> AiTradeValidations { get; set; }

    public virtual DbSet<AiTradeValidationMetric> AiTradeValidationMetrics { get; set; }

    public virtual DbSet<AiTradeValidationRule> AiTradeValidationRules { get; set; }

    public virtual DbSet<AiProviderConfiguration> AiProviderConfigurations { get; set; }

    public virtual DbSet<AiProviderModelCatalog> AiProviderModelCatalogs { get; set; }
    

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

        modelBuilder.Entity<AiTradeValidation>(entity =>
        {
            entity.ToTable("AiTradeValidation");

            entity.Property(e => e.EntryPrice).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.FinalSummary).HasMaxLength(4000);
            entity.Property(e => e.Grade)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ModelName).HasMaxLength(150);
            entity.Property(e => e.PromptVersion).HasMaxLength(50);
            entity.Property(e => e.ProviderName).HasMaxLength(100);
            entity.Property(e => e.RiskRewardRatio).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.SchemaVersion).HasMaxLength(50);
            entity.Property(e => e.StopLoss).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.TakeProfit).HasColumnType("decimal(18, 4)");
            entity.Property(e => e.UserId).HasMaxLength(450);
            entity.Property(e => e.ValidationStatus)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.VisualConfidence).HasColumnType("decimal(5, 4)");

            entity.HasOne<AspNetUser>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AiTradeValidation_AspNetUsers");

            entity.HasOne<Order>()
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .HasConstraintName("FK_AiTradeValidation_Orders");

            entity.HasOne<CatInstrument>()
                .WithMany()
                .HasForeignKey(e => e.InstrumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AiTradeValidation_Cat_Instruments");

            entity.HasOne<CatDirection>()
                .WithMany()
                .HasForeignKey(e => e.DirectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AiTradeValidation_Cat_Direction");

            entity.HasOne<CatTrigger>()
                .WithMany()
                .HasForeignKey(e => e.DetectedTriggerId)
                .HasConstraintName("FK_AiTradeValidation_Detected_Cat_Trigger");

            entity.HasOne<CatScenery>()
                .WithMany()
                .HasForeignKey(e => e.DetectedSceneryId)
                .HasConstraintName("FK_AiTradeValidation_Detected_Cat_Scenery");

            entity.HasOne<CatFigure>()
                .WithMany()
                .HasForeignKey(e => e.DetectedFigureId)
                .HasConstraintName("FK_AiTradeValidation_Detected_Cat_Figure");

            entity.HasOne<CatFrame>()
                .WithMany()
                .HasForeignKey(e => e.DetectedFrameId)
                .HasConstraintName("FK_AiTradeValidation_Detected_Cat_Frame");

            entity.HasOne<CatStage>()
                .WithMany()
                .HasForeignKey(e => e.DetectedStageId)
                .HasConstraintName("FK_AiTradeValidation_Detected_Cat_Stage");

            entity.HasOne<CatTrigger>()
                .WithMany()
                .HasForeignKey(e => e.ConfirmedTriggerId)
                .HasConstraintName("FK_AiTradeValidation_Confirmed_Cat_Trigger");

            entity.HasOne<CatScenery>()
                .WithMany()
                .HasForeignKey(e => e.ConfirmedSceneryId)
                .HasConstraintName("FK_AiTradeValidation_Confirmed_Cat_Scenery");

            entity.HasOne<CatFigure>()
                .WithMany()
                .HasForeignKey(e => e.ConfirmedFigureId)
                .HasConstraintName("FK_AiTradeValidation_Confirmed_Cat_Figure");

            entity.HasOne<CatFrame>()
                .WithMany()
                .HasForeignKey(e => e.ConfirmedFrameId)
                .HasConstraintName("FK_AiTradeValidation_Confirmed_Cat_Frame");

            entity.HasOne<CatStage>()
                .WithMany()
                .HasForeignKey(e => e.ConfirmedStageId)
                .HasConstraintName("FK_AiTradeValidation_Confirmed_Cat_Stage");
        });

        modelBuilder.Entity<AiTradeValidationMetric>(entity =>
        {
            entity.ToTable("AiTradeValidationMetric");

            entity.HasIndex(e => e.ValidationId, "UX_AiTradeValidationMetric_ValidationId")
                .IsUnique();
            entity.HasIndex(e => new { e.ProviderName, e.ModelName, e.CreatedAt }, "IX_AiTradeValidationMetric_Provider_Model_CreatedAt");
            entity.HasIndex(e => e.OrderId, "IX_AiTradeValidationMetric_OrderId");

            entity.Property(e => e.Grade)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.HumanCorrectionRate).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ModelName).HasMaxLength(150);
            entity.Property(e => e.OutcomeClassification)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ProviderName).HasMaxLength(100);

            entity.HasOne(d => d.Validation).WithOne(p => p.Metric)
                .HasForeignKey<AiTradeValidationMetric>(d => d.ValidationId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_AiTradeValidationMetric_AiTradeValidation");

            entity.HasOne(d => d.Order).WithMany(p => p.AiTradeValidationMetrics)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_AiTradeValidationMetric_Orders");
        });

        modelBuilder.Entity<AiTradeValidationRule>(entity =>
        {
            entity.ToTable("AiTradeValidationRule");

            entity.Property(e => e.Result)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.RuleCode).HasMaxLength(50);
            entity.Property(e => e.RuleName).HasMaxLength(200);
            entity.Property(e => e.ScoreObtained).HasColumnType("decimal(9, 4)");
            entity.Property(e => e.Source)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.Weight).HasColumnType("decimal(9, 4)");

            entity.HasOne(d => d.Validation).WithMany(p => p.Rules)
                .HasForeignKey(d => d.ValidationId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_AiTradeValidationRule_AiTradeValidation");
        });

        modelBuilder.Entity<AiProviderConfiguration>(entity =>
        {
            entity.ToTable("AiProviderConfiguration");

            entity.HasIndex(e => e.ProviderName, "UX_AiProviderConfiguration_ProviderName")
                .IsUnique();

            entity.HasIndex(e => e.ModelCatalogId, "IX_AiProviderConfiguration_ModelCatalogId");

            entity.HasIndex(e => e.IsActive, "UX_AiProviderConfiguration_Active")
                .IsUnique()
                .HasFilter("[IsActive] = 1");

            entity.Property(e => e.ProviderName).HasMaxLength(100);
            entity.Property(e => e.ModelName).HasMaxLength(150);
            entity.Property(e => e.Endpoint).HasMaxLength(500);
            entity.Property(e => e.ApiProtocol).HasMaxLength(50);
            entity.Property(e => e.ApiKeyEnvironmentVariable).HasMaxLength(150);
            entity.Property(e => e.TimeoutSeconds).HasDefaultValue(60);
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(d => d.ModelCatalog).WithMany()
                .HasForeignKey(d => d.ModelCatalogId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AiProviderConfiguration_AiProviderModelCatalog");
        });

        modelBuilder.Entity<AiProviderModelCatalog>(entity =>
        {
            entity.ToTable("AiProviderModelCatalog");

            entity.HasIndex(e => new { e.ProviderName, e.ModelId }, "UX_AiProviderModelCatalog_Provider_ModelId")
                .IsUnique();

            entity.HasIndex(e => new { e.ProviderName, e.IsEnabled, e.SortOrder }, "IX_AiProviderModelCatalog_Provider_Enabled_SortOrder");

            entity.Property(e => e.ProviderName).HasMaxLength(100);
            entity.Property(e => e.ModelName).HasMaxLength(150);
            entity.Property(e => e.ModelId).HasMaxLength(150);
            entity.Property(e => e.Endpoint).HasMaxLength(500);
            entity.Property(e => e.ApiProtocol).HasMaxLength(50);
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
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
            entity.Property(e => e.Grade).IsFixedLength();

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

        modelBuilder.Entity<ViewOrder>(entity =>
        {
            entity.ToView("View_Orders");

            entity.Property(e => e.Grade).IsFixedLength();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
