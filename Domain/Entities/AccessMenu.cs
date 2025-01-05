using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

[Table("AccessMenu")]
public partial class AccessMenu
{
    [Key]
    public int Id { get; set; }

    [StringLength(450)]
    public string RolId { get; set; } = null!;

    public int MenuId { get; set; }

    [ForeignKey("MenuId")]
    [InverseProperty("AccessMenus")]
    public virtual Menu Menu { get; set; } = null!;

    [ForeignKey("RolId")]
    [InverseProperty("AccessMenus")]
    public virtual AspNetRole Rol { get; set; } = null!;
}
