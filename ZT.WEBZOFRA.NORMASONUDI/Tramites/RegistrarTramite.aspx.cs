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
            InicializarGridView();
        }
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

                if (HfLoginUsuario != null && !string.IsNullOrEmpty(HfLoginUsuario.Value))
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

                if (HfOrdenFirma != null && !string.IsNullOrEmpty(HfOrdenFirma.Value))
                {
                    DdlOrden.SelectedValue = HfOrdenFirma.Value;
                }
            }
        }
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

        // Validaciones Básicas
        if (string.IsNullOrWhiteSpace(TxtAsunto.Text) ||
            string.IsNullOrWhiteSpace(CbxTipoDocumento.SelectedValue) ||
            string.IsNullOrWhiteSpace(TxtAreaResponsable.Text) ||
            string.IsNullOrWhiteSpace(TxtCodigoDocumento.Text) ||
            string.IsNullOrWhiteSpace(TxtFechaDocumento.Text))
        {
            LblError.Text = "Debe completar todos los campos del documento.";
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

        // Proceso de Registro
        try
        {
            byte[] archivoPDF = FuPdf.FileBytes;
            int idArchivo = 0;
            string usuarioActivo = Session["strUsuario"].ToString();
            string nombreActivo = Session["strNombre"].ToString();
            string ipEquipo = Request.UserHostAddress;

            // 1. Guardar PDF
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
            string connFirmadorStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;

            // 2. Registrar Documento
            using (SqlConnection conn = new SqlConnection(connFirmadorStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("FIR_I_Documento_OUT", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Asunto", TxtAsunto.Text.Trim());
                    cmd.Parameters.AddWithValue("@CodigoTipoDocumento", CbxTipoDocumento.SelectedValue);
                    cmd.Parameters.AddWithValue("@AreaResponsable", TxtAreaResponsable.Text.Trim());
                    cmd.Parameters.AddWithValue("@FechaDocumento", Convert.ToDateTime(TxtFechaDocumento.Text));
                    cmd.Parameters.AddWithValue("@CodigoDocumento", TxtCodigoDocumento.Text.Trim());
                    cmd.Parameters.AddWithValue("@RutaArchivoPDF", "ARC::" + idArchivo);
                    cmd.Parameters.AddWithValue("@Orientacion", "V");
                    cmd.Parameters.AddWithValue("@LoginRegistrador", usuarioActivo);
                    cmd.Parameters.AddWithValue("@IDEquipo", ipEquipo);

                    SqlParameter pout = new SqlParameter("@IDDocumento", SqlDbType.Int);
                    pout.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(pout);

                    cmd.ExecuteNonQuery();
                    idDocumento = Convert.ToInt32(pout.Value);
                }

                string correoActivo = ObtenerEmailPorLogin(usuarioActivo);

                // 3. Insertar registrador como AUTOR
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

                // 4. Insertar Firmantes del Grid (Todos como VB)
                foreach (GridViewRow row in GvFirmantes.Rows)
                {
                    DropDownList DdlFirmante = (DropDownList)row.FindControl("DdlFirmante");
                    DropDownList DdlOrden = (DropDownList)row.FindControl("DdlOrden");

                    string loginFirmante = DdlFirmante.SelectedValue;
                    string nombreFirmante = DdlFirmante.SelectedItem.Text;
                    string correoFirmante = ObtenerEmailPorLogin(loginFirmante);
                    int ordenFirma = Convert.ToInt32(DdlOrden.SelectedValue);

                    // Revisor
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

                    // Firmante
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

                // 5. Iniciar Revisión
                using (SqlCommand cmdInit = new SqlCommand("FIR_U_IniciarRevision", conn))
                {
                    cmdInit.CommandType = CommandType.StoredProcedure;
                    cmdInit.Parameters.AddWithValue("@IDDocumento", idDocumento);
                    cmdInit.Parameters.AddWithValue("@LoginUsuario", usuarioActivo);
                    cmdInit.Parameters.AddWithValue("@IDEquipo", ipEquipo);
                    cmdInit.ExecuteNonQuery();
                }
            }

            // Redirigir en caso de éxito
            Response.Redirect("~/Tramites/Bandeja.aspx");
        }
        catch (Exception ex)
        {
            LblError.Text = "Error en el registro: " + ex.Message;
            LblError.Visible = true;
        }
    }
}
