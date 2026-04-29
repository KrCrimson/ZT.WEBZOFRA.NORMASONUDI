<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Dashboard.aspx.cs" Inherits="Dashboard" Title="Panel de Administrador" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <title>Administracion - ZOFRATACNA</title>
    <link rel="stylesheet" href="../styles.css" />
    <style>
        body { display: block; padding: 0; background: #f1f5f9; }
        .admin-layout { display: flex; min-height: 100vh; }
        
        /* SIDEBAR */
        .sidebar {
            width: 240px;
            min-width: 240px;
            background: #1e3a8a;
            color: white;
            display: flex;
            flex-direction: column;
        }
        .sidebar-header {
            padding: 1.5rem;
            border-bottom: 1px solid rgba(255,255,255,0.1);
            text-align: center;
        }
        .sidebar-user {
            padding: 1rem;
            background: rgba(0,0,0,0.1);
            font-size: 0.85rem;
            text-align: center;
        }
        .sidebar-nav { flex: 1; padding: 1rem 0; }
        .nav-btn {
            display: block;
            width: 100%;
            padding: 0.8rem 1.5rem;
            color: #cbd5e1;
            text-decoration: none;
            background: none;
            border: none;
            text-align: left;
            cursor: pointer;
            font-size: 0.9rem;
            transition: all 0.2s;
            border-left: 4px solid transparent;
        }
        .nav-btn:hover { background: rgba(255,255,255,0.05); color: white; }
        .nav-btn.active {
            background: rgba(255,255,255,0.1);
            color: white;
            border-left-color: #fbbf24;
            font-weight: 600;
        }
        
        /* MAIN */
        .main-panel { flex: 1; padding: 2rem; overflow-y: auto; }
        .card-stats-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
            gap: 1rem;
            margin-bottom: 2rem;
        }
        .stat-card {
            background: white;
            padding: 1.5rem;
            border-radius: 10px;
            box-shadow: 0 1px 3px rgba(0,0,0,0.1);
            text-align: center;
            border-bottom: 4px solid #cbd5e1;
        }
        .stat-val { font-size: 1.8rem; font-weight: 700; display: block; }
        .stat-lbl { font-size: 0.75rem; color: #64748b; text-transform: uppercase; margin-top: 5px; }
        
        /* Colors */
        .border-blue { border-bottom-color: #3b82f6; color: #3b82f6; }
        .border-yellow { border-bottom-color: #eab308; color: #eab308; }
        .border-red { border-bottom-color: #ef4444; color: #ef4444; }
        .border-purple { border-bottom-color: #a855f7; color: #a855f7; }
        .border-orange { border-bottom-color: #f97316; color: #f97316; }
        .border-green { border-bottom-color: #22c55e; color: #22c55e; }

        .filter-bar {
            background: white;
            padding: 1rem;
            border-radius: 8px;
            margin-bottom: 1rem;
            display: flex;
            gap: 1rem;
            align-items: flex-end;
            box-shadow: 0 1px 2px rgba(0,0,0,0.05);
        }
        
        .badge-ya-firmo { background: #dcfce7; color: #166534; padding: 2px 8px; border-radius: 4px; font-size: 0.7rem; font-weight: 600; }
        .badge-pendiente { background: #fef9c3; color: #854d0e; padding: 2px 8px; border-radius: 4px; font-size: 0.7rem; font-weight: 600; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="admin-layout">
            
            <div class="sidebar">
                <div class="sidebar-header">
                    <h2 style="color:white; margin:0; border:none; padding:0; font-size:1.2rem;">Panel Admin</h2>
                </div>
                <div class="sidebar-user">
                    Hola, <asp:Label ID="LblAdminName" runat="server" Font-Bold="true"></asp:Label>
                </div>
                <div class="sidebar-nav">
                    <asp:LinkButton ID="BtnNavStats" runat="server" CssClass="nav-btn active" OnClick="Nav_Click" CommandArgument="Stats">Estadisticas</asp:LinkButton>
                    <asp:LinkButton ID="BtnNavTramites" runat="server" CssClass="nav-btn" OnClick="Nav_Click" CommandArgument="Tramites">Todos los Tramites</asp:LinkButton>
                    <asp:LinkButton ID="BtnNavRoles" runat="server" CssClass="nav-btn" OnClick="Nav_Click" CommandArgument="Roles">Gestionar Roles</asp:LinkButton>
                    <asp:LinkButton ID="BtnNavOrden" runat="server" CssClass="nav-btn" OnClick="Nav_Click" CommandArgument="Orden">Modificar Orden</asp:LinkButton>
                    <asp:LinkButton ID="BtnNavAuditoria" runat="server" CssClass="nav-btn" OnClick="Nav_Click" CommandArgument="Auditoria">Auditoria</asp:LinkButton>
                    <hr style="opacity:0.1; margin: 1rem 0;" />
                    <asp:LinkButton ID="BtnLogout" runat="server" CssClass="nav-btn" OnClick="BtnLogout_Click" style="color:#f87171;">Cerrar Sesion</asp:LinkButton>
                </div>
            </div>

            <div class="main-panel">
                <asp:Label ID="LblGlobalMsg" runat="server" CssClass="alert alert-success" Visible="false" style="display:block; margin-bottom:1rem;"></asp:Label>

                <!-- PANEL: ESTADISTICAS -->
                <asp:Panel ID="PnlStats" runat="server">
                    <h2 style="border:none;">Resumen del Sistema</h2>
                    <div class="card-stats-grid">
                        <div class="stat-card border-blue">
                            <asp:Label ID="LblTotal" runat="server" CssClass="stat-val">0</asp:Label>
                            <span class="stat-lbl">Total Documentos</span>
                        </div>
                        <div class="stat-card border-yellow">
                            <asp:Label ID="LblEnRev" runat="server" CssClass="stat-val">0</asp:Label>
                            <span class="stat-lbl">En Revision</span>
                        </div>
                        <div class="stat-card border-red">
                            <asp:Label ID="LblObs" runat="server" CssClass="stat-val">0</asp:Label>
                            <span class="stat-lbl">Observados</span>
                        </div>
                        <div class="stat-card border-purple">
                            <asp:Label ID="LblAprFirma" runat="server" CssClass="stat-val">0</asp:Label>
                            <span class="stat-lbl">Aprob. Firma</span>
                        </div>
                        <div class="stat-card border-orange">
                            <asp:Label ID="LblEnFirma" runat="server" CssClass="stat-val">0</asp:Label>
                            <span class="stat-lbl">En Firma</span>
                        </div>
                        <div class="stat-card border-orange" style="border-bottom-color:#fdba74;">
                            <asp:Label ID="LblFPar" runat="server" CssClass="stat-val">0</asp:Label>
                            <span class="stat-lbl">Firm. Parcial</span>
                        </div>
                        <div class="stat-card border-green">
                            <asp:Label ID="LblCompletos" runat="server" CssClass="stat-val">0</asp:Label>
                            <span class="stat-lbl">Completados</span>
                        </div>
                    </div>
                </asp:Panel>

                <!-- PANEL: TODOS LOS TRAMITES -->
                <asp:Panel ID="PnlTramites" runat="server" Visible="false">
                    <h2>Listado General de Tramites</h2>
                    <div class="filter-bar">
                        <div style="flex:1;">
                            <label>Filtrar por Estado:</label>
                            <asp:DropDownList ID="DdlFiltroEstado" runat="server" AutoPostBack="true" OnSelectedIndexChanged="DdlFiltroEstado_SelectedIndexChanged" style="margin:0;"></asp:DropDownList>
                        </div>
                    </div>
                    <asp:GridView ID="GvTodosTramites" runat="server" AutoGenerateColumns="false" CssClass="grid-view" OnRowCommand="GvTodosTramites_RowCommand">
                        <Columns>
                            <asp:BoundField DataField="CodigoDocumento" HeaderText="Codigo" />
                            <asp:BoundField DataField="Asunto" HeaderText="Asunto" />
                            <asp:BoundField DataField="AreaResponsable" HeaderText="Area" />
                            <asp:BoundField DataField="FechaDocumento" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:BoundField DataField="Estado" HeaderText="Estado" />
                            <asp:BoundField DataField="LoginRegistrador" HeaderText="Registrador" />
                            <asp:TemplateField HeaderText="Accion">
                                <ItemTemplate>
                                    <asp:LinkButton ID="LnkVer" runat="server" CommandName="VerDetalle" CommandArgument='<%# Eval("IDDocumento") %>'>Ver Detalle</asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </asp:Panel>

                <!-- PANEL: GESTIONAR ROLES -->
                <asp:Panel ID="PnlRoles" runat="server" Visible="false">
                    <h2>Gestion de Roles de Usuario</h2>
                    <asp:GridView ID="GvUsuarios" runat="server" AutoGenerateColumns="false" CssClass="grid-view" OnRowCommand="GvUsuarios_RowCommand">
                        <Columns>
                            <asp:BoundField DataField="LoginUsuario" HeaderText="Usuario" />
                            <asp:BoundField DataField="NombreCompleto" HeaderText="Nombre" />
                            <asp:BoundField DataField="Email" HeaderText="Email" />
                            <asp:BoundField DataField="CodigoRol" HeaderText="Rol Actual" />
                            <asp:TemplateField HeaderText="Nuevo Rol">
                                <ItemTemplate>
                                    <asp:DropDownList ID="DdlNuevoRol" runat="server" style="margin:0; width:auto;">
                                        <asp:ListItem Text="ADMIN" Value="ADMIN"></asp:ListItem>
                                        <asp:ListItem Text="REGISTRADOR" Value="REGISTRADOR"></asp:ListItem>
                                        <asp:ListItem Text="FIRMADOR" Value="FIRMADOR"></asp:ListItem>
                                    </asp:DropDownList>
                                    <asp:Button ID="BtnCambiar" runat="server" Text="Cambiar" CommandName="CambiarRol" CommandArgument='<%# Container.DataItemIndex %>' CssClass="btn" style="padding: 0.4rem 1rem; font-size: 0.8rem;" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </asp:Panel>

                <!-- PANEL: MODIFICAR ORDEN -->
                <asp:Panel ID="PnlOrden" runat="server" Visible="false">
                    <h2>Modificar Orden de Firma</h2>
                    <div class="filter-bar">
                        <div style="flex:1;">
                            <label>Buscar Documento (Codigo o ID):</label>
                            <asp:TextBox ID="TxtBuscarDoc" runat="server" placeholder="Ej: ACT-2026-0001" style="margin:0;"></asp:TextBox>
                        </div>
                        <asp:Button ID="BtnBuscarDoc" runat="server" Text="Buscar" OnClick="BtnBuscarDoc_Click" CssClass="btn" />
                    </div>

                    <asp:GridView ID="GvDocsBusqueda" runat="server" AutoGenerateColumns="false" CssClass="grid-view" Visible="false" OnRowCommand="GvDocsBusqueda_RowCommand">
                        <Columns>
                            <asp:BoundField DataField="CodigoDocumento" HeaderText="Codigo" />
                            <asp:BoundField DataField="Asunto" HeaderText="Asunto" />
                            <asp:BoundField DataField="CodigoEstado" HeaderText="Estado" />
                            <asp:TemplateField HeaderText="Accion">
                                <ItemTemplate>
                                    <asp:LinkButton ID="LnkEditarFirmantes" runat="server" CommandName="EditarF" CommandArgument='<%# Eval("IDDocumento") %>'>Editar Firmantes</asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>

                    <asp:Panel ID="PnlEditarFirmantes" runat="server" Visible="false" style="background:white; padding:1.5rem; border-radius:8px;">
                        <h3>Editando Orden para: <asp:Label ID="LblDocEdicion" runat="server" ForeColor="#1e3a8a"></asp:Label></h3>
                        <asp:GridView ID="GvFirmantesEditar" runat="server" AutoGenerateColumns="false" CssClass="grid-view" OnRowDataBound="GvFirmantesEditar_RowDataBound">
                            <Columns>
                                <asp:BoundField DataField="NombreFirmante" HeaderText="Firmante" />
                                <asp:BoundField DataField="CorreoFirmante" HeaderText="Correo" />
                                <asp:TemplateField HeaderText="Estado">
                                    <ItemTemplate>
                                        <asp:Label ID="LblEstadoFirma" runat="server" Text='<%# Eval("CodigoEstadoFirma") %>'></asp:Label>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Orden Actual">
                                    <ItemTemplate><%# Eval("OrdenFirma") %></ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Nuevo Orden">
                                    <ItemTemplate>
                                        <asp:HiddenField ID="HfIDFirmante" runat="server" Value='<%# Eval("IDDocumentoFirmante") %>' />
                                        <asp:HiddenField ID="HfEstadoFirma" runat="server" Value='<%# Eval("CodigoEstadoFirma") %>' />
                                        <asp:TextBox ID="TxtNuevoOrden" runat="server" Text='<%# Eval("OrdenFirma") %>' Width="50px" style="margin:0; text-align:center;" TextMode="Number"></asp:TextBox>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                        <div style="text-align:right;">
                            <asp:Button ID="BtnGuardarOrden" runat="server" Text="Guardar Nuevo Orden" OnClick="BtnGuardarOrden_Click" CssClass="btn" />
                        </div>
                    </asp:Panel>
                </asp:Panel>

                <!-- PANEL: AUDITORIA -->
                <asp:Panel ID="PnlAuditoria" runat="server" Visible="false">
                    <h2>Auditoria de Documentos</h2>
                    <div class="filter-bar">
                        <div>
                            <label>Desde:</label>
                            <asp:TextBox ID="TxtFechaDesde" runat="server" TextMode="Date" style="margin:0;"></asp:TextBox>
                        </div>
                        <div>
                            <label>Hasta:</label>
                            <asp:TextBox ID="TxtFechaHasta" runat="server" TextMode="Date" style="margin:0;"></asp:TextBox>
                        </div>
                        <div style="flex:1;">
                            <label>Codigo Documento:</label>
                            <asp:TextBox ID="TxtFiltroDocAuditoria" runat="server" placeholder="Codigo..." style="margin:0;"></asp:TextBox>
                        </div>
                        <asp:Button ID="BtnFiltrarAuditoria" runat="server" Text="Filtrar" OnClick="BtnFiltrarAuditoria_Click" CssClass="btn" />
                    </div>
                    <asp:GridView ID="GvAuditoria" runat="server" AutoGenerateColumns="false" CssClass="grid-view" AllowPaging="true" PageSize="50" OnPageIndexChanging="GvAuditoria_PageIndexChanging">
                        <Columns>
                            <asp:BoundField DataField="FechaCambio" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy HH:mm}" />
                            <asp:BoundField DataField="CodigoDocumento" HeaderText="Documento" />
                            <asp:BoundField DataField="NombreUsuario" HeaderText="Usuario" />
                            <asp:BoundField DataField="TipoAccion" HeaderText="Accion" />
                            <asp:BoundField DataField="Descripcion" HeaderText="Descripcion" />
                            <asp:BoundField DataField="IDEquipo" HeaderText="IP" />
                        </Columns>
                    </asp:GridView>
                </asp:Panel>

            </div>
        </div>
    </form>
</body>
</html>