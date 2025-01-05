using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

[Table("Status")]
public partial class Status
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string? Name { get; set; }
}
