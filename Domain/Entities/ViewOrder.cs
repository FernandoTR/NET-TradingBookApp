using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

[Keyless]
public partial class ViewOrder
{
    public int Id { get; set; }

    [Column("Cat_CategoryId")]
    public int CatCategoryId { get; set; }

    [StringLength(50)]
    public string? CategoryName { get; set; }

    [Column("Cat_InstrumentId")]
    public int CatInstrumentId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string InstrumentsName { get; set; } = null!;

    public DateOnly Date { get; set; }

    public TimeOnly Time { get; set; }

    [Column("Cat_DayId")]
    public int CatDayId { get; set; }

    [StringLength(50)]
    public string? DayName { get; set; }

    [Column("Cat_StageId")]
    public int CatStageId { get; set; }

    [StringLength(50)]
    public string? StageName { get; set; }

    [Column("Cat_FigureId")]
    public int CatFigureId { get; set; }

    [StringLength(200)]
    public string? FigureName { get; set; }

    [Column("Cat_FrameId")]
    public int CatFrameId { get; set; }

    [StringLength(50)]
    public string? FrameName { get; set; }

    [Column("Cat_TriggerId")]
    public int CatTriggerId { get; set; }

    [StringLength(200)]
    public string? TriggerName { get; set; }

    [Column("Cat_DirectionId")]
    public int CatDirectionId { get; set; }

    [StringLength(50)]
    public string? DirectionName { get; set; }

    [Column("Cat_SceneryId")]
    public int CatSceneryId { get; set; }

    [StringLength(20)]
    public string? SceneryName { get; set; }

    [Column("Cat_StatusId")]
    public int CatStatusId { get; set; }

    [StringLength(50)]
    public string? StatusName { get; set; }

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

    public string? Comments { get; set; }

    public string Chart { get; set; } = null!;

    public bool? IsTrendAligned { get; set; }

    public byte? LocationType { get; set; }

    public byte? ConfirmationType { get; set; }

    public bool? IsPivotZone { get; set; }

    public short? StructuralScore { get; set; }

    public double? TotalScore { get; set; }

    [StringLength(2)]
    [Unicode(false)]
    public string? Grade { get; set; }
}
