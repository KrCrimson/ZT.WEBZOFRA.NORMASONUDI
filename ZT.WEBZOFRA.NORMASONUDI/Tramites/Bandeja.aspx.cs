using System;
public partial class Bandeja : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["strUsuario"] == null)
            Response.Redirect("~/Login.aspx");
        if (LblBienvenida != null)
            LblBienvenida.Text = "Bienvenido, " + Session["strNombre"].ToString();
    }
}