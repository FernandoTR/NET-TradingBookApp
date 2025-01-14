using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public partial class Account
{
    [Key]
    public int Id { get; set; }

    [StringLength(450)]
    public string UserId { get; set; } = null!;

    [Column("Cat_AccountTypeId")]
    public int CatAccountTypeId { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal InitialBalance { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal CurrentBalance { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string Currency { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; }

    [InverseProperty("Account")]
    public virtual ICollection<AccountBalance> AccountBalances { get; set; } = new List<AccountBalance>();

    [ForeignKey("CatAccountTypeId")]
    [InverseProperty("Accounts")]
    public virtual CatAccountType CatAccountType { get; set; } = null!;

    [InverseProperty("Account")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    [InverseProperty("Account")]
    public virtual ICollection<RiskManagementRule> RiskManagementRules { get; set; } = new List<RiskManagementRule>();

    [ForeignKey("UserId")]
    [InverseProperty("Accounts")]
    public virtual AspNetUser User { get; set; } = null!;
}
