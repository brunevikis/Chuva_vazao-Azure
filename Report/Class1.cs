using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Excel;
using System.IO;
using ChuvaVazaoTools.Tools;

namespace Report
{
    class PegaDados                                                                                                         //  Localiza os arquivos fontes de dados 
    {
        public string[,] Bacias { get; set; }
        public double[,] Precipitacoes { get; set; }

        public double[,] bgrandeVetor = new double[1, 10];
        public double[,] bparanaiVetor = new double[1, 10];
        public double[,] bparanapVetor = new double[1, 10];
        public double[,] biguaVetor = new double[1, 10];
        public double[,] burugVetor = new double[1, 10];
        public double[,] bsfrancVetor = new double[1, 10];
        public double[,] bparanaVetor = new double[1, 10];
        public double[,] bitaiVetor = new double[1, 10];
        public double[,] btietVetor = new double[1, 10];
        public double[,] btocaVetor = new double[1, 10];

        public static PegaDados LeituraPrecipitation(string name)
        {
            var pegaDados = new PegaDados();

            if (File.Exists(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Previsao_Numerica\" + name))
            {

                String input = System.IO.File.ReadAllText(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Previsao_Numerica\" + name);

                double[,] result = null;

                int i = 0;
                var rows = input.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Skip(1);

                pegaDados.Bacias = new string[rows.Count(), 2];

                foreach (var row in rows)
                {
                    int j = 0;
                    var cols = row.Trim().Split(new string[] { "\t" }, StringSplitOptions.RemoveEmptyEntries);

                    pegaDados.Bacias[i, 0] = cols[0];
                    pegaDados.Bacias[i, 1] = cols[1];

                    foreach (var col in cols.Skip(2))
                    {
                        if (result == null) result = new double[rows.Count(), cols.Count() - 2];

                        result[i, j] = double.Parse(col.Trim()/*, System.Globalization.CultureInfo.InvariantCulture*/);

                        j++;
                    }
                    i++;
                }

                pegaDados.Precipitacoes = result;

                pegaDados.Fill();
            }

            return pegaDados;
        }

        void Fill()
        {
            var func = new Action<double[,], string>((vetor, nome) =>
            {

                for (int j = 0; j < 10; j++)
                {
                    var _c = 0;
                    for (int i = 0; i < Bacias.GetLength(0); i++)
                        if (Bacias[i, 0] == nome)
                        {
                            vetor[0, j] += Precipitacoes[i, j];
                            _c++;
                        }

                    vetor[0, j] = vetor[0, j] / _c;
                }

            });

            func(bgrandeVetor, "GRANDE");
            func(bparanaiVetor, "PARANAÍBA");
            func(bparanapVetor, "PARANAPANEMA");
            func(biguaVetor, "IGUAÇU");
            func(burugVetor, "URUGUAI");
            func(bsfrancVetor, "SÃO FRANCISCO");
            func(bparanaVetor, "PARANÁ");
            func(bitaiVetor, "ITAIPU");
            func(btietVetor, "TIETÊ");
            func(btocaVetor, "TOCANTINS");
        }

    }
    class ElegirArquivo                                                                                                         //  Extrae dados dos arquivos fontes
    {
        public PegaDados pmdxsbetaUltimo;   //Precipitação media diaria x Sub-bacia com retiro de Vies para modelo ETA40
        public PegaDados pmdxsbetaAnterior;   //Precipitação media diaria x Sub-bacia com retiro de Vies para modelo ETA40
        public PegaDados pmdxsbgefsUltimo;  //Precipitação media diaria x Sub-bacia com retirove de Vies para modelo GEFS
        public PegaDados pmdxsbgefsAnterior;  //Precipitação media diaria x Sub-bacia com retiro de Vies para modelo GEFS
        public PegaDados pmdxsbgfsUltimo;  //Precipitação media diaria x Sub-bacia com retirove de Vies para modelo GFS
        public PegaDados pmdxsbgfsAnterior;  //Precipitação media diaria x Sub-bacia com retiro de Vies para modelo GFS
        public PegaDados pmdxsbconjUltimo;  //Precipitação media diaria x Sub-bacia com retiro de Vies para o Conjunto
        public PegaDados pmdxsbconjAnterior;  //Precipitação media diaria x Sub-bacia com retiro de Vies para o Conjunto
        public void Criar(DateTime data, int hora)
        {
            var dataUltima = data.AddHours(hora);
            var logEtaUltimo = dataUltima.ToString("yyyyMM") + "\\" + dataUltima.ToString("dd") + "\\ETA" + dataUltima.Hour.ToString("00") + ".log";
            var logGefsUltimo = dataUltima.ToString("yyyyMM") + "\\" + dataUltima.ToString("dd") + "\\GEFS" + dataUltima.Hour.ToString("00") + ".log";
            var logGfsUltimo = dataUltima.ToString("yyyyMM") + "\\" + dataUltima.ToString("dd") + "\\GFS" + dataUltima.Hour.ToString("00") + ".log";
            var logConjuntoUltimo = dataUltima.ToString("yyyyMM") + "\\" + dataUltima.ToString("dd") + "\\CONJUNTO" + dataUltima.Hour.ToString("00") + ".log";

            var dataAnterior = dataUltima.AddHours(-12);
            var logEtaAnterior = dataAnterior.ToString("yyyyMM") + "\\" + dataAnterior.ToString("dd") + "\\ETA" + dataAnterior.Hour.ToString("00") + ".log";
            var logGefsAnterior = dataAnterior.ToString("yyyyMM") + "\\" + dataAnterior.ToString("dd") + "\\GEFS" + dataAnterior.Hour.ToString("00") + ".log";
            var logGfsAnterior = dataAnterior.ToString("yyyyMM") + "\\" + dataAnterior.ToString("dd") + "\\GFS" + dataAnterior.Hour.ToString("00") + ".log";
            var logConjuntoAnterior = dataAnterior.ToString("yyyyMM") + "\\" + dataAnterior.ToString("dd") + "\\CONJUNTO" + dataAnterior.Hour.ToString("00") + ".log";


            pmdxsbetaUltimo = PegaDados.LeituraPrecipitation(logEtaUltimo);                     // logEtaUltimo
            pmdxsbetaAnterior = PegaDados.LeituraPrecipitation(logEtaAnterior);                     // logEtaAnterior
            pmdxsbgefsUltimo = PegaDados.LeituraPrecipitation(logGefsUltimo);                   // logGefsUltimo
            pmdxsbgefsAnterior = PegaDados.LeituraPrecipitation(logGefsAnterior);                   // logGefsAnterior
            pmdxsbgfsUltimo = PegaDados.LeituraPrecipitation(logGfsUltimo);                     // logGfsUltimo
            pmdxsbgfsAnterior = PegaDados.LeituraPrecipitation(logGfsAnterior);                     // logGfsAnterior
            pmdxsbconjUltimo = PegaDados.LeituraPrecipitation(logConjuntoUltimo);               // logConjuntoUltimo
            pmdxsbconjAnterior = PegaDados.LeituraPrecipitation(logConjuntoAnterior);               // logConjuntoAnterior

        }
    }
    public class Program
    {

        internal static string caminhoBase = @"C:\Files\Relatorios\Relatorio Final\";

        public static string CriarRelatorio2(DateTime data, string caminho = null, bool preliminar = false)
        {
            Tools.addHistory("C:\\Sistemas\\ChuvaVazao\\Log\\report.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "- metodo CriarRelatorio2 - preliminar == " + preliminar);
            DateTime data1 = data.AddDays(-1);

            Tuple<DateTime, DateTime> semana0;
            Tuple<DateTime, DateTime> semana1;
            Tuple<DateTime, DateTime> semana2;

            {
                var dt = data;
                while (dt.DayOfWeek != DayOfWeek.Saturday) dt = dt.AddDays(-1);

                semana0 = new Tuple<DateTime, DateTime>(dt, dt.AddDays(6));
                semana1 = new Tuple<DateTime, DateTime>(dt.AddDays(7), dt.AddDays(7 + 6));
                semana2 = new Tuple<DateTime, DateTime>(dt.AddDays(14), dt.AddDays(14 + 6));
            }

            //if (true)
            // {
            CriarImagemChuvas(data, data1, semana0, semana1, semana2);
            //CriarImagens(data, preliminar, 0);
            var nomescasosCV = CriarImagensEnas(data, preliminar);
            //}
            if (nomescasosCV == null)
            {
                return "";
            }


            //   CriarImagens(data, preliminar, hora);

            //////////////////////////////////////////////////////////////
            ///////////// CRIAÇÂO DO PDF
            //////////////////////////////////////////////////////////////
            int hora = 0;
            if (caminho == null)
                caminho = Path.Combine(caminhoBase, "Relatorios", "Relatorio_Compass_" + data.ToString("dd_MM_yyyy") + "_(" + hora.ToString() + " hrs)_TESTE.pdf");

            var doc = PdfExtensions.NovoPdf2(caminho, data, hora);

            doc.InserirTexto("Este relatório tem por objetivo apresentar subsídios à previsão de ENAs para as semanas seguintes e auxiliar na tomada de decisão. Para tanto, é feito um acompanhamento das previsões de precipitação dos diversos modelos disponíveis e uma comparação entre estas e os valores realizados para os últimos dias. Posteriormente, é apresentado um acompanhamento diário das ENAs dos quatro submercados do SIN, considerando dados realizados e previsões pelo modelo chuva-vazão, e uma discretização da evolução das ENAs nas 10 principais bacias hidrográficas que compõem o sistema.");
            doc.InserirEspaco();


            {
                doc.InserirParte("Impacto da remoção de viés e aplicação de limites na Previsão por Conjunto (ETA 40 + GEFS)");
                doc.InserirTexto("Esta seção apresenta a quantidade de chuva que é desconsiderada pelo modelo chuva-vazão na previsão de ENA.");
                doc.InserirEspaco();

                doc.InserirImagens(1, Path.Combine(caminhoBase, "semanas3x.gif"));
                doc.InserirImagens(1,
                    Path.Combine(caminhoBase, @"imgs_temp", "semana0_conjviesDiff_" + data.ToString("yyyyMMdd") + ".gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana1_conjviesDiff_" + data.ToString("yyyyMMdd") + ".gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana2_conjviesDiff_" + data.ToString("yyyyMMdd") + ".gif"));
            }




            doc.NovaPagina2(); // OBSERVADA
            {
                doc.InserirParte("Acompanhamento de precipitação observada");
                doc.InserirTexto("Nesta seção é feita a comparação da previsão de precipitação pelos modelos Conjunto (ETA 40+GEFS) e Europeu com os valores realizados de precipitação para os últimos três dias.");

                doc.InserirSubtitulo("Previsão por Conjunto (ETA40+GEFS)");
                doc.InserirImagens(1,
                    Path.Combine(caminhoBase, @"imgs_temp", "conjpassado2.gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "conjpassado1.gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "conjpassado0.gif"));

                doc.InserirSubtitulo("Previsão Modelo Europeu (ECMWF)");
                doc.InserirImagens(1,
                    Path.Combine(caminhoBase, @"imgs_temp", "europassado2.gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "europassado1.gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "europassado0.gif"));


                doc.InserirSubtitulo("Precipitação Observada (Merge/Funceme)");
                doc.InserirImagens(1,
                    Path.Combine(caminhoBase, @"imgs_temp", "observado2.gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "observado1.gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "observado0.gif"));

            }

            doc.NovaPagina2();
            {
                doc.InserirSubtitulo("Acumulado de precipitação observada por bacia hidrográfica");

                doc.InserirImagens(1,
                    Path.Combine(caminhoBase, @"imgs_temp", "GRANDEPrecip.jpg"));
                doc.InserirImagens(1,
                    Path.Combine(caminhoBase, @"imgs_temp", "TIETÊPrecip.jpg"));
                doc.InserirImagens(1,
                    Path.Combine(caminhoBase, @"imgs_temp", "PARANAÍBAPrecip.jpg"));
                doc.InserirImagens(1,
                    Path.Combine(caminhoBase, @"imgs_temp", "PARANAPANEMAPrecip.jpg"));
                doc.InserirImagens(1,
                    Path.Combine(caminhoBase, @"imgs_temp", "PARANÁPrecip.jpg"));
                doc.InserirImagens(1,
                    Path.Combine(caminhoBase, @"imgs_temp", "ITAIPUPrecip.jpg"));
                doc.InserirImagens(1,
                    Path.Combine(caminhoBase, @"imgs_temp", "IGUAÇUPrecip.jpg"));
                doc.InserirImagens(1,
                    Path.Combine(caminhoBase, @"imgs_temp", "URUGUAIPrecip.jpg"));
                //doc.InserirImagens(1,
                //    Path.Combine(caminhoBase, @"imgs_temp", "TOCANTINSPrecip.jpg"));
                //doc.InserirImagens(1,
                //    Path.Combine(caminhoBase, @"imgs_temp", "SÃO FRANCISCOPrecip.jpg"));
            }



            doc.NovaPagina2();

            doc.InserirParte("Acompanhamento de previsão de precipitação");

            doc.InserirTexto("Nesta seção é feita a comparação da previsão de precipitação do dia anterior com a previsão atual para cada modelo disponível, acumulada em uma semana, para todas as três semanas de previsão dos modelos, incorporando-se o último dado realizado de chuva à semana atual.");

            // CONJ
            {
                doc.InserirSubtitulo("Previsão por Conjunto (ETA40+GEFS) com remoção de viés e aplicação de limites");

                doc.InserirImagens(1, Path.Combine(caminhoBase, "semanas3x.gif"));

                doc.InserirSubSubtitulo("Previsões de " + data1.ToString("dd/MM"));

                doc.InserirImagens(0.8f,
                    Path.Combine(caminhoBase, @"imgs_temp", "semana0_conj_" + data1.ToString("yyyyMMdd") + ".gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana1_conj_" + data1.ToString("yyyyMMdd") + ".gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana2_conj_" + data1.ToString("yyyyMMdd") + ".gif"));

                doc.InserirSubSubtitulo("Previsões de " + data.ToString("dd/MM"));

                doc.InserirImagens(0.8f,
                    Path.Combine(caminhoBase, @"imgs_temp", "semana0_conj_" + data.ToString("yyyyMMdd") + ".gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana1_conj_" + data.ToString("yyyyMMdd") + ".gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana2_conj_" + data.ToString("yyyyMMdd") + ".gif"));


                doc.InserirSubSubtitulo("Variação entre previsões acumuladas");

                doc.InserirImagens(0.8f,
                    Path.Combine(caminhoBase, @"imgs_temp", "semana0_conjDiff_" + data.ToString("yyyyMMdd") + ".gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana1_conjDiff_" + data.ToString("yyyyMMdd") + ".gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana2_conjDiff_" + data.ToString("yyyyMMdd") + ".gif"));
            }

            doc.NovaPagina2();
            // GEFS
            {
                doc.InserirSubtitulo("Previsão pelo modelo GLOBAL ENSEMBLE (GEFS)");

                doc.InserirImagens(1, Path.Combine(caminhoBase, "semanas3x.gif"));

                doc.InserirSubSubtitulo("Previsões de " + data1.ToString("dd/MM"));

                doc.InserirImagens(1,
                    Path.Combine(caminhoBase, @"imgs_temp", "semana0_gefs_" + data1.ToString("yyyyMMdd") + ".gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana1_gefs_" + data1.ToString("yyyyMMdd") + ".gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana2_gefs_" + data1.ToString("yyyyMMdd") + ".gif")
                    );

                doc.InserirSubSubtitulo("Previsões de " + data.ToString("dd/MM"));

                doc.InserirImagens(1,
                   Path.Combine(caminhoBase, @"imgs_temp", "semana0_gefs_" + data.ToString("yyyyMMdd") + ".gif"),
                   Path.Combine(caminhoBase, @"imgs_temp", "semana1_gefs_" + data.ToString("yyyyMMdd") + ".gif"),
                   Path.Combine(caminhoBase, @"imgs_temp", "semana2_gefs_" + data.ToString("yyyyMMdd") + ".gif")
                    );

                doc.InserirSubSubtitulo("Variação entre previsões acumuladas");

                doc.InserirImagens(1,
                    Path.Combine(caminhoBase, @"imgs_temp", "semana0_gefsDiff_" + data.ToString("yyyyMMdd") + ".gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana1_gefsDiff_" + data.ToString("yyyyMMdd") + ".gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana2_gefsDiff_" + data.ToString("yyyyMMdd") + ".gif"));
            }



            doc.NovaPagina2();
            //GFS
            {
                doc.InserirSubtitulo("Previsão pelo modelo GLOBAL OPERATIVO (GFSNOAA)");

                doc.InserirImagens(1, Path.Combine(caminhoBase, "semanas3x.gif"));

                doc.InserirSubSubtitulo("Previsões de " + data1.ToString("dd/MM"));

                doc.InserirImagens(1,
                    Path.Combine(caminhoBase, @"imgs_temp", "semana0_gfs_" + data1.ToString("yyyyMMdd") + ".gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana1_gfs_" + data1.ToString("yyyyMMdd") + ".gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana2_gfs_" + data1.ToString("yyyyMMdd") + ".gif")
                    );

                doc.InserirSubSubtitulo("Previsões de " + data.ToString("dd/MM"));

                doc.InserirImagens(1,
                   Path.Combine(caminhoBase, @"imgs_temp", "semana0_gfs_" + data.ToString("yyyyMMdd") + ".gif"),
                   Path.Combine(caminhoBase, @"imgs_temp", "semana1_gfs_" + data.ToString("yyyyMMdd") + ".gif"),
                   Path.Combine(caminhoBase, @"imgs_temp", "semana2_gfs_" + data.ToString("yyyyMMdd") + ".gif")
                    );

                doc.InserirSubSubtitulo("Variação entre previsões acumuladas");

                doc.InserirImagens(1,
                    Path.Combine(caminhoBase, @"imgs_temp", "semana0_gfsDiff_" + data.ToString("yyyyMMdd") + ".gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana1_gfsDiff_" + data.ToString("yyyyMMdd") + ".gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana2_gfsDiff_" + data.ToString("yyyyMMdd") + ".gif"));
            }

            doc.NovaPagina2();
            // EURO
            {
                doc.InserirSubtitulo("Previsão pelo modelo EUROPEU (ECMWF Operativo)");

                doc.InserirImagens(1, Path.Combine(caminhoBase, "semanas2x.gif"));

                doc.InserirSubSubtitulo("Previsões de " + data1.ToString("dd/MM"));

                doc.InserirImagens(1,
                    Path.Combine(caminhoBase, @"imgs_temp", "semana0_euro_" + data1.ToString("yyyyMMdd") + ".gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana1_euro_" + data1.ToString("yyyyMMdd") + ".gif")
                    );

                doc.InserirSubSubtitulo("Previsões de " + data.ToString("dd/MM"));

                doc.InserirImagens(1,
                    Path.Combine(caminhoBase, @"imgs_temp", "semana0_euro_" + data.ToString("yyyyMMdd") + ".gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana1_euro_" + data.ToString("yyyyMMdd") + ".gif")
                    );


                doc.InserirSubSubtitulo("Variação entre previsões acumuladas");

                doc.InserirImagens(1,
                    Path.Combine(caminhoBase, @"imgs_temp", "semana0_euroDiff_" + data.ToString("yyyyMMdd") + ".gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana1_euroDiff_" + data.ToString("yyyyMMdd") + ".gif"));
            }

            doc.NovaPagina2();
            // ECMWF ENS
            {
                doc.InserirSubtitulo("Previsão pelo modelo EUROPEU (ECMWF Ensemble)");

                doc.InserirImagens(1, Path.Combine(caminhoBase, "semanas2x.gif"));

                doc.InserirSubSubtitulo("Previsões de " + data1.ToString("dd/MM"));

                doc.InserirImagens(1,
                    Path.Combine(caminhoBase, @"imgs_temp", "semana0_ECMWF_Ens_" + data1.ToString("yyyyMMdd") + ".gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana1_ECMWF_Ens_" + data1.ToString("yyyyMMdd") + ".gif")
                    );

                doc.InserirSubSubtitulo("Previsões de " + data.ToString("dd/MM"));

                doc.InserirImagens(1,
                    Path.Combine(caminhoBase, @"imgs_temp", "semana0_ECMWF_Ens_" + data.ToString("yyyyMMdd") + ".gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana1_ECMWF_Ens_" + data.ToString("yyyyMMdd") + ".gif")
                    );


                doc.InserirSubSubtitulo("Variação entre previsões acumuladas");

                doc.InserirImagens(1,
                    Path.Combine(caminhoBase, @"imgs_temp", "semana0_ECMWF_EnsDiff_" + data.ToString("yyyyMMdd") + ".gif"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana1_ECMWF_EnsDiff_" + data.ToString("yyyyMMdd") + ".gif"));
            }

            doc.NovaPagina2();

            doc.InserirParte("Acompanhamento ENA Chuva-Vazão");
            doc.InserirTexto("Nesta seção é feita a comparação entre rodadas chuva vazão: os dois primeiros cenários são as rodadas do dia anterior, antes e depois da atualização do Acomph (Relatório de Acompanhamento Hidrológico de 30 dias); e os cenários seguintes correspondem às rodadas com as atualizações de previsão de precipitação e vazão (quando disponível).");


            for (int i = (nomescasosCV?.Count() ?? 0) - 1; i >= 0; i--)
            {
                var imgResumo = Path.Combine(caminhoBase, "imgs_temp", "resumo" + i.ToString() + "_" + data.ToString("yyyyMMdd") + ".gif");

                if (File.Exists(imgResumo))
                {
                    doc.InserirSubtitulo(nomescasosCV[i]);
                    doc.InserirImagens(0.75f, Path.Combine(caminhoBase, "imgs_temp", "resumo" + i.ToString() + "_" + data.ToString("yyyyMMdd") + ".gif"));
                }
            }

            doc.NovaPagina2();
            doc.InserirParte("Acompanhamento Diário de ENA");

            doc.InserirTexto("Nesta seção é possível observar a evolução da ENA diariamente, considerando os dados realizados e a saída das previsões, para todos os submercados.");

            doc.InserirEspaco();

            doc.InserirImagens(1.2f,
                Path.Combine(caminhoBase, @"imgs_temp", "SECO_ENA.jpg"));
            doc.InserirImagens(1.2f,
                Path.Combine(caminhoBase, @"imgs_temp", "SECO_VARENA.jpg"));
            doc.InserirEspaco();
            doc.InserirImagens(1.2f,
                Path.Combine(caminhoBase, @"imgs_temp", "S_ENA.jpg"));
            doc.InserirImagens(1.2f,
                Path.Combine(caminhoBase, @"imgs_temp", "S_VARENA.jpg"));
            doc.InserirImagens(1.2f,
                Path.Combine(caminhoBase, @"imgs_temp", "NE_ENA.jpg"));
            doc.InserirEspaco();
            doc.InserirImagens(1.2f,
                Path.Combine(caminhoBase, @"imgs_temp", "N_ENA.jpg"));

            doc.NovaPagina2();
            //doc.InserirSubSubtitulo("");            

            doc.InserirImagens(1,
                Path.Combine(caminhoBase, @"imgs_temp", "GRANDEHist.jpg"),
                Path.Combine(caminhoBase, @"imgs_temp", "GRANDEDetalhe.jpg"));
            doc.InserirImagens(1,
                Path.Combine(caminhoBase, @"imgs_temp", "TIETÊHist.jpg"),
                Path.Combine(caminhoBase, @"imgs_temp", "TIETÊDetalhe.jpg"));
            doc.InserirImagens(1,
                Path.Combine(caminhoBase, @"imgs_temp", "PARANAÍBAHist.jpg"),
                Path.Combine(caminhoBase, @"imgs_temp", "PARANAÍBADetalhe.jpg"));
            doc.InserirImagens(1,
                Path.Combine(caminhoBase, @"imgs_temp", "PARANAPANEMAHist.jpg"),
                Path.Combine(caminhoBase, @"imgs_temp", "PARANAPANEMADetalhe.jpg"));
            doc.InserirImagens(1,
                Path.Combine(caminhoBase, @"imgs_temp", "PARANÁHist.jpg"),
                Path.Combine(caminhoBase, @"imgs_temp", "PARANÁDetalhe.jpg"));
            doc.InserirImagens(1,
                Path.Combine(caminhoBase, @"imgs_temp", "ITAIPUHist.jpg"),
                Path.Combine(caminhoBase, @"imgs_temp", "ITAIPUDetalhe.jpg"));
            doc.InserirImagens(1,
                Path.Combine(caminhoBase, @"imgs_temp", "IGUAÇUHist.jpg"),
                Path.Combine(caminhoBase, @"imgs_temp", "IGUAÇUDetalhe.jpg"));
            doc.InserirImagens(1,
                Path.Combine(caminhoBase, @"imgs_temp", "URUGUAIHist.jpg"),
                Path.Combine(caminhoBase, @"imgs_temp", "URUGUAIDetalhe.jpg"));
            doc.InserirImagens(1,
                Path.Combine(caminhoBase, @"imgs_temp", "TOCANTINSHist.jpg"),
                Path.Combine(caminhoBase, @"imgs_temp", "TOCANTINSDetalhe.jpg"));
            doc.InserirImagens(1,
                Path.Combine(caminhoBase, @"imgs_temp", "SÃO FRANCISCOHist.jpg"),
                Path.Combine(caminhoBase, @"imgs_temp", "SÃO FRANCISCODetalhe.jpg"));


            doc.Close();

            return caminho;
        }

        public static string[] buscaPrevNOAA(string caminho, DateTime datref, string complemento, int cont, string camNOAA, bool preliminar)
        {
            var camConj = Path.Combine(camNOAA, complemento);

            string observadoFunc = Path.Combine(caminho, DateTime.Today.ToString("yyyy_MM_dd"), "OBSERVADO", "funceme.gif");
            string observadoOns = Path.Combine(caminho, DateTime.Today.ToString("yyyy_MM_dd"), "OBSERVADO", "ons.gif");

            var conjunto = Directory.GetFiles(Path.Combine(camNOAA, complemento), "*.gif");

            var tamanho = conjunto.Length > 15 ? 15 : conjunto.Length;// para considerar apenas 15 dias do gefs extendido

            // var imgsConj = new string[conjunto.Length - cont + 1];
            var imgsConj = new string[tamanho - cont + 1];
            if (!preliminar && camConj.Contains("00"))
            {
                imgsConj = new string[conjunto.Length - cont + 2];
            }

            if (datref.Date == DateTime.Today.Date)
            {
                if (!preliminar && camConj.Contains("00"))
                {
                    if (File.Exists(observadoFunc))
                    {
                        imgsConj[0] = (observadoFunc);
                    }
                    else
                    {
                        imgsConj[0] = (observadoOns);
                    }
                    for (int i = 1; i <= imgsConj.Count(); i++)
                    {
                        var arqConj = Path.Combine(camConj, "prev" + i + ".gif");
                        if (File.Exists(arqConj)) imgsConj[i] = (arqConj);
                        if (i == imgsConj.Count()) imgsConj[i - 1] = (@"C:\Files\Relatorios\Relatorio Final\quadBranco.gif");
                    }
                    return imgsConj;
                }
                else
                {
                    if (File.Exists(observadoFunc))
                    {
                        imgsConj[0] = (observadoFunc);
                    }
                    else
                    {
                        imgsConj[0] = (observadoOns);
                    }
                    for (int i = 1; i <= imgsConj.Count(); i++)
                    {
                        var arqConj = Path.Combine(camConj, "prev" + i + ".gif");
                        if (File.Exists(arqConj) && ((i + 1) <= imgsConj.Count()))
                        {
                            imgsConj[i] = (arqConj);
                        }
                    }
                    return imgsConj;
                }

            }
            else
            {
                for (int i = 1; i <= imgsConj.Count(); i++)
                {
                    var arqConj = Path.Combine(camConj, "prev" + i + ".gif");
                    if (File.Exists(arqConj)) imgsConj[i - 1] = (arqConj);
                    if (i == imgsConj.Count()) imgsConj[i - 1] = (@"C:\Files\Relatorios\Relatorio Final\quadBranco.gif");
                }
                return imgsConj;
            }

        }
        public static string[] buscaPrev(string caminho, DateTime datref, string complemento, int cont)
        {
            var camConj = Path.Combine(caminho, datref.ToString("yyyy_MM_dd"), complemento);

            string observadoFunc = Path.Combine(caminho, DateTime.Today.ToString("yyyy_MM_dd"), "OBSERVADO", "funceme.gif");
            string observadoOns = Path.Combine(caminho, DateTime.Today.ToString("yyyy_MM_dd"), "OBSERVADO", "ons.gif");

            var conjunto = Directory.GetFiles(Path.Combine(caminho, datref.ToString("yyyy_MM_dd"), complemento), "*.gif");

            var imgsConj = new string[conjunto.Length - cont + 1];

            if (datref.Date == DateTime.Today.Date)
            {
                if (File.Exists(observadoFunc))
                {
                    imgsConj[0] = (observadoFunc);
                }
                else
                {
                    imgsConj[0] = (observadoOns);
                }
                for (int i = 1; i <= imgsConj.Count(); i++)
                {
                    var arqConj = Path.Combine(camConj, "prev" + i + ".gif");
                    if (File.Exists(arqConj)) imgsConj[i] = (arqConj);
                }
                return imgsConj;
            }
            else
            {
                for (int i = 1; i <= conjunto.Count(); i++)
                {
                    var arqConj = Path.Combine(camConj, "prev" + i + ".gif");
                    if (File.Exists(arqConj)) imgsConj[i - 1] = (arqConj);
                    if (i == imgsConj.Count()) imgsConj[i - 1] = (@"C:\Files\Relatorios\Relatorio Final\quadBranco.gif");
                }
                return imgsConj;
            }

        }

        public static string CriarRelatorioPrevs(DateTime data, string caminho = null, bool preliminar = false)
        {
            // camPrev = Path.Combine(@"C:\Files\Trading\Acompanhamento Metereologico Semanal\spiderman" , DateTime.Now.ToString("yyyy_MM_dd") , "CONJUNTO00PREV");



            DateTime data1 = data.AddDays(-1);
            var camNOAA = Path.Combine(@"B:\Enercore\Energy Core Trading\Energy Core Pricing - Documents\Acompanhamento_de_Precipitacao\Mapas", data.ToString("yyyy"), data.ToString("MM"), data.ToString("dd"));
            var camNOAAOntem = Path.Combine(@"B:\Enercore\Energy Core Trading\Energy Core Pricing - Documents\Acompanhamento_de_Precipitacao\Mapas", data1.ToString("yyyy"), data1.ToString("MM"), data1.ToString("dd"));

            //var camAlterNOAA = Path.Combine(@"B:\Compass\OneDrive - MinhaTI\Compass\Pedro\NOAA", data.ToString("yyyy"), data.ToString("MM"), data.ToString("dd"));
            // var camAlterNOAAOntem = Path.Combine(@"B:\Compass\OneDrive - MinhaTI\Compass\Pedro\NOAA", data1.ToString("yyyy"), data1.ToString("MM"), data1.ToString("dd"));

            string GefsNOAA00 = "GEFS_0.5_00";
            string GfsNOAA00 = "GFS00";
            string GefsNOAA12 = "GEFS_0.5_12";
            string GfsNOAA12 = "GFS12";

            string caminhoSpider = @"C:\Files\Trading\Acompanhamento Metereologico Semanal\spiderman\";
            string conjEta = "CONJUNTO00PREV";
            string camGefs = "GEFS00";
            string camGfs = "GFS00";
            string camECMWFEnsemble = "ECMWF_ONS";
            string camEcwmf = "ECMWF00";
            string camEta40 = "ETA00";

            string camGefs12 = "GEFS12";
            string camGfs12 = "GFS12";
            string camEcwmf12 = "ECMWF12";




            //////////////////////////////////////////////////////////////
            ///////////// CRIAÇÂO DO PDF
            //////////////////////////////////////////////////////////////
            if (preliminar)
            {
                if (Directory.Exists(Path.Combine(camNOAA, GefsNOAA00)) && Directory.Exists(Path.Combine(camNOAA, GfsNOAA00)))
                {
                    var pastPrev00 = Path.Combine(@"C:\Files\Trading\Acompanhamento Metereologico Semanal\spiderman", DateTime.Now.ToString("yyyy_MM_dd"), "CONJUNTO00PREV");

                    int hora = 0;
                    var doc = PdfExtensions.NovoPdfPrevs(caminho, data, hora);

                    var imgsConj = buscaPrevNOAA(caminhoSpider, data, GfsNOAA00, 0, camNOAA, preliminar);
                    var imgsConj1 = buscaPrevNOAA(caminhoSpider, data1, GfsNOAA00, 0, camNOAAOntem, preliminar);

                    if (Directory.Exists(pastPrev00))
                    {
                        doc.InserirParte2("Previsão de precipitação pelos modelos Conjunto (ETA 40 + GEFS + ECMWF Ensemble) dos dias " + data1.ToString("dd/MM/yyyy") + " e " + data.ToString("dd/MM/yyyy"));

                        try
                        {
                            //doc.InserirSubtitulo2("Previsão por Conjunto (ETA 40 + GEFS + ECMWF Ensemble) do dia " + data1.ToString("dd/MM/yyyy"));
                            doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data1, conjEta, 1));
                            doc.InserirMeioEspaco();

                            // doc.InserirSubtitulo2("Previsão por Conjunto (ETA 40 + GEFS + ECMWF Ensemble) do dia " + data.ToString("dd/MM/yyyy"));
                            doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, conjEta, 1));
                        }
                        catch { }

                        //doc.InserirEspaco();
                        doc.InserirMeioEspaco();

                    }

                    #region mapa nao utilizado
                    //doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL ENSEMBLE (GEFS)");

                    //try
                    //{
                    //    doc.InserirSubtitulo2("Previsão por modelo (GEFS) do dia " + data1.ToString("dd/MM/yyyy"));
                    //    doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data1, camGefs, 3));
                    //    doc.InserirSubtitulo2("Previsão por modelo (GEFS) do dia " + data.ToString("dd/MM/yyyy"));
                    //    doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, camGefs, 3));
                    //}
                    //catch { }

                    //doc.InserirEspaco();

                    //doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL OPERATIVO (GFS)");

                    //try
                    //{
                    //    doc.InserirSubtitulo2("Previsão por modelo (GFS) do dia " + data1.ToString("dd/MM/yyyy"));
                    //    doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data1, camGfs, 3));
                    //    doc.InserirSubtitulo2("Previsão por modelo (GFS) do dia " + data.ToString("dd/MM/yyyy"));
                    //    doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, camGfs, 3));
                    //}
                    //catch { }

                    //doc.NovaPagina2();
                    #endregion

                    doc.InserirParte2("Previsão de precipitação pelo modelo EUROPEU (ECMWF Operativo) dos dias " + data1.ToString("dd/MM/yyyy") + " e " + data.ToString("dd/MM/yyyy"));

                    try
                    {
                        // doc.InserirSubtitulo2("Previsão por modelo (ECMWF Operativo) do dia " + data1.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data1, camEcwmf, 3));
                        doc.InserirMeioEspaco();

                        // doc.InserirSubtitulo2("Previsão por modelo (ECMWF Operativo) do dia " + data.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, camEcwmf, 3));
                    }
                    catch { }

                    //doc.InserirEspaco();
                    doc.InserirMeioEspaco();


                    doc.InserirParte2("Previsão de precipitação pelo modelo EUROPEU (ECMWF Ensemble) dos dias " + data1.ToString("dd/MM/yyyy") + " e " + data.ToString("dd/MM/yyyy"));

                    try
                    {
                        //doc.InserirSubtitulo2("Previsão por modelo (ECMWF Ensemble) do dia " + data1.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data1, camECMWFEnsemble, 3));
                        doc.InserirMeioEspaco();

                        //doc.InserirSubtitulo2("Previsão por modelo (ECMWF Ensemble) do dia " + data.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, camECMWFEnsemble, 3));
                    }
                    catch { }

                    // doc.InserirEspaco();
                    //doc.InserirMeioEspaco();
                    doc.NovaPagina2();

                    doc.InserirParte2("Previsão de precipitação pelo modelo REGIONAL (ETA40) dos dias " + data1.ToString("dd/MM/yyyy") + " e " + data.ToString("dd/MM/yyyy"));
                    try
                    {
                        //doc.InserirSubtitulo2("Previsão por  modelo (ETA40) do dia " + data1.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data1, camEta40, 3));
                        doc.InserirMeioEspaco();

                        //doc.InserirSubtitulo2("Previsão por modelo (ETA40) do dia " + data.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, camEta40, 3));
                    }
                    catch { }
                    doc.InserirMeioEspaco();

                    //doc.NovaPagina2();
                    doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL ENSEMBLE (GEFS) NOAA dos dias " + data1.ToString("dd/MM/yyyy") + " e " + data.ToString("dd/MM/yyyy"));

                    try
                    {
                        //doc.InserirSubtitulo2("Previsão por modelo (GEFS) do dia " + data1.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj1 = buscaPrevNOAA(caminhoSpider, data1, GefsNOAA00, 0, camNOAAOntem, preliminar));
                        doc.InserirMeioEspaco();

                        // doc.InserirSubtitulo2("Previsão por modelo (GEFS) do dia " + data.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj = buscaPrevNOAA(caminhoSpider, data, GefsNOAA00, 0, camNOAA, preliminar));
                    }
                    catch { }

                    //doc.InserirEspaco();
                    doc.InserirMeioEspaco();


                    doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL ENSEMBLE (GFS) NOAA dos dias " + data1.ToString("dd/MM/yyyy") + " e " + data.ToString("dd/MM/yyyy"));

                    try
                    {
                        //doc.InserirSubtitulo2("Previsão por modelo (GFS) do dia " + data1.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj1 = buscaPrevNOAA(caminhoSpider, data1, GfsNOAA00, 0, camNOAAOntem, preliminar));
                        doc.InserirMeioEspaco();

                        //doc.InserirSubtitulo2("Previsão por modelo (GFS) do dia " + data.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj = buscaPrevNOAA(caminhoSpider, data, GfsNOAA00, 0, camNOAA, preliminar));
                    }
                    catch { }
                    doc.Close();

                    return caminho;

                }
                #region caminho nao utilizado
                /*else if (Directory.Exists(Path.Combine(camAlterNOAA, GefsNOAA00)) && Directory.Exists(Path.Combine(camAlterNOAA, GfsNOAA00)))
                {
                    var pastPrev00 = Path.Combine(@"C:\Files\Trading\Acompanhamento Metereologico Semanal\spiderman", DateTime.Now.ToString("yyyy_MM_dd"), "CONJUNTO00PREV");

                    int hora = 0;
                    var doc = PdfExtensions.NovoPdfPrevs(caminho, data, hora);

                    var imgsConj = buscaPrevNOAA(caminhoSpider, data, GfsNOAA00, 0, camAlterNOAA, preliminar);
                    var imgsConj1 = buscaPrevNOAA(caminhoSpider, data1, GfsNOAA00, 0, camAlterNOAAOntem, preliminar);

                    if (Directory.Exists(pastPrev00))
                    {
                        doc.InserirParte2("Previsão de precipitação pelos modelos Conjunto (ETA 40 + GEFS + ECMWF Ensemble) dos dias " + data1.ToString("dd/MM/yyyy") + " e " + data.ToString("dd/MM/yyyy"));

                        try
                        {
                            //doc.InserirSubtitulo2("Previsão por Conjunto (ETA 40 + GEFS + ECMWF Ensemble) do dia " + data1.ToString("dd/MM/yyyy"));
                            doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data1, conjEta, 1));
                            doc.InserirMeioEspaco();

                            //doc.InserirSubtitulo2("Previsão por Conjunto (ETA 40 + GEFS + ECMWF Ensemble) do dia " + data.ToString("dd/MM/yyyy"));
                            doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, conjEta, 1));
                        }
                        catch { }

                        // doc.InserirEspaco();
                        doc.InserirMeioEspaco();

                    }


                    //doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL ENSEMBLE (GEFS)");

                    //try
                    //{
                    //    doc.InserirSubtitulo2("Previsão por modelo (GEFS) do dia " + data1.ToString("dd/MM/yyyy"));
                    //    doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data1, camGefs, 3));
                    //    doc.InserirSubtitulo2("Previsão por modelo (GEFS) do dia " + data.ToString("dd/MM/yyyy"));
                    //    doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, camGefs, 3));
                    //}
                    //catch { }

                    //doc.InserirEspaco();

                    //doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL OPERATIVO (GFS)");

                    //try
                    //{
                    //    doc.InserirSubtitulo2("Previsão por modelo (GFS) do dia " + data1.ToString("dd/MM/yyyy"));
                    //    doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data1, camGfs, 3));
                    //    doc.InserirSubtitulo2("Previsão por modelo (GFS) do dia " + data.ToString("dd/MM/yyyy"));
                    //    doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, camGfs, 3));
                    //}
                    //catch { }

                    //doc.NovaPagina2();

                    doc.InserirParte2("Previsão de precipitação pelo modelo EUROPEU (ECMWF Operativo) dos dias " + data1.ToString("dd/MM/yyyy") + " e " + data.ToString("dd/MM/yyyy"));

                    try
                    {
                        //doc.InserirSubtitulo2("Previsão por modelo (ECMWF Operativo) do dia " + data1.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data1, camEcwmf, 3));
                        doc.InserirMeioEspaco();

                        //doc.InserirSubtitulo2("Previsão por modelo (ECMWF Operativo) do dia " + data.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, camEcwmf, 3));
                    }
                    catch { }

                    //doc.InserirEspaco();
                    doc.InserirMeioEspaco();


                    doc.InserirParte2("Previsão de precipitação pelo modelo EUROPEU (ECMWF Ensemble) dos dias " + data1.ToString("dd/MM/yyyy") + " e " + data.ToString("dd/MM/yyyy"));

                    try
                    {
                        // doc.InserirSubtitulo2("Previsão por modelo (ECMWF Ensemble) do dia " + data1.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data1, camECMWFEnsemble, 3));
                        doc.InserirMeioEspaco();

                        // doc.InserirSubtitulo2("Previsão por modelo (ECMWF Ensemble) do dia " + data.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, camECMWFEnsemble, 3));
                    }
                    catch { }

                    //doc.InserirEspaco();
                    doc.NovaPagina2();
                    doc.InserirParte2("Previsão de precipitação pelo modelo REGIONAL (ETA40) dos dias " + data1.ToString("dd/MM/yyyy") + " e " + data.ToString("dd/MM/yyyy"));
                    try
                    {
                        //doc.InserirSubtitulo2("Previsão por  modelo (ETA40) do dia " + data1.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data1, camEta40, 3));
                        doc.InserirMeioEspaco();

                        //doc.InserirSubtitulo2("Previsão por modelo (ETA40) do dia " + data.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, camEta40, 3));
                    }
                    catch { }

                    // doc.NovaPagina2();

                    doc.InserirMeioEspaco();

                    doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL ENSEMBLE (GEFS) NOAA dos dias " + data1.ToString("dd/MM/yyyy") + " e " + data.ToString("dd/MM/yyyy"));

                    try
                    {
                        //doc.InserirSubtitulo2("Previsão por modelo (GEFS) do dia " + data1.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj1 = buscaPrevNOAA(caminhoSpider, data1, GefsNOAA00, 0, camAlterNOAAOntem, preliminar));
                        doc.InserirMeioEspaco();

                        //doc.InserirSubtitulo2("Previsão por modelo (GEFS) do dia " + data.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj = buscaPrevNOAA(caminhoSpider, data, GefsNOAA00, 0, camAlterNOAA, preliminar));
                    }
                    catch { }

                    //doc.InserirEspaco();
                    doc.InserirMeioEspaco();


                    doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL ENSEMBLE (GFS) NOAA dos dias " + data1.ToString("dd/MM/yyyy") + " e " + data.ToString("dd/MM/yyyy"));

                    try
                    {
                        //doc.InserirSubtitulo2("Previsão por modelo (GFS) do dia " + data1.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj1 = buscaPrevNOAA(caminhoSpider, data1, GfsNOAA00, 0, camAlterNOAAOntem, preliminar));
                        doc.InserirMeioEspaco();

                        // doc.InserirSubtitulo2("Previsão por modelo (GFS) do dia " + data.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj = buscaPrevNOAA(caminhoSpider, data, GfsNOAA00, 0, camAlterNOAA, preliminar));
                    }
                    catch { }
                    doc.Close();

                    return caminho;
                }*/
                #endregion
            }
            else
            {
                if (Directory.Exists(Path.Combine(camNOAA, GefsNOAA12)) && Directory.Exists(Path.Combine(camNOAA, GfsNOAA12)))
                {
                    var qtdGefsNOAA12 = Directory.GetFiles(Path.Combine(camNOAA, GefsNOAA12), "*.gif").ToList();
                    var qtdGfsNOAA12 = Directory.GetFiles(Path.Combine(camNOAA, GfsNOAA12), "*.gif").ToList();

                    if (qtdGefsNOAA12.Count() >= 15 && qtdGfsNOAA12.Count >= 15)
                    {
                        int hora = 12;
                        var doc = PdfExtensions.NovoPdfPrevs(caminho, data, hora);



                        var imgsConj = buscaPrevNOAA(caminhoSpider, data, GefsNOAA12, 0, camNOAA, preliminar);
                        var imgsConj1 = buscaPrevNOAA(caminhoSpider, data, GefsNOAA00, 0, camNOAA, preliminar);

                        #region mapa nao utlizado
                        //doc.InserirParte2("Previsão de precipitação pelos modelos Conjunto (ETA 40+GEFS)");

                        //try
                        //{
                        //    doc.InserirSubtitulo2("Previsão por Conjunto (ETA40+GEFS) do dia " + data1.ToString("dd/MM/yyyy"));
                        //    doc.InserirImagens(1, imgsConj1);
                        //    doc.InserirSubtitulo2("Previsão por Conjunto (ETA40+GEFS) do dia " + data.ToString("dd/MM/yyyy"));
                        //    doc.InserirImagens(1, imgsConj);
                        //}
                        //catch { }

                        //doc.InserirEspaco();

                        //doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL ENSEMBLE (GEFS)");

                        //try
                        //{


                        //    doc.InserirSubtitulo2("Previsão por modelo (GEFS) 00z do dia " + data.ToString("dd/MM/yyyy"));
                        //    doc.InserirImagens(1, imgsConj1);
                        //    doc.InserirSubtitulo2("Previsão por modelo (GEFS) 12z do dia " + data.ToString("dd/MM/yyyy"));
                        //    doc.InserirImagens(1, imgsConj);
                        //}
                        //catch { }

                        //doc.InserirEspaco();

                        //doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL OPERATIVO (GFS)");

                        //try
                        //{
                        //    doc.InserirSubtitulo2("Previsão por modelo (GFS) 00z do dia " + data.ToString("dd/MM/yyyy"));
                        //    doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data, camGfs, 3));
                        //    doc.InserirSubtitulo2("Previsão por modelo (GFS) 12z do dia " + data.ToString("dd/MM/yyyy"));
                        //    doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, camGfs12, 3));
                        //}
                        //catch { }

                        //doc.NovaPagina2();

                        //doc.InserirParte2("Previsão de precipitação pelo modelo EUROPEU (ECMWF)");

                        //try
                        //{
                        //    doc.InserirSubtitulo2("Previsão por modelo (ECMWF) 00z do dia " + data.ToString("dd/MM/yyyy"));
                        //    doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data, camEcwmf, 3));
                        //    doc.InserirSubtitulo2("Previsão por modelo (ECMWF) 12z do dia " + data.ToString("dd/MM/yyyy"));
                        //    doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, camEcwmf12, 3));
                        //}
                        //catch { }

                        //doc.InserirEspaco();

                        //doc.InserirParte2("Previsão de precipitação pelo modelo REGIONAL (ETA40)");
                        //try
                        //{
                        //    doc.InserirSubtitulo2("Previsão por  modelo (ETA40) do dia " + data1.ToString("dd/MM/yyyy"));
                        //    doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data1, camEta40, 3));
                        //    doc.InserirSubtitulo2("Previsão por modelo (ETA40) do dia " + data.ToString("dd/MM/yyyy"));
                        //    doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, camEta40, 3));
                        //}
                        //catch { }

                        //doc.NovaPagina2();
                        #endregion

                        doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL ENSEMBLE (GEFS) NOAA 00z e 12z do dia " + data.ToString("dd/MM/yyyy"));

                        try
                        {
                            // doc.InserirSubtitulo2("Previsão por modelo (GEFS) 00z do dia " + data.ToString("dd/MM/yyyy"));
                            doc.InserirImagens(1, imgsConj1 = buscaPrevNOAA(caminhoSpider, data, GefsNOAA00, 0, camNOAA, preliminar));
                            doc.InserirMeioEspaco();

                            //doc.InserirSubtitulo2("Previsão por modelo (GEFS) 12z do dia " + data.ToString("dd/MM/yyyy"));
                            doc.InserirImagens(1, imgsConj = buscaPrevNOAA(caminhoSpider, data, GefsNOAA12, 0, camNOAA, preliminar));
                        }
                        catch { }

                        doc.InserirMeioEspaco();
                        //doc.InserirEspaco();

                        doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL ENSEMBLE (GFS) NOAA 00z e 12z do dia " + data.ToString("dd/MM/yyyy"));

                        try
                        {
                            //doc.InserirSubtitulo2("Previsão por modelo (GFS) 00z do dia " + data.ToString("dd/MM/yyyy"));
                            doc.InserirImagens(1, imgsConj1 = buscaPrevNOAA(caminhoSpider, data, GfsNOAA00, 0, camNOAA, preliminar));
                            doc.InserirMeioEspaco();

                            //doc.InserirSubtitulo2("Previsão por modelo (GFS) 12z do dia " + data.ToString("dd/MM/yyyy"));
                            doc.InserirImagens(1, imgsConj = buscaPrevNOAA(caminhoSpider, data, GfsNOAA12, 0, camNOAA, preliminar));
                        }
                        catch { }
                        doc.Close();

                        return caminho;
                    }
                    

                }
                #region caminho nao utilizado
                /*else if (Directory.Exists(Path.Combine(camAlterNOAA, GefsNOAA12)) && Directory.Exists(Path.Combine(camAlterNOAA, GfsNOAA12)) && Directory.Exists(Path.Combine(caminhoSpider, data.ToString("yyyy_MM_dd"), camGfs12)) && Directory.Exists(Path.Combine(caminhoSpider, data.ToString("yyyy_MM_dd"), camGefs12)))
                {
                    int hora = 12;
                    var doc = PdfExtensions.NovoPdfPrevs(caminho, data, hora);


                    var imgsConj = buscaPrev(caminhoSpider, data, camGefs12, 3);
                    var imgsConj1 = buscaPrev(caminhoSpider, data, camGefs, 3);

                    //doc.InserirParte2("Previsão de precipitação pelos modelos Conjunto (ETA 40+GEFS)");

                    //try
                    //{
                    //    doc.InserirSubtitulo2("Previsão por Conjunto (ETA40+GEFS) do dia " + data1.ToString("dd/MM/yyyy"));
                    //    doc.InserirImagens(1, imgsConj1);
                    //    doc.InserirSubtitulo2("Previsão por Conjunto (ETA40+GEFS) do dia " + data.ToString("dd/MM/yyyy"));
                    //    doc.InserirImagens(1, imgsConj);
                    //}
                    //catch { }

                    //doc.InserirEspaco();

                    //doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL ENSEMBLE (GEFS)");

                    //try
                    //{


                    //    doc.InserirSubtitulo2("Previsão por modelo (GEFS) 00z do dia " + data.ToString("dd/MM/yyyy"));
                    //    doc.InserirImagens(1, imgsConj1);
                    //    doc.InserirSubtitulo2("Previsão por modelo (GEFS) 12z do dia " + data.ToString("dd/MM/yyyy"));
                    //    doc.InserirImagens(1, imgsConj);
                    //}
                    //catch { }

                    //doc.InserirEspaco();

                    //doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL OPERATIVO (GFS)");

                    //try
                    //{
                    //    doc.InserirSubtitulo2("Previsão por modelo (GFS) 00z do dia " + data.ToString("dd/MM/yyyy"));
                    //    doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data, camGfs, 3));
                    //    doc.InserirSubtitulo2("Previsão por modelo (GFS) 12z do dia " + data.ToString("dd/MM/yyyy"));
                    //    doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, camGfs12, 3));
                    //}
                    //catch { }

                    //doc.NovaPagina2();

                    //doc.InserirParte2("Previsão de precipitação pelo modelo EUROPEU (ECMWF)");

                    //try
                    //{
                    //    doc.InserirSubtitulo2("Previsão por modelo (ECMWF) 00z do dia " + data.ToString("dd/MM/yyyy"));
                    //    doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data, camEcwmf, 3));
                    //    doc.InserirSubtitulo2("Previsão por modelo (ECMWF) 12z do dia " + data.ToString("dd/MM/yyyy"));
                    //    doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, camEcwmf12, 3));
                    //}
                    //catch { }

                    //doc.InserirEspaco();

                    //doc.InserirParte2("Previsão de precipitação pelo modelo REGIONAL (ETA40)");
                    //try
                    //{
                    //    doc.InserirSubtitulo2("Previsão por  modelo (ETA40) do dia " + data1.ToString("dd/MM/yyyy"));
                    //    doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data1, camEta40, 3));
                    //    doc.InserirSubtitulo2("Previsão por modelo (ETA40) do dia " + data.ToString("dd/MM/yyyy"));
                    //    doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, camEta40, 3));
                    //}
                    //catch { }

                    // doc.NovaPagina2();
                    doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL ENSEMBLE (GEFS) NOAA 00z e 12z do dia " + data.ToString("dd/MM/yyyy"));

                    try
                    {
                        //doc.InserirSubtitulo2("Previsão por modelo (GEFS) 00z do dia " + data.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj1 = buscaPrevNOAA(caminhoSpider, data, GefsNOAA00, 0, camAlterNOAA, preliminar));
                        doc.InserirMeioEspaco();

                        //doc.InserirSubtitulo2("Previsão por modelo (GEFS) 12z do dia " + data.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj = buscaPrevNOAA(caminhoSpider, data, GefsNOAA12, 0, camAlterNOAA, preliminar));
                    }
                    catch { }

                    //doc.InserirEspaco();
                    doc.InserirMeioEspaco();


                    doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL ENSEMBLE (GFS) NOAA 00z e 12z do dia " + data.ToString("dd/MM/yyyy"));

                    try
                    {
                        //doc.InserirSubtitulo2("Previsão por modelo (GFS) 00z do dia " + data.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj1 = buscaPrevNOAA(caminhoSpider, data, GfsNOAA00, 0, camAlterNOAA, preliminar));
                        doc.InserirMeioEspaco();

                        // doc.InserirSubtitulo2("Previsão por modelo (GFS) 12z do dia " + data.ToString("dd/MM/yyyy"));
                        doc.InserirImagens(1, imgsConj = buscaPrevNOAA(caminhoSpider, data, GfsNOAA12, 0, camAlterNOAA, preliminar));
                    }
                    catch { }
                    doc.Close();

                    return caminho;
                }*/
                #endregion
            }

            return "";
        }

        public static string CriarRelatorioPrevs2(DateTime data, string caminho = null, bool preliminar = false)
        {
            // camPrev = Path.Combine(@"C:\Files\Trading\Acompanhamento Metereologico Semanal\spiderman" , DateTime.Now.ToString("yyyy_MM_dd") , "CONJUNTO00PREV");



            DateTime data1 = data.AddDays(-1);
            var camNOAA = Path.Combine(@"B:\Enercore\Energy Core Trading\Energy Core Pricing - Documents\Acompanhamento_de_Precipitacao\Mapas", data.ToString("yyyy"), data.ToString("MM"), data.ToString("dd"));
            var camNOAAOntem = Path.Combine(@"B:\Enercore\Energy Core Trading\Energy Core Pricing - Documents\Acompanhamento_de_Precipitacao\Mapas", data1.ToString("yyyy"), data1.ToString("MM"), data1.ToString("dd"));

            // var camAlterNOAA = Path.Combine(@"B:\Compass\OneDrive - MinhaTI\Compass\Pedro\NOAA", data.ToString("yyyy"), data.ToString("MM"), data.ToString("dd"));
            //var camAlterNOAAOntem = Path.Combine(@"B:\Compass\OneDrive - MinhaTI\Compass\Pedro\NOAA", data1.ToString("yyyy"), data1.ToString("MM"), data1.ToString("dd"));

            string GefsNOAA00 = "GEFS_0.5_00";
            string GfsNOAA00 = "GFS00";
            string GefsNOAA12 = "GEFS_0.5_12";
            string GfsNOAA12 = "GFS12";

            string caminhoSpider = @"C:\Files\Trading\Acompanhamento Metereologico Semanal\spiderman\";
            string conjEta = "CONJUNTO00PREV";
            string camGefs = "GEFS00";
            string camGfs = "GFS00";
            string camEcwmf = "ECMWF00";
            string camEta40 = "ETA00";

            string camGefs12 = "GEFS12";
            string camGfs12 = "GFS12";
            string camEcwmf12 = "ECMWF12";




            //////////////////////////////////////////////////////////////
            ///////////// CRIAÇÂO DO PDF
            //////////////////////////////////////////////////////////////



            if (Directory.Exists(Path.Combine(camNOAA, GefsNOAA12)) && Directory.Exists(Path.Combine(camNOAA, GfsNOAA12)) && Directory.Exists(Path.Combine(caminhoSpider, data.ToString("yyyy_MM_dd"), camEcwmf12)))
            {



                int hora = 12;
                var doc = PdfExtensions.NovoPdfPrevs(caminho, data, hora);



                var imgsConj = buscaPrev(caminhoSpider, data, camEcwmf12, 3);
                var imgsConj1 = buscaPrev(caminhoSpider, data, camEcwmf, 3);

                #region mapas nao utilizados
                /*&& Directory.Exists(Path.Combine(caminhoSpider, data.ToString("yyyy_MM_dd"), camGefs12))*/
                /* && Directory.Exists(Path.Combine(caminhoSpider, data.ToString("yyyy_MM_dd"), camGfs12))*/
                //doc.InserirParte2("Previsão de precipitação pelos modelos Conjunto (ETA 40+GEFS)");

                //try
                //{
                //    doc.InserirSubtitulo2("Previsão por Conjunto (ETA40+GEFS) do dia " + data1.ToString("dd/MM/yyyy"));
                //    doc.InserirImagens(1, imgsConj1);
                //    doc.InserirSubtitulo2("Previsão por Conjunto (ETA40+GEFS) do dia " + data.ToString("dd/MM/yyyy"));
                //    doc.InserirImagens(1, imgsConj);
                //}
                //catch { }

                //doc.InserirEspaco();

                //doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL ENSEMBLE (GEFS)");

                //try
                //{


                //    doc.InserirSubtitulo2("Previsão por modelo (GEFS) 00z do dia " + data.ToString("dd/MM/yyyy"));
                //    doc.InserirImagens(1, imgsConj1);
                //    doc.InserirSubtitulo2("Previsão por modelo (GEFS) 12z do dia " + data.ToString("dd/MM/yyyy"));
                //    doc.InserirImagens(1, imgsConj);
                //}
                //catch { }

                //doc.InserirEspaco();

                //doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL OPERATIVO (GFS)");

                //try
                //{
                //    doc.InserirSubtitulo2("Previsão por modelo (GFS) 00z do dia " + data.ToString("dd/MM/yyyy"));
                //    doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data, camGfs, 3));
                //    doc.InserirSubtitulo2("Previsão por modelo (GFS) 12z do dia " + data.ToString("dd/MM/yyyy"));
                //    doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, camGfs12, 3));
                //}
                //catch { }

                //doc.NovaPagina2();
                #endregion

                doc.InserirParte2("Previsão de precipitação pelo modelo EUROPEU (ECMWF) 00z e 12z do dia " + data.ToString("dd/MM/yyyy"));

                try
                {
                    //doc.InserirSubtitulo2("Previsão por modelo (ECMWF) 00z do dia " + data.ToString("dd/MM/yyyy"));
                    doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data, camEcwmf, 3));
                    doc.InserirMeioEspaco();

                    //doc.InserirSubtitulo2("Previsão por modelo (ECMWF) 12z do dia " + data.ToString("dd/MM/yyyy"));
                    doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, camEcwmf12, 3));
                }
                catch { }

