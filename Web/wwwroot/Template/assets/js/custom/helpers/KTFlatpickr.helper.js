/**
 * KTFlatpickrHelper
 * ------------------------------------------------------------
 * Helper reusable para Flatpickr
 * compatible con diseño Metronic/Tailwind
 * ------------------------------------------------------------
 */

window.KTFlatpickrHelper = (function () {

    const instances = {};

    const defaultConfig = {

        locale: "es",

        dateFormat: "d/m/Y",

        allowInput: false,

        static: false,

        disableMobile: true,

        monthSelectorType: "static",

        appendTo: document.body,

        position: "auto center",
    };

    /**
     * Inicializar Range Picker
     */
    function initRange(selector, options = {}) {

        const config = {
            ...defaultConfig,
            ...options
        };

        const element =
            document.querySelector(selector);

        if (!element) {

            console.warn(
                `KTFlatpickrHelper: No existe ${selector}`
            );

            return null;
        }

        /**
         * Destroy previo
         */
        destroy(selector);

        /**
         * Init
         */
        const instance = flatpickr(
            element,
            {
                ...config,

                appendTo:
                    config.appendTo,

                position:
                    config.position,

                mode: "range",

                onChange: function (
                    selectedDates,
                    dateStr,
                    instance
                ) {

                    if (
                        typeof config.onChange ===
                        "function"
                    ) {

                        config.onChange({
                            selectedDates,

                            formattedDates:
                                selectedDates.map(x =>
                                    moment(x)
                                        .format("DD/MM/YYYY")
                                ),

                            value:
                                dateStr,

                            instance
                        });
                    }
                }
            }
        );

        instances[selector] = instance;

        return instance;
    }

    /**
     * Inicializar Single Picker
     */
    function initSingle(selector, options = {}) {

        const config = {
            ...defaultConfig,
            ...options
        };

        const element =
            document.querySelector(selector);

        if (!element) {

            console.warn(
                `KTFlatpickrHelper: No existe ${selector}`
            );

            return null;
        }

        destroy(selector);

        const instance = flatpickr(
            element,
            {
                ...config,

                mode: "single",

                onChange: function (
                    selectedDates,
                    dateStr,
                    instance
                ) {

                    if (
                        typeof config.onChange ===
                        "function"
                    ) {

                        config.onChange({
                            selectedDates,

                            formattedDates:
                                selectedDates.map(x =>
                                    moment(x)
                                        .format("DD/MM/YYYY")
                                ),

                            value:
                                dateStr,

                            instance
                        });
                    }
                }
            }
        );

        instances[selector] = instance;

        return instance;
    }

    /**
     * Obtener rango
     */
    function getRange(selector) {

        const instance =
            instances[selector];

        if (!instance) return null;

        const dates =
            instance.selectedDates;

        if (dates.length < 2) {
            return null;
        }

        return {

            start:
                moment(dates[0]),

            end:
                moment(dates[1]),

            startFormatted:
                moment(dates[0])
                    .format("DD/MM/YYYY"),

            endFormatted:
                moment(dates[1])
                    .format("DD/MM/YYYY")
        };
    }

    /**
     * Obtener fecha single
     */
    function getDate(selector) {

        const instance =
            instances[selector];

        if (!instance) return null;

        const dates =
            instance.selectedDates;

        if (!dates.length) {
            return null;
        }

        return {

            date:
                moment(dates[0]),

            formatted:
                moment(dates[0])
                    .format("DD/MM/YYYY")
        };
    }

    /**
     * Set rango
     */
    function setRange(
        selector,
        start,
        end
    ) {

        const instance =
            instances[selector];

        if (!instance) return;

        instance.setDate([
            moment(start)
                .toDate(),

            moment(end)
                .toDate()
        ]);
    }

    /**
     * Set single
     */
    function setDate(
        selector,
        date
    ) {

        const instance =
            instances[selector];

        if (!instance) return;

        instance.setDate(
            moment(date)
                .toDate()
        );
    }

    /**
     * Limpiar
     */
    function clear(selector) {

        const instance =
            instances[selector];

        if (!instance) return;

        instance.clear();
    }

    /**
     * Destroy
     */
    function destroy(selector) {

        const instance =
            instances[selector];

        if (!instance) return;

        instance.destroy();

        delete instances[selector];
    }

    /**
     * Obtener instancia
     */
    function getInstance(selector) {

        return instances[selector] || null;
    }

    return {

        initRange,

        initSingle,

        getRange,

        getDate,

        setRange,

        setDate,

        clear,

        destroy,

        getInstance
    };

})();