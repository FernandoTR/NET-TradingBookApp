using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums;

/// <summary>
/// Lista de permisos.
/// </summary>
public enum Permissions
{
    [Description("Empleados")]
    Employees = 1,
    [Description("Perfiles")]
    Roles = 2,
    [Description("Usuarios")]
    Users = 3,
    [Description("Eventos")]
    Logs = 4,
    [Description("Categoria")]
    Category = 5,
    [Description("Figura")]
    CatFigure = 6,
    [Description("TradingBook")]
    TradingBook = 7,
    [Description("TradingBookAnalyticsTrigger")]
    TradingBookAnalyticsTrigger = 8,
    [Description("TradingBookAnalyticsScenery")]
    TradingBookAnalyticsScenery = 9,
    [Description("TradingBookAnalyticsFigure")]
    TradingBookAnalyticsFigure = 10,
    [Description("TradingBookAnalyticsDay")]
    TradingBookAnalyticsDay = 11,
    [Description("TradingBookAnalyticsDirection")]
    TradingBookAnalyticsDirection = 12,
    [Description("TradingBookAnalyticsStage")]
    TradingBookAnalyticsStage = 13,
    [Description("TradingBookAnalyticsTime")]
    TradingBookAnalyticsTime = 14
}
