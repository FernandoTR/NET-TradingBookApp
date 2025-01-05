using System.ComponentModel;

namespace Domain.Enums;

public enum UserTypes
{
    [Description("Desconocido")]
    Unknow = 0,
    [Description("Usuario")]
    User = 1
}
