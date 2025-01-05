namespace Web.Models;

public class ActionMenuModel
{
    public string Id { get; set; } = string.Empty;
    public required List<ActionOptionMenuModel> ActionOptionMenus { get; set; }
}

public class ActionOptionMenuModel
{
    public string Title {  get; set; } = string.Empty;
    public string? UrlAction { get; set; } // Propiedad para manejar URL
    public string? JavaScriptAction { get; set; }// Propiedad para manejar funciones JavaScript
}