<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Bandeja.aspx.cs" Inherits="Bandeja" Title="Bandeja de Tramites" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <title>Bandeja de Tramites</title>
    <link rel="stylesheet" href="../styles.css" />
    <style>
        /* CONTENT LAYOUT: tramites + calendario */
        .content-row {
            display: flex;
            gap: 1.5rem;
            align-items: flex-start;
            flex-wrap: wrap;
        }
        .content-main {
            flex: 1 1 560px;
            min-width: 0;
            overflow-x: auto;
        }
        .content-aside {
            flex: 0 1 280px;
            width: 280px;
            min-width: 240px;
            max-width: 100%;
        }

        /* GRID TRAMITES */
        .gv-tramites { width: 100%; }
        .gv-tramites th { font-size: 0.78rem; padding: 0.65rem 0.8rem; }
        .gv-tramites td { font-size: 0.85rem; padding: 0.6rem 0.8rem; }
        .gv-tramites a { color: var(--primary); text-decoration: none; font-weight: 500; }
        .gv-tramites a:hover { text-decoration: underline; }

        /* STATUS BADGES */
        .badge-estado {
            display: inline-block;
            padding: 3px 10px;
            border-radius: 20px;
            font-size: 0.72rem;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.3px;
        }
        .badge-EN_REV   { background: #fef9c3; color: #854d0e; }
        .badge-OBS      { background: #fee2e2; color: #991b1b; }
        .badge-APR_FIRMA { background: #dbeafe; color: #1e40af; }
        .badge-EN_FIRMA  { background: #ffedd5; color: #9a3412; }
        .badge-FPAR      { background: #ffedd5; color: #9a3412; }
        .badge-FIRM_COM  { background: #dcfce7; color: #166534; }

        /* CALENDAR */
        .cal-panel {
            background: #fff;
            border: 1px solid var(--surface-border);
            border-radius: 8px;
            padding: 1rem;
        }
        .cal-panel table { width: 100%; border-collapse: collapse; }
        .cal-panel td { text-align: center; padding: 4px; font-size: 0.8rem; }
        .cal-panel a { color: var(--primary); text-decoration: none; }
        .cal-highlight { background: #dbeafe !important; border-radius: 4px; font-weight: 600; }

        .titulo-bandeja {
            font-size: 1.1rem;
            font-weight: 600;
            color: var(--primary);
            margin-bottom: 1rem;
        }

        .empty-msg {
            text-align: center;
            padding: 2rem;
            color: var(--text-muted);
        }

        @media (max-width: 1100px) {
            .content-row { flex-direction: column; }
            .content-aside { width: 100%; min-width: auto; }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="app-shell">

            <!-- SIDEBAR -->
            <aside class="app-sidebar">
                <div class="sidebar-brand">
                    <p class="brand-title">ZOFRATACNA</p>
                    <p class="brand-subtitle">Sistema de Gestion</p>
                </div>
                <div class="sidebar-nav">
                    <asp:Panel ID="PnlSidebar" runat="server">
                        
                        <asp:Panel ID="PnlMenuRegistrador" runat="server" Visible="false">
                            <asp:LinkButton ID="LnkNuevoTramite" runat="server" CssClass="sidebar-link" OnClick="LnkNuevoTramite_Click">Registrar Documento</asp:LinkButton>
                            <asp:LinkButton ID="LnkMisTramites" runat="server" CssClass="sidebar-link" OnClick="LnkMisTramites_Click">Mis Tramites</asp:LinkButton>
                        </asp:Panel>
 
                        <asp:Panel ID="PnlMenuFirmador" runat="server" Visible="false">
                            <asp:LinkButton ID="LnkPendientesRev" runat="server" CssClass="sidebar-link" OnClick="LnkPendientesRev_Click">Pendientes de Revision</asp:LinkButton>
                            <asp:LinkButton ID="LnkPendientesFirma" runat="server" CssClass="sidebar-link" OnClick="LnkPendientesFirma_Click">Pendientes de Firma</asp:LinkButton>
                            <asp:LinkButton ID="LnkCompletados" runat="server" CssClass="sidebar-link" OnClick="LnkCompletados_Click">Completados</asp:LinkButton>
                        </asp:Panel>
 
                        <asp:Panel ID="PnlMenuAdmin" runat="server" Visible="false">
                            <asp:LinkButton ID="LnkTodosTramites" runat="server" CssClass="sidebar-link" OnClick="LnkTodosTramites_Click">Todos los Tramites</asp:LinkButton>
                            <asp:LinkButton ID="LnkGestionarRoles" runat="server" CssClass="sidebar-link" OnClick="LnkGestionarRoles_Click">Gestionar Roles</asp:LinkButton>
                        </asp:Panel>

                    </asp:Panel>
                </div>
                <div class="sidebar-footer">
                    <asp:LinkButton ID="LnkCerrarSesion" runat="server" CssClass="sidebar-link" OnClick="LnkCerrarSesion_Click">Cerrar Sesion</asp:LinkButton>
                </div>
            </aside>

            <!-- MAIN CONTENT -->
            <div class="app-main">
                <div class="app-header">
                    <p class="page-title">Bandeja de Tramites</p>
                    <p class="page-subtitle">Gestione sus documentos y alertas pendientes.</p>
                </div>
                <div class="app-content">
                    <div class="container container-wide">
                    <h2><asp:Label ID="LblBienvenida" runat="server"></asp:Label></h2>
                    <asp:Label ID="LblError" runat="server" Visible="false"></asp:Label>
                    <asp:Label ID="LblExito" runat="server" Visible="false"></asp:Label>

                    <div class="content-row">
                        <!-- LISTA DE TRÁMITES -->
                        <div class="content-main">
                            <asp:Label ID="LblTituloBandeja" runat="server" CssClass="titulo-bandeja"></asp:Label>

                            <asp:GridView ID="GvTramites" runat="server" AutoGenerateColumns="false"
                                CssClass="grid-view gv-tramites" OnRowCommand="GvTramites_RowCommand"
                                OnRowDataBound="GvTramites_RowDataBound" EmptyDataText="No se encontraron tramites.">
                                <Columns>
                                    <asp:BoundField DataField="CodigoDocumento" HeaderText="Codigo" />
                                    <asp:BoundField DataField="Asunto" HeaderText="Asunto" />
                                    <asp:BoundField DataField="TipoDocumento" HeaderText="Tipo" />
                                    <asp:BoundField DataField="AreaResponsable" HeaderText="Area" />
                                    <asp:BoundField DataField="FechaDocumento" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                                    <asp:TemplateField HeaderText="Estado">
                                        <ItemTemplate>
                                            <span class='<%# "badge-estado badge-" + Eval("CodigoEstado") %>'>
                                                <%# Eval("Estado") %>
                                            </span>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Accion">
                                        <ItemTemplate>
                                            <asp:LinkButton ID="LnkVerDetalle" runat="server" CommandName="VerDetalle"
                                                CommandArgument='<%# Eval("IDDocumento") %>' Text="Ver Detalle"></asp:LinkButton>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                            </asp:GridView>
                        </div>

                        <!-- CALENDARIO -->
                        <div class="content-aside">
                            <div class="cal-panel">
                                <asp:Calendar ID="CalBandeja" runat="server"
                                    OnDayRender="CalBandeja_DayRender"
                                    OnSelectionChanged="CalBandeja_SelectionChanged"
                                    Width="100%" CellPadding="2"
                                    DayNameFormat="Short"
                                    Font-Size="Small"
                                    TitleStyle-BackColor="#1E3A8A"
                                    TitleStyle-ForeColor="White"
                                    TitleStyle-Font-Bold="true"
                                    NextPrevStyle-ForeColor="White"
                                    DayHeaderStyle-BackColor="#f1f5f9"
                                    DayHeaderStyle-ForeColor="#64748b"
                                    SelectedDayStyle-BackColor="#1E3A8A"
                                    SelectedDayStyle-ForeColor="White"
                                    TodayDayStyle-BackColor="#fef9c3"
                                    OtherMonthDayStyle-ForeColor="#cbd5e1">
                                </asp:Calendar>
                                <div style="margin-top: 0.5rem;">
                                    <asp:LinkButton ID="LnkLimpiarFiltroFecha" runat="server" OnClick="LnkLimpiarFiltroFecha_Click"
                                        CssClass="inline-link" style="color: var(--primary); font-size: 0.8rem; padding: 0.3rem 0;">
                                        Quitar filtro de fecha
                                    </asp:LinkButton>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>