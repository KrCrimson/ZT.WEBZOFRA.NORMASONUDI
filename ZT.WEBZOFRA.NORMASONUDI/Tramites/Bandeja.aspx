<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Shared/MasterPage.master" CodeFile="Bandeja.aspx.cs" Inherits="Bandeja" Title="Bandeja de Tramites" %>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PageTitle" runat="server">
    <h1 class="page-title"><asp:Label ID="LblBienvenida" runat="server"></asp:Label></h1>
</asp:Content>


<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <asp:Label ID="LblError" runat="server" Visible="false"></asp:Label>
        <asp:Label ID="LblExito" runat="server" Visible="false"></asp:Label>

        <div class="d-flex justify-content-between align-items-center mb-3">
            <asp:Label ID="LblTituloBandeja" runat="server" CssClass="titulo-bandeja" style="margin:0;"></asp:Label>
        </div>

        <div class="filter-bar" style="display: flex; gap: 1rem; margin-bottom: 1.5rem; background: white; padding: 1rem; border-radius: 8px; box-shadow: 0 1px 3px rgba(0,0,0,0.1);">
            <div style="flex: 1;">
                <asp:TextBox ID="TxtBuscar" runat="server" placeholder="Buscar por Codigo o Asunto..." CssClass="input-zf" style="margin: 0; width: 100%; border: 1px solid #cbd5e1; padding: 0.5rem; border-radius: 4px;"></asp:TextBox>
            </div>
            <asp:Button ID="BtnBuscar" runat="server" Text="Buscar" OnClick="BtnBuscar_Click" CssClass="btn" />
            <asp:Button ID="BtnLimpiar" runat="server" Text="Limpiar" OnClick="BtnLimpiar_Click" CssClass="btn" style="background: #e2e8f0; color: #475569;" />
        </div>

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
                                <div class="d-flex gap-2 align-items-center">
                                    <asp:LinkButton ID="LnkVerDetalle" runat="server" CommandName="VerDetalle"
                                        CommandArgument='<%# Eval("IDDocumento") %>' CssClass="btn-zf">
                                        <i class="ph ph-eye"></i> Detalle
                                    </asp:LinkButton>
                                    <asp:HyperLink ID="LnkEditar" runat="server" 
                                        NavigateUrl='<%# "RegistrarTramite.aspx?id=" + Eval("IDDocumento") %>'
                                        Visible='<%# Eval("CodigoEstado").ToString() == "REG" %>'
                                        CssClass="btn-zf btn-primary-zf">
                                        <i class="ph ph-pencil"></i> Editar
                                    </asp:HyperLink>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>

    </div>
</asp:Content>
