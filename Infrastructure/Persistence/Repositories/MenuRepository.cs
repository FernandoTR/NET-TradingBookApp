using Application.Interfaces;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class MenuRepository : IMenuRepository
{
    private readonly ILogService _logService;
    private readonly ApplicationDbContext _context;

    public MenuRepository(ApplicationDbContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    public async Task<IEnumerable<Menu>> GetAllAsync(){
        try
        {
            return await _context.Menus.ToListAsync();
        }
        catch (Exception ex)
        {
            _logService.ErrorLog(nameof(GetAllAsync), ex);
            return Enumerable.Empty<Menu>();
        }
       
    } 



}
