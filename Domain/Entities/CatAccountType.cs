using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

[Table("Cat_AccountType")]
public partial class CatAccountType
{
    [Key]
    public int Id { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string Code { get; set; } = null!;

    [StringLength(200)]
    [Unicode(false)]
    public string Description { get; set; } = null!;

    public bool IsActived { get; set; }

    [InverseProperty("CatAccountType")]
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}
