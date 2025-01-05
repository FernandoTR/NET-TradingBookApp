namespace Application.DTOs;

public class GetMenuByUserIdDto
{
    public Nullable<int> MenuId { get; set; }
    public string Name { get; set; }
    public string URL { get; set; }
    public string Icon { get; set; }
    public Nullable<int> ParentMenuId { get; set; }
    public Nullable<int> Position { get; set; }
}
