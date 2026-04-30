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
        <div class="app-shell">
            <aside class="app-sidebar">
                <div class="sidebar-brand">
                    <p class="brand-title">ZOFRATACNA</p>
                    <p class="brand-subtitle">Sistema de Gestion</p>
                </div>
                <div class="sidebar-nav">
                    <% string rol = Session["strRol"] != null ? Session["strRol"].ToString() : ""; %>
                    <% if (rol == "REGISTRADOR") { %>
                        <div class="sidebar-section">Registro</div>
                        <a class="sidebar-link active" href="<%= ResolveUrl("~/Tramites/RegistrarTramite.aspx") %>">Registrar Documento</a>
                        <a class="sidebar-link" href="<%= ResolveUrl("~/Tramites/Bandeja.aspx") %>">Mis Tramites</a>
                    <% } %>

                    <% if (rol == "FIRMADOR") { %>
                        <div class="sidebar-section">Revision</div>
                        <a class="sidebar-link" href="<%= ResolveUrl("~/Tramites/Bandeja.aspx?filtro=EN_REV") %>">Pendientes de Revision</a>
                        <a class="sidebar-link" href="<%= ResolveUrl("~/Tramites/Bandeja.aspx?filtro=APR_FIRMA,EN_FIRMA") %>">Pendientes de Firma</a>
                        <a class="sidebar-link" href="<%= ResolveUrl("~/Tramites/Bandeja.aspx?filtro=FIRM_COM") %>">Completados</a>
                    <% } %>

                    <% if (rol == "ADMIN") { %>
                        <div class="sidebar-section">Administracion</div>
                        <a class="sidebar-link active" href="<%= ResolveUrl("~/Tramites/RegistrarTramite.aspx") %>">Registrar Documento</a>
                        <a class="sidebar-link" href="<%= ResolveUrl("~/Tramites/Bandeja.aspx") %>">Todos los Tramites</a>
                        <a class="sidebar-link" href="<%= ResolveUrl("~/Admin/Dashboard.aspx") %>">Gestionar Roles</a>
                    <% } %>
                </div>
                <div class="sidebar-footer">
                    <a class="sidebar-link" href="<%= ResolveUrl("~/Login.aspx?logout=1") %>">Cerrar Sesion</a>
                </div>
            </aside>

            <div class="app-main">
                <div class="app-header">
                    <p class="page-title">Registrar Nuevo Documento</p>
                    <p class="page-subtitle">Complete los metadatos y configure el flujo de firmas.</p>
                </div>
                <div class="app-content">
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
                        <asp:DropDownList ID="CbxTipoDocumento" runat="server"></asp:DropDownList>
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
                <p style="color: var(--text-muted); font-size: 0.85rem; margin-top: 0; margin-bottom: 1rem;">Selecciona a cada firmante y se agrega en orden automatico.</p>

                <div class="row">
                    <div class="col-md-8">
                        <label>Seleccionar firmante:</label>
                        <asp:DropDownList ID="DdlFirmanteNuevo" runat="server"></asp:DropDownList>
                    </div>
                    <div class="col-md-4" style="display: flex; align-items: flex-end;">
                        <button type="button" class="btn btn-secondary" id="BtnAgregarFirmante">+ Anadir</button>
                    </div>
                </div>

                <asp:HiddenField ID="HfFirmantesJson" runat="server" />

                <table class="grid-view" id="TblFirmantes">
                    <thead>
                        <tr>
                            <th>Orden</th>
                            <th>Firmante</th>
                            <th>Correo</th>
                            <th>Accion</th>
                        </tr>
                    </thead>
                    <tbody></tbody>
                </table>
                <div id="FirmantesVacio" class="empty-msg">Aun no se agregan firmantes.</div>
            </div>
            
                        <div style="text-align: right; margin-top: 1rem; border-top: 1px solid var(--surface-border); padding-top: 1.5rem;">
                            <asp:Button ID="BtnRegistrar" runat="server" Text="Registrar Tramite" OnClick="BtnRegistrar_Click" CssClass="btn" style="font-size: 1.1rem; padding: 0.8rem 2rem;" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </form>

    <script type="text/javascript">
        document.addEventListener('DOMContentLoaded', function() {
            const select = document.getElementById('<%= DdlFirmanteNuevo.ClientID %>');
            const addBtn = document.getElementById('BtnAgregarFirmante');
            const tableBody = document.querySelector('#TblFirmantes tbody');
            const emptyMsg = document.getElementById('FirmantesVacio');
            const hidden = document.getElementById('<%= HfFirmantesJson.ClientID %>');

            function getFirmantes() {
                try {
                    return hidden.value ? JSON.parse(hidden.value) : [];
                } catch (e) {
                    return [];
                }
            }

            function setFirmantes(list) {
                hidden.value = JSON.stringify(list);
                renderTable(list);
            }

            function renderTable(list) {
                tableBody.innerHTML = '';
                if (!list.length) {
                    emptyMsg.style.display = 'block';
                    return;
                }

                emptyMsg.style.display = 'none';
                list.forEach(function(item, index) {
                    const row = document.createElement('tr');
                    row.innerHTML =
                        '<td>' + (index + 1) + '</td>' +
                        '<td>' + item.nombre + '</td>' +
                        '<td>' + item.correo + '</td>' +
                        '<td><button type="button" class="inline-link" data-index="' + index + '">Quitar</button></td>';
                    tableBody.appendChild(row);
                });
            }

            function addFirmante() {
                const value = select.value;
                if (!value) {
                    return;
                }

                const selectedOption = select.options[select.selectedIndex];
                const nombre = selectedOption.text;
                const correo = selectedOption.getAttribute('data-email') || '';
                const list = getFirmantes();

                if (list.some(item => item.login === value)) {
                    return;
                }

                list.push({ login: value, nombre: nombre, correo: correo });
                setFirmantes(list);
            }

            addBtn.addEventListener('click', addFirmante);

            tableBody.addEventListener('click', function(e) {
                const target = e.target;
                if (target && target.dataset && target.dataset.index) {
                    const list = getFirmantes();
                    const index = parseInt(target.dataset.index, 10);
                    list.splice(index, 1);
                    setFirmantes(list);
                }
            });

            setFirmantes(getFirmantes());
        });
    </script>
</body>
</html>
