<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Shared/MasterPage.master" CodeFile="DetalleAdmin.aspx.cs" Inherits="DetalleAdmin" Title="Detalle Admin" %>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PageTitle" runat="server">
    <h1 class="page-title">
        <asp:Label ID="LblCodigoDoc" runat="server"></asp:Label>
    </h1>
</asp:Content>

<asp:Content ID="PageStatus" ContentPlaceHolderID="PageStatus" runat="server">
    <asp:Literal ID="LblEstadoBadge" runat="server"></asp:Literal>
</asp:Content>

<asp:Content ID="HeaderActions" ContentPlaceHolderID="HeaderActions" runat="server">
    <a href="Dashboard.aspx" class="btn btn-secondary">Volver al Dashboard</a>
</asp:Content>

<asp:Content ID="SidebarContent" ContentPlaceHolderID="SidebarContent" runat="server">
    <a href="Dashboard.aspx" class="nav-btn"><i class="ph ph-house"></i> Dashboard</a>
    <a href="Dashboard.aspx" class="nav-btn"><i class="ph ph-list"></i> Tramites</a>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container">
        <asp:Label ID="LblError" runat="server" Visible="false" ForeColor="Red"></asp:Label>

        <h3>Info</h3>
        <div class="info-grid">
            <div><b>Asunto:</b> <br/><asp:Label ID="LblAsunto" runat="server"></asp:Label></div>
            <div><b>Tipo:</b> <br/><asp:Label ID="LblTipoDoc" runat="server"></asp:Label></div>
            <div><b>Area:</b> <br/><asp:Label ID="LblArea" runat="server"></asp:Label></div>
            <div><b>Fecha:</b> <br/><asp:Label ID="LblFechaDoc" runat="server"></asp:Label></div>
            <div><b>Registrador:</b> <br/><asp:Label ID="LblRegistrador" runat="server"></asp:Label></div>
        </div>

        <h3>Revisores</h3>
        <asp:GridView ID="GvRevisores" runat="server" AutoGenerateColumns="false" CssClass="grid-view" EmptyDataText="Sin revisores.">
            <Columns>
                <asp:BoundField DataField="NombreRevisor" HeaderText="Revisor" />
                <asp:BoundField DataField="CorreoRevisor" HeaderText="Correo" />
                <asp:BoundField DataField="CodigoResultado" HeaderText="Resultado" />
            </Columns>
        </asp:GridView>

        <asp:Panel ID="PnlObservaciones" runat="server" Visible="false">
            <h3>Observaciones</h3>
            <asp:Repeater ID="RptObservaciones" runat="server">
                <ItemTemplate>
                    <div class="obs-card">
                        <strong><%# Eval("NombreRevisor") %></strong> <small>(<%# Eval("FechaCreacion", "{0:dd/MM/yyyy HH:mm}") %>)</small>
                        <p><%# Eval("Descripcion") %></p>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </asp:Panel>

        <h3>PDF</h3>
        <asp:HiddenField ID="HfRutaPDF" runat="server" />
        <iframe id="IframePDF" runat="server" class="pdf-frame" src="about:blank"></iframe>
    </div>
</asp:Content>