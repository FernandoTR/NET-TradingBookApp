namespace Application.DTOs;

public class GetTBAnalyticsTriggerDto
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public int Quantity { get; set; }
    public int SL { get; set; }
    public int TP1 { get; set; }
    public int TP2 { get; set; }
    public int TP3 { get; set; }
    public double SLP { get; set; }
    public double TP1P { get; set; }
    public double TP2P { get; set; }
    public double TP3P { get; set; }
    public int Valid { get; set; }


}
