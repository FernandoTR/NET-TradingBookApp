using System.ComponentModel;

namespace Web.Models;

public class CatFigureViewModel
{
    public int Id { get; set; }

    [DisplayName("Código")]
    public string? Code { get; set; }

    [DisplayName("Descripción")]
    public string? Description { get; set; }
    public bool? IsActived { get; set; }

    public string? Task { get; set; }
}
