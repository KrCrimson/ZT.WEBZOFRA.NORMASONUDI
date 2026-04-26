using System;
using ZT.WEBZOFRA.NORMASONUDI.Helpers;

namespace ZT.WEBZOFRA.NORMASONUDI.Tramites
{
    public partial class Bandeja : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SesionHelper.EstaAutenticado(Session))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LblBienvenida.Text = "Bienvenido, " + SesionHelper.ObtenerNombre(Session);
            }
        }
    }
}