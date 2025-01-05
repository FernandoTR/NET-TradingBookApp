using System.ComponentModel;

namespace Domain.Enums;

public enum UserStatus
{
    [Description("Activo")]
    Active = 1,
    [Description("Inactivo")]
    Inactive = 0
}
