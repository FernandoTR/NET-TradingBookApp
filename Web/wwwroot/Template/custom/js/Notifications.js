function ShowNotification(pType, pMessage) {
    const notificationTypes = {
        1: { icon: "info", confirmButton: "btn btn-info" },
        2: { icon: "success", confirmButton: "btn btn-success" },
        3: { icon: "warning", confirmButton: "btn btn-warning" },
        4: { icon: "error", confirmButton: "btn btn-danger" }
    };

    const { icon, confirmButton } = notificationTypes[pType] || { icon: "info", confirmButton: "btn btn-default" };

    KTToast.show({
        message: pMessage,
        progress: true,
        pauseOnHover: true,
        variant: icon,
        appearance: 'outline',
        size: 'lg',
        beep: true,
        duration: 10000,
        position: 'top-end',
        icon: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-info-icon lucide-info"><circle cx="12" cy="12" r="10"/><path d="M12 16v-4"/><path d="M12 8h.01"/></svg>`,
    });
    // Swal.fire({
    //     html: pMessage,
    //     icon,
    //     buttonsStyling: false,
    //     confirmButtonText: "Ok",
    //     customClass: {
    //         confirmButton
    //     }
    // });
}

function ConfigToastNotification() {
    // Configuración de notificaciones
    toastr.options = {
        progress: true,
        pauseOnHover: true,
        appearance: 'outline',
        size: 'lg',
        beep: true,
        duration: 10000,
        position: 'top-end',
        icon: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-info-icon lucide-info"><circle cx="12" cy="12" r="10"/><path d="M12 16v-4"/><path d="M12 8h.01"/></svg>`,
    };
}
