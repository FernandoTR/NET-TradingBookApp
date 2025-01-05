using Web.Models;

namespace Web.Helpers;

public class ActionButtonHelper
{
    public static string GenerateActionMenu(ActionMenuModel model)
    {
        var actions = new List<string>();

        foreach (var option in model.ActionOptionMenus)
        {
            // Omitir si las propiedades necesarias son nulas o vacías
            if (string.IsNullOrEmpty(option.UrlAction) && string.IsNullOrEmpty(option.JavaScriptAction))
            {
                continue;
            }

            // Construir el enlace según el tipo de acción
            var action = !string.IsNullOrEmpty(option.JavaScriptAction)
                   ? $"<a href='javascript:{option.JavaScriptAction}(&#39;{model.Id}&#39;)' class='menu-link px-3'>{option.Title}</a>"
                   : $"<a href='{option.UrlAction}' class='menu-link px-3'>{option.Title}</a>";

            // Agregar el elemento completo al listado de acciones
            actions.Add($@"
            <div class='menu-item px-3'>
                {action}
            </div>");
        }

        var menu = string.Join(Environment.NewLine, actions);

        return $@"
            <a href='#' class='btn btn-sm btn-light btn-flex btn-center btn-active-light-primary' data-kt-menu-trigger='click' data-kt-menu-placement='bottom-end'>
                Acciones 
                <i class='ki-duotone ki-down fs-5 ms-1'></i>
            </a>
            <div class='menu menu-sub menu-sub-dropdown menu-column menu-rounded menu-gray-600 menu-state-bg-light-primary fw-semibold fs-7 w-125px py-4' data-kt-menu='true'>
                {menu}
            </div>";
    }
}
