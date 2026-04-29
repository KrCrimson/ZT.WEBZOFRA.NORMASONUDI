using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Web.UI;

public partial class FirmaDigital : System.Web.UI.Page
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
            
            if (!VerificarHabilitacion(idDoc))
            {
                LblError.Text = "No es su turno de firmar o el documento no requiere su firma en este momento.";
                LblError.Visible = true;
                PnlFirma.Enabled = false;
                return;
            }

            CargarInfoDocumento(idDoc);
        }
    }

    private bool VerificarHabilitacion(int idDoc)
    {
        string loginActual = Session["strUsuario"].ToString();
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        bool habilitado = false;

        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"SELECT IDDocumentoFirmante 
                             FROM FIR_DocumentoFirmante 
                             WHERE IDDocumento = @IDDocumento 
                               AND LoginUsuario = @LoginUsuario 
                               AND Habilitado = 1 
                               AND CodigoEstadoFirma = 'PEN'";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@IDDocumento", idDoc);
                cmd.Parameters.AddWithValue("@LoginUsuario", loginActual);
                conn.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    ViewState["IDDocumentoFirmante"] = Convert.ToInt32(result);
                    habilitado = true;
                }
            }
        }
        return habilitado;
    }

    private void CargarInfoDocumento(int idDoc)
    {
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = "SELECT CodigoDocumento, Asunto, IDArchivoPDF FROM FIR_Documento WHERE IDDocumento = @IDDocumento";
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
                        ViewState["IDArchivoPDF"] = dr["IDArchivoPDF"];
                    }
                }
            }
        }
    }

    protected void BtnDescargar_Click(object sender, EventArgs e)
    {
        if (ViewState["IDArchivoPDF"] != null)
        {
            Response.Redirect("~/Handlers/VerPDF.ashx?id=" + ViewState["IDArchivoPDF"].ToString());
        }
    }

    protected void BtnSubirFirma_Click(object sender, EventArgs e)
    {
        if (!FuPdfFirmado.HasFile || Path.GetExtension(FuPdfFirmado.FileName).ToLower() != ".pdf")
        {
            LblError.Text = "Debe seleccionar un archivo PDF válido.";
            LblError.Visible = true;
            return;
        }

        int idDoc = (int)ViewState["IDDocumento"];
        
        // Re-verificar habilitación para seguridad
        if (!VerificarHabilitacion(idDoc))
        {
            LblError.Text = "Sesión expirada o ya no tiene permisos para firmar este documento.";
            LblError.Visible = true;
            return;
        }

        int idDocFirmante = (int)ViewState["IDDocumentoFirmante"];
        string loginActual = Session["strUsuario"].ToString();
        string ipEquipo = Request.UserHostAddress;

        try
        {
            byte[] contenidoFirmado = FuPdfFirmado.FileBytes;
            int idArchivoFirmado = 0;

            // 1. Guardar en BD Archivos
            string connArchivosStr = ConfigurationManager.ConnectionStrings["FirmadorArchivos"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connArchivosStr))
            {
                using (SqlCommand cmd = new SqlCommand("ARC_I_GuardarArchivo_OUT", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Contenido", contenidoFirmado);
                    cmd.Parameters.AddWithValue("@NombreOriginal", FuPdfFirmado.FileName);
                    cmd.Parameters.AddWithValue("@TipoArchivo", "PDF_FIRMADO");
                    cmd.Parameters.AddWithValue("@IDUsuarioCreador", loginActual);

                    SqlParameter pout = new SqlParameter("@IDArchivo", SqlDbType.Int);
                    pout.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(pout);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    idArchivoFirmado = Convert.ToInt32(pout.Value);
                }
            }

            // 2. Generar Hash SHA256
            string hashStr = "";
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(contenidoFirmado);
                hashStr = BitConverter.ToString(hash).Replace("-", "");
            }

            // 3. Registrar Firma en BD Firmador
            bool procesoCompleto = false;
            string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("FIR_X_RegistrarFirma_OUT", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IDDocumento", idDoc);
                    cmd.Parameters.AddWithValue("@IDDocumentoFirmante", idDocFirmante);
                    cmd.Parameters.AddWithValue("@HashFirma", hashStr);
                    cmd.Parameters.AddWithValue("@CertificadoSN", "MANUAL");
                    cmd.Parameters.AddWithValue("@CertificadoDNI", DBNull.Value);
                    cmd.Parameters.AddWithValue("@MotivoFirma", TxtMotivo.Text.Trim());
                    cmd.Parameters.AddWithValue("@FirmaValida", 1);
                    cmd.Parameters.AddWithValue("@RespuestaMotor", "Firma manual via ReFirma/FirmaPerú");
                    cmd.Parameters.AddWithValue("@RutaPDFFirmado", "ARC::" + idArchivoFirmado);
                    cmd.Parameters.AddWithValue("@LoginUsuario", loginActual);
                    cmd.Parameters.AddWithValue("@IDEquipo", ipEquipo);

                    SqlParameter pOut = new SqlParameter("@ProcesoCompleto", SqlDbType.Bit);
                    pOut.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(pOut);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    procesoCompleto = Convert.ToBoolean(pOut.Value);
                }

                // 4. Notificaciones
                EnviarNotificaciones(conn, idDoc, procesoCompleto);
            }

            // Mostrar Exito
            PnlFirma.Visible = false;
            PnlExito.Visible = true;
            LblMensajeExito.Text = procesoCompleto 
                ? "El documento ha sido firmado por todos los participantes y se encuentra completado."
                : "Su firma ha sido registrada correctamente. El documento ha sido notificado al siguiente firmante.";

        }
        catch (Exception ex)
        {
            LblError.Text = "Error al procesar la firma: " + ex.Message;
            LblError.Visible = true;
        }
    }

    private void EnviarNotificaciones(SqlConnection conn, int idDoc, bool completo)
    {
        string codigoDoc = LblCodigoDoc.Text;
        string asunto = LblAsunto.Text;

        if (completo)
        {
            // Notificar a todos + registrador
            DataTable dtPersonas = new DataTable();
            string query = @"SELECT CorreoFirmante FROM FIR_DocumentoFirmante WHERE IDDocumento = @IDD
                             UNION
                             SELECT Email FROM GEN_VW_Usuario WHERE LoginUsuario = (SELECT LoginRegistrador FROM FIR_Documento WHERE IDDocumento = @IDD)";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@IDD", idDoc);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dtPersonas);
            }

            foreach (DataRow row in dtPersonas.Rows)
            {
                string correo = row[0].ToString();
                if (!string.IsNullOrWhiteSpace(correo))
                {
                    EnviarMail(conn, correo, "Documento Firmado Completamente: " + codigoDoc, 
                        "<p>El documento <b>" + codigoDoc + "</b> (" + asunto + ") ha finalizado el proceso de firmas satisfactoriamente.</p>");
                }
            }
        }
        else
        {
            // Notificar al siguiente habilitado
            string querySiguiente = @"SELECT TOP 1 CorreoFirmante, NombreFirmante 
                                      FROM FIR_DocumentoFirmante 
                                      WHERE IDDocumento = @IDD AND Habilitado = 1 AND CodigoEstadoFirma = 'PEN'";
            using (SqlCommand cmd = new SqlCommand(querySiguiente, conn))
            {
                cmd.Parameters.AddWithValue("@IDD", idDoc);
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        string correo = dr["CorreoFirmante"].ToString();
                        string nombre = dr["NombreFirmante"].ToString();
                        if (!string.IsNullOrWhiteSpace(correo))
                        {
                            EnviarMail(conn, correo, "Trámite pendiente de firma: " + codigoDoc, 
                                "<p>Estimado(a) <b>" + nombre + "</b>, el documento <b>" + codigoDoc + "</b> ya fue firmado por el participante anterior y se encuentra disponible para su firma.</p>");
                        }
                    }
                }
            }
        }
    }

    private void EnviarMail(SqlConnection conn, string para, string asunto, string body)
    {
        try
        {
            using (SqlCommand cmd = new SqlCommand("GEN_X_EnviarMail", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Para", para);
                cmd.Parameters.AddWithValue("@Asunto", asunto);
                cmd.Parameters.AddWithValue("@Mensaje", body);
                cmd.ExecuteNonQuery();
            }
        }
        catch { /* Ignorar errores de correo en este flujo */ }
    }

    protected void LnkVolver_Click(object sender, EventArgs e)
    {
        Response.Redirect("~/Tramites/Detalle.aspx?id=" + ViewState["IDDocumento"]);
    }

    protected void BtnIrBandeja_Click(object sender, EventArgs e)
    {
        Response.Redirect("~/Tramites/Bandeja.aspx");
    }
}
