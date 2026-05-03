<%@ Page Language="C#" AutoEventWireup="true" CodeFile="DetalleAdmin.aspx.cs" Inherits="DetalleAdmin" Title="Detalle Admin" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <title>Detalle del Documento - Admin</title>
    <link rel="stylesheet" href="../styles.css" />
    <style>
        body { display: block; padding: 1.5rem; background: #f1f5f9; }
        .container { max-width: 1100px; margin: 0 auto; background: white; padding: 2rem; border-radius: 8px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }
        .btn-volver { background: #64748b; color: white; padding: 0.5rem 1rem; text-decoration: none; border-radius: 4px; display: inline-block; margin-bottom: 1rem; }
        .badge-estado { padding: 3px 10px; border-radius: 20px; font-size: 0.75rem; font-weight: bold; text-transform: uppercase; }
        .info-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1rem; margin-bottom: 1.5rem; }
        .pdf-frame { width: 100%; height: 600px; border: 1px solid #cbd5e1; border-radius: 8px; }
        table { width: 100%; border-collapse: collapse; margin-bottom: 2rem; }
        th, td { padding: 0.5rem; border: 1px solid #cbd5e1; text-align: left; }
        th { background: #f8fafc; }
        .obs-card { background: #fffbeb; border: 1px solid #fde68a; padding: 1rem; margin-bottom: 1rem; border-radius: 4px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <a href="Dashboard.aspx" class="btn-volver">&larr; Volver al Dashboard</a>
            <h2>
                <asp:Label ID="LblCodigoDoc" runat="server"></asp:Label>
                <asp:Literal ID="LblEstadoBadge" runat="server"></asp:Literal>
            </h2>

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
    </form>
</body>
</html>