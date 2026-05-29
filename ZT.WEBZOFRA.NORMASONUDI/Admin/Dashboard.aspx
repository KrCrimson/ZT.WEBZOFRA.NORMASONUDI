<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Shared/MasterPage.master" CodeFile="Dashboard.aspx.cs" Inherits="Dashboard" Title="Panel de Administrador" %>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PageTitle" runat="server">
    <h1 class="page-title">Panel de Administrador</h1>
</asp:Content>

<asp:Content ID="SidebarContent" ContentPlaceHolderID="SidebarContent" runat="server">
    <div class="sidebar-user">
        Hola, <asp:Label ID="LblAdminName" runat="server" Font-Bold="true"></asp:Label>
    </div>
    <asp:LinkButton ID="BtnNavStats" runat="server" CssClass="nav-btn active" OnClick="Nav_Click" CommandArgument="Stats">
        <i class="ph ph-chart-bar"></i> Estadisticas
    </asp:LinkButton>
    <asp:LinkButton ID="BtnNavOrden" runat="server" CssClass="nav-btn" OnClick="Nav_Click" CommandArgument="Orden">
        <i class="ph ph-list-numbers"></i> Modificar Orden
    </asp:LinkButton>
    <asp:LinkButton ID="BtnNavAuditoria" runat="server" CssClass="nav-btn" OnClick="Nav_Click" CommandArgument="Auditoria">
        <i class="ph ph-clipboard-text"></i> Auditoria
    </asp:LinkButton>
</asp:Content>

