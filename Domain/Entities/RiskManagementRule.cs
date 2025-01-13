using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public partial class RiskManagementRule
{
    [Key]
    public int Id { get; set; }

    public int AccountId { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal? MaxDailyLoss { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal? MaxPositionSize { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal? MaxExposure { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("AccountId")]
    [InverseProperty("RiskManagementRules")]
    public virtual Account Account { get; set; } = null!;
}
