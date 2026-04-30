<%@ Page Language="C#" AutoEventWireup="true" CodeFile="FirmaDigital.aspx.cs" Inherits="FirmaDigital" Title="Firma Digital" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <title>Firma Digital - ZOFRATACNA</title>
    <link rel="stylesheet" href="../styles.css" />
    <style>
        .firma-container { max-width: 900px; margin: 0 auto; }
        
        .step-card {
            background: white;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            padding: 1.5rem;
            margin-bottom: 1.5rem;
            display: flex;
            align-items: center;
            gap: 1.5rem;
        }
        .step-number {
            background: #1e3a8a;
            color: white;
            width: 40px;
            height: 40px;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: bold;
            font-size: 1.2rem;
            flex-shrink: 0;
        }
        .step-content { flex: 1; }
        .step-content h3 { margin: 0 0 0.5rem 0; border: none; padding: 0; color: #1e3a8a; }
        .step-content p { margin: 0; color: #64748b; font-size: 0.9rem; }

        .doc-summary {
            background: #f1f5f9;
            padding: 1rem;
            border-radius: 6px;
            margin-bottom: 2rem;
            border-left: 4px solid #1e3a8a;
        }
        
        .upload-box {
            margin-top: 1rem;
            padding: 1rem;
            border: 2px dashed #cbd5e1;
            border-radius: 6px;
            background: #fdfdfd;
        }
        
        .btn-descargar { background: #0f172a; }
        .btn-descargar:hover { background: #1e293b; }
        
        .success-panel {
            text-align: center;
            padding: 3rem;
            background: white;
            border-radius: 12px;
            box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1);
        }
        .success-icon { font-size: 4rem; color: #059669; margin-bottom: 1rem; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="app-shell">
            <aside class="app-sidebar">
                <div class="sidebar-brand">
                    <p class="brand-title">ZOFRATACNA</p>
                    <p class="brand-subtitle">Sistema de Gestion</p>
                </div>
                <div class="sidebar-nav">
                    <% string rol = Session["strRol"] != null ? Session["strRol"].ToString() : ""; %>
                    <% if (rol == "REGISTRADOR") { %>
                        <div class="sidebar-section">Registro</div>
                        <a class="sidebar-link" href="<%= ResolveUrl("~/Tramites/RegistrarTramite.aspx") %>">Registrar Documento</a>
                        <a class="sidebar-link" href="<%= ResolveUrl("~/Tramites/Bandeja.aspx") %>">Mis Tramites</a>
                    <% } %>

                    <% if (rol == "FIRMADOR") { %>
                        <div class="sidebar-section">Revision</div>
                        <a class="sidebar-link" href="<%= ResolveUrl("~/Tramites/Bandeja.aspx?filtro=EN_REV") %>">Pendientes de Revision</a>
                        <a class="sidebar-link active" href="<%= ResolveUrl("~/Tramites/Bandeja.aspx?filtro=APR_FIRMA,EN_FIRMA") %>">Pendientes de Firma</a>
                        <a class="sidebar-link" href="<%= ResolveUrl("~/Tramites/Bandeja.aspx?filtro=FIRM_COM") %>">Completados</a>
                    <% } %>

                    <% if (rol == "ADMIN") { %>
                        <div class="sidebar-section">Administracion</div>
                        <a class="sidebar-link" href="<%= ResolveUrl("~/Tramites/Bandeja.aspx") %>">Todos los Tramites</a>
                        <a class="sidebar-link" href="<%= ResolveUrl("~/Admin/Dashboard.aspx") %>">Gestionar Roles</a>
                    <% } %>
                </div>
                <div class="sidebar-footer">
                    <a class="sidebar-link" href="<%= ResolveUrl("~/Login.aspx?logout=1") %>">Cerrar Sesion</a>
                </div>
            </aside>

            <div class="app-main">
                <div class="app-header">
                    <p class="page-title">Firma Digital</p>
                    <p class="page-subtitle">Siga los pasos para firmar el documento.</p>
                </div>
                <div class="app-content">
                    <div class="firma-container">
            
            <asp:Panel ID="PnlFirma" runat="server">
                <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem;">
                    <h2 style="margin:0;">Proceso de Firma Digital</h2>
                    <asp:LinkButton ID="LnkVolver" runat="server" OnClick="LnkVolver_Click" CssClass="btn btn-secondary" style="font-size:0.85rem;">Volver al Detalle</asp:LinkButton>
                </div>

                <div class="doc-summary">
                    <strong>Documento:</strong> <asp:Label ID="LblCodigoDoc" runat="server"></asp:Label><br />
                    <strong>Asunto:</strong> <asp:Label ID="LblAsunto" runat="server"></asp:Label>
                </div>

                <asp:Label ID="LblError" runat="server" CssClass="alert alert-danger" Visible="false" style="display:block; margin-bottom:1rem;"></asp:Label>

                <!-- PASO 1 -->
                <div class="step-card">
                    <div class="step-number">1</div>
                    <div class="step-content">
                        <h3>Descargar Documento</h3>
                        <p>Obtenga el archivo PDF original para proceder con la firma.</p>
                    </div>
                    <div>
                        <asp:Button ID="BtnDescargar" runat="server" Text="Descargar PDF" OnClick="BtnDescargar_Click" CssClass="btn btn-descargar" />
                    </div>
                </div>

                <!-- PASO 2 -->
                <div class="step-card">
                    <div class="step-number">2</div>
                    <div class="step-content">
                        <h3>Firmar con ReFirma / Firma Peru</h3>
                        <p>Abra el PDF descargado en su aplicacion de firma instalada y aplique su firma digital con su DNIe o Token.</p>
                    </div>
                    <div style="font-size: 2rem;"></div>
                </div>

                <!-- PASO 3 -->
                <div class="step-card">
                    <div class="step-number">3</div>
                    <div class="step-content">
                        <h3>Subir Archivo Firmado</h3>
                        <p>Seleccione el archivo PDF resultante despues de haberlo firmado digitalmente.</p>
                        
                        <div class="upload-box">
                            <label style="display:block; margin-bottom:0.5rem; font-weight:600;">Archivo Firmado (.pdf):</label>
                            <asp:FileUpload ID="FuPdfFirmado" runat="server" accept=".pdf" style="width:100%; margin-bottom:1rem;" />
                            
                            <label style="display:block; margin-bottom:0.5rem; font-weight:600;">Motivo de Firma (opcional):</label>
                            <asp:TextBox ID="TxtMotivo" runat="server" placeholder="Ej: Aprobacion de contenido" CssClass="form-control" style="width:100%; margin-bottom:1rem;"></asp:TextBox>
                            
                            <asp:Button ID="BtnSubirFirma" runat="server" Text="Registrar Firma Digital" OnClick="BtnSubirFirma_Click" CssClass="btn" style="width:100%; padding: 0.8rem;" />
                        </div>
                    </div>
                </div>
            </asp:Panel>

            <asp:Panel ID="PnlExito" runat="server" Visible="false">
                <div class="success-panel">
                    <div class="success-icon">OK</div>
                    <h2>Exito! Firma Registrada</h2>
                    <p style="color: #64748b; font-size: 1.1rem; margin-bottom: 2rem;">
                        <asp:Label ID="LblMensajeExito" runat="server"></asp:Label>
                    </p>
                    <asp:Button ID="BtnIrBandeja" runat="server" Text="Ir a mi Bandeja" OnClick="BtnIrBandeja_Click" CssClass="btn" style="padding: 0.8rem 2.5rem;" />
                </div>
            </asp:Panel>

                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
