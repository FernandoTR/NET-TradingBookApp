using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public partial class Application
{
    [Key]
    public int Id { get; set; }

    [StringLength(150)]
    public string Name { get; set; } = null!;

    [InverseProperty("Application")]
    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

    [InverseProperty("Application")]
    public virtual ICollection<ApplicationRole> ApplicationRoles { get; set; } = new List<ApplicationRole>();

    [InverseProperty("Application")]
    public virtual ICollection<ErrorLog> ErrorLogs { get; set; } = new List<ErrorLog>();

    [InverseProperty("Application")]
    public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();
}
