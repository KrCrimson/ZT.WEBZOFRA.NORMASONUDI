using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace ZT.WEBZOFRA.NORMASONUDI
{
    public class CorreoBLL
    {
        private static string ConexionFirmador
        {
            get { return ConfigurationManager.ConnectionStrings["Firmador"].ConnectionString; }
        }

        private static void EnviarCorreoInterno(string para, string cco, string asunto, string mensaje, string adjunto)
        {
            if (string.IsNullOrWhiteSpace(para))
            {
                return;
            }

            using (SqlConnection conn = new SqlConnection(ConexionFirmador))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("GEN_X_EnviarMail", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Para", para.Trim());
                    cmd.Parameters.AddWithValue("@CCO", string.IsNullOrWhiteSpace(cco) ? (object)DBNull.Value : cco.Trim());
                    cmd.Parameters.AddWithValue("@Asunto", string.IsNullOrWhiteSpace(asunto) ? string.Empty : asunto.Trim());
                    cmd.Parameters.AddWithValue("@Mensaje", string.IsNullOrWhiteSpace(mensaje) ? string.Empty : mensaje);
                    cmd.Parameters.AddWithValue("@Adjunto", string.IsNullOrWhiteSpace(adjunto) ? (object)DBNull.Value : adjunto.Trim());
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static List<string> NormalizarCorreos(IEnumerable<string> correos)
        {
            return correos == null
                ? new List<string>()
                : correos.Where(c => !string.IsNullOrWhiteSpace(c))
                         .Select(c => c.Trim())
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .ToList();
        }

        private static string PlantillaCorreo(string contenido)
        {
            return @"
<!DOCTYPE html>
<html>
<body style='margin:0;padding:0;font-family:Arial,sans-serif;background:#f5f5f5;'>
  <table width='100%' cellpadding='0' cellspacing='0'>
    <tr>
      <td align='center' style='padding:20px;'>
        <table width='600' cellpadding='0' cellspacing='0' style='background:#fff;border-radius:8px;overflow:hidden;'>
          <tr>
            <td style='background:#1a5c38;padding:24px;text-align:center;'>
              <h1 style='color:#fff;margin:0;font-size:18px;letter-spacing:2px;'>
                SISTEMA DE FIRMA ZOFRATACNA
              </h1>
            </td>
          </tr>
          <tr>
            <td style='padding:32px;color:#333;'>
              <p>Estimado(a),</p>
              " + contenido + @"
              <br/>
              <p>Atentamente,</p>
              <p><b>Oficina de Tecnologias de la Informacion</b><br/>
              ZOFRATACNA</p>
            </td>
          </tr>
          <tr>
            <td style='background:#222;padding:16px;text-align:center;'>
              <p style='color:#aaa;margin:0;font-size:12px;'>
                Este es un correo automatico del Sistema Firmador ZOFRATACNA.
                Por favor no responda este mensaje.
              </p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
        }

        private static void EnviarCorreos(IEnumerable<string> destinatarios, string asunto, string contenido)
        {
            foreach (string correo in NormalizarCorreos(destinatarios))
            {
                EnviarCorreoInterno(correo, string.Empty, asunto, PlantillaCorreo(contenido), null);
            }
        }

        public static void NotificarDocumentoCreado(List<string> correosRevisores, string codigoDoc, string asunto, string loginRegistrador)
        {
            string contenido = "<p>Se ha registrado un nuevo documento para su revision:</p>" +
                               "<div style='background:#f9f9f9;border-left:4px solid #1a5c38;padding:16px;margin:16px 0;'>" +
                               "  <b>Codigo:</b> " + codigoDoc + "<br/>" +
                               "  <b>Asunto:</b> " + asunto + "<br/>" +
                               "  <b>Registrador:</b> " + loginRegistrador +
                               "</div>" +
                               "<p>El documento ya se encuentra disponible en el sistema.</p>";

            EnviarCorreos(correosRevisores, "Nuevo documento para revision - " + codigoDoc, contenido);
        }

        public static void NotificarInicioRevision(List<string> correosRevisores, string codigoDoc, string asunto)
        {
            string contenido = "<p>El documento ha ingresado a la etapa de revision:</p>" +
                               "<div style='background:#f9f9f9;border-left:4px solid #1a5c38;padding:16px;margin:16px 0;'>" +
                               "  <b>Codigo:</b> " + codigoDoc + "<br/>" +
                               "  <b>Asunto:</b> " + asunto +
                               "</div>" +
                               "<p>Por favor ingrese al sistema para iniciar su revision.</p>";

            EnviarCorreos(correosRevisores, "Inicio de revision - " + codigoDoc, contenido);
        }

        public static void NotificarObservacion(string correoRegistrador, string codigoDoc, string asunto, string nombreRevisor)
        {
            if (string.IsNullOrWhiteSpace(correoRegistrador))
            {
                return;
            }

            string contenido = "<p>Se ha registrado una observacion en su documento:</p>" +
                               "<div style='background:#f9f9f9;border-left:4px solid #dc3545;padding:16px;margin:16px 0;'>" +
                               "  <b>Codigo:</b> " + codigoDoc + "<br/>" +
                               "  <b>Asunto:</b> " + asunto + "<br/>" +
                               "  <b>Revisor:</b> " + nombreRevisor +
                               "</div>" +
                               "<p>Ingrese al sistema para subir la version corregida.</p>";

            EnviarCorreoInterno(correoRegistrador, string.Empty, "Observacion registrada - " + codigoDoc, PlantillaCorreo(contenido), null);
        }

        public static void NotificarCorreccionReinicio(List<string> correosRevisores, string codigoDoc, string asunto)
        {
            string contenido = "<p>Se ha cargado una version corregida del documento y la revision fue reiniciada:</p>" +
                               "<div style='background:#f9f9f9;border-left:4px solid #1a5c38;padding:16px;margin:16px 0;'>" +
                               "  <b>Codigo:</b> " + codigoDoc + "<br/>" +
                               "  <b>Asunto:</b> " + asunto +
                               "</div>" +
                               "<p>Por favor retome la revision desde el sistema.</p>";

            EnviarCorreos(correosRevisores, "Revision reiniciada - " + codigoDoc, contenido);
        }

        public static void NotificarTurnoFirma(string correoFirmante, string nombreFirmante, string codigoDoc, string asunto, int orden)
        {
            if (string.IsNullOrWhiteSpace(correoFirmante))
            {
                return;
            }

            string contenido = "<p>El documento se encuentra listo para su firma:</p>" +
                               "<div style='background:#f9f9f9;border-left:4px solid #1a5c38;padding:16px;margin:16px 0;'>" +
                               "  <b>Codigo:</b> " + codigoDoc + "<br/>" +
                               "  <b>Asunto:</b> " + asunto + "<br/>" +
                               "  <b>Firmante:</b> " + nombreFirmante + "<br/>" +
                               "  <b>Orden:</b> " + orden +
                               "</div>" +
                               "<p>Ingrese al sistema para continuar con la firma digital.</p>";

            EnviarCorreoInterno(correoFirmante, string.Empty, "Turno de firma - " + codigoDoc, PlantillaCorreo(contenido), null);
        }

        public static void NotificarProcesoCompleto(List<string> todosCorreos, string codigoDoc, string asunto)
        {
            string contenido = "<p>El proceso de revision y firma ha concluido correctamente:</p>" +
                               "<div style='background:#f9f9f9;border-left:4px solid #1a5c38;padding:16px;margin:16px 0;'>" +
                               "  <b>Codigo:</b> " + codigoDoc + "<br/>" +
                               "  <b>Asunto:</b> " + asunto +
                               "</div>" +
                               "<p>El documento final ya se encuentra completado en el sistema.</p>";

            EnviarCorreos(todosCorreos, "Proceso completado - " + codigoDoc, contenido);
        }
    }
}
