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
    }
}
