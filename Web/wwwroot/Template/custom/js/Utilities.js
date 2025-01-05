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


// Funcion para cambiar la etiqueta del empleado dependiendo del estatus que se asigna en el dataTable
var renderStatusEmployee = function (data) {
    return data == "Activo" ?
        '<div class="badge py-3 px-4 fs-7 badge-light-success">Activo</div>' :
        '<div class="badge py-3 px-4 fs-7 badge-light-danger">Inactivo</div>';
}

// Funcion para cambiar la etiqueta dependiendo del valor booleano del dato obtenido por el dataTable
var renderTrueFalse = function (data) {
    return data == true ?
        '<i class="ki-duotone ki-verify text-success fs-3x"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span></i>' :
        '<i class="ki-duotone ki-minus-circle text-danger fs-3x"><span class="path1"></span><span class="path2"></span><span class="path3"></span><span class="path4"></span></i>';
}