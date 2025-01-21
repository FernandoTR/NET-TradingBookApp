using Application.Common;
using Application.Interfaces;
using Infrastructure;

namespace Application.Services;

public class AccountsService : IAccountsService
{
    private readonly IGenericRepository<Account> _repository;
    private readonly IAccountBalancesService _accountBalancesService;
    public AccountsService(IGenericRepository<Account> repository,
                           IAccountBalancesService accountBalancesService)
    {
        _repository = repository;
        _accountBalancesService = accountBalancesService;
    }

    public async Task<bool> AddAsync(Account entity)
    {
        return await _repository.AddAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Account>> GetAllAsync(QueryOptions<Account>? options = null)
    {
        return await _repository.GetAllAsync(options);
    }

    public async Task<Account?> GetByIdAsync(int id, QueryOptions<Account>? options = null)
    {
        return await _repository.GetByIdAsync(id, options);
    }

    public async Task<bool> UpdateAsync(Account entity)
    {
        return await _repository.UpdateAsync(entity);
    }

    /// <summary>
    /// Realiza un retiro de saldo de una cuenta específica.
    /// </summary>
    /// <param name="model">modelo del balance de la cuenta</param>
    /// <returns>
    /// Retorna <c>true</c> si el retiro se realizó correctamente; 
    /// de lo contrario, <c>false</c>.
    /// </returns>
    public async Task<bool> WithDrawCashAsync(AccountBalance model)
    {
        // Obtener la cuenta por su ID.
        var account = await _repository.GetByIdAsync(model.AccountId);
        if (account == null)
        {
            // Retorna false si la cuenta no existe.
            return false;
        }

        // Crear el registro de la transacción del retiro.
        var accountBalance = new AccountBalance
        {
            AccountId = account.Id,
            OrderId = model.OrderId == 0 ? null : model.OrderId,
            Balance = -model.Balance,
            Reference = model.Reference,
            UpdateAt = DateTime.Now 
        };

        // Registrar la transacción en el servicio de balances.
        var balanceAdded = await _accountBalancesService.AddAsync(accountBalance);
        if (!balanceAdded)
        {
            // Si no se puede registrar la transacción, retorna false.
            return false;
        }

        // Actualizar el saldo y la fecha de modificación de la cuenta.
        account.CurrentBalance -= model.Balance;
        account.UpdatedAt = DateTime.UtcNow;

        // Guardar los cambios en la base de datos.
        var accountUpdated = await _repository.UpdateAsync(account);
        if (!accountUpdated)
        {
            // Si no se puede actualizar la cuenta, retorna false.
            return false;
        }

        // Retorna true si todo el proceso fue exitoso.
        return true;
    }

    /// <summary>
    /// Realiza un abono de saldo de una cuenta específica.
    /// </summary>
    /// <param name="model">modelo del balance de la cuenta</param>
    /// <returns>
    /// Retorna <c>true</c> si el retiro se realizó correctamente; 
    /// de lo contrario, <c>false</c>.
    /// </returns>
    public async Task<bool> AddCashAsync(AccountBalance model)
    {
        // Obtener la cuenta por su ID.
        var account = await _repository.GetByIdAsync(model.AccountId);
        if (account == null)
        {
            // Retorna false si la cuenta no existe.
            return false;
        }

        // Crear el registro de la transacción del retiro.
        var accountBalance = new AccountBalance
        {
            AccountId = account.Id,
            OrderId = model.OrderId == 0 ? null : model.OrderId,
            Balance = model.Balance,
            Reference = model.Reference,
            UpdateAt = DateTime.Now
        };

        // Registrar la transacción en el servicio de balances.
        var balanceAdded = await _accountBalancesService.AddAsync(accountBalance);
        if (!balanceAdded)
        {
            // Si no se puede registrar la transacción, retorna false.
            return false;
        }

        // Actualizar el saldo y la fecha de modificación de la cuenta.
        account.CurrentBalance += model.Balance;
        account.UpdatedAt = DateTime.UtcNow;

        // Guardar los cambios en la base de datos.
        var accountUpdated = await _repository.UpdateAsync(account);
        if (!accountUpdated)
        {
            // Si no se puede actualizar la cuenta, retorna false.
            return false;
        }

        // Retorna true si todo el proceso fue exitoso.
        return true;
    }


}
