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
            <h2>Registrar Nuevo Documento <span class="badge-revisar">ESTADO: EN REVISION</span></h2>
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
                        <asp:TextBox ID="TxtAreaResponsable" runat="server" MaxLength="150" placeholder="Ej: Gerencia de Servicios"></asp:TextBox>
                        <asp:RegularExpressionValidator ID="RevArea" runat="server" ControlToValidate="TxtAreaResponsable" 
                            ValidationExpression="^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑüÜ\s\.,;:\-_()!¡?¿]+$" 
                            ErrorMessage="Caracteres no validos." ForeColor="Red" CssClass="text-danger" Display="Dynamic"></asp:RegularExpressionValidator>
                    </div>
                    <div class="col-md-6">
                        <label>Tipo de Documento:</label>
                        <asp:DropDownList ID="CbxTipoDocumento" runat="server" AutoPostBack="true" OnSelectedIndexChanged="CbxTipoDocumento_SelectedIndexChanged"></asp:DropDownList>
                    </div>
                </div>
                <div class="row">
                    <div class="col-md-6">
                        <label>Codigo de Documento:</label>
                        <div style="padding: 0.65rem 1rem; border: 1px solid #cbd5e1; background: #e2e8f0; border-radius: 6px; margin-bottom: 1rem;">
                            <asp:Label ID="LblCodigoDocumento" runat="server" Font-Bold="true" ForeColor="#1E3A8A"></asp:Label>
                        </div>
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
                
                <asp:GridView ID="GvFirmantes" runat="server" AutoGenerateColumns="false" OnRowDataBound="GvFirmantes_RowDataBound" CssClass="grid-view">
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
                        <asp:TemplateField HeaderText="Orden de Firma">
                            <ItemTemplate>
                                <asp:HiddenField ID="HfOrdenFirma" runat="server" Value='<%# Eval("OrdenFirma") %>' />
                                <asp:DropDownList ID="DdlOrden" runat="server" CssClass="ddl-orden" Width="100px"></asp:DropDownList>
                            </ItemTemplate>
                        </asp:TemplateField>
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
