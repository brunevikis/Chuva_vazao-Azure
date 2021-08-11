using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace ChuvaVazaoTools.SMAP
{

    public class ModeloSmap : ModeloChuvaVazao
    {

        public List<string> ModelosPrecipitacao { get; set; }

        public List<SubBacia> SubBacias { get; set; }

        private string Execucao { get; set; }

        private string ArquivosDeEntrada { get { return System.IO.Path.Combine(Caminho, "Arq_Entrada"); } }
        private string ArquivosDeSaida { get { return System.IO.Path.Combine(Caminho, "Arq_Saida"); } }

        public ModeloSmap(string path, Boolean manual = false)
        {
            Caminho = path;

            PostosPlu = new List<PostoPlu>();

            var caso = System.IO.Directory.GetFiles(ArquivosDeEntrada, "CASO.TXT")[0];

            SubBacias =
            System.IO.File.ReadLines(caso)
                .Skip(1)
                .Select(x => x.PadRight(30).Substring(0, 30).Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x =>
                {
                    var subbaciaNome = x;
                    var subbacia = new SubBacia(path, subbaciaNome, manual);
                    return subbacia;
                }).ToList();


            PostosPlu = SubBacias.SelectMany(x => x.Postos).ToList();


            foreach (var posto in PostosPlu)
            {
                var existingFiles = System.IO.Directory.GetFiles(Caminho,
                    posto.Codigo + "_c.txt", System.IO.SearchOption.AllDirectories);

                if (existingFiles.Length > 0)
                {
                    posto.Carregar(existingFiles[0], manual);
                    posto.l1 = "";
                }
            }

            var modelos = System.IO.Directory.GetFiles(ArquivosDeEntrada, "MODELOS_PRECIPITACAO.TXT")[0];

            ModelosPrecipitacao = System.IO.File.ReadLines(modelos)
                  .Skip(1).Take(1)
                  .Select(x => x.Split(' ')[0])
                  .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x).ToList();


            Vazoes = SubBacias.Cast<IArqVazao>();


        }

        public override void Executar()
        {
            //batsmap-desktop.exe
            //bin
            //logs
            //Arq_Saida


            var f = System.IO.Path.Combine(Config.SmapApp, "batsmap-desktop.exe");
            var d1 = System.IO.Path.Combine(Config.SmapApp, "bin");

            var fb = System.IO.Path.Combine(Caminho, "batsmap-desktop.exe");
            var d1b = System.IO.Path.Combine(Caminho, "bin");


            var d2b = System.IO.Path.Combine(Caminho, "logs");
            var d3b = System.IO.Path.Combine(Caminho, "Arq_Saida");

            var logFile = System.IO.Path.Combine(d2b, "desktop_bat.log");


            if (!System.IO.File.Exists(fb)) System.IO.File.Copy(f, fb, true);

            if (!System.IO.Directory.Exists(d1b)) System.IO.Directory.CreateDirectory(d1b);
            System.IO.Directory.EnumerateFiles(d1).ToList().ForEach(x =>
            {
                System.IO.File.Copy(x,
                    System.IO.Path.Combine(d1b, System.IO.Path.GetFileName(x))
                    , true);
           });

            if (!System.IO.Directory.Exists(d2b)) System.IO.Directory.CreateDirectory(d2b);

            if (!System.IO.Directory.Exists(d3b)) System.IO.Directory.CreateDirectory(d3b);

            if (System.IO.File.Exists(logFile)) System.IO.File.Delete(logFile);

            //executar


            System.Diagnostics.Process pr = new System.Diagnostics.Process();

            var si = pr.StartInfo;

            si.FileName = fb;

            si.WorkingDirectory = Caminho;

            si.CreateNoWindow = true;
            si.UseShellExecute = false;
            si.RedirectStandardOutput = true;
            si.RedirectStandardInput = true;

            pr.StartInfo = si;

            pr.Start();


            while (true && !pr.HasExited)
            {

                if (!pr.StandardOutput.EndOfStream)
                {

                    var l = pr.StandardOutput.ReadLine();

                    Execucao += l + Environment.NewLine;

                    if (l.Contains("nao sera executada") || l.Contains("Finalizando programa"))
                    {
                        pr.StandardInput.Write(ConsoleKey.Enter.ToString());

                        if (l.Contains("nao sera executada")) this.ErroNaExecucao = true;
                        else this.ErroNaExecucao = false;

                        break;
                    }
                }
            }

            pr.WaitForExit();

            if (System.IO.File.Exists(fb)) System.IO.File.Delete(fb);
            if (System.IO.Directory.Exists(d1b)) System.IO.Directory.Delete(d1b, true);


        }

        public override void ColetarSaida()
        {
            foreach (var sb in SubBacias)
            {
                //travado para considerar somente um modelo de precipitacao por rodada;
                sb.CarregaSaida(ModelosPrecipitacao[0]);
            }
        }
        public override void SalvarPrecObservada()
        {

            foreach (var postoPlu in this.PostosPlu)
            {

                var c = System.IO.Path.Combine(ArquivosDeEntrada, postoPlu.Codigo + "_c.txt");
                postoPlu.Salvar(c);

            }
        }
        public override void SalvarPrecPrevista(Dictionary<DateTime, Precipitacao> previsaoChuva)
        {

            //ModelosPrecipitacao.Clear();
            //ModelosPrecipitacao.Add(ModelosPrecipitacao.First());

            System.IO.File.WriteAllText(System.IO.Path.Combine(ArquivosDeEntrada, "MODELOS_PRECIPITACAO.TXT"),
                ModelosPrecipitacao.Count.ToString() + "\r\n" +
                String.Join("\r\n", ModelosPrecipitacao));


            //travado para apenas um modelo - melhorar depois
            var modelo = ModelosPrecipitacao[0];

            foreach (var prec in previsaoChuva.Where(x => x.Key > DataPrevisao))
            {

                var raiznome = modelo + "_p" + DataPrevisao.ToString("ddMMyy") + "a" + prec.Key.ToString("ddMMyy") + ".dat";

                prec.Value.SalvarModeloEta(
                    System.IO.Path.Combine(ArquivosDeEntrada, raiznome),
                     -30.2m, -13.8m, -55.8m, -41.8m
                    );

            }

            var diasDePrevisao = previsaoChuva.Where(x => x.Key > DataPrevisao).Count() + 2;

            SubBacias.ForEach(x => x.Inicializacao.DiasPrevisao = diasDePrevisao);

        }
        public override void SalvarPrecPrevista_R(Dictionary<DateTime, Precipitacao> previsaoChuva)
        {

            //ModelosPrecipitacao.Clear();
            //ModelosPrecipitacao.Add(ModelosPrecipitacao.First());

            System.IO.File.WriteAllText(System.IO.Path.Combine(ArquivosDeEntrada, "MODELOS_PRECIPITACAO.TXT"),
                ModelosPrecipitacao.Count.ToString() + "\r\n" +
                String.Join("\r\n", ModelosPrecipitacao));


            //travado para apenas um modelo - melhorar depois
            var modelo = ModelosPrecipitacao[0];

            foreach (var prec in previsaoChuva.Where(x => x.Key > DataPrevisao))
            {

                var raiznome = modelo + "_p" + DataPrevisao.ToString("ddMMyy") + "a" + prec.Key.ToString("ddMMyy") + ".dat";


            }

            // var diasDePrevisao = previsaoChuva.Where(x => x.Key > DataPrevisao).Count() + 2;
            var diasDePrevisao = previsaoChuva.Where(x => x.Key > DataPrevisao).Count();
            SubBacias.ForEach(x => x.Inicializacao.DiasPrevisao = diasDePrevisao);

        }
        public override DateTime DataPrevisao
        {
            get
            {
                return SubBacias.First().Inicializacao.Data;
            }
            set
            {
                foreach (var item in SubBacias)
                {
                    item.Inicializacao.Data = value;
                }
            }
        }
        public override void SalvarParametros()
        {
            foreach (var item in SubBacias)
            {

                item.SalvarInicializacao();
            }
        }
        public override void SalvarVazaoObservada()
        {
            foreach (var sb in this.SubBacias)
            {
                sb.SalvarVazoes();
            }
        }
    }

    public class SubBacia : IArqVazao
    {

        VazoesRealizadas vazRealizadas;

        public SubBacia(string path, string subbacia, Boolean manual = false)
        {
            // TODO: Complete member initialization
            this.Caminho = path;
            this.Nome = subbacia;

            //inicializacao
            var iniFile = System.IO.Path.Combine(ArquivosDeEntrada, Nome + "_INICIALIZACAO.txt");
            Inicializacao = new InicializacaoSubBacia(iniFile);

            var postosFile = System.IO.Path.Combine(ArquivosDeEntrada, Nome + "_POSTOS_PLU.txt");

            Postos = System.IO.File.ReadLines(postosFile).Skip(1)
                            .Select(x => x.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                            .Where(x => x.Length >= 2)
                            .Select(x =>
                                new PostoPlu()
                                {
                                    Codigo = x[0],
                                    Peso = x[1]
                                }).ToList();

            vazRealizadas = new VazoesRealizadas(System.IO.Path.Combine(ArquivosDeEntrada, Nome + ".txt"), manual);

        }

        private string Caminho { get; set; }
        public string Nome { get; set; }
        public InicializacaoSubBacia Inicializacao { get; set; }
        public List<PostoPlu> Postos { get; set; }

        /// <summary>
        /// lat;lon
        /// </summary>
        public List<Tuple<float, float>> ETA40 { get; set; }

        //entrada
        public Dictionary<DateTime, float> Vazoes { get { return vazRealizadas.Vazoes; } set { vazRealizadas.Vazoes = value; } }

        //saida
        public Dictionary<DateTime, float> VazoesCal { get; set; }
        public Dictionary<DateTime, Ajuste> Ajustes { get; set; }

        private string ArquivosDeEntrada { get { return System.IO.Path.Combine(Caminho, "Arq_Entrada"); } }
        private string ArquivosDeSaida { get { return System.IO.Path.Combine(Caminho, "Arq_Saida"); } }

        public void CarregaSaida(string modeloPrecipitacao)
        {

            var ajustesFile = System.IO.Path.Combine(ArquivosDeSaida, Nome + "_AJUSTE.txt");
            var previsaoFile = System.IO.Path.Combine(ArquivosDeSaida, Nome + "_" + modeloPrecipitacao + "_PREVISAO.txt");

            Ajustes = new Dictionary<DateTime, Ajuste>();
            if (System.IO.File.Exists(ajustesFile))
            {
                System.IO.File.ReadLines(ajustesFile)
                    .Skip(1)
                    .Select(x => x.Split(' '))
                    .Where(x => x.Length >= 5)
                    .Select(x => new
                    {
                        Data = DateTime.ParseExact(x[0], "dd/MM/yyyy", System.Globalization.DateTimeFormatInfo.InvariantInfo),
                        Tu = float.Parse(x[1], System.Globalization.NumberFormatInfo.InvariantInfo),
                        Eb = float.Parse(x[2], System.Globalization.NumberFormatInfo.InvariantInfo),
                        Sup = float.Parse(x[3], System.Globalization.NumberFormatInfo.InvariantInfo),
                        Qaj = float.Parse(x[4], System.Globalization.NumberFormatInfo.InvariantInfo)
                    }).ToList().ForEach(x => Ajustes[x.Data] = new Ajuste() { EB = x.Eb, Q = x.Qaj, SUP = x.Sup, TU = x.Tu });
            }

            VazoesCal = new Dictionary<DateTime, float>();
            if (System.IO.File.Exists(previsaoFile))
            {
                System.IO.File.ReadLines(previsaoFile)
                    .Skip(1)
                    .Select(x => x.Split(' '))
                    .Where(x => x.Length >= 2)
                    .Select(x => new
                    {
                        Data = DateTime.ParseExact(x[0], "dd/MM/yyyy", System.Globalization.DateTimeFormatInfo.InvariantInfo),
                        Qcal = float.Parse(x[1], System.Globalization.NumberFormatInfo.InvariantInfo)
                    }).ToList().ForEach(x =>
                    {
                        VazoesCal[x.Data] = x.Qcal;
                        Vazoes[x.Data] = x.Qcal;
                    }
                    );
            }

        }

        internal void SalvarInicializacao()
        {
            var iniFile = System.IO.Path.Combine(ArquivosDeEntrada, Nome + "_INICIALIZACAO.txt");
            Inicializacao.Write(iniFile);

        }

        public string CaminhoArquivo
        {
            get
            {
                return vazRealizadas.CaminhoArquivo;
            }
            set
            {
                vazRealizadas.CaminhoArquivo = value;
            }
        }

        public void SalvarVazoes()
        {
            vazRealizadas.SalvarVazoes();
        }

        public void CarregarVazoes(Boolean manual = false)
        {
            vazRealizadas.CarregarVazoes(manual);
        }

        internal void ReiniciarParametros()
        {
            //var dataPar = Inicializacao.Data.AddDays(1-Inicializacao.DiasPassados);
            var dataPar = Inicializacao.Data.AddDays(-Inicializacao.DiasPassados);

            if (Ajustes.ContainsKey(dataPar))
            {
                Inicializacao.Ebin = Ajustes[dataPar].EB;
                Inicializacao.Supin = Ajustes[dataPar].SUP;
                Inicializacao.Tuin = Ajustes[dataPar].TU;
            }
            else
            {
                throw new Exception("Não foram encontrados parametros inicias para a subbacia: " + this.Nome);
            }
        }
    }

    public class InicializacaoSubBacia
    {

        public InicializacaoSubBacia(string path)
        {
            this.Read(path);
        }

        public DateTime Data { get; set; }
        public int DiasPassados { get; set; }
        public int DiasPrevisao { get; set; }
        public float Ebin { get; set; }
        public float Supin { get; set; }
        public float Tuin { get; set; }

        public void Write(string filePath)
        {
            var txt = Data.ToString("dd/MM/yyyy") + "\r\n" +
                DiasPassados.ToString() + "\r\n" +
                DiasPrevisao.ToString() + "\r\n" +
                Ebin.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo).PadRight(15) + "'ebin\r\n" +
                Supin.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo).PadRight(15) + "'supin\r\n" +
                Tuin.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo).PadRight(15) + "'tuin\r\n";

            System.IO.File.WriteAllText(filePath, txt);
        }

        public void Read(string filePath)
        {

            using (var sr = System.IO.File.OpenText(filePath))
            {

                var temp = sr.ReadLine().Trim().Split(' ')[0];
                Data = DateTime.ParseExact(temp, "dd/MM/yyyy", System.Globalization.DateTimeFormatInfo.InvariantInfo);

                temp = sr.ReadLine().Trim().Split(' ')[0];
                DiasPassados = int.Parse(temp);

                temp = sr.ReadLine().Trim().Split(' ')[0];
                DiasPrevisao = int.Parse(temp);

                temp = sr.ReadLine().Trim().Split(' ')[0];
                Ebin = float.Parse(temp, System.Globalization.NumberFormatInfo.InvariantInfo);

                temp = sr.ReadLine().Trim().Split(' ')[0];
                Supin = float.Parse(temp, System.Globalization.NumberFormatInfo.InvariantInfo);

                temp = sr.ReadLine().Trim().Split(' ')[0];
                Tuin = float.Parse(temp, System.Globalization.NumberFormatInfo.InvariantInfo);

            }

        }

    }

    public struct Ajuste
    {
        public float TU { get; set; }
        public float EB { get; set; }
        public float SUP { get; set; }
        public float Q { get; set; }
    }
}
