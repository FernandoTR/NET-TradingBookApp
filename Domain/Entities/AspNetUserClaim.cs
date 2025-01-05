using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public partial class AspNetUserClaim
{
    [Key]
    public int Id { get; set; }

    [StringLength(450)]
    public string UserId { get; set; } = null!;

    [StringLength(256)]
    public string? ClaimType { get; set; }

    [StringLength(256)]
    public string? ClaimValue { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("AspNetUserClaims")]
    public virtual AspNetUser User { get; set; } = null!;
}
