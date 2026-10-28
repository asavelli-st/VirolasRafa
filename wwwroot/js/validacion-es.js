// jQuery Validation con números en formato argentino ("1.234,56") o con punto decimal, igual que el servidor.
(function ($) {
    if (!$ || !$.validator) return;

    $.validator.methods.number = function (value, element) {
        return this.optional(element) || !isNaN(window.leerNumero(value));
    };

    $.validator.methods.range = function (value, element, param) {
        if (this.optional(element)) return true;
        var n = window.leerNumero(value);
        return !isNaN(n) && n >= Number(param[0]) && n <= Number(param[1]);
    };

    $.validator.methods.min = function (value, element, param) {
        return this.optional(element) || window.leerNumero(value) >= Number(param);
    };

    $.validator.methods.max = function (value, element, param) {
        return this.optional(element) || window.leerNumero(value) <= Number(param);
    };
})(window.jQuery);
