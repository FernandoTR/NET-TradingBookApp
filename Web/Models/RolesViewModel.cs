using System.ComponentModel;

namespace Web.Models;

public class RolesViewModel
{
    [DisplayName("Id")]
    public string RolId { get; set; }

    [DisplayName("Nombre")]
    public string NameRoles { get; set; }

    public string Task { get; set; }
    public List<int> listAccess { get; set; }
    public string listAccessString { get; set; }
}

public class AccessMenuViewModel
{
    public int MenuId { get; set; }
    public string Name { get; set; }
    public int? ParentMenuId { get; set; }
    public int? Position { get; set; }
    public bool IsSeleted { get; set; }

}

public class AccessMenuTreeViewModel
{
    public int id { get; set; }

    public string text { get; set; }

    //public bool @checked { get; set; }
    public JsTreeNodeState State { get; set; }


    public bool hasChildren { get; set; }

    public virtual List<AccessMenuTreeViewModel> children { get; set; }

}

public class JsTreeNodeState
{
    public bool Selected { get; set; } // Representa "state.selected" en el JSON
}