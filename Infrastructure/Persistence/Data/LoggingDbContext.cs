using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Data;

// Define un contexto separado para logs:  esto te permite aislar las operaciones de logging del resto de tu aplicación
public class LoggingDbContext : DbContext
{
    public LoggingDbContext(DbContextOptions<LoggingDbContext> options)
        : base(options)
    {
    }

    public DbSet<ErrorLog> ErrorLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuraciones adicionales para la tabla de logs (si es necesario)
        modelBuilder.Entity<ErrorLog>().ToTable("ErrorLogs");
    }
}
