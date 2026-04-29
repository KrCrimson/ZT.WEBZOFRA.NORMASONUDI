<%@ WebHandler Language="C#" Class="VerPDF" %>

using System;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

public class VerPDF : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        string idParam = context.Request.QueryString["id"];
        if (string.IsNullOrEmpty(idParam))
        {
            context.Response.StatusCode = 400;
            context.Response.Write("Parámetro id requerido.");
            return;
        }

        int idArchivo = 0;
        if (!int.TryParse(idParam, out idArchivo))
        {
            context.Response.StatusCode = 400;
            context.Response.Write("Parámetro id inválido.");
            return;
        }

        string connStr = ConfigurationManager.ConnectionStrings["FirmadorArchivos"].ConnectionString;

        try
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT Contenido, NombreOriginal FROM ARC_DocumentoArchivo WHERE IDArchivo = @IDArchivo";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IDArchivo", idArchivo);
                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
                    {
                        if (dr.Read())
                        {
                            byte[] contenido = (byte[])dr["Contenido"];
                            string nombreOriginal = dr["NombreOriginal"].ToString();

                            context.Response.ContentType = "application/pdf";
                            context.Response.AddHeader("Content-Disposition", "inline; filename=" + nombreOriginal);
                            context.Response.BinaryWrite(contenido);
                            context.Response.Flush();
                        }
                        else
                        {
                            context.Response.StatusCode = 404;
                            context.Response.Write("Archivo no encontrado.");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.Write("Error: " + ex.Message);
        }
    }

    public bool IsReusable
    {
        get { return false; }
    }
}
