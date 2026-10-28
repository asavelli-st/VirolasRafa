// Selector de tema Auto / Claro / Oscuro. El tema inicial lo aplica un script en el <head> del layout.
(function () {
    var oscuro = matchMedia('(prefers-color-scheme: dark)');
    var iconos = { auto: 'fa-circle-half-stroke', light: 'fa-sun', dark: 'fa-moon' };

    function leer() {
        try { return localStorage.getItem('tema') || 'auto'; } catch (e) { return 'auto'; }
    }

    function aplicar(tema) {
        var efectivo = tema === 'light' || tema === 'dark' ? tema : (oscuro.matches ? 'dark' : 'light');
        document.documentElement.setAttribute('data-bs-theme', efectivo);
        document.querySelectorAll('[data-tema]').forEach(function (b) {
            var activo = b.getAttribute('data-tema') === tema;
            b.classList.toggle('active', activo);
            b.setAttribute('aria-pressed', activo);
        });
        var icono = document.getElementById('icono-tema');
        if (icono) icono.className = 'fa-solid ' + (iconos[tema] || iconos.auto);
    }

    document.querySelectorAll('[data-tema]').forEach(function (b) {
        b.addEventListener('click', function () {
            var tema = b.getAttribute('data-tema');
            try { localStorage.setItem('tema', tema); } catch (e) { }
            aplicar(tema);
        });
    });
    oscuro.addEventListener('change', function () { if (leer() === 'auto') aplicar('auto'); });
    aplicar(leer());
})();

// Confirmación antes de acciones irreversibles: <form data-confirmar="Mensaje" data-confirmar-boton="Texto">.
(function () {
    var elemento = document.getElementById('modal-confirmar');
    if (!elemento) return;
    var modal = bootstrap.Modal.getOrCreateInstance(elemento);
    var pendiente = null;

    document.addEventListener('submit', function (e) {
        var form = e.target;
        if (!form.hasAttribute('data-confirmar') || form.dataset.confirmado === '1') return;
        e.preventDefault();
        pendiente = form;
        document.getElementById('modal-confirmar-texto').textContent = form.getAttribute('data-confirmar');
        document.getElementById('modal-confirmar-ok').textContent = form.getAttribute('data-confirmar-boton') || 'Confirmar';
        modal.show();
    });

    document.getElementById('modal-confirmar-ok').addEventListener('click', function () {
        if (!pendiente) return;
        pendiente.dataset.confirmado = '1';
        modal.hide();
        pendiente.requestSubmit ? pendiente.requestSubmit() : pendiente.submit();
    });

    elemento.addEventListener('hidden.bs.modal', function () { pendiente = null; });
})();
