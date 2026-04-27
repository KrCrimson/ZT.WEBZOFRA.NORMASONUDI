<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" Title="Acceso - Sistema Firmador" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Acceso - Sistema Firmador</title>
    <link rel="stylesheet" href="styles.css" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="container" style="max-width: 400px; margin-top: 5rem;">
            <h2>Acceso al Sistema</h2>
            <label>Seleccione Usuario:</label>
            <asp:DropDownList ID="CbxUsuario" runat="server"></asp:DropDownList>
            <asp:Button ID="BtnIngresar" runat="server" Text="Ingresar" OnClick="BtnIngresar_Click" CssClass="btn" Width="100%" />
            <br /><br />
            <asp:Label ID="LblError" runat="server" Visible="false"></asp:Label>
        </div>
    </form>
</body>
</html>