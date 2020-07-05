using System;

namespace Yadl
{
    public class YadlMessage
    {
        /// <summary>
        /// Campo ID
        /// </summary>
        public int ID { get; set; }
        
        /// <summary>
        /// Campo ADDDT
        /// </summary>
        public DateTimeOffset ADDDT { get; set; }
        
        /// <summary>
        /// Campo NIVEL
        /// </summary>
        public int NIVEL { get; set; }
        
        /// <summary>
        /// Campo NIVEL_DESCRIPCION
        /// </summary>
        public string NIVEL_DESCRIPCION { get; set; }
        
        /// <summary>
        /// Campo PAQUETE
        /// </summary>
        public string PAQUETE { get; set; }
        
        /// <summary>
        /// Campo EP_ORIGEN
        /// </summary>
        public string EP_ORIGEN { get; set; }
        
        /// <summary>
        /// Campo EP_DESTINO
        /// </summary>
        public string EP_DESTINO { get; set; }
        
        /// <summary>
        /// Campo COD_RESP
        /// </summary>
        public string COD_RESP { get; set; }
        
        /// <summary>
        /// Campo DESCRIPCION
        /// </summary>
        public string DESCRIPCION { get; set; }
        
        /// <summary>
        /// Campo DATOS
        /// </summary>
        public string DATOS { get; set; }
        
        /// <summary>
        /// Campo TIPO_OBJ
        /// </summary>
        public string TIPO_OBJ { get; set; }
        
        /// <summary>
        /// Campo ID_OBJ
        /// </summary>
        public long? ID_OBJ { get; set; }
        
        /// <summary>
        /// Campo ID_OBJ_HASH
        /// </summary>
        public string ID_OBJ_HASH { get; set; }
        
        /// <summary>
        /// Campo SEC_CLIENT
        /// </summary>
        public string SEC_CLIENT { get; set; }
        
        /// <summary>
        /// Campo SEC_BANCO
        /// </summary>
        public string SEC_BANCO { get; set; }
        
        /// <summary>
        /// Campo ENDDT
        /// </summary>
        public DateTimeOffset? ENDDT { get; set; }
        
        /// <summary>
        /// Campo DIFFT
        /// </summary>
        public int? DIFFT { get; set; }

        /// <summary>
        /// Campo INFO_ADICIONAL
        /// </summary>
        public string INFO_ADICIONAL { get; set; }
    }
}