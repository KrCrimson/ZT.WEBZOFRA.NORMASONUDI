using System;
public partial class Dashboard : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["strUsuario"] == null)
            Response.Redirect("~/Login.aspx");
        if (Session["strRol"] == null || Session["strRol"].ToString() != "ADMIN")
            Response.Redirect("~/Login.aspx");
    }
}