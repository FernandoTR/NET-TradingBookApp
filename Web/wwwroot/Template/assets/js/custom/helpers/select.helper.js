window.KTSelectHelper = (() => {

    function getElement(selector) {

        if (selector instanceof HTMLElement) {
            return selector;
        }

        return document.querySelector(selector);
    }

    function getInstance(element) {

        return KTSelect.getOrCreateInstance(element);
    }

    function dispatchNativeChange(element) {

        element.dispatchEvent(
            new Event('change', {
                bubbles: true
            })
        );
    }

    function setValue(selector, value) {

        const element = getElement(selector);

        if (!element) {
            console.warn('KTSelect element not found');
            return;
        }

        const instance = getInstance(element);

        if (!instance) {
            console.warn('KTSelect instance not found');
            return;
        }

        // limpiar selección previa
        instance.clearSelection();

        // seleccionar nuevo valor
        instance.toggleSelection(
            String(value)
        );

        // sincronizar select nativo
        element.value = String(value);

        dispatchNativeChange(element);
    }

    function setValues(selector, values = []) {

        const element = getElement(selector);

        if (!element) return;

        const instance = getInstance(element);

        if (!instance) return;

        instance.clearSelection();

        values.forEach(value => {

            instance.toggleSelection(
                String(value)
            );
        });

        dispatchNativeChange(element);
    }

    function clear(selector) {

        const element = getElement(selector);

        if (!element) return;

        const instance = getInstance(element);

        if (!instance) return;

        instance.clearSelection();

        element.value = '';

        dispatchNativeChange(element);
    }

    function getValue(selector) {

        const element = getElement(selector);

        if (!element) return null;

        return element.value;
    }

    function getValues(selector) {

        const element = getElement(selector);

        if (!element) return [];

        return Array
            .from(element.selectedOptions)
            .map(x => x.value);
    }

    function disable(selector) {

        const element = getElement(selector);

        if (!element) return;

        element.disabled = true;

        element.setAttribute(
            'data-kt-select-disabled',
            'true'
        );

        const instance = KTSelect.getInstance(element);

        if (instance?.disable) {
            instance.disable();
        }
    }

    function enable(selector) {

        const element = getElement(selector);

        if (!element) return;

        element.disabled = false;

        element.setAttribute(
            'data-kt-select-disabled',
            'false'
        );

        const instance = KTSelect.getInstance(element);

        if (instance?.enable) {
            instance.enable();
        }
    }

    return {

        setValue,
        setValues,
        clear,
        getValue,
        getValues,
        disable,
        enable
    };

})();