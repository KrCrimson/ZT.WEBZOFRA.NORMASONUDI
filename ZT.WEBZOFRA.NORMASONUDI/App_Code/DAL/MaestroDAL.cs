using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ZT.WEBZOFRA.FIRMADOR.DAL;
using ZT.WEBZOFRA.NORMASONUDI.Entities;

namespace ZT.WEBZOFRA.NORMASONUDI.DAL
{
    public class MaestroDAL
    {
        public static List<Maestro> ObtenerPorTipo(string tipo)
        {
            List<Maestro> lista = new List<Maestro>();
            SqlConnection conn = null;
            try
            {
                conn = ConexionDB.GetFirmador();
                using (SqlCommand cmd = new SqlCommand("FIR_S_MaestroPorTipo", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Tipo", tipo);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Maestro obj = new Maestro();
                            if (reader["IDMaestro"] != DBNull.Value) obj.IDMaestro = Convert.ToInt32(reader["IDMaestro"]);
                            if (reader["Codigo"] != DBNull.Value) obj.Codigo = reader["Codigo"].ToString();
                            if (reader["Descripcion"] != DBNull.Value) obj.Descripcion = reader["Descripcion"].ToString();
                            if (reader["Orden"] != DBNull.Value) obj.Orden = Convert.ToInt32(reader["Orden"]);

                            // It states "Mapea IDMaestro, Codigo, Descripcion, Orden al objeto Maestro"
                            // If Tipo is needed, we map it, if not, it's fine.

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
    }
}