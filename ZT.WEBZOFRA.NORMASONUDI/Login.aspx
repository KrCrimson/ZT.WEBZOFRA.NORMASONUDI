<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" Title="Acceso - Sistema Firmador" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Acceso - Sistema Firmador | ZOFRATACNA</title>
    <!-- Bootstrap 5 & Icons -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.0/font/bootstrap-icons.css" rel="stylesheet" />
    <link rel="stylesheet" runat="server" href="~/styles.css?v=2" />
    <style>
        .login-logo {
            font-size: 2.5rem;
            color: var(--primary);
            font-weight: 800;
            margin-bottom: 0.5rem;
            display: block;
        }
        .login-subtitle {
            color: #64748b;
            font-size: 0.9rem;
            margin-bottom: 2rem;
            display: block;
        }

        /* Tamaño responsive del login-card */
        .login-card {
            width: clamp(300px, 85%, 500px) !important;
            padding: clamp(2rem, 6vw, 3rem) !important;
            box-sizing: border-box !important;
            margin: 0 auto !important;
        }
    </style>
</head>
<body class="login-body">
    <form id="form1" runat="server" class="login-form-wrapper">
        <div class="login-card text-center">
            <span class="login-logo">ZOFRA<span style="color: var(--accent);">TACNA</span></span>
            <span class="login-subtitle">SISTEMA DE GESTI&Oacute;N DE FIRMA DIGITAL</span>
            
            <div class="text-start mb-4">
                <label class="form-label fw-bold small text-uppercase">Seleccione su cuenta</label>
                <div class="input-group">
                    <span class="input-group-text bg-light border-end-0"><i class="bi bi-person-circle"></i></span>
                    <asp:DropDownList ID="CbxUsuario" runat="server" CssClass="form-select border-start-0"></asp:DropDownList>
                </div>
            </div>

            <asp:Button ID="BtnIngresar" runat="server" Text="Iniciar Sesi&oacute;n" OnClick="BtnIngresar_Click" 
                CssClass="btn btn-primary-zf w-100 py-3 mb-3 shadow-sm" />

            <asp:Label ID="LblError" runat="server" Visible="false" CssClass="alert alert-danger d-block mt-3 small"></asp:Label>
            
            <div class="mt-4 pt-3 border-top">
                <small class="text-muted">© 2026 Zofratacna - &Aacute;rea de TI</small>
            </div>
        </div>
    </form>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>