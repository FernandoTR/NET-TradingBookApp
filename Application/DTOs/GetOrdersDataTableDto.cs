

namespace Application.DTOs;

public class GetOrdersDataTableDto
{
    public int Id { get; set; }
    public int StatusId { get; set; }
    public string CategoryName { get; set; }
    public string AccountTypeName { get; set; }
    public string SymbolName { get; set; }
    public DateTime CreationDate { get; set; }
    public string TimeFormat { get; set; }
    public string DayName { get; set; }
    public string StageName { get; set; }
    public string FigureName { get; set; }
    public string FrameName { get; set; }
    public string TriggerName { get; set; }
    public int DirectionId { get; set; }
    public string DirectionName { get; set; }
    public string SceneryName { get; set; }
    public string StatusStyleName { get; set; }
    public string SLStyle { get; set; }
    public string TP1Style { get; set; }
    public string TP2Style { get; set; }
    public string TP3Style { get; set; }
    public bool SL { get; set; }
    public bool TP1 { get; set; }
    public bool TP2 { get; set; }
    public bool TP3 { get; set; }
    public string Target { get; set; }
    public string Chart { get; set; }
    public string Task { get; set; }
    public Int64 NumRow { get; set; }
    public string Bloque { get; set; }
    public bool Comodin { get; set; }


}
