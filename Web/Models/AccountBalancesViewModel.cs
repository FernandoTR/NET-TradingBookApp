namespace Web.Models;

public class AccountBalancesViewModel
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public int? OrderId { get; set; }
    public decimal Balance { get; set; }
    public string Reference { get; set; } = null!;
    public DateTime UpdateAt { get; set; }
    public string? AccountTypeName { get; set; }


}
