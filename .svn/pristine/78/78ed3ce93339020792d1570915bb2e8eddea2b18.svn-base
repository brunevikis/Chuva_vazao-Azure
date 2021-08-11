using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Temperatura {
    static class Program {

        static string caminhoBase = @"H:\Middle - Preço\Acompanhamento de Temperatura\Previsao_Numerica";
        static DateTime dt = new DateTime(2018, 03, 01);
        
        [STAThread]
        static void Main(string[] args) {

            var caminho = System.IO.Path.Combine(caminhoBase, dt.ToString("yyyyMM"), dt.ToString("dd"));
            var prevAtual = System.IO.Path.Combine(caminho, "dia.txt");
            var prevComp = System.IO.Path.Combine(caminho, "dia_4.txt");

            var atual = ReadFile(prevAtual);
            var anterior = ReadFile(prevComp);

            //"SAO_PAULO, SP, BR"
            
            graf frm = new graf(atual.Where(x => x.Cidade == "SAO_PAULO, SP, BR").First().Previsao,
                anterior.Where(x => x.Cidade == "SAO_PAULO, SP, BR").First().Previsao);

            frm.ShowDialog();                

        }

        private static List<Temp> ReadFile(string prev) {
            List<Temp> tempReading = new List<Temp>();

            using (var sr = System.IO.File.OpenText(prev)) {
                Temp temp = null;
                do {
                    var line = sr.ReadLine();

                    DateTime date;
                    if (!DateTime.TryParseExact(line.Substring(0, 13)
                        , "yyyyMMdd/HHmm"
                        , System.Globalization.CultureInfo.InvariantCulture
                        , System.Globalization.DateTimeStyles.AssumeLocal
                        , out date)) {
                        if (temp != null) { tempReading.Add(temp); temp = null; }
                        temp = new Temp() { Cidade = line.Trim() };
                        sr.ReadLine();
                    } else if (temp != null) {

                        var t = float.Parse(line.Substring(16, 4)
                            , System.Globalization.CultureInfo.InvariantCulture);
                        temp.Previsao[date] = t;
                    }

                } while (!sr.EndOfStream);
            }

            return tempReading;
        }





        public class Temp {

            public string Cidade { get; set; }
            public Dictionary<DateTime, float> Previsao { get; set; }

            public Temp() {
                Previsao = new Dictionary<DateTime, float>();
            }
        }
    }
}
