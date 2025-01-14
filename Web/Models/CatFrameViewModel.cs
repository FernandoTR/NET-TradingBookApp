using System.ComponentModel;

namespace Web.Models
{
    public class CatFrameViewModel
    {
        public int Id { get; set; }

        [DisplayName("Código")]
        public required string Code { get; set; }

        [DisplayName("Descripción")]
        public required string Description { get; set; }
        public string? Task { get; set; }
    }
}
