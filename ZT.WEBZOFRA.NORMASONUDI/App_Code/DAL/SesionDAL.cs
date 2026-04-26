using System;
using System.Collections.Generic;
using System.Data;
using ZT.WEBZOFRA.NORMASONUDI;
using System.Data.SqlClient;

namespace ZT.WEBZOFRA.NORMASONUDI
{
    public class SesionDAL
    {
        public static List<UsuarioSesion> ObtenerUsuariosActivos()
        {
            List<UsuarioSesion> lista = new List<UsuarioSesion>();
            SqlConnection conn = null;
            try
            {
                conn = ConexionDB.GetFirmador();
                using (SqlCommand cmd = new SqlCommand("SELECT LoginUsuario, NombreCompleto, Email, CodigoRol, UrlDashboard FROM FIR_VW_UsuarioSesionPrueba ORDER BY NombreCompleto", conn))
                {
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            UsuarioSesion obj = new UsuarioSesion();
                            if (reader["LoginUsuario"] != DBNull.Value) obj.LoginUsuario = reader["LoginUsuario"].ToString();
                            if (reader["NombreCompleto"] != DBNull.Value) obj.NombreCompleto = reader["NombreCompleto"].ToString();
                            if (reader["Email"] != DBNull.Value) obj.Email = reader["Email"].ToString();
                            if (reader["CodigoRol"] != DBNull.Value) obj.CodigoRol = reader["CodigoRol"].ToString();
                            if (reader["UrlDashboard"] != DBNull.Value) obj.UrlDashboard = reader["UrlDashboard"].ToString();
                            lista.Add(obj);
                        }
                    }
                }
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            return lista;
        }

        public static UsuarioSesion ObtenerSesion(string loginUsuario)
        {
            UsuarioSesion usuario = null;
            SqlConnection conn = null;
            try
            {
                conn = ConexionDB.GetFirmador();
                using (SqlCommand cmd = new SqlCommand("FIR_S_ObtenerSesion", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@LoginUsuario", loginUsuario);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = new UsuarioSesion();
                            if (reader["LoginUsuario"] != DBNull.Value) usuario.LoginUsuario = reader["LoginUsuario"].ToString();
                            if (reader["NombreCompleto"] != DBNull.Value) usuario.NombreCompleto = reader["NombreCompleto"].ToString();
                            if (reader["Email"] != DBNull.Value) usuario.Email = reader["Email"].ToString();
                            if (reader["CodigoRol"] != DBNull.Value) usuario.CodigoRol = reader["CodigoRol"].ToString();
                            if (reader["DescripcionRol"] != DBNull.Value) usuario.DescripcionRol = reader["DescripcionRol"].ToString();
                            if (reader["UrlDashboard"] != DBNull.Value) usuario.UrlDashboard = reader["UrlDashboard"].ToString();
                        }
                    }
                }
            }
            finally
            {
                if (conn != null && conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            return usuario;
        }
    }
}