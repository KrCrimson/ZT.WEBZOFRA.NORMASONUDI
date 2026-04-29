using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Linq;

public partial class Dashboard : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["strUsuario"] == null || Session["strRol"] == null || Session["strRol"].ToString() != "ADMIN")
        {
            Response.Redirect("~/Login.aspx");
            return;
        }

        if (!IsPostBack)
        {
            LblAdminName.Text = Session["strNombre"].ToString();
            CargarEstadisticas();
            CargarEstadosFiltro();
        }
    }

    #region Navegacion
    protected void Nav_Click(object sender, EventArgs e)
    {
        LinkButton btn = (LinkButton)sender;
        string panel = btn.CommandArgument;

        PnlStats.Visible = (panel == "Stats");
        PnlTramites.Visible = (panel == "Tramites");
        PnlRoles.Visible = (panel == "Roles");
        PnlOrden.Visible = (panel == "Orden");
        PnlAuditoria.Visible = (panel == "Auditoria");

        // Actualizar estilos sidebar
        BtnNavStats.CssClass = panel == "Stats" ? "nav-btn active" : "nav-btn";
        BtnNavTramites.CssClass = panel == "Tramites" ? "nav-btn active" : "nav-btn";
        BtnNavRoles.CssClass = panel == "Roles" ? "nav-btn active" : "nav-btn";
        BtnNavOrden.CssClass = panel == "Orden" ? "nav-btn active" : "nav-btn";
        BtnNavAuditoria.CssClass = panel == "Auditoria" ? "nav-btn active" : "nav-btn";

        // Cargar datos segun panel
        if (panel == "Stats") CargarEstadisticas();
        if (panel == "Tramites") CargarTodosTramites();
        if (panel == "Roles") CargarUsuariosRoles();
        if (panel == "Auditoria") CargarAuditoria();
    }

    protected void BtnLogout_Click(object sender, EventArgs e)
    {
        Session.Abandon();
        Response.Redirect("~/Login.aspx");
    }
    #endregion

    #region Panel 1: Estadisticas
    private void CargarEstadisticas()
    {
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string sql = @"SELECT 
                              COUNT(*) AS Total,
                              SUM(CASE WHEN CodigoEstado='EN_REV' THEN 1 ELSE 0 END) AS EnRevision,
                              SUM(CASE WHEN CodigoEstado='OBS' THEN 1 ELSE 0 END) AS Observados,
                              SUM(CASE WHEN CodigoEstado='APR_FIRMA' THEN 1 ELSE 0 END) AS AprobParaFirma,
                              SUM(CASE WHEN CodigoEstado='EN_FIRMA' THEN 1 ELSE 0 END) AS EnFirma,
                              SUM(CASE WHEN CodigoEstado='FPAR' THEN 1 ELSE 0 END) AS FirmadoParcial,
                              SUM(CASE WHEN CodigoEstado='FIRM_COM' THEN 1 ELSE 0 END) AS Completados
                           FROM FIR_Documento";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        LblTotal.Text = dr["Total"].ToString();
                        LblEnRev.Text = dr["EnRevision"].ToString();
                        LblObs.Text = dr["Observados"].ToString();
                        LblAprFirma.Text = dr["AprobParaFirma"].ToString();
                        LblEnFirma.Text = dr["EnFirma"].ToString();
                        LblFPar.Text = dr["FirmadoParcial"].ToString();
                        LblCompletos.Text = dr["Completados"].ToString();
                    }
                }
            }
        }
    }
    #endregion

    #region Panel 2: Todos los Tramites
    private void CargarEstadosFiltro()
    {
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string sql = "SELECT Codigo, Descripcion FROM FIR_Maestro WHERE Tipo='ESTADO_DOC' ORDER BY Descripcion";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                DdlFiltroEstado.DataSource = cmd.ExecuteReader();
                DdlFiltroEstado.DataTextField = "Descripcion";
                DdlFiltroEstado.DataValueField = "Codigo";
                DdlFiltroEstado.DataBind();
                DdlFiltroEstado.Items.Insert(0, new ListItem("-- Todos los estados --", ""));
            }
        }
    }

    private void CargarTodosTramites()
    {
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string sql = @"SELECT d.IDDocumento, d.CodigoDocumento, d.Asunto,
                                   m.Descripcion AS TipoDocumento, d.AreaResponsable,
                                   d.FechaDocumento, d.CodigoEstado,
                                   me.Descripcion AS Estado, d.LoginRegistrador,
                                   d.FechaCreacion
                            FROM FIR_Documento d
                            LEFT JOIN FIR_Maestro m ON m.Tipo='TIPO_DOC' AND m.Codigo=d.CodigoTipoDocumento
                            LEFT JOIN FIR_Maestro me ON me.Tipo='ESTADO_DOC' AND me.Codigo=d.CodigoEstado
                            WHERE (@Estado = '' OR d.CodigoEstado = @Estado)
                            ORDER BY d.FechaCreacion DESC";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Estado", DdlFiltroEstado.SelectedValue);
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                GvTodosTramites.DataSource = dt;
                GvTodosTramites.DataBind();
            }
        }
    }

    protected void DdlFiltroEstado_SelectedIndexChanged(object sender, EventArgs e)
    {
        CargarTodosTramites();
    }

    protected void GvTodosTramites_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "VerDetalle")
        {
            Response.Redirect("~/Tramites/Detalle.aspx?id=" + e.CommandArgument);
        }
    }
    #endregion

    #region Panel 3: Gestion de Roles
    private void CargarUsuariosRoles()
    {
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string sql = @"SELECT e.LoginUsuario, e.NombreCompleto, e.Email,
                              CASE e.IdRol
                                WHEN 1 THEN 'ADMIN'
                                WHEN 2 THEN 'REGISTRADOR'
                                WHEN 3 THEN 'FIRMADOR'
                                ELSE 'SIN ROL'
                              END AS CodigoRol
                            FROM FIR_VW_EmpleadosActivos e
                            ORDER BY e.NombreCompleto";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                GvUsuarios.DataSource = dt;
                GvUsuarios.DataBind();
            }
        }
    }

    protected void GvUsuarios_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "CambiarRol")
        {
            int index = Convert.ToInt32(e.CommandArgument);
            GridViewRow row = GvUsuarios.Rows[index];
            string loginUsuario = row.Cells[0].Text;
            DropDownList ddl = (DropDownList)row.FindControl("DdlNuevoRol");
            string nuevoRol = ddl.SelectedValue;

            ActualizarRol(loginUsuario, nuevoRol);
        }
    }

    private void ActualizarRol(string login, string rol)
    {
        int idRol = 3; // default FIRMADOR
        if (rol == "ADMIN") idRol = 1;
        if (rol == "REGISTRADOR") idRol = 2;

        string connAdminStr = ConfigurationManager.ConnectionStrings["Administracion"].ConnectionString;
        try
        {
            using (SqlConnection conn = new SqlConnection(connAdminStr))
            {
                string sql = "UPDATE dbo.Empleado SET IdRol = @IdRol WHERE LoginUsuario = @Login";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@IdRol", idRol);
                    cmd.Parameters.AddWithValue("@Login", login);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            MostrarMensaje("Rol de " + login + " actualizado correctamente a " + rol);
            CargarUsuariosRoles();
        }
        catch (Exception ex)
        {
            MostrarMensaje("Error: " + ex.Message, true);
        }
    }
    #endregion

    #region Panel 4: Modificar Orden
    protected void BtnBuscarDoc_Click(object sender, EventArgs e)
    {
        string busqueda = TxtBuscarDoc.Text.Trim();
        if (string.IsNullOrEmpty(busqueda)) return;

        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string sql = @"SELECT IDDocumento, CodigoDocumento, Asunto, CodigoEstado 
                           FROM FIR_Documento 
                           WHERE (CodigoDocumento LIKE @B OR IDDocumento = @ID)
                             AND CodigoEstado IN ('APR_FIRMA','EN_FIRMA','FPAR')";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@B", "%" + busqueda + "%");
                int id = 0; int.TryParse(busqueda, out id);
                cmd.Parameters.AddWithValue("@ID", id);
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                GvDocsBusqueda.DataSource = dt;
                GvDocsBusqueda.DataBind();
                GvDocsBusqueda.Visible = true;
                PnlEditarFirmantes.Visible = false;
            }
        }
    }

    protected void GvDocsBusqueda_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "EditarF")
        {
            int idDoc = Convert.ToInt32(e.CommandArgument);
            ViewState["IDDocEdicion"] = idDoc;
            CargarFirmantesEdicion(idDoc);
        }
    }

    private void CargarFirmantesEdicion(int idDoc)
    {
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string sql = "SELECT IDDocumentoFirmante, NombreFirmante, CorreoFirmante, OrdenFirma, CodigoEstadoFirma FROM FIR_DocumentoFirmante WHERE IDDocumento = @IDD ORDER BY OrdenFirma";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@IDD", idDoc);
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                GvFirmantesEditar.DataSource = dt;
                GvFirmantesEditar.DataBind();
                
                // Info doc
                string sqlDoc = "SELECT CodigoDocumento FROM FIR_Documento WHERE IDDocumento=@ID";
                SqlCommand cmdD = new SqlCommand(sqlDoc, conn);
                cmdD.Parameters.AddWithValue("@ID", idDoc);
                LblDocEdicion.Text = cmdD.ExecuteScalar().ToString();

                PnlEditarFirmantes.Visible = true;
            }
        }
    }

    protected void GvFirmantesEditar_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            Label lblEstado = (Label)e.Row.FindControl("LblEstadoFirma");
            TextBox txtOrden = (TextBox)e.Row.FindControl("TxtNuevoOrden");
            string estado = lblEstado.Text;

            if (estado == "FIR")
            {
                lblEstado.Text = "Ya firmo";
                lblEstado.CssClass = "badge-ya-firmo";
                txtOrden.Enabled = false;
            }
            else
            {
                lblEstado.Text = "Pendiente";
                lblEstado.CssClass = "badge-pendiente";
            }
        }
    }

    protected void BtnGuardarOrden_Click(object sender, EventArgs e)
    {
        int idDoc = (int)ViewState["IDDocEdicion"];
        List<int> nuevosOrdenes = new List<int>();
        
        // Validar duplicados
        foreach (GridViewRow row in GvFirmantesEditar.Rows)
        {
            TextBox txt = (TextBox)row.FindControl("TxtNuevoOrden");
            int val = 0;
            if (int.TryParse(txt.Text, out val))
            {
                if (nuevosOrdenes.Contains(val))
                {
                    MostrarMensaje("Error: El orden " + val + " esta duplicado.", true);
                    return;
                }
                nuevosOrdenes.Add(val);
            }
        }

        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        string loginAdmin = Session["strUsuario"].ToString();
        string ip = Request.UserHostAddress;

        try
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                foreach (GridViewRow row in GvFirmantesEditar.Rows)
                {
                    HiddenField hfID = (HiddenField)row.FindControl("HfIDFirmante");
                    HiddenField hfEst = (HiddenField)row.FindControl("HfEstadoFirma");
                    TextBox txt = (TextBox)row.FindControl("TxtNuevoOrden");

                    if (hfEst.Value == "PEN")
                    {
                        int idFirmante = Convert.ToInt32(hfID.Value);
                        int nOrden = Convert.ToInt32(txt.Text);
                        string nombre = row.Cells[0].Text;

                        // Update
                        string sqlUp = @"UPDATE FIR_DocumentoFirmante SET OrdenFirma = @O, IDUsuarioModificador = @U, FechaModificacion = GETDATE() 
                                         WHERE IDDocumentoFirmante = @ID AND CodigoEstadoFirma = 'PEN'";
                        using (SqlCommand cmdU = new SqlCommand(sqlUp, conn))
                        {
                            cmdU.Parameters.AddWithValue("@O", nOrden);
                            cmdU.Parameters.AddWithValue("@U", loginAdmin);
                            cmdU.Parameters.AddWithValue("@ID", idFirmante);
                            cmdU.ExecuteNonQuery();
                        }

                        // Audit
                        string sqlAud = @"INSERT INTO FIR_DocumentoAuditoria (IDDocumento, IDUsuario, NombreUsuario, IDEquipo, TipoOperacion, TipoAccion, Descripcion, FechaCambio)
                                          VALUES (@IDD, @U, @N, @IP, 'M', 'CAMBIO_ORDEN_FIRMA', @D, GETDATE())";
                        using (SqlCommand cmdA = new SqlCommand(sqlAud, conn))
                        {
                            cmdA.Parameters.AddWithValue("@IDD", idDoc);
                            cmdA.Parameters.AddWithValue("@U", loginAdmin);
                            cmdA.Parameters.AddWithValue("@N", Session["strNombre"]);
                            cmdA.Parameters.AddWithValue("@IP", ip);
                            cmdA.Parameters.AddWithValue("@D", "Admin cambio orden de firma de " + nombre + " a " + nOrden);
                            cmdA.ExecuteNonQuery();
                        }
                    }
                }
            }
            MostrarMensaje("Orden de firmas actualizado correctamente.");
            CargarFirmantesEdicion(idDoc);
        }
        catch (Exception ex)
        {
            MostrarMensaje("Error al guardar: " + ex.Message, true);
        }
    }
    #endregion

    #region Panel 5: Auditoria
    private void CargarAuditoria()
    {
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string sql = @"SELECT a.FechaCambio, d.CodigoDocumento, d.Asunto,
                                   a.IDUsuario, a.NombreUsuario, a.TipoOperacion,
                                   a.TipoAccion, a.Descripcion, a.IDEquipo
                            FROM FIR_DocumentoAuditoria a
                            LEFT JOIN FIR_Documento d ON d.IDDocumento=a.IDDocumento
                            WHERE (@Desde = '' OR a.FechaCambio >= @Desde)
                              AND (@Hasta = '' OR a.FechaCambio <= @Hasta)
                              AND (@Doc = '' OR d.CodigoDocumento LIKE @DocB)
                            ORDER BY a.FechaCambio DESC";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Desde", TxtFechaDesde.Text);
                cmd.Parameters.AddWithValue("@Hasta", TxtFechaHasta.Text);
                cmd.Parameters.AddWithValue("@Doc", TxtFiltroDocAuditoria.Text.Trim());
                cmd.Parameters.AddWithValue("@DocB", "%" + TxtFiltroDocAuditoria.Text.Trim() + "%");
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                GvAuditoria.DataSource = dt;
                GvAuditoria.DataBind();
            }
        }
    }

    protected void BtnFiltrarAuditoria_Click(object sender, EventArgs e)
    {
        CargarAuditoria();
    }

    protected void GvAuditoria_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        GvAuditoria.PageIndex = e.NewPageIndex;
        CargarAuditoria();
    }
    #endregion

    private void MostrarMensaje(string msg, bool esError = false)
    {
        LblGlobalMsg.Text = msg;
        LblGlobalMsg.Visible = true;
        LblGlobalMsg.CssClass = esError ? "alert alert-danger" : "alert alert-success";
    }
}