using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

[Index("Email", Name = "UK_Email", IsUnique = true)]
[Index("Number", Name = "UK_EmployeeNumber", IsUnique = true)]
public partial class Employee
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreationDate { get; set; }

    [StringLength(450)]
    public string AuthorId { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? AlterDate { get; set; }

    [StringLength(450)]
    public string? AlterAuthorId { get; set; }

    [StringLength(7)]
    public string Number { get; set; } = null!;

    [StringLength(250)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string LastName { get; set; } = null!;

    [StringLength(200)]
    public string Email { get; set; } = null!;

    public string? Location { get; set; }

    public bool ConfirmedEmail { get; set; }

    [StringLength(200)]
    public string? ConfirmationHash { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ConfirmationHashEndDate { get; set; }

    public int StatusEmployeeId { get; set; }

    [ForeignKey("AlterAuthorId")]
    [InverseProperty("EmployeeAlterAuthors")]
    public virtual AspNetUser? AlterAuthor { get; set; }

    [ForeignKey("AuthorId")]
    [InverseProperty("EmployeeAuthors")]
    public virtual AspNetUser Author { get; set; } = null!;

    [ForeignKey("StatusEmployeeId")]
    [InverseProperty("Employees")]
    public virtual StatusEmployee StatusEmployee { get; set; } = null!;
}
