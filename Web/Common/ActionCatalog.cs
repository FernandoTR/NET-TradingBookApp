using Web.Enums;
using Web.Models;

namespace Web.Common;

public static class ActionCatalog
{
    public static readonly IReadOnlyDictionary<ActionType, ActionDefinition> Items = new Dictionary<ActionType, ActionDefinition>
        {
            [ActionType.View] = new()
            {
                Title = "Ver",
                Icon = "ki-eye",
                CssClass = "text-info"
            },

            [ActionType.Edit] = new()
            {
                Title = "Editar",
                Icon = "ki-pencil",
                CssClass = "text-warning"
            },

            [ActionType.Delete] = new()
            {
                Title = "Eliminar",
                Icon = "ki-trash",
                CssClass = "text-danger"
            },

            [ActionType.CreateUser] = new()
            {
                Title = "Generar usuario",
                Icon = "ki-user",
                CssClass = "text-primary"
            },

            [ActionType.ResendEmail] = new()
            {
                Title = "Reenviar correo",
                Icon = "ki-sms",
                CssClass = "text-info"
            },

            [ActionType.Activate] = new()
            {
                Title = "Activar",
                Icon = "ki-check-circle",
                CssClass = "text-success"
            },

            [ActionType.Deactivate] = new()
            {
                Title = "Desactivar",
                Icon = "ki-cross-circle",
                CssClass = "text-danger"
            },

            [ActionType.History] = new()
            {
                Title = "Historial",
                Icon = "ki-time",
                CssClass = "text-secondary"
            },

            [ActionType.Close] = new()
            {
                Title = "Cerrar",
                Icon = "ki-toggle-off",
                CssClass = "text-danger"
            }
    };
}