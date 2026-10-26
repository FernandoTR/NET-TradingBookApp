using Application.DTOs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Data;

// Archivo separado para mantener y proteger las personalizaciones en OnModelCreating mientras usas Database First.
public partial class ApplicationDbContext
{
    public virtual DbSet<ContextualScoreSnapshot> ContextualScoreSnapshots { get; set; } = null!;

    public virtual DbSet<AiTradeValidationMetricSnapshot> AiTradeValidationMetricSnapshots { get; set; } = null!;

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // Configurar la clase como entidad sin clave
        modelBuilder.Entity<GetMenuByUserIdDto>().HasNoKey();
        modelBuilder.Entity<GetTBAnalyticsDayDto>().HasNoKey();
        modelBuilder.Entity<GetTBAnalyticsDirectionDto>().HasNoKey();
        modelBuilder.Entity<GetTBAnalyticsFigureDto>().HasNoKey();
        modelBuilder.Entity<GetTBAnalyticsLastBlockDto>().HasNoKey();
        modelBuilder.Entity<GetTBAnalyticsSceneryDto>().HasNoKey();
        modelBuilder.Entity<GetTBAnalyticsStageDto>().HasNoKey();
        modelBuilder.Entity<GetTBAnalyticsTimeDto>().HasNoKey();
        modelBuilder.Entity<GetTBAnalyticsTriggerDto>().HasNoKey();
        modelBuilder.Entity<GetTBAnalyticsConvergenceDto>().HasNoKey();
        modelBuilder.Entity<GetOrdersDataTableDto>().HasNoKey();

