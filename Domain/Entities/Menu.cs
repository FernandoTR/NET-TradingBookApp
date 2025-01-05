using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public partial class Menu
{
    [Key]
    public int Id { get; set; }

    [StringLength(250)]
    public string Name { get; set; } = null!;

    [Column("URL")]
    [StringLength(500)]
    public string Url { get; set; } = null!;

    [StringLength(250)]
    public string Icon { get; set; } = null!;

    public int? ParentMenuId { get; set; }

    public int Position { get; set; }

    public int? PermissionNumber { get; set; }

    public bool Visible { get; set; }

    public string? Comment { get; set; }

    public int ApplicationId { get; set; }

    [InverseProperty("Menu")]
    public virtual ICollection<AccessMenu> AccessMenus { get; set; } = new List<AccessMenu>();

    [ForeignKey("ApplicationId")]
    [InverseProperty("Menus")]
    public virtual Application Application { get; set; } = null!;
}
