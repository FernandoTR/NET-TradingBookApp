using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories;

public class AccountBalanceRepository : IAccountBalanceRepository
{
    private readonly ILogService _logService;
    private readonly ApplicationDbContext _context;

    public AccountBalanceRepository(ApplicationDbContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }


    public async Task<IEnumerable<AccountBalanceDto>> GetAllAccountBalanceByDateRangeAsync(string userId,DateTime dateStart, DateTime dateEnd)
    {
        if (dateStart > dateEnd)
            throw new ArgumentException("La fecha de inicio no puede ser mayor que la fecha de fin.", nameof(dateStart));

        try
        {
            // Filtra los registros por la usuario y el rango de fechas.
            return await _context.AccountBalances
                         .Where(accountBalance =>
                             accountBalance.Account.UserId == userId &&
                             accountBalance.UpdateAt >= dateStart &&
                             accountBalance.UpdateAt <= dateEnd)
                         .Select(accountBalance => new AccountBalanceDto
                         {
                             Id = accountBalance.Id,
                             OrderId = accountBalance.OrderId,
                             AccountId = accountBalance.AccountId,
                             AccountTypeName = accountBalance.Account.CatAccountType.Description,
                             Balance = accountBalance.Balance,
                             Reference = accountBalance.Reference,
                             UpdateAt = accountBalance.UpdateAt
                         })
                         .ToListAsync();

        }
        catch (Exception ex)
        {
            // Registra el error utilizando el servicio de registro.
            _logService.ErrorLog(nameof(GetAllAccountBalanceByDateRangeAsync), ex);

            // Retorna una colección vacía en caso de error.
            return Enumerable.Empty<AccountBalanceDto>();
        }
    }


}
