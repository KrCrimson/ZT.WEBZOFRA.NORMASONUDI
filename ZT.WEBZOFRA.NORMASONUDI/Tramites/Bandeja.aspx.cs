using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Drawing;
using System.Web.UI.WebControls;

public partial class Bandeja : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["strUsuario"] == null)
        {
            Response.Redirect("~/Login.aspx");
            return;
        }

        if (!IsPostBack)
        {
            LblBienvenida.Text = "Bienvenido, " + Session["strNombre"].ToString();
            ConfigurarSidebar();
            CargarTramites();
            BindCalendario();

            if (Session["AlertasPendientes"] != null)
            {
                int pendientes = (int)Session["AlertasPendientes"];
                LblError.Text = "⚠️ Tiene " + pendientes + " revisión(es) pendiente(s) o vencida(s). Por favor, revise sus trámites.";
                LblError.Visible = true;
                Session.Remove("AlertasPendientes");
            }
        }
    }

    private void ConfigurarSidebar()
    {
        string rol = Session["strRol"] != null ? Session["strRol"].ToString() : "";

        PnlMenuRegistrador.Visible = (rol == "REGISTRADOR");
        PnlMenuFirmador.Visible = (rol == "FIRMADOR");
        PnlMenuAdmin.Visible = (rol == "ADMIN");
    }

    private void CargarTramites()
    {
        string rol = Session["strRol"] != null ? Session["strRol"].ToString() : "";
        string login = Session["strUsuario"].ToString();
        string filtroEstado = ViewState["FiltroEstado"] != null ? ViewState["FiltroEstado"].ToString() : "";

        string connStr = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
        DataTable dt = new DataTable();

        try
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "";

                if (rol == "ADMIN")
                {
                    query = @"SELECT d.IDDocumento, d.CodigoDocumento, d.Asunto,
                                     m.Descripcion AS TipoDocumento, d.AreaResponsable,
                                     d.FechaDocumento, d.CodigoEstado,
                                     me.Descripcion AS Estado, d.Version,
                                     d.LoginRegistrador, d.FechaLimiteRevision
                              FROM FIR_Documento d
                              LEFT JOIN FIR_Maestro m ON m.Tipo='TIPO_DOC' AND m.Codigo=d.CodigoTipoDocumento
                              LEFT JOIN FIR_Maestro me ON me.Tipo='ESTADO_DOC' AND me.Codigo=d.CodigoEstado
                              ORDER BY d.FechaCreacion DESC";
                }
                else if (rol == "FIRMADOR")
                {
                    query = @"SELECT DISTINCT d.IDDocumento, d.CodigoDocumento, d.Asunto,
                                     m.Descripcion AS TipoDocumento, d.AreaResponsable,
                                     d.FechaDocumento, d.CodigoEstado,
                                     me.Descripcion AS Estado, d.Version,
                                     d.FechaLimiteRevision, d.FechaCreacion
                              FROM FIR_Documento d
                              INNER JOIN FIR_DocumentoFirmante df ON df.IDDocumento=d.IDDocumento AND df.LoginUsuario=@LoginUsuario
                              LEFT JOIN FIR_Maestro m ON m.Tipo='TIPO_DOC' AND m.Codigo=d.CodigoTipoDocumento
                              LEFT JOIN FIR_Maestro me ON me.Tipo='ESTADO_DOC' AND me.Codigo=d.CodigoEstado
                              ORDER BY d.FechaCreacion DESC";
                }
                else
                {
                    query = @"SELECT d.IDDocumento, d.CodigoDocumento, d.Asunto,
                                     m.Descripcion AS TipoDocumento, d.AreaResponsable,
                                     d.FechaDocumento, d.CodigoEstado,
                                     me.Descripcion AS Estado, d.Version,
                                     d.FechaLimiteRevision
                              FROM FIR_Documento d
                              LEFT JOIN FIR_Maestro m ON m.Tipo='TIPO_DOC' AND m.Codigo=d.CodigoTipoDocumento
                              LEFT JOIN FIR_Maestro me ON me.Tipo='ESTADO_DOC' AND me.Codigo=d.CodigoEstado
                              WHERE d.LoginRegistrador = @LoginUsuario
                              ORDER BY d.FechaCreacion DESC";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LoginUsuario", login);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }

            if (!string.IsNullOrEmpty(filtroEstado))
            {
                DataRow[] filas;
                if (filtroEstado == "APR_FIRMA,EN_FIRMA")
                {
                    filas = dt.Select("CodigoEstado = 'APR_FIRMA' OR CodigoEstado = 'EN_FIRMA'");
                }
                else
                {
                    filas = dt.Select("CodigoEstado = '" + filtroEstado.Replace("'", "''") + "'");
                }

                DataTable dtFiltrado = dt.Clone();
                foreach (DataRow fila in filas)
                {
                    dtFiltrado.ImportRow(fila);
                }
                dt = dtFiltrado;
            }

            ViewState["dtTramites"] = dt;
            GvTramites.DataSource = dt;
            GvTramites.DataBind();
        }
        catch (Exception ex)
        {
            LblError.Text = "Error al cargar trámites: " + ex.Message;
            LblError.Visible = true;
        }
    }

    private void BindCalendario()
    {
        DataTable dt = ViewState["dtTramites"] as DataTable;
        if (dt == null) return;

        List<DateTime> fechas = new List<DateTime>();
        foreach (DataRow row in dt.Rows)
        {
            if (row["FechaDocumento"] != DBNull.Value)
            {
                DateTime fecha = Convert.ToDateTime(row["FechaDocumento"]).Date;
                if (!fechas.Contains(fecha))
                {
                    fechas.Add(fecha);
                }
            }
        }
        ViewState["FechasTramites"] = fechas;
    }

    protected void CalBandeja_DayRender(object sender, DayRenderEventArgs e)
    {
        List<DateTime> fechas = ViewState["FechasTramites"] as List<DateTime>;
        if (fechas != null && fechas.Contains(e.Day.Date))
        {
            e.Cell.CssClass = "cal-highlight";
            e.Cell.ToolTip = "Hay trámites en esta fecha";
        }

        DataTable dt = ViewState["dtTramites"] as DataTable;
        if (dt != null && dt.Columns.Contains("FechaLimiteRevision"))
        {
            foreach (DataRow row in dt.Rows)
            {
                if (row["FechaLimiteRevision"] != DBNull.Value && row["CodigoEstado"].ToString() == "EN_REV")
                {
                    DateTime fechaLimite = Convert.ToDateTime(row["FechaLimiteRevision"]).Date;
                    if (fechaLimite == e.Day.Date)
                    {
                        TimeSpan diff = fechaLimite - DateTime.Now.Date;
                        string codigo = row["CodigoDocumento"].ToString();
                        if (diff.Days <= 0)
                        {
                            e.Cell.BackColor = Color.Red;
                            e.Cell.ForeColor = Color.White;
                            e.Cell.ToolTip = "VENCIDO: " + codigo;
                        }
                        else if (diff.Days <= 7)
                        {
                            e.Cell.BackColor = Color.LightCoral;
                            e.Cell.ToolTip = "Vence: " + codigo;
                        }
                        else
                        {
                            e.Cell.BackColor = Color.LightBlue;
                            e.Cell.ToolTip = "Límite: " + codigo;
                        }
                        break;
                    }
                }
            }
        }
    }

    protected void CalBandeja_SelectionChanged(object sender, EventArgs e)
    {
        DateTime fechaSeleccionada = CalBandeja.SelectedDate.Date;
        DataTable dt = ViewState["dtTramites"] as DataTable;
        if (dt == null) return;

        DataRow[] filas = dt.Select("FechaDocumento = '" + fechaSeleccionada.ToString("yyyy-MM-dd") + "'");
        DataTable dtFiltrado = dt.Clone();
        foreach (DataRow fila in filas)
        {
            dtFiltrado.ImportRow(fila);
        }

        GvTramites.DataSource = dtFiltrado;
        GvTramites.DataBind();

        LblTituloBandeja.Text = "Trámites del " + fechaSeleccionada.ToString("dd/MM/yyyy");
    }

    protected void LnkLimpiarFiltroFecha_Click(object sender, EventArgs e)
    {
        ViewState["FiltroEstado"] = null;
        CargarTramites();
        BindCalendario();
        LblTituloBandeja.Text = "Todos los trámites";
    }

    protected void GvTramites_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "VerDetalle")
        {
            Response.Redirect("~/Tramites/Detalle.aspx?id=" + e.CommandArgument.ToString());
        }
    }

    protected void GvTramites_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            DataRowView drv = (DataRowView)e.Row.DataItem;
            string codigoEstado = drv["CodigoEstado"] != DBNull.Value ? drv["CodigoEstado"].ToString() : "";

            if (codigoEstado == "EN_REV" && drv.DataView.Table.Columns.Contains("FechaLimiteRevision") && drv["FechaLimiteRevision"] != DBNull.Value)
            {
                DateTime fechaLimite = Convert.ToDateTime(drv["FechaLimiteRevision"]);
                TimeSpan diff = fechaLimite - DateTime.Now;

                if (diff.Days <= 0)
                {
                    e.Row.BackColor = Color.Red;
                    e.Row.ForeColor = Color.White;
                }
                else if (diff.Days <= 7)
                {
                    e.Row.BackColor = Color.LightCoral;
                }
            }
        }
    }

    // ─── SIDEBAR: REGISTRADOR ───

    protected void LnkNuevoTramite_Click(object sender, EventArgs e)
    {
        Response.Redirect("~/Tramites/RegistrarTramite.aspx");
    }

    protected void LnkMisTramites_Click(object sender, EventArgs e)
    {
        ViewState["FiltroEstado"] = null;
        CargarTramites();
        BindCalendario();
        LblTituloBandeja.Text = "Mis Trámites";
    }

    // ─── SIDEBAR: FIRMADOR ───

    protected void LnkPendientesRev_Click(object sender, EventArgs e)
    {
        ViewState["FiltroEstado"] = "EN_REV";
        CargarTramites();
        BindCalendario();
        LblTituloBandeja.Text = "Pendientes de Revisión";
    }

    protected void LnkPendientesFirma_Click(object sender, EventArgs e)
    {
        ViewState["FiltroEstado"] = "APR_FIRMA,EN_FIRMA";
        CargarTramites();
        BindCalendario();
        LblTituloBandeja.Text = "Pendientes de Firma";
    }

    protected void LnkCompletados_Click(object sender, EventArgs e)
    {
        ViewState["FiltroEstado"] = "FIRM_COM";
        CargarTramites();
        BindCalendario();
        LblTituloBandeja.Text = "Completados";
    }

    // ─── SIDEBAR: ADMIN ───

    protected void LnkTodosTramites_Click(object sender, EventArgs e)
    {
        ViewState["FiltroEstado"] = null;
        CargarTramites();
        BindCalendario();
        LblTituloBandeja.Text = "Todos los Trámites";
    }

    protected void LnkGestionarRoles_Click(object sender, EventArgs e)
    {
        Response.Redirect("~/Admin/Dashboard.aspx");
    }

    protected void LnkCerrarSesion_Click(object sender, EventArgs e)
    {
        Session.Abandon();
        Response.Redirect("~/Login.aspx");
    }
}