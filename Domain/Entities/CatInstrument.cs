using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

[Table("Cat_Instruments")]
public partial class CatInstrument
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string Ticker { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string InstrumentType { get; set; } = null!;

    [StringLength(10)]
    [Unicode(false)]
    public string Currency { get; set; } = null!;

    [StringLength(50)]
    [Unicode(false)]
    public string Market { get; set; } = null!;

    public bool IsActived { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime UpdatedAt { get; set; }

    [InverseProperty("CatInstrument")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
