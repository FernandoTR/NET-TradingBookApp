namespace Web.Models;

public class OrdersViewModel
{
    public int Id { get; set; }
    public DateTime? AlterDate { get; set; }
    public string? AuthorId { get; set; }
    public int CategoryId { get; set; }
    public int AccountTypeId { get; set; }
    public int InstrumentsId { get; set; }
    public DateTime CreationDate { get; set; }
    public TimeSpan Time { get; set; }
    public int DayId { get; set; }
    public int StageId { get; set; }
    public int FigureId { get; set; }
    public int FrameId { get; set; }
    public int TriggerId { get; set; }
    public int DirectionId { get; set; }
    public int SceneryId { get; set; }
    public int? StatusId { get; set; }
    public bool? SL { get; set; }
    public bool? TP1 { get; set; }
    public bool? TP2 { get; set; }
    public bool? TP3 { get; set; }
    public decimal? Target { get; set; }
    public string? Chart { get; set; }
    public string? Comments { get; set; }
    public bool? IsTrendAligned { get; set; }
    public byte? LocationType { get; set; }
    public byte? ConfirmationType { get; set; }
    public bool? IsPivotZone { get; set; }
    public short? StructuralScore { get; set; }
    public double? TotalScore { get; set; }
    public string? Grade { get; set; }
}

public class OrdersCreateViewModel
{   
    public int CategoryId { get; set; }
    public int AccountTypeId { get; set; }
    public int InstrumentsId { get; set; }
    public DateTime CreationDate { get; set; }
    public TimeSpan Time { get; set; }
    public int DayId { get; set; }
    public int StageId { get; set; }
    public int FigureId { get; set; }
    public int FrameId { get; set; }
    public int TriggerId { get; set; }
    public int DirectionId { get; set; }
    public int SceneryId { get; set; }
    public string? OrderTypeId { get; set; }
    public string? TradeTypeId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? Price { get; set; }
    public decimal? CommissionRate { get; set; }
    public decimal? Total { get; set; }

    public bool? IsTrendAligned { get; set; }
    public byte? LocationType { get; set; }
    public byte? ConfirmationType { get; set; }
    public bool? IsPivotZone { get; set; }
}

public class OrdersSellViewModel
{
    public int Id { get; set; }
    public bool? SL { get; set; }
    public bool? TP1 { get; set; }
    public bool? TP2 { get; set; }
    public bool? TP3 { get; set; }
    public decimal? Target { get; set; }
    public string? Chart { get; set; }
    public string? Comments { get; set; }
    public string? OrderTypeId { get; set; }
    public string? TradeTypeId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? Price { get; set; }
    public decimal? CommissionRate { get; set; }
    public decimal? Total { get; set; }
}