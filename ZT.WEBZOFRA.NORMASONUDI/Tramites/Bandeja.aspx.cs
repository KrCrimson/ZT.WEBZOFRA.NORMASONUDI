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
            ManejarAccionMenu();
            CargarTramites();

            string msg = Request.QueryString["msg"];
            if (msg == "registrado")
            {
                LblExito.Text = "Tramite registrado exitosamente.";
                LblExito.Visible = true;
            }

            if (Session["AlertasPendientes"] != null)
            {
                int pendientes = (int)Session["AlertasPendientes"];
                LblError.Text = "Tiene " + pendientes + " revision(es) pendiente(s) o vencida(s). Por favor, revise sus tramites.";
                LblError.Visible = true;
                Session.Remove("AlertasPendientes");
            }
        }
    }

    private void ManejarAccionMenu()
    {
        string action = Request.QueryString["action"];
        if (string.IsNullOrEmpty(action))
        {
            string rol = Session["strRol"] != null ? Session["strRol"].ToString() : "";
            if (rol == "REGISTRADOR") action = "Historial";
            else if (rol == "FIRMADOR") action = "PendientesRev";
            else if (rol == "ADMIN") action = "TodosTramites";
        }

        switch (action)
        {
            case "Historial":
                ViewState["FiltroEstado"] = null;
                LblTituloBandeja.Text = "Historial";
                break;
            case "EnRevision":
                ViewState["FiltroEstado"] = "EN_REV";
                LblTituloBandeja.Text = "En Revision";
                break;
            case "EnFirma":
                ViewState["FiltroEstado"] = "APR_FIRMA,EN_FIRMA,FPAR";
                LblTituloBandeja.Text = "En Firma";
                break;
            case "PendientesRev":
                ViewState["FiltroEstado"] = "EN_REV,OBS";
                LblTituloBandeja.Text = "Pendientes de Revisión";
                break;
            case "PendientesFirma":
                ViewState["FiltroEstado"] = "APR_FIRMA,EN_FIRMA,FPAR";
                LblTituloBandeja.Text = "Pendientes de Firma";
                break;
            case "Completados":
                ViewState["FiltroEstado"] = "FIRM_COM";
                LblTituloBandeja.Text = "Completados";
                break;
            case "TodosTramites":
                ViewState["FiltroEstado"] = null;
                LblTituloBandeja.Text = "Todos los Trámites";
                break;
        }
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
                              ORDER BY d.FechaDocumento DESC, d.FechaCreacion DESC";
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
                              ORDER BY d.FechaDocumento DESC, d.FechaCreacion DESC";
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
                              ORDER BY d.FechaDocumento DESC, d.FechaCreacion DESC";
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
                if (filtroEstado == "APR_FIRMA,EN_FIRMA,FPAR" || filtroEstado == "APR_FIRMA,EN_FIRMA")
                {
                    filas = dt.Select("CodigoEstado = 'APR_FIRMA' OR CodigoEstado = 'EN_FIRMA' OR CodigoEstado = 'FPAR'");
                }
                else if (filtroEstado == "EN_REV,OBS")
                {
                    filas = dt.Select("CodigoEstado = 'EN_REV' OR CodigoEstado = 'OBS'");
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

            if (!string.IsNullOrEmpty(TxtBuscar.Text.Trim()))
            {
                string termino = TxtBuscar.Text.Trim().Replace("'", "''");
                DataRow[] filas = dt.Select($"CodigoDocumento LIKE '%{termino}%' OR Asunto LIKE '%{termino}%'");
                DataTable dtBuscado = dt.Clone();
                foreach (DataRow fila in filas)
                {
                    dtBuscado.ImportRow(fila);
                }
                dt = dtBuscado;
            }

            DataView dv = dt.DefaultView;
            dv.Sort = "FechaDocumento DESC";
            DataTable dtOrdenado = dv.ToTable();

            ViewState["dtTramites"] = dtOrdenado;
            GvTramites.DataSource = dtOrdenado;
            GvTramites.DataBind();
        }
        catch (Exception ex)
        {
            LblError.Text = "Error al cargar tramites: " + ex.Message;
            LblError.Visible = true;
        }
    }

    protected void BtnBuscar_Click(object sender, EventArgs e)
    {
        CargarTramites();
    }

    protected void BtnLimpiar_Click(object sender, EventArgs e)
    {
        TxtBuscar.Text = "";
        CargarTramites();
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
}