                //doc.InserirEspaco();
                doc.InserirMeioEspaco();
                #region mapas nao utilizados
                //doc.InserirParte2("Previsão de precipitação pelo modelo REGIONAL (ETA40)");
                //try
                //{
                //    doc.InserirSubtitulo2("Previsão por  modelo (ETA40) do dia " + data1.ToString("dd/MM/yyyy"));
                //    doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data1, camEta40, 3));
                //    doc.InserirSubtitulo2("Previsão por modelo (ETA40) do dia " + data.ToString("dd/MM/yyyy"));
                //    doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, camEta40, 3));
                //}
                //catch { }

                //doc.NovaPagina2();
                #endregion

                doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL ENSEMBLE (GEFS) NOAA 00z e 12z do dia " + data.ToString("dd/MM/yyyy"));

                try
                {
                    // doc.InserirSubtitulo2("Previsão por modelo (GEFS) 00z do dia " + data.ToString("dd/MM/yyyy"));
                    doc.InserirImagens(1, imgsConj1 = buscaPrevNOAA(caminhoSpider, data, GefsNOAA00, 0, camNOAA, preliminar));
                    doc.InserirMeioEspaco();

                    //doc.InserirSubtitulo2("Previsão por modelo (GEFS) 12z do dia " + data.ToString("dd/MM/yyyy"));
                    doc.InserirImagens(1, imgsConj = buscaPrevNOAA(caminhoSpider, data, GefsNOAA12, 0, camNOAA, preliminar));
                }
                catch { }
                doc.InserirMeioEspaco();

