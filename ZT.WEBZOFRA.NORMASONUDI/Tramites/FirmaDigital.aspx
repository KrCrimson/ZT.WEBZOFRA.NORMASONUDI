<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Shared/MasterPage.master" CodeFile="FirmaDigital.aspx.cs" Inherits="FirmaDigital" Title="Firma con DNI electronico" %>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PageTitle" runat="server">
    <h1 class="page-title">Firma con DNI electronico</h1>
</asp:Content>

<asp:Content ID="HeaderActions" ContentPlaceHolderID="HeaderActions" runat="server">
    <asp:LinkButton ID="LnkVolver" runat="server" OnClick="LnkVolver_Click" CssClass="btn btn-secondary">Volver al Detalle</asp:LinkButton>
</asp:Content>

<asp:Content ID="SidebarContent" ContentPlaceHolderID="SidebarContent" runat="server">
    <a href="Bandeja.aspx" class="nav-btn"><i class="ph ph-arrow-left"></i> Bandeja</a>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <asp:Panel ID="PnlFirma" runat="server">
            <div class="section-card">
                <div class="doc-summary" style="margin-bottom:1rem;">
                    <strong>Documento:</strong> <asp:Label ID="LblCodigoDoc" runat="server"></asp:Label><br />
                    <strong>Asunto:</strong> <asp:Label ID="LblAsunto" runat="server"></asp:Label>
                </div>

                <asp:Label ID="LblError" runat="server" CssClass="alert alert-danger" Visible="false" style="display:block; margin-bottom:1rem;"></asp:Label>

                <div id="tokenBanner"></div>

                <div class="field" style="margin-bottom:1rem;">
                    <label>Certificado disponible:</label>
                    <div style="display:flex; gap:0.6rem; align-items:center; flex-wrap:wrap;">
                        <asp:DropDownList ID="DdlCertificados" runat="server" CssClass="input-zf" style="min-width:320px;"></asp:DropDownList>
                        <asp:Button ID="BtnRefrescar" runat="server" Text="Refrescar" CssClass="btn btn-secondary"
                            OnClick="BtnRefrescar_Click" CausesValidation="false" />
                    </div>
                    <div class="input-hint">Conecte su DNIe y haga clic en Refrescar para actualizar la lista.</div>
                    <asp:Label ID="LblCertInfo" runat="server" CssClass="input-hint" />
                </div>

                <div class="field" style="margin-bottom:1rem;">
                    <label>Orientacion de la firma:</label>
                    <div style="display:flex; gap:1rem; align-items:center; flex-wrap:wrap;">
                        <label style="display:flex; gap:0.4rem; align-items:center; margin:0;">
                            <input type="radio" name="oriFirmaUsb" value="H" checked /> Horizontal
                        </label>
                        <label style="display:flex; gap:0.4rem; align-items:center; margin:0;">
                            <input type="radio" name="oriFirmaUsb" value="V" /> Vertical
                        </label>
                    </div>
                </div>

                <div class="field" style="margin-bottom:1rem;">
                    <label>Seleccione la posicion de la firma (arrastre el recuadro):</label>
                    <div id="pdfUsbContainer" style="border:1px solid #e2e8f0; border-radius:8px; background:#f8fafc; max-width:900px; max-height:520px; overflow:auto;">
                        <div id="pdfUsbPages" style="display:flex; flex-direction:column; gap:1rem; padding:12px;"></div>
                    </div>
                    <div id="pdfUsbNav" style="display:flex; gap:8px; align-items:center; margin-top:8px;">
                        <button type="button" class="btn btn-secondary" id="btnPrevPage">Anterior</button>
                        <span id="pdfUsbPageInfo" style="font-size:12px;color:#64748b;">Pagina 1 de 1</span>
                        <button type="button" class="btn btn-secondary" id="btnNextPage">Siguiente</button>
                    </div>
                    <div class="input-hint" style="margin-top:0.5rem;">Haga clic en la pagina donde desea firmar y arrastre el recuadro.</div>
                </div>

                <asp:Button ID="BtnFirmarDni" runat="server" Text="Firmar Documento" CssClass="btn" OnClick="BtnFirmarDni_Click" />
                <div class="input-hint" style="margin-top:0.5rem;">El dispositivo solicitara el PIN mediante su propio dialogo de seguridad.</div>

                <asp:Panel ID="PnlResultado" runat="server" Visible="false" CssClass="status" style="margin-top:1rem;">
                    <asp:Label ID="LblMensaje" runat="server" />
                </asp:Panel>
            </div>
        </asp:Panel>

        <asp:Panel ID="PnlExito" runat="server" Visible="false">
            <div class="success-panel">
                <div class="success-icon">OK</div>
                <h2>Exito! Firma Registrada</h2>
                <p style="color: #64748b; font-size: 1.1rem; margin-bottom: 2rem;">
                    <asp:Label ID="LblMensajeExito" runat="server"></asp:Label>
                </p>
                <asp:Button ID="BtnIrBandeja" runat="server" Text="Ir a mi Bandeja" OnClick="BtnIrBandeja_Click" CssClass="btn" style="padding: 0.8rem 2.5rem;" />
            </div>
        </asp:Panel>
    </div>

    <asp:HiddenField ID="HfPdfId" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="HfSigPage" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="HfSigX" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="HfSigY" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="HfSigW" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="HfSigH" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="HfSigOrient" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="HfSigRects" runat="server" ClientIDMode="Static" />

    <script src="https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.min.js"></script>
    <script src="/Scripts/token-detector.js"></script>
    <script type="text/javascript">
        (function() {
            var pdfjsLib = window['pdfjs-dist/build/pdf'];
            if (pdfjsLib) {
                pdfjsLib.GlobalWorkerOptions.workerSrc = "https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js";
            }

            var pdfId = document.getElementById('HfPdfId');
            var pagesHost = document.getElementById('pdfUsbPages');
            var hfX = document.getElementById('HfSigX');
            var hfY = document.getElementById('HfSigY');
            var hfW = document.getElementById('HfSigW');
            var hfH = document.getElementById('HfSigH');
            var hfOrient = document.getElementById('HfSigOrient');
            var hfPage = document.getElementById('HfSigPage');
            var hfRects = document.getElementById('HfSigRects');
            var btnPrev = document.getElementById('btnPrevPage');
            var btnNext = document.getElementById('btnNextPage');
            var pageInfo = document.getElementById('pdfUsbPageInfo');

            var pdfDoc = null;
            var currentPage = 1;
            var totalPages = 1;
            var scale = 1.3;
            var sigBox = null;
            var canvasEl = null;
            var wrapperEl = null;
            var pageWidthPts = 0;
            var pageHeightPts = 0;
            var existingRects = [];
            var dragging = false;
            var dragOffsetX = 0;
            var dragOffsetY = 0;
            var lastValidRect = null;

            var BOX_W = 200;
            var BOX_H = 60;

            function clamp(val, min, max) {
                return Math.max(min, Math.min(max, val));
            }

            function parseRects() {
                if (!hfRects || !hfRects.value) return [];
                try {
                    var parsed = JSON.parse(hfRects.value);
                    return Array.isArray(parsed) ? parsed : [];
                } catch (e) {
                    return [];
                }
            }

            function getBoxSize() {
                var ori = hfOrient && hfOrient.value ? hfOrient.value : 'H';
                return ori === 'V' ? { w: BOX_H, h: BOX_W } : { w: BOX_W, h: BOX_H };
            }

            function setHiddenFromRect(rectPts) {
                if (hfX) hfX.value = rectPts.x.toFixed(2);
                if (hfY) hfY.value = rectPts.y.toFixed(2);
                if (hfW) hfW.value = rectPts.w.toFixed(2);
                if (hfH) hfH.value = rectPts.h.toFixed(2);
                if (hfPage) hfPage.value = rectPts.page.toString();
            }

            function getDisplayScale() {
                if (!canvasEl || !pageWidthPts) return scale;
                var sx = canvasEl.clientWidth / pageWidthPts;
                return sx > 0 ? sx : scale;
            }

            function computeRectFromBoxPts() {
                if (!sigBox || !canvasEl) return null;
                var size = getBoxSize();
                var displayScale = getDisplayScale();
                var boxRect = sigBox.getBoundingClientRect();
                var canvasRect = canvasEl.getBoundingClientRect();
                var boxLeftPx = boxRect.left - canvasRect.left;
                var boxTopPx = boxRect.top - canvasRect.top;
                var xPts = boxLeftPx / displayScale;
                var yPts = pageHeightPts - ((boxTopPx / displayScale) + size.h);
                return { page: currentPage, x: xPts, y: yPts, w: size.w, h: size.h };
            }

            function applyBoxPosition(pxLeft, pxTop) {
                if (!sigBox || !canvasEl) return;
                sigBox.style.left = (canvasEl.offsetLeft + pxLeft) + 'px';
                sigBox.style.top = (canvasEl.offsetTop + pxTop) + 'px';
            }

            function placeBoxAt(pxLeft, pxTop) {
                if (!canvasEl || !wrapperEl) return;
                var size = getBoxSize();
                var displayScale = getDisplayScale();
                var maxLeft = canvasEl.clientWidth - (size.w * displayScale);
                var maxTop = canvasEl.clientHeight - (size.h * displayScale);
                var left = clamp(pxLeft, 0, Math.max(0, maxLeft));
                var top = clamp(pxTop, 0, Math.max(0, maxTop));

                if (!sigBox) {
                    sigBox = document.createElement('div');
                    sigBox.id = 'sigBoxUsb';
                    sigBox.style.position = 'absolute';
                    sigBox.style.border = '2px solid #dc2626';
                    sigBox.style.background = 'rgba(220,38,38,0.12)';
                    sigBox.style.cursor = 'move';
                    sigBox.style.zIndex = '2';
                    sigBox.style.userSelect = 'none';
                    sigBox.style.pointerEvents = 'auto';
                    wrapperEl.appendChild(sigBox);
                    bindDrag();
                }

                sigBox.style.width = (size.w * displayScale) + 'px';
                sigBox.style.height = (size.h * displayScale) + 'px';
                applyBoxPosition(left, top);

                var rectPts = computeRectFromBoxPts();
                if (rectPts) {
                    lastValidRect = rectPts;
                    setHiddenFromRect(rectPts);
                }
            }

            function drawExistingRects() {
                document.querySelectorAll('.sig-existing').forEach(el => el.remove());
                if (!wrapperEl || !canvasEl) return;
                existingRects.filter(r => r.page === currentPage).forEach(function(r) {
                    var box = document.createElement('div');
                    box.className = 'sig-existing';
                    box.style.position = 'absolute';
                    box.style.border = '2px dashed #64748b';
                    box.style.background = 'rgba(148,163,184,0.12)';
                    box.style.pointerEvents = 'none';
                    box.style.zIndex = '1';

                    var displayScale = getDisplayScale();
                    var pxLeft = r.x * displayScale;
                    var pxTop = (pageHeightPts - (r.y + r.h)) * displayScale;
                    box.style.left = (canvasEl.offsetLeft + pxLeft) + 'px';
                    box.style.top = (canvasEl.offsetTop + pxTop) + 'px';
                    box.style.width = (r.w * displayScale) + 'px';
                    box.style.height = (r.h * displayScale) + 'px';

                    wrapperEl.appendChild(box);
                });
            }

            function bindDrag() {
                if (!sigBox) return;
                sigBox.addEventListener('mousedown', function(e) {
                    dragging = true;
                    var rect = sigBox.getBoundingClientRect();
                    dragOffsetX = e.clientX - rect.left;
                    dragOffsetY = e.clientY - rect.top;
                    e.preventDefault();
                });

                document.addEventListener('mousemove', function(e) {
                    if (!dragging || !canvasEl) return;
                    var canvasRect = canvasEl.getBoundingClientRect();
                    var pxLeft = e.clientX - canvasRect.left - dragOffsetX;
                    var pxTop = e.clientY - canvasRect.top - dragOffsetY;
                    placeBoxAt(pxLeft, pxTop);
                });

                document.addEventListener('mouseup', function() {
                    if (!dragging) return;
                    dragging = false;
                    if (lastValidRect) setHiddenFromRect(lastValidRect);
                });
            }

            function renderPage(pageNum) {
                if (!pdfDoc) return;
                pdfDoc.getPage(pageNum).then(function(page) {
                    var viewport = page.getViewport({ scale: scale });
                    var canvas = document.createElement('canvas');
                    var context = canvas.getContext('2d');
                    canvas.height = viewport.height;
                    canvas.width = viewport.width;

                    pageWidthPts = viewport.width / scale;
                    pageHeightPts = viewport.height / scale;

                    var wrapper = document.createElement('div');
                    wrapper.style.position = 'relative';
                    wrapper.appendChild(canvas);

                    pagesHost.innerHTML = '';
                    pagesHost.appendChild(wrapper);
                    wrapperEl = wrapper;
                    canvasEl = canvas;

                    var renderTask = page.render({ canvasContext: context, viewport: viewport });
                    renderTask.promise.then(function() {
                        drawExistingRects();
                        if (!sigBox) {
                            placeBoxAt(40, 40);
                        }
                    });
                });
            }

            function updatePageInfo() {
                if (pageInfo) {
                    pageInfo.textContent = 'Pagina ' + currentPage + ' de ' + totalPages;
                }
            }

            function loadPdf() {
                if (!pdfId || !pdfId.value) return;
                var url = '/Handlers/VerPDF.ashx?id=' + encodeURIComponent(pdfId.value);
                pdfjsLib.getDocument(url).promise.then(function(pdf) {
                    pdfDoc = pdf;
                    totalPages = pdf.numPages || 1;
                    if (currentPage > totalPages) currentPage = 1;
                    updatePageInfo();
                    renderPage(currentPage);
                });
            }

            function onPrev() {
                if (currentPage <= 1) return;
                currentPage -= 1;
                updatePageInfo();
                renderPage(currentPage);
            }

            function onNext() {
                if (currentPage >= totalPages) return;
                currentPage += 1;
                updatePageInfo();
                renderPage(currentPage);
            }

            function refreshRects() {
                existingRects = parseRects();
                drawExistingRects();
            }

            function bindOrientation() {
                var radios = document.querySelectorAll('input[name="oriFirmaUsb"]');
                radios.forEach(function(radio) {
                    radio.addEventListener('change', function() {
                        if (!hfOrient) return;
                        hfOrient.value = radio.value;
                        placeBoxAt(40, 40);
                    });
                });
                if (hfOrient && !hfOrient.value) {
                    hfOrient.value = 'H';
                }
            }

            document.addEventListener('DOMContentLoaded', function() {
                window.location.href = 'zofrafirma://dnie';

                existingRects = parseRects();
                bindOrientation();
                if (btnPrev) btnPrev.addEventListener('click', onPrev);
                if (btnNext) btnNext.addEventListener('click', onNext);
                loadPdf();
                refreshRects();

                var elementsToReset = document.querySelectorAll('[id*="LnkVolver"], [id*="BtnIrBandeja"], [id*="LnkCerrarSesion"], a[href*="Bandeja.aspx"]');
                elementsToReset.forEach(function(el) {
                    el.addEventListener('click', function() {
                        window.location.href = 'zofrafirma://reset';
                    });
                });
            });
        })();
    </script>
</asp:Content>
