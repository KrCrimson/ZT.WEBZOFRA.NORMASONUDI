using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.Script.Serialization;

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
            CargarFirmantesSelector();
            HfFirmantesJson.Value = "[]";
        }
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
        LblCodigoDocumento.Text = codigoGenerado;
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
            string query = "SELECT LoginUsuario, NombreCompleto, Email FROM FIR_VW_EmpleadosActivos WHERE LoginUsuario != @UsuarioActual ORDER BY NombreCompleto";
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

    private void CargarFirmantesSelector()
    {
        DataTable dtEmpleados = CargarEmpleados();
        DdlFirmanteNuevo.Items.Clear();
        DdlFirmanteNuevo.Items.Add(new ListItem("-- Seleccione empleado --", ""));

        foreach (DataRow row in dtEmpleados.Rows)
        {
            string login = row["LoginUsuario"].ToString();
            string nombre = row["NombreCompleto"].ToString();
            string correo = row["Email"].ToString();

            ListItem item = new ListItem(nombre, login);
            item.Attributes["data-email"] = correo;
            DdlFirmanteNuevo.Items.Add(item);
        }
    }

    protected void BtnRegistrar_Click(object sender, EventArgs e)
    {
        LblError.Visible = false;
        LblExito.Visible = false;

        GenerarCodigoDocumento();

        if (string.IsNullOrWhiteSpace(TxtAsunto.Text) ||
            string.IsNullOrWhiteSpace(CbxTipoDocumento.SelectedValue) ||
            string.IsNullOrWhiteSpace(TxtAreaResponsable.Text))
        {
            LblError.Text = "Debe completar todos los campos del documento.";
            LblError.Visible = true;
            return;
        }

        if (ViewState["CodigoDocumento"] == null || ViewState["FechaDocumento"] == null)
        {
            LblError.Text = "Faltan datos internos del documento.";
            LblError.Visible = true;
            return;
        }

        if (!FuPdf.HasFile || Path.GetExtension(FuPdf.FileName).ToLower() != ".pdf")
        {
            LblError.Text = "Debe seleccionar un archivo PDF válido.";
            LblError.Visible = true;
            return;
        }

        List<FirmanteItem> firmantes = ObtenerFirmantesSeleccionados();

        if (firmantes.Count == 0)
        {
            LblError.Text = "Debe agregar al menos un firmante.";
            LblError.Visible = true;
            return;
        }

        HashSet<string> firmantesUnicos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (FirmanteItem firmante in firmantes)
        {
            if (string.IsNullOrWhiteSpace(firmante.Login))
            {
                LblError.Text = "Debe seleccionar un empleado para todos los firmantes.";
                LblError.Visible = true;
                return;
            }

            if (!firmantesUnicos.Add(firmante.Login))
            {
                LblError.Text = "Un empleado no puede ser firmante dos veces en el mismo trámite.";
                LblError.Visible = true;
                return;
            }
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
                        TxtAreaResponsable.Text.Trim());
                    cmd.Parameters.AddWithValue("@FechaDocumento", 
                        (DateTime)ViewState["FechaDocumento"]);
                    cmd.Parameters.AddWithValue("@CodigoDocumento", 
                        ViewState["CodigoDocumento"].ToString());
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

                // (El registrador ya no se inserta automáticamente como revisor/firmante)

                for (int i = 0; i < firmantes.Count; i++)
                {
                    FirmanteItem firmante = firmantes[i];
                    string loginFirmante = firmante.Login;
                    string nombreFirmante = firmante.Nombre;
                    string correoFirmante = string.IsNullOrWhiteSpace(firmante.Correo)
                        ? ObtenerEmailPorLogin(loginFirmante)
                        : firmante.Correo;
                    int ordenFirma = i + 1;

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

                using (SqlCommand cmdInit = new SqlCommand("FIR_U_IniciarRevision", conn))
                {
                    cmdInit.CommandType = CommandType.StoredProcedure;
                    cmdInit.Parameters.AddWithValue("@IDDocumento", idDocumento);
                    cmdInit.Parameters.AddWithValue("@LoginUsuario", usuarioActivo);
                    cmdInit.Parameters.AddWithValue("@IDEquipo", ipEquipo);
                    cmdInit.ExecuteNonQuery();
                }
            }

            Response.Redirect("~/Tramites/Bandeja.aspx");
        }
        catch (Exception ex)
        {
            LblError.Text = "Error en el registro: " + ex.Message;
            LblError.Visible = true;
        }
    }

    private List<FirmanteItem> ObtenerFirmantesSeleccionados()
    {
        string json = HfFirmantesJson.Value ?? "[]";
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        try
        {
            return serializer.Deserialize<List<FirmanteItem>>(json) ?? new List<FirmanteItem>();
        }
        catch
        {
            return new List<FirmanteItem>();
        }
    }

    private class FirmanteItem
    {
        public string Login { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
    }
}
