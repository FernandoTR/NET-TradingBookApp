using Infrastructure;

namespace Application.Interfaces;

public interface ICatInstrumentsService
{
    Task<bool> AddAsync(CatInstrument entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<CatInstrument>> GetAllAsync();
    Task<CatInstrument?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(CatInstrument entity);
}
