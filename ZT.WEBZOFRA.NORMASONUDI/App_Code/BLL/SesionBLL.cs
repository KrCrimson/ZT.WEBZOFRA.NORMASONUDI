using System;
using System.Collections.Generic;
using ZT.WEBZOFRA.NORMASONUDI;

namespace ZT.WEBZOFRA.NORMASONUDI
{
    public class SesionBLL
    {
        public static List<UsuarioSesion> ObtenerUsuariosActivos()
        {
            return SesionDAL.ObtenerUsuariosActivos();
        }

        public static UsuarioSesion ValidarAcceso(string loginUsuario)
        {
            if (string.IsNullOrEmpty(loginUsuario))
            {
                throw new Exception("El usuario no puede ser nulo o vacío.");
            }

            UsuarioSesion usuario = SesionDAL.ObtenerSesion(loginUsuario);
            if (usuario == null)
            {
                throw new Exception("Usuario sin acceso al sistema.");
            }

            return usuario;
        }
    }
}