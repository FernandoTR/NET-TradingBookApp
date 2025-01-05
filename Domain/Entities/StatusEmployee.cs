using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

[Table("StatusEmployee")]
public partial class StatusEmployee
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [InverseProperty("StatusEmployee")]
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
