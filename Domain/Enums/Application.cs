using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums;

public enum Application
{
    [Description("Desconocido")]
    Unknow = 0,
    [Description("WebAppBase")]
    WebAppBase = 1
}