                //doc.InserirEspaco();
                //doc.NovaPagina2();


                doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL ENSEMBLE (GFS) NOAA 00z e 12z do dia " + data.ToString("dd/MM/yyyy"));

                try
                {
                    //doc.InserirSubtitulo2("Previsão por modelo (GFS) 00z do dia " + data.ToString("dd/MM/yyyy"));
                    doc.InserirImagens(1, imgsConj1 = buscaPrevNOAA(caminhoSpider, data, GfsNOAA00, 0, camNOAA, preliminar));
                    doc.InserirMeioEspaco();

                    // doc.InserirSubtitulo2("Previsão por modelo (GFS) 12z do dia " + data.ToString("dd/MM/yyyy"));
                    doc.InserirImagens(1, imgsConj = buscaPrevNOAA(caminhoSpider, data, GfsNOAA12, 0, camNOAA, preliminar));
                }
                catch (Exception e) { }
                doc.Close();

                return caminho;

            }

            #region caminho nao utilizado
            /*else if (Directory.Exists(Path.Combine(camAlterNOAA, GefsNOAA12)) && Directory.Exists(Path.Combine(camAlterNOAA, GfsNOAA12)) && Directory.Exists(Path.Combine(caminhoSpider, data.ToString("yyyy_MM_dd"), camGfs12)) && Directory.Exists(Path.Combine(caminhoSpider, data.ToString("yyyy_MM_dd"), camEcwmf12)) && Directory.Exists(Path.Combine(caminhoSpider, data.ToString("yyyy_MM_dd"), camGefs12)))
            {
                int hora = 12;
                var doc = PdfExtensions.NovoPdfPrevs(caminho, data, hora);


                var imgsConj = buscaPrev(caminhoSpider, data, camGefs12, 3);
                var imgsConj1 = buscaPrev(caminhoSpider, data, camGefs, 3);

                //doc.InserirParte2("Previsão de precipitação pelos modelos Conjunto (ETA 40+GEFS)");

                //try
                //{
                //    doc.InserirSubtitulo2("Previsão por Conjunto (ETA40+GEFS) do dia " + data1.ToString("dd/MM/yyyy"));
                //    doc.InserirImagens(1, imgsConj1);
                //    doc.InserirSubtitulo2("Previsão por Conjunto (ETA40+GEFS) do dia " + data.ToString("dd/MM/yyyy"));
                //    doc.InserirImagens(1, imgsConj);
                //}
                //catch { }

                //doc.InserirEspaco();

                //doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL ENSEMBLE (GEFS)");

                //try
                //{


                //    doc.InserirSubtitulo2("Previsão por modelo (GEFS) 00z do dia " + data.ToString("dd/MM/yyyy"));
                //    doc.InserirImagens(1, imgsConj1);
                //    doc.InserirSubtitulo2("Previsão por modelo (GEFS) 12z do dia " + data.ToString("dd/MM/yyyy"));
                //    doc.InserirImagens(1, imgsConj);
                //}
                //catch { }

                //doc.InserirEspaco();

                //doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL OPERATIVO (GFS)");

                //try
                //{
                //    doc.InserirSubtitulo2("Previsão por modelo (GFS) 00z do dia " + data.ToString("dd/MM/yyyy"));
                //    doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data, camGfs, 3));
                //    doc.InserirSubtitulo2("Previsão por modelo (GFS) 12z do dia " + data.ToString("dd/MM/yyyy"));
                //    doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, camGfs12, 3));
                //}
                //catch { }

                //doc.NovaPagina2();

                doc.InserirParte2("Previsão de precipitação pelo modelo EUROPEU (ECMWF) 00z e 12z do dia " + data.ToString("dd/MM/yyyy"));

                try
                {
                    //doc.InserirSubtitulo2("Previsão por modelo (ECMWF) 00z do dia " + data.ToString("dd/MM/yyyy"));
                    doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data, camEcwmf, 3));
                    doc.InserirMeioEspaco();

                    //doc.InserirSubtitulo2("Previsão por modelo (ECMWF) 12z do dia " + data.ToString("dd/MM/yyyy"));
                    doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, camEcwmf12, 3));
                }
                catch { }

                //doc.InserirEspaco();
                doc.InserirMeioEspaco();

                //doc.InserirParte2("Previsão de precipitação pelo modelo REGIONAL (ETA40)");
                //try
                //{
                //    doc.InserirSubtitulo2("Previsão por  modelo (ETA40) do dia " + data1.ToString("dd/MM/yyyy"));
                //    doc.InserirImagens(1, imgsConj1 = buscaPrev(caminhoSpider, data1, camEta40, 3));
                //    doc.InserirSubtitulo2("Previsão por modelo (ETA40) do dia " + data.ToString("dd/MM/yyyy"));
                //    doc.InserirImagens(1, imgsConj = buscaPrev(caminhoSpider, data, camEta40, 3));
                //}
                //catch { }

                //doc.NovaPagina2();
                doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL ENSEMBLE (GEFS) NOAA 00z e 12z do dia " + data.ToString("dd/MM/yyyy"));

                try
                {
                    //doc.InserirSubtitulo2("Previsão por modelo (GEFS) 00z do dia " + data.ToString("dd/MM/yyyy"));
                    doc.InserirImagens(1, imgsConj1 = buscaPrevNOAA(caminhoSpider, data, GefsNOAA00, 0, camAlterNOAA, preliminar));
                    doc.InserirMeioEspaco();

                    //doc.InserirSubtitulo2("Previsão por modelo (GEFS) 12z do dia " + data.ToString("dd/MM/yyyy"));
                    doc.InserirImagens(1, imgsConj = buscaPrevNOAA(caminhoSpider, data, GefsNOAA12, 0, camAlterNOAA, preliminar));
                }
                catch { }

                //doc.InserirEspaco();
                doc.InserirMeioEspaco();
                doc.InserirParte2("Previsão de precipitação pelo modelo GLOBAL ENSEMBLE (GFS) NOAA 00z e 12z do dia " + data.ToString("dd/MM/yyyy"));

                try
                {
                    //doc.InserirSubtitulo2("Previsão por modelo (GFS) 00z do dia " + data.ToString("dd/MM/yyyy"));
                    doc.InserirImagens(1, imgsConj1 = buscaPrevNOAA(caminhoSpider, data, GfsNOAA00, 0, camAlterNOAA, preliminar));
                    doc.InserirMeioEspaco();

                    //doc.InserirSubtitulo2("Previsão por modelo (GFS) 12z do dia " + data.ToString("dd/MM/yyyy"));
                    doc.InserirImagens(1, imgsConj = buscaPrevNOAA(caminhoSpider, data, GfsNOAA12, 0, camAlterNOAA, preliminar));
                }
                catch { }
                doc.Close();

                return caminho;
            }*/
            #endregion


