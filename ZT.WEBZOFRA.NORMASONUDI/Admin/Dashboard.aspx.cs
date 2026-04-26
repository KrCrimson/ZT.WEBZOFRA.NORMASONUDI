using System;
using ZT.WEBZOFRA.NORMASONUDI.Helpers;

namespace ZT.WEBZOFRA.NORMASONUDI.Admin
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SesionHelper.EstaAutenticado(Session))
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (SesionHelper.ObtenerRol(Session) != "ADMIN")
            {
                Response.Redirect("~/Login.aspx");
                return;
            }
        }
    }
}