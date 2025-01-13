using System.ComponentModel;

namespace Web.Models;

public class CatCategoryViewModel
{
    public int Id { get; set; }

    [DisplayName("Nombre")]
    public string Name { get; set; }

    public string Task { get; set; }
}
