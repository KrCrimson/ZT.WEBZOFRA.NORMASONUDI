<%@ Page Language="C#" AutoEventWireup="true" CodeFile="RegistrarTramite.aspx.cs" Inherits="RegistrarTramite" Title="Registrar Tramite" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <title>Registrar Tramite</title>
    <link rel="stylesheet" href="../styles.css" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem;">
                <h2 style="margin: 0;">Registrar Nuevo Documento <span class="badge-revisar">ESTADO: EN REVISION</span></h2>
                <a href="Bandeja.aspx" class="btn btn-secondary" style="display: flex; align-items: center; gap: 0.5rem; text-decoration: none;">
                    <i class="bi bi-arrow-left"></i> Volver a Bandeja
                </a>
            </div>
            <asp:Label ID="LblError" runat="server" Visible="false"></asp:Label>
            <asp:Label ID="LblExito" runat="server" Visible="false"></asp:Label>
            
            <div class="section-card">
                <h3>1. Subir Documento Original</h3>
                <div style="padding: 1rem;">
                    <asp:FileUpload ID="FuPdf" runat="server" accept=".pdf" />
                </div>
            </div>

            <div class="section-card">
                <h3>2. Clasificacion del Documento</h3>
                <div class="row">
                    <div class="col-md-6">
                        <label>Area de Origen (Parte el documento):</label>
                        <asp:DropDownList ID="DdlAreaResponsable" runat="server" CssClass="input-zf" style="width: 100%; border: 1px solid #cbd5e1; padding: 0.65rem 1rem; border-radius: 6px;">
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
                    </div>
                    <div class="col-md-6">
                        <label>Tipo de Documento:</label>
                        <asp:DropDownList ID="CbxTipoDocumento" runat="server" AutoPostBack="true" OnSelectedIndexChanged="CbxTipoDocumento_SelectedIndexChanged"></asp:DropDownList>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <label>Codigo de Documento:</label>
                        <asp:TextBox ID="TxtCodigoDocumento" runat="server" MaxLength="50" style="width: 100%; padding: 0.65rem 1rem; border: 1px solid #cbd5e1; font-weight: bold; color: #1E3A8A; background: white; border-radius: 6px; margin-bottom: 1rem;"></asp:TextBox>
                    </div>
                    <div class="col-md-6">
                        <label>Fecha de Documento:</label>
                        <div style="padding: 0.65rem 1rem; border: 1px solid #cbd5e1; background: #e2e8f0; border-radius: 6px; margin-bottom: 1rem;">
                            <asp:Label ID="LblFechaDocumento" runat="server" Font-Bold="true" ForeColor="#1E3A8A"></asp:Label>
                        </div>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-12">
                        <label>Asunto / Titulo:</label>
                        <asp:TextBox ID="TxtAsunto" runat="server" MaxLength="300" placeholder="Ej. Aprobacion de Plan de Contingencia..."></asp:TextBox>
                        <asp:RegularExpressionValidator ID="RevAsunto" runat="server" ControlToValidate="TxtAsunto" 
                            ValidationExpression="^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑüÜ\s\.,;:\-_()!¡?¿]+$" 
                            ErrorMessage="Caracteres no validos." ForeColor="Red" CssClass="text-danger" Display="Dynamic"></asp:RegularExpressionValidator>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <label>Fecha limite de revision (opcional):</label>
                        <asp:TextBox ID="TxtFechaLimite" runat="server" TextMode="Date"></asp:TextBox>
                    </div>
                    <div class="col-md-6"></div>
                </div>
            </div>

            <div class="section-card">
                <h3>3. Ruta de Firmas Definitiva</h3>
                <p style="color: var(--text-muted); font-size: 0.85rem; margin-top: 0; margin-bottom: 1rem;">Configura el orden secuencial de firmas.</p>
                
                <asp:GridView ID="GvFirmantes" runat="server" AutoGenerateColumns="false" OnRowDataBound="GvFirmantes_RowDataBound" OnRowDeleting="GvFirmantes_RowDeleting" CssClass="grid-view">
                    <Columns>
                        <asp:TemplateField HeaderText="Firmante">
                            <ItemTemplate>
                                <asp:HiddenField ID="HfLoginUsuario" runat="server" Value='<%# Eval("LoginUsuario") %>' />
                                <asp:DropDownList ID="DdlFirmante" runat="server" AutoPostBack="true" OnSelectedIndexChanged="DdlFirmante_SelectedIndexChanged" Width="100%" CssClass="ddl-firmante"></asp:DropDownList>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Correo">
                            <ItemTemplate>
                                <asp:Label ID="LblCorreo" runat="server" Text='<%# Eval("CorreoFirmante") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Orden">
                            <ItemTemplate>
                                <div class="badge bg-light text-primary border fw-bold" style="min-width: 40px; text-align: center; padding: 0.5rem;">
                                    <%# Container.DataItemIndex + 1 %>
                                </div>
                                <asp:HiddenField ID="HfOrdenFirma" runat="server" Value='<%# Container.DataItemIndex + 1 %>' />
                            </ItemTemplate>
                            <ItemStyle Width="80px" HorizontalAlign="Center" />
                        </asp:TemplateField>
                        <asp:CommandField ShowDeleteButton="True" DeleteText="&times;" ControlStyle-CssClass="btn btn-outline-danger btn-sm fw-bold" HeaderText="Quitar">
                            <ItemStyle Width="50px" HorizontalAlign="Center" />
                        </asp:CommandField>
                    </Columns>
                </asp:GridView>

                <asp:Button ID="BtnAgregarFirmante" runat="server" Text="+ Anadir" OnClick="BtnAgregarFirmante_Click" CssClass="btn btn-secondary" CausesValidation="false" />
            </div>
            
            <div style="text-align: right; margin-top: 1rem; border-top: 1px solid var(--surface-border); padding-top: 1.5rem;">
                <asp:Button ID="BtnRegistrar" runat="server" Text="Registrar Tramite" OnClick="BtnRegistrar_Click" CssClass="btn" style="font-size: 1.1rem; padding: 0.8rem 2rem;" />
            </div>
        </div>
    </form>

    <script type="text/javascript">
        document.addEventListener('DOMContentLoaded', function() {
            function updateDropdownGroup(selector) {
                const selects = document.querySelectorAll(selector);
                const selectedValues = new Set();
                
                selects.forEach(select => {
                    if (select.value) {
                        selectedValues.add(select.value);
                    }
                });
                
                selects.forEach(select => {
                    const currentValue = select.value;
                    Array.from(select.options).forEach(option => {
                        if (option.value && option.value !== currentValue && selectedValues.has(option.value)) {
                            option.disabled = true;
                            option.style.color = '#ef4444'; 
                        } else {
                            option.disabled = false;
                            option.style.color = '';
                        }
                    });
                });
            }

            function updateAllDropdowns() {
                updateDropdownGroup('.ddl-orden');
                updateDropdownGroup('.ddl-firmante');
            }

            const container = document.querySelector('.grid-view');
            if (container) {
                container.addEventListener('change', function(e) {
                    if (e.target && (e.target.classList.contains('ddl-orden') || e.target.classList.contains('ddl-firmante'))) {
                        updateAllDropdowns();
                    }
                });
                updateAllDropdowns(); 
            }
        });
    </script>
</body>
</html>
