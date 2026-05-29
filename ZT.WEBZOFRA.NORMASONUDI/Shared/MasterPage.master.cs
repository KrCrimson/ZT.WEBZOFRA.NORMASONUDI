using System;
using System.Linq;

public partial class Shared_MasterPage : System.Web.UI.MasterPage
{
    public string BodyCssClass
    {
        get
        {
            return BodyTag.Attributes["class"] ?? "app-body";
        }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                BodyTag.Attributes["class"] = "app-body";
                return;
            }

            BodyTag.Attributes["class"] = "app-body " + value.Trim();
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            string nombre = Session["strNombre"] != null ? Session["strNombre"].ToString() : "Usuario";
            string rol = Session["strRol"] != null ? Session["strRol"].ToString() : "Sesion";

            LblHeaderUser.Text = string.IsNullOrWhiteSpace(nombre) ? "Usuario" : nombre;
            LblHeaderRole.Text = string.IsNullOrWhiteSpace(rol) ? "Sesion" : rol;

            string initials = BuildInitials(nombre);
            UserInitials.InnerText = string.IsNullOrWhiteSpace(initials) ? "US" : initials;

            ConfigurarSidebar(rol);
            MarcarMenuActivo();
        }

    }

    private void ConfigurarSidebar(string rol)
    {
        System.Web.UI.WebControls.Panel PnlMenuRegistrador = (System.Web.UI.WebControls.Panel)SidebarContent.FindControl("PnlMenuRegistrador");
        System.Web.UI.WebControls.Panel PnlMenuFirmador = (System.Web.UI.WebControls.Panel)SidebarContent.FindControl("PnlMenuFirmador");
        System.Web.UI.WebControls.Panel PnlMenuAdmin = (System.Web.UI.WebControls.Panel)SidebarContent.FindControl("PnlMenuAdmin");

        if (PnlMenuRegistrador != null) PnlMenuRegistrador.Visible = (rol == "REGISTRADOR");
        if (PnlMenuFirmador != null) PnlMenuFirmador.Visible = (rol == "FIRMADOR");
        if (PnlMenuAdmin != null) PnlMenuAdmin.Visible = (rol == "ADMIN");
    }

    private void MarcarMenuActivo()
    {
        string currentUrl = Request.Url.AbsolutePath.ToLower();
        string activeMenu = Session["ActiveMenu"] as string;

        System.Web.UI.WebControls.LinkButton LnkHistorial = (System.Web.UI.WebControls.LinkButton)SidebarContent.FindControl("LnkHistorial");
        System.Web.UI.WebControls.LinkButton LnkRegistrarDocumento = (System.Web.UI.WebControls.LinkButton)SidebarContent.FindControl("LnkRegistrarDocumento");
        System.Web.UI.WebControls.LinkButton LnkEnRevision = (System.Web.UI.WebControls.LinkButton)SidebarContent.FindControl("LnkEnRevision");
        System.Web.UI.WebControls.LinkButton LnkEnFirma = (System.Web.UI.WebControls.LinkButton)SidebarContent.FindControl("LnkEnFirma");
        System.Web.UI.WebControls.LinkButton LnkPendientesRev = (System.Web.UI.WebControls.LinkButton)SidebarContent.FindControl("LnkPendientesRev");
        System.Web.UI.WebControls.LinkButton LnkPendientesFirma = (System.Web.UI.WebControls.LinkButton)SidebarContent.FindControl("LnkPendientesFirma");
        System.Web.UI.WebControls.LinkButton LnkCompletados = (System.Web.UI.WebControls.LinkButton)SidebarContent.FindControl("LnkCompletados");
        System.Web.UI.WebControls.LinkButton LnkTodosTramites = (System.Web.UI.WebControls.LinkButton)SidebarContent.FindControl("LnkTodosTramites");
        System.Web.UI.WebControls.LinkButton LnkGestionarRoles = (System.Web.UI.WebControls.LinkButton)SidebarContent.FindControl("LnkGestionarRoles");

        if (LnkHistorial != null) LnkHistorial.CssClass = "nav-link";
        if (LnkRegistrarDocumento != null) LnkRegistrarDocumento.CssClass = "nav-link";
        if (LnkEnRevision != null) LnkEnRevision.CssClass = "nav-link";
        if (LnkEnFirma != null) LnkEnFirma.CssClass = "nav-link";
        if (LnkPendientesRev != null) LnkPendientesRev.CssClass = "nav-link";
        if (LnkPendientesFirma != null) LnkPendientesFirma.CssClass = "nav-link";
        if (LnkCompletados != null) LnkCompletados.CssClass = "nav-link";
        if (LnkTodosTramites != null) LnkTodosTramites.CssClass = "nav-link";
        if (LnkGestionarRoles != null) LnkGestionarRoles.CssClass = "nav-link";

        if (currentUrl.Contains("registrartramite.aspx"))
        {
            if (LnkRegistrarDocumento != null) LnkRegistrarDocumento.CssClass = "nav-link active";
        }
        else if (currentUrl.Contains("dashboard.aspx"))
        {
            if (LnkGestionarRoles != null) LnkGestionarRoles.CssClass = "nav-link active";
        }
        else if (activeMenu != null)
        {
            switch (activeMenu)
            {
                case "Historial": if (LnkHistorial != null) LnkHistorial.CssClass = "nav-link active"; break;
                case "EnRevision": if (LnkEnRevision != null) LnkEnRevision.CssClass = "nav-link active"; break;
                case "EnFirma": if (LnkEnFirma != null) LnkEnFirma.CssClass = "nav-link active"; break;
                case "PendientesRev": if (LnkPendientesRev != null) LnkPendientesRev.CssClass = "nav-link active"; break;
                case "PendientesFirma": if (LnkPendientesFirma != null) LnkPendientesFirma.CssClass = "nav-link active"; break;
                case "Completados": if (LnkCompletados != null) LnkCompletados.CssClass = "nav-link active"; break;
                case "TodosTramites": if (LnkTodosTramites != null) LnkTodosTramites.CssClass = "nav-link active"; break;
            }
        }
        else
        {
            string rol = Session["strRol"] != null ? Session["strRol"].ToString() : "";
            if (rol == "REGISTRADOR" && LnkHistorial != null) LnkHistorial.CssClass = "nav-link active";
            else if (rol == "FIRMADOR" && LnkPendientesRev != null) LnkPendientesRev.CssClass = "nav-link active";
            else if (rol == "ADMIN" && LnkTodosTramites != null) LnkTodosTramites.CssClass = "nav-link active";
        }
    }

    protected void LnkMenu_Click(object sender, EventArgs e)
    {
        System.Web.UI.WebControls.LinkButton btn = (System.Web.UI.WebControls.LinkButton)sender;
        string action = btn.CommandArgument;
        Session["ActiveMenu"] = action;

        if (action == "RegistrarDocumento")
        {
            Response.Redirect("~/Tramites/RegistrarTramite.aspx");
        }
        else if (action == "GestionarRoles")
        {
            Response.Redirect("~/Admin/Dashboard.aspx");
        }
        else
        {
            Response.Redirect("~/Tramites/Bandeja.aspx?action=" + action);
        }
    }

    protected void LnkCerrarSesion_Click(object sender, EventArgs e)
    {
        Session.Abandon();
        Response.Redirect("~/Login.aspx");
    }

    private string BuildInitials(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return "";
        }

        string[] parts = nombre.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string initials = string.Concat(parts.Take(2).Select(p => p[0].ToString().ToUpperInvariant()));
        return initials;
    }
}