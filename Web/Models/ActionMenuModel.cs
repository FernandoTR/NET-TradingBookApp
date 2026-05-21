using Web.Enums;

namespace Web.Models;

public class ActionMenuModel
{
    public string Id { get; set; } = string.Empty;
    public required List<ActionOptionMenuModel> ActionOptionMenus { get; set; }
}

public class ActionOptionMenuModel
{
    public ActionType ActionType { get; set; } // Propiedad el tipo de accion a realizar
    public string? UrlAction { get; set; } // Propiedad para manejar URL
    public string? JavaScriptAction { get; set; }// Propiedad para manejar funciones JavaScript  

}

public sealed class ActionDefinition
{
    public string Title { get; init; } = string.Empty;

    public string Icon { get; init; } = string.Empty;

    public string CssClass { get; init; } = string.Empty;
}