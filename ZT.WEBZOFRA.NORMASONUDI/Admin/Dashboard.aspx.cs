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
            CargarTodosTramites();
        }
    }

    #region Navegacion
    protected void Nav_Click(object sender, EventArgs e)
    {
        LinkButton btn = (LinkButton)sender;
        string panel = btn.CommandArgument;

        PnlStats.Visible = (panel == "Stats");
        PnlOrden.Visible = (panel == "Orden");
        PnlAuditoria.Visible = (panel == "Auditoria");

        // Actualizar estilos sidebar
        BtnNavStats.CssClass = panel == "Stats" ? "nav-btn active" : "nav-btn";
        BtnNavOrden.CssClass = panel == "Orden" ? "nav-btn active" : "nav-btn";
        BtnNavAuditoria.CssClass = panel == "Auditoria" ? "nav-btn active" : "nav-btn";

        // Cargar datos segun panel
        if (panel == "Stats") {
            CargarEstadisticas();
            CargarTodosTramites();
        }
        if (panel == "Orden") {
            CargarDocumentosParaOrden("");
        }
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
                            ORDER BY d.FechaDocumento DESC, d.FechaCreacion DESC";
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
            Response.Redirect("~/Admin/DetalleAdmin.aspx?id=" + e.CommandArgument);
        }
    }
    #endregion

    // Fin del Panel Todos los Tramites

    #region Panel 4: Modificar Orden
    private void CargarDocumentosParaOrden(string busqueda)
    {
        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            string sql = @"SELECT IDDocumento, CodigoDocumento, Asunto, CodigoEstado, FechaDocumento 
                           FROM FIR_Documento 
                           WHERE CodigoEstado IN ('EN_REV', 'APR_FIRMA', 'EN_FIRMA', 'FPAR')";
            
            if (!string.IsNullOrEmpty(busqueda))
            {
                sql += " AND (CodigoDocumento LIKE @B OR IDDocumento = @ID)";
            }
            
            sql += " ORDER BY FechaDocumento DESC, FechaCreacion DESC";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (!string.IsNullOrEmpty(busqueda))
                {
                    cmd.Parameters.AddWithValue("@B", "%" + busqueda + "%");
                    int id = 0; int.TryParse(busqueda, out id);
                    cmd.Parameters.AddWithValue("@ID", id);
                }
                
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

    protected void BtnBuscarDoc_Click(object sender, EventArgs e)
    {
        string busqueda = TxtBuscarDoc.Text.Trim();
        CargarDocumentosParaOrden(busqueda);
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
            // Obtener estado del documento
            string sqlEstado = "SELECT CodigoEstado FROM FIR_Documento WHERE IDDocumento=@ID";
            SqlCommand cmdEst = new SqlCommand(sqlEstado, conn);
            cmdEst.Parameters.AddWithValue("@ID", idDoc);
            conn.Open();
            string estadoDoc = cmdEst.ExecuteScalar().ToString();
            ViewState["EstadoDocEdicion"] = estadoDoc;
            
            // Obtener firmantes
            string sql = "SELECT IDDocumentoFirmante, NombreFirmante, CorreoFirmante, OrdenFirma, CodigoEstadoFirma FROM FIR_DocumentoFirmante WHERE IDDocumento = @IDD ORDER BY OrdenFirma";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@IDD", idDoc);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (!dt.Columns.Contains("OrdenDisplay"))
                {
                    dt.Columns.Add("OrdenDisplay", typeof(int));
                }

                HashSet<int> ordenesReservados = new HashSet<int>();
                foreach (DataRow row in dt.Rows)
                {
                    if (row["CodigoEstadoFirma"].ToString() == "FIR")
                    {
                        int ordenFirmado;
                        if (int.TryParse(row["OrdenFirma"].ToString(), out ordenFirmado) && ordenFirmado > 0)
                        {
                            ordenesReservados.Add(ordenFirmado);
                        }
                    }
                }

                int siguienteOrden = 1;
                foreach (DataRow row in dt.Rows)
                {
                    bool firmado = row["CodigoEstadoFirma"].ToString() == "FIR";
                    int ordenDisplay = 0;

                    if (firmado)
                    {
                        int ordenFirmado;
                        if (int.TryParse(row["OrdenFirma"].ToString(), out ordenFirmado) && ordenFirmado > 0)
                        {
                            ordenDisplay = ordenFirmado;
                        }
                    }

                    if (ordenDisplay == 0)
                    {
                        while (ordenesReservados.Contains(siguienteOrden))
                        {
                            siguienteOrden += 1;
                        }
                        ordenDisplay = siguienteOrden;
                        siguienteOrden += 1;

                        if (firmado)
                        {
                            ordenesReservados.Add(ordenDisplay);
                        }
                    }

                    row["OrdenDisplay"] = ordenDisplay;
                }
                
                // Contar cuantos ya firmaron
                int yaFirmaron = 0;
                int totalPendientes = 0;
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["CodigoEstadoFirma"].ToString() == "FIR") yaFirmaron++;
                    else totalPendientes++;
                }
                ViewState["CantidadFirmantes"] = dt.Rows.Count;
                ViewState["YaFirmaron"] = yaFirmaron;
                ViewState["TotalPendientes"] = totalPendientes;
                
                GvFirmantesEditar.DataSource = dt;
                GvFirmantesEditar.DataBind();
                
                // Info doc
                string sqlDoc = "SELECT CodigoDocumento FROM FIR_Documento WHERE IDDocumento=@ID";
                SqlCommand cmdD = new SqlCommand(sqlDoc, conn);
                cmdD.Parameters.AddWithValue("@ID", idDoc);
                LblDocEdicion.Text = cmdD.ExecuteScalar().ToString();

                // Mostrar alerta segun estado
                MostrarAlertaModificacion(estadoDoc, yaFirmaron, totalPendientes);
                
                PnlEditarFirmantes.Visible = true;
            }
        }
    }

    private void MostrarAlertaModificacion(string estadoDoc, int yaFirmaron, int pendientes)
    {
        string alerta = "";
        bool puedeModificar = true;

        switch (estadoDoc)
        {
            case "EN_REV":
                alerta = "📋 ESTADO: EN REVISIÓN - Puede modificar el orden de firmantes libremente.";
                puedeModificar = true;
                break;
            case "EN_FIRMA":
            case "FPAR":
                if (yaFirmaron > 0)
                {
                    alerta = "⚠️ ESTADO: EN PROCESO DE FIRMA - " + yaFirmaron + " firmante(s) ya completaron su firma. Puede reordenar los " + pendientes + " pendientes.";
                }
                else
                {
                    alerta = "✅ ESTADO: EN PROCESO DE FIRMA - Todos los firmantes están pendientes. Puede modificar el orden.";
                }
                puedeModificar = true;
                break;
            case "FIRM_COM":
                alerta = "❌ ESTADO: FIRMADO COMPLETAMENTE - No se permite modificar el orden (documento finalizado).";
                puedeModificar = false;
                break;
            case "OBS":
                alerta = "❌ ESTADO: OBSERVADO - No se permite modificar el orden en este estado.";
                puedeModificar = false;
                break;
            default:
                alerta = "⚠️ ESTADO: " + estadoDoc + " - Validar antes de modificar.";
                puedeModificar = false;
                break;
        }

        ViewState["PuedeModificarOrden"] = puedeModificar;
        MostrarMensaje(alerta, !puedeModificar);
    }

    protected void GvFirmantesEditar_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            Label lblEstado = (Label)e.Row.FindControl("LblEstadoFirma");
            DropDownList ddlOrden = (DropDownList)e.Row.FindControl("DdlNuevoOrden");
            Label lblOrdenActual = (Label)e.Row.FindControl("LblOrdenActual");
            Label lblOrdenNuevo = (Label)e.Row.FindControl("LblOrdenNuevo");
            HiddenField hfOrdenAnterior = (HiddenField)e.Row.FindControl("HfOrdenAnterior");
            string estado = lblEstado.Text;
            int numFirmantes = (int)(ViewState["CantidadFirmantes"] ?? 0);

            e.Row.Attributes["data-estado"] = estado;

            int ordenActual = 0;
            object ordenDisplayObj = DataBinder.Eval(e.Row.DataItem, "OrdenDisplay");
            if (ordenDisplayObj != null)
            {
                int.TryParse(ordenDisplayObj.ToString(), out ordenActual);
            }
            if (ordenActual < 1)
            {
                int.TryParse(DataBinder.Eval(e.Row.DataItem, "OrdenFirma").ToString(), out ordenActual);
            }
            if (ordenActual < 1)
            {
                ordenActual = e.Row.RowIndex + 1;
            }

            if (lblOrdenActual != null)
            {
                lblOrdenActual.Text = ordenActual.ToString();
            }
            if (lblOrdenNuevo != null)
            {
                lblOrdenNuevo.Text = ordenActual.ToString();
            }
            if (hfOrdenAnterior != null)
            {
                hfOrdenAnterior.Value = ordenActual.ToString();
            }

            // Llenar dropdown
            if (ddlOrden != null)
            {
                for (int i = 1; i <= numFirmantes; i++)
                {
                    ddlOrden.Items.Add(new ListItem(i.ToString(), i.ToString()));
                }
                string currentOrder = ordenActual.ToString();
                if (ddlOrden.Items.FindByValue(currentOrder) != null)
                {
                    ddlOrden.SelectedValue = currentOrder;
                }
            }

            if (estado == "FIR")
            {
                lblEstado.Text = "Ya firmo";
                lblEstado.CssClass = "badge-ya-firmo";
                ddlOrden.Enabled = false;
                e.Row.CssClass = "row-locked";
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
        string estadoDoc = ViewState["EstadoDocEdicion"].ToString();
        bool puedeModificar = (bool)(ViewState["PuedeModificarOrden"] ?? false);

        // Validar si puede modificar según estado
        if (!puedeModificar)
        {
            MostrarMensaje("❌ No se permite modificar el orden en estado: " + estadoDoc, true);
            return;
        }

        List<int> nuevosOrdenes = new List<int>();
        
        // Validar duplicados
        foreach (GridViewRow row in GvFirmantesEditar.Rows)
        {
            DropDownList ddl = (DropDownList)row.FindControl("DdlNuevoOrden");
            HiddenField hfEst = (HiddenField)row.FindControl("HfEstadoFirma");
            
            // Solo validar los que no han firmado
            if (hfEst.Value != "FIR")
            {
                int val = 0;
                if (int.TryParse(ddl.SelectedValue, out val))
                {
                    if (nuevosOrdenes.Contains(val))
                    {
                        MostrarMensaje("Error: El orden " + val + " esta duplicado en firmantes pendientes.", true);
                        return;
                    }
                    nuevosOrdenes.Add(val);
                }
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
                
                // Solo actualizar los que no han firmado (CodigoEstadoFirma = 'PEN')
                foreach (GridViewRow row in GvFirmantesEditar.Rows)
                {
                    HiddenField hfID = (HiddenField)row.FindControl("HfIDFirmante");
                    HiddenField hfEst = (HiddenField)row.FindControl("HfEstadoFirma");
                    HiddenField hfOrdenAnterior = (HiddenField)row.FindControl("HfOrdenAnterior");
                    DropDownList ddl = (DropDownList)row.FindControl("DdlNuevoOrden");

                    // Si ya firmó, NO modificar
                    if (hfEst.Value == "FIR") continue;

                    int nuevoOrden = int.Parse(ddl.SelectedValue);
                    int ordenAnterior = int.Parse(hfOrdenAnterior.Value);

                    // Solo actualizar si cambió el orden
                    if (nuevoOrden != ordenAnterior)
                    {
                        string sqlUp = @"UPDATE FIR_DocumentoFirmante SET OrdenFirma = @O, IDUsuarioModificador = @U, FechaModificacion = GETDATE() 
                                         WHERE IDDocumentoFirmante = @ID AND CodigoEstadoFirma = 'PEN'";
                        using (SqlCommand cmdU = new SqlCommand(sqlUp, conn))
                        {
                            cmdU.Parameters.AddWithValue("@O", nuevoOrden);
                            cmdU.Parameters.AddWithValue("@U", loginAdmin);
                            cmdU.Parameters.AddWithValue("@ID", hfID.Value);
                            cmdU.ExecuteNonQuery();
                        }

                        // Auditoria
                        string sqlAud = @"INSERT INTO FIR_DocumentoAuditoria (IDDocumento, IDUsuario, NombreUsuario, IDEquipo, TipoOperacion, TipoAccion, Descripcion, FechaCambio)
                                          VALUES (@IDD, @U, @N, @IP, 'M', 'CAMBIO_ORDEN_FIRMA', @D, GETDATE())";
                        using (SqlCommand cmdA = new SqlCommand(sqlAud, conn))
                        {
                            cmdA.Parameters.AddWithValue("@IDD", idDoc);
                            cmdA.Parameters.AddWithValue("@U", loginAdmin);
                            cmdA.Parameters.AddWithValue("@N", Session["strNombre"]);
                            cmdA.Parameters.AddWithValue("@IP", ip);
                            cmdA.Parameters.AddWithValue("@D", "Cambio orden de firma: " + ordenAnterior + " → " + nuevoOrden);
                            cmdA.ExecuteNonQuery();
                        }
                    }
                }

                // Recalcular Habilitado segun estado: Solo EN_FIRMA y FPAR requieren recalcular
                if (estadoDoc == "EN_FIRMA" || estadoDoc == "FPAR")
                {
                    string sqlRecalc = @"
                        UPDATE FIR_DocumentoFirmante SET Habilitado = 0 WHERE IDDocumento = @IDD AND CodigoEstadoFirma = 'PEN';
                        UPDATE FIR_DocumentoFirmante SET Habilitado = 1 
                        WHERE IDDocumentoFirmante = (
                            SELECT TOP 1 IDDocumentoFirmante 
                            FROM FIR_DocumentoFirmante 
                            WHERE IDDocumento = @IDD AND CodigoEstadoFirma = 'PEN' 
                            ORDER BY OrdenFirma ASC
                        );";
                    using (SqlCommand cmdRecalc = new SqlCommand(sqlRecalc, conn))
                    {
                        cmdRecalc.Parameters.AddWithValue("@IDD", idDoc);
                        cmdRecalc.ExecuteNonQuery();
                    }
                }
            }
            
            MostrarMensaje("✅ Orden de firmas actualizado. Los usuarios verán el cambio en su próximo acceso.");
            CargarFirmantesEdicion(idDoc);
        }
        catch (Exception ex)
        {
            MostrarMensaje("❌ Error al guardar: " + ex.Message, true);
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
                            WHERE (@Desde = '' OR CAST(a.FechaCambio as DATE) >= @Desde)
                              AND (@Hasta = '' OR CAST(a.FechaCambio as DATE) <= @Hasta)
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