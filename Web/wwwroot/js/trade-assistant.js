(function () {
    const maxImages = 4;
    const minImages = 1;
    const maxImageBytes = 8 * 1024 * 1024;
    const maxTotalBytes = 32 * 1024 * 1024;
    const allowedTypes = ['image/jpeg', 'image/png', 'image/webp'];

    document.addEventListener('DOMContentLoaded', function () {
        const root = document.querySelector('[data-trade-assistant]');
        if (!root) {
            return;
        }

        const form = root.querySelector('#trade-assistant-form');
        const list = root.querySelector('[data-image-list]');
        const template = root.querySelector('#trade-assistant-image-template');
        const addButton = root.querySelector('[data-add-image]');
        const limitMessage = root.querySelector('[data-image-limit]');
        const errorContainer = root.querySelector('[data-image-errors]');

        if (!form || !list || !template || !addButton || !errorContainer) {
            return;
        }

        addButton.addEventListener('click', function () {
            const rows = getRows(list);

            if (rows.length >= maxImages) {
                showLimit(limitMessage);
                return;
            }

            const index = rows.length;
            const html = template.innerHTML
                .split('__displayIndex__').join((index + 1).toString())
                .split('__index__').join(index.toString());

            list.insertAdjacentHTML('beforeend', html);
            refreshRows(list, addButton, limitMessage);
            initializeSelects(list);
        });

        list.addEventListener('click', function (event) {
            const removeButton = event.target.closest('[data-remove-image]');
            if (!removeButton) {
                return;
            }

            const rows = getRows(list);
            if (rows.length <= minImages) {
                return;
            }

            removeButton.closest('[data-image-row]').remove();
            refreshRows(list, addButton, limitMessage);
            validateImages(list, errorContainer);
        });

        list.addEventListener('change', function (event) {
            if (event.target.matches('[data-image-file]')) {
                validateImages(list, errorContainer);
            }
        });

        form.addEventListener('submit', function (event) {
            if (!validateImages(list, errorContainer)) {
                event.preventDefault();
            }
        });

        refreshRows(list, addButton, limitMessage);
        initializeSelects(list);
    });

    function getRows(list) {
        return Array.from(list.querySelectorAll('[data-image-row]'));
    }

    function refreshRows(list, addButton, limitMessage) {
        const rows = getRows(list);

        rows.forEach(function (row, index) {
            const displayIndex = index + 1;
            const title = row.querySelector('[data-image-title]');
            const sortOrder = row.querySelector('[data-sort-order]');
            const removeButton = row.querySelector('[data-remove-image]');

            if (title) {
                title.textContent = 'Imagen ' + displayIndex;
            }

            if (sortOrder) {
                sortOrder.value = displayIndex.toString();
            }

            if (removeButton) {
                removeButton.disabled = rows.length <= minImages;
            }

            reindexFields(row, index);
        });

        addButton.disabled = rows.length >= maxImages;

        if (rows.length < maxImages && limitMessage) {
            limitMessage.hidden = true;
        }
    }

    function reindexFields(row, index) {
        row.querySelectorAll('input, select, textarea, label').forEach(function (element) {
            if (element.name) {
                element.name = element.name.replace(/Images\[\d+\]/, 'Images[' + index + ']');
            }

            if (element.id) {
                element.id = element.id.replace(/Images_\d+__/, 'Images_' + index + '__');
            }

            if (element.htmlFor) {
                element.htmlFor = element.htmlFor.replace(/Images_\d+__/, 'Images_' + index + '__');
            }
        });
    }

    function validateImages(list, errorContainer) {
        const rows = getRows(list);
        const errors = [];
        let totalBytes = 0;

        if (rows.length < minImages) {
            errors.push('Agrega al menos una imagen.');
        }

        if (rows.length > maxImages) {
            errors.push('El maximo es 4 imagenes por validacion.');
        }

        rows.forEach(function (row, index) {
            const fileInput = row.querySelector('[data-image-file]');
            const file = fileInput && fileInput.files ? fileInput.files[0] : null;

            if (!file) {
                errors.push('La imagen ' + (index + 1) + ' requiere un archivo.');
                return;
            }

            totalBytes += file.size;

            if (!allowedTypes.includes(file.type)) {
                errors.push('La imagen ' + (index + 1) + ' debe ser JPEG, PNG o WebP.');
            }

            if (file.size > maxImageBytes) {
                errors.push('La imagen ' + (index + 1) + ' supera 8 MB.');
            }
        });

        if (totalBytes > maxTotalBytes) {
            errors.push('El total de imagenes supera 32 MB.');
        }

        errorContainer.textContent = errors.join(' ');
        return errors.length === 0;
    }

    function showLimit(limitMessage) {
        if (!limitMessage) {
            return;
        }

        limitMessage.hidden = false;
    }

    function initializeSelects(container) {
        if (!window.KTSelect) {
            return;
        }

        if (typeof window.KTSelect.createInstances === 'function') {
            window.KTSelect.createInstances();
            return;
        }

        container.querySelectorAll('[data-kt-select="true"]').forEach(function (select) {
            if (!select.instance) {
                new window.KTSelect(select);
            }
        });
    }
})();
