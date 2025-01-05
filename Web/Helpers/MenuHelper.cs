using Application.DTOs;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Helpers;

public static class MenuHelper
{
    //-- Contiene los Id de los menus que tienen un padre y ya fueron recorridos
    static HashSet<int>? oListMenuId;

    public static IHtmlContent RenderMenu(this IHtmlHelper htmlHelper, List<GetMenuByUserIdDto> menuItems)
    {
        oListMenuId = new HashSet<int>();

        var builder = new TagBuilder("div");
        builder.AddCssClass("menu menu-column menu-rounded menu-sub-indention fw-semibold fs-6");
        builder.Attributes.Add("id", "#kt_app_sidebar_menu");
        builder.Attributes.Add("data-kt-menu", "true");
        builder.Attributes.Add("data-kt-menu-expand", "false");


        // Recorremos y ordenamos los elementos de menú
        foreach (var menuItem in menuItems.OrderBy(x => x.MenuId))
        {
            if(menuItem.ParentMenuId == null)
            {
                builder.InnerHtml.AppendHtml(RenderMenuItemTittle(menuItem));
            }
            else if (menuItems.Any(x => x.ParentMenuId == menuItem.MenuId) && !oListMenuId.Any(x => x.Equals(Convert.ToInt32(menuItem.MenuId))))
            {                    
                oListMenuId.Add(Convert.ToInt32(menuItem.MenuId));

                builder.InnerHtml.AppendHtml(RenderMenuItemParents(menuItem, menuItems));
            }
            else if (!oListMenuId.Any(x => x.Equals(Convert.ToInt32(menuItem.MenuId))))
            {
                builder.InnerHtml.AppendHtml(RenderMenuItem(menuItem));
            }                  
        }

        return builder;
    }

    private static TagBuilder RenderMenuItemTittle(GetMenuByUserIdDto menuItem)
    {
        var itemBuilder = new TagBuilder("div");
        itemBuilder.AddCssClass("menu-item");

        var menuContentBuilder = new TagBuilder("div");
        menuContentBuilder.AddCssClass("menu-content");

        var tittleBuilder = new TagBuilder("span");
        tittleBuilder.AddCssClass("menu-heading fw-bold text-uppercase fs-7");
        tittleBuilder.InnerHtml.Append(menuItem.Name);

        menuContentBuilder.InnerHtml.AppendHtml(tittleBuilder);
        itemBuilder.InnerHtml.AppendHtml(menuContentBuilder);        

        return itemBuilder;
    }
    private static TagBuilder RenderMenuItem(GetMenuByUserIdDto menuItem)
    {
        var itemBuilder = new TagBuilder("div");
        itemBuilder.AddCssClass("menu-item");

        var linkBuilder = new TagBuilder("a");
        linkBuilder.AddCssClass("menu-link");
        if (!string.IsNullOrEmpty(menuItem.URL))
        {
            linkBuilder.Attributes.Add("href", menuItem.URL);
        }

        var iconBuilder = new TagBuilder("span");
        iconBuilder.AddCssClass("menu-icon");
        var iconTag = new TagBuilder("i");
        iconTag.AddCssClass(menuItem.Icon ?? "fa-default fs-6");
        iconBuilder.InnerHtml.AppendHtml(iconTag);

        var titleBuilder = new TagBuilder("span");
        titleBuilder.AddCssClass("menu-title");
        titleBuilder.InnerHtml.Append(menuItem.Name);

        linkBuilder.InnerHtml.AppendHtml(iconBuilder);
        linkBuilder.InnerHtml.AppendHtml(titleBuilder);
        itemBuilder.InnerHtml.AppendHtml(linkBuilder);         

        return itemBuilder;
    }
    private static TagBuilder RenderMenuItemParents(GetMenuByUserIdDto menuItem, List<GetMenuByUserIdDto> menuItems)
    {
        var itemBuilder = new TagBuilder("div");
        itemBuilder.AddCssClass("menu-item menu-accordion");
        itemBuilder.Attributes.Add("data-kt-menu-trigger", "click");

        var linkBuilder = new TagBuilder("span");
        linkBuilder.AddCssClass("menu-link");

        var iconBuilder = new TagBuilder("span");
        iconBuilder.AddCssClass("menu-icon");
        var iconTag = new TagBuilder("i");
        iconTag.AddCssClass(menuItem.Icon ?? "fa-default fs-6");
        iconBuilder.InnerHtml.AppendHtml(iconTag);

        var titleBuilder = new TagBuilder("span");
        titleBuilder.AddCssClass("menu-title");
        titleBuilder.InnerHtml.Append(menuItem.Name);

        var arrowBuilder = new TagBuilder("span");
        arrowBuilder.AddCssClass("menu-arrow");

        linkBuilder.InnerHtml.AppendHtml(iconBuilder);
        linkBuilder.InnerHtml.AppendHtml(titleBuilder);
        linkBuilder.InnerHtml.AppendHtml(arrowBuilder);
        itemBuilder.InnerHtml.AppendHtml(linkBuilder);

        if (menuItems.Any())
        {
            var subMenuBuilder = new TagBuilder("div");
            subMenuBuilder.AddCssClass("menu-sub menu-sub-accordion menu-active-bg");
            subMenuBuilder.Attributes.Add("style", "display: none; overflow: hidden;");

            foreach (var subMenuItem in menuItems
                                        .Where(a => a.ParentMenuId.Equals(Convert.ToInt32(menuItem.MenuId)))
                                        .OrderBy(x => x.Position).ToList())
            {
                oListMenuId.Add(Convert.ToInt32(subMenuItem.MenuId));

                subMenuBuilder.InnerHtml.AppendHtml(RenderMenuItem(subMenuItem));
            }

            itemBuilder.InnerHtml.AppendHtml(subMenuBuilder);
        }

        return itemBuilder;
    }

}
