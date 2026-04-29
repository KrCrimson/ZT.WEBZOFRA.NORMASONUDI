<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Detalle.aspx.cs" Inherits="Detalle" Title="Detalle del Documento" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <title>Detalle del Documento</title>
    <link rel="stylesheet" href="../styles.css" />
    <style>
        body { display: block; padding: 1.5rem; }
        .container { max-width: 1100px; margin: 0 auto; }

        .detalle-header {
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            flex-wrap: wrap;
            gap: 1rem;
            margin-bottom: 1.5rem;
        }
        .detalle-header h2 { margin-bottom: 0; border-bottom: none; padding-bottom: 0; }

        .btn-volver {
            background: var(--text-muted);
            font-size: 0.85rem;
            padding: 0.5rem 1.2rem;
        }
        .btn-volver:hover { background: #475569; }

        /* Info grid */
        .info-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 1rem;
            margin-bottom: 1.5rem;
        }
        .info-item label {
            font-size: 0.75rem;
            color: var(--text-muted);
            text-transform: uppercase;
            margin-bottom: 2px;
        }
        .info-item .info-value {
            font-size: 0.95rem;
            font-weight: 500;
            color: var(--text-main);
        }

        /* Badge */
        .badge-estado {
            display: inline-block;
            padding: 3px 10px;
            border-radius: 20px;
            font-size: 0.72rem;
            font-weight: 600;
            text-transform: uppercase;
        }
        .badge-EN_REV   { background: #fef9c3; color: #854d0e; }
        .badge-OBS      { background: #fee2e2; color: #991b1b; }
        .badge-APR_FIRMA { background: #dbeafe; color: #1e40af; }
        .badge-EN_FIRMA  { background: #ffedd5; color: #9a3412; }
        .badge-FPAR      { background: #ffedd5; color: #9a3412; }
        .badge-FIRM_COM  { background: #dcfce7; color: #166534; }

        /* PDF Preview */
        .pdf-frame {
            width: 100%;
            height: 600px;
            border: 1px solid var(--surface-border);
            border-radius: 8px;
        }

        /* Revisores */
        .rev-conforme td { background: #f0fdf4 !important; }
        .rev-observado td { background: #fef2f2 !important; }
        .rev-pendiente td { background: #f8fafc !important; }

        /* Observaciones */
        .obs-card {
            background: #fffbeb;
            border: 1px solid #fde68a;
            border-radius: 6px;
            padding: 0.8rem 1rem;
            margin-bottom: 0.8rem;
        }
        .obs-card .obs-autor {
            font-weight: 600;
            font-size: 0.85rem;
            color: #92400e;
        }
        .obs-card .obs-fecha {
            font-size: 0.75rem;
            color: var(--text-muted);
            float: right;
        }
        .obs-card .obs-texto {
            margin-top: 0.4rem;
            font-size: 0.9rem;
        }

        /* Acciones */
        .acciones-panel {
            display: flex;
            gap: 0.8rem;
            flex-wrap: wrap;
            align-items: flex-end;
        }
        .acciones-panel textarea {
            width: 100%;
            min-height: 80px;
            padding: 0.65rem 1rem;
            border: 1px solid #cbd5e1;
            border-radius: 6px;
            font-family: 'Roboto', sans-serif;
            font-size: 0.9rem;
            resize: vertical;
            box-sizing: border-box;
        }
        .btn-success { background: #059669; }
        .btn-success:hover { background: #047857; }
        .btn-warning { background: #d97706; }
        .btn-warning:hover { background: #b45309; }
        .btn-danger { background: #dc2626; }
        .btn-danger:hover { background: #b91c1c; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">

            <!-- HEADER -->
            <div class="detalle-header">
                <h2>
                    <asp:Label ID="LblCodigoDoc" runat="server"></asp:Label>
                    <asp:Label ID="LblEstadoBadge" runat="server"></asp:Label>
                </h2>
                <asp:Button ID="BtnVolver" runat="server" Text="<- Volver a Bandeja" CssClass="btn btn-volver"
                    OnClick="BtnVolver_Click" CausesValidation="false" />
            </div>

            <asp:Label ID="LblError" runat="server" Visible="false"></asp:Label>
            <asp:Label ID="LblExito" runat="server" Visible="false"></asp:Label>

            <!-- SECCIÓN 1: INFO DEL TRÁMITE -->
            <div class="section-card">
                <h3>Informacion del Documento</h3>
                <div class="info-grid">
                    <div class="info-item">
                        <label>Asunto</label>
                        <div class="info-value"><asp:Label ID="LblAsunto" runat="server"></asp:Label></div>
                    </div>
                    <div class="info-item">
                        <label>Tipo de Documento</label>
                        <div class="info-value"><asp:Label ID="LblTipoDoc" runat="server"></asp:Label></div>
                    </div>
                    <div class="info-item">
                        <label>Area Responsable</label>
                        <div class="info-value"><asp:Label ID="LblArea" runat="server"></asp:Label></div>
                    </div>
                    <div class="info-item">
                        <label>Fecha del Documento</label>
                        <div class="info-value"><asp:Label ID="LblFechaDoc" runat="server"></asp:Label></div>
                    </div>
                    <div class="info-item">
                        <label>Version</label>
                        <div class="info-value"><asp:Label ID="LblVersion" runat="server"></asp:Label></div>
                    </div>
                    <div class="info-item">
                        <label>Registrador</label>
                        <div class="info-value"><asp:Label ID="LblRegistrador" runat="server"></asp:Label></div>
                    </div>
                    <div class="info-item">
                        <label>Fecha Limite de Revision</label>
                        <div class="info-value"><asp:Label ID="LblFechaLimite" runat="server"></asp:Label></div>
                    </div>
                </div>
            </div>

            <!-- SECCIÓN 2: PDF PREVIEW -->
            <div class="section-card">
                <h3>Vista Previa del Documento</h3>
                <asp:HiddenField ID="HfRutaPDF" runat="server" />
                <iframe id="IframePDF" runat="server" class="pdf-frame" src="about:blank"></iframe>
            </div>

            <!-- SECCIÓN 3: REVISORES -->
            <div class="section-card">
                <h3>Revisores y Estado de Revision</h3>
                <asp:GridView ID="GvRevisores" runat="server" AutoGenerateColumns="false"
                    CssClass="grid-view" OnRowDataBound="GvRevisores_RowDataBound"
                    EmptyDataText="Sin revisores asignados.">
                    <Columns>
                        <asp:BoundField DataField="NombreRevisor" HeaderText="Revisor" />
                        <asp:BoundField DataField="CorreoRevisor" HeaderText="Correo" />
                        <asp:TemplateField HeaderText="Completado">
                            <ItemTemplate>
                                <%# Convert.ToBoolean(Eval("Completado")) ? "Si" : "No" %>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Resultado">
                            <ItemTemplate>
                                <%# Eval("CodigoResultado") != DBNull.Value ? Eval("CodigoResultado").ToString() : "Pendiente" %>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>

            <!-- SECCIÓN 3B: OBSERVACIONES -->
            <asp:Panel ID="PnlObservaciones" runat="server" Visible="false">
                <div class="section-card">
                    <h3>Observaciones</h3>
                    <asp:Repeater ID="RptObservaciones" runat="server">
                        <ItemTemplate>
                            <div class="obs-card">
                                <span class="obs-fecha"><%# Eval("FechaCreacion", "{0:dd/MM/yyyy HH:mm}") %></span>
                                <span class="obs-autor"><%# Eval("NombreRevisor") %></span>
                                <div class="obs-texto"><%# Eval("Descripcion") %></div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </asp:Panel>

            <!-- SECCIÓN 4A: ACCIONES FIRMADOR -->
            <asp:Panel ID="PnlAccionesFirmador" runat="server" Visible="false">
                <div class="section-card">
                    <h3>Acciones de Revision</h3>
                    <div class="acciones-panel">
                        <div style="flex: 1; min-width: 250px;">
                            <label>Observacion (opcional para dar conformidad):</label>
                            <asp:TextBox ID="TxtObservacion" runat="server" TextMode="MultiLine" MaxLength="2000"
                                placeholder="Escriba su observacion..." Rows="3"></asp:TextBox>
                        </div>
                    </div>
                    <div style="margin-top: 1rem; display: flex; gap: 0.8rem;">
                        <asp:Button ID="BtnObservar" runat="server" Text="Registrar Observacion"
                            CssClass="btn btn-danger" OnClick="BtnObservar_Click" CausesValidation="false" />
                        <asp:Button ID="BtnConforme" runat="server" Text="Dar Visto Bueno"
                            CssClass="btn btn-success" OnClick="BtnConforme_Click" CausesValidation="false" />
                        <asp:Button ID="BtnIrAFirmar" runat="server" Text="Proceder a Firmar"
                            CssClass="btn" style="background:#1e3a8a;" OnClick="BtnIrAFirmar_Click" CausesValidation="false" Visible="false" />
                    </div>
                </div>
            </asp:Panel>

            <!-- SECCIÓN 4B: ACCIONES REGISTRADOR -->
            <asp:Panel ID="PnlAccionesRegistrador" runat="server" Visible="false">
                <div class="section-card">
                    <h3>Acciones del Registrador</h3>

                    <asp:Button ID="BtnRecordar" runat="server" Text="Enviar Recordatorio"
                        CssClass="btn btn-warning" OnClick="BtnRecordar_Click" CausesValidation="false" Visible="false" />

                    <asp:Panel ID="PnlCorreccion" runat="server" Visible="false">
                        <div style="margin-top: 1rem;">
                            <label>Subir PDF Corregido:</label>
                            <asp:FileUpload ID="FuPdfCorregido" runat="server" accept=".pdf" />
                            <asp:Button ID="BtnCorregir" runat="server" Text="Subir PDF Corregido"
                                CssClass="btn" OnClick="BtnCorregir_Click" CausesValidation="false" style="margin-top: 0.8rem;" />
                        </div>
                    </asp:Panel>
                </div>
            </asp:Panel>

        </div>
    </form>
</body>
</html>
