
namespace Application.DTOs;

public class AccountBalanceDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public int? OrderId { get; set; }
    public string AccountTypeName{ get; set; } 
    public decimal Balance { get; set; }
    public string Reference { get; set; }
    public DateTime UpdateAt { get; set; }
}
