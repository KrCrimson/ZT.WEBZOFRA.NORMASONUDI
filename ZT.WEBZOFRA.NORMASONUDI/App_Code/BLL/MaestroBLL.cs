using System;
using System.Collections.Generic;
using ZT.WEBZOFRA.NORMASONUDI;

namespace ZT.WEBZOFRA.NORMASONUDI
{
    public class MaestroBLL
    {
        public static List<Maestro> ObtenerPorTipo(string tipo)
        {
            if (string.IsNullOrEmpty(tipo))
            {
                throw new ArgumentException("El tipo no puede ser nulo o vacío.", "tipo");
            }

            return MaestroDAL.ObtenerPorTipo(tipo);
        }
    }
}