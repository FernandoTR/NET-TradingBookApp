using Infrastructure;

namespace Application.DTOs;

public class EmployeeDto
{
    public required Employee Employee { get; set; }
    public AspNetUser? AspNetUser { get; set; }
}
