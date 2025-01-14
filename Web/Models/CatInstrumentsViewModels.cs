using System.ComponentModel;

namespace Web.Models
{
    public class CatInstrumentsViewModels
    {
        public int Id { get; set; }

        [DisplayName("Nombre")]
        public string Name { get; set; } = null!;
        public string Ticker { get; set; } = null!;

        [DisplayName("Tipo de Instrumento")]
        public string InstrumentType { get; set; } = null!;

        [DisplayName("Moneda")]
        public string Currency { get; set; } = null!;

        [DisplayName("Mercado")]
        public string Market { get; set; } = null!;
        public bool IsActived { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string? LinkIcon { get; set; }
        public string Task { get; set; } = null!;

    }
}
