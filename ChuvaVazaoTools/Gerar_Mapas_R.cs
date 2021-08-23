﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace ChuvaVazaoTools
{
    class Gerar_Mapas_R
    {
        public static void Gerar_R(string path_Conj, System.IO.TextWriter logF)
        {

            DateTime data_Atual = DateTime.Today;
            var path_H = @"C:\Files\Middle - Preço\Acompanhamento de Precipitação\";
            var path_Previsao = Path.Combine(path_H, "Previsao_Numerica");
            var path_CSV = Path.Combine(path_Conj, "Trabalho\\Uruguai\\Passo Sao Joao");
            var path_Acomph = @"C:\Files\Middle - Preço\Acompanhamento de vazões\ACOMPH\1_historico\";
            var path_ModeloR = Path.Combine(path_Previsao, "Modelo_R");

            var oneDrivePath_ori = Environment.GetEnvironmentVariable("OneDriveCommercial");
            //B:\Compass\MinhaTI\Alex Freires Marques - Compass\Trading
            //var oneDrive = Path.Combine(oneDrivePath_ori, @"Compass\Pedro\NOAA\");
            //if (!Directory.Exists(oneDrive))
            //{
            //    oneDrive = Path.Combine(oneDrivePath_ori.Replace(oneDrivePath_ori.Split('\\').Last(), @"MinhaTI\Alex Freires Marques - Compass\Pedro\NOAA\"));
            //}

            var oneDrive_preco = Path.Combine(oneDrivePath_ori,@"Energy Core Pricing - Documents\Acompanhamento_de_Precipitacao\Previsao\");
            //B:\Enercore\Energy Core Trading\Energy Core Pricing - Documents\Acompanhamento_de_Precipitacao
            if (!Directory.Exists(oneDrive_preco))
            {
                oneDrive_preco = oneDrive_preco.Replace("Energy Core Pricing - Documents", "Energy Core Pricing - Documentos");
            }
            // Date of VE
            int dias_ve = -1;
            var runRev_Curr = ChuvaVazaoTools.Tools.Tools.GetCurrRev(data_Atual);

            var cv1 = runRev_Curr.revDate.AddDays(dias_ve);

            var runRev = ChuvaVazaoTools.Tools.Tools.GetNextRev(data_Atual);
            var cv2 = runRev.revDate.AddDays(-1);

            var runRev3 = ChuvaVazaoTools.Tools.Tools.GetNextRev(data_Atual, 2);
            var cv3 = runRev3.revDate.AddDays(-1);

            var runRev4 = ChuvaVazaoTools.Tools.Tools.GetNextRev(data_Atual, 3);
            var cv4 = runRev4.revDate.AddDays(-1);

            var runRev5 = ChuvaVazaoTools.Tools.Tools.GetNextRev(data_Atual, 4);
            var cv5 = runRev5.revDate.AddDays(-1);

            if (File.Exists(Path.Combine(path_Conj, "error.log")))
            {
                File.Delete(Path.Combine(path_Conj, "error.log"));
            }

            try
            {
                // Roda PSAT
                //  executar_R(path_Conj, "ons.R convert_psat_remvies_V2.R");

                //Last day of Acomph

                var dt_acomph = data_Atual;

                logF.WriteLine("Verificando Acomph");
                while (!File.Exists(Path.Combine(path_Acomph, dt_acomph.ToString("yyyy"), dt_acomph.ToString("MM_yyyy"), "ACOMPH_" + dt_acomph.ToString("dd-MM-yyyy") + ".xls")))
                {
                    dt_acomph = dt_acomph.AddDays(-1);
                }
                // dt_acomph = dt_acomph.AddDays(-1);
                //Check if exist funceme of Today

                var funceme = Directory.GetFiles(Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Previsao_Numerica\Modelo_R\funceme\", data_Atual.ToString("yyyyMM"), data_Atual.ToString("dd")));
                var funcemeFolder = Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Previsao_Numerica\Modelo_R\funceme\", data_Atual.ToString("yyyyMM"), data_Atual.ToString("dd"));

                logF.WriteLine("Verificando Funceme data atual");
                if (funceme.Length != 0)
                {
                    logF.WriteLine("Funceme Encontrado!");
                    //ultimo dia de atualização da previsão

                    /*      StreamReader stream = new StreamReader(Path.Combine(path_CSV, "ETA40.csv"));
                          string dia = null;
                          string linha = null;
                          while ((linha = stream.ReadLine()) != null)
                          {
                              string[] coluna = linha.Split(';');
                              dia = coluna[0];

                          }
                          var last_day = Convert.ToDateTime(dia);
                          stream.Close();
                          */
                    logF.WriteLine("Tranferindo arquivos GEFS para Entrada");
                    //Verifca o GEFS ONS, caso existir copia para os arquivos de entrada 

                    var path_Dia = Path.Combine(path_Previsao, data_Atual.ToString("yyyyMM"), data_Atual.ToString("dd"));

                    var GEFS_NOAA = Path.Combine(path_Previsao, "Modelo_R\\GEFS00");
                    var GEFS_NOAA_05 = Path.Combine(oneDrive_preco, data_Atual.ToString("yyyy"), data_Atual.ToString("MM"), data_Atual.ToString("dd"), "GEFS_0.5_00");

                    //var path_ArqPrev = Path.Combine(path_Conj, "Arq_Entrada\\Previsao");
                    var path_ArqPrev = Path.Combine(path_Conj, "grid");
                    if (!Directory.Exists(path_ArqPrev)) Directory.CreateDirectory(path_ArqPrev);

                    var GEFS_ONS = Directory.GetFiles(path_Dia, "GEFS_*").Where(x => x.EndsWith(".dat"));
                    var GEFS_05 = Directory.GetFiles(GEFS_NOAA_05, "GEFS_*").Where(x => x.EndsWith(".dat"));

                    if (GEFS_ONS != null)
                    {
                        if (!Directory.Exists(Path.Combine(path_ArqPrev, "GEFS"))) Directory.CreateDirectory(Path.Combine(path_ArqPrev, "GEFS"));
                        //14 dias do GEFS ONS
                        ///foreach (var GEFS in GEFS_ONS)
                        foreach (var GEFS in GEFS_ONS)
                        {

                            if (GEFS.EndsWith(".dat"))
                            {
                                var num_carecteres = GEFS.Split('\\').Last().Length;
                                if (num_carecteres == 23)
                                {
                                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"p(\d{2})(\d{2})(\d{2})a(\d{2})(\d{2})(\d{2})");
                                    var data_mapa = r.Match(GEFS);
                                    if (data_mapa.Success)
                                    {
                                        File.Copy(GEFS, Path.Combine(path_ArqPrev, "GEFS", GEFS.Split('\\').Last()), true);
                                    }
                                }
                                //else if (GEFS.Contains("GEFS_m"))
                                // {
                                //    File.Copy(GEFS, Path.Combine(path_Conj, "Arq_Entrada", "GEFS", GEFS.Split('\\').Last()), true);
                                // }
                            }
                        }
                    }

                    if (GEFS_05 != null)
                    {
                        //Todos os dias do GEFS NOAA
                        var Ult_GEFS = Directory.GetFiles(GEFS_NOAA_05).Where(File => !GEFS_ONS.Any(x => File.EndsWith(x.Split('\\').Last(), StringComparison.OrdinalIgnoreCase)));

                        if (Ult_GEFS != null)
                        {
                            foreach (var Ult in Ult_GEFS)
                            {
                                if (Ult.EndsWith(".dat"))
                                {

                                    var num_carecteres = Ult.Split('\\').Last().Length;
                                    if (num_carecteres == 23)
                                    {
                                        System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"p(\d{2})(\d{2})(\d{2})a(\d{2})(\d{2})(\d{2})");
                                        var data_mapa = r.Match(Ult);
                                        if (data_mapa.Success)
                                        {
                                            File.Copy(Ult, Path.Combine(path_ArqPrev, "GEFS", Ult.Split('\\').Last()), true);
                                        }
                                    }
                                }
                            }

                        }
                    }
                    GEFS_Ext(cv2, Path.Combine(path_ArqPrev, "GEFS"));

                    //ETA 10 ONS dias 


                    var ETA_ONS = Directory.GetFiles(path_Dia, "ETA40_*").Where(x => x.EndsWith(".dat"));

                    if (ETA_ONS != null)
                    {
                        if (!Directory.Exists(Path.Combine(path_ArqPrev, "ETA40"))) Directory.CreateDirectory(Path.Combine(path_ArqPrev, "ETA40"));
                        foreach (var ETA in ETA_ONS)
                        {
                            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"p(\d{2})(\d{2})(\d{2})a(\d{2})(\d{2})(\d{2})");
                            var data_mapa = r.Match(ETA);
                            if (data_mapa.Success)
                            {
                                File.Copy(ETA, Path.Combine(path_ArqPrev, "ETA40", ETA.Split('\\').Last()), true);
                            }

                        }
                    }

                    //Euro 14 ONS dias 


                    var Euro_ONS = Directory.GetFiles(path_Dia, "ECMWF_*").Where(x => x.EndsWith(".dat"));

                    if (Euro_ONS != null)
                    {
                        if (!Directory.Exists(Path.Combine(path_ArqPrev, "ECMWF"))) Directory.CreateDirectory(Path.Combine(path_ArqPrev, "ECMWF"));
                        foreach (var Euro in Euro_ONS)
                        {
                            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"p(\d{2})(\d{2})(\d{2})a(\d{2})(\d{2})(\d{2})");
                            var data_mapa = r.Match(Euro);
                            if (data_mapa.Success)
                            {
                                File.Copy(Euro, Path.Combine(path_ArqPrev, "ECMWF", Euro.Split('\\').Last()), true);
                            }

                        }
                    }

                    // ECWMF OP
                    logF.WriteLine("Tranferindo arquivos ECWMF OP para Entrada");

                    var ECMWFs = Directory.GetFiles(Path.Combine(path_ModeloR, "ECMWF00", data_Atual.ToString("yyyyMM"), data_Atual.ToString("dd"))).Where(x => x.EndsWith(".dat"));

                    if (ECMWFs != null)
                    {
                        if (!Directory.Exists(Path.Combine(path_ArqPrev, "ECMWFop"))) Directory.CreateDirectory(Path.Combine(path_ArqPrev, "ECMWFop"));
                        foreach (var ECMWF in ECMWFs)
                        {

                            System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"pp(\d{4})(\d{2})(\d{2})_(\d+)");

                            var fMatch = r.Match(ECMWF);
                            if (fMatch.Success)
                            {
                                var data = new DateTime(
                                    int.Parse(fMatch.Groups[1].Value),
                                    int.Parse(fMatch.Groups[2].Value),
                                    int.Parse(fMatch.Groups[3].Value))
                                    ;

                                var horas = int.Parse(fMatch.Groups[4].Value);

                                var dataPrev = data.AddHours(horas).Date;

                                File.Copy(ECMWF, Path.Combine(path_ArqPrev, "ECMWFop", "ECMWFop_p" + data.ToString("ddMMyy") + "a" + dataPrev.ToString("ddMMyy") + ".dat"), true);

                            }
                        }
                    }
                    var data_ecmwf_ext = ECMWF_Ext(cv2, Path.Combine(path_ArqPrev, "ECMWF"), -dias_ve + 13);
                    //gfs
                    logF.WriteLine("Tranferindo arquivos GFS para Entrada");
                    if (!Directory.Exists(Path.Combine(path_ArqPrev, "GFS"))) Directory.CreateDirectory(Path.Combine(path_ArqPrev, "GFS"));
                    var GFS_NOAA = Directory.GetFiles(Path.Combine(oneDrive_preco, data_Atual.ToString("yyyy"), data_Atual.ToString("MM"), data_Atual.ToString("dd"), "GFS00","txt")).Where(x => x.EndsWith(".dat"));
                    
                    foreach(var arq in GFS_NOAA)
                    {
                        File.Copy(arq, Path.Combine(path_ArqPrev, "GFS", arq.Split('\\').Last()),true);
                    }
                    //Descompactar o Zip Com dat
                    

                   // System.IO.Compression.ZipFile.ExtractToDirectory(Path.Combine(oneDrive, data_Atual.ToString("yyyy"), data_Atual.ToString("MM"), data_Atual.ToString("dd"), "GFS00", "txt.zip"), Path.Combine(path_ArqPrev, "GFS"));
                    
                    //System.IO.Compression.ZipFile.ExtractToDirectory(Path.Combine(path_Previsao, data_Atual.ToString("yyyyMM"), data_Atual.ToString("dd"), "GFSNOAA00", "txt.zip"), Path.Combine(path_ArqPrev, "GFS"));

                    var GFSs = Directory.GetFiles(Path.Combine(path_ArqPrev, "GFS"), "gfs_mp*");

                    foreach (var GFS in GFSs)
                    {
                        try
                        {
                            File.Delete(GFS);
                        }
                        catch
                        {

                        }
                    }

                    // Verifica Merge, caso não tenha usa o funceme
                    logF.WriteLine("Verifica Funceme/Merge");
                    DateTime dt_func = data_Atual;
                    // while (dt_func != dt_acomph)
                    //{
                    var Merge = Directory.GetFiles(Path.Combine(path_ModeloR, "merge", dt_func.ToString("yyyyMM"), dt_func.ToString("dd"))).Where(x => x.EndsWith(".dat"));
                    if (!Directory.Exists(Path.Combine(path_ArqPrev, "funceme"))) Directory.CreateDirectory(Path.Combine(path_ArqPrev, "funceme"));

                    if (Merge.Count() > 0)
                    {
                        foreach (var arq in Merge)
                        {
                            File.Copy(arq, Path.Combine(path_ArqPrev, "Funceme", arq.Split('\\').Last().Replace("merge", "funceme")), true);
                        }
                    }
                    else
                    {
                        var Func = Directory.GetFiles(Path.Combine(path_ModeloR, "funceme", dt_func.ToString("yyyyMM"), dt_func.ToString("dd"))).Where(x => x.EndsWith(".dat"));
                        string funcArq = "";
                        if (Func.Any(x =>x.Contains("LATE_Inmet")))
                        {
                            funcArq = Func.Where(x => x.Contains("LATE_Inmet")).First();
                            File.Copy(funcArq, Path.Combine(path_ArqPrev, "funceme", "funceme_" + funcArq.Split('\\').Last().Split('_').Last()), true);
                        }
                        else if (Func.Any(x => x.Contains("Inmet_funceme")))
                        {
                            funcArq = Func.Where(x => x.Contains("Inmet_funceme")).First();
                            File.Copy(funcArq, Path.Combine(path_ArqPrev, "funceme", "funceme_" + funcArq.Split('\\').Last().Split('_').Last()), true);
                        }
                        else if (Func.Any(x => x.Contains("LATE_")))
                        {
                            funcArq = Func.Where(x => x.Contains("LATE_")).First();
                            File.Copy(funcArq, Path.Combine(path_ArqPrev, "funceme", "funceme_" + funcArq.Split('\\').Last().Split('_').Last()), true);
                        }
                        else
                        {
                            funcArq = Func.Where(x => x.Contains("funceme_p")).First();
                            File.Copy(funcArq, Path.Combine(path_ArqPrev, "funceme", funcArq.Split('\\').Last()), true);
                        }
                        //foreach (var arq in Func)
                        //{
                        //    //File.Copy(arq, Path.Combine(path_ArqPrev, "funceme", arq.Split('\\').Last().Replace('p' + dt_func.ToString("ddMMyy"), dt_func.ToString("ddMMyy"))), true);

                        //    if (Func.Count() > 1 && arq.Contains("LATE"))
                        //    {
                        //        File.Copy(arq, Path.Combine(path_ArqPrev, "funceme", "funceme_" + arq.Split('\\').Last().Split('_').Last()), true);
                        //    }
                        //    else
                        //    {
                        //        File.Copy(arq, Path.Combine(path_ArqPrev, "funceme", arq.Split('\\').Last()), true);
                        //    }

                        //}
                    }
                    //   dt_func = dt_func.AddDays(-1);
                    //}



                    //Completa Historico Arq Entrada

                   // Hist_Entrada("ECMWF", path_Conj, path_Previsao, data_Atual);
                   // Hist_Entrada("ETA40", path_Conj, path_Previsao, data_Atual);



                    logF.WriteLine("Executando Script");
                    executar_R(path_Conj, "formato_novo.r");
                    executar_R(path_Conj, "ons.R Roda_Conjunto_V3.2.R");
                    // executar_R(path_Conj, "vies_ve_woutGEFS.R " + cv1.ToString("dd/MM/yy") + " " + cv2.ToString("dd/MM/yy"));
                    logF.WriteLine("Vies VE" + cv1.ToString("dd/MM/yy") + "   " + cv2.ToString("dd/MM/yy"));
                    executar_R(path_Conj, "vies_ve.R " + cv1.ToString("dd/MM/yy") + " " + cv2.ToString("dd/MM/yy") + " " + cv3.ToString("dd/MM/yy") + " " + cv4.ToString("dd/MM/yy"));
                    executar_R(path_Conj, "madeira.r");


                    //Organização das Rodada para rvx+1

                    logF.WriteLine("Criando Pastas RVX+1");

                  //  var path_ArqSaida = Path.Combine(path_Conj, "Arq_Saida");
                    var path_ArqSaida = Path.Combine(path_Conj, "madeira");


                    var vies_cv1 = Directory.GetFiles(Path.Combine(path_ArqSaida, "vies_" + cv1.ToString("dd-MM")));


                    Directory.CreateDirectory(Path.Combine(path_ArqSaida, "vies_" + cv2.ToString("dd-MM")));
                    var vies_cv2 = Directory.GetFiles(Path.Combine(path_ArqSaida, "vies_" + cv2.ToString("dd-MM")));

                    var vies_cv3 = Directory.GetFiles(Path.Combine(path_ArqSaida, "vies_" + cv3.ToString("dd-MM")));
                    var vies_cv4 = Directory.GetFiles(Path.Combine(path_ArqSaida, "vies_" + cv4.ToString("dd-MM")));



                    rvx1(path_Conj, "GEFS", "CV_VIES_VE", vies_cv1, vies_cv2);

                    logF.WriteLine("CV_VIES_VE Criada!");

                    rvx1(path_Conj, "GFS", "CV_GFS", vies_cv1, vies_cv2);

                    logF.WriteLine("CV_GFS Criada!");

                    rvx1(path_Conj, "ECMWF", "CV_EURO", vies_cv1, vies_cv2);

                    logF.WriteLine("CV_EURO Criada!");

                    rvx1(path_Conj, "ECMWFop", "CV_EUROop", vies_cv1, vies_cv2);

                    logF.WriteLine("CV_EURO_op Criada!");








                    //Organização das Rodada para rvx+2

                    logF.WriteLine("Criando Pastas RVX+2");

                    rvx2(path_Conj, "ECMWF", "CV2_EURO", vies_cv2);
                  //  MCP(cv2, Path.Combine(path_Conj, "CV2_EURO"), path_ModeloR);
                    logF.WriteLine("CV2_EURO Criada!");

                    rvx2(path_Conj, "ECMWFop", "CV2_EUROop", vies_cv2);
                  //  MCP(cv2, Path.Combine(path_Conj, "CV2_EUROop"), path_ModeloR);
                    logF.WriteLine("CV2_EURO_op Criada!");


                    rvx2(path_Conj, "GEFS", "CV2_GEFS", vies_cv2);
                 //   MCP(cv2, Path.Combine(path_Conj, "CV2_GEFS"), path_ModeloR);
                    logF.WriteLine("CV2_GEFS Criada!");

                    rvx2(path_Conj, "GFS", "CV2_GFS", vies_cv2);
                 //   MCP(cv2, Path.Combine(path_Conj, "CV2_GFS"), path_ModeloR);
                    logF.WriteLine("CV2_GFS Criada!");


                    rvxX(path_Conj, "GEFS", "CV3_GEFS", vies_cv3);
                    logF.WriteLine("CV3_GEFS Criada!");

                    rvxX(path_Conj, "ECMWF", "CV3_EURO", vies_cv3);
                    logF.WriteLine("CV3_ECMWF Criada!");

                    rvxX(path_Conj, "GEFS", "CV4_GEFS", vies_cv4);
                    logF.WriteLine("CV4_GEFS Criada!");

                    rvxX(path_Conj, "ECMWF", "CV4_EURO", vies_cv4);
                    logF.WriteLine("CV4_ECMWF Criada!");

                    //CV_FUNC 
                    //Remoção de vies a partir do dia atual, completando com MCP se necessário
                    logF.WriteLine("Criando Pasta CV_FUNC");

                    var arqs_PMEDIA = Directory.GetFiles(path_ArqSaida, "PM*.dat");
                    var cv_func = Path.Combine(path_Conj, "CV_FUNC");

                    Directory.CreateDirectory(cv_func);

                    foreach (var arq in arqs_PMEDIA)
                    {
                        System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"p(\d{2})(\d{2})(\d{2})a(\d{2})(\d{2})(\d{2})");

                        var data_mapa = r.Match(arq);

                        string mapa = data_mapa.ToString() + ".dat";


                        File.Copy(arq, Path.Combine(cv_func, mapa), true);
                    }
                    MCP_FUNC(cv1.AddDays(-(dias_ve+1)), Path.Combine(path_Conj, "CV_FUNC"), path_ModeloR);
                    logF.WriteLine("CV_FUNC Criada!");





                    //Completa com Funceme se não tiver acomph referente a data

                    if (data_Atual != dt_acomph)
                    {
                        logF.WriteLine("Acomph desatualizado, renoamendo arquivos");
                        var dirs = Directory.GetDirectories(path_Conj).Where(x => x.Split('\\').Last().StartsWith("CV"));
                        var arq_funceme = Directory.GetFiles(Path.Combine(path_ArqSaida, "funceme"));

                        foreach (var arq in arq_funceme)
                        {
                            foreach (var dir in dirs)
                            {

                                File.Copy(arq, Path.Combine(dir, arq.Split('\\').Last()), true);
                                Atualiza_DT(dir, dt_acomph);
                            }
                        }

                    }

                    if (data_Atual.DayOfWeek == DayOfWeek.Friday)
                    {
                        var count_mapas = Directory.GetFiles(Path.Combine(path_Conj, "CV_EURO")).Count();
                        if (count_mapas < 15) MCP_rv1(dt_acomph, Path.Combine(path_Conj, "CV_VIES_VE"), path_ModeloR, true);
                        if (count_mapas < 15) MCP_rv1(dt_acomph, Path.Combine(path_Conj, "CV_GFS"), path_ModeloR, true);
                        MCP_rv1(dt_acomph, Path.Combine(path_Conj, "CV_EURO"), path_ModeloR);
                        MCP_rv1(dt_acomph, Path.Combine(path_Conj, "CV_EUROop"), path_ModeloR);
                        MCP_rv1(dt_acomph, Path.Combine(path_Conj, "CV_FUNC"), path_ModeloR);
                    }






                }

                var dirs_cvs = Directory.GetDirectories(path_Conj).Where(x => x.Split('\\').Last().StartsWith("CV"));

                foreach (var dir in dirs_cvs)
                {
                    var name_cv = dir.Split('\\').Last().Split('_').First();

                    if (!Directory.Exists(Path.Combine(path_Conj, name_cv)))
                    {
                        Directory.CreateDirectory(Path.Combine(path_Conj, name_cv));
                    }

                    DirectoryCopy(dir, Path.Combine(path_Conj, name_cv, dir.Split('\\').Last()), true);
                    Directory.Delete(dir, true);


                }

                logF.WriteLine("Mapas Gerados com Sucesso!");

            }
            catch (Exception a)
            {
                var log_C = Path.Combine(path_Conj, "error.log");


                File.WriteAllText(log_C, a.ToString());
                logF.WriteLine("Erro ao Gerar Mapas");
            }


        }

        internal static void Hist_Entrada(string modelo, string path_Conj, string path_Previsao, DateTime data_Atual)
        {
            //Completa Historico Arq Entrada
            var arq_Ent_EURO = Directory.GetFiles(Path.Combine(path_Conj, "Arq_Entrada", modelo));

            var data_hist = data_Atual;
            var File_modelo = modelo + "_m_" + data_hist.ToString("ddMMyy") + ".dat";
            var Path_modelo = Path.Combine(path_Conj, "Arq_Entrada", modelo, File_modelo);

            while (!File.Exists(Path_modelo))
            {
                var arq_Dia_euro = Path.Combine(path_Previsao, data_hist.ToString("yyyyMM"), data_hist.ToString("dd"), File_modelo);
                if (File.Exists(arq_Dia_euro)) File.Copy(arq_Dia_euro, Path_modelo);

                data_hist = data_hist.AddDays(-1);
                File_modelo = modelo + "_m_" + data_hist.ToString("ddMMyy") + ".dat";
                Path_modelo = Path.Combine(path_Conj, "Arq_Entrada", modelo, File_modelo);
            }

        }

        internal static void Atualiza_DT(string dir, DateTime dt_acomph)
        {


            var arqs = Directory.GetFiles(dir);

            foreach (var arq in arqs)
            {
                var nome = arq.Split('\\').Last();

                var fim_nome = nome.Split('.').First().Split('a').Last();

                var nome_Final = "p" + dt_acomph.ToString("ddMMyy") + "a" + fim_nome + ".dat";

                File.Move(arq, Path.Combine(dir, nome_Final));

            }
        }

        internal static void rvx1(string path_Conj, string modelo, string nome_path, string[] vies_cv1, string[] vies_cv2)
        {

            var path_cv = Path.Combine(path_Conj, nome_path);
            //    var path_ArqSaida = Path.Combine(path_Conj, "Arq_Saida");
            var path_ArqSaida = Path.Combine(path_Conj, "madeira");
            Directory.CreateDirectory(path_cv);
            var out_Modelo = Directory.GetFiles(Path.Combine(path_ArqSaida, modelo));
            var Modelo1 = out_Modelo.Where(File => !vies_cv1.Any(x => File.EndsWith(x.Split('\\').Last(), StringComparison.OrdinalIgnoreCase)));
            var Modelo2 = Modelo1.Where(File => !vies_cv2.Any(x => File.EndsWith(x.Split('\\').Last(), StringComparison.OrdinalIgnoreCase)));

            DateTime data_final = DateTime.Today.AddDays(-1);
            foreach (var arq_CV in vies_cv1)
            {
                File.Copy(arq_CV, Path.Combine(path_cv, arq_CV.Split('\\').Last()), true);
                var data_arq = DateTime.ParseExact(arq_CV.Split('\\').Last().Split('.').First().Split('a').Last(), "ddMMyy", System.Globalization.CultureInfo.InvariantCulture);
                if (data_arq >= data_final)
                {
                    data_final = data_arq;
                }
            }

            foreach (var arq in Modelo2)
            {
                var data_arq = DateTime.ParseExact(arq.Split('\\').Last().Split('.').First().Split('a').Last(), "ddMMyy", System.Globalization.CultureInfo.InvariantCulture);
                if (data_arq <= data_final)
                {
                    File.Copy(arq, Path.Combine(path_cv, arq.Split('\\').Last()), true);
                }
            }


        }

        internal static void rvx2(string path_Conj, string modelo, string nome_path, string[] vies_cv2)
        {

            var path_cv = Path.Combine(path_Conj, nome_path);
            //  var path_ArqSaida = Path.Combine(path_Conj, "Arq_Saida");
            var path_ArqSaida = Path.Combine(path_Conj, "madeira");
            Directory.CreateDirectory(path_cv);



            var out_Modelo = Directory.GetFiles(Path.Combine(path_ArqSaida, modelo));

            var Modelo2 = out_Modelo.Where(File => !vies_cv2.Any(x => File.EndsWith(x.Split('\\').Last(), StringComparison.OrdinalIgnoreCase)));

            DateTime data_final = DateTime.Today.AddDays(-1);

            foreach (var arq_CV in vies_cv2)
            {
                File.Copy(arq_CV, Path.Combine(path_cv, arq_CV.Split('\\').Last()), true);
                var data_arq = DateTime.ParseExact(arq_CV.Split('\\').Last().Split('.').First().Split('a').Last(), "ddMMyy", System.Globalization.CultureInfo.InvariantCulture);
                if (data_arq >= data_final)
                {
                    data_final = data_arq;
                }
            }


            foreach (var arq in Modelo2)
            {
                var data_arq = DateTime.ParseExact(arq.Split('\\').Last().Split('.').First().Split('a').Last(), "ddMMyy", System.Globalization.CultureInfo.InvariantCulture);
                if (data_arq <= data_final)
                {
                    File.Copy(arq, Path.Combine(path_cv, arq.Split('\\').Last()), true);
                }
            }

            if (modelo == "ECMWFop")
            {
                var arqs_ONS = Directory.GetFiles(Path.Combine(path_ArqSaida, "ECMWF"));

                var Modelo3 = arqs_ONS.Where(File => !vies_cv2.Any(x => File.EndsWith(x.Split('\\').Last(), StringComparison.OrdinalIgnoreCase)));
                var arqs_GEFS_EURO = Modelo3.Where(File => !Modelo2.Any(x => File.EndsWith(x.Split('\\').Last(), StringComparison.OrdinalIgnoreCase)));

                foreach (var arq_Euro in arqs_GEFS_EURO)
                {
                    var data_arq = DateTime.ParseExact(arq_Euro.Split('\\').Last().Split('.').First().Split('a').Last(), "ddMMyy", System.Globalization.CultureInfo.InvariantCulture);
                    if (data_arq <= data_final)
                    {
                        File.Copy(arq_Euro, Path.Combine(path_cv, arq_Euro.Split('\\').Last()), true);
                    }
                }
            }



        }

        internal static void rvxX(string path_Conj, string modelo, string nome_path, string[] vies_cv)
        {

            var path_cv = Path.Combine(path_Conj, nome_path);
            //  var path_ArqSaida = Path.Combine(path_Conj, "Arq_Saida");
            var path_ArqSaida = Path.Combine(path_Conj, "madeira");
            Directory.CreateDirectory(path_cv);



            var out_Modelo = Directory.GetFiles(Path.Combine(path_ArqSaida, modelo));

            var Modelo2 = out_Modelo.Where(File => !vies_cv.Any(x => File.EndsWith(x.Split('\\').Last(), StringComparison.OrdinalIgnoreCase)));

            DateTime data_final = DateTime.Today.AddDays(-1);
            foreach (var arq_CV in vies_cv)
            {
                File.Copy(arq_CV, Path.Combine(path_cv, arq_CV.Split('\\').Last()), true);

                var data_arq = DateTime.ParseExact(arq_CV.Split('\\').Last().Split('.').First().Split('a').Last(), "ddMMyy", System.Globalization.CultureInfo.InvariantCulture);
                if (data_arq >= data_final)
                {
                    data_final = data_arq;
                }
            }

            foreach (var arq in Modelo2)
            {
                var data_arq = DateTime.ParseExact(arq.Split('\\').Last().Split('.').First().Split('a').Last(), "ddMMyy", System.Globalization.CultureInfo.InvariantCulture);
                if (data_arq <= data_final)
                {
                    File.Copy(arq, Path.Combine(path_cv, arq.Split('\\').Last()), true);
                }
            }




        }

        internal static void GEFS_Ext(DateTime cv, string path)
        {
            var dt = DateTime.Today.AddDays(-1);
            var oneDrivePath_ori = Environment.GetEnvironmentVariable("OneDriveCommercial");
            //B:\Compass\MinhaTI\Alex Freires Marques - Compass\Trading
            var oneDrive = Path.Combine(oneDrivePath_ori, @"Energy Core Pricing - Documents\Acompanhamento_de_Precipitacao\Previsao\");
            if (!Directory.Exists(oneDrive))
            {
                oneDrive = oneDrive.Replace("Energy Core Pricing - Documents", "Energy Core Pricing - Documentos");
            }

            var oneDrive_gefs = Path.Combine(oneDrive, dt.ToString("yyyy"), dt.ToString("MM"), dt.ToString("dd"), "GEFS_0.5_00");
            while (!Directory.Exists(oneDrive_gefs))
            {
                dt = dt.AddDays(-1);
                oneDrive_gefs = Path.Combine(oneDrive, dt.ToString("yyyy"), dt.ToString("MM"), dt.ToString("dd"), "GEFS_0.5_00");
            }
            if (Directory.Exists(oneDrive_gefs))
            {
                var files_gefs = Directory.GetFiles(oneDrive_gefs);
                while (files_gefs.Count() < 30)
                {
                    dt = dt.AddDays(-1);
                    oneDrive_gefs = Path.Combine(oneDrive, dt.ToString("yyyy"), dt.ToString("MM"), dt.ToString("dd"), "GEFS_0.5_00");
                    files_gefs = Directory.GetFiles(oneDrive_gefs);
                }

                var arqs = Directory.GetFiles(path);
                //for (int i = 0; i <= dias; i++)
                for (int i = 0; i <= files_gefs.Count(); i++)
                {
                    var data = DateTime.Today.AddDays(i + 1);
                    if (!File.Exists(Path.Combine(path, "GEFS_p" + DateTime.Today.ToString("ddMMyy") + "a" + data.ToString("ddMMyy") + ".dat")))
                    {
                        var file_gefs = files_gefs.Where(x => x.Contains(data.ToString("ddMMyy") + ".dat")).FirstOrDefault();
                        try
                        {
                            File.Copy(file_gefs, Path.Combine(path, "GEFS_p" + DateTime.Today.ToString("ddMMyy") + "a" + data.ToString("ddMMyy") + ".dat"));
                        }
                        catch { }
                        //File.Copy(Path.Combine(Modelo_R, "MCP", "prec_mct1318_" + data.Month.ToString().PadLeft(2, '0') + ".dat"), Path.Combine(path, "p" + DateTime.Today.ToString("ddMMyy") + "a" + data.ToString("ddMMyy") + ".dat"), true);

                    }
                }
            }
        }

        internal static void MCP(DateTime cv, string path, string Modelo_R)
        {
            var arqs = Directory.GetFiles(path);
            for (int i = 1; i <= 12; i++)
            {
                var data = cv.AddDays(i);
                if (!File.Exists(Path.Combine(path, "p" + DateTime.Today.ToString("ddMMyy") + "a" + data.ToString("ddMMyy") + ".dat")))
                {
                    File.Copy(Path.Combine(Modelo_R, "MCP", "prec_mct1318_" + data.Month.ToString().PadLeft(2, '0') + ".dat"), Path.Combine(path, "p" + DateTime.Today.ToString("ddMMyy") + "a" + data.ToString("ddMMyy") + ".dat"), true);

                }
            }
        }

        internal static void MCP_rv1(DateTime dt, string path, string Modelo_R, Boolean Gfs = false)
        {
            var arqs = Directory.GetFiles(path);
            var dias = arqs.Count();
            if (dias < 15)
            {
                for (int i = 1; i <= 15; i++)
                {
                    var data = dt.AddDays(i);
                    if (!File.Exists(Path.Combine(path, "p" + dt.ToString("ddMMyy") + "a" + data.ToString("ddMMyy") + ".dat")))
                    {
                        File.Copy(Path.Combine(Modelo_R, "MCP", "prec_mct1318_" + data.Month.ToString().PadLeft(2, '0') + ".dat"), Path.Combine(path, "p" + dt.ToString("ddMMyy") + "a" + data.ToString("ddMMyy") + ".dat"), true);

                    }
                }
            }
            else if (Gfs)
            {

                var data = DateTime.Today.AddDays(dias - 1);

                var last_file = Path.Combine(path, "p" + dt.ToString("ddMMyy") + "a" + data.ToString("ddMMyy") + ".dat");
                if (File.Exists(last_file))
                {
                    File.Copy(Path.Combine(Modelo_R, "MCP", "prec_mct1318_" + data.Month.ToString().PadLeft(2, '0') + ".dat"), Path.Combine(path, "p" + dt.ToString("ddMMyy") + "a" + data.ToString("ddMMyy") + ".dat"), true);

                }

            }
        }


        internal static void MCP_FUNC(DateTime cv, string path, string Modelo_R)
        {
            var arqs = Directory.GetFiles(path);
            for (int i = 1; i <= 18; i++)
            {
                var data = cv.AddDays(i);
                if (!File.Exists(Path.Combine(path, "p" + DateTime.Today.ToString("ddMMyy") + "a" + data.ToString("ddMMyy") + ".dat")) && !File.Exists(Path.Combine(path, "PMEDIA_p" + DateTime.Today.ToString("ddMMyy") + "a" + data.ToString("ddMMyy") + ".dat")))
                {
                    File.Copy(Path.Combine(Modelo_R, "MCP", "prec_mct1318_" + data.Month.ToString().PadLeft(2, '0') + ".dat"), Path.Combine(path, "p" + DateTime.Today.ToString("ddMMyy") + "a" + data.ToString("ddMMyy") + ".dat"), true);

                }
            }
        }


        internal static DateTime ECMWF_Ext(DateTime cv, string path, int dias = 14)
        {
            var dt = DateTime.Today;
            var data_final = DateTime.Today;
            var oneDrivePath_ori = Environment.GetEnvironmentVariable("OneDriveCommercial");
            //B:\Compass\MinhaTI\Alex Freires Marques - Compass\Trading
            var oneDrive = Path.Combine(oneDrivePath_ori, @"Energy Core Pricing - Documents\Acompanhamento_de_Precipitacao\Previsao\");
            if (!Directory.Exists(oneDrive))
            {
                oneDrive = oneDrive.Replace("Energy Core Pricing - Documents", "Energy Core Pricing - Documentos");
            }
            var oneDrive_ecmwf = Path.Combine(oneDrive, dt.ToString("yyyy"), dt.ToString("MM"), dt.ToString("dd"), "ECMWF45");
            while (!Directory.Exists(oneDrive_ecmwf))
            {
                dt = dt.AddDays(-1);
                oneDrive_ecmwf = Path.Combine(oneDrive, dt.ToString("yyyy"), dt.ToString("MM"), dt.ToString("dd"), "ECMWF45");
            }
            if (Directory.Exists(oneDrive_ecmwf))
            {
                var files_ecmwf = Directory.GetFiles(oneDrive_ecmwf);
                while (files_ecmwf.Count() < 30)
                {
                    dt = dt.AddDays(-1);
                    oneDrive_ecmwf = Path.Combine(oneDrive, dt.ToString("yyyy"), dt.ToString("MM"), dt.ToString("dd"), "ECMWF45");
                    files_ecmwf = Directory.GetFiles(oneDrive_ecmwf);
                }

                var arqs = Directory.GetFiles(path);
                //for (int i = 0; i <= dias; i++)
                for (int i = 0; i <= files_ecmwf.Count(); i++)
                {



                    var data = DateTime.Today.AddDays(i + 1);
                    if (!File.Exists(Path.Combine(path, "ECMWF_p" + DateTime.Today.ToString("ddMMyy") + "a" + data.ToString("ddMMyy") + ".dat")))
                    {
                        var file_gefs = files_ecmwf.Where(x => x.Contains(data.ToString("ddMMyy") + ".dat")).FirstOrDefault();
                        try
                        {
                            if (data >= data_final)
                            {
                                data_final = data;
                            }
                            File.Copy(file_gefs, Path.Combine(path, "ECMWF_p" + DateTime.Today.ToString("ddMMyy") + "a" + data.ToString("ddMMyy") + ".dat"));
                        }
                        catch { }
                        //File.Copy(Path.Combine(Modelo_R, "MCP", "prec_mct1318_" + data.Month.ToString().PadLeft(2, '0') + ".dat"), Path.Combine(path, "p" + DateTime.Today.ToString("ddMMyy") + "a" + data.ToString("ddMMyy") + ".dat"), true);

                    }
                }
            }
            return data_final;
        }

        static void executar_R(string path, string Comando)
        {

            //string executar = @"/C " + "H: & cd " + txtCaminho.Text + "& bat.bat";


            //string executar = @"/c " + "N: & cd Middle - Preço\\16_Chuva_Vazao\\Conjunto-PastasEArquivos/ & bat.bat";


            var letra_Dir = path.Split('\\').First();
            var path_Scripts = @"C:\Sistemas\ChuvaVazao\remocao_R\scripts\";
            string executar = @"/C " + letra_Dir + " & cd " + path + @" & Rscript.exe " + path_Scripts + Comando;


            System.Diagnostics.Process.Start("cmd.exe", executar).WaitForExit(1200000);

        

        }
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, true);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }





    }


}



