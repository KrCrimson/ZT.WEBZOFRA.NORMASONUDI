<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Shared/MasterPage.master" CodeFile="RegistrarTramite.aspx.cs" Inherits="RegistrarTramite" Title="Registrar Tramite" %>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PageTitle" runat="server">
    <h1 class="page-title">Registrar Nuevo Documento</h1>
</asp:Content>

<asp:Content ID="PageStatus" ContentPlaceHolderID="PageStatus" runat="server">
    <span class="status-pill"><i class="ph-fill ph-magnifying-glass"></i> ESTADO: EN REVISION</span>
</asp:Content>

<asp:Content ID="HeaderActions" ContentPlaceHolderID="HeaderActions" runat="server">
</asp:Content>



<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <asp:Label ID="LblError" runat="server" Visible="false"></asp:Label>
        <asp:Label ID="LblExito" runat="server" Visible="false"></asp:Label>
        <div style="font-size:12px;color:#64748b;margin-bottom:10px;">(*) Campos obligatorios.</div>
        <asp:ValidationSummary ID="ValidationSummaryRegistrar" runat="server" ValidationGroup="Registrar"
            ShowMessageBox="true" ShowSummary="false" />
        <div id="validationNotice" class="validation-notice hidden" role="alert">
            Revisa los campos obligatorios antes de registrar.
        </div>

        <div class="section-card">
            <h3>1. Subir Documento Original</h3>
            <div class="dropzone" id="pdfDropzone">
                <div class="dropzone-content">
                    <i class="ph ph-upload-simple" style="font-size: 2rem; color: #94a3b8;"></i>
                    <p><strong>Haz clic para subir</strong> o arrastra y suelta aqui</p>
                    <p style="font-size: 0.8rem; color: #64748b;">Solo archivos PDF</p>
                </div>
                <asp:FileUpload ID="FuPdf" runat="server" accept=".pdf" ClientIDMode="Static" />
            </div>
            <div id="pdfClientError" class="hidden" style="font-size:12px;color:#b91c1c;margin-top:6px;"></div>
            <div style="font-size:12px;color:#64748b;margin-top:6px;">Solo se permite subir PDF sin firma digital previa. Tamaño maximo: 50 MB.</div>
            <div id="fileIndicator" class="file-indicator hidden">
                <div>
                    <strong id="fileName">documento.pdf</strong><br />
                    <span id="fileSize" style="color:#64748b; font-size:0.8rem;">0 MB</span>
                </div>
                <button type="button" id="removeFileBtn" class="icon-btn"><i class="ph ph-x"></i></button>
            </div>
            <div id="pdfPreviewWrap" class="pdf-preview hidden">
                <div class="pdf-preview__header">
                    <span>Previsualizacion del PDF</span>
                    <small>Esta vista es solo para revisarlo antes de registrar.</small>
                </div>
                <div class="pdf-preview__body">
                    <iframe id="pdfPreviewFrame" title="Previsualizacion de PDF" loading="lazy"></iframe>
                </div>
            </div>
        </div>

        <div class="section-card">
            <h3>2. Clasificacion del Documento</h3>
            <asp:UpdatePanel ID="UpClasificacion" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="row">
                        <div class="col-md-6">
                            <label>Area de Origen (Parte el documento): <span class="req-mark">*</span></label>
                            <asp:DropDownList ID="DdlAreaResponsable" runat="server" CssClass="input-zf">
                        <asp:ListItem Text="-- Seleccione &Aacute;rea --" Value=""></asp:ListItem>
                        <asp:ListItem Text="GERENCIA GENERAL" Value="GERENCIA GENERAL"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Area de Fiscalizaci&oacute;n" Value="Area de Fiscalizacion"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Unidad de Relaciones P&uacute;blicas e Imagen Institucional" Value="Unidad de Relaciones Publicas e Imagen Institucional"></asp:ListItem>
                        <asp:ListItem Text="ORGANO DE CONTROL INSTITUCIONAL" Value="ORGANO DE CONTROL INSTITUCIONAL"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Sistema de Actividades de Control y Seguimiento" Value="Sistema de Actividades de Control y Seguimiento"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Sistema de Acciones de Control" Value="Sistema de Acciones de Control"></asp:ListItem>
                        <asp:ListItem Text="OFICINA DE ASESORIA JURIDICA" Value="OFICINA DE ASESORIA JURIDICA"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Unidad de Asuntos Judiciales" Value="Unidad de Asuntos Judiciales"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Unidad de Asuntos Administrativos" Value="Unidad de Asuntos Administrativos"></asp:ListItem>
                        <asp:ListItem Text="OFICINA DE PLANEAMIENTO Y PRESUPUESTO" Value="OFICINA DE PLANEAMIENTO Y PRESUPUESTO"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Sistemas de Gesti&oacute;n" Value="Sistemas de Gestion"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Sistema de Presupuesto y Proyectos" Value="Sistema de Presupuesto y Proyectos"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Sistema de Planes y Programas" Value="Sistema de Planes y Programas"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Sistema de Racionalizaci&oacute;n" Value="Sistema de Racionalizacion"></asp:ListItem>
                        <asp:ListItem Text="OFICINA DE ADMINISTRACION Y FINANZAS" Value="OFICINA DE ADMINISTRACION Y FINANZAS"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; &Aacute;rea de Gesti&oacute;n del Talento Humano" Value="Area de Gestion del Talento Humano"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Area de Contabilidad" Value="Area de Contabilidad"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Area de Tesorer&iacute;a" Value="Area de Tesoreria"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Area de Log&iacute;stica" Value="Area de Logistica"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Unidad de Transporte y Mantenimiento" Value="Unidad de Transporte y Mantenimiento"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Unidad de Areas Verdes y Jardineria" Value="Unidad de Areas Verdes y Jardineria"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Unidad de Control Patrimonial" Value="Unidad de Control Patrimonial"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Unidad de Archivo Central" Value="Unidad de Archivo Central"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Unidad de Seguridad y Vigilancia" Value="Unidad de Seguridad y Vigilancia"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Unidad de Tr&aacute;mite Documentario" Value="Unidad de Tramite Documentario"></asp:ListItem>
                        <asp:ListItem Text="GERENCIA DE PROMOCION Y DESARROLLO" Value="GERENCIA DE PROMOCION Y DESARROLLO"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Area de Marketing y Promoci&oacute;n" Value="Area de Marketing y Promocion"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Area de Soluciones al Usuario" Value="Area de Soluciones al Usuario"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Area de Desarrollo e Infraestructura" Value="Area de Desarrollo e Infraestructura"></asp:ListItem>
                        <asp:ListItem Text="GERENCIA DE OPERACIONES" Value="GERENCIA DE OPERACIONES"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Secci&oacute;n de Registro de Usuarios" Value="Seccion de Registro de Usuarios"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Secci&oacute;n Archivo GO" Value="Seccion Archivo GO"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Area de Control Operativo, Zona Comercial y de Franquicia" Value="Area de Control Operativo, Zona Comercial y de Franquicia"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Secci&oacute;n de Control Operativo y de Zona Comercial" Value="Seccion de Control Operativo y de Zona Comercial"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Secci&oacute;n Control de Franquicia" Value="Seccion Control de Franquicia"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Area de T&eacute;cnica Aduanera" Value="Area de Tecnica Aduanera"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Secci&oacute;n de Valoraci&oacute;n" Value="Seccion de Valoracion"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Secci&oacute;n de Nomenclatura y Procedimientos" Value="Seccion de Nomenclatura y Procedimientos"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Area de Operaciones Aduaneras" Value="Area de Operaciones Aduaneras"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Secci&oacute;n de Garita y Balanza" Value="Seccion de Garita y Balanza"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Secci&oacute;n de Dep&oacute;sito Franco" Value="Seccion de Deposito Franco"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Area de Actividades Productivas" Value="Area de Actividades Productivas"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Area de R&eacute;gimen Simplificado" Value="Area de Regimen Simplificado"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Secci&oacute;n de Registro de Informaci&oacute;n de R&eacute;gimen Simplificado" Value="Seccion de Registro de Informacion de Regimen Simplificado"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Secci&oacute;n de Almac&eacute;n - Regimen Simplificado" Value="Seccion de Almacen - Regimen Simplificado"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Secci&oacute;n de Control de Plataforma Regimen Simplificado" Value="Seccion de Control de Plataforma Regimen Simplificado"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Secci&oacute;n de Clasificaci&oacute;n, Codificaci&oacute;n y Valoraci&oacute;n" Value="Seccion de Clasificacion, Codificacion y Valoracion"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Secci&oacute;n de Aforo" Value="Seccion de Aforo"></asp:ListItem>
                        <asp:ListItem Text="Area de Tecnolog&iacute;as de la Informaci&oacute;n y Comunicaciones" Value="Area de Tecnologias de la Informacion y Comunicaciones"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Secci&oacute;n de Desarrollo de Sistemas" Value="Seccion de Desarrollo de Sistemas"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Secci&oacute;n de Administraci&oacute;n de la Informaci&oacute;n" Value="Seccion de Administracion de la Informacion"></asp:ListItem>
                        <asp:ListItem Text="&nbsp;&nbsp;&bull; Secci&oacute;n de Soporte" Value="Seccion de Soporte"></asp:ListItem>
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="RfvAreaResponsable" runat="server" ControlToValidate="DdlAreaResponsable"
                                InitialValue="" ErrorMessage="Debe seleccionar un area de origen." ForeColor="Red"
                                CssClass="text-danger" Display="Dynamic" ValidationGroup="Registrar"></asp:RequiredFieldValidator>
                        </div>
                        <div class="col-md-6">
                            <label>Tipo de Documento: <span class="req-mark">*</span></label>
                            <asp:DropDownList ID="CbxTipoDocumento" runat="server" AutoPostBack="true" OnSelectedIndexChanged="CbxTipoDocumento_SelectedIndexChanged"></asp:DropDownList>
                            <asp:RequiredFieldValidator ID="RfvTipoDocumento" runat="server" ControlToValidate="CbxTipoDocumento"
                                InitialValue="" ErrorMessage="Debe seleccionar un tipo de documento." ForeColor="Red"
                                CssClass="text-danger" Display="Dynamic" ValidationGroup="Registrar"></asp:RequiredFieldValidator>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6">
                            <label>Codigo de Documento: <span class="req-mark">*</span></label>
                            <asp:TextBox ID="TxtCodigoDocumento" runat="server" MaxLength="50"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RfvCodigoDocumento" runat="server" ControlToValidate="TxtCodigoDocumento"
                                ErrorMessage="Debe ingresar el codigo de documento." ForeColor="Red"
                                CssClass="text-danger" Display="Dynamic" ValidationGroup="Registrar"></asp:RequiredFieldValidator>
                        </div>
                        <div class="col-md-6">
                            <label>Fecha de Documento:</label>
                            <div class="input-muted">
                                <asp:Label ID="LblFechaDocumento" runat="server" Font-Bold="true"></asp:Label>
                            </div>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-12">
                            <label>Asunto / Titulo: <span class="req-mark">*</span></label>
                            <asp:TextBox ID="TxtAsunto" runat="server" MaxLength="300" placeholder="Ej. Aprobacion de Plan de Contingencia..."></asp:TextBox>
                            <asp:RequiredFieldValidator ID="RfvAsunto" runat="server" ControlToValidate="TxtAsunto"
                                ErrorMessage="Debe ingresar el asunto o titulo." ForeColor="Red"
                                CssClass="text-danger" Display="Dynamic" ValidationGroup="Registrar"></asp:RequiredFieldValidator>
                            <asp:RegularExpressionValidator ID="RevAsunto" runat="server" ControlToValidate="TxtAsunto" 
                                ValidationExpression="^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑüÜ\s\.,;:\-_()!¡?¿]+$" 
                                ErrorMessage="Caracteres no validos." ForeColor="Red" CssClass="text-danger" Display="Dynamic" ValidationGroup="Registrar"></asp:RegularExpressionValidator>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6">
                            <label>Fecha limite de revision (opcional):</label>
                            <asp:TextBox ID="TxtFechaLimite" runat="server" TextMode="Date"></asp:TextBox>
                        </div>
                        <div class="col-md-6"></div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <style>
            .hidden { display: none; }
            .dropzone.is-locked { opacity: 0.6; }
            .dropzone.has-file .dropzone-content { display: none; }
            .dropzone.has-file input[type='file'] { display: none; }
            .dropzone.has-file { display: none; }
            .req-mark { color: #dc2626; font-weight: 700; margin-left: 2px; }
            .validation-notice { margin: 10px 0 16px; padding: 10px 12px; border-radius: 8px; background: #fee2e2; color: #991b1b; font-size: 12px; font-weight: 600; border: 1px solid #fecaca; }
            .pdf-preview { margin-top: 14px; border: 1px solid var(--color-border-tertiary, #e2e8f0); border-radius: 10px; overflow: hidden; background: var(--color-background-secondary, #f8fafc); }
            .pdf-preview__header { display: flex; flex-direction: column; gap: 2px; padding: 10px 12px; font-size: 12px; color: var(--color-text-secondary, #475569); border-bottom: 1px solid var(--color-border-tertiary, #e2e8f0); background: var(--color-background-primary, #fff); }
            .pdf-preview__header small { font-size: 11px; color: var(--color-text-tertiary, #94a3b8); }
            .pdf-preview__body { height: 420px; background: #fff; }
            .pdf-preview__body iframe { width: 100%; height: 100%; border: 0; }
            .wrap-tl { display: flex; flex-direction: column; gap: 16px; padding: 1rem 0; }
            .panel-tl { background: var(--color-background-primary, #fff); border: 1px solid var(--color-border-tertiary, #e2e8f0); border-radius: var(--border-radius-lg, 8px); overflow: hidden; }
            .panel-header-tl { padding: 10px 14px; border-bottom: 1px solid var(--color-border-tertiary, #e2e8f0); display: flex; align-items: center; justify-content: space-between; }
            .panel-title-tl { font-size: 13px; font-weight: 500; color: var(--color-text-secondary, #475569); }
            .badge-tl { font-size: 11px; background: var(--color-background-secondary, #f8fafc); border: 1px solid var(--color-border-tertiary, #e2e8f0); border-radius: 20px; padding: 2px 8px; color: var(--color-text-secondary, #475569); }
            .panel-search-tl { padding: 10px 14px; border-bottom: 1px solid var(--color-border-tertiary, #e2e8f0); background: var(--color-background-secondary, #f8fafc); }
            .panel-search-tl input { width: 100%; font-size: 13px; padding: 6px 10px; border-radius: 6px; border: 1px solid var(--color-border-secondary, #cbd5e1); }
            .listbox-tl { list-style: none; max-height: 220px; overflow-y: auto; margin:0; padding:0; }
            .listbox-tl li { display: flex; align-items: center; gap: 10px; padding: 9px 14px; font-size: 13px; cursor: pointer; border-bottom: 1px solid var(--color-border-tertiary, #e2e8f0); color: var(--color-text-primary, #1e293b); transition: background .1s; user-select: none; }
            .listbox-tl li:last-child { border-bottom: none; }
            .listbox-tl li:hover { background: var(--color-background-secondary, #f8fafc); }
            .listbox-tl li.selected { background: #E6F1FB; color: #0C447C; }
            .listbox-tl li.selected .chk-tl { background: #378ADD; border-color: #378ADD; color: #fff; }
            .chk-tl { width: 16px; height: 16px; border: 1.5px solid var(--color-border-secondary, #cbd5e1); border-radius: 4px; display: flex; align-items: center; justify-content: center; flex-shrink: 0; font-size: 10px; color: transparent; transition: all .15s; }
            .btn-row-tl { display: flex; gap: 8px; padding: 10px 14px; border-top: 1px solid var(--color-border-tertiary, #e2e8f0); background: var(--color-background-secondary, #f8fafc); flex-wrap: wrap; }
            .btn-tl { font-size: 12px; padding: 5px 12px; border-radius: var(--border-radius-md, 4px); border: 1px solid var(--color-border-secondary, #cbd5e1); background: var(--color-background-primary, #fff); color: var(--color-text-primary, #1e293b); cursor: pointer; }
            .btn-tl:hover { background: var(--color-background-secondary, #f8fafc); }
            .btn-tl.primary { background: #E6F1FB; border-color: #85B7EB; color: #0C447C; }
            .btn-tl.danger { background: #FCEBEB; border-color: #F09595; color: #A32D2D; }
            .btn-tl.danger:hover { background: #F7C1C1; }
            .timeline-area { padding: 14px; overflow-x: auto; }
            .tl-empty { text-align: center; padding: 32px 0; font-size: 13px; color: var(--color-text-tertiary, #94a3b8); }
            .tl-track { display: flex; align-items: flex-start; gap: 0; padding-bottom: 4px; min-height: 100px; flex-wrap: nowrap; }
            .tl-node { display: flex; flex-direction: column; align-items: center; flex-shrink: 0; width: 100px; }
            .tl-connector { width: 24px; height: 2px; background: var(--color-border-secondary, #cbd5e1); flex-shrink: 0; margin-top: 27px; }
            .tl-dot { width: 54px; height: 54px; border-radius: 50%; background: #E6F1FB; border: 2px solid #378ADD; display: flex; align-items: center; justify-content: center; font-size: 11px; font-weight: 500; color: #0C447C; text-align: center; cursor: grab; line-height: 1.2; padding: 4px; word-break: break-word; transition: background .15s, transform .1s; }
            .tl-dot:active { cursor: grabbing; transform: scale(1.08); background: #B5D4F4; }
            .tl-dot.drag-over { background: #B5D4F4; border-style: dashed; }
            .tl-pos { font-size: 11px; color: var(--color-text-tertiary, #94a3b8); margin-top: 5px; }
            .tl-del { font-size: 11px; color: #A32D2D; cursor: pointer; margin-top: 2px; padding: 2px 6px; border-radius: 4px; background: #FCEBEB; border: 1px solid #F09595; }
            .tl-del:hover { background: #F7C1C1; }
            .drop-hint { font-size: 11px; color: var(--color-text-tertiary, #94a3b8); text-align: center; padding: 6px 14px; border-top: 1px solid var(--color-border-tertiary, #e2e8f0); background: var(--color-background-secondary, #f8fafc); }
        </style>

        <div class="section-card">
            <h3>3. Ruta Secuencial (Revisión y Firma) <span class="req-mark">*</span></h3>
            <div class="wrap-tl">
                <div class="panel-tl">
                    <div class="panel-header-tl">
                        <span class="panel-title-tl">Empleados Disponibles</span>
                        <span class="badge-tl" id="sel-count">0 selec.</span>
                    </div>
                    <div class="panel-search-tl">
                        <input type="text" id="firmanteSearch" placeholder="Buscar por nombre o correo..." />
                    </div>
                    <ul class="listbox-tl" id="listbox"></ul>
                    <div class="btn-row-tl">
                        <button type="button" class="btn-tl primary" onclick="addSelected()"><i class="ti ti-plus" style="font-size:13px"></i> Agregar seleccionados</button>
                        <button type="button" class="btn-tl" onclick="clearSelection()">Limpiar selección</button>
                    </div>
                </div>

                <div class="panel-tl">
                    <div class="panel-header-tl">
                        <span class="panel-title-tl">Orden de Firma</span>
                        <span class="badge-tl" id="tl-count">0 items</span>
                    </div>
                    <div class="timeline-area">
                        <div id="tl-empty" class="tl-empty"><i class="ti ti-arrow-up" style="font-size:18px;display:block;margin-bottom:6px"></i>Agrega items desde la lista de arriba</div>
                        <div class="tl-track" id="timeline" style="display:none"></div>
                    </div>
                    <div class="btn-row-tl" id="tl-buttons" style="display:none">
                        <button type="button" class="btn-tl danger" onclick="clearTimeline()"><i class="ti ti-trash" style="font-size:13px"></i> Limpiar todo</button>
                        <span style="font-size:11px;color:var(--color-text-tertiary, #94a3b8);display:flex;align-items:center">&nbsp;&nbsp;Arrastra para reordenar · usa la X para eliminar</span>
                    </div>
                </div>
            </div>

            <asp:HiddenField ID="HfFirmantesJSON" runat="server" ClientIDMode="Static" />
            <asp:CustomValidator ID="CvFirmantes" runat="server" ErrorMessage="Debe agregar al menos un firmante."
                CssClass="text-danger" Display="Dynamic" ClientValidationFunction="validateFirmantes" ValidationGroup="Registrar"></asp:CustomValidator>
        </div>

        <div class="form-footer">
            <asp:Button ID="BtnRegistrar" runat="server" Text="Registrar Tramite" OnClick="BtnRegistrar_Click" CssClass="btn" ValidationGroup="Registrar" OnClientClick="return showValidationNotice();" />
        </div>
    </div>

    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/css/select2.min.css" />
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/select2@4.1.0-rc.0/dist/js/select2.min.js"></script>

    <script type="text/javascript">
        function initRegistrarTramiteUI() {
            var pdfInput = document.getElementById('FuPdf');
            var fileIndicator = document.getElementById('fileIndicator');
            var fileName = document.getElementById('fileName');
            var fileSize = document.getElementById('fileSize');
            var removeBtn = document.getElementById('removeFileBtn');
            var dropzone = document.getElementById('pdfDropzone');
            var clientError = document.getElementById('pdfClientError');
            var pdfPreviewWrap = document.getElementById('pdfPreviewWrap');
            var pdfPreviewFrame = document.getElementById('pdfPreviewFrame');
            var previewUrl = null;
            var maxPdfBytes = 50 * 1024 * 1024;

            function clearClientError() {
                if (clientError) {
                    clientError.textContent = '';
                    clientError.classList.add('hidden');
                }
            }

            function showClientError(msg) {
                if (clientError) {
                    clientError.textContent = msg;
                    clientError.classList.remove('hidden');
                }
            }

            function resetPdfUI() {
                if (pdfInput) {
                    pdfInput.value = '';
                }
                fileIndicator.classList.add('hidden');
                if (dropzone) dropzone.classList.remove('has-file');
                if (pdfPreviewFrame) {
                    pdfPreviewFrame.removeAttribute('src');
                }
                if (pdfPreviewWrap) {
                    pdfPreviewWrap.classList.add('hidden');
                }
                if (previewUrl) {
                    URL.revokeObjectURL(previewUrl);
                    previewUrl = null;
                }
            }

            function looksSignedPdf(buffer) {
                try {
                    var bytes = new Uint8Array(buffer);
                    var text = new TextDecoder('latin1').decode(bytes);
                    return text.indexOf('/ByteRange') !== -1 || text.indexOf('/Sig') !== -1;
                } catch (e) {
                    return false;
                }
            }

            if (pdfInput && !pdfInput.dataset.bound) {
                pdfInput.dataset.bound = 'true';
                pdfInput.addEventListener('change', function() {
                    clearClientError();
                    if (pdfInput.files && pdfInput.files.length > 0) {
                        var file = pdfInput.files[0];
                        if (file.type && file.type !== 'application/pdf') {
                            resetPdfUI();
                            showClientError('Debe seleccionar un archivo PDF válido.');
                            return;
                        }
                        if (file.size > maxPdfBytes) {
                            resetPdfUI();
                            showClientError('El PDF supera el tamaño máximo permitido de 50 MB.');
                            return;
                        }
                        var reader = new FileReader();
                        reader.onload = function(e) {
                            if (looksSignedPdf(e.target.result)) {
                                resetPdfUI();
                                showClientError('Debe seleccionar un archivo PDF sin firmas digitales previas.');
                                return;
                            }
                            fileName.textContent = file.name;
                            fileSize.textContent = (file.size / (1024 * 1024)).toFixed(2) + ' MB';
                            fileIndicator.classList.remove('hidden');
                            if (dropzone) dropzone.classList.add('has-file');
                            if (pdfPreviewFrame && pdfPreviewWrap) {
                                if (previewUrl) {
                                    URL.revokeObjectURL(previewUrl);
                                }
                                previewUrl = URL.createObjectURL(file);
                                pdfPreviewFrame.src = previewUrl;
                                pdfPreviewWrap.classList.remove('hidden');
                            }
                        };
                        reader.onerror = function() {
                            resetPdfUI();
                            showClientError('No se pudo leer el PDF. Intenta nuevamente.');
                        };
                        reader.readAsArrayBuffer(file);
                    } else {
                        resetPdfUI();
                    }
                });
            }

            if (removeBtn && !removeBtn.dataset.bound) {
                removeBtn.dataset.bound = 'true';
                removeBtn.addEventListener('click', function() {
                    clearClientError();
                    resetPdfUI();
                });
            }

            function updateDropdownGroup(selector) {}

            function updateAllDropdowns() {}

            function getDataRows(tbody) { return []; }

            function syncOrdenFirmantes(tbody) {}

            function initFirmantesDragAndDrop() {}

            function initFirmanteSelect2() {}
        } // Fin initRegistrarTramiteUI

        let SOURCE = []; 
        let timeline = [];
        let selectedLogins = new Set();
        let filterText = '';
        let dragIdx = null;
        let dragStartY = 0;

        function initFirmantes() {
            if (typeof window.EmpleadosDisponibles !== 'undefined') {
                SOURCE = window.EmpleadosDisponibles;
            }
            
            // Si el servidor devolvió un error y recargó la página, recuperamos las selecciones
            const hf = document.getElementById('HfFirmantesJSON');
            if (hf && hf.value) {
                try {
                    const parsed = JSON.parse(hf.value);
                    if (Array.isArray(parsed) && parsed.length > 0) {
                        timeline = parsed;
                    }
                } catch(e) {}
            }

            renderList();
            renderTimeline();
        }

        function renderList() {
            const lb = document.getElementById('listbox');
            if(!lb) return;
            lb.innerHTML = '';

            const f = (filterText || '').trim().toLowerCase();
            const list = f
                ? SOURCE.filter(emp => {
                    const hay = (emp.NombreCompleto + ' ' + emp.Email + ' ' + emp.LoginUsuario).toLowerCase();
                    return hay.indexOf(f) !== -1;
                })
                : SOURCE;

            list.forEach(emp => {
                const inTl = timeline.some(t => t.LoginUsuario === emp.LoginUsuario);
                const li = document.createElement('li');
                li.dataset.login = emp.LoginUsuario;
                if (inTl) li.style.opacity = '0.4';
                
                li.innerHTML = `<span class="chk-tl"></span><span style="flex-grow:1">${emp.NombreCompleto}<br><small style="color:gray">${emp.Email}</small></span>${inTl ? '<span style="font-size:11px;color:gray;">ya agregado</span>' : ''}`;
                
                if (!inTl) {
                    if (selectedLogins.has(emp.LoginUsuario)) {
                        li.classList.add('selected');
                        li.querySelector('.chk-tl').textContent = '✓';
                    }
                    li.addEventListener('click', () => toggleSelect(li, emp));
                    li.addEventListener('dblclick', () => {
                        if (!timeline.some(t => t.LoginUsuario === emp.LoginUsuario)) {
                            timeline.push(emp);
                            selectedLogins.delete(emp.LoginUsuario);
                            renderList();
                            renderTimeline();
                        }
                    });
                }
                lb.appendChild(li);
            });
            updateSelCount();
        }

        function toggleSelect(li, emp) {
            li.classList.toggle('selected');
            li.querySelector('.chk-tl').textContent = li.classList.contains('selected') ? '✓' : '';
            if (li.classList.contains('selected')) {
                selectedLogins.add(emp.LoginUsuario);
            } else {
                selectedLogins.delete(emp.LoginUsuario);
            }
            updateSelCount();
        }

        function updateSelCount() {
            const countEl = document.getElementById('sel-count');
            if(countEl) countEl.textContent = document.querySelectorAll('.listbox-tl li.selected').length + ' selec.';
        }

        function addSelected() {
            document.querySelectorAll('.listbox-tl li.selected').forEach(li => {
                const login = li.dataset.login;
                const emp = SOURCE.find(s => s.LoginUsuario === login);
                if (emp && !timeline.some(t => t.LoginUsuario === emp.LoginUsuario)) {
                    timeline.push(emp);
                    selectedLogins.delete(login);
                }
            });
            renderList();
            renderTimeline();
        }

        function clearSelection() {
            document.querySelectorAll('.listbox-tl li.selected').forEach(li => {
                li.classList.remove('selected');
                li.querySelector('.chk-tl').textContent = '';
            });
            selectedLogins.clear();
            updateSelCount();
        }

        function clearTimeline() {
            timeline = [];
            renderList();
            renderTimeline();
        }

        function removeFromTimeline(idx) {
            timeline.splice(idx, 1);
            renderList();
            renderTimeline();
        }

        function renderTimeline() {
            const tl = document.getElementById('timeline');
            const empty = document.getElementById('tl-empty');
            const btns = document.getElementById('tl-buttons');
            const countEl = document.getElementById('tl-count');
            
            if(!tl) return;
            if(countEl) countEl.textContent = timeline.length + ' items';

            // Guardar al HiddenField para C#
            document.getElementById('HfFirmantesJSON').value = JSON.stringify(timeline);

            if (timeline.length === 0) {
                empty.style.display = '';
                tl.style.display = 'none';
                btns.style.display = 'none';
                return;
            }
            empty.style.display = 'none';
            tl.style.display = 'flex';
            btns.style.display = 'flex';
            tl.innerHTML = '';

            timeline.forEach((emp, i) => {
                if (i > 0) {
                    const conn = document.createElement('div');
                    conn.className = 'tl-connector';
                    tl.appendChild(conn);
                }
                const node = document.createElement('div');
                node.className = 'tl-node';
                node.dataset.idx = i;

                const dot = document.createElement('div');
                dot.className = 'tl-dot';
                
                const partes = emp.NombreCompleto.split(' ');
                dot.textContent = partes[0] + (partes.length > 1 ? ' ' + partes[1] : '');
                dot.title = emp.NombreCompleto;
                dot.draggable = true;

                dot.addEventListener('dragstart', e => {
                    dragIdx = i;
                    dragStartY = e.clientY;
                    setTimeout(() => dot.style.opacity = '0.4', 0);
                });
                dot.addEventListener('dragend', e => {
                    dot.style.opacity = '1';
                    const delta = e.clientY - dragStartY;
                    if (delta > 60) { timeline.splice(i, 1); renderList(); renderTimeline(); return; }
                    renderTimeline();
                });
                dot.addEventListener('dragover', e => { e.preventDefault(); dot.classList.add('drag-over'); });
                dot.addEventListener('dragleave', () => dot.classList.remove('drag-over'));
                dot.addEventListener('drop', e => {
                    e.preventDefault();
                    dot.classList.remove('drag-over');
                    if (dragIdx !== null && dragIdx !== i) {
                        const moved = timeline.splice(dragIdx, 1)[0];
                        timeline.splice(i, 0, moved);
                        dragIdx = null;
                        renderTimeline();
                    }
                });

                const pos = document.createElement('div');
                pos.className = 'tl-pos';
                pos.textContent = '#' + (i + 1);

                const del = document.createElement('button');
                del.type = 'button';
                del.className = 'tl-del';
                del.innerHTML = 'X';
                del.title = 'Eliminar';
                del.addEventListener('click', () => removeFromTimeline(i));

                node.appendChild(dot);
                node.appendChild(pos);
                node.appendChild(del);
                tl.appendChild(node);
            });
        }

        document.addEventListener('DOMContentLoaded', function() {
            initRegistrarTramiteUI();
            initFirmantes();
        });
        if (window.Sys && Sys.Application) {
            Sys.Application.add_load(function() {
                initRegistrarTramiteUI();
                initFirmantes();
            });
        }

        document.addEventListener('input', function(e) {
            if (e.target && e.target.id === 'firmanteSearch') {
                filterText = e.target.value || '';
                renderList();
            }
        });

        function validateFirmantes(source, args) {
            var hf = document.getElementById('HfFirmantesJSON');
            if (!hf || !hf.value) {
                args.IsValid = false;
                return;
            }
            try {
                var parsed = JSON.parse(hf.value);
                args.IsValid = Array.isArray(parsed) && parsed.length > 0;
            } catch (e) {
                args.IsValid = false;
            }
        }

        function showValidationNotice() {
            if (typeof Page_ClientValidate === 'function') {
                var isValid = Page_ClientValidate('Registrar');
                var notice = document.getElementById('validationNotice');
                if (notice) {
                    if (isValid) {
                        notice.classList.add('hidden');
                    } else {
                        notice.classList.remove('hidden');
                    }
                }
                return isValid;
            }
            return true;
        }
    </script>
</asp:Content>
