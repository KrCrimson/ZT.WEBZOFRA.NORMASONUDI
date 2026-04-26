using System;
using System.Collections.Generic;
using ZT.WEBZOFRA.NORMASONUDI.DAL;
using ZT.WEBZOFRA.NORMASONUDI.Entities;

namespace ZT.WEBZOFRA.NORMASONUDI.BLL
{
    public class MaestroBLL
    {
        public static List<Maestro> ObtenerPorTipo(string tipo)
        {
            if (string.IsNullOrEmpty(tipo))
            {
                throw new ArgumentException("El tipo no puede ser nulo o vacío.", nameof(tipo));
            }

            return MaestroDAL.ObtenerPorTipo(tipo);
        }
    }
}