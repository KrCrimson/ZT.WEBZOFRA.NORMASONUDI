using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Web.UI.WebControls;

public partial class Detalle : System.Web.UI.Page
{
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
            CargarRevisores(idDoc);
            CargarObservaciones(idDoc);
            ConfigurarAcciones(idDoc);
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
                                        d.RutaArchivoPDF, d.IDArchivoPDF, d.LoginRegistrador,
                                        d.FechaLimiteRevision
                                 FROM FIR_Documento d
                                 LEFT JOIN FIR_Maestro m ON m.Tipo='TIPO_DOC' AND m.Codigo=d.CodigoTipoDocumento
                                 LEFT JOIN FIR_Maestro me ON me.Tipo='ESTADO_DOC' AND me.Codigo=d.CodigoEstado
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
                            LblRegistrador.Text = dr["LoginRegistrador"].ToString();

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
                string query = @"SELECT LoginUsuario, NombreRevisor, Descripcion, FechaCreacion
                                 FROM FIR_Observacion
                                 WHERE IDDocumento = @IDDocumento
                                 ORDER BY FechaCreacion DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IDDocumento", idDoc);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

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

    private void ConfigurarAcciones(int idDoc)
    {
        string estado = ViewState["Estado"] != null ? ViewState["Estado"].ToString() : "";
        string rol = Session["strRol"] != null ? Session["strRol"].ToString() : "";
        string loginActual = Session["strUsuario"].ToString();
        string loginRegistrador = ViewState["LoginRegistrador"] != null ? ViewState["LoginRegistrador"].ToString() : "";
        bool yaReviso = ViewState["YaReviso"] != null && (bool)ViewState["YaReviso"];

        // Acciones de Revisión
        PnlAccionesFirmador.Visible = (rol == "FIRMADOR") && (estado == "EN_REV") && !yaReviso;

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
            BtnConforme.Visible = false;
            BtnObservar.Visible = false;
            TxtObservacion.Visible = false;
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

    protected void BtnVolver_Click(object sender, EventArgs e)
    {
        Response.Redirect("~/Tramites/Bandeja.aspx");
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
            CargarRevisores(idDoc);
            CargarObservaciones(idDoc);
            ConfigurarAcciones(idDoc);
        }
        catch (Exception ex)
        {
            LblError.Text = "Error al registrar observación: " + ex.Message;
            LblError.Visible = true;
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
                            string queryPrimerFirmante = @"SELECT TOP 1 df.LoginUsuario, df.NombreFirmante, df.CorreoFirmante
                                                           FROM FIR_DocumentoFirmante df
                                                           WHERE df.IDDocumento = @IDDocumento AND df.OrdenFirma = 1";
                            using (SqlCommand cmdFir = new SqlCommand(queryPrimerFirmante, conn))
                            {
                                cmdFir.Parameters.AddWithValue("@IDDocumento", idDoc);
                                using (SqlDataReader drFir = cmdFir.ExecuteReader())
                                {
                                    if (drFir.Read())
                                    {
                                        string correoFirmante = drFir["CorreoFirmante"].ToString();
                                        string nombreFirmante = drFir["NombreFirmante"].ToString();
                                        string codigoDoc = LblCodigoDoc.Text;
                                        drFir.Close();

                                        if (!string.IsNullOrWhiteSpace(correoFirmante))
                                        {
                                            string mensajeHtml = "<p>Estimado(a) <b>" + nombreFirmante + "</b>,</p>"
                                                + "<p>El documento <b>" + codigoDoc + "</b> ha sido aprobado por todos los revisores y está listo para su firma.</p>"
                                                + "<p>Por favor, ingrese al sistema para proceder con la firma digital.</p>"
                                                + "<p>Atentamente,<br/>Sistema de Gestión - ZOFRATACNA</p>";

                                            using (SqlCommand cmdMail = new SqlCommand("GEN_X_EnviarMail", conn))
                                            {
                                                cmdMail.CommandType = CommandType.StoredProcedure;
                                                cmdMail.Parameters.AddWithValue("@Para", correoFirmante);
                                                cmdMail.Parameters.AddWithValue("@Asunto", "Documento listo para firma: " + codigoDoc);
                                                cmdMail.Parameters.AddWithValue("@Mensaje", mensajeHtml);
                                                cmdMail.ExecuteNonQuery();
                                            }

                                            LblExito.Text += " Se notificó a " + nombreFirmante + " para iniciar la firma.";
                                        }
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

                    string mensajeHtml = "<p>Estimado(a) <b>" + nombre + "</b>,</p>"
                        + "<p>Se le recuerda que tiene pendiente la revisión del documento:</p>"
                        + "<ul>"
                        + "<li><b>Código:</b> " + codigoDoc + "</li>"
                        + "<li><b>Asunto:</b> " + asunto + "</li>"
                        + "</ul>"
                        + "<p>Por favor, ingrese al sistema para completar su revisión.</p>"
                        + "<p>Atentamente,<br/>Sistema de Gestión - ZOFRATACNA</p>";

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

            string connArchStr = ConfigurationManager.ConnectionStrings["FirmadorArchivos"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connArchStr))
            {
                using (SqlCommand cmd = new SqlCommand("ARC_I_GuardarArchivo_OUT", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Contenido", archivoPDF);
                    cmd.Parameters.AddWithValue("@NombreOriginal", FuPdfCorregido.FileName);
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

            int version = 1;
            string connFirStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connFirStr))
            {
                conn.Open();

                using (SqlCommand cmdVer = new SqlCommand("SELECT Version FROM FIR_Documento WHERE IDDocumento = @IDDocumento", conn))
                {
                    cmdVer.Parameters.AddWithValue("@IDDocumento", idDoc);
                    object result = cmdVer.ExecuteScalar();
                    if (result != null) version = Convert.ToInt32(result);
                }

                using (SqlCommand cmd = new SqlCommand("FIR_U_DocumentoCorreccion", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IDDocumento", idDoc);
                    cmd.Parameters.AddWithValue("@NuevaRuta", "ARC::" + nuevoIdArchivo);
                    cmd.Parameters.AddWithValue("@LoginUsuario", loginActual);
                    cmd.Parameters.AddWithValue("@IDEquipo", ipEquipo);
                    cmd.Parameters.AddWithValue("@Motivo", "Corrección v" + version);
                    cmd.ExecuteNonQuery();
                }
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
