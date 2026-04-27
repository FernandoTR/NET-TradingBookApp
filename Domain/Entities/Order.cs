using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure;

[Index("Grade", Name = "IX_Orders_Grade")]
[Index("IsTrendAligned", Name = "IX_Orders_IsTrendAligned")]
[Index("LocationType", Name = "IX_Orders_LocationType")]
[Index("StructuralScore", Name = "IX_Orders_StructuralScore")]
public partial class Order
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime AlterDate { get; set; }

    [StringLength(450)]
    public string AuthorId { get; set; } = null!;

    [Column("Cat_CategoryId")]
    public int CatCategoryId { get; set; }

    public int AccountId { get; set; }

    [Column("Cat_InstrumentId")]
    public int CatInstrumentId { get; set; }

    public DateOnly Date { get; set; }

    public TimeOnly Time { get; set; }

    [Column("Cat_DayId")]
    public int CatDayId { get; set; }

    [Column("Cat_StageId")]
    public int CatStageId { get; set; }

    [Column("Cat_FigureId")]
    public int CatFigureId { get; set; }

    [Column("Cat_FrameId")]
    public int CatFrameId { get; set; }

    [Column("Cat_TriggerId")]
    public int CatTriggerId { get; set; }

    [Column("Cat_DirectionId")]
    public int CatDirectionId { get; set; }

    [Column("Cat_SceneryId")]
    public int CatSceneryId { get; set; }

    [Column("Cat_StatusId")]
    public int CatStatusId { get; set; }

    [Column("SL")]
    public bool? Sl { get; set; }

    [Column("TP1")]
    public bool? Tp1 { get; set; }

    [Column("TP2")]
    public bool? Tp2 { get; set; }

    [Column("TP3")]
    public bool? Tp3 { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal? Target { get; set; }

    public string Chart { get; set; } = null!;

    public string? Comments { get; set; }

    // =========================
    // 🧠 NUEVO MOTOR CUANTITATIVO
    // =========================

    [Column("IsTrendAligned")]
    public bool? IsTrendAligned { get; set; }

    [Column("LocationType")]
    public byte? LocationType { get; set; }

    [Column("ConfirmationType")]
    public byte? ConfirmationType { get; set; }

    [Column("IsPivotZone")]
    public bool? IsPivotZone { get; set; }

    [Column("StructuralScore")]
    public short? StructuralScore { get; set; }

    [Column("TotalScore")]
    public double? TotalScore { get; set; }

    [StringLength(2)]
    [Unicode(false)]
    public string? Grade { get; set; }

    // =========================
    // 🧠 HELPERS (NO MAPEADOS)
    // =========================

    //public GradeType? GradeEnum
    //{
    //    get
    //    {
    //        if (string.IsNullOrEmpty(Grade))
    //            return null;

    //        return Enum.TryParse<GradeType>(Grade, out var result)
    //            ? result
    //            : null;
    //    }
    //    set
    //    {
    //        Grade = value?.ToString();
    //    }
    //}

    [ForeignKey("AccountId")]
    [InverseProperty("Orders")]
    public virtual Account Account { get; set; } = null!;

    [InverseProperty("Order")]
    public virtual ICollection<AccountBalance> AccountBalances { get; set; } = new List<AccountBalance>();

    [ForeignKey("AuthorId")]
    [InverseProperty("Orders")]
    public virtual AspNetUser Author { get; set; } = null!;

    [ForeignKey("CatCategoryId")]
    [InverseProperty("Orders")]
    public virtual CatCategory CatCategory { get; set; } = null!;

    [ForeignKey("CatDayId")]
    [InverseProperty("Orders")]
    public virtual CatDay CatDay { get; set; } = null!;

    [ForeignKey("CatDirectionId")]
    [InverseProperty("Orders")]
    public virtual CatDirection CatDirection { get; set; } = null!;

    [ForeignKey("CatFigureId")]
    [InverseProperty("Orders")]
    public virtual CatFigure CatFigure { get; set; } = null!;

    [ForeignKey("CatFrameId")]
    [InverseProperty("Orders")]
    public virtual CatFrame CatFrame { get; set; } = null!;

    [ForeignKey("CatInstrumentId")]
    [InverseProperty("Orders")]
    public virtual CatInstrument CatInstrument { get; set; } = null!;

    [ForeignKey("CatSceneryId")]
    [InverseProperty("Orders")]
    public virtual CatScenery CatScenery { get; set; } = null!;

    [ForeignKey("CatStageId")]
    [InverseProperty("Orders")]
    public virtual CatStage CatStage { get; set; } = null!;

    [ForeignKey("CatStatusId")]
    [InverseProperty("Orders")]
    public virtual CatStatus CatStatus { get; set; } = null!;

    [ForeignKey("CatTriggerId")]
    [InverseProperty("Orders")]
    public virtual CatTrigger CatTrigger { get; set; } = null!;

    [InverseProperty("Order")]
    public virtual ICollection<Trade> Trades { get; set; } = new List<Trade>();
}
