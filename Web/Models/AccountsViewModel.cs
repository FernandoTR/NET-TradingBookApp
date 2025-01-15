using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class AccountsViewModel
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public int CatAccountTypeId { get; set; }
    public decimal InitialBalance { get; set; }

    [Display(Name = "Saldo actual")]
    public decimal CurrentBalance { get; set; }
    public string Currency { get; set; } = null!;
    public DateTime UpdatedAt { get; set; }

    [Display(Name = "Monto")]
    public decimal? Cash { get; set; }
    public string Task { get; set; }

}

public class AccountsOperationsViewModel
{
    public int Id { get; set; }   
    public decimal? Cash { get; set; }
}
