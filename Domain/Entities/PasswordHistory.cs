using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

[Table("PasswordHistory")]
public partial class PasswordHistory
{
    [Key]
    public int PasswordHistoryId { get; set; }

    [StringLength(450)]
    public string UserId { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime UpdateDate { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("PasswordHistories")]
    public virtual AspNetUser User { get; set; } = null!;
}
