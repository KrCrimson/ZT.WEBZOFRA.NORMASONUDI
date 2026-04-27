using System;
public partial class Bandeja : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string rol = Session["strRol"] != null ? Session["strRol"].ToString() : "";
        BtnNuevoTramite.Visible = (rol == "REGISTRADOR" || rol == "ADMIN");

        if (Session["strUsuario"] == null)
            Response.Redirect("~/Login.aspx");
        if (LblBienvenida != null)
            LblBienvenida.Text = "Bienvenido, " + Session["strNombre"].ToString();

    }
    protected void BtnNuevoTramite_Click(object sender, EventArgs e)
    {
        Response.Redirect("~/Tramites/RegistrarTramite.aspx");
    }
}