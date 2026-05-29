using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Web.Script.Serialization;
using ZT.WEBZOFRA.NORMASONUDI;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Web.UI.WebControls;

public partial class Detalle : System.Web.UI.Page
{
    private class PdfAnnotation
    {
        public int page { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float w { get; set; }
        public float h { get; set; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["strUsuario"] == null)
        {
            Response.Redirect("~/Login.aspx");
            return;
        }

        if (string.IsNullOrEmpty(Request.QueryString["id"]))
        {
            Response.Redirect("~/Tramites/Bandeja.aspx");
            return;
        }

        if (!IsPostBack)
        {
            int idDoc = Convert.ToInt32(Request.QueryString["id"]);
            ViewState["IDDocumento"] = idDoc;
            CargarDocumento(idDoc);
            CargarVersiones(idDoc);
            CargarRevisores(idDoc);
            CargarFirmantes(idDoc);
            CargarObservaciones(idDoc);
            ConfigurarAcciones(idDoc);

            string rol = Session["strRol"] != null ? Session["strRol"].ToString() : "";
            if (rol == "FIRMADOR")
            {
                string estado = ViewState["Estado"] != null ? ViewState["Estado"].ToString() : "";
                bool puedeObservar = (estado == "EN_REV" || estado == "OBS");
                if (PnlPdfAnnotator != null) PnlPdfAnnotator.Visible = puedeObservar;
                if (PnlPdfViewer != null) PnlPdfViewer.Visible = !puedeObservar;
            }
            else
            {
                if (PnlPdfAnnotator != null) PnlPdfAnnotator.Visible = false;
                if (PnlPdfViewer != null) PnlPdfViewer.Visible = true;
            }

            if (PnlVersiones != null) PnlVersiones.Visible = (rol == "ADMIN");
        }
    }

    private void CargarDocumento(int idDoc)
    {
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        try
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                  string query = @"SELECT d.IDDocumento, d.CodigoDocumento, d.Asunto,
                                 d.CodigoTipoDocumento, m.Descripcion AS TipoDocumento,
                                 d.AreaResponsable, d.FechaDocumento, d.CodigoEstado,
                                 me.Descripcion AS Estado, d.Version,
                                 ISNULL(
                                     (SELECT TOP 1 RutaFirmaParcial
                                      FROM FIR_DocumentoFirmante
                                      WHERE IDDocumento = d.IDDocumento AND CodigoEstadoFirma = 'FIR'
                                      ORDER BY OrdenFirma DESC),
                                     d.RutaArchivoPDF
                                 ) AS RutaArchivoPDF,
                                 d.IDArchivoPDF, d.LoginRegistrador,
                                 e.NombreCompleto AS NombreRegistrador,
                                 d.FechaLimiteRevision
                             FROM FIR_Documento d
                             LEFT JOIN FIR_Maestro m ON m.Tipo='TIPO_DOC' AND m.Codigo=d.CodigoTipoDocumento
                             LEFT JOIN FIR_Maestro me ON me.Tipo='ESTADO_DOC' AND me.Codigo=d.CodigoEstado
                             LEFT JOIN FIR_VW_EmpleadosActivos e ON e.LoginUsuario = d.LoginRegistrador
                             WHERE d.IDDocumento = @IDDocumento";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IDDocumento", idDoc);
                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            LblCodigoDoc.Text = dr["CodigoDocumento"].ToString();
                            LblAsunto.Text = dr["Asunto"].ToString();
                            LblTipoDoc.Text = dr["TipoDocumento"] != DBNull.Value ? dr["TipoDocumento"].ToString() : dr["CodigoTipoDocumento"].ToString();
                            LblArea.Text = dr["AreaResponsable"].ToString();
                            LblFechaDoc.Text = Convert.ToDateTime(dr["FechaDocumento"]).ToString("dd/MM/yyyy");
                            LblVersion.Text = "v" + dr["Version"].ToString();
                            ViewState["Version"] = dr["Version"] != DBNull.Value ? Convert.ToInt32(dr["Version"]) : 1;
                            LblRegistrador.Text = dr["NombreRegistrador"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["NombreRegistrador"].ToString())
                                ? dr["NombreRegistrador"].ToString()
                                : dr["LoginRegistrador"].ToString();

                            string codigoEstado = dr["CodigoEstado"].ToString();
                            string descripcionEstado = dr["Estado"] != DBNull.Value ? dr["Estado"].ToString() : codigoEstado;
                            LblEstadoBadge.Text = "<span class='badge-estado badge-" + codigoEstado + "'>" + descripcionEstado + "</span>";

