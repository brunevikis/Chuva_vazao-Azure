using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChuvaVazaoTools
{
    public abstract class ModeloChuvaVazao
    {

        public string Caminho { get; set; }

        public bool? ErroNaExecucao { get; set; }

        public List<PostoPlu> PostosPlu { get; set; }
        public abstract DateTime DataPrevisao { get; set; }
        public IEnumerable<IArqVazao> Vazoes { get; set; }

        public abstract void SalvarPrecObservada();

        public abstract void SalvarPrecPrevista(Dictionary<DateTime, Precipitacao> previsaoChuva);
        public abstract void SalvarPrecPrevista_R(Dictionary<DateTime, Precipitacao> previsaoChuva);

        public abstract void SalvarParametros();

        public abstract void Executar();
        public virtual async Task ExecutarAsync()
        {
            await Task.Factory.StartNew(() => Executar());
        }

        public abstract void ColetarSaida();

        public abstract void SalvarVazaoObservada();
    }

    public class PostoPlu
    {
        public string l1 = "";

        public string Codigo { get; set; }
        public string Peso { get; set; }

        public Dictionary<DateTime, float?> Preciptacao { get; set; }

        public PostoPlu()
        {
            Preciptacao = new Dictionary<DateTime, float?>();
        }

        internal void Carregar(string p, Boolean manual = false)
        {

            var data_verifica = DateTime.Today;
            var runRev = ChuvaVazaoTools.Tools.Tools.GetCurrRev(data_verifica);
            var acomph_atual = File.Exists(Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de vazões\ACOMPH\1_historico", data_verifica.ToString("yyyy"), data_verifica.ToString("MM_yyyy"), "ACOMPH_" + data_verifica.ToString("dd-MM-yyyy") + ".xls"));
            var modelos_atual = File.Exists(Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de vazões", runRev.revDate.ToString("MM_yyyy"), @"Dados_de_Entrada_e_Saida_" + runRev.revDate.ToString("yyyyMM") + "_RV" + runRev.rev, @"Modelos_Chuva_Vazao\CPINS\Arq_Saida", data_verifica.ToString("dd-MM-yyyy") + "_PLANILHA_USB.txt"));


            var ls = System.IO.File.ReadLines(p);


            if (acomph_atual == false || modelos_atual == true || manual == true)
            {
                l1 = ls.First();

                ls.Select(x => x.Split(' '))
                    .Where(x => x.Length >= 4)
                    .Select(x =>
                        new
                        {
                            Data = DateTime.ParseExact(x[1], "dd/MM/yyyy", System.Globalization.DateTimeFormatInfo.InvariantInfo),
                            Prec = x[3].Contains('-') ? (float?)null :
                                float.Parse(x[3], System.Globalization.NumberFormatInfo.InvariantInfo),
                        }).ToList().ForEach(x =>
                            Preciptacao[x.Data] = x.Prec
                            );
            }
            else
            {
                l1 = ls.First();

                ls.Skip(1)
                    .Select(x => x.Split(' '))
                    .Where(x => x.Length >= 4)
                    .Select(x =>
                        new
                        {
                            Data = DateTime.ParseExact(x[1], "dd/MM/yyyy", System.Globalization.DateTimeFormatInfo.InvariantInfo),
                            Prec = x[3].Contains('-') ? (float?)null :
                                float.Parse(x[3], System.Globalization.NumberFormatInfo.InvariantInfo),
                        }).ToList().ForEach(x =>
                            Preciptacao[x.Data] = x.Prec
                            );

            }

        }



        internal void Salvar(string c)
        {

            var cont = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(l1)) cont.AppendLine(l1);

            Preciptacao
                .OrderByDescending(x => x.Key).Take(120)
                .OrderBy(x => x.Key).ToList().ForEach(x => cont.AppendLine(
                               string.Join(" ", this.Codigo, x.Key.ToString("dd/MM/yyyy"), "1000", x.Value.HasValue ? x.Value.Value.ToString("0.0", System.Globalization.NumberFormatInfo.InvariantInfo) : "-")
                ));

            System.IO.File.WriteAllText(c, cont.ToString());

        }
    }

    public interface IArqVazao
    {
        string Nome { get; set; }
        string CaminhoArquivo { get; set; }

        Dictionary<DateTime, float> Vazoes { get; set; }
        void SalvarVazoes();
        void CarregarVazoes(Boolean teste = false);




    }

    public class VazoesRealizadas : IArqVazao
    {
        public int Id { get; set; }
        public string Tipo { get; set; }

        string key = "0123456789|COD|DI|VVV";


        public string CaminhoArquivo { get; set; }


        public Dictionary<DateTime, float> Vazoes
        {
            get;
            set;
        }

        public VazoesRealizadas(string arquivo, Boolean manual = false)
        {
            CaminhoArquivo = arquivo;
            CarregarVazoes(manual);
        }

        public void SalvarVazoes()
        {
            System.IO.File.WriteAllLines(CaminhoArquivo,
                Vazoes
                .OrderByDescending(x => x.Key).Take(120)
                .OrderBy(x => x.Key).Select(x =>
                  string.Join("|", key, x.Key.ToString("yyyy-MM-dd hh:mm:ss"), x.Value.ToString("0.0", System.Globalization.NumberFormatInfo.InvariantInfo))
                ).ToArray()
            );
        }

        public void CarregarVazoes(Boolean manual = false)
        {

            var data_verifica = DateTime.Today;
            var runRev = ChuvaVazaoTools.Tools.Tools.GetCurrRev(data_verifica);
            var acomph_atual = File.Exists(Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de vazões\ACOMPH\1_historico", data_verifica.ToString("yyyy"), data_verifica.ToString("MM_yyyy"), "ACOMPH_" + data_verifica.ToString("dd-MM-yyyy") + ".xls"));
            var modelos_atual = File.Exists(Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de vazões", runRev.revDate.ToString("MM_yyyy"), @"Dados_de_Entrada_e_Saida_" + runRev.revDate.ToString("yyyyMM") + "_RV" + runRev.rev, @"Modelos_Chuva_Vazao\CPINS\Arq_Saida", data_verifica.ToString("dd-MM-yyyy") + "_PLANILHA_USB.txt"));

            Vazoes = new Dictionary<DateTime, float>();

            if (System.IO.File.Exists(CaminhoArquivo))
            {

                if (acomph_atual == false || modelos_atual == true || manual == true)
                {

                    var ls = System.IO.File.ReadLines(CaminhoArquivo).Select(x => x.Split('|')).Where(x => x.Length >= 6);


                    ls.Select(x => new
                    {
                        Data = DateTime.ParseExact(x[4], "yyyy-MM-dd hh:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo),
                        Q = float.Parse(x[5].Replace("-", "0"), System.Globalization.NumberFormatInfo.InvariantInfo),
                    }).ToList().ForEach(x => Vazoes[x.Data] = x.Q);


                    key = ls.First()[0] + "|" + ls.First()[1] + "|" + ls.First()[2] + "|" + ls.First()[3];
                }
                else
                {
                    var ls = System.IO.File.ReadLines(CaminhoArquivo).Select(x => x.Split('|')).Where(x => x.Length >= 6);


                    ls.Skip(1).Select(x => new
                    {
                        Data = DateTime.ParseExact(x[4], "yyyy-MM-dd hh:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo),
                        Q = float.Parse(x[5].Replace("-", "0"), System.Globalization.NumberFormatInfo.InvariantInfo),
                    }).ToList().ForEach(x => Vazoes[x.Data] = x.Q);


                    key = ls.First()[0] + "|" + ls.First()[1] + "|" + ls.First()[2] + "|" + ls.First()[3];

                }
            }



        }


        public float this[DateTime data]
        {
            get { return this.Vazoes[data]; }
            set { this.Vazoes[data] = value; }
        }


        public string Nome
        {
            get { return System.IO.Path.GetFileNameWithoutExtension(CaminhoArquivo); }
            set { throw new NotImplementedException(); }
        }
    }
}
