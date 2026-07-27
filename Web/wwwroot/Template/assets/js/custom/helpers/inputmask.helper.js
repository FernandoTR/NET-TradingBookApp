window.InputMaskHelper = (() => {

    /**
     * Decimal / currency inputs
     */
    const decimal = (selector, options = {}) => {

        const defaultOptions = {
            alias: 'decimal',
            groupSeparator: ',',
            digits: 4,
            digitsOptional: false,
            rightAlignNumerics: false,
            allowMinus: false,
            autoUnmask: true,
            removeMaskOnSubmit: true,
            autoGroup: true,
            placeholder: '0'
        };

        const config = {
            ...defaultOptions,
            ...options
        };

        Inputmask(config).mask(selector);
    };

    /**
     * Percentage inputs
     */
    const percentage = (selector, options = {}) => {

        const defaultOptions = {
            alias: 'percentage',
            digits: 2,
            digitsOptional: false,
            rightAlignNumerics: false,
            autoUnmask: true,
            removeMaskOnSubmit: true
        };

        const config = {
            ...defaultOptions,
            ...options
        };

        Inputmask(config).mask(selector);
    };

    /**
     * Integer inputs
     */
    const integer = (selector, options = {}) => {

        const defaultOptions = {
            alias: 'integer',
            groupSeparator: ',',
            rightAlignNumerics: false,
            autoGroup: true,
            autoUnmask: true,
            removeMaskOnSubmit: true
        };

        const config = {
            ...defaultOptions,
            ...options
        };

        Inputmask(config).mask(selector);
    };

    return {
        decimal,
        percentage,
        integer
    };

})();