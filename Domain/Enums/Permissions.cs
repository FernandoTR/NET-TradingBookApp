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
    CatCategory = 5,
    [Description("Figura")]
    CatFigure = 6,
    [Description("TradingBook")]
    TradingBook = 7,
    [Description("AnalyticsTrigger")]
    AnalyticsTrigger = 8,
    [Description("AnalyticsScenery")]
    AnalyticsScenery = 9,
    [Description("AnalyticsFigure")]
    AnalyticsFigure = 10,
    [Description("AnalyticsDay")]
    AnalyticsDay = 11,
    [Description("AnalyticsDirection")]
    AnalyticsDirection = 12,
    [Description("AnalyticsStage")]
    AnalyticsStage = 13,
    [Description("AnalyticsTime")]
    AnalyticsTime = 14,
    [Description("TipoCuenta")]
    CatAccountType = 15,
    [Description("Frame")]
    CatFrame = 16,
    [Description("Instrumentos")]
    CatInstruments = 17,
}
