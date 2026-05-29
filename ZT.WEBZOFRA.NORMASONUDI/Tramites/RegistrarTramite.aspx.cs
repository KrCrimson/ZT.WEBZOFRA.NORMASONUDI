using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.UI.WebControls;
using System.IO;
using ZT.WEBZOFRA.NORMASONUDI;
using iTextSharp.text.pdf;

public partial class RegistrarTramite : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["strUsuario"] == null)
        {
            Response.Redirect("~/Login.aspx");
        }

        string rol = Session["strRol"] != null ? Session["strRol"].ToString() : "";
        if (rol != "REGISTRADOR" && rol != "ADMIN")
        {
            Response.Redirect("~/Tramites/Bandeja.aspx");
        }

        if (!IsPostBack)
        {
            CargarTipos();
            
            DateTime fechaActual = DateTime.Now;
            LblFechaDocumento.Text = fechaActual.ToString("dd/MM/yyyy");
            ViewState["FechaDocumento"] = fechaActual;
            
            GenerarCodigoDocumento();
        }

        // CARGAR EMPLEADOS SIEMPRE (incluso en postback) para que el javascript no pierda la lista si hay error
        DataTable dtEmpleados = CargarEmpleados();
        var lstEmpleados = new System.Collections.Generic.List<object>();
        foreach(DataRow row in dtEmpleados.Rows)
        {
            lstEmpleados.Add(new { 
                LoginUsuario = row["LoginUsuario"].ToString(), 
                NombreCompleto = row["NombreCompleto"].ToString(),
                Email = row["Email"].ToString()
            });
        }
        string jsonEmpleados = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(lstEmpleados);
        ClientScript.RegisterStartupScript(this.GetType(), "empleadosArr", $"window.EmpleadosDisponibles = {jsonEmpleados};", true);
    }

    private void GenerarCodigoDocumento()
    {
        string codigoTipo = "";
        if (CbxTipoDocumento.SelectedIndex > 0)
        {
            codigoTipo = CbxTipoDocumento.SelectedValue;
        }
        else
        {
            codigoTipo = "DOC";
        }

        int siguienteId = 1;
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        try
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(MAX(IDDocumento),0)+1 FROM FIR_Documento", conn))
                {
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        siguienteId = Convert.ToInt32(result);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Omitir error de generación silenciosamente
        }

        string anio = DateTime.Now.Year.ToString();
        string numeroSecuencial = siguienteId.ToString().PadLeft(4, '0');
        
        string codigoGenerado = codigoTipo + "-" + anio + "-" + numeroSecuencial;
        TxtCodigoDocumento.Text = codigoGenerado;
        ViewState["CodigoDocumento"] = codigoGenerado;
    }

    protected void CbxTipoDocumento_SelectedIndexChanged(object sender, EventArgs e)
    {
        GenerarCodigoDocumento();
    }

    private void CargarTipos()
    {
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        try
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                using (SqlCommand cmd = new SqlCommand("FIR_S_MaestroPorTipo", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Tipo", "TIPO_DOC");

                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    CbxTipoDocumento.DataSource = dt;
                    CbxTipoDocumento.DataTextField = "Descripcion";
                    CbxTipoDocumento.DataValueField = "Codigo";
                    CbxTipoDocumento.DataBind();
                    CbxTipoDocumento.Items.Insert(0, new ListItem("-- Seleccione tipo --", ""));
                }
            }
        }
        catch (Exception ex)
        {
            LblError.Text = "Error al cargar tipos: " + ex.Message;
            LblError.Visible = true;
        }
    }

    private DataTable CargarEmpleados()
    {
        DataTable dt = new DataTable();
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        string usuarioActual = Session["strUsuario"] != null ? Session["strUsuario"].ToString() : "";

        using (SqlConnection conn = new SqlConnection(connStr))
        {
            // Filtra solo empleados con Rol ID = 3 (FIRMADOR) y excluye el usuario actual
            string query = "SELECT LoginUsuario, NombreCompleto, Email FROM FIR_VW_EmpleadosActivos WHERE LoginUsuario != @UsuarioActual AND IdRol = 3 ORDER BY NombreCompleto";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UsuarioActual", usuarioActual);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
        }
        return dt;
    }

    private string ObtenerEmailPorLogin(string loginUsuario)
    {
        if (string.IsNullOrWhiteSpace(loginUsuario)) return "";
        string email = "";
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = "SELECT Email FROM FIR_VW_EmpleadosActivos WHERE LoginUsuario = @LoginUsuario";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@LoginUsuario", loginUsuario);
                conn.Open();
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    email = result.ToString();
                }
            }
        }
        return email;
    }

    private void InicializarGridView()
    {
        // Ya no es necesario inicializar el GridView ya que fue reemplazado por la interfaz javascript
    }

    private DataTable ObtenerDatosFirmantes()
    {
        // Este método ya no es necesario
        return new DataTable();
    }

    // Los métodos GvFirmantes_RowDataBound, DdlFirmante_SelectedIndexChanged, BtnAgregarFirmante_Click y GvFirmantes_RowDeleting 
    // han quedado obsoletos y vacíos debido a que ya no usamos GridView.

    protected void BtnRegistrar_Click(object sender, EventArgs e)
    {
        LblError.Visible = false;
        LblExito.Visible = false;

        if (string.IsNullOrWhiteSpace(TxtAsunto.Text) ||
            string.IsNullOrWhiteSpace(CbxTipoDocumento.SelectedValue) ||
            string.IsNullOrWhiteSpace(DdlAreaResponsable.SelectedValue))
        {
            LblError.Text = "Debe completar todos los campos del documento.";
            LblError.Visible = true;
            return;
        }

        if (ViewState["FechaDocumento"] == null)
        {
            LblError.Text = "Faltan datos internos del documento.";
            LblError.Visible = true;
            return;
        }

        string codigoDocumento = TxtCodigoDocumento.Text != null ? TxtCodigoDocumento.Text.Trim() : "";
        if (string.IsNullOrWhiteSpace(codigoDocumento))
        {
            LblError.Text = "Debe ingresar el codigo de documento.";
            LblError.Visible = true;
            return;
        }
        ViewState["CodigoDocumento"] = codigoDocumento;

        if (!FuPdf.HasFile || Path.GetExtension(FuPdf.FileName).ToLower() != ".pdf")
        {
            LblError.Text = "Debe seleccionar un archivo PDF válido.";
            LblError.Visible = true;
            return;
        }

        const int maxPdfBytes = 50 * 1024 * 1024;
        if (FuPdf.PostedFile == null || FuPdf.PostedFile.ContentLength > maxPdfBytes)
        {
            LblError.Text = "El PDF supera el tamaño máximo permitido de 50 MB.";
            LblError.Visible = true;
            return;
        }

        if (PdfTieneFirma(FuPdf.FileBytes))
        {
            LblError.Text = "Debe seleccionar un archivo PDF sin firmas digitales previas.";
            LblError.Visible = true;
            return;
        }

        var jss = new System.Web.Script.Serialization.JavaScriptSerializer();
        string jsonFirmantes = HfFirmantesJSON.Value;
        if (string.IsNullOrWhiteSpace(jsonFirmantes)) jsonFirmantes = "[]";
        
        var timelineList = jss.Deserialize<List<Dictionary<string, string>>>(jsonFirmantes);

        if (timelineList == null || timelineList.Count == 0)
        {
            LblError.Text = "Debe agregar al menos un firmante.";
            LblError.Visible = true;
            return;
        }

        try
        {
            byte[] archivoPDF = FuPdf.FileBytes;
            int idArchivo = 0;
            string usuarioActivo = Session["strUsuario"].ToString();
            string nombreActivo = Session["strNombre"].ToString();
            string ipEquipo = Request.UserHostAddress;

            string connArchivosStr = ConfigurationManager.ConnectionStrings["FirmadorArchivos"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connArchivosStr))
            {
                using (SqlCommand cmd = new SqlCommand("ARC_I_GuardarArchivo_OUT", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Contenido", archivoPDF);
                    cmd.Parameters.AddWithValue("@NombreOriginal", FuPdf.FileName);
                    cmd.Parameters.AddWithValue("@TipoArchivo", "PDF_ORIGINAL");
                    cmd.Parameters.AddWithValue("@IDUsuarioCreador", usuarioActivo);

                    SqlParameter pout = new SqlParameter("@IDArchivo", SqlDbType.Int);
                    pout.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(pout);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    idArchivo = Convert.ToInt32(pout.Value);
                }
            }

            int idDocumento = 0;
            
            using (SqlConnection conn = new SqlConnection(
                ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("FIR_I_Documento_OUT", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Asunto", TxtAsunto.Text.Trim());
                    cmd.Parameters.AddWithValue("@CodigoTipoDocumento", 
                        CbxTipoDocumento.SelectedValue);
                    cmd.Parameters.AddWithValue("@AreaResponsable", 
                        DdlAreaResponsable.SelectedValue.Trim());
                    cmd.Parameters.AddWithValue("@FechaDocumento", 
                        (DateTime)ViewState["FechaDocumento"]);
                    cmd.Parameters.AddWithValue("@CodigoDocumento", codigoDocumento);
                    cmd.Parameters.AddWithValue("@RutaArchivoPDF", 
                        "ARC::" + idArchivo);
                    cmd.Parameters.AddWithValue("@Orientacion", "V");
                    cmd.Parameters.AddWithValue("@LoginRegistrador", 
                        Session["strUsuario"].ToString());
                    cmd.Parameters.AddWithValue("@IDEquipo", 
                        Request.UserHostAddress);

                    if (!string.IsNullOrWhiteSpace(TxtFechaLimite.Text))
                        cmd.Parameters.AddWithValue("@FechaLimiteRevision", Convert.ToDateTime(TxtFechaLimite.Text));
                    else
                        cmd.Parameters.AddWithValue("@FechaLimiteRevision", DBNull.Value);

                    SqlParameter paramIDDocumento = new SqlParameter(
                        "@IDDocumento", SqlDbType.Int);
                    paramIDDocumento.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(paramIDDocumento);

                    cmd.ExecuteNonQuery();

                    idDocumento = Convert.ToInt32(paramIDDocumento.Value);
                }

                var correosFirmantes = new System.Collections.Generic.List<string>();

                // (El registrador ya no se inserta automáticamente como revisor/firmante)

                var listaFirmantes = new System.Collections.Generic.List<Tuple<int, string, string, string>>();

                // Extraemos el orden ya convertido a diccionario desde el Javascript oculto (timelineList)
                int orderIndex = 1;
                foreach (var firmanteDict in timelineList)
                {
                    string loginFirmante = firmanteDict["LoginUsuario"];
                    string nombreFirmante = firmanteDict["NombreCompleto"];
                    string correoFirmante = firmanteDict["Email"];

                    if (!string.IsNullOrWhiteSpace(correoFirmante))
                    {
                        correosFirmantes.Add(correoFirmante);
                    }
                    
                    listaFirmantes.Add(new Tuple<int, string, string, string>(orderIndex, loginFirmante, nombreFirmante, correoFirmante));
                    orderIndex++;
                }

                // (No necesitamos order.Sort ya que los recolectamos exactamente en el orden del DOM)

                foreach (var f in listaFirmantes)
                {
                    int ordenFirma = f.Item1;
                    string loginFirmante = f.Item2;
                    string nombreFirmante = f.Item3;
                    string correoFirmante = f.Item4;

                    using (SqlCommand cmdRev = new SqlCommand("FIR_I_DocumentoRevisor", conn))
                    {
                        cmdRev.CommandType = CommandType.StoredProcedure;
                        cmdRev.Parameters.AddWithValue("@IDDocumento", idDocumento);
                        cmdRev.Parameters.AddWithValue("@LoginUsuario", loginFirmante);
                        cmdRev.Parameters.AddWithValue("@NombreRevisor", nombreFirmante);
                        cmdRev.Parameters.AddWithValue("@CorreoRevisor", correoFirmante);
                        cmdRev.Parameters.AddWithValue("@DiasMaxRevision", 3);
                        cmdRev.Parameters.AddWithValue("@Version", 1);
                        cmdRev.Parameters.AddWithValue("@IDUsuarioCreador", usuarioActivo);
                        cmdRev.ExecuteNonQuery();
                    }

                    using (SqlCommand cmdFir = new SqlCommand("FIR_I_DocumentoFirmante", conn))
                    {
                        cmdFir.CommandType = CommandType.StoredProcedure;
                        cmdFir.Parameters.AddWithValue("@IDDocumento", idDocumento);
                        cmdFir.Parameters.AddWithValue("@LoginUsuario", loginFirmante);
                        cmdFir.Parameters.AddWithValue("@NombreFirmante", nombreFirmante);
                        cmdFir.Parameters.AddWithValue("@CorreoFirmante", correoFirmante);
                        cmdFir.Parameters.AddWithValue("@OrdenFirma", ordenFirma);
                        cmdFir.Parameters.AddWithValue("@CodigoRolFirmante", "VB");
                        cmdFir.Parameters.AddWithValue("@IDUsuarioCreador", usuarioActivo);
                        cmdFir.ExecuteNonQuery();
                    }
                }

                try
                {
                    CorreoBLL.NotificarDocumentoCreado(correosFirmantes,
                        codigoDocumento,
                        TxtAsunto.Text.Trim(),
                        usuarioActivo);
                }
                catch { }

                using (SqlCommand cmdStatus = new SqlCommand("UPDATE FIR_Documento SET CodigoEstado = 'EN_REV' WHERE IDDocumento = @IDD", conn))
                {
                    cmdStatus.Parameters.AddWithValue("@IDD", idDocumento);
                    cmdStatus.ExecuteNonQuery();
                }

                using (SqlCommand cmdInit = new SqlCommand("FIR_U_IniciarRevision", conn))
                {
                    cmdInit.CommandType = CommandType.StoredProcedure;
                    cmdInit.Parameters.AddWithValue("@IDDocumento", idDocumento);
                    cmdInit.Parameters.AddWithValue("@LoginUsuario", usuarioActivo);
                    cmdInit.Parameters.AddWithValue("@IDEquipo", ipEquipo);
                    cmdInit.ExecuteNonQuery();
                }

                try
                {
                    CorreoBLL.NotificarInicioRevision(correosFirmantes,
                        codigoDocumento,
                        TxtAsunto.Text.Trim());
                }
                catch { }
            }

            Response.Redirect("Bandeja.aspx?msg=registrado");
        }
        catch (Exception ex)
        {
            LblError.Text = "Error en el registro: " + ex.Message;
            LblError.Visible = true;
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
                string query = @"SELECT DISTINCT e.Email
                                 FROM FIR_VW_EmpleadosActivos e
                                 INNER JOIN FIR_DocumentoFirmante df 
                                     ON df.LoginUsuario = e.LoginUsuario
                                 WHERE df.IDDocumento = @IDDocumento";

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

                string mensajeCompleto = PlantillaCorreo(mensaje);

                foreach (string email in correos)
                {
                    try
                    {
                        using (SqlCommand cmdMail = new SqlCommand("GEN_X_EnviarMail", conn))
                        {
                            cmdMail.CommandType = CommandType.StoredProcedure;
                            cmdMail.Parameters.AddWithValue("@Para", email);
                            cmdMail.Parameters.AddWithValue("@Asunto", asunto);
                            cmdMail.Parameters.AddWithValue("@Mensaje", mensajeCompleto);
                            cmdMail.ExecuteNonQuery();
                        }
                    }
                    catch { }
                }
            }
        }
        catch { }
    }

    private bool PdfTieneFirma(byte[] pdfBytes)
    {
        if (pdfBytes == null || pdfBytes.Length == 0) return false;
        try
        {
            using (PdfReader reader = new PdfReader(pdfBytes))
            {
                var acro = reader.AcroFields;
                if (acro == null) return false;
                var firmas = acro.GetSignatureNames();
                return firmas != null && firmas.Count > 0;
            }
        }
        catch
        {
            return true; // Si no se puede leer, tratamos como no valido
        }
    }
}
