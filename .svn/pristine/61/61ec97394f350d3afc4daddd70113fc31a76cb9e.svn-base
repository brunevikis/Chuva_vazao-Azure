using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChuvaVazaoTools.Mpcv {
    public class ModeloMpcv : ModeloChuvaVazao {
        //"Semana_Estimada_Uruguai.prn"        

        Semana_Estimada_Uruguai semobservada = null;

        internal static string[] arqVaz = new string[] {             
            "Campos_Novos-Uruguai.txt",
            "Barra_Grande-Uruguai.txt",
            "Ita-Uruguai.txt",
            
        };

        //92	ITA	ITA
        //215	BG	BARRA GRANDE
        //216	CN	CAMPOS NOVOS

        public override DateTime DataPrevisao {
            get;
            set;
        }

        public override void SalvarPrecObeservada() {
            foreach (var postoPlu in this.PostosPlu) {
                var c = System.IO.Path.Combine(ArquivosDeEntrada, postoPlu.Codigo + "_c.txt");
                postoPlu.Salvar(c);
            }
        }

        public override void SalvarPrecPrevista(Dictionary<DateTime, Precipitacao> previsaoChuva) {

            var dataFimSemanaPrevisao = this.DataPrevisao.AddDays(5 - (int)this.DataPrevisao.DayOfWeek);

            foreach (var prec in previsaoChuva.Where(x => x.Key > DataPrevisao)) {

                var raiznome = "p" + DataPrevisao.ToString("ddMMyy") + "a" + prec.Key.ToString("ddMMyy") + ".dat";

                prec.Value.SalvarModeloEta(
                    System.IO.Path.Combine(ArquivosETA, raiznome), -29, -27, -53, -49
                    );

                if (prec.Key > this.DataPrevisao && prec.Key <= dataFimSemanaPrevisao) {
                    foreach (var postoPlu in this.PostosPlu) {
                        postoPlu.Preciptacao[prec.Key] = prec.Value[postoPlu.Codigo];
                    }
                }
            }
        }

        public override void SalvarParametros() {

            var dataFimSemanaPrevisao = this.DataPrevisao.AddDays(5 - (int)this.DataPrevisao.DayOfWeek);
            var dataIncioSemanaPrevisao = dataFimSemanaPrevisao.AddDays(-6);

            semobservada.DataInicioSemanaEstimada = dataIncioSemanaPrevisao;

            return;
            throw new NotImplementedException();
        }

        public override void Executar() {
            System.Windows.Forms.MessageBox.Show("Aguardando Execução do MPCV");
            return;
        }

        public override void ColetarSaida() {

            var dataInicioCV = this.DataPrevisao.AddDays(5 - (int)this.DataPrevisao.DayOfWeek).AddDays(1);

            var fs = System.IO.Directory.GetFiles(Caminho, "RESUMO.XLSX")
                .Select(x => new System.IO.FileInfo(x))
                .OrderByDescending(x => x.CreationTime).FirstOrDefault();


            if (fs != null) using (var fstream = new System.IO.FileStream(fs.FullName, System.IO.FileMode.Open)) {

                    var zip = new System.IO.Compression.ZipArchive(fstream);
                    var entry = zip.GetEntry("xl/sharedStrings.xml");
                    using (var txtReader = new System.IO.StreamReader(entry.Open())) {


                        var resumoTxt = txtReader.ReadToEnd().Split(new string[] { "<t>" }, StringSplitOptions.RemoveEmptyEntries);


                        var rx1 = @"(\d+,?\d*)m";
                        var rgx = new System.Text.RegularExpressions.Regex(rx1, System.Text.RegularExpressions.RegexOptions.CultureInvariant);

                        foreach (var line in resumoTxt) {


                            var matchs = rgx.Matches(line);
                            if (matchs.Count > 0) {

                                var vaz = float.Parse(matchs[matchs.Count - 1].Groups[1].Value,
                                    System.Globalization.CultureInfo.GetCultureInfo("pt-BR").NumberFormat);


                                if (line.Contains("ITA")) {
                                    var v92 = Vazoes.First(x => System.IO.Path.GetFileName(x.CaminhoArquivo).Equals(arqVaz[2]));
                                    for (int i = 0; i < 7; i++) v92.Vazoes[dataInicioCV.AddDays(i)] = vaz;
                                } else if (line.Contains("BARRA GRANDE")) {
                                    var v215 = Vazoes.First(x => System.IO.Path.GetFileName(x.CaminhoArquivo).Equals(arqVaz[1]));
                                    for (int i = 0; i < 7; i++) v215.Vazoes[dataInicioCV.AddDays(i)] = vaz;
                                } else if (line.Contains("CAMPOS NOVOS")) {
                                    var v216 = Vazoes.First(x => System.IO.Path.GetFileName(x.CaminhoArquivo).Equals(arqVaz[0]));
                                    for (int i = 0; i < 7; i++) v216.Vazoes[dataInicioCV.AddDays(i)] = vaz;
                                }

                                /*
                        * "ITA"
                        * "BARRA GRANDE"
                        * "CAMPOS NOVOS"
                        * 
                        */
                            }
                        }
                    }
                }

            return;
        }

        public override void SalvarVazaoObservada() {

            var dataFimSemanaPrevisao = this.DataPrevisao.AddDays(5 - (int)this.DataPrevisao.DayOfWeek);
            var dataIncioSemanaPrevisao = dataFimSemanaPrevisao.AddDays(-6);

            var v216 = Vazoes.First(x => System.IO.Path.GetFileName(x.CaminhoArquivo).Equals(arqVaz[0]));
            var v215 = Vazoes.First(x => System.IO.Path.GetFileName(x.CaminhoArquivo).Equals(arqVaz[1]));
            var v92 = Vazoes.First(x => System.IO.Path.GetFileName(x.CaminhoArquivo).Equals(arqVaz[2]));

            for (DateTime dt = dataIncioSemanaPrevisao; dt <= dataFimSemanaPrevisao; dt = dt.AddDays(1)) {
                if (dt >= this.DataPrevisao)
                    v92.Vazoes[dt] = RegrideVazao(dt, v92.Vazoes);
            }
            for (DateTime dt = dataIncioSemanaPrevisao; dt <= dataFimSemanaPrevisao; dt = dt.AddDays(1)) {
                if (dt >= this.DataPrevisao)
                    v215.Vazoes[dt] = RegrideVazao(dt, v215.Vazoes);
            }
            for (DateTime dt = dataIncioSemanaPrevisao; dt <= dataFimSemanaPrevisao; dt = dt.AddDays(1)) {
                if (dt >= this.DataPrevisao)
                    v216.Vazoes[dt] = RegrideVazao(dt, v216.Vazoes);
            }

            foreach (var item in this.Vazoes) {
                item.SalvarVazoes();
            }

            semobservada.Gravar();
            //this.DataPrevisao = this.Vazoes.SelectMany(x => x.Vazoes.Keys).Max().AddDays(1);
        }

        private string ArquivosDeEntrada { get { return System.IO.Path.Combine(Caminho, "ARQ_ENTRADA"); } }
        private string ArquivosETA { get { return System.IO.Path.Combine(Caminho, "ETA"); } }

        private float RegrideVazao(DateTime data, Dictionary<DateTime, float> vazoes) {

            var min = vazoes.Values.Where(x => x > 0).Min();
            var max = vazoes.Values.Max();

            float chuva_1 = 0;
            for (int i = -4; i < -1; i++) {
                chuva_1 += this.PostosPlu.Select(x => x.Preciptacao)
                .Where(x => x[data.AddDays(i)].HasValue)
                .Select(x => x[data.AddDays(i)].Value).Average(x => x);
            }

            float chuva0 = 0;
            for (int i = -3; i < 0; i++) {
                chuva0 += this.PostosPlu.Select(x => x.Preciptacao)
                .Where(x => x[data.AddDays(i)].HasValue)
                .Select(x => x[data.AddDays(i)].Value).Average(x => x);
            }

            float chuva1 = 0;
            for (int i = -2; i < 1; i++) {
                chuva1 += this.PostosPlu.Select(x => x.Preciptacao)
                .Where(x => x[data.AddDays(i)].HasValue)
                .Select(x => x[data.AddDays(i)].Value).Average(x => x);
            }


            float q0 = vazoes[data.AddDays(-1)];
            float q_1 = vazoes[data.AddDays(-2)];
            float q1 = q0;

            //aumento no volume de chuvas
            if ((chuva1 <= chuva0 && q0 >= q_1) || (chuva1 >= chuva0 && q0 <= q_1)) {
                q1 = q0;
            } else if (chuva0 > chuva_1 && chuva1 > chuva0 && q0 >= q_1) {
                q1 = q0 + Math.Abs(q0 - q_1);
            } else if (chuva0 < chuva_1 && chuva1 < chuva0 && q0 <= q_1) {
                q1 = q0 - Math.Abs(q0 - q_1);
            } else q1 = q0 + (q0 - q_1);

            Console.WriteLine(string.Join("\t",
                "mpcv - esti",
                data.ToShortDateString(),
                q_1.ToString(),
                q0.ToString(),
                q1.ToString(),
                chuva_1.ToString(),
                chuva0.ToString(),
                chuva1.ToString()
                ));


            return Math.Min(Math.Max(q1, min), max);
            //return vazoes.Where(x => x.Key < data && x.Key >= dataIncioSemanaPrevisao.AddDays(-2)).Average(x => x.Value);
        }

        public ModeloMpcv(string path) {
            Caminho = path;

            var arqPostosPlu = System.IO.Directory.GetFiles(ArquivosDeEntrada, "*_c.txt");

            PostosPlu = arqPostosPlu.Select(x => {
                var p = new PostoPlu() {
                    Codigo =
                        System.IO.Path.GetFileNameWithoutExtension(x).ToUpperInvariant().Replace("_C", "")
                };
                p.Carregar(x);
                return p;
            }).ToList();

            this.Vazoes =
            arqVaz.Select(a => {
                var arq = System.IO.Directory.GetFiles(ArquivosDeEntrada, a)[0];
                var vaz = new VazoesRealizadas(arq);
                vaz.CarregarVazoes();
                return vaz;
            }).ToList();

            this.DataPrevisao = this.Vazoes.SelectMany(x => x.Vazoes.Keys).Max().AddDays(1);


            semobservada = new Semana_Estimada_Uruguai(this);

        }

        internal class Semana_Estimada_Uruguai {

            string arquivo = "";

            Dictionary<int, float> VazoesSemanaEstimada;

            public DateTime DataInicioSemanaEstimada { get; set; }

            public int[] ordPostos = new int[] {
            216,
            215,
            92            
            };

            public IArqVazao[] arqVazoes = null;

            private ModeloMpcv modeloMpcv;


            internal float this[int posto] {
                get { return VazoesSemanaEstimada[posto]; }
                set { VazoesSemanaEstimada[posto] = value; }
            }

            private Semana_Estimada_Uruguai() {
                VazoesSemanaEstimada = new Dictionary<int, float>();
            }

            internal Semana_Estimada_Uruguai(string arquivo)
                : this() {
                this.arquivo = arquivo;

                VazoesSemanaEstimada = System.IO.File.ReadAllLines(arquivo).Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => new {
                        posto = int.Parse(x.Substring(0, 8).Trim()),
                        vaz = float.Parse(x.Substring(9).Trim(), System.Globalization.NumberFormatInfo.InvariantInfo)
                    }).ToDictionary(x => x.posto, x => x.vaz)

                    ;
            }

            public Semana_Estimada_Uruguai(ModeloMpcv modeloMpcv)
                : this() {
                // TODO: Complete member initialization
                this.modeloMpcv = modeloMpcv;


                this.arquivo = System.IO.Directory.GetFiles(modeloMpcv.ArquivosDeEntrada, "Semana_Estimada_Uruguai.prn")[0];

                var arqLines = System.IO.File.ReadAllLines(this.arquivo).Where(x => !string.IsNullOrWhiteSpace(x));

                VazoesSemanaEstimada[216] = float.Parse(arqLines.Skip(1).Take(1).First().Substring(18, 5));
                VazoesSemanaEstimada[215] = float.Parse(arqLines.Skip(1).Take(1).First().Substring(38, 5));
                VazoesSemanaEstimada[92] = float.Parse(arqLines.Skip(1).Take(1).First().Substring(58, 5));


                var arq216 = modeloMpcv.Vazoes.First(x => System.IO.Path.GetFileName(x.CaminhoArquivo).Equals(ModeloMpcv.arqVaz[0], StringComparison.OrdinalIgnoreCase));
                var arq215 = modeloMpcv.Vazoes.First(x => System.IO.Path.GetFileName(x.CaminhoArquivo).Equals(ModeloMpcv.arqVaz[1], StringComparison.OrdinalIgnoreCase));
                var arq92 = modeloMpcv.Vazoes.First(x => System.IO.Path.GetFileName(x.CaminhoArquivo).Equals(ModeloMpcv.arqVaz[2], StringComparison.OrdinalIgnoreCase));


                arqVazoes = new IArqVazao[] {
                    arq216,
                    arq215,
                    arq92 ,
                };

                var primeira = true;
                foreach (var line in arqLines.Skip(2)) {


                    var data = DateTime.ParseExact(line.Substring(0, 10), "dd/MM/yyyy", System.Globalization.DateTimeFormatInfo.InvariantInfo);

                    if (primeira) modeloMpcv.DataPrevisao = data;

                    primeira = false;

                    float v216 = float.Parse(line.Substring(18, 5));
                    float v215 = float.Parse(line.Substring(38, 5));
                    float v92 = float.Parse(line.Substring(58, 5));

                    arq216.Vazoes[data] = v216;
                    arq215.Vazoes[data] = v215;
                    arq92.Vazoes[data] = v92;
                }
            }


            internal void Gravar() {
                var arqContent = new StringBuilder();
                arqContent.AppendLine("Aproveitamento     Campos Novos        Barra Grande        Itá");


                var dataFimSemanaPrevisao = modeloMpcv.DataPrevisao.AddDays(5 - (int)modeloMpcv.DataPrevisao.DayOfWeek);
                var dataIncioSemanaPrevisao = dataFimSemanaPrevisao.AddDays(-6);

                arqContent.AppendLine("Semana Estimada   " +
                    arqVazoes[0].Vazoes.Where(x =>
                    x.Key >= dataIncioSemanaPrevisao && x.Key <= dataFimSemanaPrevisao
                    ).Average(x => x.Value).ToString("0").PadLeft(5)
                    + "               "
                    +
                    arqVazoes[1].Vazoes.Where(x =>
                    x.Key >= dataIncioSemanaPrevisao && x.Key <= dataFimSemanaPrevisao
                    ).Average(x => x.Value).ToString("0").PadLeft(5)
                    + "               " +
                    arqVazoes[2].Vazoes.Where(x =>
                    x.Key >= dataIncioSemanaPrevisao && x.Key <= dataFimSemanaPrevisao
                    ).Average(x => x.Value).ToString("0").PadLeft(5)
                    + " ");


                for (DateTime dt = modeloMpcv.DataPrevisao; dt < dataIncioSemanaPrevisao.AddDays(7); dt = dt.AddDays(1)) {
                    //arqContent.AppendLine("Semana Estimada   " +
                    arqContent.AppendLine(dt.ToString("dd/MM/yyyy").PadRight(18) +
                    arqVazoes[0].Vazoes[dt].ToString("0").PadLeft(5)
                    + "               " +
                    arqVazoes[1].Vazoes[dt].ToString("0").PadLeft(5)
                    + "               " +
                    arqVazoes[2].Vazoes[dt].ToString("0").PadLeft(5)
                    + " ");

                }

                System.IO.File.WriteAllText(this.arquivo, arqContent.ToString());
            }
        }
    }
}
