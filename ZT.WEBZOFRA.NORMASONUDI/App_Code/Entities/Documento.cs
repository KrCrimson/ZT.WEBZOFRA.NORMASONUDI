using System;

namespace ZT.WEBZOFRA.NORMASONUDI
{
    public class Documento
    {
        public int IDDocumento { get; set; }
        public string Asunto { get; set; }
        public string CodigoTipoDocumento { get; set; }
        public string TipoDocumento { get; set; }
        public string AreaResponsable { get; set; }
        public DateTime FechaDocumento { get; set; }
        public string CodigoDocumento { get; set; }
        public string RutaArchivoPDF { get; set; }
        public int IDArchivoPDF { get; set; }
        public string CodigoEstado { get; set; }
        public string Estado { get; set; }
        public int Version { get; set; }
        public string Orientacion { get; set; }
        public string LoginRegistrador { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaUltimaAccion { get; set; }
    }
}