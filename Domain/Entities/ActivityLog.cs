using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public partial class ActivityLog
{
    [Key]
    public long Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime LogDate { get; set; }

    [StringLength(250)]
    public string EventType { get; set; } = null!;

    public string Description { get; set; } = null!;

    [StringLength(450)]
    public string UserId { get; set; } = null!;

    public int ApplicationId { get; set; }

    [ForeignKey("ApplicationId")]
    [InverseProperty("ActivityLogs")]
    public virtual Application Application { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("ActivityLogs")]
    public virtual AspNetUser User { get; set; } = null!;
}
