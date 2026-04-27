using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.UI.WebControls;
using System.IO;

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
            
            InicializarGridView();
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
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = "SELECT LoginUsuario, NombreCompleto, Email FROM FIR_VW_EmpleadosActivos ORDER BY NombreCompleto";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
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
        DataTable dt = new DataTable();
        dt.Columns.Add("LoginUsuario");
        dt.Columns.Add("CorreoFirmante");
        dt.Columns.Add("OrdenFirma");

        dt.Rows.Add(dt.NewRow()); // Fila vacía inicial
        ViewState["Firmantes"] = dt;
        GvFirmantes.DataSource = dt;
        GvFirmantes.DataBind();
    }

    private DataTable ObtenerDatosFirmantes()
    {
        DataTable dt = new DataTable();
        dt.Columns.Add("LoginUsuario");
        dt.Columns.Add("CorreoFirmante");
        dt.Columns.Add("OrdenFirma");

        foreach (GridViewRow row in GvFirmantes.Rows)
        {
            DropDownList DdlFirmante = (DropDownList)row.FindControl("DdlFirmante");
            Label LblCorreo = (Label)row.FindControl("LblCorreo");
            DropDownList DdlOrden = (DropDownList)row.FindControl("DdlOrden");

            DataRow dr = dt.NewRow();
            dr["LoginUsuario"] = DdlFirmante.SelectedValue;
            dr["CorreoFirmante"] = LblCorreo.Text;
            dr["OrdenFirma"] = DdlOrden.SelectedValue;
            dt.Rows.Add(dr);
        }
        return dt;
    }

    protected void GvFirmantes_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            DropDownList DdlFirmante = (DropDownList)e.Row.FindControl("DdlFirmante");
            HiddenField HfLoginUsuario = (HiddenField)e.Row.FindControl("HfLoginUsuario");
            DropDownList DdlOrden = (DropDownList)e.Row.FindControl("DdlOrden");
            HiddenField HfOrdenFirma = (HiddenField)e.Row.FindControl("HfOrdenFirma");

            if (DdlFirmante != null)
            {
                DataTable dtEmpleados = CargarEmpleados();
                DdlFirmante.DataSource = dtEmpleados;
                DdlFirmante.DataTextField = "NombreCompleto";
                DdlFirmante.DataValueField = "LoginUsuario";
                DdlFirmante.DataBind();
                DdlFirmante.Items.Insert(0, new ListItem("-- Seleccione empleado --", ""));

                if (HfLoginUsuario != null && !string.IsNullOrWhiteSpace(HfLoginUsuario.Value))
                {
                    DdlFirmante.SelectedValue = HfLoginUsuario.Value;
                }
            }

            if (DdlOrden != null)
            {
                DdlOrden.Items.Insert(0, new ListItem("-- Seleccione --", ""));
                for (int i = 1; i <= 15; i++)
                {
                    DdlOrden.Items.Add(new ListItem(i.ToString(), i.ToString()));
                }

                if (HfOrdenFirma != null && !string.IsNullOrWhiteSpace(HfOrdenFirma.Value))
                {
                    DdlOrden.SelectedValue = HfOrdenFirma.Value;
                }
            }
        }
    }

    protected void DdlFirmante_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddl = (DropDownList)sender;
        GridViewRow row = (GridViewRow)ddl.NamingContainer;
        Label lblCorreo = (Label)row.FindControl("LblCorreo");
        
        string login = ddl.SelectedValue;
        if (!string.IsNullOrWhiteSpace(login))
        {
            lblCorreo.Text = ObtenerEmailPorLogin(login);
        }
        else
        {
            lblCorreo.Text = "";
        }
        
        DataTable dt = ObtenerDatosFirmantes();
        ViewState["Firmantes"] = dt;
    }

    protected void BtnAgregarFirmante_Click(object sender, EventArgs e)
    {
        LblError.Visible = false;
        DataTable dt = ObtenerDatosFirmantes();

        if (dt.Rows.Count >= 15)
        {
            LblError.Text = "Máximo 15 firmantes.";
            LblError.Visible = true;
            return;
        }

        dt.Rows.Add(dt.NewRow());
        ViewState["Firmantes"] = dt;
        GvFirmantes.DataSource = dt;
        GvFirmantes.DataBind();
    }

    protected void BtnRegistrar_Click(object sender, EventArgs e)
    {
        LblError.Visible = false;
        LblExito.Visible = false;

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

        if (GvFirmantes.Rows.Count == 0)
        {
            LblError.Text = "Debe agregar al menos un firmante.";
            LblError.Visible = true;
            return;
        }

        List<int> ordenes = new List<int>();
        List<string> logins = new List<string>();

        foreach (GridViewRow row in GvFirmantes.Rows)
        {
            DropDownList DdlFirmante = (DropDownList)row.FindControl("DdlFirmante");
            DropDownList DdlOrden = (DropDownList)row.FindControl("DdlOrden");

            string login = DdlFirmante.SelectedValue;
            string ordenStr = DdlOrden.SelectedValue;

            if (string.IsNullOrWhiteSpace(login))
            {
                LblError.Text = "Debe seleccionar un empleado para todos los firmantes.";
                LblError.Visible = true;
                return;
            }

            if (string.IsNullOrWhiteSpace(ordenStr))
            {
                LblError.Text = "Debe asignar un orden de firma a cada empleado.";
                LblError.Visible = true;
                return;
            }

            if (logins.Contains(login))
            {
                LblError.Text = "Un empleado no puede ser firmante dos veces en el mismo trámite.";
                LblError.Visible = true;
                return;
            }
            logins.Add(login);

            int orden = Convert.ToInt32(ordenStr);
            if (ordenes.Contains(orden))
            {
                LblError.Text = "No puede haber números de orden de firma duplicados.";
                LblError.Visible = true;
                return;
            }
            ordenes.Add(orden);
        }

        ordenes.Sort();
        for (int i = 0; i < ordenes.Count; i++)
        {
            if (ordenes[i] != i + 1)
            {
                LblError.Text = "El orden de firmas debe ser secuencial y sin saltos empezando desde 1 (ej: 1, 2, 3...).";
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

                    SqlParameter paramIDDocumento = new SqlParameter(
                        "@IDDocumento", SqlDbType.Int);
                    paramIDDocumento.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(paramIDDocumento);

                    cmd.ExecuteNonQuery();

                    idDocumento = Convert.ToInt32(paramIDDocumento.Value);
                }

                string correoActivo = ObtenerEmailPorLogin(usuarioActivo);

                using (SqlCommand cmdRev = new SqlCommand("FIR_I_DocumentoRevisor", conn))
                {
                    cmdRev.CommandType = CommandType.StoredProcedure;
                    cmdRev.Parameters.AddWithValue("@IDDocumento", idDocumento);
                    cmdRev.Parameters.AddWithValue("@LoginUsuario", usuarioActivo);
                    cmdRev.Parameters.AddWithValue("@NombreRevisor", nombreActivo);
                    cmdRev.Parameters.AddWithValue("@CorreoRevisor", correoActivo);
                    cmdRev.Parameters.AddWithValue("@DiasMaxRevision", 3);
                    cmdRev.Parameters.AddWithValue("@Version", 1);
                    cmdRev.Parameters.AddWithValue("@IDUsuarioCreador", usuarioActivo);
                    cmdRev.ExecuteNonQuery();
                }

                using (SqlCommand cmdFir = new SqlCommand("FIR_I_DocumentoFirmante", conn))
                {
                    cmdFir.CommandType = CommandType.StoredProcedure;
                    cmdFir.Parameters.AddWithValue("@IDDocumento", idDocumento);
                    cmdFir.Parameters.AddWithValue("@LoginUsuario", usuarioActivo);
                    cmdFir.Parameters.AddWithValue("@NombreFirmante", nombreActivo);
                    cmdFir.Parameters.AddWithValue("@CorreoFirmante", correoActivo);
                    cmdFir.Parameters.AddWithValue("@OrdenFirma", 0);
                    cmdFir.Parameters.AddWithValue("@CodigoRolFirmante", "AUT");
                    cmdFir.Parameters.AddWithValue("@IDUsuarioCreador", usuarioActivo);
                    cmdFir.ExecuteNonQuery();
                }

                foreach (GridViewRow row in GvFirmantes.Rows)
                {
                    DropDownList DdlFirmante = (DropDownList)row.FindControl("DdlFirmante");
                    DropDownList DdlOrden = (DropDownList)row.FindControl("DdlOrden");

                    string loginFirmante = DdlFirmante.SelectedValue;
                    string nombreFirmante = DdlFirmante.SelectedItem.Text;
                    
                    string correoFirmante = ObtenerEmailPorLogin(loginFirmante);
                    
                    int ordenFirma = Convert.ToInt32(DdlOrden.SelectedValue);

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
}