            return "";
        }
        private static void CriarImagemChuvas(DateTime data, DateTime data1, Tuple<DateTime, DateTime> semana0, Tuple<DateTime, DateTime> semana1, Tuple<DateTime, DateTime> semana2)
        {
            var caminhoMerge = @"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Observado_MERGE";
            var caminhoFunceme = @"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Observado_Funceme";
            var caminhoPrevisao = @"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Previsao_Numerica";

            #region imgconj
            ///criar imagens
            Func<DateTime, List<List<string>>> createImgsConj = new Func<DateTime, List<List<string>>>(dataRef =>
            {
                var imgs0 = new List<string>();

                //order = Merge - Funceme - Conjunto2w                                          
                for (DateTime dt = semana0.Item1; dt <= semana0.Item2; dt = dt.AddDays(1))
                {
                    var fmerge = Path.Combine(caminhoMerge, dt.ToString("yyyy"), dt.ToString("MM"), "prec_" + dt.ToString("yyyyMMdd") + ".ctl");
                    if (File.Exists(fmerge)) imgs0.Add(fmerge);
                    else
                    {
                        var ffunceme = Path.Combine(caminhoFunceme, dt.ToString("yyyy"), dt.ToString("MM"), "funceme_" + dt.ToString("yyyyMMdd") + ".ctl");
                        if (File.Exists(ffunceme)) imgs0.Add(ffunceme);
                        else
                        {

                            //CONJUNTO2W00 / GEFSVIES2W00
                            var fprev = Path.Combine(caminhoPrevisao, dataRef.ToString("yyyyMM"), dataRef.ToString("dd"), "CONJUNTO2W00", "pp" + dataRef.ToString("yyyyMMdd") + "_"
                                + ((dt - dataRef).TotalHours + 12).ToString("0000") + ".ctl");
                            if (File.Exists(fprev)) imgs0.Add(fprev);
                            else
                            {
                                fprev = Path.Combine(caminhoPrevisao, dataRef.ToString("yyyyMM"), dataRef.ToString("dd"), "GEFSVIES2W00", "pp" + dataRef.ToString("yyyyMMdd") + "_"
                                + ((dt - dataRef).TotalHours + 12).ToString("0000") + ".ctl");
                                if (File.Exists(fprev)) imgs0.Add(fprev);
                                else
                                    throw new Exception("Chuva não exitente para criar relatório");
                            }
                        }
                    }
                }

                GradsHelper.Grads.CreateImgFromFiles(imgs0, "Obser + Prev Conj em  " + dataRef.ToString("dd/MM"), semana0.Item1.ToString("dd/MM") + " e " + semana0.Item2.ToString("dd/MM"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana0_conj_" + dataRef.ToString("yyyyMMdd") + ".gif"),
                    @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgFromFiles.gs");

                var imgs1 = new List<string>();
                for (DateTime dt = semana1.Item1; dt <= semana1.Item2; dt = dt.AddDays(1))
                {

                    var fprev = Path.Combine(caminhoPrevisao, dataRef.ToString("yyyyMM"), dataRef.ToString("dd"), "CONJUNTO2W00", "pp" + dataRef.ToString("yyyyMMdd") + "_"
                        + ((dt - dataRef).TotalHours + 12).ToString("0000") + ".ctl");
                    if (File.Exists(fprev)) imgs1.Add(fprev);
                    else
                    {
                        fprev = Path.Combine(caminhoPrevisao, dataRef.ToString("yyyyMM"), dataRef.ToString("dd"), "GEFSVIES2W00", "pp" + dataRef.ToString("yyyyMMdd") + "_"
                        + ((dt - dataRef).TotalHours + 12).ToString("0000") + ".ctl");
                        if (File.Exists(fprev)) imgs0.Add(fprev);
                        else
                            throw new Exception("Chuva não exitente para criar relatório");
                    }
                }

                GradsHelper.Grads.CreateImgFromFiles(imgs1, "Prev Conj em  " + dataRef.ToString("dd/MM"), semana1.Item1.ToString("dd/MM") + " e " + semana1.Item2.ToString("dd/MM"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana1_conj_" + dataRef.ToString("yyyyMMdd") + ".gif"),
                    @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgFromFiles.gs");

                var imgs2 = new List<string>();
                for (DateTime dt = semana2.Item1; dt <= semana2.Item2; dt = dt.AddDays(1))
                {

                    var fprev = Path.Combine(caminhoPrevisao, dataRef.ToString("yyyyMM"), dataRef.ToString("dd"), "CONJUNTO2W00", "pp" + dataRef.ToString("yyyyMMdd") + "_"
                        + ((dt - dataRef).TotalHours + 12).ToString("0000") + ".ctl");
                    if (File.Exists(fprev)) imgs2.Add(fprev);
                    else
                    {
                        fprev = Path.Combine(caminhoPrevisao, dataRef.ToString("yyyyMM"), dataRef.ToString("dd"), "GEFSVIES2W00", "pp" + dataRef.ToString("yyyyMMdd") + "_"
                        + ((dt - dataRef).TotalHours + 12).ToString("0000") + ".ctl");
                        if (File.Exists(fprev)) imgs0.Add(fprev);
                        else
                            throw new Exception("Chuva não exitente para criar relatório");
                    }
                }

                GradsHelper.Grads.CreateImgFromFiles(imgs2, "Prev Conj em  " + dataRef.ToString("dd/MM"), semana2.Item1.ToString("dd/MM") + " e " + semana2.Item2.ToString("dd/MM"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana2_conj_" + dataRef.ToString("yyyyMMdd") + ".gif"),
                    @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgFromFiles.gs");


                var ret = new List<List<string>>();
                ret.Add(imgs0);
                ret.Add(imgs1);
                ret.Add(imgs2);

                return ret;

            });
            #endregion

            #region imgConjvies
            Func<DateTime, List<List<string>>> createImgsConjVies = new Func<DateTime, List<List<string>>>(dataRef =>
            {
                var imgs0 = new List<string>();

                //order = Merge - Funceme - Conjunto2w                                          
                for (DateTime dt = semana0.Item1; dt <= semana0.Item2; dt = dt.AddDays(1))
                {
                    var fmerge = Path.Combine(caminhoMerge, dt.ToString("yyyy"), dt.ToString("MM"), "prec_" + dt.ToString("yyyyMMdd") + ".ctl");
                    if (File.Exists(fmerge)) imgs0.Add(fmerge);
                    else
                    {
                        var ffunceme = Path.Combine(caminhoFunceme, dt.ToString("yyyy"), dt.ToString("MM"), "funceme_" + dt.ToString("yyyyMMdd") + ".ctl");
                        if (File.Exists(ffunceme)) imgs0.Add(ffunceme);
                        else
                        {

                            //CONJUNTO2W00 / GEFSVIES2W00
                            var fprev = Path.Combine(caminhoPrevisao, dataRef.ToString("yyyyMM"), dataRef.ToString("dd"), "CONJUNTO2W_COMVIES_00", "pp" + dataRef.ToString("yyyyMMdd") + "_"
                                + ((dt - dataRef).TotalHours + 12).ToString("0000") + ".ctl");
                            if (File.Exists(fprev)) imgs0.Add(fprev);

                        }
                    }
                }

                GradsHelper.Grads.CreateImgFromFiles(imgs0, "Obser + Prev Conj em  " + dataRef.ToString("dd/MM"), semana0.Item1.ToString("dd/MM") + " e " + semana0.Item2.ToString("dd/MM"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana0_conjvies_" + dataRef.ToString("yyyyMMdd") + ".gif"),
                    @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgFromFiles.gs");

                var imgs1 = new List<string>();
                for (DateTime dt = semana1.Item1; dt <= semana1.Item2; dt = dt.AddDays(1))
                {

                    var fprev = Path.Combine(caminhoPrevisao, dataRef.ToString("yyyyMM"), dataRef.ToString("dd"), "CONJUNTO2W_COMVIES_00", "pp" + dataRef.ToString("yyyyMMdd") + "_"
                        + ((dt - dataRef).TotalHours + 12).ToString("0000") + ".ctl");
                    if (File.Exists(fprev)) imgs1.Add(fprev);
                }

                GradsHelper.Grads.CreateImgFromFiles(imgs1, "Prev Conj em  " + dataRef.ToString("dd/MM"), semana1.Item1.ToString("dd/MM") + " e " + semana1.Item2.ToString("dd/MM"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana1_conjvies_" + dataRef.ToString("yyyyMMdd") + ".gif"),
                    @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgFromFiles.gs");

                var imgs2 = new List<string>();
                for (DateTime dt = semana2.Item1; dt <= semana2.Item2; dt = dt.AddDays(1))
                {

                    var fprev = Path.Combine(caminhoPrevisao, dataRef.ToString("yyyyMM"), dataRef.ToString("dd"), "CONJUNTO2W_COMVIES_00", "pp" + dataRef.ToString("yyyyMMdd") + "_"
                        + ((dt - dataRef).TotalHours + 12).ToString("0000") + ".ctl");
                    if (File.Exists(fprev)) imgs2.Add(fprev);

                }

                GradsHelper.Grads.CreateImgFromFiles(imgs2, "Prev Conj em  " + dataRef.ToString("dd/MM"), semana2.Item1.ToString("dd/MM") + " e " + semana2.Item2.ToString("dd/MM"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana2_conjvies_" + dataRef.ToString("yyyyMMdd") + ".gif"),
                    @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgFromFiles.gs");


                var ret = new List<List<string>>();
                ret.Add(imgs0);
                ret.Add(imgs1);
                ret.Add(imgs2);

                return ret;

            });
            #endregion

            #region ECMWF ensemble

            Func<DateTime, List<List<string>>> createImgsECMWFens = new Func<DateTime, List<List<string>>>(dataRef =>
            {
                var imgs0 = new List<string>();

                //order = Merge - Funceme - Conjunto2w                                          
                for (DateTime dt = semana0.Item1; dt <= semana0.Item2; dt = dt.AddDays(1))
                {
                    var fmerge = Path.Combine(caminhoMerge, dt.ToString("yyyy"), dt.ToString("MM"), "prec_" + dt.ToString("yyyyMMdd") + ".ctl");
                    if (File.Exists(fmerge)) imgs0.Add(fmerge);
                    else
                    {
                        var ffunceme = Path.Combine(caminhoFunceme, dt.ToString("yyyy"), dt.ToString("MM"), "funceme_" + dt.ToString("yyyyMMdd") + ".ctl");
                        if (File.Exists(ffunceme)) imgs0.Add(ffunceme);
                        else
                        {
                            var fprev = Path.Combine(caminhoPrevisao, dataRef.ToString("yyyyMM"), dataRef.ToString("dd"), "ECMWF_ONS", "pp" + dataRef.ToString("yyyyMMdd") + "_"
                                + ((dt - dataRef).TotalHours + 12).ToString("0000") + ".ctl");
                            if (File.Exists(fprev)) imgs0.Add(fprev);
                            //else
                            //    throw new Exception("Chuva não exitente para criar relatório");
                        }
                    }
                }

                GradsHelper.Grads.CreateImgFromFiles(imgs0, "Obser + Modelo ECMWF Ensemble em  " + dataRef.ToString("dd/MM"), semana0.Item1.ToString("dd/MM") + " e " + semana0.Item2.ToString("dd/MM"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana0_ECMWF_Ens_" + dataRef.ToString("yyyyMMdd") + ".gif"),
                    @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgFromFiles.gs");

                var imgs1 = new List<string>();
                for (DateTime dt = semana1.Item1; dt <= semana1.Item2; dt = dt.AddDays(1))
                {

                    var fprev = Path.Combine(caminhoPrevisao, dataRef.ToString("yyyyMM"), dataRef.ToString("dd"), "ECMWF_ONS", "pp" + dataRef.ToString("yyyyMMdd") + "_"
                        + ((dt - dataRef).TotalHours + 12).ToString("0000") + ".ctl");
                    if (File.Exists(fprev)) imgs1.Add(fprev);
                    else
                    {
                        fprev = Path.Combine(caminhoPrevisao, dataRef.ToString("yyyyMM"), dataRef.ToString("dd"), "GEFS00", "pp" + dataRef.ToString("yyyyMMdd") + "_"
                        + ((dt - dataRef).TotalHours + 12).ToString("0000") + ".ctl");
                        if (File.Exists(fprev)) imgs1.Add(fprev);
                        // else
                        //     throw new Exception("Chuva não exitente para criar relatório");
                    }
                }

                GradsHelper.Grads.CreateImgFromFiles(imgs1, "Modelo ECMWF Ensemble em  " + dataRef.ToString("dd/MM"), semana1.Item1.ToString("dd/MM") + " e " + semana1.Item2.ToString("dd/MM"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana1_ECMWF_Ens_" + dataRef.ToString("yyyyMMdd") + ".gif"),
                    @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgFromFiles.gs");

                var ret = new List<List<string>>();
                ret.Add(imgs0);
                ret.Add(imgs1);

                return ret;

            });

            #endregion

            #region ImgEuro tropical
            Func<DateTime, List<List<string>>> createImgsEuro = new Func<DateTime, List<List<string>>>(dataRef =>
            {
                var imgs0 = new List<string>();

                //order = Merge - Funceme - Conjunto2w                                          
                for (DateTime dt = semana0.Item1; dt <= semana0.Item2; dt = dt.AddDays(1))
                {
                    var fmerge = Path.Combine(caminhoMerge, dt.ToString("yyyy"), dt.ToString("MM"), "prec_" + dt.ToString("yyyyMMdd") + ".ctl");
                    if (File.Exists(fmerge)) imgs0.Add(fmerge);
                    else
                    {
                        var ffunceme = Path.Combine(caminhoFunceme, dt.ToString("yyyy"), dt.ToString("MM"), "funceme_" + dt.ToString("yyyyMMdd") + ".ctl");
                        if (File.Exists(ffunceme)) imgs0.Add(ffunceme);
                        else
                        {
                            var fprev = Path.Combine(caminhoPrevisao, dataRef.ToString("yyyyMM"), dataRef.ToString("dd"), "ECMWF00", "pp" + dataRef.ToString("yyyyMMdd") + "_"
                                + ((dt - dataRef).TotalHours + 12).ToString("0000") + ".ctl");
                            if (File.Exists(fprev)) imgs0.Add(fprev);
                            //else
                            //    throw new Exception("Chuva não exitente para criar relatório");
                        }
                    }
                }

                GradsHelper.Grads.CreateImgFromFiles(imgs0, "Obser + Modelo Europeu em  " + dataRef.ToString("dd/MM"), semana0.Item1.ToString("dd/MM") + " e " + semana0.Item2.ToString("dd/MM"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana0_euro_" + dataRef.ToString("yyyyMMdd") + ".gif"),
                    @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgFromFiles.gs");

                var imgs1 = new List<string>();
                for (DateTime dt = semana1.Item1; dt <= semana1.Item2; dt = dt.AddDays(1))
                {

                    var fprev = Path.Combine(caminhoPrevisao, dataRef.ToString("yyyyMM"), dataRef.ToString("dd"), "ECMWF00", "pp" + dataRef.ToString("yyyyMMdd") + "_"
                        + ((dt - dataRef).TotalHours + 12).ToString("0000") + ".ctl");
                    if (File.Exists(fprev)) imgs1.Add(fprev);
                    else
                    {
                        fprev = Path.Combine(caminhoPrevisao, dataRef.ToString("yyyyMM"), dataRef.ToString("dd"), "GEFS40_00", "pp" + dataRef.ToString("yyyyMMdd") + "_"
                        + ((dt - dataRef).TotalHours + 12).ToString("0000") + ".ctl");
                        if (File.Exists(fprev)) imgs1.Add(fprev);
                        // else
                        //     throw new Exception("Chuva não exitente para criar relatório");
                    }
                }

                GradsHelper.Grads.CreateImgFromFiles(imgs1, "Modelo Europeu em  " + dataRef.ToString("dd/MM"), semana1.Item1.ToString("dd/MM") + " e " + semana1.Item2.ToString("dd/MM"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana1_euro_" + dataRef.ToString("yyyyMMdd") + ".gif"),
                    @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgFromFiles.gs");

                var ret = new List<List<string>>();
                ret.Add(imgs0);
                ret.Add(imgs1);

                return ret;

            });
            #endregion

            #region ImgGEFS tropical
            Func<DateTime, List<List<string>>> createImgsGEFS = new Func<DateTime, List<List<string>>>(dataRef =>
            {
                var imgs0 = new List<string>();

                //order = Merge - Funceme - Conjunto2w                                          
                for (DateTime dt = semana0.Item1; dt <= semana0.Item2; dt = dt.AddDays(1))
                {
                    var fmerge = Path.Combine(caminhoMerge, dt.ToString("yyyy"), dt.ToString("MM"), "prec_" + dt.ToString("yyyyMMdd") + ".ctl");
                    if (File.Exists(fmerge)) imgs0.Add(fmerge);
                    else
                    {
                        var ffunceme = Path.Combine(caminhoFunceme, dt.ToString("yyyy"), dt.ToString("MM"), "funceme_" + dt.ToString("yyyyMMdd") + ".ctl");
                        if (File.Exists(ffunceme)) imgs0.Add(ffunceme);
                        else
                        {
                            var fprev = Path.Combine(caminhoPrevisao, dataRef.ToString("yyyyMM"), dataRef.ToString("dd"), "GEFS00", "pp" + dataRef.ToString("yyyyMMdd") + "_"
                                + ((dt - dataRef).TotalHours + 12).ToString("0000") + ".ctl");
                            if (File.Exists(fprev)) imgs0.Add(fprev);
                            else
                                throw new Exception("Chuva não exitente para criar relatório");
                        }
                    }
                }

                GradsHelper.Grads.CreateImgFromFiles(imgs0, "Obser + Modelo Global em  " + dataRef.ToString("dd/MM"), semana0.Item1.ToString("dd/MM") + " e " + semana0.Item2.ToString("dd/MM"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana0_gefs_" + dataRef.ToString("yyyyMMdd") + ".gif"),
                    @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgFromFiles.gs");

                var imgs1 = new List<string>();
                for (DateTime dt = semana1.Item1; dt <= semana1.Item2; dt = dt.AddDays(1))
                {

                    var fprev = Path.Combine(caminhoPrevisao, dataRef.ToString("yyyyMM"), dataRef.ToString("dd"), "GEFS00", "pp" + dataRef.ToString("yyyyMMdd") + "_"
                        + ((dt - dataRef).TotalHours + 12).ToString("0000") + ".ctl");
                    if (File.Exists(fprev)) imgs1.Add(fprev);
                    else
                        throw new Exception("Chuva não exitente para criar relatório");
                }

                GradsHelper.Grads.CreateImgFromFiles(imgs1, "Modelo Global em  " + dataRef.ToString("dd/MM"), semana1.Item1.ToString("dd/MM") + " e " + semana1.Item2.ToString("dd/MM"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana1_gefs_" + dataRef.ToString("yyyyMMdd") + ".gif"),
                    @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgFromFiles.gs");


                var imgs2 = new List<string>();
                for (DateTime dt = semana2.Item1; dt <= semana2.Item2; dt = dt.AddDays(1))
                {

                    var fprev = Path.Combine(caminhoPrevisao, dataRef.ToString("yyyyMM"), dataRef.ToString("dd"), "GEFS00", "pp" + dataRef.ToString("yyyyMMdd") + "_"
                       + ((dt - dataRef).TotalHours + 12).ToString("0000") + ".ctl");
                    if (File.Exists(fprev)) imgs2.Add(fprev);
                    else
                    {
                        fprev = System.IO.Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Observado_MERGE\MCP\040", "prec_mct1318_" + dt.ToString("MM") + ".ctl");

                        if (File.Exists(fprev)) imgs2.Add(fprev);
                        else
                            throw new Exception("Chuva não exitente para criar relatório");
                    }
                }

                GradsHelper.Grads.CreateImgFromFiles(imgs2, "Modelo Global em  " + dataRef.ToString("dd/MM"), semana2.Item1.ToString("dd/MM") + " e " + semana2.Item2.ToString("dd/MM"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana2_gefs_" + dataRef.ToString("yyyyMMdd") + ".gif"),
                    @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgFromFiles.gs");


                var ret = new List<List<string>>();
                ret.Add(imgs0);
                ret.Add(imgs1);
                ret.Add(imgs2);

                return ret;
            });
            #endregion

            #region ImgGFS tropical
            Func<DateTime, List<List<string>>> createImgsGFS = new Func<DateTime, List<List<string>>>(dataRef =>
            {
                var imgs0 = new List<string>();

                //order = Merge - Funceme - Conjunto2w                                          
                for (DateTime dt = semana0.Item1; dt <= semana0.Item2; dt = dt.AddDays(1))
                {
                    var fmerge = Path.Combine(caminhoMerge, dt.ToString("yyyy"), dt.ToString("MM"), "prec_" + dt.ToString("yyyyMMdd") + ".ctl");
                    if (File.Exists(fmerge)) imgs0.Add(fmerge);
                    else
                    {
                        var ffunceme = Path.Combine(caminhoFunceme, dt.ToString("yyyy"), dt.ToString("MM"), "funceme_" + dt.ToString("yyyyMMdd") + ".ctl");
                        if (File.Exists(ffunceme)) imgs0.Add(ffunceme);
                        else
                        {
                            var fprev = Path.Combine(caminhoPrevisao, dataRef.ToString("yyyyMM"), dataRef.ToString("dd"), "GFSNOAA00", "pp" + dataRef.ToString("yyyyMMdd") + "_"
                                + ((dt - dataRef).TotalHours + 12).ToString("0000") + ".ctl");
                            if (File.Exists(fprev)) imgs0.Add(fprev);
                            else
                                throw new Exception("Chuva não exitente para criar relatório");
                        }
                    }
                }

                GradsHelper.Grads.CreateImgFromFiles(imgs0, "Obser + Modelo GFSNOAA em  " + dataRef.ToString("dd/MM"), semana0.Item1.ToString("dd/MM") + " e " + semana0.Item2.ToString("dd/MM"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana0_gfs_" + dataRef.ToString("yyyyMMdd") + ".gif"),
                    @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgFromFiles.gs");

                var imgs1 = new List<string>();
                for (DateTime dt = semana1.Item1; dt <= semana1.Item2; dt = dt.AddDays(1))
                {

                    var fprev = Path.Combine(caminhoPrevisao, dataRef.ToString("yyyyMM"), dataRef.ToString("dd"), "GFSNOAA00", "pp" + dataRef.ToString("yyyyMMdd") + "_"
                        + ((dt - dataRef).TotalHours + 12).ToString("0000") + ".ctl");
                    if (File.Exists(fprev)) imgs1.Add(fprev);
                    else
                        throw new Exception("Chuva não exitente para criar relatório");
                }

                GradsHelper.Grads.CreateImgFromFiles(imgs1, "Modelo GFSNOAA em  " + dataRef.ToString("dd/MM"), semana1.Item1.ToString("dd/MM") + " e " + semana1.Item2.ToString("dd/MM"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana1_gfs_" + dataRef.ToString("yyyyMMdd") + ".gif"),
                    @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgFromFiles.gs");


                var imgs2 = new List<string>();
                for (DateTime dt = semana2.Item1; dt <= semana2.Item2; dt = dt.AddDays(1))
                {

                    var fprev = Path.Combine(caminhoPrevisao, dataRef.ToString("yyyyMM"), dataRef.ToString("dd"), "GFSNOAA00", "pp" + dataRef.ToString("yyyyMMdd") + "_"
                       + ((dt - dataRef).TotalHours + 12).ToString("0000") + ".ctl");
                    if (File.Exists(fprev)) imgs2.Add(fprev);
                    else
                    {
                        fprev = System.IO.Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Observado_MERGE\MCP\040", "prec_mct1318_" + dt.ToString("MM") + ".ctl");

                        if (File.Exists(fprev)) imgs2.Add(fprev);
                        else
                            throw new Exception("Chuva não exitente para criar relatório");
                    }
                }

                GradsHelper.Grads.CreateImgFromFiles(imgs2, "Modelo GFSNOAA em  " + dataRef.ToString("dd/MM"), semana2.Item1.ToString("dd/MM") + " e " + semana2.Item2.ToString("dd/MM"),
                    Path.Combine(caminhoBase, @"imgs_temp", "semana2_gfs_" + dataRef.ToString("yyyyMMdd") + ".gif"),
                    @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgFromFiles.gs");


                var ret = new List<List<string>>();
                ret.Add(imgs0);
                ret.Add(imgs1);
                ret.Add(imgs2);

                return ret;
            });
            #endregion

            #region Img Observadas
            Action<DateTime> createImgsObserv = new Action<DateTime>(dataRef =>
            {

                var days = 3;
                var imgs0 = new List<string>();

                //order = Merge - Funceme - Conjunto2w                                          
                for (DateTime dt = dataRef; dt > dataRef.AddDays(-days); dt = dt.AddDays(-1))
                {
                    var fmerge = Path.Combine(caminhoMerge, dt.ToString("yyyy"), dt.ToString("MM"), "prec_" + dt.ToString("yyyyMMdd") + ".ctl");
                    if (File.Exists(fmerge)) imgs0.Add(fmerge);
                    else
                    {
                        var ffunceme = Path.Combine(caminhoFunceme, dt.ToString("yyyy"), dt.ToString("MM"), "funceme_" + dt.ToString("yyyyMMdd") + ".ctl");
                        if (File.Exists(ffunceme)) imgs0.Add(ffunceme);
                    }
                }
                for (int i = 0; i < days; i++)
                {
                    GradsHelper.Grads.CreateImgFromFiles(imgs0.Skip(i).Take(1), "Obser em  " + dataRef.AddDays(-i).ToString("dd/MM"), dataRef.AddDays(-i - 1).ToString("dd/MM") + " e " + dataRef.AddDays(-i).ToString("dd/MM"),
                    Path.Combine(caminhoBase, @"imgs_temp", "observado" + i.ToString() + ".gif"),
                    @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgFromFiles.gs");
                }


                var imgs1 = new List<string>();
                for (DateTime dt = dataRef; dt > dataRef.AddDays(-days); dt = dt.AddDays(-1))
                {

                    var fprev = Path.Combine(caminhoPrevisao, dt.AddDays(-1).ToString("yyyyMM"), dt.AddDays(-1).ToString("dd"), "CONJUNTO2W00", "pp" + dt.AddDays(-1).ToString("yyyyMMdd") + "_"
                        + (36).ToString("0000") + ".ctl");
                    if (File.Exists(fprev)) imgs1.Add(fprev);

                }
                for (int i = 0; i < days; i++)
                {
                    GradsHelper.Grads.CreateImgFromFiles(imgs1.Skip(i).Take(1), "Previsao regional em  " + dataRef.AddDays(-i - 1).ToString("dd/MM"), dataRef.AddDays(-i - 1).ToString("dd/MM") + " e " + dataRef.AddDays(-i).ToString("dd/MM"),
                    Path.Combine(caminhoBase, @"imgs_temp", "conjpassado" + i.ToString() + ".gif"),
                    @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgFromFiles.gs");
                }

                var imgs2 = new List<string>();
                for (DateTime dt = dataRef; dt > dataRef.AddDays(-days); dt = dt.AddDays(-1))
                {

                    var fprev = Path.Combine(caminhoPrevisao, dt.AddDays(-1).ToString("yyyyMM"), dt.AddDays(-1).ToString("dd"), "ETA00", "pp" + dt.AddDays(-1).ToString("yyyyMMdd") + "_"
                        + (36).ToString("0000") + ".ctl");
                    if (File.Exists(fprev)) imgs2.Add(fprev);

                }

                for (int i = 0; i < days; i++)
                {
                    GradsHelper.Grads.CreateImgFromFiles(imgs2.Skip(i).Take(1), "Previsao regional em  " + dataRef.AddDays(-i - 1).ToString("dd/MM"), dataRef.AddDays(-i - 1).ToString("dd/MM") + " e " + dataRef.AddDays(-i).ToString("dd/MM"),
                    Path.Combine(caminhoBase, @"imgs_temp", "etapassado" + i.ToString() + ".gif"),
                    @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgFromFiles.gs");
                }


                imgs2.Clear();
                for (DateTime dt = dataRef; dt > dataRef.AddDays(-days); dt = dt.AddDays(-1))
                {

                    var fprev = Path.Combine(caminhoPrevisao, dt.AddDays(-1).ToString("yyyyMM"), dt.AddDays(-1).ToString("dd"), "ECMWF00", "pp" + dt.AddDays(-1).ToString("yyyyMMdd") + "_"
                        + (36).ToString("0000") + ".ctl");
                    if (File.Exists(fprev)) imgs2.Add(fprev);

                }

                for (int i = 0; i < days; i++)
                {
                    GradsHelper.Grads.CreateImgFromFiles(imgs2.Skip(i).Take(1), "Previsao Euro em  " + dataRef.AddDays(-i - 1).ToString("dd/MM"), dataRef.AddDays(-i - 1).ToString("dd/MM") + " e " + dataRef.AddDays(-i).ToString("dd/MM"),
                    Path.Combine(caminhoBase, @"imgs_temp", "europassado" + i.ToString() + ".gif"),
                    @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgFromFiles.gs");
                }

            });
            #endregion

            var dC0 = createImgsConj(data);
            var dC1 = createImgsConj(data1);

            GradsHelper.Grads.CreateImgDiffFromFiles(dC0[0], dC1[0], "Prev Conj em  " + data.ToString("dd/MM"), semana0.Item1.ToString("dd/MM") + " e " + semana0.Item2.ToString("dd/MM"),
               Path.Combine(caminhoBase, @"imgs_temp", "semana0_conjDiff_" + data.ToString("yyyyMMdd") + ".gif"),
               @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgDiffFromFiles.gs");

            GradsHelper.Grads.CreateImgDiffFromFiles(dC0[1], dC1[1], "Prev Conj em  " + data.ToString("dd/MM"), semana1.Item1.ToString("dd/MM") + " e " + semana1.Item2.ToString("dd/MM"),
               Path.Combine(caminhoBase, @"imgs_temp", "semana1_conjDiff_" + data.ToString("yyyyMMdd") + ".gif"),
               @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgDiffFromFiles.gs");

            GradsHelper.Grads.CreateImgDiffFromFiles(dC0[2], dC1[2], "Prev Conj em  " + data.ToString("dd/MM"), semana2.Item1.ToString("dd/MM") + " e " + semana2.Item2.ToString("dd/MM"),
               Path.Combine(caminhoBase, @"imgs_temp", "semana2_conjDiff_" + data.ToString("yyyyMMdd") + ".gif"),
               @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgDiffFromFiles.gs");


            var dCV0 = createImgsConjVies(data);

            GradsHelper.Grads.CreateImgDiffFromFiles(dC0[0], dCV0[0], "Prev Conj em  " + data.ToString("dd/MM"), semana0.Item1.ToString("dd/MM") + " e " + semana0.Item2.ToString("dd/MM"),
                 Path.Combine(caminhoBase, @"imgs_temp", "semana0_conjviesDiff_" + data.ToString("yyyyMMdd") + ".gif"),
                @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgDiffFromFiles.gs");

            GradsHelper.Grads.CreateImgDiffFromFiles(dC0[1], dCV0[1], "Prev Conj em  " + data.ToString("dd/MM"), semana1.Item1.ToString("dd/MM") + " e " + semana1.Item2.ToString("dd/MM"),
               Path.Combine(caminhoBase, @"imgs_temp", "semana1_conjviesDiff_" + data.ToString("yyyyMMdd") + ".gif"),
               @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgDiffFromFiles.gs");

            GradsHelper.Grads.CreateImgDiffFromFiles(dC0[2], dCV0[2], "Prev Conj em  " + data.ToString("dd/MM"), semana2.Item1.ToString("dd/MM") + " e " + semana2.Item2.ToString("dd/MM"),
               Path.Combine(caminhoBase, @"imgs_temp", "semana2_conjviesDiff_" + data.ToString("yyyyMMdd") + ".gif"),
               @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgDiffFromFiles.gs");



            var dE0 = createImgsECMWFens(data);
            var dE1 = createImgsECMWFens(data1);

            if (dE0.Count > 0 && dE1.Count > 0)
            {



                GradsHelper.Grads.CreateImgDiffFromFiles(dE0[0], dE1[0], "Prev ECMWF Ensemble em  " + data.ToString("dd/MM"), semana0.Item1.ToString("dd/MM") + " e " + semana0.Item2.ToString("dd/MM"),
                   Path.Combine(caminhoBase, @"imgs_temp", "semana0_ECMWF_EnsDiff_" + data.ToString("yyyyMMdd") + ".gif"),
                   @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgDiffFromFiles.gs");

                GradsHelper.Grads.CreateImgDiffFromFiles(dE0[1], dE1[1], "Prev ECMWF Ensemble em  " + data.ToString("dd/MM"), semana1.Item1.ToString("dd/MM") + " e " + semana1.Item2.ToString("dd/MM"),
                   Path.Combine(caminhoBase, @"imgs_temp", "semana1_ECMWF_EnsDiff_" + data.ToString("yyyyMMdd") + ".gif"),
                   @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgDiffFromFiles.gs");

                //GradsHelper.Grads.CreateImgDiffFromFiles(dC0[0], dE1[0], "Prev Conj - Regional em  " + data.ToString("dd/MM"), semana0.Item1.ToString("dd/MM") + " e " + semana0.Item2.ToString("dd/MM"),
                //   Path.Combine(caminhoBase, @"imgs_temp", "semana0_viesDiff_" + data.ToString("yyyyMMdd") + ".gif"),
                //   @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgDiffFromFiles.gs");

                //GradsHelper.Grads.CreateImgDiffFromFiles(dC0[1], dE1[1], "Prev Conj - Regional em  " + data.ToString("dd/MM"), semana1.Item1.ToString("dd/MM") + " e " + semana1.Item2.ToString("dd/MM"),
                //   Path.Combine(caminhoBase, @"imgs_temp", "semana1_viesDiff_" + data.ToString("yyyyMMdd") + ".gif"),
                //   @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgDiffFromFiles.gs");

            }
            var dG0 = createImgsGEFS(data);
            var dG1 = createImgsGEFS(data1);

            GradsHelper.Grads.CreateImgDiffFromFiles(dG0[0], dG1[0], "Prev Global em  " + data.ToString("dd/MM"), semana0.Item1.ToString("dd/MM") + " e " + semana0.Item2.ToString("dd/MM"),
               Path.Combine(caminhoBase, @"imgs_temp", "semana0_gefsDiff_" + data.ToString("yyyyMMdd") + ".gif"),
               @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgDiffFromFiles.gs");

            GradsHelper.Grads.CreateImgDiffFromFiles(dG0[1], dG1[1], "Prev Global em  " + data.ToString("dd/MM"), semana1.Item1.ToString("dd/MM") + " e " + semana1.Item2.ToString("dd/MM"),
               Path.Combine(caminhoBase, @"imgs_temp", "semana1_gefsDiff_" + data.ToString("yyyyMMdd") + ".gif"),
               @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgDiffFromFiles.gs");


            GradsHelper.Grads.CreateImgDiffFromFiles(dG0[2], dG1[2], "Prev Global em  " + data.ToString("dd/MM"), semana2.Item1.ToString("dd/MM") + " e " + semana2.Item2.ToString("dd/MM"),
               Path.Combine(caminhoBase, @"imgs_temp", "semana2_gefsDiff_" + data.ToString("yyyyMMdd") + ".gif"),
               @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgDiffFromFiles.gs");


            createImgsObserv(data);

            var dGf0 = createImgsGFS(data);
            var dGf1 = createImgsGFS(data1);
            GradsHelper.Grads.CreateImgDiffFromFiles(dGf0[0], dGf1[0], "Prev GFSNOAA em  " + data.ToString("dd/MM"), semana0.Item1.ToString("dd/MM") + " e " + semana0.Item2.ToString("dd/MM"),
                Path.Combine(caminhoBase, @"imgs_temp", "semana0_gfsDiff_" + data.ToString("yyyyMMdd") + ".gif"),
                @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgDiffFromFiles.gs");

            GradsHelper.Grads.CreateImgDiffFromFiles(dGf0[1], dGf1[1], "Prev GFSNOAA em  " + data.ToString("dd/MM"), semana1.Item1.ToString("dd/MM") + " e " + semana1.Item2.ToString("dd/MM"),
               Path.Combine(caminhoBase, @"imgs_temp", "semana1_gfsDiff_" + data.ToString("yyyyMMdd") + ".gif"),
               @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgDiffFromFiles.gs");


            GradsHelper.Grads.CreateImgDiffFromFiles(dGf0[2], dGf1[2], "Prev GFSNOAA em  " + data.ToString("dd/MM"), semana2.Item1.ToString("dd/MM") + " e " + semana2.Item2.ToString("dd/MM"),
               Path.Combine(caminhoBase, @"imgs_temp", "semana2_gfsDiff_" + data.ToString("yyyyMMdd") + ".gif"),
               @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgDiffFromFiles.gs");


            var dEu0 = createImgsEuro(data);
            var dEu1 = createImgsEuro(data1);


            GradsHelper.Grads.CreateImgDiffFromFiles(dEu0[0], dEu1[0], "Prev Europeu em  " + data.ToString("dd/MM"), semana0.Item1.ToString("dd/MM") + " e " + semana0.Item2.ToString("dd/MM"),
                Path.Combine(caminhoBase, @"imgs_temp", "semana0_euroDiff_" + data.ToString("yyyyMMdd") + ".gif"),
                @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgDiffFromFiles.gs");

            GradsHelper.Grads.CreateImgDiffFromFiles(dEu0[1], dEu1[1], "Prev Europeu em  " + data.ToString("dd/MM"), semana1.Item1.ToString("dd/MM") + " e " + semana1.Item2.ToString("dd/MM"),
               Path.Combine(caminhoBase, @"imgs_temp", "semana1_euroDiff_" + data.ToString("yyyyMMdd") + ".gif"),
               @"C:\Sistemas\ChuvaVazao\Auxiliar\CreateImgDiffFromFiles.gs");
        }

        private static void CriarImagens(DateTime data, bool preliminar, int hora)
        {


            #region Excel



            var app = new Microsoft.Office.Interop.Excel.Application();
            app.Visible = true;

            var wb = app.Workbooks.Add(Path.Combine(caminhoBase, "Pasta1alfa.xltx"));
            var ws = wb.Worksheets["Geral 1"] as Microsoft.Office.Interop.Excel.Worksheet;
            ws.Activate();


            Excel.Workbook wb2 = null;
            Excel.Worksheet ws2 = null;

            if (!preliminar)
            {
                wb2 = app.Workbooks.Add(Path.Combine(caminhoBase, "Pasta2beta.xltx"));
                ws2 = wb2.Worksheets["ENA"] as Microsoft.Office.Interop.Excel.Worksheet;
                ws2.Range["A1"].Value2 = data;
                wb2.RefreshAll();
            }


            //// !!!!!!!!!!!!!!!  AQUI BOTÃO EXTERNO !!!!!!!!!!!

            ws.Range["A1"].Value2 = data; // Formato DD/MM/YYYY
            ws.Range["B1"].Value2 = hora; // 0 ou 12



            ElegirArquivo primario = new ElegirArquivo();
            primario.Criar(data, hora);
            ws.Activate();


            ws.Range["O4", "X4"].Value2 = primario.pmdxsbetaUltimo.bgrandeVetor;
            ws.Range["O5", "X5"].Value2 = primario.pmdxsbetaUltimo.bparanaiVetor;
            ws.Range["O6", "X6"].Value2 = primario.pmdxsbetaUltimo.bparanapVetor;
            ws.Range["O7", "X7"].Value2 = primario.pmdxsbetaUltimo.biguaVetor;
            ws.Range["O8", "X8"].Value2 = primario.pmdxsbetaUltimo.burugVetor;
            ws.Range["O9", "X9"].Value2 = primario.pmdxsbetaUltimo.bsfrancVetor;
            ws.Range["O10", "X10"].Value2 = primario.pmdxsbetaUltimo.bparanaVetor;
            ws.Range["O11", "X11"].Value2 = primario.pmdxsbetaUltimo.bitaiVetor;
            ws.Range["O12", "X12"].Value2 = primario.pmdxsbetaUltimo.btietVetor;
            ws.Range["O13", "X13"].Value2 = primario.pmdxsbetaUltimo.btocaVetor;


            ws.Range["C4", "L4"].Value2 = primario.pmdxsbetaAnterior.bgrandeVetor;
            ws.Range["C5", "L5"].Value2 = primario.pmdxsbetaAnterior.bparanaiVetor;
            ws.Range["C6", "L6"].Value2 = primario.pmdxsbetaAnterior.bparanapVetor;
            ws.Range["C7", "L7"].Value2 = primario.pmdxsbetaAnterior.biguaVetor;
            ws.Range["C8", "L8"].Value2 = primario.pmdxsbetaAnterior.burugVetor;
            ws.Range["C9", "L9"].Value2 = primario.pmdxsbetaAnterior.bsfrancVetor;
            ws.Range["C10", "L10"].Value2 = primario.pmdxsbetaAnterior.bparanaVetor;
            ws.Range["C11", "L11"].Value2 = primario.pmdxsbetaAnterior.bitaiVetor;
            ws.Range["C12", "L12"].Value2 = primario.pmdxsbetaAnterior.btietVetor;
            ws.Range["C13", "L13"].Value2 = primario.pmdxsbetaAnterior.btocaVetor;


            ws.Range["O18", "X18"].Value2 = primario.pmdxsbgefsUltimo.bgrandeVetor;
            ws.Range["O19", "X19"].Value2 = primario.pmdxsbgefsUltimo.bparanaiVetor;
            ws.Range["O20", "X20"].Value2 = primario.pmdxsbgefsUltimo.bparanapVetor;
            ws.Range["O21", "X21"].Value2 = primario.pmdxsbgefsUltimo.biguaVetor;
            ws.Range["O22", "X22"].Value2 = primario.pmdxsbgefsUltimo.burugVetor;
            ws.Range["O23", "X23"].Value2 = primario.pmdxsbgefsUltimo.bsfrancVetor;
            ws.Range["O24", "X24"].Value2 = primario.pmdxsbgefsUltimo.bparanaVetor;
            ws.Range["O25", "X25"].Value2 = primario.pmdxsbgefsUltimo.bitaiVetor;
            ws.Range["O26", "X26"].Value2 = primario.pmdxsbgefsUltimo.btietVetor;
            ws.Range["O27", "X27"].Value2 = primario.pmdxsbgefsUltimo.btocaVetor;


            ws.Range["C18", "L18"].Value2 = primario.pmdxsbgefsAnterior.bgrandeVetor;
            ws.Range["C19", "L19"].Value2 = primario.pmdxsbgefsAnterior.bparanaiVetor;
            ws.Range["C20", "L20"].Value2 = primario.pmdxsbgefsAnterior.bparanapVetor;
            ws.Range["C21", "L21"].Value2 = primario.pmdxsbgefsAnterior.biguaVetor;
            ws.Range["C22", "L22"].Value2 = primario.pmdxsbgefsAnterior.burugVetor;
            ws.Range["C23", "L23"].Value2 = primario.pmdxsbgefsAnterior.bsfrancVetor;
            ws.Range["C24", "L24"].Value2 = primario.pmdxsbgefsAnterior.bparanaVetor;
            ws.Range["C25", "L25"].Value2 = primario.pmdxsbgefsAnterior.bitaiVetor;
            ws.Range["C26", "L26"].Value2 = primario.pmdxsbgefsAnterior.btietVetor;
            ws.Range["C27", "L27"].Value2 = primario.pmdxsbgefsAnterior.btocaVetor;


            ws.Range["O32", "X32"].Value2 = primario.pmdxsbgfsUltimo.bgrandeVetor;
            ws.Range["O33", "X33"].Value2 = primario.pmdxsbgfsUltimo.bparanaiVetor;
            ws.Range["O34", "X34"].Value2 = primario.pmdxsbgfsUltimo.bparanapVetor;
            ws.Range["O35", "X35"].Value2 = primario.pmdxsbgfsUltimo.biguaVetor;
            ws.Range["O36", "X36"].Value2 = primario.pmdxsbgfsUltimo.burugVetor;
            ws.Range["O37", "X37"].Value2 = primario.pmdxsbgfsUltimo.bsfrancVetor;
            ws.Range["O38", "X38"].Value2 = primario.pmdxsbgfsUltimo.bparanaVetor;
            ws.Range["O39", "X39"].Value2 = primario.pmdxsbgfsUltimo.bitaiVetor;
            ws.Range["O40", "X40"].Value2 = primario.pmdxsbgfsUltimo.btietVetor;
            ws.Range["O41", "X41"].Value2 = primario.pmdxsbgfsUltimo.btocaVetor;



            ws.Range["C32", "L32"].Value2 = primario.pmdxsbgfsAnterior.bgrandeVetor;
            ws.Range["C33", "L33"].Value2 = primario.pmdxsbgfsAnterior.bparanaiVetor;
            ws.Range["C34", "L34"].Value2 = primario.pmdxsbgfsAnterior.bparanapVetor;
            ws.Range["C35", "L35"].Value2 = primario.pmdxsbgfsAnterior.biguaVetor;
            ws.Range["C36", "L36"].Value2 = primario.pmdxsbgfsAnterior.burugVetor;
            ws.Range["C37", "L37"].Value2 = primario.pmdxsbgfsAnterior.bsfrancVetor;
            ws.Range["C38", "L38"].Value2 = primario.pmdxsbgfsAnterior.bparanaVetor;
            ws.Range["C39", "L39"].Value2 = primario.pmdxsbgfsAnterior.bitaiVetor;
            ws.Range["C40", "L40"].Value2 = primario.pmdxsbgfsAnterior.btietVetor;
            ws.Range["C41", "L41"].Value2 = primario.pmdxsbgfsAnterior.btocaVetor;


            ws.Range["O46", "X46"].Value2 = primario.pmdxsbconjUltimo.bgrandeVetor;
            ws.Range["O47", "X47"].Value2 = primario.pmdxsbconjUltimo.bparanaiVetor;
            ws.Range["O48", "X48"].Value2 = primario.pmdxsbconjUltimo.bparanapVetor;
            ws.Range["O49", "X49"].Value2 = primario.pmdxsbconjUltimo.biguaVetor;
            ws.Range["O50", "X50"].Value2 = primario.pmdxsbconjUltimo.burugVetor;
            ws.Range["O51", "X51"].Value2 = primario.pmdxsbconjUltimo.bsfrancVetor;
            ws.Range["O52", "X52"].Value2 = primario.pmdxsbconjUltimo.bparanaVetor;
            ws.Range["O53", "X53"].Value2 = primario.pmdxsbconjUltimo.bitaiVetor;
            ws.Range["O54", "X54"].Value2 = primario.pmdxsbconjUltimo.btietVetor;
            ws.Range["O55", "X55"].Value2 = primario.pmdxsbconjUltimo.btocaVetor;


            ws.Range["C46", "L46"].Value2 = primario.pmdxsbconjAnterior.bgrandeVetor;
            ws.Range["C47", "L47"].Value2 = primario.pmdxsbconjAnterior.bparanaiVetor;
            ws.Range["C48", "L48"].Value2 = primario.pmdxsbconjAnterior.bparanapVetor;
            ws.Range["C49", "L49"].Value2 = primario.pmdxsbconjAnterior.biguaVetor;
            ws.Range["C50", "L50"].Value2 = primario.pmdxsbconjAnterior.burugVetor;
            ws.Range["C51", "L51"].Value2 = primario.pmdxsbconjAnterior.bsfrancVetor;
            ws.Range["C52", "L52"].Value2 = primario.pmdxsbconjAnterior.bparanaVetor;
            ws.Range["C53", "L53"].Value2 = primario.pmdxsbconjAnterior.bitaiVetor;
            ws.Range["C54", "L54"].Value2 = primario.pmdxsbconjAnterior.btietVetor;
            ws.Range["C55", "L55"].Value2 = primario.pmdxsbconjAnterior.btocaVetor;

            //////////////////////////////////////////////////////////////////////////////////////

            ws = wb.Worksheets["Geral 1"] as Microsoft.Office.Interop.Excel.Worksheet;
            ws.Activate();

            Excel.Range r1 = ws.Range["Z2", "AC13"];
            Excel.Range r2 = ws.Range["Z16", "AC27"];
            Excel.Range r3 = ws.Range["Z30", "AC41"];
            Excel.Range r4 = ws.Range["Z44", "AC55"];

            System.Threading.Thread thread1 = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(obj =>
            {
                if (obj is Range range)
                {
                    range.Copy();
                    if (System.Windows.Forms.Clipboard.ContainsImage())
                    {
                        var img = System.Windows.Forms.Clipboard.GetImage();
                        img.Save(Path.Combine(caminhoBase, @"imgs_temp", "Tabela_ETA40_Prec_Med_Ac.gif"));
                    }
                }
            }));
            thread1.SetApartmentState(System.Threading.ApartmentState.STA);
            thread1.Start(r1);
            thread1.Join();

            System.Threading.Thread thread2 = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(obj =>
            {
                if (obj is Range range)
                {
                    range.Copy();
                    if (System.Windows.Forms.Clipboard.ContainsImage())
                    {
                        var img = System.Windows.Forms.Clipboard.GetImage();
                        img.Save(Path.Combine(caminhoBase, @"imgs_temp", "Tabela_GEFS_Prec_Med_Ac.gif"));
                    }
                }
            }));
            thread2.SetApartmentState(System.Threading.ApartmentState.STA);
            thread2.Start(r2);
            thread2.Join();

            System.Threading.Thread thread3 = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(obj =>
            {
                if (obj is Range range)
                {
                    range.Copy();
                    if (System.Windows.Forms.Clipboard.ContainsImage())
                    {
                        var img = System.Windows.Forms.Clipboard.GetImage();
                        img.Save(Path.Combine(caminhoBase, @"imgs_temp", "Tabela_GFS_Prec_Med_Ac.gif"));
                    }
                }
            }));
            thread3.SetApartmentState(System.Threading.ApartmentState.STA);
            thread3.Start(r3);
            thread3.Join();

            System.Threading.Thread thread4 = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(obj =>
            {
                if (obj is Range range)
                {
                    range.Copy();
                    if (System.Windows.Forms.Clipboard.ContainsImage())
                    {
                        var img = System.Windows.Forms.Clipboard.GetImage();
                        img.Save(Path.Combine(caminhoBase, @"imgs_temp", "Tabela_CONJ_Prec_Med_Ac.gif"));
                    }
                }
            }));
            thread4.SetApartmentState(System.Threading.ApartmentState.STA);
            thread4.Start(r4);
            thread4.Join();

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            if (hora == 00)
            {
                ws = wb.Worksheets["Geral 2_00"] as Microsoft.Office.Interop.Excel.Worksheet;
                ws.Activate();
                foreach (ChartObject chart in ws.ChartObjects())
                {
                    chart.Activate();
                    chart.Chart.Export(Path.Combine(caminhoBase, @"imgs_temp", chart.Name + ".gif"));
                }
            }
            else
            {
                ws = wb.Worksheets["Geral 2_12"] as Microsoft.Office.Interop.Excel.Worksheet;
                ws.Activate();
                foreach (ChartObject chart in ws.ChartObjects())
                {
                    chart.Activate();
                    chart.Chart.Export(Path.Combine(caminhoBase, @"imgs_temp", chart.Name + ".gif", "GIF"));
                }
            }


            wb.Close(false);


            if (!preliminar)
            {
                wb2.Activate();
                ws2.Activate();

                foreach (ChartObject chart in ws2.ChartObjects())
                {
                    chart.Activate();
                    chart.Chart.Export(Path.Combine(caminhoBase, @"imgs_temp", chart.Name + ".gif"));
                }

                wb2.Close(false);
            }

            app.Quit();

            #endregion

        }

        private static List<string> CriarImagensEnas(DateTime data, bool preliminar)
        {

            var re = new List<string>();

            var app = new Microsoft.Office.Interop.Excel.Application();

            var wb = app.Workbooks.Add(Path.Combine(caminhoBase, "EnasDiarias3.xlsm"));
            var ws = wb.Worksheets["PREVISÕES"] as Microsoft.Office.Interop.Excel.Worksheet;
            ws.Activate();


            var nextRev = Tools.GetNextRev(data);


            // var previsoesPath0 = Path.Combine(@"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\16_Chuva_Vazao", nextRev.revDate.ToString("yyyy_MM"), $"RV{nextRev.rev}", data.ToString("yy-MM-dd"));
            // var previsoesPath1 = Path.Combine(@"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\16_Chuva_Vazao", nextRev.revDate.ToString("yyyy_MM"), $"RV{nextRev.rev}", data.AddDays(-1).ToString("yy-MM-dd"));
            var previsoesPath0 = Path.Combine(@"C:\Files\Middle - Preço\16_Chuva_Vazao", nextRev.revDate.ToString("yyyy_MM"), $"RV{nextRev.rev}", data.ToString("yy-MM-dd"));
            var previsoesPath1 = Path.Combine(@"C:\Files\Middle - Preço\16_Chuva_Vazao", nextRev.revDate.ToString("yyyy_MM"), $"RV{nextRev.rev}", data.AddDays(-1).ToString("yy-MM-dd"));

            var dirs = new List<string>();

            if (preliminar)
            {

                dirs.Add(Path.Combine(previsoesPath0, "CV_ACOMPH_FUNC_d-1"));
                dirs.Add(Path.Combine(previsoesPath0, "CV_ACOMPH_FUNC_d-1_EURO"));
                dirs.Add(Path.Combine(previsoesPath1, "CV_ACOMPH_FUNC"));
                dirs.Add(Path.Combine(previsoesPath1, "CV_ACOMPH_FUNC_EURO"));


                re.Add($"CV {data.ToString("dd_MM")} CONJ ACOMPH ANT.");
                re.Add($"CV {data.ToString("dd_MM")} EURO ACOMPH ANT.");
                re.Add($"CV {data.AddDays(-1).ToString("dd_MM")} CONJ");
                re.Add($"CV {data.AddDays(-1).ToString("dd_MM")} EURO");
            }
            else
            {
                dirs.Add(Path.Combine(previsoesPath0, "CV_ACOMPH_FUNC"));
                dirs.Add(Path.Combine(previsoesPath0, "CV_ACOMPH_FUNC_EURO"));
                dirs.Add(Path.Combine(previsoesPath0, "CV_ACOMPH_FUNC_d-1"));
                dirs.Add(Path.Combine(previsoesPath1, "CV_ACOMPH_FUNC"));
                //dirs.Add(Path.Combine(previsoesPath1, "CV_ACOMPH_FUNC_d-1"));

                re.Add($"CV {data.ToString("dd_MM")} CONJ");
                re.Add($"CV {data.ToString("dd_MM")} EURO");
                re.Add($"CV {data.ToString("dd_MM")} CONJ ACOMPH ANT.");
                re.Add($"CV {data.AddDays(-1).ToString("dd_MM")} CONJ");
            }





            //var dirs = System.IO.Directory.GetDirectories(previsoesPath0, "*", SearchOption.TopDirectoryOnly)
            //    .Select(x => new System.IO.DirectoryInfo(x))
            //    .OrderByDescending(x => x.CreationTime).Where(x => x.Name.StartsWith("CV_")).Take(2)
            //    .Union(
            //        System.IO.Directory.GetDirectories(previsoesPath1, "*", SearchOption.TopDirectoryOnly)
            //            .Select(x => new System.IO.DirectoryInfo(x))
            //            .OrderByDescending(x => x.CreationTime).Where(x => x.Name.StartsWith("CV_")).Take(2)
            //    ).ToList();



            for (int i = 0; i < dirs.Count; i++)
            {
                try
                {

                    //re.Add(dirs[i].Parent.Name + "_" + dirs[i].Name);

                    var f1 = Path.Combine(dirs[i], "enadiaria.log");
                    var f2 = Path.Combine(dirs[i], "chuvamedia.log");

                    if (Directory.Exists(dirs[i]) && File.Exists(f1)) //&& File.Exists(f2))
                    {

                        ws.Cells[2 + i * 50, 1].Value2 = re[i];

                        var valsText = System.IO.File.ReadAllLines(f1).Select(x => x.Split('\t')).ToArray();
                        var vals = new dynamic[valsText.Length, valsText.Max(x => x.Length)];

                        for (int r = 0; r < valsText.Length; r++)
                            for (int c = 0; c < valsText[r].Length; c++)
                            {
                                if (double.TryParse(valsText[r][c], out double dbl))
                                    vals[r, c] = dbl;
                                else if (DateTime.TryParse(valsText[r][c], out DateTime dt))
                                    vals[r, c] = dt;
                                else
                                    vals[r, c] = valsText[r][c];
                            }

                        ws.Range["B" + (i * 50 + 1), "N" + (i * 50 + vals.GetLength(0))].Value2 = vals;
                    }
                    if (Directory.Exists(dirs[i]) && File.Exists(f2)) //&& File.Exists(f2))
                    {

                        var valsText = System.IO.File.ReadAllLines(f2).Select(x => x.Split('\t')).ToArray();
                        var vals = new dynamic[valsText.Length, valsText.Max(x => x.Length)];

                        for (int r = 0; r < valsText.Length; r++)
                            for (int c = 0; c < valsText[r].Length; c++)
                            {
                                if (double.TryParse(valsText[r][c], out double dbl))
                                    vals[r, c] = dbl;
                                else if (DateTime.TryParse(valsText[r][c], out DateTime dt))
                                    vals[r, c] = dt;
                                else
                                    vals[r, c] = valsText[r][c];
                            }

                        ws.Range["P" + (i * 50 + 1), "AA" + (i * 50 + vals.GetLength(0))].Value2 = vals;
                    }

                    var f3 = Path.Combine(dirs[i], "resumoENA.gif");
                    if (Directory.Exists(dirs[i]) && File.Exists(f3))
                    {
                        var f3final = Path.Combine(caminhoBase, @"imgs_temp", "resumo" + i.ToString() + "_" + data.ToString("yyyyMMdd") + ".gif");
                        if (File.Exists(f3final)) File.Delete(f3final);
                        File.Copy(f3, f3final);
                    }
                }
                catch (Exception e)
                {
                }
            }

            try
            {
                app.Visible = true;
                app.ScreenUpdating = true;
                wb.Activate();
                wb.RefreshAll();

                app.CalculateFull();

                System.Threading.Thread.Sleep(12000);

                while (!app.Ready) System.Threading.Thread.Sleep(1000);

            }
            catch (Exception ec)
            {
                try
                {
                    System.Threading.Thread.Sleep(12000);
                    app.Visible = true;
                    app.ScreenUpdating = true;
                    app.Calculation = XlCalculation.xlCalculationAutomatic;

                    while (!app.Ready) System.Threading.Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    wb.Close(SaveChanges: false);
                    app.Quit();
                    e.ToString();
                    return null;
                }
            }

            var fname = Path.Combine(caminhoBase, @"imgs_temp", "EnasDiarias_" + data.ToString("yyyyMMdd") + ".xlsm");
            if (File.Exists(fname)) File.Delete(fname);

            wb.SaveAs(fname, wb.FileFormat);

            var aprovado = false;
            while (!aprovado)
            {
                try
                {
                    app.Run(wb.Name + "!Export");
                    aprovado = true;
                }
                catch
                {
                    aprovado = false;
                }
            }

            wb.Saved = true;
            wb.Close(SaveChanges: false);

            app.Quit();

            return re;

        }
    }
}
