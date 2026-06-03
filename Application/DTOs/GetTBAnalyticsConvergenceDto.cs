namespace Application.DTOs;

public class GetTBAnalyticsConvergenceDto
{
    public string Setup { get; set; }
    public int Trades { get; set; }
    public decimal TP1Rate { get; set; }
    public decimal TP2Rate { get; set; }
    public decimal TP3Rate { get; set; }
    public decimal SLRate { get; set; }
    public decimal Score { get; set; }
}
