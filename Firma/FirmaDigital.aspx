<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FirmaDigital.aspx.cs" Inherits="Firma.FirmaDigital" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <title>Firma Digital - ZOFRATACNA</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; background-color: #f4f4f4; }
        .container { background: white; padding: 24px; border-radius: 8px; box-shadow: 0 2px 5px rgba(0,0,0,0.1); max-width: 940px; margin: auto; }
        h2 { color: #003366; margin-top: 0; }
        label { font-weight: bold; display: block; margin-bottom: 4px; }
        .field { margin-bottom: 16px; }
        .cert-row { display: flex; gap: 8px; align-items: flex-start; }
        .cert-row select { flex: 1; padding: 6px; font-size: 13px; border: 1px solid #ccc; border-radius: 4px; }
        .info { font-size: 12px; color: #555; margin-top: 4px; }
        input[type=file] { display: block; margin-top: 4px; }
        .btn       { background-color: #0056b3; color: white; padding: 10px 22px; border: none; cursor: pointer; border-radius: 4px; font-size: 15px; }
        .btn:hover { background-color: #004494; }
        .btn-sm    { background-color: #6c757d; color: white; padding: 6px 12px; border: none; cursor: pointer; border-radius: 4px; font-size: 13px; white-space: nowrap; }
        .btn-sm:hover { background-color: #5a6268; }
        .status    { margin-top: 20px; padding: 12px; border: 1px solid #ccc; border-radius: 4px; }
        .download  { display: inline-block; margin-top: 8px; font-weight: bold; color: #0056b3; }
        .nota-pin  { font-size: 12px; color: #777; margin-top: 6px; font-style: italic; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <h2>Firma Digital &mdash; DNIe / Token</h2>

            <div class="field">
                <label>Certificado disponible:</label>
                <div class="cert-row">
                    <asp:DropDownList ID="ddlCertificados" runat="server" />
                    <asp:Button ID="btnRefrescar" runat="server" Text="Refrescar" CssClass="btn-sm"
                        OnClick="btnRefrescar_Click" CausesValidation="false" />
                </div>
                <asp:Label ID="lblCertInfo" runat="server" CssClass="info" />
            </div>

            <div class="field">
                <label>Documento PDF a firmar:</label>
                <asp:FileUpload ID="fileUploadPdf" runat="server" />
            </div>

            <asp:Button ID="btnFirmar" runat="server" Text="Firmar Documento" CssClass="btn" OnClick="btnFirmar_Click" />
            <p class="nota-pin">El dispositivo solicitará el PIN mediante su propio diálogo de seguridad.</p>

            <asp:Panel ID="pnlResultado" runat="server" Visible="false" CssClass="status">
                <asp:Label ID="lblMensaje" runat="server" />
                <br />
                <asp:Label ID="lblFirmaVisible" runat="server" Visible="false"
                    Text="✔ La firma aparece visible en la esquina inferior izquierda de la primera página del PDF."
                    style="font-size:12px; color:#2c6e2c; display:block; margin-top:6px;" />
                <asp:HyperLink ID="lnkDescargar" runat="server" Visible="false" CssClass="download">
                    Descargar PDF Firmado
                </asp:HyperLink>
            </asp:Panel>
        </div>
    </form>
</body>
</html>
