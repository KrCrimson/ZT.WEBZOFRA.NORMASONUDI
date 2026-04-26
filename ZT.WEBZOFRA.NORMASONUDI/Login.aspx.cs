using System;
using ZT.WEBZOFRA.NORMASONUDI.BLL;
using ZT.WEBZOFRA.NORMASONUDI.Entities;
using ZT.WEBZOFRA.NORMASONUDI.Helpers;
using System.Web.UI.WebControls;

namespace ZT.WEBZOFRA.NORMASONUDI
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarUsuariosActivos();
            }
        }

        private void CargarUsuariosActivos()
        {
            try
            {
                var usuarios = SesionBLL.ObtenerUsuariosActivos();
                CbxUsuario.DataSource = usuarios;
                CbxUsuario.DataTextField = "NombreCompleto";
                CbxUsuario.DataValueField = "LoginUsuario";
                CbxUsuario.DataBind();

                CbxUsuario.Items.Insert(0, new ListItem("-- Seleccione su usuario --", ""));
            }
            catch (Exception ex)
            {
                LblError.Text = "Error al cargar usuarios: " + ex.Message;
                LblError.Visible = true;
            }
        }

        protected void BtnIngresar_Click(object sender, EventArgs e)
        {
            string strLogin = CbxUsuario.SelectedValue;

            if (string.IsNullOrEmpty(strLogin))
            {
                LblError.Text = "Seleccione un usuario.";
                LblError.Visible = true;
                return;
            }

            try
            {
                UsuarioSesion usuario = SesionBLL.ValidarAcceso(strLogin);
                SesionHelper.Guardar(usuario, Session);
                SesionHelper.RedirigirSegunRol(usuario, Response);
            }
            catch (Exception ex)
            {
                LblError.Text = ex.Message;
                LblError.Visible = true;
            }
        }
    }
}