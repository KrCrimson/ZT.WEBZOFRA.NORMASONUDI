using System;
using System.Configuration;
using System.Data.SqlClient;

namespace ZT.WEBZOFRA.FIRMADOR.DAL
{
    public static class ConexionDB
    {
        public static SqlConnection GetFirmador()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString;
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al conectar con la base de datos Firmador.", ex);
            }
        }

        public static SqlConnection GetAdministracion()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["Administracion"].ConnectionString;
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al conectar con la base de datos Administracion.", ex);
            }
        }

        public static SqlConnection GetFirmadorArchivos()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["FirmadorArchivos"].ConnectionString;
                SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                return conn;
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al conectar con la base de datos FirmadorArchivos.", ex);
            }
        }
    }
}
