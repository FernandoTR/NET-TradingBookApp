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
        builder.AddCssClass("kt-menu flex flex-col grow gap-1");
        builder.Attributes.Add("id", "sidebar_menu");
        builder.Attributes.Add("data-kt-menu", "true");
        builder.Attributes.Add("data-kt-menu-accordion-expand-all", "false");


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
        itemBuilder.AddCssClass("kt-menu-item pt-2.25 pb-px");

        //var menuContentBuilder = new TagBuilder("div");
        //menuContentBuilder.AddCssClass("kt-menu-content");

        var tittleBuilder = new TagBuilder("span");
        tittleBuilder.AddCssClass("kt-menu-heading uppercase text-xs font-medium text-muted-foreground ps-[10px] pe-[10px]");
        tittleBuilder.InnerHtml.Append(menuItem.Name);

        //menuContentBuilder.InnerHtml.AppendHtml(tittleBuilder);
        itemBuilder.InnerHtml.AppendHtml(tittleBuilder);        

        return itemBuilder;
    }
    private static TagBuilder RenderMenuItem(GetMenuByUserIdDto menuItem)
    {
        var itemBuilder = new TagBuilder("div");
        itemBuilder.AddCssClass("kt-menu-item");

        var linkBuilder = new TagBuilder("a");
        linkBuilder.AddCssClass("kt-menu-link gap-[10px] ps-[10px] pe-[10px] py-[6px] border border-transparent kt-menu-item-active:bg-accent/60 dark:menu-item-active:border-border kt-menu-item-active:rounded-lg hover:bg-accent/60 hover:rounded-lg");
        if (!string.IsNullOrEmpty(menuItem.URL))
        {
            linkBuilder.Attributes.Add("href", menuItem.URL);
        }

        var iconBuilder = new TagBuilder("span");
        iconBuilder.AddCssClass("kt-menu-icon items-start text-muted-foreground kt-menu-item-active:text-primary kt-menu-link-hover:!text-primary w-[20px]");
        var iconTag = new TagBuilder("i");
        iconTag.AddCssClass(menuItem.Icon ?? "ki-filled ki-calendar-tick text-lg");
        iconBuilder.InnerHtml.AppendHtml(iconTag);

        var titleBuilder = new TagBuilder("span");
        titleBuilder.AddCssClass("kt-menu-title text-sm font-medium text-foreground kt-menu-item-active:text-primary kt-menu-link-hover:!text-primary");
        titleBuilder.InnerHtml.Append(menuItem.Name);

        linkBuilder.InnerHtml.AppendHtml(iconBuilder);
        linkBuilder.InnerHtml.AppendHtml(titleBuilder);
        itemBuilder.InnerHtml.AppendHtml(linkBuilder);         

        return itemBuilder;
    }
    private static TagBuilder RenderMenuItemParents(GetMenuByUserIdDto menuItem, List<GetMenuByUserIdDto> menuItems)
    {
        var itemBuilder = new TagBuilder("div");
        itemBuilder.AddCssClass("kt-menu-item");
        itemBuilder.Attributes.Add("data-kt-menu-item-toggle", "accordion");
        itemBuilder.Attributes.Add("data-kt-menu-item-trigger", "click");


        var linkBuilder = new TagBuilder("span");
        linkBuilder.AddCssClass("kt-menu-link flex items-center grow cursor-pointer border border-transparent gap-[10px] ps-[10px] pe-[10px] py-[6px]");
        linkBuilder.Attributes.Add("tabindex", "0");

        var iconBuilder = new TagBuilder("span");
        iconBuilder.AddCssClass("kt-menu-icon items-start text-muted-foreground w-[20px]");
        var iconTag = new TagBuilder("i");
        iconTag.AddCssClass(menuItem.Icon ?? "ki-filled ki-element-11 text-lg");
        iconBuilder.InnerHtml.AppendHtml(iconTag);

        var titleBuilder = new TagBuilder("span");
        titleBuilder.AddCssClass("kt-menu-title text-sm font-medium text-foreground kt-menu-item-active:text-primary kt-menu-link-hover:!text-primary");
        titleBuilder.InnerHtml.Append(menuItem.Name);

        var arrowBuilder = new TagBuilder("span");
        arrowBuilder.AddCssClass("kt-menu-arrow text-muted-foreground w-[20px] shrink-0 justify-end ms-1 me-[-10px]");

        var arrowBuilderMenu1 = new TagBuilder("span");
        arrowBuilderMenu1.AddCssClass("inline-flex kt-menu-item-show:hidden");
        var iconTagMenu1 = new TagBuilder("i");
        iconTagMenu1.AddCssClass("ki-filled ki-plus text-[11px]");
        arrowBuilderMenu1.InnerHtml.AppendHtml(iconTagMenu1);

        var arrowBuilderMenu2 = new TagBuilder("span");
        arrowBuilderMenu2.AddCssClass("hidden kt-menu-item-show:inline-flex");
        var iconTagMenu2 = new TagBuilder("i");
        iconTagMenu2.AddCssClass("ki-filled ki-minus text-[11px]\"");
        arrowBuilderMenu2.InnerHtml.AppendHtml(iconTagMenu2);

        arrowBuilder.InnerHtml.AppendHtml(arrowBuilderMenu1);
        arrowBuilder.InnerHtml.AppendHtml(arrowBuilderMenu2);




        linkBuilder.InnerHtml.AppendHtml(iconBuilder);
        linkBuilder.InnerHtml.AppendHtml(titleBuilder);
        linkBuilder.InnerHtml.AppendHtml(arrowBuilder);
        itemBuilder.InnerHtml.AppendHtml(linkBuilder);

        if (menuItems.Any())
        {
            var subMenuBuilder = new TagBuilder("div");
            subMenuBuilder.AddCssClass("kt-menu-accordion gap-1 ps-[10px] relative before:absolute before:start-[20px] before:top-0 before:bottom-0 before:border-s before:border-border");
            //subMenuBuilder.Attributes.Add("style", "display: none; overflow: hidden;");

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
