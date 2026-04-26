<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="ZT.WEBZOFRA.NORMASONUDI.Login" Title="Acceso - Sistema Firmador" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Acceso - Sistema Firmador</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h2>Acceso - Sistema Firmador</h2>
            <asp:DropDownList ID="CbxUsuario" runat="server">
            </asp:DropDownList>
            <br /><br />
            <asp:Button ID="BtnIngresar" runat="server" Text="Ingresar" OnClick="BtnIngresar_Click" />
            <br /><br />
            <asp:Label ID="LblError" runat="server" ForeColor="Red" Visible="false"></asp:Label>
        </div>
    </form>
</body>
</html>