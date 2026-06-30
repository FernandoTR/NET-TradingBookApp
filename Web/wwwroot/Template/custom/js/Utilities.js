function Mayus(e) {
    e.value = e.value.toUpperCase();
}

function changeHtmlLetters(str) {
    return str.replace('&#193;', 'Á')
        .replace('&#201;', 'É')
        .replace('&#205;', 'Í')
        .replace('&#211;', 'Ó')
        .replace('&#218;', 'Ú');
}


var validationPasswordRegexp = /^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/;

// Funcion para cambiar la etiqueta del empleado dependiendo del estatus que se asigna en el dataTable
var renderStatusEmployee = function (data) {
    return data == "Activo" ?
        '<div class="kt-badge kt-badge-success">Activo</div>' :
        '<div class="kt-badge kt-badge-destructive">Inactivo</div>';
}

// Funcion para cambiar la etiqueta dependiendo del valor booleano del dato obtenido por el dataTable
var renderStatus = function (data) {
    return data ?
        '<div class="kt-badge kt-badge-success">Activo</div>' :
        '<div class="kt-badge kt-badge-destructive">Inactivo</div>';
}

// Funcion para cambiar la etiqueta dependiendo del valor booleano del dato obtenido por el dataTable
var renderTrueFalse = function (data) {
    return data == true ?
        '<i class="ki-duotone ki-verify text-green-600 text-2xl"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span></i>' :
        '<i class="ki-duotone ki-minus-circle text-destructive text-2xl"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span></i>';
}

// Funcion para cambiar el icono de bandera dependiendo del valor booleano del dato obtenido por el dataTable
var renderFlag = function (data) {
    return data == true ?
        '<i class="ki-duotone ki-flag text-green-600 text-2xl"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span></i>' :
        '<i class="ki-duotone ki-flag text-destructive text-2xl"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span></i>';
}

// Funcion para colocar imagen dependiendo del valor del dato obtenido por el dataTable
var renderIconCoin = function (data) {
    return '<img alt="IconCoin" class="rounded-full size-9 shrink-0" src="' + data + '">';
}

// Funcion para cambiar la etiqueta del tipo de cuenta dependiendo del valor que se asigna en el dataTable
var renderAccountType = function (data) {
    let badge;
    switch (data) {
        case "Back Testing":
            badge = '<div class="kt-badge kt-badge-light kt-badge-destructive">Back Testing</div>';
            break;
        case "Paper Trading":
            badge = '<div class="kt-badge kt-badge-light kt-badge-warning">Paper Trading</div>';
            break;
        case "Real Trading":
            badge = '<div class="kt-badge kt-badge-light kt-badge-success">Real Trading</div>';
            break;
        default:
            badge = '<div class="kt-badge kt-badge-light kt-badge-warning">Desconocido</div>';
            break;
    }
    return badge;
};

// Funcion para cambiar la etiqueta del tipo de cuenta dependiendo del valor que se asigna en el dataTable
var renderAccountBalance= function (data) {
    let badge;
    const formattedAmount = new Intl.NumberFormat('es-MX', {
        style: 'currency',
        currency: 'MXN',
        minimumFractionDigits: 2,
    }).format(parseFloat(data));

    if (parseFloat(data) >= 0) {
        badge = '<span class="text-sm font-medium text-secondary-foreground text-end">' + formattedAmount +'</span>';
    } else {
        badge = '<span class="text-sm font-medium text-secondary-foreground text-end">' + formattedAmount + '</span>';
    }
    
    return badge;
};

// Funcion para cambiar la etiqueta dependiendo del valor booleano del dato obtenido por el dataTable
var renderStatusAnalytics = function (data) {
    return data ?
        `<div class="relative size-[44px] shrink-0" >            
             <div class="absolute leading-none start-2/4 top-2/4 -translate-y-2/4 -translate-x-2/4 rtl:translate-x-2/4" >
              <i class="ki-filled ki-check-circle text-md ps-px text-green-600">
              </i>
             </div>
        </div>` :
        `<div class="relative size-[44px] shrink-0" >
            <div class="absolute leading-none start-2/4 top-2/4 -translate-y-2/4 -translate-x-2/4 rtl:translate-x-2/4"  >
                <i class="ki-filled ki-minus-circle text-md ps-px text-yellow-600">
                </i>
            </div>              
        </div>`;
}

