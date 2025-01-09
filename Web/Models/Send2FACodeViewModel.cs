using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class Send2FACodeViewModel
{
    [Required]
    public required string Provider { get; set; }

    [Required]
    [Display(Name = "Código")]
    public string Code { get; set; } = string.Empty;
    public string? ReturnUrl { get; set; }

    [Display(Name = "¿Recordar este explorador?")]
    public bool RememberBrowser { get; set; }
    public bool RememberMe { get; set; }
    public required string UserId { get; set; }
    public string SendTo { get; set; } = string.Empty;


}
