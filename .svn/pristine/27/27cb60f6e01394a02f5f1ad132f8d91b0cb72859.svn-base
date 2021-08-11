using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuvaVazaoTools {
    public class Temperatura {
        Temperatura() {
            Previsoes = new Dictionary<string, TemperaturaCidade>();
        }

        public static void Show(params string[] arquivosTemperatura) {
            var list = new List<Temperatura>();

            foreach (var file in arquivosTemperatura) {
                list.Add(ReadFile(file));
            }
            TempViewer vwr = new TempViewer(list.ToArray());

            vwr.Show();

        }

        internal static void ShowCompara(string filePrevd, string filePrevdAnt) {
            var list = new List<Temperatura>();

            var td = ReadFile(filePrevd);
            var tant = ReadFile(filePrevdAnt);

            var dtd = td.Previsoes.First().Value.Previsao.Min(x => x.Key);
            var dtant = tant.Previsoes.First().Value.Previsao.Min(x => x.Key);

            td.Arquivo = @"0\" + dtd.ToString("yyyy-MM-dd");

            var tant2 = new Temperatura();
            tant2.Arquivo = @"1\" + dtant.ToString("yyyy-MM-dd");


            tant.Previsoes.ToList().ForEach(
                x => {
                    tant2.Previsoes[x.Key] = new TemperaturaCidade();


                    tant2.Previsoes[x.Key].Cidade = x.Key;

                    x.Value.Previsao.ToList().ForEach(y => {
                        tant2.Previsoes[x.Key].Previsao[y.Key.AddDays(7)] = y.Value;

                    });
                }
                );

            list.Add(td); list.Add(tant2);

            TempViewer vwr = new TempViewer(list.ToArray());

            vwr.Show();
        }

        public static Temperatura ReadFile(string prev) {
            Temperatura tempReading = new Temperatura();

            tempReading.Arquivo = prev;


            using (var sr = System.IO.File.OpenText(prev)) {
                TemperaturaCidade temp = null;
                do {
                    var line = sr.ReadLine();

                    DateTime date;
                    if (!DateTime.TryParseExact(line.Substring(0, 13)
                        , "yyyyMMdd/HHmm"
                        , System.Globalization.CultureInfo.InvariantCulture
                        , System.Globalization.DateTimeStyles.AssumeLocal
                        , out date)) {
                        if (temp != null) { tempReading.Previsoes[temp.Cidade] = temp; temp = null; }
                        temp = new TemperaturaCidade() { Cidade = line.Trim() };
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

        public string Arquivo { get; set; }
        public Dictionary<string, TemperaturaCidade> Previsoes { get; set; }

        public void Resumo() {

            //Previsoes.Values
            //    .Select(x => {

            //        var pr = x.Previsao.Select(y => new { y.Value, y.Key.Date });

            //        return new {
            //            Cidade = x.Cidade,
            //            Min = pr.GroupBy(y=>y.Date).SelectMany(z=>z.Min()).ToArray(),
            //            Max = new float[] { },
            //            Med = new float[] { },
            //            MedRed = new float[] { },
            //        };
            //    });
                       
        }
    }

    public class TemperaturaCidade {
        internal TemperaturaCidade() { Previsao = new Dictionary<DateTime, float>(); }
        public string Cidade { get; set; }
        public Dictionary<DateTime, float> Previsao { get; set; }

    }

}

