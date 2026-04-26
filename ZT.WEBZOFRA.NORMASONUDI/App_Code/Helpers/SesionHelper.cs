using System.Web.SessionState;
using System.Web;
using System.Web.SessionState;
using ZT.WEBZOFRA.NORMASONUDI.Entities;

namespace ZT.WEBZOFRA.NORMASONUDI.Helpers
{
    public static class SesionHelper
    {
        private const string STR_USUARIO = "strUsuario";
        private const string STR_ROL = "strRol";
        private const string STR_NOMBRE = "strNombre";
        private const string STR_EMAIL = "strEmail";

        public static void Guardar(UsuarioSesion u, HttpSessionState session)
        {
            session[STR_USUARIO] = u.LoginUsuario;
            session[STR_ROL] = u.CodigoRol;
            session[STR_NOMBRE] = u.NombreCompleto;
            session[STR_EMAIL] = u.Email;
        }

        public static string ObtenerLogin(HttpSessionState session)
        {
            return session[STR_USUARIO] as string;
        }

        public static string ObtenerRol(HttpSessionState session)
        {
            return session[STR_ROL] as string;
        }

        public static string ObtenerNombre(HttpSessionState session)
        {
            return session[STR_NOMBRE] as string;
        }

        public static bool EstaAutenticado(HttpSessionState session)
        {
            string usuario = ObtenerLogin(session);
            return !string.IsNullOrEmpty(usuario);
        }

        public static void Cerrar(HttpSessionState session)
        {
            session.Clear();
        }

        public static void RedirigirSegunRol(UsuarioSesion u, HttpResponse response)
        {
            response.Redirect(u.UrlDashboard);
        }
    }
}