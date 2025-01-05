using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

[Index("Name", Name = "RoleNameIndex", IsUnique = true)]
public partial class AspNetRole
{
    [Key]
    public string Id { get; set; } = null!;

    [StringLength(256)]
    public string Name { get; set; } = null!;

    [StringLength(256)]
    public string? NormalizedName { get; set; }

    [StringLength(256)]
    public string? ConcurrencyStamp { get; set; }

    [InverseProperty("Rol")]
    public virtual ICollection<AccessMenu> AccessMenus { get; set; } = new List<AccessMenu>();

    [InverseProperty("Rol")]
    public virtual ICollection<ApplicationRole> ApplicationRoles { get; set; } = new List<ApplicationRole>();

    [InverseProperty("Role")]
    public virtual ICollection<AspNetRoleClaim> AspNetRoleClaims { get; set; } = new List<AspNetRoleClaim>();

    [InverseProperty("Role")]
    public virtual ICollection<AspNetUserRole> AspNetUserRoles { get; set; } = new List<AspNetUserRole>();
}
