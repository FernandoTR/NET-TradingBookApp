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
            badge = '<div class="kt-badge kt-badge-danger">Back Testing</div>';
            break;
        case "Paper Trading":
            badge = '<div class="kt-badge kt-badge-warning">Paper Trading</div>';
            break;
        case "Real Trading":
            badge = '<div class="kt-badge kt-badge-success">Real Trading</div>';
            break;
        default:
            badge = '<div class="kt-badge kt-badge-warning">Desconocido</div>';
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
        badge = '<span class="text-white min-w-60px d-block text-end fw-bold fs-6">' + formattedAmount +'</span>';
    } else {
        badge = '<span class="text-white min-w-60px d-block text-end fw-bold fs-6">' + formattedAmount + '</span>';
    }
    
    return badge;
};

// Funcion para cambiar la etiqueta dependiendo del valor booleano del dato obtenido por el dataTable
var renderStatusAnalytics = function (data) {
    return data ?
        '<div class="kt-badge kt-badge-success">Válido</div>' :
        '<div class="kt-badge kt-badge-warning">Pausa</div>';
}



/**
 * Función para generar una barra de progreso con porcentaje.
 * @param {number|string} data - El valor del porcentaje a mostrar.
 * @param {string} backgroundColor - Clase CSS para el color de fondo de la barra.
 * @returns {string} - HTML de la barra de progreso.
 */
const renderProgressBar = (data, backgroundColor) => {
    const value = parseFloat(data).toFixed(0); 

    return `
    <span class="kt-badge kt-badge-light   kt-badge-${backgroundColor} w-[40px]">${value}%</span
       
    `;
};

// Funciones específicas para diferentes tipos de barras de progreso dependiendo del valor del dato obtenido por el dataTable
const renderSLPChart = (data) => renderProgressBar(data, 'destructive');
const renderTP1PChart = (data) => renderProgressBar(data, 'success');
const renderTP2PChart = (data) => renderProgressBar(data, 'warning');
const renderTP3PChart = (data) => renderProgressBar(data, 'primary');