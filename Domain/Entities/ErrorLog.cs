using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public partial class ErrorLog
{
    [Key]
    public long Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime LogDate { get; set; }

    [StringLength(250)]
    public string MethodName { get; set; } = null!;

    public string? ExceptionMessage { get; set; }

    public string? ExceptionStackTrace { get; set; }

    public string? ExceptionString { get; set; }

    public int ApplicationId { get; set; }

    [ForeignKey("ApplicationId")]
    [InverseProperty("ErrorLogs")]
    public virtual Application Application { get; set; } = null!;
}
