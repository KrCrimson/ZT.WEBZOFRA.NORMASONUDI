<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Bandeja.aspx.cs" Inherits="Bandeja" Title="Bandeja de Trámites" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <title>Bandeja de Trámites</title>
    <link rel="stylesheet" href="../styles.css" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <h2><asp:Label ID="LblBienvenida" runat="server"></asp:Label></h2>
            <br />
            <asp:Button ID="BtnNuevoTramite" runat="server" Text="+ Nuevo Trámite" OnClick="BtnNuevoTramite_Click" CssClass="btn" />
        </div>
    </form>
</body>
</html>