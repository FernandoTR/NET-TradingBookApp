using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public partial class ApplicationRole
{
    [Key]
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    [StringLength(450)]
    public string RolId { get; set; } = null!;

    [ForeignKey("ApplicationId")]
    [InverseProperty("ApplicationRoles")]
    public virtual Application Application { get; set; } = null!;

    [ForeignKey("RolId")]
    [InverseProperty("ApplicationRoles")]
    public virtual AspNetRole Rol { get; set; } = null!;
}
