using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs;

public class CurrentUserDto
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string LastName { get; set; }
    public string Number { get; set; }
    public List<string> RolIdList { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool Enable { get; set; }
    public bool ResetFlag { get; set; }
    public int EmployeeId { get; set; }
    public bool IsValid { get; set; }
    public string PasswordHash { get; set; }
    public List<int> PermissionNumberList { get; set; }
    public List<GetMenuByUserIdDto> MenuOptions { get; set; }
}
