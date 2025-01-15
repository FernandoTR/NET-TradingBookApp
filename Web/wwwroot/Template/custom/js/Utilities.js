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
        '<div class="badge py-3 px-4 fs-7 badge-light-success">Activo</div>' :
        '<div class="badge py-3 px-4 fs-7 badge-light-danger">Inactivo</div>';
}

// Funcion para cambiar la etiqueta dependiendo del valor booleano del dato obtenido por el dataTable
var renderStatus = function (data) {
    return data ?
        '<div class="badge py-3 px-4 fs-7 badge-light-success">Activo</div>' :
        '<div class="badge py-3 px-4 fs-7 badge-light-danger">Inactivo</div>';
}

// Funcion para cambiar la etiqueta dependiendo del valor booleano del dato obtenido por el dataTable
var renderTrueFalse = function (data) {
    return data == true ?
        '<i class="ki-duotone ki-verify text-success fs-3x"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span></i>' :
        '<i class="ki-duotone ki-minus-circle text-danger fs-3x"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span></i>';
}

// Funcion para cambiar el icono de bandera dependiendo del valor booleano del dato obtenido por el dataTable
var renderFlag = function (data) {
    return data == true ?
        '<i class="ki-duotone ki-flag text-success fs-3x"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span></i>' :
        '<i class="ki-duotone ki-flag text-danger fs-3x"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span></i>';
}

// Funcion para colocar imagen dependiendo del valor del dato obtenido por el dataTable
var renderIconCoin = function (data) {
    return '<div class="symbol symbol-circle symbol-50px overflow-hidden me-3"><div class="symbol-label"><img src="' + data + '" alt="IconCoin" class="w-100"></div></div > ';
}

// Funcion para cambiar la etiqueta del tipo de cuenta dependiendo del valor que se asigna en el dataTable
var renderAccountType = function (data) {
    let badge;
    switch (data) {
        case "Back Testing":
            badge = '<div class="badge py-3 px-4 fs-7 badge-light-danger">Back Testing</div>';
            break;
        case "Paper Trading":
            badge = '<div class="badge py-3 px-4 fs-7 badge-light-warning">Paper Trading</div>';
            break;
        case "Real Trading":
            badge = '<div class="badge py-3 px-4 fs-7 badge-light-success">Real Trading</div>';
            break;
        default:
            badge = '<div class="badge py-3 px-4 fs-7 badge-light-warning">Desconocido</div>';
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
        badge = '<span class="text-success min-w-60px d-block text-end fw-bold fs-6">' + formattedAmount +'</span>';
    } else {
        badge = '<span class="text-danger min-w-60px d-block text-end fw-bold fs-6">' + formattedAmount + '</span>';
    }
    
    return badge;
};