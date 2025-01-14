using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

[Table("Cat_Stage")]
public partial class CatStage
{
    [Key]
    public int Id { get; set; }

    [StringLength(5)]
    public string? Code { get; set; }

    [StringLength(50)]
    public string? Description { get; set; }

    [InverseProperty("CatStage")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
