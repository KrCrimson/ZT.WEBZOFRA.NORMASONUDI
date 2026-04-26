using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

public partial class Login : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            CargarUsuariosActivos();
        }
    }

    private void CargarUsuariosActivos()
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("FIR_S_UsuariosPrueba", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    CbxUsuario.DataSource = dt;
                    CbxUsuario.DataTextField = "NombreCompleto";
                    CbxUsuario.DataValueField = "LoginUsuario";
                    CbxUsuario.DataBind();

                    CbxUsuario.Items.Insert(0, new ListItem("-- Seleccione su usuario --", ""));
                }
            }
        }
        catch (Exception ex)
        {
            LblError.Text = "Error al cargar usuarios: " + ex.Message;
            LblError.Visible = true;
        }
    }

    protected void BtnIngresar_Click(object sender, EventArgs e)
    {
        string strLogin = CbxUsuario.SelectedValue;
        if (string.IsNullOrEmpty(strLogin))
        {
            LblError.Text = "Seleccione un usuario.";
            LblError.Visible = true;
            return;
        }
        try
        {
            using (SqlConnection conn = new SqlConnection(
                System.Configuration.ConfigurationManager
                .ConnectionStrings["Firmador"].ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("FIR_S_ObtenerSesion", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@LoginUsuario", strLogin);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Session["strUsuario"] = reader["LoginUsuario"].ToString();
                            Session["strRol"] = reader["CodigoRol"].ToString();
                            Session["strNombre"] = reader["NombreCompleto"].ToString();
                            Session["strEmail"] = reader["Email"].ToString();
                            string urlDashboard = reader["UrlDashboard"].ToString();
                            Response.Redirect(urlDashboard);
                        }
                        else
                        {
                            LblError.Text = "Usuario sin acceso al sistema.";
                            LblError.Visible = true;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LblError.Text = ex.Message;
            LblError.Visible = true;
        }
    }
}