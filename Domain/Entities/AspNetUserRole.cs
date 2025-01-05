using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

[PrimaryKey("UserId", "RoleId")]
public partial class AspNetUserRole
{
    [Key]
    public string UserId { get; set; } = null!;

    [Key]
    public string RoleId { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? CreatedDate { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("AspNetUserRoles")]
    public virtual AspNetRole Role { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("AspNetUserRoles")]
    public virtual AspNetUser User { get; set; } = null!;
}
