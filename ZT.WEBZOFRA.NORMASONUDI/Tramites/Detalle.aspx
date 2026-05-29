<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Shared/MasterPage.master" CodeFile="Detalle.aspx.cs" Inherits="Detalle" Title="Detalle del Documento" %>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PageTitle" runat="server">
    <h1 class="page-title">
        <asp:Label ID="LblCodigoDoc" runat="server"></asp:Label>
    </h1>
</asp:Content>

<asp:Content ID="PageStatus" ContentPlaceHolderID="PageStatus" runat="server">
    <asp:Label ID="LblEstadoBadge" runat="server"></asp:Label>
</asp:Content>

<asp:Content ID="HeaderActions" ContentPlaceHolderID="HeaderActions" runat="server">
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <asp:Label ID="LblError" runat="server" Visible="false"></asp:Label>
        <asp:Label ID="LblExito" runat="server" Visible="false"></asp:Label>

        <asp:Panel ID="PnlDocumentoFinal" runat="server" Visible="false">
            <div class="section-card highlight-card">
                <div class="highlight-row">
                    <div>
                        <h3 class="highlight-title">Documento Firmado Digitalmente</h3>
                        <p class="highlight-text">El proceso de firmas ha finalizado correctamente. Ya puede descargar el archivo oficial.</p>
                    </div>
                    <asp:LinkButton ID="BtnDescargarFinal" runat="server" CssClass="btn btn-warning" OnClick="BtnDescargarFinal_Click">
                        Descargar PDF Final
                    </asp:LinkButton>
                </div>
            </div>
        </asp:Panel>

        <div class="section-card">
            <h3>Informacion del Documento</h3>
            <div class="info-grid">
                <div class="info-item">
                    <label>Asunto</label>
                    <div class="info-value"><asp:Label ID="LblAsunto" runat="server"></asp:Label></div>
                </div>
                <div class="info-item">
                    <label>Tipo de Documento</label>
                    <div class="info-value"><asp:Label ID="LblTipoDoc" runat="server"></asp:Label></div>
                </div>
                <div class="info-item">
                    <label>Area Responsable</label>
                    <div class="info-value"><asp:Label ID="LblArea" runat="server"></asp:Label></div>
                </div>
                <div class="info-item">
                    <label>Fecha del Documento</label>
                    <div class="info-value"><asp:Label ID="LblFechaDoc" runat="server"></asp:Label></div>
                </div>
                <div class="info-item">
                    <label>Version</label>
                    <div class="info-value"><asp:Label ID="LblVersion" runat="server"></asp:Label></div>
                </div>
                <div class="info-item">
                    <label>Registrador</label>
                    <div class="info-value"><asp:Label ID="LblRegistrador" runat="server"></asp:Label></div>
                </div>
                <div class="info-item">
                    <label>Fecha Limite de Revision</label>
                    <div class="info-value"><asp:Label ID="LblFechaLimite" runat="server"></asp:Label></div>
                </div>
            </div>
        </div>

        <asp:Panel ID="PnlVersiones" runat="server">
            <div class="section-card">
                <h3>Versiones del Documento</h3>
                <asp:GridView ID="GvVersiones" runat="server" AutoGenerateColumns="false" CssClass="grid-view" OnRowDataBound="GvVersiones_RowDataBound">
                    <Columns>
                        <asp:TemplateField HeaderText="Versión">
                            <ItemTemplate>
                                <asp:Label ID="LblVersionGrid" runat="server"></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="Motivo" HeaderText="Motivo" />
                        <asp:BoundField DataField="FechaCreacion" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                        <asp:TemplateField HeaderText="Acción">
                            <ItemTemplate>
                                <asp:HyperLink ID="LnkVerPDF" runat="server" Text="Ver PDF" Target="_blank"></asp:HyperLink>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </asp:Panel>

        <div class="section-card">
            <h3>Revisores y Estado de Revision</h3>
            <asp:GridView ID="GvRevisores" runat="server" AutoGenerateColumns="false"
                CssClass="grid-view" OnRowDataBound="GvRevisores_RowDataBound"
                EmptyDataText="Sin revisores asignados.">
                <Columns>
                    <asp:BoundField DataField="NombreRevisor" HeaderText="Revisor" />
                    <asp:BoundField DataField="CorreoRevisor" HeaderText="Correo" />
                    <asp:TemplateField HeaderText="Completado">
                        <ItemTemplate>
                            <%# Convert.ToBoolean(Eval("Completado")) ? "Si" : "No" %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Resultado">
                        <ItemTemplate>
                            <span class='<%# "badge-firma " + (Eval("CodigoResultado") != DBNull.Value && Eval("CodigoResultado").ToString() == "CONF" ? "badge-firma-fir" : "badge-firma-pen") %>'>
                                <%# Eval("CodigoResultado") != DBNull.Value ? (Eval("CodigoResultado").ToString() == "CONF" ? "Confirmado" : (Eval("CodigoResultado").ToString() == "OBS" ? "Observado" : Eval("CodigoResultado").ToString())) : "Pendiente" %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <asp:Panel ID="PnlFirmantesEstado" runat="server" Visible="false">
            <div class="section-card">
                <h3>Firmantes y Estado de Firma</h3>
                <asp:GridView ID="GvFirmantesEstado" runat="server" AutoGenerateColumns="false" CssClass="grid-view"
                    EmptyDataText="Sin firmantes asignados.">
                    <Columns>
                        <asp:BoundField DataField="OrdenFirma" HeaderText="Orden" />
                        <asp:BoundField DataField="NombreFirmante" HeaderText="Firmante" />
                        <asp:BoundField DataField="CorreoFirmante" HeaderText="Correo" />
                        <asp:TemplateField HeaderText="Estado">
                            <ItemTemplate>
                                <span class='<%# "badge-firma " + (Eval("CodigoEstadoFirma").ToString() == "FIR" ? "badge-firma-fir" : "badge-firma-pen") %>'>
                                    <%# Eval("CodigoEstadoFirma").ToString() == "FIR" ? "Firmado" : "Pendiente" %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </asp:Panel>

        <asp:Panel ID="PnlObservaciones" runat="server" Visible="false">
            <div class="section-card">
                <h3>Observaciones</h3>
                <asp:Repeater ID="RptObservaciones" runat="server">
                    <ItemTemplate>
                        <div class="obs-card" style="display:flex;flex-direction:column;gap:0.6rem;">
                            <div style="display:flex;flex-wrap:wrap;gap:0.8rem;align-items:center;justify-content:space-between;">
                                <div style="display:flex;flex-wrap:wrap;gap:0.6rem;align-items:center;">
                                    <span class="obs-autor"><%# Eval("NombreRevisor") %></span>
                                    <span class="obs-fecha"><%# Eval("FechaCreacion", "{0:dd/MM/yyyy HH:mm}") %></span>
                                    <span class="badge-estado" style='<%# Convert.ToBoolean(Eval("ObservacionCorregida")) ? "padding:0.2rem 0.5rem;background:#dcfce7;color:#166534;" : "padding:0.2rem 0.5rem;" %>'>
                                        <%# Convert.ToBoolean(Eval("ObservacionCorregida")) ? "Corregido" : "Observado" %>
                                    </span>
                                </div>
                                <asp:Panel runat="server" Visible='<%# Eval("RutaObservadaId") != DBNull.Value %>' style="display:flex;align-items:center;">
                                    <asp:HyperLink ID="LnkPdfObservado" runat="server"
                                        NavigateUrl='<%# "~/Handlers/VerPDF.ashx?id=" + Eval("RutaObservadaId") %>'
                                        Target="_blank" CssClass="btn btn-secondary" style="min-width:170px;text-align:center;">
                                        Ver PDF observado
                                    </asp:HyperLink>
                                </asp:Panel>
                            </div>
                            <div class="obs-texto"><%# Eval("Descripcion") %></div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </asp:Panel>

        <div class="section-card">
            <h3>Vista Previa del Documento</h3>
            <asp:HiddenField ID="HfRutaPDF" runat="server" />
            <asp:Panel ID="PnlPdfViewer" runat="server">
                <iframe id="IframePDF" runat="server" class="pdf-frame" src="about:blank"></iframe>
            </asp:Panel>
            <asp:Panel ID="PnlPdfAnnotator" runat="server" Visible="false">
                <div class="pdf-annotator-toolbar">
                    <span class="pdf-annotator-title">Marcar observaciones sobre el PDF</span>
                    <div class="pdf-annotator-actions">
                        <button type="button" class="btn btn-secondary" id="btnUndoMark">Deshacer ultimo</button>
                        <button type="button" class="btn btn-warning" id="btnClearMarks">Limpiar marcas</button>
                    </div>
                </div>
                <div id="pdfAnnotator" class="pdf-annotator"></div>
                <asp:HiddenField ID="HfPdfAnnotations" runat="server" ClientIDMode="Static" />
                <div class="input-hint">Dibuja rectangulos sobre el PDF para indicar correcciones.</div>
            </asp:Panel>
        </div>

        <asp:Panel ID="PnlAccionesFirmador" runat="server" Visible="false">
            <div class="section-card">
                <h3 id="TituloAcciones" runat="server">Acciones de Revision</h3>
                <div class="acciones-panel" id="DivObservacionUI" runat="server">
                    <div style="flex: 1; min-width: 250px;">
                        <label>Observacion (opcional para dar conformidad):</label>
                        <asp:TextBox ID="TxtObservacion" runat="server" TextMode="MultiLine" MaxLength="2000"
                            placeholder="Escriba su observacion..." Rows="3" onkeyup="sincronizarBotones(this)"></asp:TextBox>
                        <div class="input-hint">Si escribe una observacion, se habilita "Registrar Observacion".</div>
                    </div>
                </div>
                <div style="margin-top: 1rem; display: flex; gap: 0.8rem; flex-wrap:wrap;">
                    <asp:Button ID="BtnObservar" runat="server" Text="Registrar Observacion"
                        CssClass="btn btn-observar" OnClick="BtnObservar_Click" CausesValidation="false" />
                    <asp:Button ID="BtnConforme" runat="server" Text="Dar Visto Bueno"
                        CssClass="btn btn-conforme" OnClick="BtnConforme_Click" CausesValidation="false" />
                    <asp:HyperLink ID="LnkFirmarUsb" runat="server" Text="Firmar con USB-Token"
                        CssClass="btn" NavigateUrl="~/Tramites/FirmaUsbToken.aspx" Visible="false" style="text-decoration:none;" />
                    <asp:Button ID="BtnIrAFirmar" runat="server" Text="Firmar con DNI electronico"
                        CssClass="btn" OnClick="BtnIrAFirmar_Click" CausesValidation="false" Visible="false" />
                </div>
            </div>
        </asp:Panel>

        <asp:Panel ID="PnlAccionesRegistrador" runat="server" Visible="false">
            <div class="section-card">
                <h3>Acciones del Registrador</h3>
                <div style="display:flex;flex-wrap:wrap;gap:0.8rem;align-items:center;">
                    <asp:Button ID="BtnRecordar" runat="server" Text="Enviar Recordatorio"
                        CssClass="btn btn-warning" OnClick="BtnRecordar_Click" CausesValidation="false" Visible="false" />
                </div>

                <asp:Panel ID="PnlCorreccion" runat="server" Visible="false">
                    <div style="margin-top: 1rem; display:flex; flex-direction:column; gap:0.6rem;">
                        <label>Subir PDF Corregido:</label>
                        <asp:FileUpload ID="FuPdfCorregido" runat="server" accept=".pdf" style="max-width:420px;" />
                        <asp:Button ID="BtnCorregir" runat="server" Text="Subir PDF Corregido"
                            CssClass="btn" OnClick="BtnCorregir_Click" CausesValidation="false" style="width:220px;" />
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>
    </div>

    <script src="https://cdnjs.cloudflare.com/ajax/libs/pdf.js/2.16.105/pdf.min.js"></script>
    <script type="text/javascript">
        var pdfjsLib = window['pdfjs-dist/build/pdf'];
        if (pdfjsLib) {
            pdfjsLib.GlobalWorkerOptions.workerSrc = "https://cdnjs.cloudflare.com/ajax/libs/pdf.js/2.16.105/pdf.worker.min.js";
        }

        function sincronizarBotones(txt) {
            var tieneTexto = txt.value.trim().length > 0;
            var btnConforme = document.getElementById('<%= BtnConforme.ClientID %>');
            var btnObservar = document.getElementById('<%= BtnObservar.ClientID %>');
            var tieneMarcas = false;
            var hidden = document.getElementById('HfPdfAnnotations');
            if (hidden && hidden.value) {
                try {
                    var parsed = JSON.parse(hidden.value);
                    tieneMarcas = Array.isArray(parsed) && parsed.length > 0;
                } catch (e) { }
            }
            var debeObservar = tieneTexto || tieneMarcas;
            if (btnConforme) btnConforme.disabled = debeObservar;
            if (btnObservar) btnObservar.disabled = !debeObservar;
        }

        function initPdfAnnotator() {
            var annotator = document.getElementById('pdfAnnotator');
            var pdfId = document.getElementById('<%= HfRutaPDF.ClientID %>');
            if (!annotator || !pdfId || !pdfId.value || !pdfjsLib) return;

            var annotations = [];
            var overlayMap = {};
            var hidden = document.getElementById('HfPdfAnnotations');
            var btnClear = document.getElementById('btnClearMarks');
            var btnUndo = document.getElementById('btnUndoMark');

            function syncHidden() {
                if (hidden) hidden.value = JSON.stringify(annotations);
                var txt = document.getElementById('<%= TxtObservacion.ClientID %>');
                if (txt) sincronizarBotones(txt);
            }

            function drawMarks(pageNumber, tempRect) {
                var overlay = overlayMap[pageNumber];
                if (!overlay) return;
                var ctx = overlay.getContext('2d');
                ctx.clearRect(0, 0, overlay.width, overlay.height);
                ctx.strokeStyle = '#dc2626';
                ctx.lineWidth = 2;

                annotations.forEach(function(mark) {
                    if (mark.page !== pageNumber) return;
                    var x = mark.x * overlay.width;
                    var y = mark.y * overlay.height;
                    var w = mark.w * overlay.width;
                    var h = mark.h * overlay.height;
                    ctx.strokeRect(x, y, w, h);
                });

                if (tempRect) {
                    ctx.strokeRect(tempRect.x, tempRect.y, tempRect.w, tempRect.h);
                }
            }

            function redrawAllMarks() {
                Object.keys(overlayMap).forEach(function(key) {
                    var pageNum = parseInt(key, 10);
                    if (!isNaN(pageNum)) drawMarks(pageNum);
                });
            }

            function clearMarks() {
                annotations = [];
                redrawAllMarks();
                syncHidden();
            }

            function undoLast() {
                if (annotations.length === 0) return;
                annotations.pop();
                redrawAllMarks();
                syncHidden();
            }

            if (btnClear) {
                btnClear.addEventListener('click', function() {
                    clearMarks();
                });
            }

            if (btnUndo) {
                btnUndo.addEventListener('click', function() {
                    undoLast();
                });
            }

            var url = '<%= ResolveUrl("~/Handlers/VerPDF.ashx?id=") %>' + pdfId.value;
            pdfjsLib.getDocument(url).promise.then(function(pdf) {
                for (var i = 1; i <= pdf.numPages; i++) {
                    (function(pageNumber) {
                        pdf.getPage(pageNumber).then(function(page) {
                            var viewport = page.getViewport({ scale: 1.2 });
                            var wrapper = document.createElement('div');
                            wrapper.className = 'pdf-page';
                            wrapper.style.width = viewport.width + 'px';

                            var canvas = document.createElement('canvas');
                            canvas.width = viewport.width;
                            canvas.height = viewport.height;
                            var ctx = canvas.getContext('2d');

                            var overlay = document.createElement('canvas');
                            overlay.className = 'annot-layer';
                            overlay.width = viewport.width;
                            overlay.height = viewport.height;
                            overlay.dataset.page = pageNumber;
                            overlayMap[pageNumber] = overlay;

                            wrapper.appendChild(canvas);
                            wrapper.appendChild(overlay);
                            annotator.appendChild(wrapper);

                            page.render({ canvasContext: ctx, viewport: viewport });
                            drawMarks(pageNumber);

                            var drawing = false;
                            var startX = 0;
                            var startY = 0;

                            overlay.addEventListener('mousedown', function(e) {
                                var rect = overlay.getBoundingClientRect();
                                startX = e.clientX - rect.left;
                                startY = e.clientY - rect.top;
                                drawing = true;
                            });

                            overlay.addEventListener('mousemove', function(e) {
                                if (!drawing) return;
                                var rect = overlay.getBoundingClientRect();
                                var x = e.clientX - rect.left;
                                var y = e.clientY - rect.top;
                                var w = x - startX;
                                var h = y - startY;
                                drawMarks(pageNumber, { x: startX, y: startY, w: w, h: h });
                            });

                            overlay.addEventListener('mouseup', function(e) {
                                if (!drawing) return;
                                drawing = false;
                                var rect = overlay.getBoundingClientRect();
                                var endX = e.clientX - rect.left;
                                var endY = e.clientY - rect.top;
                                var w = endX - startX;
                                var h = endY - startY;

                                if (Math.abs(w) < 6 || Math.abs(h) < 6) {
                                    drawMarks(pageNumber);
                                    return;
                                }

                                var x = w < 0 ? startX + w : startX;
                                var y = h < 0 ? startY + h : startY;
                                var width = Math.abs(w);
                                var height = Math.abs(h);

                                annotations.push({
                                    page: pageNumber,
                                    x: x / overlay.width,
                                    y: y / overlay.height,
                                    w: width / overlay.width,
                                    h: height / overlay.height
                                });

                                drawMarks(pageNumber);

                                syncHidden();
                            });

                            overlay.addEventListener('mouseleave', function() {
                                if (!drawing) return;
                                drawing = false;
                                drawMarks(pageNumber);
                            });
                        });
                    })(i);
                }
            });
        }

        document.addEventListener('DOMContentLoaded', function() {
            var txt = document.getElementById('<%= TxtObservacion.ClientID %>');
            if (txt) sincronizarBotones(txt);
            initPdfAnnotator();
        });
    </script>
</asp:Content>
