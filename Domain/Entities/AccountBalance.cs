using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public partial class AccountBalance
{
    [Key]
    public int Id { get; set; }

    public int AccountId { get; set; }

    public int? OrderId { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal Balance { get; set; }

    [StringLength(500)]
    [Unicode(false)]
    public string Reference { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime UpdateAt { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("AccountBalances")]
    public virtual Account Account { get; set; } = null!;

    [ForeignKey("OrderId")]
    [InverseProperty("AccountBalances")]
    public virtual Order? Order { get; set; }
}