// Funcion para colocar icono centrado en lugar de texto para valores booleanos o estatus 1/2
var renderBooleanStatusIcon = function (data) {
    let badge;
    const normalizedValue = typeof data === 'string' ? data.trim().toLowerCase() : data;
    const isActive = normalizedValue === true || normalizedValue === 1 || normalizedValue === 'true' || normalizedValue === '1' || normalizedValue === 'activo' || normalizedValue === 'habilitado';
    const isInactive = normalizedValue === false || normalizedValue === 0 || normalizedValue === 2 || normalizedValue === 'false' || normalizedValue === '0' || normalizedValue === '2' || normalizedValue === 'inactivo' || normalizedValue === 'deshabilitado';

    if (isActive) {
        badge = `<div class="relative size-[44px] shrink-0" >
                     <div class="absolute leading-none start-2/4 top-2/4 -translate-y-2/4 -translate-x-2/4 rtl:translate-x-2/4" >
                         <i class="ki-filled ki-check-circle text-md ps-px text-green-600">
                         </i>
                     </div>
                 </div>`;
    } else if (isInactive) {
        badge = `<div class="relative size-[44px] shrink-0" >
                     <div class="absolute leading-none start-2/4 top-2/4 -translate-y-2/4 -translate-x-2/4 rtl:translate-x-2/4" >
                         <i class="ki-filled ki-minus-circle text-md ps-px text-destructive">
                         </i>
                     </div>
                 </div>`;
    } else {
        badge = ``;
    }

    return badge;
};

// Funcion para renderizar columnas Activo/Inactivo con icono
var renderActiveIcon = function (data) {
    return renderBooleanStatusIcon(data);
};

// Funcion para renderizar columnas Estado/Habilitado con icono
var renderEnabledIcon = function (data) {
    return renderBooleanStatusIcon(data);
};



/**
 * Función para generar una barra de progreso con porcentaje.
 * @param {number|string} data - El valor del porcentaje a mostrar.
 * @param {string} backgroundColor - Clase CSS para el color de fondo de la barra.
 * @returns {string} - HTML de la barra de progreso.
 */
const renderProgressBar = (value, valuePercent, color, opacity = '') => {
    const percent = parseFloat(valuePercent).toFixed(0); 

    return `
    <div class="flex flex-col gap-1 min-w-[90px]">

        <div class="flex items-center justify-between">
            
            <span class="text-xs font-medium text-white">
                ${value}
            </span>

            <span class="text-[11px] text-muted-foreground">
                ${percent}%
            </span>

        </div>

        <div class="kt-progress h-[3px] bg-white/10 kt-progress-${color} ${opacity}">
            
            <div 
                class="kt-progress-indicator rounded-full transition-all duration-700 ease-out"
                data-width="${percent}"
                style="width:0%">
            </div>

        </div>

    </div>
`;
};
// Funciones específicas para diferentes tipos de barras de progreso dependiendo del valor del dato obtenido por el dataTable
const renderSLPChart = (data, type, row) => renderProgressBar(row.sl, row.slp, 'destructive');
const renderTP1PChart = (data, type, row) => renderProgressBar(row.tP1, row.tP1P, 'primary', 'opacity-90');
const renderTP2PChart = (data, type, row) => renderProgressBar(row.tP2, row.tP2P, 'primary', 'opacity-80');
const renderTP3PChart = (data, type, row) => renderProgressBar(row.tP3, row.tP3P, 'primary', 'opacity-60');

// Funcion para cambiar la etiqueta del tipo de cuenta dependiendo del valor que se asigna en el dataTable
var renderAccountTypeCode = function (data) {    
    let color;

    switch (data) {
        case "BT":
            color = "destructive";
            break;
        case "PT":
            color = "warning";
            break;
        case "RT":
            color = "success";
            break;
        default:
            color = "primary";
            break;
    }

    return `<div class="kt-badge kt-badge-light kt-badge-${color} opacity-60">${data}</div>`;
};