                            ViewState["Estado"] = codigoEstado;
                            ViewState["LoginRegistrador"] = dr["LoginRegistrador"].ToString();

                            string rutaPDF = dr["RutaArchivoPDF"] != DBNull.Value ? dr["RutaArchivoPDF"].ToString() : "";
                            if (rutaPDF.StartsWith("ARC::"))
                            {
                                string idArchivo = rutaPDF.Replace("ARC::", "");
                                HfRutaPDF.Value = idArchivo;
                                IframePDF.Attributes["src"] = ResolveUrl("~/Handlers/VerPDF.ashx?id=" + idArchivo);
                            }

                            if (dr["FechaLimiteRevision"] != DBNull.Value)
                            {
                                DateTime fechaLimite = Convert.ToDateTime(dr["FechaLimiteRevision"]);
                                TimeSpan diff = fechaLimite - DateTime.Now;
                                if (diff.Days <= 0)
                                {
                                    LblFechaLimite.Text = "<span style='color:#dc2626;font-weight:700;'>VENCIDA (" + fechaLimite.ToString("dd/MM/yyyy") + ")</span>";
                                }
                                else if (diff.Days <= 7)
                                {
                                    LblFechaLimite.Text = "<span style='color:#dc2626;font-weight:600;'>" + fechaLimite.ToString("dd/MM/yyyy") + " (" + diff.Days + " dias restantes)</span>";
                                }
                                else
                                {
                                    LblFechaLimite.Text = fechaLimite.ToString("dd/MM/yyyy") + " (" + diff.Days + " dias restantes)";
                                }
                            }
                            else
                            {
                                LblFechaLimite.Text = "<span style='color:#64748b;'>Sin fecha limite</span>";
                            }
                            if (codigoEstado == "FIRM_COM")
                            {
                                PnlDocumentoFinal.Visible = true;
                            }
                        }
                        else
                        {
                            LblError.Text = "Documento no encontrado.";
                            LblError.Visible = true;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LblError.Text = "Error al cargar documento: " + ex.Message;
            LblError.Visible = true;
        }
    }

    protected void BtnDescargarFinal_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(HfRutaPDF.Value))
        {
            Response.Redirect("~/Handlers/VerPDF.ashx?id=" + HfRutaPDF.Value);
        }
    }

    private void CargarVersiones(int idDoc)
    {
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        try
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT v.NumeroVersion, v.RutaArchivo, v.Motivo, v.FechaCreacion
                                 FROM FIR_VersionDocumento v
                                 WHERE v.IDDocumento = @IDDocumento
                                 ORDER BY v.NumeroVersion DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IDDocumento", idDoc);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    GvVersiones.DataSource = dt;
                    GvVersiones.DataBind();
                }
            }
        }
        catch (Exception ex)
        {
            LblError.Text = "Error al cargar versiones: " + ex.Message;
            LblError.Visible = true;
        }
    }

    protected void GvVersiones_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            Label lblVersionGrid = (Label)e.Row.FindControl("LblVersionGrid");
            HyperLink lnkVerPDF = (HyperLink)e.Row.FindControl("LnkVerPDF");
            DataRowView drv = (DataRowView)e.Row.DataItem;

            if (lblVersionGrid != null)
                lblVersionGrid.Text = "v" + drv["NumeroVersion"].ToString();

            if (lnkVerPDF != null)
            {
                string ruta = drv["RutaArchivo"] != DBNull.Value ? drv["RutaArchivo"].ToString() : "";
                if (ruta.StartsWith("ARC::"))
                {
                    string idArchivo = ruta.Replace("ARC::", "");
                    lnkVerPDF.NavigateUrl = "~/Handlers/VerPDF.ashx?id=" + idArchivo;
                }
            }
        }
    }

    private void CargarRevisores(int idDoc)
    {
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        try
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT dr.LoginUsuario, dr.NombreRevisor, dr.CorreoRevisor,
                                        dr.Completado, dr.CodigoResultado
                                 FROM FIR_DocumentoRevisor dr
                                 WHERE dr.IDDocumento = @IDDocumento
                                   AND dr.Version = (SELECT Version FROM FIR_Documento WHERE IDDocumento = @IDDocumento)
                                 ORDER BY dr.IDDocumentoRevisor";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IDDocumento", idDoc);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    GvRevisores.DataSource = dt;
                    GvRevisores.DataBind();

                    string loginActual = Session["strUsuario"].ToString();
                    bool yaReviso = false;
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row["LoginUsuario"].ToString() == loginActual && Convert.ToBoolean(row["Completado"]))
                        {
                            yaReviso = true;
                            break;
                        }
                    }
                    ViewState["YaReviso"] = yaReviso;
                }
            }
        }
        catch (Exception ex)
        {
            LblError.Text = "Error al cargar revisores: " + ex.Message;
            LblError.Visible = true;
        }
    }

    private void CargarObservaciones(int idDoc)
    {
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        try
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string rol = Session["strRol"] != null ? Session["strRol"].ToString() : "";
                                string query = @"SELECT o.LoginUsuario, o.NombreRevisor, o.Descripcion, o.FechaCreacion, o.Version,
                                                                                REPLACE(vx.RutaArchivo, 'ARC::', '') AS RutaObservadaId
                                                                 FROM FIR_Observacion o
                                                                 OUTER APPLY (
                                                                         SELECT TOP 1 v.RutaArchivo
                                                                         FROM FIR_VersionDocumento v
                                                                         WHERE v.IDDocumento = o.IDDocumento
                                                                             AND v.NumeroVersion = o.Version
                                                                             AND v.Motivo LIKE 'Observaciones v%'
                                                                             AND v.IDUsuarioCreador = o.LoginUsuario
                                                                         ORDER BY ABS(DATEDIFF(SECOND, v.FechaCreacion, o.FechaCreacion))
                                                                 ) vx
                                                                 WHERE o.IDDocumento = @IDDocumento";
                if (rol == "FIRMADOR")
                {
                    query += " AND Version = (SELECT Version FROM FIR_Documento WHERE IDDocumento = @IDDocumento)";
                }
                query += " ORDER BY FechaCreacion DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IDDocumento", idDoc);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    int versionActual = ViewState["Version"] != null ? (int)ViewState["Version"] : 1;
                    if (!dt.Columns.Contains("ObservacionCorregida"))
                    {
                        dt.Columns.Add("ObservacionCorregida", typeof(bool));
                    }
                    foreach (DataRow row in dt.Rows)
                    {
                        int versionObs = 1;
                        if (row["Version"] != DBNull.Value)
                        {
                            int.TryParse(row["Version"].ToString(), out versionObs);
                        }
                        row["ObservacionCorregida"] = versionActual > versionObs;
                    }

                    if (dt.Rows.Count > 0)
                    {
                        PnlObservaciones.Visible = true;
                        RptObservaciones.DataSource = dt;
                        RptObservaciones.DataBind();
                    }
                    else
                    {
                        PnlObservaciones.Visible = false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LblError.Text = "Error al cargar observaciones: " + ex.Message;
            LblError.Visible = true;
        }
    }

    private void CargarFirmantes(int idDoc)
    {
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        try
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT OrdenFirma, NombreFirmante, CorreoFirmante, CodigoEstadoFirma
                                 FROM FIR_DocumentoFirmante
                                 WHERE IDDocumento = @IDDocumento
                                 ORDER BY OrdenFirma";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IDDocumento", idDoc);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        PnlFirmantesEstado.Visible = true;
                        GvFirmantesEstado.DataSource = dt;
                        GvFirmantesEstado.DataBind();
                    }
                    else
                    {
                        PnlFirmantesEstado.Visible = false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LblError.Text = "Error al cargar firmantes: " + ex.Message;
            LblError.Visible = true;
        }
    }

    private void ConfigurarAcciones(int idDoc)
    {
        string estado = ViewState["Estado"] != null ? ViewState["Estado"].ToString() : "";
        string rol = Session["strRol"] != null ? Session["strRol"].ToString() : "";
        string loginActual = Session["strUsuario"].ToString();
        string loginRegistrador = ViewState["LoginRegistrador"] != null ? ViewState["LoginRegistrador"].ToString() : "";
        bool yaReviso = ViewState["YaReviso"] != null && (bool)ViewState["YaReviso"];

        // Acciones de Revisión
        PnlAccionesFirmador.Visible = (rol == "FIRMADOR") && (estado == "EN_REV" || estado == "OBS") && !yaReviso;

        // Acciones de Firma (Nuevo)
        bool esTurnoFirma = false;
        if (rol == "FIRMADOR" && (estado == "APR_FIRMA" || estado == "EN_FIRMA" || estado == "FPAR"))
        {
            string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "SELECT COUNT(1) FROM FIR_DocumentoFirmante WHERE IDDocumento=@IDD AND LoginUsuario=@LU AND Habilitado=1 AND CodigoEstadoFirma='PEN'";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@IDD", idDoc);
                    cmd.Parameters.AddWithValue("@LU", loginActual);
                    conn.Open();
                    esTurnoFirma = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }
        
        if (esTurnoFirma)
        {
            PnlAccionesFirmador.Visible = true;
            BtnIrAFirmar.Visible = true;
            if (LnkFirmarUsb != null)
            {
                LnkFirmarUsb.Visible = true;
                LnkFirmarUsb.NavigateUrl = "~/Tramites/FirmaUsbToken.aspx?id=" + idDoc;
            }
            BtnConforme.Visible = false;
            BtnObservar.Visible = false;
            TxtObservacion.Visible = false;
            if (DivObservacionUI != null) DivObservacionUI.Visible = false;
            if (TituloAcciones != null) TituloAcciones.InnerText = "Acciones de Firma";
        }

        PnlAccionesRegistrador.Visible = ((rol == "REGISTRADOR") || (rol == "ADMIN")) && (loginRegistrador == loginActual || rol == "ADMIN");
        BtnRecordar.Visible = (estado == "EN_REV");
        PnlCorreccion.Visible = (estado == "OBS");
    }

    protected void BtnIrAFirmar_Click(object sender, EventArgs e)
    {
        int idDoc = (int)ViewState["IDDocumento"];
        string loginActual = Session["strUsuario"].ToString();
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;

        try
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("FIR_X_IniciarFirmado_OUT", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IDDocumento", idDoc);
                    cmd.Parameters.AddWithValue("@LoginUsuario", loginActual);
                    cmd.Parameters.AddWithValue("@IDEquipo", Request.UserHostAddress);
                    
                    SqlParameter pOut = new SqlParameter("@Iniciado", SqlDbType.Bit);
                    pOut.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(pOut);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            Response.Redirect("~/Tramites/FirmaDigital.aspx?id=" + idDoc);
        }
        catch (Exception ex)
        {
            LblError.Text = "Error al iniciar proceso de firma: " + ex.Message;
            LblError.Visible = true;
        }
    }

    protected void GvRevisores_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            DataRowView drv = (DataRowView)e.Row.DataItem;
            bool completado = Convert.ToBoolean(drv["Completado"]);
            object resultado = drv["CodigoResultado"];
            string codigoRes = resultado != DBNull.Value ? resultado.ToString() : "";

            if (completado && codigoRes == "CONF")
            {
                e.Row.CssClass = "rev-conforme";
            }
            else if (completado && codigoRes == "OBS")
            {
                e.Row.CssClass = "rev-observado";
            }
            else
            {
                e.Row.CssClass = "rev-pendiente";
            }
        }
    }

    private string PlantillaCorreo(string contenido) {
        return @"
<!DOCTYPE html>
<html>
<body style='margin:0;padding:0;font-family:Arial,sans-serif;background:#f5f5f5;'>
  <table width='100%' cellpadding='0' cellspacing='0'>
    <tr>
      <td align='center' style='padding:20px;'>
        <table width='600' cellpadding='0' cellspacing='0' 
               style='background:#fff;border-radius:8px;overflow:hidden;'>
          
          <!-- HEADER -->
          <tr>
            <td style='background:#1a5c38;padding:24px;text-align:center;'>
              <h1 style='color:#fff;margin:0;font-size:18px;
                         letter-spacing:2px;'>
                SISTEMA DE FIRMA ZOFRATACNA
              </h1>
            </td>
          </tr>
          
          <!-- BODY -->
          <tr>
            <td style='padding:32px;color:#333;'>
              <p>Estimado(a),</p>
              " + contenido + @"
              <br/>
              <p>Atentamente,</p>
              <p><b>Oficina de Tecnologías de la Información</b><br/>
              ZOFRATACNA</p>
            </td>
          </tr>
          
          <!-- FOOTER -->
          <tr>
            <td style='background:#222;padding:16px;text-align:center;'>
              <p style='color:#aaa;margin:0;font-size:12px;'>
                Este es un correo automático del Sistema Firmador ZOFRATACNA. 
                Por favor no responda este mensaje.
              </p>
            </td>
          </tr>
          
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    private void EnviarCorreoInvolucrados(int idDocumento, string asunto, string mensaje)
    {
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        try
        {
            System.Collections.Generic.List<string> correos = new System.Collections.Generic.List<string>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string query = @"SELECT DISTINCT e.Email, e.NombreCompleto
                                 FROM FIR_VW_EmpleadosActivos e
                                 WHERE e.LoginUsuario IN (
                                     SELECT LoginRegistrador FROM FIR_Documento WHERE IDDocumento = @IDDocumento
                                     UNION
                                     SELECT LoginUsuario FROM FIR_DocumentoFirmante WHERE IDDocumento = @IDDocumento
                                 )
                                 AND (e.Email NOT LIKE '%zofratacna.com.pe' OR e.Email LIKE '%zofratacna.com.pe')";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IDDocumento", idDocumento);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string email = dr["Email"].ToString();
                            if (!string.IsNullOrWhiteSpace(email))
                            {
                                correos.Add(email);
                            }
                        }
                    }
                }

                string mensajeHTML = PlantillaCorreo(mensaje);

                foreach (string email in correos)
                {
                    try
                    {
                        using (SqlCommand cmdMail = new SqlCommand("GEN_X_EnviarMail", conn))
                        {
                            cmdMail.CommandType = CommandType.StoredProcedure;
                            cmdMail.Parameters.AddWithValue("@Para", email);
                            cmdMail.Parameters.AddWithValue("@Asunto", asunto);
                            cmdMail.Parameters.AddWithValue("@Mensaje", mensajeHTML);
                            cmdMail.ExecuteNonQuery();
                        }
                    }
                    catch { }
                }
            }
        }
        catch { }
    }

    protected void BtnObservar_Click(object sender, EventArgs e)
    {
        LblError.Visible = false;
        LblExito.Visible = false;

        if (string.IsNullOrWhiteSpace(TxtObservacion.Text))
        {
            LblError.Text = "Debe escribir una observación.";
            LblError.Visible = true;
            return;
        }

        int idDoc = (int)ViewState["IDDocumento"];
        string loginActual = Session["strUsuario"].ToString();
        string nombreActual = Session["strNombre"].ToString();
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;

        try
        {
            GuardarPdfConMarcasSiAplica();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("FIR_I_Observacion", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IDDocumento", idDoc);
                    cmd.Parameters.AddWithValue("@LoginUsuario", loginActual);
                    cmd.Parameters.AddWithValue("@NombreRevisor", nombreActual);
                    cmd.Parameters.AddWithValue("@Descripcion", TxtObservacion.Text.Trim());
                    cmd.Parameters.AddWithValue("@IDEquipo", Request.UserHostAddress);
                    cmd.ExecuteNonQuery();
                }
            }

            LblExito.Text = "Observación registrada correctamente.";
            LblExito.Visible = true;
            TxtObservacion.Text = "";

            try
            {
                System.Collections.Generic.List<string> correosRegistrador = new System.Collections.Generic.List<string>();
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string queryReg = @"SELECT e.Email FROM FIR_VW_EmpleadosActivos e
                                        WHERE e.LoginUsuario = (
                                          SELECT LoginRegistrador FROM FIR_Documento 
                                          WHERE IDDocumento = @IDDocumento
                                        )";
                    using (SqlCommand cmdReg = new SqlCommand(queryReg, conn))
                    {
                        cmdReg.Parameters.AddWithValue("@IDDocumento", idDoc);
                        using (SqlDataReader dr = cmdReg.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                string email = dr["Email"].ToString();
                                if (!string.IsNullOrWhiteSpace(email))
                                    correosRegistrador.Add(email);
                            }
                        }
                    }

                    foreach (string email in correosRegistrador)
                    {
                        try
                        {
                            CorreoBLL.NotificarObservacion(email, LblCodigoDoc.Text, LblAsunto.Text, nombreActual);
                        }
                        catch { }
                    }
                }
            }
            catch { }

            CargarRevisores(idDoc);
            CargarVersiones(idDoc);
            CargarObservaciones(idDoc);
            ConfigurarAcciones(idDoc);
        }
        catch (Exception ex)
        {
            LblError.Text = "Error al registrar observación: " + ex.Message;
            LblError.Visible = true;
        }
    }

    private void GuardarPdfConMarcasSiAplica()
    {
        string rol = Session["strRol"] != null ? Session["strRol"].ToString() : "";
        if (rol != "FIRMADOR") return;

        if (string.IsNullOrWhiteSpace(HfPdfAnnotations.Value)) return;

        var jss = new JavaScriptSerializer();
        var marks = jss.Deserialize<System.Collections.Generic.List<PdfAnnotation>>(HfPdfAnnotations.Value);
        if (marks == null || marks.Count == 0) return;

        int idDoc = (int)ViewState["IDDocumento"];
        int idArchivo = 0;
        int.TryParse(HfRutaPDF.Value, out idArchivo);
        if (idArchivo <= 0) return;

        byte[] pdfBytes = ObtenerArchivoPdf(idArchivo);
        if (pdfBytes == null || pdfBytes.Length == 0) return;

        byte[] pdfMarcado = AplicarMarcas(pdfBytes, marks);
        if (pdfMarcado == null || pdfMarcado.Length == 0) return;

        int versionActual = ViewState["Version"] != null ? (int)ViewState["Version"] : 1;
        string codigoDocumento = LblCodigoDoc.Text;
        string nombreArchivoG = codigoDocumento + "_obs_v" + versionActual + ".pdf";
        string loginActual = Session["strUsuario"].ToString();

        int nuevoIdArchivo = GuardarArchivoFirmador(pdfMarcado, nombreArchivoG, loginActual);
        if (nuevoIdArchivo <= 0) return;

        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString))
        {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand(@"INSERT INTO FIR_VersionDocumento (IDDocumento, NumeroVersion, RutaArchivo, Motivo, IDUsuarioCreador)
                                                    VALUES (@IDDocumento, @NumeroVersion, @RutaArchivo, @Motivo, @IDUsuarioCreador)", conn))
            {
                cmd.Parameters.AddWithValue("@IDDocumento", idDoc);
                cmd.Parameters.AddWithValue("@NumeroVersion", versionActual);
                cmd.Parameters.AddWithValue("@RutaArchivo", "ARC::" + nuevoIdArchivo);
                cmd.Parameters.AddWithValue("@Motivo", "Observaciones v" + versionActual);
                cmd.Parameters.AddWithValue("@IDUsuarioCreador", loginActual);
                cmd.ExecuteNonQuery();
            }
        }
    }

    private byte[] ObtenerArchivoPdf(int idArchivo)
    {
        string connArchStr = ConfigurationManager.ConnectionStrings["FirmadorArchivos"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connArchStr))
        {
            using (SqlCommand cmd = new SqlCommand("SELECT Contenido FROM ARC_DocumentoArchivo WHERE IDArchivo = @IDArchivo", conn))
            {
                cmd.Parameters.AddWithValue("@IDArchivo", idArchivo);
                conn.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? (byte[])result : null;
            }
        }
    }

    private int GuardarArchivoFirmador(byte[] contenido, string nombreOriginal, string loginActual)
    {
        string connArchStr = ConfigurationManager.ConnectionStrings["FirmadorArchivos"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connArchStr))
        {
            using (SqlCommand cmd = new SqlCommand("ARC_I_GuardarArchivo_OUT", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Contenido", contenido);
                cmd.Parameters.AddWithValue("@NombreOriginal", nombreOriginal);
                cmd.Parameters.AddWithValue("@TipoArchivo", "PDF_ORIGINAL");
                cmd.Parameters.AddWithValue("@IDUsuarioCreador", loginActual);

                SqlParameter pout = new SqlParameter("@IDArchivo", SqlDbType.Int);
                pout.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(pout);

                conn.Open();
                cmd.ExecuteNonQuery();
                return Convert.ToInt32(pout.Value);
            }
        }
    }

    private byte[] AplicarMarcas(byte[] pdfBytes, System.Collections.Generic.List<PdfAnnotation> marks)
    {
        using (PdfReader reader = new PdfReader(pdfBytes))
        using (MemoryStream ms = new MemoryStream())
        {
            using (PdfStamper stamper = new PdfStamper(reader, ms))
            {
                foreach (var mark in marks)
                {
                    if (mark.page <= 0 || mark.page > reader.NumberOfPages) continue;
                    iTextSharp.text.Rectangle pageSize = reader.GetPageSize(mark.page);

                    float x = mark.x * pageSize.Width;
                    float w = mark.w * pageSize.Width;
                    float h = mark.h * pageSize.Height;
                    float y = pageSize.Height - ((mark.y + mark.h) * pageSize.Height);

                    PdfContentByte cb = stamper.GetOverContent(mark.page);
                    cb.SetColorStroke(BaseColor.RED);
                    cb.SetLineWidth(2f);
                    cb.Rectangle(x, y, w, h);
                    cb.Stroke();
                }
            }
            return ms.ToArray();
        }
    }

    protected void BtnConforme_Click(object sender, EventArgs e)
    {
        LblError.Visible = false;
        LblExito.Visible = false;

        int idDoc = (int)ViewState["IDDocumento"];
        string loginActual = Session["strUsuario"].ToString();
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;

        try
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("FIR_I_Conformidad", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IDDocumento", idDoc);
                    cmd.Parameters.AddWithValue("@LoginUsuario", loginActual);
                    cmd.Parameters.AddWithValue("@NombreRevisor", Session["strNombre"].ToString());
                    cmd.Parameters.AddWithValue("@Comentario", "CONFORME");
                    cmd.Parameters.AddWithValue("@IDEquipo", Request.UserHostAddress);
                    cmd.ExecuteNonQuery();
                }

                using (SqlCommand cmdCierre = new SqlCommand("FIR_X_RevisionCierre_OUT", conn))
                {
                    cmdCierre.CommandType = CommandType.StoredProcedure;
                    cmdCierre.Parameters.AddWithValue("@IDDocumento", idDoc);

                    SqlParameter pCerrado = new SqlParameter("@Cerrado", SqlDbType.Bit);
                    pCerrado.Direction = ParameterDirection.Output;
                    cmdCierre.Parameters.Add(pCerrado);

                    cmdCierre.ExecuteNonQuery();

                    bool cerrado = Convert.ToBoolean(pCerrado.Value);
                    if (cerrado)
                    {
                        LblExito.Text = "Visto bueno registrado. Todos los revisores están conformes — el documento pasó a fase de firma.";

                        try
                        {
                            string queryPrimerFirmante = @"SELECT TOP 1 CorreoFirmante, NombreFirmante, OrdenFirma
                                                          FROM FIR_DocumentoFirmante
                                                          WHERE IDDocumento = @IDDocumento AND Habilitado = 1 AND CodigoEstadoFirma = 'PEN'
                                                          ORDER BY OrdenFirma";
                            using (SqlCommand cmdFirma = new SqlCommand(queryPrimerFirmante, conn))
                            {
                                cmdFirma.Parameters.AddWithValue("@IDDocumento", idDoc);
                                using (SqlDataReader drFirma = cmdFirma.ExecuteReader())
                                {
                                    if (drFirma.Read())
                                    {
                                        string correoFirmante = drFirma["CorreoFirmante"].ToString();
                                        string nombreFirmante = drFirma["NombreFirmante"].ToString();
                                        int ordenFirma = drFirma["OrdenFirma"] != DBNull.Value ? Convert.ToInt32(drFirma["OrdenFirma"]) : 1;
                                        CorreoBLL.NotificarTurnoFirma(correoFirmante, nombreFirmante, LblCodigoDoc.Text, LblAsunto.Text, ordenFirma);
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        LblExito.Text = "Visto bueno registrado correctamente. Faltan revisores por confirmar.";
                    }
                    LblExito.Visible = true;
                }
            }

            CargarDocumento(idDoc);
            CargarVersiones(idDoc);
            CargarRevisores(idDoc);
            CargarObservaciones(idDoc);
            ConfigurarAcciones(idDoc);
        }
        catch (Exception ex)
        {
            LblError.Text = "Error al dar conformidad: " + ex.Message;
            LblError.Visible = true;
        }
    }

    protected void BtnRecordar_Click(object sender, EventArgs e)
    {
        LblError.Visible = false;
        LblExito.Visible = false;

        int idDoc = (int)ViewState["IDDocumento"];
        string codigoDoc = LblCodigoDoc.Text;
        string asunto = LblAsunto.Text;
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        int enviados = 0;

        try
        {
            DataTable dtPendientes = new DataTable();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT NombreRevisor, CorreoRevisor
                                 FROM FIR_DocumentoRevisor
                                 WHERE IDDocumento = @IDDocumento AND Completado = 0
                                   AND Version = (SELECT Version FROM FIR_Documento WHERE IDDocumento = @IDDocumento)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IDDocumento", idDoc);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dtPendientes);
                }
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                foreach (DataRow row in dtPendientes.Rows)
                {
                    string correo = row["CorreoRevisor"].ToString();
                    string nombre = row["NombreRevisor"].ToString();

                    if (string.IsNullOrWhiteSpace(correo)) continue;

                    string mensajeHtml = PlantillaCorreo(
                        "<p>Se le recuerda que tiene pendiente la revisión del documento:</p>" +
                        "<ul>" +
                        "<li><b>Código:</b> " + codigoDoc + "</li>" +
                        "<li><b>Asunto:</b> " + asunto + "</li>" +
                        "</ul>" +
                        "<p>Por favor, ingrese al sistema para completar su revisión.</p>"
                    );

                    using (SqlCommand cmdMail = new SqlCommand("GEN_X_EnviarMail", conn))
                    {
                        cmdMail.CommandType = CommandType.StoredProcedure;
                        cmdMail.Parameters.AddWithValue("@Para", correo);
                        cmdMail.Parameters.AddWithValue("@Asunto", "Recordatorio revisión: " + codigoDoc);
                        cmdMail.Parameters.AddWithValue("@Mensaje", mensajeHtml);
                        cmdMail.ExecuteNonQuery();
                        enviados++;
                    }
                }
            }

            if (enviados > 0)
            {
                LblExito.Text = "Se enviaron " + enviados + " recordatorio(s) a los revisores pendientes.";
            }
            else
            {
                LblExito.Text = "No hay revisores pendientes o sin correo registrado.";
            }
            LblExito.Visible = true;
        }
        catch (Exception ex)
        {
            LblError.Text = "Error al enviar recordatorios: " + ex.Message;
            LblError.Visible = true;
        }
    }

    protected void BtnCorregir_Click(object sender, EventArgs e)
    {
        LblError.Visible = false;
        LblExito.Visible = false;

        if (!FuPdfCorregido.HasFile || Path.GetExtension(FuPdfCorregido.FileName).ToLower() != ".pdf")
        {
            LblError.Text = "Debe seleccionar un archivo PDF válido.";
            LblError.Visible = true;
            return;
        }

        int idDoc = (int)ViewState["IDDocumento"];
        string loginActual = Session["strUsuario"].ToString();
        string ipEquipo = Request.UserHostAddress;

        try
        {
            byte[] archivoPDF = FuPdfCorregido.FileBytes;
            int nuevoIdArchivo = 0;

            int versionActual = 1;
            string connFirStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connFirStr))
            {
                conn.Open();
                using (SqlCommand cmdVer = new SqlCommand("SELECT Version FROM FIR_Documento WHERE IDDocumento = @IDDocumento", conn))
                {
                    cmdVer.Parameters.AddWithValue("@IDDocumento", idDoc);
                    object result = cmdVer.ExecuteScalar();
                    if (result != null) versionActual = Convert.ToInt32(result);
                }
            }

            string codigoDocumento = LblCodigoDoc.Text;
            string nombreArchivoG = codigoDocumento + "_v" + (versionActual + 1) + ".pdf";

            string connArchStr = ConfigurationManager.ConnectionStrings["FirmadorArchivos"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connArchStr))
            {
                using (SqlCommand cmd = new SqlCommand("ARC_I_GuardarArchivo_OUT", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Contenido", archivoPDF);
                    cmd.Parameters.AddWithValue("@NombreOriginal", nombreArchivoG);
                    cmd.Parameters.AddWithValue("@TipoArchivo", "PDF_ORIGINAL");
                    cmd.Parameters.AddWithValue("@IDUsuarioCreador", loginActual);

                    SqlParameter pout = new SqlParameter("@IDArchivo", SqlDbType.Int);
                    pout.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(pout);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    nuevoIdArchivo = Convert.ToInt32(pout.Value);
                }
            }

            using (SqlConnection conn = new SqlConnection(connFirStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("FIR_U_DocumentoCorreccion", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IDDocumento", idDoc);
                    cmd.Parameters.AddWithValue("@NuevaRuta", "ARC::" + nuevoIdArchivo);
                    cmd.Parameters.AddWithValue("@LoginUsuario", loginActual);
                    cmd.Parameters.AddWithValue("@IDEquipo", ipEquipo);
                    cmd.Parameters.AddWithValue("@Motivo", "Corrección v" + versionActual);
                    cmd.ExecuteNonQuery();
                }

                try
                {
                    DataTable dtRevisores = new DataTable();
                    using (SqlCommand cmdRevisores = new SqlCommand(@"SELECT DISTINCT CorreoRevisor
                                                                     FROM FIR_DocumentoRevisor
                                                                     WHERE IDDocumento = @IDDocumento
                                                                       AND Version = (SELECT Version FROM FIR_Documento WHERE IDDocumento = @IDDocumento)
                                                                       AND ISNULL(CorreoRevisor, '') <> ''", conn))
                    {
                        cmdRevisores.Parameters.AddWithValue("@IDDocumento", idDoc);
                        SqlDataAdapter da = new SqlDataAdapter(cmdRevisores);
                        da.Fill(dtRevisores);
                    }

                    System.Collections.Generic.List<string> correosRevisores = new System.Collections.Generic.List<string>();
                    foreach (DataRow row in dtRevisores.Rows)
                    {
                        correosRevisores.Add(row["CorreoRevisor"].ToString());
                    }

                    CorreoBLL.NotificarCorreccionReinicio(correosRevisores, LblCodigoDoc.Text, LblAsunto.Text);
                }
                catch { }
            }

            Response.Redirect("~/Tramites/Detalle.aspx?id=" + idDoc);
        }
        catch (Exception ex)
        {
            LblError.Text = "Error al subir corrección: " + ex.Message;
            LblError.Visible = true;
        }
    }
}
