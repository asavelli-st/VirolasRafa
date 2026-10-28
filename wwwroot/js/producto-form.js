// Formulario de producto (Admin): en cada escalón se carga margen o precio, y el otro se recalcula en vivo.
// El campo oculto data-usar guarda cuál editó el Admin por último; el servidor recalcula y valida al guardar.
(function () {
    var costo = document.getElementById('Costo');
    if (!costo) return;

    var grupos = Array.prototype.map.call(document.querySelectorAll('[data-escalon]'), function (g) {
        return {
            margen: g.querySelector('[data-margen]'),
            precio: g.querySelector('[data-precio]'),
            usar: g.querySelector('[data-usar]'),
        };
    });

    function valorCosto() {
        var c = window.leerNumero(costo.value);
        return isNaN(c) ? null : c;
    }

    function precioDesdeMargen(g) {
        var c = valorCosto(), m = window.leerNumero(g.margen.value);
        if (c === null || isNaN(m)) return;
        g.precio.value = window.formatearCampo(c * (1 + m / 100), 2);
    }

    function margenDesdePrecio(g) {
        var c = valorCosto(), p = window.leerNumero(g.precio.value);
        g.margen.value = c && !isNaN(p) ? window.formatearCampo((p - c) / c * 100, 2) : '';
    }

    grupos.forEach(function (g) {
        g.margen.addEventListener('input', function () { g.usar.value = 'margen'; precioDesdeMargen(g); });
        g.precio.addEventListener('input', function () { g.usar.value = 'precio'; margenDesdePrecio(g); });
    });

    // Al cambiar el costo se mantiene fijo el último campo editado de cada escalón.
    costo.addEventListener('input', function () {
        grupos.forEach(function (g) {
            if (g.usar.value === 'margen') precioDesdeMargen(g); else margenDesdePrecio(g);
        });
    });
})();