/**
 * Función para representar SETUP en dataTable
 * @param {string} stageName - El valor de la ETAPA a mostrar.
 * @param {string} figureName - El valor de la FIGURA a mostrar.
 * @param {string} triggerName - El valor de la GATILLO a mostrar.
 * @param {string} sceneryName - El valor de la ESCENARIO a mostrar.
 * @returns {string} - HTML de la barra de progreso.
 */
const renderSetup = (stageName, figureName, triggerName, sceneryName) => {  

    return `${stageName} · ${figureName} · ${triggerName} · ${sceneryName}`;
};

const renderSetupDT = (data, type, row) => renderSetup(row.stageName, row.figureName, row.triggerName, row.sceneryName);

/**
 * Función para representar OPERACION en dataTable
 * @param {number} directionId - El Id de la DIRECCION a mostrar.
 * @param {string} frameName - El valor deL FRAME a mostrar.
 * @param {string} directionName - El valor deL BLOQUE a mostrar. * 
 * @returns {string} - HTML de la barra de progreso.
 */
const renderOperation = (directionId, frameName, directionName) => {
    let label;

    switch (directionId) {
        case 1:
            label = `<i class="ki-filled ki-arrow-up text-green-500"></i><span>${frameName}</span>`;
            break;
        case 2:
            label = ` <i class="ki-filled ki-arrow-down text-destructive"></i><span>${frameName}</span>`;
            break;
        default:
            label = ` ${directionName}`;
            break;
    }

    return `<span class="lg:text-right">
               ${label}
            </span>`;
};

const renderOperationDT = (data, type, row) => renderOperation(row.directionId, row.frameName, row.directionName);



/**
* Función para representar RESULTADO en dataTable
* @param {number} directionId - El Id de la DIRECCION a mostrar.
* @param {string} sl - El valor deL SL a mostrar.
* @param {string} tP1 - El valor deL TP1 a mostrar. * 
* @param {string} tP2 - El valor deL TP2 a mostrar. * 
* @param {string} tP3 - El valor deL TP3 a mostrar. *
* @param {string} target - El valor deL TARGET a mostrar. * 
* @returns {string} - HTML de la barra de progreso.
*/
const renderResult = (directionId, sl, tP1, tP2, tP3, target) => {

    if (directionId == 0) 
        return `
               <div class="flex flex-wrap gap-2.5 justify-center">
                  <span class="kt-badge kt-badge-light kt-badge-destructive opacity-90">${sl}</span>
                  <span class="kt-badge kt-badge-light kt-badge-primary opacity-90">${tP1}</span>
                  <span class="kt-badge kt-badge-light kt-badge-primary opacity-90">${tP2}</span>
                  <span class="kt-badge kt-badge-light kt-badge-primary opacity-90">${tP3}</span>
                </div>

            `;



    const tpHit = [tP1, tP2, tP3].filter(x => x == 1).length;
   
    if (sl == 1)
        return `<span class="kt-badge kt-badge-stroke">SL</span><span class="text-destructive"> -${target}</span> `;

    return `<span class="kt-badge kt-badge-stroke">${tpHit}/3 TP</span><span class="text-green-500 opacity-90"> +${target}</span> `; 

};

const renderResultDT = (data, type, row) => renderResult(row.directionId, row.slStyle, row.tP1Style, row.tP2Style, row.tP3Style, row.target);

// Funcion para cambiar la etiqueta del tipo de cuenta dependiendo del valor que se asigna en el dataTable
var renderOrderStatus = function (data) {
    let badge;

    switch (data) {
        case 1:
            badge = `<div class="relative size-[44px] shrink-0" >            
                         <div class="absolute leading-none start-2/4 top-2/4 -translate-y-2/4 -translate-x-2/4 rtl:translate-x-2/4" >
                          <i class="ki-filled ki-check-circle text-md ps-px text-green-600">
                          </i>
                         </div>
                    </div>`;
            break;
        case 2:
            badge = `<div class="relative size-[44px] shrink-0" bis_skin_checked="1">
                        <div class="absolute leading-none start-2/4 top-2/4 -translate-y-2/4 -translate-x-2/4 rtl:translate-x-2/4" bis_skin_checked="1">
                            <i class="ki-filled ki-minus-circle text-md ps-px text-destructive">
                            </i>
                        </div>              
                    </div>`;
            break;
        default:
            badge = ``;
            break;
    }
    return badge;
};
