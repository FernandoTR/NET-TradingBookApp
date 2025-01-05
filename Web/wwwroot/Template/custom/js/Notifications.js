function ShowNotification(pType, pMessage) {
    const notificationTypes = {
        1: { icon: "info", confirmButton: "btn btn-info" },
        2: { icon: "success", confirmButton: "btn btn-success" },
        3: { icon: "warning", confirmButton: "btn btn-warning" },
        4: { icon: "error", confirmButton: "btn btn-danger" }
    };

    const { icon, confirmButton } = notificationTypes[pType] || { icon: "info", confirmButton: "btn btn-default" };

    Swal.fire({
        html: pMessage,
        icon,
        buttonsStyling: false,
        confirmButtonText: "Ok",
        customClass: {
            confirmButton
        }
    });
}

function ConfigToastNotification() {
    // Configuración de notificaciones
    toastr.options = {
        closeButton: true,
        debug: false,
        newestOnTop: false,
        progressBar: true,
        positionClass: "toastr-top-right",
        preventDuplicates: false,
        onclick: null,
        showDuration: 300,
        hideDuration: 1000,
        timeOut: 5000,
        extendedTimeOut: 1000,
        showEasing: "swing",
        hideEasing: "linear",
        showMethod: "fadeIn",
        hideMethod: "fadeOut"
    };
}
