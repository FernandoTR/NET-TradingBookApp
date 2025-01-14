using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

[Table("Cat_Scenery")]
public partial class CatScenery
{
    [Key]
    public int Id { get; set; }

    [StringLength(5)]
    public string? Code { get; set; }

    [StringLength(20)]
    public string? Description { get; set; }

    [InverseProperty("CatScenery")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
