using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public partial class Trade
{
    [Key]
    public int Id { get; set; }

    public int OrderId { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string OrderType { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string TradeType { get; set; } = null!;

    [Column(TypeName = "decimal(18, 4)")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal Price { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal? CommissionRate { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal Total { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime TradeDate { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("Trades")]
    public virtual Order Order { get; set; } = null!;
}
