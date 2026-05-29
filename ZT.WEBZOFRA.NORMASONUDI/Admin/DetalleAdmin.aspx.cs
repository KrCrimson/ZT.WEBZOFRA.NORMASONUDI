using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

public partial class DetalleAdmin : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["strUsuario"] == null || Session["strRol"] == null || Session["strRol"].ToString() != "ADMIN")
        {
            Response.Redirect("~/Login.aspx");
            return;
        }

        if (string.IsNullOrEmpty(Request.QueryString["id"]))
        {
            Response.Redirect("~/Admin/Dashboard.aspx");
            return;
        }

        if (!IsPostBack)
        {
            int idDoc = Convert.ToInt32(Request.QueryString["id"]);
            CargarDocumento(idDoc);
            CargarRevisores(idDoc);
            CargarObservaciones(idDoc);
        }
    }

    private void CargarDocumento(int idDoc)
    {
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"SELECT d.CodigoDocumento, d.Asunto, d.CodigoTipoDocumento, 
                                    m.Descripcion AS TipoDocumento, d.AreaResponsable, 
                                    d.FechaDocumento, d.CodigoEstado, d.LoginRegistrador, d.RutaArchivoPDF
                             FROM FIR_Documento d
                             LEFT JOIN FIR_Maestro m ON m.Tipo='TIPO_DOC' AND m.Codigo=d.CodigoTipoDocumento
                             WHERE d.IDDocumento = @ID";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ID", idDoc);
                conn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        LblCodigoDoc.Text = dr["CodigoDocumento"].ToString();
                        LblAsunto.Text = dr["Asunto"].ToString();
                        LblTipoDoc.Text = dr["TipoDocumento"]?.ToString() ?? dr["CodigoTipoDocumento"].ToString();
                        LblArea.Text = dr["AreaResponsable"].ToString();
                        LblFechaDoc.Text = Convert.ToDateTime(dr["FechaDocumento"]).ToString("dd/MM/yyyy");
                        LblRegistrador.Text = dr["LoginRegistrador"].ToString();

                        string codigoEstado = dr["CodigoEstado"].ToString();
                        LblEstadoBadge.Text = "<span class='badge-estado' style='background:#eee; margin-left:10px;'>" + codigoEstado + "</span>";

                        string rutaPDF = dr["RutaArchivoPDF"].ToString();
                        if (rutaPDF.StartsWith("ARC::"))
                        {
                            string idArchivo = rutaPDF.Replace("ARC::", "");
                            HfRutaPDF.Value = idArchivo;
                            IframePDF.Attributes["src"] = ResolveUrl("~/Handlers/VerPDF.ashx?id=" + idArchivo);
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

    private void CargarRevisores(int idDoc)
    {
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"SELECT NombreRevisor, CorreoRevisor, ISNULL(CodigoResultado, 'Pendiente') as CodigoResultado
                             FROM FIR_DocumentoRevisor
                             WHERE IDDocumento = @ID";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ID", idDoc);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                GvRevisores.DataSource = dt;
                GvRevisores.DataBind();
            }
        }
    }

    private void CargarObservaciones(int idDoc)
    {
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string query = @"SELECT NombreRevisor, Descripcion, FechaCreacion
                             FROM FIR_Observacion
                             WHERE IDDocumento = @ID ORDER BY FechaCreacion DESC";
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@ID", idDoc);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    PnlObservaciones.Visible = true;
                    RptObservaciones.DataSource = dt;
                    RptObservaciones.DataBind();
                }
            }
        }
    }
}