<asp:Content ID="SidebarFooter" ContentPlaceHolderID="SidebarFooter" runat="server">
    <asp:LinkButton ID="BtnLogout" runat="server" CssClass="nav-btn" OnClick="BtnLogout_Click" style="color:#fca5a5;">
        <i class="ph ph-sign-out"></i> Cerrar Sesion
    </asp:LinkButton>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container container-wide">
        <asp:Label ID="LblGlobalMsg" runat="server" CssClass="alert alert-success" Visible="false" style="display:block; margin-bottom:1rem;"></asp:Label>

        <asp:Panel ID="PnlStats" runat="server">
            <h2>Resumen del Sistema</h2>
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
                <div class="stat-card border-orange" style="border-left-color:#fdba74;">
                    <asp:Label ID="LblFPar" runat="server" CssClass="stat-val">0</asp:Label>
                    <span class="stat-lbl">Firm. Parcial</span>
                </div>
                <div class="stat-card border-green">
                    <asp:Label ID="LblCompletos" runat="server" CssClass="stat-val">0</asp:Label>
                    <span class="stat-lbl">Completados</span>
                </div>
            </div>

            <hr style="margin: 2rem 0; opacity: 0.2; border-color:#cbd5e1" />

            <h2>Listado General de Tramites</h2>
            <asp:UpdatePanel ID="UpTodosTramites" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
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
                            <asp:BoundField DataField="LoginRegistrador" HeaderText="Registrador" />
                            <asp:TemplateField HeaderText="Accion">
                                <ItemTemplate>
                                    <asp:LinkButton ID="LnkVer" runat="server" CommandName="VerDetalle" CommandArgument='<%# Eval("IDDocumento") %>' CssClass="btn-zf">
                                        <i class="ph ph-eye"></i> Detalle
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </ContentTemplate>
            </asp:UpdatePanel>
        </asp:Panel>

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
                        <asp:TemplateField HeaderText="Mover">
                            <ItemTemplate>
                                <button type="button" class="drag-handle" title="Arrastrar para reordenar" aria-label="Arrastrar para reordenar">
                                    <i class="ph ph-dots-six-vertical"></i>
                                </button>
                            </ItemTemplate>
                            <ItemStyle Width="50px" HorizontalAlign="Center" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="NombreFirmante" HeaderText="Firmante" />
                        <asp:BoundField DataField="CorreoFirmante" HeaderText="Correo" />
                        <asp:TemplateField HeaderText="Estado">
                            <ItemTemplate>
                                <asp:Label ID="LblEstadoFirma" runat="server" Text='<%# Eval("CodigoEstadoFirma") %>'></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Orden Actual">
                            <ItemTemplate>
                                <asp:Label ID="LblOrdenActual" runat="server"></asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Nuevo Orden">
                            <ItemTemplate>
                                <asp:HiddenField ID="HfIDFirmante" runat="server" Value='<%# Eval("IDDocumentoFirmante") %>' />
                                <asp:HiddenField ID="HfEstadoFirma" runat="server" Value='<%# Eval("CodigoEstadoFirma") %>' />
                                <asp:HiddenField ID="HfOrdenAnterior" runat="server" Value='<%# Eval("OrdenDisplay") %>' />
                                <asp:Label ID="LblOrdenNuevo" runat="server" CssClass="orden-admin-badge badge bg-light text-primary border fw-bold" style="min-width: 40px; text-align: center; padding: 0.5rem;"></asp:Label>
                                <asp:DropDownList ID="DdlNuevoOrden" runat="server" CssClass="ddl-orden-admin orden-admin-select" style="margin:0; text-align:center; padding: 4px;"></asp:DropDownList>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <div style="text-align:right;">
                    <asp:Button ID="BtnGuardarOrden" runat="server" Text="Guardar Nuevo Orden" OnClick="BtnGuardarOrden_Click" CssClass="btn" />
                </div>
            </asp:Panel>
        </asp:Panel>

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
                    <asp:TemplateField HeaderText="Usuario">
                        <ItemTemplate>
                            <strong><%# Eval("IDUsuario") %></strong><br />
                            <small><%# Eval("NombreUsuario") %></small>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="TipoAccion" HeaderText="Accion" />
                    <asp:BoundField DataField="Descripcion" HeaderText="Descripcion" />
                    <asp:BoundField DataField="IDEquipo" HeaderText="IP" />
                </Columns>
            </asp:GridView>
        </asp:Panel>
    </div>

    <script type="text/javascript">
        function initAdminOrdenDragAndDrop() {
            var table = document.getElementById('<%= GvFirmantesEditar.ClientID %>');
            if (!table || !table.tBodies || table.tBodies.length === 0) {
                return;
            }

            var tbody = table.tBodies[0];
            var dragRow = null;

            function getDataRows() {
                return Array.prototype.slice.call(tbody.querySelectorAll('tr'));
            }

            function syncOrdenAdmin() {
                var rows = getDataRows();
                var lockedOrders = new Set();

                rows.forEach(function(row) {
                    if (!row.classList.contains('row-locked')) {
                        return;
                    }
                    var lockedOrderInput = row.querySelector('input[type="hidden"][id$="HfOrdenAnterior"]');
                    if (lockedOrderInput && lockedOrderInput.value) {
                        var lockedValue = parseInt(lockedOrderInput.value, 10);
                        if (!isNaN(lockedValue) && lockedValue > 0) {
                            lockedOrders.add(lockedValue);
                        }
                    }
                });

                var nextOrder = 1;
                rows.forEach(function(row, index) {
                    var ddl = row.querySelector('.ddl-orden-admin');
                    var badge = row.querySelector('.orden-admin-badge');
                    var lockedOrderInput = row.querySelector('input[type="hidden"][id$="HfOrdenAnterior"]');
                    if (!ddl || ddl.disabled) {
                        if (badge && lockedOrderInput && lockedOrderInput.value) {
                            badge.textContent = lockedOrderInput.value;
                        }
                        return;
                    }
                    while (lockedOrders.has(nextOrder)) {
                        nextOrder += 1;
                    }
                    var value = nextOrder.toString();
                    if (ddl.querySelector('option[value="' + value + '"]')) {
                        ddl.value = value;
                    }
                    if (badge) {
                        badge.textContent = value;
                    }
                    nextOrder += 1;
                });
            }

            function bindRow(row) {
                if (row.dataset.dragBound) {
                    return;
                }
                row.dataset.dragBound = 'true';

                row.addEventListener('dragover', function(e) {
                    if (!dragRow || dragRow === row) {
                        return;
                    }
                    if (row.classList.contains('row-locked') || dragRow.classList.contains('row-locked')) {
                        return;
                    }
                    e.preventDefault();

                    var rect = row.getBoundingClientRect();
                    var shouldInsertAfter = (e.clientY - rect.top) > (rect.height / 2);
                    tbody.insertBefore(dragRow, shouldInsertAfter ? row.nextSibling : row);
                    syncOrdenAdmin();
                });

                row.addEventListener('dragenter', function() {
                    if (dragRow && dragRow !== row && !row.classList.contains('row-locked')) {
                        row.classList.add('drag-target');
                    }
                });

                row.addEventListener('dragleave', function() {
                    row.classList.remove('drag-target');
                });

                row.addEventListener('drop', function(e) {
                    e.preventDefault();
                    row.classList.remove('drag-target');
                });
            }

            function bindHandle(handle) {
                if (handle.dataset.dragHandleBound) {
                    return;
                }
                handle.dataset.dragHandleBound = 'true';

                var row = handle.closest('tr');
                if (row && row.classList.contains('row-locked')) {
                    handle.setAttribute('aria-disabled', 'true');
                    return;
                }

                handle.setAttribute('draggable', 'true');
                handle.addEventListener('dragstart', function(e) {
                    dragRow = handle.closest('tr');
                    if (!dragRow || dragRow.classList.contains('row-locked')) {
                        dragRow = null;
                        return;
                    }
                    dragRow.classList.add('is-dragging');
                    e.dataTransfer.effectAllowed = 'move';
                    e.dataTransfer.setData('text/plain', '');
                });

                handle.addEventListener('dragend', function() {
                    if (dragRow) {
                        dragRow.classList.remove('is-dragging');
                    }
                    dragRow = null;
                    var targets = tbody.querySelectorAll('.drag-target');
                    targets.forEach(function(target) {
                        target.classList.remove('drag-target');
                    });
                });
            }

            getDataRows().forEach(bindRow);
            Array.prototype.slice.call(tbody.querySelectorAll('.drag-handle')).forEach(bindHandle);
            syncOrdenAdmin();
        }

        document.addEventListener('DOMContentLoaded', initAdminOrdenDragAndDrop);
        if (window.Sys && Sys.Application) {
            Sys.Application.add_load(initAdminOrdenDragAndDrop);
        }
    </script>
</asp:Content>
