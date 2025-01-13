using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

[Table("Cat_Trigger")]
public partial class CatTrigger
{
    [Key]
    public int Id { get; set; }

    [StringLength(10)]
    public string? Code { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }

    public bool? IsActived { get; set; }

    [InverseProperty("CatTrigger")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