        modelBuilder.Entity<ContextualScoreSnapshot>(entity =>
        {
            entity.ToTable("ContextualScoreSnapshots", table =>
            {
                table.HasCheckConstraint("CK_ContextualScoreSnapshots_Source", "([SourceKind] = 1 AND [OrderId] IS NOT NULL) OR ([SourceKind] = 2 AND [ValidationId] IS NOT NULL)");
                table.HasCheckConstraint("CK_ContextualScoreSnapshots_State", "([State] = 2 AND [ContextRank] BETWEEN 0 AND 6 AND [Value] = CASE [ContextRank] WHEN 0 THEN CONVERT(decimal(30,28), 0) WHEN 1 THEN CONVERT(decimal(30,28), 16.666666666666666666666666667) WHEN 2 THEN CONVERT(decimal(30,28), 33.333333333333333333333333333) WHEN 3 THEN CONVERT(decimal(30,28), 50) WHEN 4 THEN CONVERT(decimal(30,28), 66.666666666666666666666666667) WHEN 5 THEN CONVERT(decimal(30,28), 83.333333333333333333333333333) WHEN 6 THEN CONVERT(decimal(30,28), 100) END AND [Grade] IS NOT NULL) OR ([State] IN (0, 1) AND [ContextRank] IS NULL AND [Value] IS NULL AND [Grade] IS NULL)");
                table.HasCheckConstraint("CK_ContextualScoreSnapshots_Revision", "[EvaluationRevision] IS NULL OR [EvaluationRevision] > 0");
                table.HasCheckConstraint("CK_ContextualScoreSnapshots_Json", "ISJSON([DefinitionJson]) = 1 AND ISJSON([InputSnapshotJson]) = 1 AND ISJSON([ComponentsJson]) = 1");
            });
            entity.HasKey(snapshot => snapshot.Id).HasName("PK_ContextualScoreSnapshots");
            entity.Property(snapshot => snapshot.UserId).HasMaxLength(450).IsRequired();
            entity.Property(snapshot => snapshot.SourceKind).HasConversion<byte>();
            entity.Property(snapshot => snapshot.ScoreVersion).HasMaxLength(100).IsRequired();
            entity.Property(snapshot => snapshot.State).HasConversion<byte>();
            entity.Property(snapshot => snapshot.Value).HasPrecision(30, 28);
            entity.Property(snapshot => snapshot.Grade).HasConversion<string>().HasMaxLength(2).IsUnicode(false);
            entity.Property(snapshot => snapshot.DefinitionJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(snapshot => snapshot.InputSnapshotJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(snapshot => snapshot.ComponentsJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(snapshot => snapshot.CalculatedAtUtc).HasColumnType("datetimeoffset(7)");
            entity.HasIndex(snapshot => new { snapshot.UserId, snapshot.CalculatedAtUtc })
                .HasDatabaseName("IX_ContextualScoreSnapshots_User_CalculatedAtUtc");
            entity.HasIndex(snapshot => new { snapshot.OrderId, snapshot.ScoreVersion })
                .HasDatabaseName("IX_ContextualScoreSnapshots_Order_Version");
            entity.HasIndex(snapshot => new { snapshot.ValidationId, snapshot.ScoreVersion })
                .HasDatabaseName("IX_ContextualScoreSnapshots_Validation_Version");
            entity.HasOne<Infrastructure.Identity.ApplicationUser>().WithMany().HasForeignKey(snapshot => snapshot.UserId)
                .OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_ContextualScoreSnapshots_AspNetUsers");
            entity.HasOne<Infrastructure.Order>().WithMany().HasForeignKey(snapshot => snapshot.OrderId)
                .OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_ContextualScoreSnapshots_Orders");
            entity.HasOne<Infrastructure.AiTradeValidation>().WithMany().HasForeignKey(snapshot => snapshot.ValidationId)
                .OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_ContextualScoreSnapshots_AiTradeValidations");
        });

        modelBuilder.Entity<AiTradeValidationMetricSnapshot>(entity =>
        {
            entity.ToTable("AiTradeValidationMetricSnapshots", table =>
            {
                table.HasCheckConstraint("CK_AiTradeValidationMetricSnapshots_Population", "([PopulationKind] = 1 AND [OrderId] IS NOT NULL) OR ([PopulationKind] = 2 AND [ValidationId] IS NOT NULL)");
                table.HasCheckConstraint("CK_AiTradeValidationMetricSnapshots_Revision", "[EvaluationRevision] IS NULL OR [EvaluationRevision] > 0");
                table.HasCheckConstraint("CK_AiTradeValidationMetricSnapshots_Json", "ISJSON([FieldsJson]) = 1 AND ([GraphicResultsJson] IS NULL OR ISJSON([GraphicResultsJson]) = 1) AND ([OutcomeObservationsJson] IS NULL OR ISJSON([OutcomeObservationsJson]) = 1)");
            });
            entity.HasKey(snapshot => snapshot.Id).HasName("PK_AiTradeValidationMetricSnapshots");
            entity.Property(snapshot => snapshot.PopulationKind).HasConversion<byte>();
            entity.Property(snapshot => snapshot.UserId).HasMaxLength(450).IsRequired();
            entity.Property(snapshot => snapshot.InstrumentId);
            entity.Property(snapshot => snapshot.StageId);
            entity.Property(snapshot => snapshot.TriggerId);
            entity.Property(snapshot => snapshot.StrategyVersion).HasMaxLength(100);
            entity.Property(snapshot => snapshot.EvidenceSchemaVersion).HasMaxLength(100);
            entity.Property(snapshot => snapshot.GraphicalRulesVersion).HasMaxLength(100);
            entity.Property(snapshot => snapshot.ClassificationRulesVersion).HasMaxLength(100);
            entity.Property(snapshot => snapshot.ScoreVersion).HasMaxLength(100);
            entity.Property(snapshot => snapshot.MetricsVersion).HasMaxLength(100);
            entity.Property(snapshot => snapshot.PromptVersion).HasMaxLength(100);
            entity.Property(snapshot => snapshot.ExtractionSchemaVersion).HasMaxLength(100);
            entity.Property(snapshot => snapshot.ProviderName).HasMaxLength(200);
            entity.Property(snapshot => snapshot.ModelName).HasMaxLength(200);
            entity.Property(snapshot => snapshot.FieldsJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(snapshot => snapshot.GraphicResultsJson).HasColumnType("nvarchar(max)");
            entity.Property(snapshot => snapshot.OutcomeObservationsJson).HasColumnType("nvarchar(max)");
            entity.Property(snapshot => snapshot.CapturedAtUtc).HasColumnType("datetimeoffset(7)");
            entity.HasIndex(snapshot => new { snapshot.UserId, snapshot.PopulationKind, snapshot.CapturedAtUtc })
                .HasDatabaseName("IX_AiTradeValidationMetricSnapshots_User_Population_CapturedAtUtc");
            entity.HasIndex(snapshot => new { snapshot.ValidationId, snapshot.EvaluationRevision })
                .HasDatabaseName("IX_AiTradeValidationMetricSnapshots_Validation_Revision");
            entity.HasIndex(snapshot => new
                {
                    snapshot.PopulationKind, snapshot.InstrumentId, snapshot.StageId, snapshot.TriggerId, snapshot.CapturedAtUtc
                })
                .HasDatabaseName("IX_AiTradeValidationMetricSnapshots_Population_Dimensions_CapturedAtUtc");
            entity.HasOne<Infrastructure.Identity.ApplicationUser>().WithMany().HasForeignKey(snapshot => snapshot.UserId)
                .OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_AiTradeValidationMetricSnapshots_AspNetUsers");
            entity.HasOne<Infrastructure.Order>().WithMany().HasForeignKey(snapshot => snapshot.OrderId)
                .OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_AiTradeValidationMetricSnapshots_Orders");
            entity.HasOne<Infrastructure.AiTradeValidation>().WithMany().HasForeignKey(snapshot => snapshot.ValidationId)
                .OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_AiTradeValidationMetricSnapshots_AiTradeValidations");
            entity.HasOne<ContextualScoreSnapshot>().WithMany().HasForeignKey(snapshot => snapshot.ContextualScoreSnapshotId)
                .OnDelete(DeleteBehavior.NoAction).HasConstraintName("FK_AiTradeValidationMetricSnapshots_ContextualScoreSnapshots");
        });

    }
}
