<%@ Page Language="C#" AutoEventWireup="true" CodeFile="RegistrarTramite.aspx.cs" Inherits="RegistrarTramite" Title="Registrar Trámite" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Registrar Trámite</title>
    <link rel="stylesheet" href="../styles.css" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <h2>Registrar Nuevo Documento</h2>
            <asp:Label ID="LblError" runat="server" Visible="false"></asp:Label>
            <asp:Label ID="LblExito" runat="server" Visible="false"></asp:Label>
            
            <label>Asunto:</label>
            <asp:TextBox ID="TxtAsunto" runat="server" MaxLength="300"></asp:TextBox>
            
            <label>Tipo de Documento:</label>
            <asp:DropDownList ID="CbxTipoDocumento" runat="server"></asp:DropDownList>
            
            <label>Área Responsable:</label>
            <asp:TextBox ID="TxtAreaResponsable" runat="server" MaxLength="150"></asp:TextBox>
            
            <label>Código de Documento:</label>
            <asp:TextBox ID="TxtCodigoDocumento" runat="server" MaxLength="50"></asp:TextBox>
            
            <label>Fecha de Documento:</label>
            <asp:TextBox ID="TxtFechaDocumento" runat="server" TextMode="Date"></asp:TextBox>
            
            <label>Archivo PDF:</label>
            <asp:FileUpload ID="FuPdf" runat="server" accept=".pdf" />

            <h3>Firmantes</h3>
            <asp:GridView ID="GvFirmantes" runat="server" AutoGenerateColumns="false" OnRowDataBound="GvFirmantes_RowDataBound" CssClass="grid-view">
                <Columns>
                    <asp:TemplateField HeaderText="Firmante">
                        <ItemTemplate>
                            <asp:HiddenField ID="HfLoginUsuario" runat="server" Value='<%# Eval("LoginUsuario") %>' />
                            <asp:DropDownList ID="DdlFirmante" runat="server" AutoPostBack="false" Width="100%"></asp:DropDownList>
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

            <asp:Button ID="BtnAgregarFirmante" runat="server" Text="+ Agregar Firmante" OnClick="BtnAgregarFirmante_Click" CssClass="btn" style="background-color: var(--surface-border); margin-bottom: 2rem;" />
            
            <div style="text-align: right; margin-top: 1rem; border-top: 1px solid var(--surface-border); padding-top: 1.5rem;">
                <asp:Button ID="BtnRegistrar" runat="server" Text="Registrar Trámite" OnClick="BtnRegistrar_Click" CssClass="btn" />
            </div>
        </div>
    </form>

    <script type="text/javascript">
        document.addEventListener('DOMContentLoaded', function() {
            function updateDropdowns() {
                const selects = document.querySelectorAll('.ddl-orden');
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
                            option.style.color = '#ef4444'; // Red-ish to show disabled
                        } else {
                            option.disabled = false;
                            option.style.color = '';
                        }
                    });
                });
            }

            const container = document.querySelector('.grid-view');
            if (container) {
                container.addEventListener('change', function(e) {
                    if (e.target && e.target.classList.contains('ddl-orden')) {
                        updateDropdowns();
                    }
                });
                updateDropdowns(); // init
            }
        });
    </script>
</body>
</html>
