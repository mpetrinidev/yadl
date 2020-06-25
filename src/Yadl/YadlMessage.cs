using System;

namespace Yadl
{
    public class YadlMessage
    {
        /// <summary>
        /// Campo ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Campo ADDDT
        /// </summary>
        public DateTimeOffset AddDt { get; set; }
        
        /// <summary>
        /// Campo NIVEL
        /// </summary>
        public int Nivel { get; set; }
        
        /// <summary>
        /// Campo NIVEL_DESCRIPCION
        /// </summary>
        public string NivelDescripcion { get; set; }
        
        /// <summary>
        /// Campo PAQUETE
        /// </summary>
        public string Paquete { get; set; }
        
        /// <summary>
        /// Campo EP_ORIGEN
        /// </summary>
        public string IpOrigen { get; set; }
        
        /// <summary>
        /// Campo EP_DESTINO
        /// </summary>
        public string IpDestino { get; set; }
        
        /// <summary>
        /// Campo COD_RESP
        /// </summary>
        public string CodRespuesta { get; set; }
        
        /// <summary>
        /// Campo DESCRIPCION
        /// </summary>
        public string Descripcion { get; set; }
        
        /// <summary>
        /// Campo DATOS
        /// </summary>
        public string Datos { get; set; }
        
        /// <summary>
        /// Campo TIPO_OBJ
        /// </summary>
        public string TipoObj { get; set; }
        
        /// <summary>
        /// Campo ID_OBJ
        /// </summary>
        public long? IdObj { get; set; }
        
        /// <summary>
        /// Campo ID_OBJ_HASH
        /// </summary>
        public string IdObjHash { get; set; }
        
        /// <summary>
        /// Campo SEC_CLIENT
        /// </summary>
        public string SecClient { get; set; }
        
        /// <summary>
        /// Campo SEC_BANCO
        /// </summary>
        public string SecBanco { get; set; }
        
        /// <summary>
        /// Campo ENDDT
        /// </summary>
        public DateTimeOffset? EndDt { get; set; }
        
        /// <summary>
        /// Campo DIFFT
        /// </summary>
        public int? Difft { get; set; }
    }
}