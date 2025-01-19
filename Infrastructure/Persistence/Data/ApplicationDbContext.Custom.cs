using Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Data;

// Archivo separado para mantener y proteger las personalizaciones en OnModelCreating mientras usas Database First.
public partial class ApplicationDbContext
{
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
        modelBuilder.Entity<GetOrdersDataTableDto>().HasNoKey();

    }
}
