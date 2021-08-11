using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChuvaVazaoTools.Fuzzy {
    public class ModeloFuzzy : ModeloChuvaVazao {


        static string[] arqVaz = new string[] { 
            "Foz_do_Areia-Iguacu.txt"

            ,"INC_Salto_Osorio-Iguacu.txt"
            ,"INC_Salto_Santiago-Iguacu.txt"
            ,"INC_Segredo-Iguacu.txt"      

            ,"Jordao-Iguacu.txt"             
            ,"Salto_Osorio-Iguacu.txt"
            ,"Salto_Santiago-Iguacu.txt"
            ,"Segredo-Iguacu.txt"
        };

        static string[] arqVazCalc = new string[] {         
            "Dfartot",
            
            "Dosoinc",
            "Dsaninc",
            "Dseginc",
        
            "Djortot",
            "Dosotot",        
            "Dsantot",
            "Dsegtot", 
        };

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

            var data = DataPrevisao;

            foreach (var prec in previsaoChuva.Where(x => x.Key > DataPrevisao)) {

                //if (++i == 11) data = data.AddDays(10);

                var raiznome = "p" + data.ToString("ddMMyy") + "a" + prec.Key.ToString("ddMMyy") + ".dat";

                prec.Value.SalvarModeloEta(
                    System.IO.Path.Combine(ArquivosDeEntrada, raiznome), -28.2m, -23.8m, -55, -47.8m
                    );
            }
        }

        public override void SalvarParametros() {
            return;
            throw new NotImplementedException();
        }

        public override void Executar() {
            System.Windows.Forms.MessageBox.Show("Aguardando Execução do FUZZY");
            return;
        }

        public override void ColetarSaida() {

            for (int i = 0; i < arqVazCalc.Length; i++) {

                var fs = System.IO.Directory.GetFiles(
                   this.Caminho,
                   arqVazCalc[i] + this.DataPrevisao.ToString("ddMMyyyy") + ".dat"
                   , System.IO.SearchOption.AllDirectories);

                if (fs.Length > 0) {

                    var a = (VazoesRealizadas)this.Vazoes.First(x => System.IO.Path.GetFileName(x.CaminhoArquivo).Equals(arqVaz[i], StringComparison.OrdinalIgnoreCase));

                    System.IO.File.ReadAllLines(fs[0])
                        .Skip(1)
                        .Where(y => y.Length >= 13)
                        .Select(y =>
                            new {
                                Data = DateTime.ParseExact(y.Substring(0, 10), "dd-MM-yyyy", System.Globalization.DateTimeFormatInfo.InvariantInfo),
                                Vaz = float.Parse(y.Substring(10).Trim(), System.Globalization.NumberFormatInfo.InvariantInfo)
                            }).ToList().ForEach(y => {
                                a[y.Data] = y.Vaz;
                            });
                }
            }
        }

        public override void SalvarVazaoObservada() {
            foreach (var item in this.Vazoes) {
                item.SalvarVazoes();
            }

            //this.DataPrevisao = this.Vazoes.SelectMany(x => x.Vazoes.Keys).Max().AddDays(1);
        }


        private string ArquivosDeEntrada { get { return System.IO.Path.Combine(Caminho, "ARQ_ENTRADA"); } }


        public ModeloFuzzy(string path) {
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
        }
    }
}
