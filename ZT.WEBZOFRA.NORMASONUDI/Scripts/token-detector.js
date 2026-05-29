/*
   token-detector.js
   Incluye en la pagina con:
   <script src="/Scripts/token-detector.js"></script>
*/

(function () {
    'use strict';

    var CONFIG = {
        endpoint: '/Handlers/TokenStatus.ashx',
        intervalo: 3000,
        ddlId: 'DdlCertificados',
        bannerId: 'tokenBanner',
        ultimosThumbs: null
    };

    function crearBanner() {
        if (document.getElementById(CONFIG.bannerId)) return;
        var banner = document.createElement('div');
        banner.id = CONFIG.bannerId;
        banner.style.cssText = [
            'display:none',
            'padding:10px 16px',
            'border-radius:6px',
            'font-size:14px',
            'font-weight:bold',
            'margin-bottom:12px',
            'transition:all .3s ease',
            'border:1px solid transparent'
        ].join(';');

        var form = document.getElementById('form1') || document.body;
        form.insertBefore(banner, form.firstChild);
    }

    function mostrarBanner(estado, msg) {
        var banner = document.getElementById(CONFIG.bannerId);
        if (!banner) return;

        var estilos = {
            ok:        { bg: '#d4edda', border: '#28a745', color: '#155724' },
            warn:      { bg: '#fff3cd', border: '#ffc107', color: '#856404' },
            sin_token: { bg: '#f8d7da', border: '#dc3545', color: '#721c24' },
            escaneando:{ bg: '#e2e3e5', border: '#adb5bd', color: '#383d41' }
        };

        var s = estilos[estado] || estilos.escaneando;
        banner.style.display = 'block';
        banner.style.background = s.bg;
        banner.style.borderColor = s.border;
        banner.style.color = s.color;
        banner.innerHTML = msg;
    }

    function getDdl() {
        var el = document.getElementById(CONFIG.ddlId);
        if (el) return el;

        var todos = document.querySelectorAll('select');
        for (var i = 0; i < todos.length; i++) {
            var id = (todos[i].id || '').toLowerCase();
            if (id.indexOf('ddlcertificados') !== -1) return todos[i];
        }
        return null;
    }

    function actualizarDropdown(certs) {
        var ddl = getDdl();
        if (!ddl) return;

        var selActual = ddl.value;

        ddl.innerHTML = '';
        var opDefault = document.createElement('option');
        opDefault.value = '';
        opDefault.textContent = certs.length === 0
            ? '-- Sin certificados disponibles --'
            : '-- Seleccione un certificado --';
        ddl.appendChild(opDefault);

        certs.forEach(function (cert) {
            var op = document.createElement('option');
            op.value = cert.thumbprint;
            op.textContent = cert.label;
            if (cert.esToken) op.textContent = '[Token] ' + cert.label;
            ddl.appendChild(op);
        });

        if (selActual) {
            var encontrado = certs.some(function (c) { return c.thumbprint === selActual; });
            if (encontrado) ddl.value = selActual;
        }
    }

    function thumbsIguales(certs) {
        var thumbsNuevos = certs.map(function (c) { return c.thumbprint; }).sort().join(',');
        if (CONFIG.ultimosThumbs === null) {
            CONFIG.ultimosThumbs = thumbsNuevos;
            return true;
        }
        var igual = CONFIG.ultimosThumbs === thumbsNuevos;
        CONFIG.ultimosThumbs = thumbsNuevos;
        return igual;
    }

    function consultarToken() {
        var xhr = new XMLHttpRequest();
        xhr.open('GET', CONFIG.endpoint + '?t=' + Date.now(), true);
        xhr.timeout = 2500;

        xhr.onload = function () {
            if (xhr.status !== 200) return;
            try {
                var data = JSON.parse(xhr.responseText);
                if (!data.ok) return;

                var certs = data.certs || [];
                thumbsIguales(certs);

                var tokens = certs.filter(function (c) { return c.esToken; });
                var soft = certs.filter(function (c) { return !c.esToken; });

                actualizarDropdown(certs);

                if (tokens.length > 0) {
                    var nombres = tokens.map(function (t) { return '<strong>' + t.titular + '</strong>'; }).join(', ');
                    mostrarBanner('ok', 'Token USB detectado: ' + nombres + ' | Listo para firmar');
                } else if (certs.length > 0) {
                    mostrarBanner('warn', 'Certificados detectados. El token se validara al firmar para evitar pedir PIN.');
                } else {
                    mostrarBanner('sin_token', 'No se detectaron certificados. Conecta el token USB y espera unos segundos.');
                }
            } catch (e) {
                // JSON invalido, ignorar
            }
        };

        xhr.onerror = function () { };
        xhr.ontimeout = function () { };
        xhr.send();
    }

    document.addEventListener('DOMContentLoaded', function () {
        crearBanner();
        consultarToken();
        setInterval(consultarToken, CONFIG.intervalo);
    });
})();
