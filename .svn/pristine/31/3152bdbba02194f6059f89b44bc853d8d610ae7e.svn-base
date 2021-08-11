using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuvaVazaoTools
{
    public static class Config
    {
        public static string ScriptGrads { get; internal set; }
        public static string CaminhoAuxiliar { get; internal set; }
        public static string CaminhoInicialEntrada { get; internal set; }
        public static string CaminhoTemperatura { get; internal set; }
        public static string ConfigConjunto { get; internal set; }
        public static string PostosFlu { get; internal set; }
        
        public static string Postos_Vazaoes { get; internal set; }
        public static string CaminhoPrevisao { get; internal set; }
        public static string CaminhoMerge { get; internal set; }
        public static string CaminhoFunceme { get; internal set; }

        
        public static string CaminhoModelo { get; internal set; }


        public static string SmapApp { get; internal set; }
        public static string ConfigMapa { get; internal set; }
 
        public static string ConfigMapaGEFS { get; internal set; }

        public static string ConfigPontosBase { get; internal set; }
        public static string ConfigPostosPlu { get; internal set; }
        public static string XltmResultado { get; internal set; }
        public static string IniVazao { get; internal set; }
        public static string HistoricoVazao { get; internal set; }
        public static string FonteVazao { get; internal set; }
        public static string CaminhoLogAutoRun { get; internal set; }


        public static void Read()
        {
            SmapApp = System.Configuration.ConfigurationManager.AppSettings["smapApp"];
            CaminhoInicialEntrada = System.Configuration.ConfigurationManager.AppSettings["caminhoInicialEntrada"];
            CaminhoTemperatura = System.Configuration.ConfigurationManager.AppSettings["caminhoTemperatura"];
            ConfigConjunto = System.Configuration.ConfigurationManager.AppSettings["configConjunto"];
            PostosFlu = System.Configuration.ConfigurationManager.AppSettings["configPostosFlu"];
            Postos_Vazaoes = System.Configuration.ConfigurationManager.AppSettings["configPostos_Vazaoes"];
            CaminhoPrevisao = System.Configuration.ConfigurationManager.AppSettings["caminhoPrevisao"];
            CaminhoMerge = System.Configuration.ConfigurationManager.AppSettings["caminhoMerge"];
            ConfigMapa = System.Configuration.ConfigurationManager.AppSettings["configMapa"];
            ConfigPostosPlu = System.Configuration.ConfigurationManager.AppSettings["configPostosPlu"];

            CaminhoModelo = System.Configuration.ConfigurationManager.AppSettings["caminhoModelo"];
            ConfigMapaGEFS = System.Configuration.ConfigurationManager.AppSettings["configMapaGEFS"];
            ConfigPontosBase = System.Configuration.ConfigurationManager.AppSettings["configPontosBase"];

            IniVazao = System.Configuration.ConfigurationManager.AppSettings["iniVazao"];
            XltmResultado = System.Configuration.ConfigurationManager.AppSettings["xltmResultado"];
            HistoricoVazao = System.Configuration.ConfigurationManager.AppSettings["historicoVazao"];
            FonteVazao = System.Configuration.ConfigurationManager.AppSettings["fonteVazao"];

            ScriptGrads = System.Configuration.ConfigurationManager.AppSettings["gradsScript"];
            CaminhoAuxiliar = System.Configuration.ConfigurationManager.AppSettings["caminhoAuxiliar"];

            CaminhoLogAutoRun = System.Configuration.ConfigurationManager.AppSettings["caminhoLogAutoRun"];

            CaminhoFunceme = System.Configuration.ConfigurationManager.AppSettings["caminhoFunceme"];

        }
    }
}
