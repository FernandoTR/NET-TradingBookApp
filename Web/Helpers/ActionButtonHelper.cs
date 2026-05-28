using Web.Common;
using Web.Models;

namespace Web.Helpers;

public class ActionButtonHelper
{
    public static string GenerateActionMenu(ActionMenuModel model)
    {
        var actions = new List<string>();

        foreach (var option in model.ActionOptionMenus)
        {
            if (string.IsNullOrEmpty(option.UrlAction) &&
                string.IsNullOrEmpty(option.JavaScriptAction))
            {
                continue;
            }

            var actionDefinition = ActionCatalog.Items[option.ActionType];

            var action = !string.IsNullOrEmpty(option.JavaScriptAction)
                ? $@"
                    <a class='kt-menu-link'
                        href='javascript:{option.JavaScriptAction}(&#39;{model.Id}&#39;)'>

                        <span class='kt-menu-icon'>
                            <i class='ki-filled {actionDefinition.Icon} {actionDefinition.CssClass}'></i>
                        </span>

                        <span class='kt-menu-title'>
                            {actionDefinition.Title}
                        </span>

                    </a>"
                : $@"
                    <a class='kt-menu-link'
                        href='{option.UrlAction}'>

                        <span class='kt-menu-icon'>
                            <i class='ki-filled {actionDefinition.Icon} {actionDefinition.CssClass}'></i>
                        </span>

                        <span class='kt-menu-title'>
                            {actionDefinition.Title}
                        </span>

                    </a>";

            actions.Add($@"<div class='kt-menu-item'>
                                {action}
                            </div>");
        }

        var menu = string.Join(Environment.NewLine, actions);

        return $@"
        <div class='kt-menu' data-kt-menu='true'>
            <div class='kt-menu-item kt-menu-item-dropdown' data-kt-menu-item-offset='0, 10px' data-kt-menu-item-placement='bottom-end' data-kt-menu-item-toggle='dropdown' data-kt-menu-item-trigger='click'>
                <button class='kt-menu-toggle kt-btn kt-btn-sm kt-btn-icon kt-btn-ghost'>
                    <i class='ki-filled ki-dots-vertical text-lg'></i>
                </button>
                <div class='kt-menu-dropdown kt-menu-default w-full max-w-[200px]' data-kt-menu-dismiss='true'>
                    {menu}
                </div>
            </div>
        </div>";
    }
}
