using Infrastructure;

namespace Application.Interfaces;

public interface ITradingScoreEngineService
{
    void Evaluate(Order order);
}
