﻿using GradsHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace ChuvaVazaoTools
{
    public static class Program
    {
        public static Tuple<string, string> GetPrevivazExPath(string path)
        {
            string anchorKeyD = @"SOFTWARE\Classes\*\shell\decompToolsShellX";
            string ctxMenuD = @"SOFTWARE\Classes\*\ContextMenus\decompToolsShellX";

            try
            {
                var k = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(anchorKeyD);

                var k2 = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(ctxMenuD);
                k2 = k2.OpenSubKey("shell");

                var k2_1 = k2.OpenSubKey("cmd3");
                var p = k2_1.OpenSubKey("command").GetValue("");

                var fcmd = p.ToString().Replace("%1", path);

                var tm = fcmd.Split(new string[] { " previvaz " }, StringSplitOptions.None);

                var ret = new Tuple<string, string>(tm[0], fcmd.Substring(tm[0].Length));

                return ret;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Config.Read();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length > 0)
            {

                try
                {
                    if (args[0].Equals("auto", StringComparison.OrdinalIgnoreCase))
                    {
                        while (true)
                        {
                            RunAutoRoutines("all");
                            Thread.Sleep(600000);
                        }
                       
                    }
                    else
                    {
                        RunAutoRoutines(args[0].ToLower());
                    }
                }
                finally { }
            }
            else
            {
                Application.Run(new FrmMain());
            }

        }

        private static void RunAutoRoutines(string tipo)
        {

            var logFile = Config.CaminhoLogAutoRun;

            var logF = new LogFile(logFile);
            logF.WriteLine("Iniciando AutoRoutine");

            var data = DateTime.Today;

            switch (tipo)
            {
                case "download":
                    logF.WriteLine("Iniciando AutoDownload");//System.Environment.UserName   C:\Sistemas\ChuvaVazao\Log
                    Tools.Tools.addHistory(@"C:\LOGS SISTEMAS\Chuva Vazao" + "\\downloadViaSelf.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "- tentativa de fazer o download(download) via Self Enforcing");
                    Tools.Tools.addHistory(@"C:\Sistemas\ChuvaVazao\Log\" + "LogChuva_Down.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss ") + System.Environment.UserName.ToString() + " - tentativa de executar o download via Self Enforcing");

                    AutoDownload(data, logF);
                    break;
                case "run":
                    logF.WriteLine("Iniciando AutoRun");
                   
                    var p_count = Process.GetProcesses().Where(p => p.ProcessName.Contains("ChuvaVazaoTools")).Count();
                    Tools.Tools.addHistory(@"C:\Sistemas\ChuvaVazao\Log\" + "LogChuva_Run.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss ") + System.Environment.UserName.ToString() + " - tentativa de executar as rodadas via Self Enforcing");


                    if (p_count <= 2)
                    {
                        AutoRun_R(data, logF);
                        Tools.Tools.addHistory(@"C:\Sistemas\ChuvaVazao\Log\" + "LogChuva_Run.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss ") + System.Environment.UserName.ToString() + " - tentativa de executar as rodadas via Self Enforcing - Dentro dos Processos");
                    }
                    break;
                case "report":
                    logF.WriteLine("Iniciando AutoReport");
                    Tools.Tools.addHistory(@"C:\Sistemas\ChuvaVazao\Log\" + "LogChuva_Report.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss ") + System.Environment.UserName.ToString() + " - tentativa de executar relatórios via Self Enforcing");

                    AutoReport(data, logF);
                    break;
                case "sistema":
                    logF.WriteLine("Iniciando rodadas em Sistema");

                    var prCount = Process.GetProcesses().Where(p => p.ProcessName.Contains("ChuvaVazaoTools")).Count();
                    Tools.Tools.addHistory(@"C:\Sistemas\ChuvaVazao\Log\" + "LogChuva_Run.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss ") + System.Environment.UserName.ToString() + " - tentativa de executar as rodadas [SEM EXCEL] via Self Enforcing");


                    if (prCount <= 10)
                    {
                        AutoExec(data, logF);
                        Tools.Tools.addHistory(@"C:\Sistemas\ChuvaVazao\Log\" + "LogChuva_Run.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss ") + System.Environment.UserName.ToString() + " - tentativa de executar as rodadas [SEM EXCEL] via Self Enforcing - Dentro dos Processos");
                    }
                    
                    break;
                case "all": //roda tudo
                    logF.WriteLine("Iniciando Tudo");
                    Tools.Tools.addHistory(@"C:\Sistemas\ChuvaVazao\Log\" + "LogChuva_All.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss ") + System.Environment.UserName.ToString() + " - tentativa de execução total via Self Enforcing");

                    AutoDownload(data, logF);
                    AutoRun_R(data, logF);
                    AutoReport(data, logF);
                    break;
                default:
                    break;
            }

            logF.WriteLine("Encerrando AutoRoutine");
        }


        internal static void AutoDownload(DateTime date, System.IO.TextWriter logF)
        {
            var config = Config.ConfigConjunto;

            var searchPath = System.IO.Path.Combine(Config.CaminhoPrevisao, date.ToString("yyyyMM"), date.ToString("dd"));

            if (!System.IO.Directory.Exists(searchPath)) System.IO.Directory.CreateDirectory(searchPath);
            string funEuro = string.Empty;
            #region Download Funceme 
            //FUNCEME
            try
            {

                var directoryToSaveGif = @"C:\Files\Trading\Acompanhamento Metereologico Semanal\spiderman\" + date.ToString("yyyy_MM_dd") + @"\OBSERVADO";

                List<string> h;

                if (!File.Exists(Path.Combine(directoryToSaveGif, "statusFunceme.txt")))
                    File.Create(Path.Combine(directoryToSaveGif, "statusFunceme.txt"));

                h = Tools.Tools.readHistory(Path.Combine(directoryToSaveGif, "statusFunceme.txt")).ToList();


                if (DateTime.Now.Hour >= 7 && (h.Count() == 0) || h.Any(y => y.Contains("EURO")) || DateTime.Now.Hour >= 11)
                {
                    string horaFunc = "";
                    Boolean late = false;
                    if (DateTime.Now.Hour >= 11)
                    {
                        late = true;
                        horaFunc = "LATE_";
                    }
                    logF.WriteLine("FUNCEME");

                    var pr = cptec.DownloadFunceme(horaFunc);
                    string[] filesInside;
                    string path = string.Empty;

                    #region Euro Funceme 

                    //caso os dados de precipitação estejam vazios, para o dia atual, são coletados os dados antigos(quanto mais recente, melhor) de EURO

                    if (pr == null && pr.Prec.Any(x => x.Value == 0) && h.Count() == 0)
                    {


                        pr = null;

                        for (int x = 1; true; x++)
                            if (System.IO.Directory.Exists(Path.Combine("C:\\Files\\Middle - Preço\\Acompanhamento de Precipitação\\Previsao_Numerica", date.AddDays(-x).ToString("yyyyMM"), date.AddDays(-x).ToString("dd"), "ECMWF00")))
                            {
                                path = Path.Combine("C:\\Files\\Middle - Preço\\Acompanhamento de Precipitação\\Previsao_Numerica", date.AddDays(-x).ToString("yyyyMM"), date.AddDays(-x).ToString("dd"), "ECMWF00");

                                filesInside = Directory.GetFiles(path);
                                break;
                            }


                        foreach (var file in filesInside.Where(y => y.Contains("ctl")))
                        {
                            pr = PrecipitacaoFactory.BuildFromMergeFile(file);

                            if (pr.Data == date && pr.Prec.Count != 0)
                            {
                                funEuro = " do Euro";
                                Tools.Tools.addHistory(directoryToSaveGif, "Funceme do EURO");
                                var retornoEmail = Tools.Tools.SendMail("", "Dados coletados em: http://mobile.funceme.br/tempo/inmet.php?acao=4&sensor=22&intervalo=24 <br> Será utilizado os dados anteriormente fornecidos pelo modelo do Euro.", "Alerta de Funceme Vazio [AUTO]", "preco");
                                break;
                            }
                            pr = null;
                        }
                    }



                    #endregion

                    if (pr != null && (h.Count() == 0 || h.Any(y => y.Contains("EURO")) || (late == true && !File.Exists(Path.Combine(directoryToSaveGif, "LateFunceme.txt")))))
                    {
                        var localPath = System.IO.Path.GetTempPath() + "FUNCEME\\";
                        if (!System.IO.Directory.Exists(localPath)) System.IO.Directory.CreateDirectory(localPath);

                        var fanem = System.IO.Path.Combine(localPath, horaFunc + "funceme_" + date.ToString("yyyyMMdd"));
                        pr.SalvarModeloBin(fanem);
                        Grads.ConvertCtlToImg(fanem, "FUNCEME" + funEuro, "Precipacao observada entre " + date.AddDays(-1).ToString("dd/MM") + " e " + date.ToString("dd/MM"), horaFunc + "funceme.gif", System.IO.Path.Combine(Config.CaminhoAuxiliar, "CREATE_GIF.gs")); cptec.CopyGifs(localPath, directoryToSaveGif);
                        cptec.CopyBin(localPath, System.IO.Path.Combine(Config.CaminhoFunceme, date.Year.ToString("0000"), date.Month.ToString("00")));

                        var remo = new PrecipitacaoConjunto(config);
                        var dic = new Dictionary<DateTime, Precipitacao>();
                        dic[pr.Data] = pr;
                        var chuvaMEDIA = remo.ConjuntoLivre(dic, null);

                        fanem = System.IO.Path.Combine(localPath, horaFunc + "funcememed_" + date.ToString("yyyyMMdd"));
                        chuvaMEDIA[date].SalvarModeloBin(fanem);
                        Grads.ConvertCtlToImg(fanem, "FUNCEME Medio", "Precipacao observada entre " + date.AddDays(-1).ToString("dd/MM") + " e " + date.ToString("dd/MM"), horaFunc + "funcememed.gif", System.IO.Path.Combine(Config.CaminhoAuxiliar, "CREATE_GIF.gs"));
                        cptec.CopyGifs(localPath, directoryToSaveGif);

                        System.IO.Directory.Delete(localPath, true);

                        foreach (var pCo in remo.RegioesConjunto.Where(x => x.Modelo == "CONJ"))
                        {
                            for (int i = 0; i < chuvaMEDIA.Keys.Count; i++)
                            {
                                PrecipitacaoRepository.SaveAverage(chuvaMEDIA.Keys.ToArray()[i], pCo.Agrupamento.Nome, pCo.Nome, pCo.precMedia[i], "FUNCEME");
                            }

                        }

                        remo = new PrecipitacaoConjunto(config);
                        var chuvaMediaBacia = remo.MediaBacias(dic);
                        foreach (var pCo in remo.RegioesConjunto.Where(x => x.Modelo == "CONJ").GroupBy(x => x.Agrupamento))
                        {
                            for (int i = 0; i < chuvaMediaBacia.Keys.Count; i++)
                            {
                                PrecipitacaoRepository.SaveAverage(chuvaMediaBacia.Keys.ToArray()[i], pCo.Key.Nome, "", pCo.First().precMedia[i], "FUNCEME");
                            }
                        }

                        if (funEuro == "")
                        {
                            File.WriteAllText(Path.Combine(directoryToSaveGif, "statusFunceme.txt"), "Funceme Oficial");
                        }
                        if (late == true && File.Exists(Path.Combine(directoryToSaveGif, horaFunc + "funceme.gif")))
                        {
                            File.WriteAllText(Path.Combine(directoryToSaveGif, "LateFunceme.txt"), "Funceme Late");
                            
                        }
                    }

                    else
                        logF.WriteLine("FUNCEME OK");

                    var oneDrive_equip = Path.Combine(@"B:\Enercore\Energy Core Trading\Energy Core Pricing - Documents\Acompanhamento_de_Precipitacao");

                    if (!Directory.Exists(oneDrive_equip))
                    {
                        oneDrive_equip = oneDrive_equip.Replace("Documents", "Documentos");
                    }

                    var oneDrive_Gif = Path.Combine(oneDrive_equip, "Mapas", date.ToString("yyyy"), date.ToString("MM"), date.ToString("dd"), "OBSERVADO");

                    if (!Directory.Exists(oneDrive_Gif))
                    {
                        Directory.CreateDirectory(oneDrive_Gif);
                    }

                    foreach (string newPath in Directory.GetFiles(directoryToSaveGif, ".",
                        SearchOption.AllDirectories))
                        File.Copy(newPath, newPath.Replace(directoryToSaveGif, oneDrive_Gif), true);

                }


            }
            catch (Exception ex)
            {
                logF.WriteLine(ex.ToString());
            }
            #endregion

            #region Download Merge
            //MERGE
            try
            {
                  

                var pastaMerge = Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Previsao_Numerica\Modelo_R\merge\", DateTime.Today.ToString("yyyyMM"), DateTime.Today.ToString("dd"));
                if (!Directory.Exists(pastaMerge))
                {
                    Directory.CreateDirectory(pastaMerge);
                }
                //==================

                logF.WriteLine("MERGE");
                var resp = cptec.ListNewMerge(logF);

                if (resp.Contains("baixando Merge do dia"))
                {
                    var retornoEmail = Tools.Tools.SendMail("", "Precipitação obvservada (MERGE) mais recente disponível", "Precipitação Obvservada [AUTO]", "preco");
             
                }

                //create image
                if (!resp.Equals("Nada novo", StringComparison.OrdinalIgnoreCase))
                {
                    
           
                   var chuvasMerge = new Dictionary<DateTime, Precipitacao>();

                    var localPath =Path.Combine(System.IO.Path.GetTempPath(), "MERGE"+DateTime.Now.ToString("HHmmss"));
                    if (!System.IO.Directory.Exists(localPath)) System.IO.Directory.CreateDirectory(localPath);
                    
                    for (DateTime dt = date.AddDays(-7); dt <= date.Date; dt = dt.AddDays(1))
                    {
                        try
                        {
                            
                            var mergeCtlFile = System.IO.Directory.GetFiles(Path.Combine(Config.CaminhoMerge, dt.ToString("yyyy")), "MERGE_CPTEC_" + dt.ToString("yyyyMMdd") + ".ctl", System.IO.SearchOption.AllDirectories);
                            var mergeGrib2File = System.IO.Directory.GetFiles(Path.Combine(Config.CaminhoMerge, dt.ToString("yyyy")), "MERGE_CPTEC_" + dt.ToString("yyyyMMdd") + ".grib2", System.IO.SearchOption.AllDirectories);
                            var mergeIdxFile = System.IO.Directory.GetFiles(Path.Combine(Config.CaminhoMerge, dt.ToString("yyyy")), "MERGE_CPTEC_" + dt.ToString("yyyyMMdd") + ".idx", System.IO.SearchOption.AllDirectories);

                          

                            if (mergeCtlFile.Length == 1)
                            {
                                
                                File.Copy(mergeCtlFile[0], Path.Combine(localPath, "merge_" + dt.ToString("yyyyMMdd") + ".ctl"), true);
                                File.Copy(mergeGrib2File[0], Path.Combine(localPath, mergeGrib2File[0].Split('\\').Last()), true);
                                File.Copy(mergeIdxFile[0], Path.Combine(localPath, mergeIdxFile[0].Split('\\').Last()), true);
                                var fanem = System.IO.Path.Combine(localPath, "merge_" + dt.ToString("yyyyMMdd"));

                                Grads.ConvertCtlToImg(fanem, "MERGE", "Precipacao observada entre " + dt.AddDays(-1).ToString("dd/MM") + " e " + dt.ToString("dd/MM"), "merge_" + dt.ToString("ddMMyy") + ".gif", System.IO.Path.Combine(Config.CaminhoAuxiliar, "CREATE_DAT_Merge.gs"));
                                
                                File.Copy(Path.Combine(localPath, "merge_" + dt.ToString("ddMMyy") + ".dat"), Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Previsao_Numerica\Modelo_R\merge\", dt.ToString("yyyyMM"), dt.ToString("dd"), "merge_p" + dt.ToString("ddMMyy") + "a" + dt.ToString("ddMMyy") + ".dat"), true);
                                
                                cptec.CopyGifs(localPath, @"C:\Files\Trading\Acompanhamento Metereologico Semanal\spiderman\" + dt.ToString("yyyy_MM_dd") + @"\OBSERVADO");



                            }
                        }
                        catch(Exception erro)
                        {
                            logF.WriteLine(erro.ToString());
                        }
                    }
                  
                    System.IO.Directory.Delete(localPath, true);
                }

            }
            catch (Exception ex)
            {
                logF.WriteLine(ex.ToString());
            }
            #endregion

            #region Download ETA CPTEC
            //ETA
            try
            {
                IPrecipitacaoForm frm = null;


                var funcLogs = new Action<string>(hora =>
                {

                    WaitForm2.TipoEta eta;
                    WaitForm2.TipoGefs gefs = WaitForm2.TipoGefs._00h;

                    switch (hora)
                    {
                        case "00":
                            eta = WaitForm2.TipoEta._00h;
                            break;
                        case "12":
                            eta = WaitForm2.TipoEta._12h;
                            break;
                        default:
                            return;
                    }
                    frm.Eta = eta;
                    frm.Gefs = gefs;
                    frm.Tipo = WaitForm.TipoConjunto.Eta40;
                    frm.RemoveLimiteETA = false;
                    frm.RemoveViesETA = false;
                    frm.SalvarDados = true;

                    frm.ProcessarConjunto(saveLogFile: System.IO.Path.Combine(searchPath, "eta" + hora + ".log"));
                });

                if (!System.IO.File.Exists(System.IO.Path.Combine(searchPath, "eta00.log")) || !System.IO.Directory.Exists(System.IO.Path.Combine(searchPath, "ETA00")))
                {
                    logF.WriteLine("ETA 00");
                    cptec.DownloadETA40_Atual(date, logF, "00");
                

                    frm = WaitForm2.CreateInstance(date);

                    if (frm.TemEta00)
                    {
                        funcLogs("00");
                    }
                }
                else logF.WriteLine("ETA 00 OK");

            }
            catch (Exception ex)
            {
                logF.WriteLine(ex.ToString());
            }
            #endregion

            #region Download GEFS Tropical TidBits  
            //GEFS
            try
            {

                IPrecipitacaoForm frm = null;

                var funcLogs = new Action<string>(hora =>
                {

                    var eta = WaitForm2.TipoEta._00h;
                    WaitForm2.TipoGefs gefs;

                    switch (hora)
                    {
                        case "00":
                            gefs = WaitForm2.TipoGefs._00h;
                            break;
                        case "06":
                            gefs = WaitForm2.TipoGefs._06h;
                            break;
                        case "12":
                            gefs = WaitForm2.TipoGefs._12h;
                            break;
                        default:
                            return;
                    }

                    frm.Eta = eta;
                    frm.Gefs = gefs;
                    frm.Tipo = WaitForm.TipoConjunto.Gefs;
                    frm.RemoveLimiteGEFS = false;
                    frm.RemoveViesGEFS = false;
                    frm.SalvarDados = true;

                    frm.ProcessarConjunto(saveLogFile: System.IO.Path.Combine(searchPath, "gefs" + hora + ".log"));
                });


                if (!System.IO.File.Exists(System.IO.Path.Combine(searchPath, "gefs00.log")))
                {

                    logF.WriteLine("GEFS 00");
                    cptec.DownloadNoaaImgs(date, logF, "GEFS", "00");

                    frm = WaitForm2.CreateInstance(date);

                    if (frm.TemGefs00)
                    {
                        funcLogs("00");

                    }
                }
                else logF.WriteLine("GEFS 00 OK");
                if (!System.IO.File.Exists(System.IO.Path.Combine(searchPath, "gefs06.log")))
                {
                    logF.WriteLine("GEFS 06");
                    cptec.DownloadNoaaImgs(date, logF, "GEFS", "06");
                    // gefsOK = cptec.DownloadGEFSNoaa(data, logF, "GEFS", "06");

                    frm = WaitForm2.CreateInstance(date);

                    if (frm.TemGefs06)
                    {
                        funcLogs("06");
                    }
                }
                else logF.WriteLine("GEFS 06 OK");
                if (!System.IO.File.Exists(System.IO.Path.Combine(searchPath, "gefs12.log")))
                {

                    logF.WriteLine("GEFS 12");
                    cptec.DownloadNoaaImgs(date, logF, "GEFS", "12");


                    frm = WaitForm2.CreateInstance(date);

                    if (frm.TemGefs12)
                    {
                        funcLogs("12");

                    }
                }
                else logF.WriteLine("GEFS 12 OK");
            }
            catch (Exception ex)
            {
                logF.WriteLine(ex.ToString());
            }
            #endregion

            #region Download GFS Tropical TidBits
            ////GFS
            try
            {
                IPrecipitacaoForm frm = null;


                var funcLogs = new Action<string>(hora =>
                {

                    var eta = hora == "00" ? WaitForm2.TipoEta._00h : WaitForm2.TipoEta._12h;
                    WaitForm2.TipoGefs gefs;

                    switch (hora)
                    {
                        case "00":
                            gefs = WaitForm2.TipoGefs._ctl_00h;
                            break;
                        case "06":
                            gefs = WaitForm2.TipoGefs._ctl_06h;
                            break;
                        case "12":
                            gefs = WaitForm2.TipoGefs._ctl_12h;
                            break;
                        default:
                            return;
                    }
                    frm.Eta = eta;
                    frm.Gefs = gefs;
                    frm.Tipo = WaitForm.TipoConjunto.Gefs;
                    frm.RemoveLimiteGEFS = false;
                    frm.RemoveViesGEFS = false;
                    frm.SalvarDados = true;

                    frm.ProcessarConjunto(saveLogFile: System.IO.Path.Combine(searchPath, "gfs" + hora + ".log"));
                });


                if (!System.IO.File.Exists(System.IO.Path.Combine(searchPath, "gfs00.log")))
                {
                    logF.WriteLine("GFS 00");
          

                    cptec.DownloadNoaaImgs(date, logF, "GFS", "00");

                    frm = WaitForm2.CreateInstance(date);

                    if (frm.TemGfs00)
                    {
                        funcLogs("00");
                    }
                }
                else logF.WriteLine("GFS 00 OK");
                if (!System.IO.File.Exists(System.IO.Path.Combine(searchPath, "gfs06.log")))
                {
                    logF.WriteLine("GFS 06");
     
                    cptec.DownloadNoaaImgs(date, logF, "GFS", "06");

                    frm = WaitForm2.CreateInstance(date);

                    if (frm.TemGfs06)
                    {
                        funcLogs("06");
                    }
                }
                else logF.WriteLine("GFS 06 OK");
                if (!System.IO.File.Exists(System.IO.Path.Combine(searchPath, "gfs12.log")))
                {

                    logF.WriteLine("GFS 12");
                    //gefsOK = cptec.DownloadGEFSNoaa(data, logF, "GFS", "12");
                    //cptec.DownloadGFSNoaa(data, logF, "GFS", "12");
                    cptec.DownloadNoaaImgs(date, logF, "GFS", "12");

                    frm = WaitForm2.CreateInstance(date);

                    if (frm.TemGfs12)
                    {
                        funcLogs("12");
                    }
                }
                else logF.WriteLine("GFS 12 OK");

            }
            catch (Exception ex)
            {
                logF.WriteLine(ex.ToString());
            }
            #endregion

            #region Download ECMWF Meteologix

            //MODELO EURO
            try
            {
                if (!System.IO.Directory.Exists(System.IO.Path.Combine(searchPath, "ECMWF00")))
                {
                    logF.WriteLine("EURO 00");
                    logF.WriteLine(cptec.DownloadMeteologixImgs(date, logF, out _));
                }
                else logF.WriteLine("EURO 00 OK");

                if (!System.IO.Directory.Exists(System.IO.Path.Combine(searchPath, "ECMWF12")))
                {
                    logF.WriteLine("EURO 12");
                    logF.WriteLine(cptec.DownloadMeteologixImgs(date, logF, out _, "12"));

                }
                else logF.WriteLine("EURO 12 OK");

            }
            catch (Exception ex)
            {
                logF.WriteLine(ex.ToString());
            }

            #endregion

            #region Conjunto ONS
            //CONJUNTO
            try
            {
                IPrecipitacaoForm frm = null;

                var funcLogsExtended = new Action(() =>
                {

                    var eta = WaitForm2.TipoEta._00h;
                    var gefs = WaitForm2.TipoGefs._00h;

                    frm.LimparCache();
                    frm.Eta = eta;
                    frm.Gefs = gefs;
                    frm.Tipo = frm.TemEta00 ? WaitForm.TipoConjunto.Conjunto : WaitForm.TipoConjunto.Gefs;
                    frm.Previsoes2Semanas = true;
                    frm.SalvarDados = true;

                    var chuvasConjunto = frm.ProcessarConjunto(saveLogFile: System.IO.Path.Combine(searchPath, frm.TemEta00 ? "conjunto00_EXT.log" : "gefsvies00_EXT.log"));

                    var conjPath = System.IO.Path.Combine(searchPath, frm.TemEta00 ? "CONJUNTO2W00" : "GEFSVIES2W00");

                    if (!System.IO.Directory.Exists(conjPath)) System.IO.Directory.CreateDirectory(conjPath);

                    foreach (var prec in chuvasConjunto)
                    {
                        PrecipitacaoFactory.SalvarModeloBin(prec.Value,
                            System.IO.Path.Combine(conjPath,
                            "pp" + date.ToString("yyyyMMdd") + "_" + ((prec.Key - date).TotalHours + 12).ToString("0000")
                            )
                        );
                    }
                });

                var funcLogsExtendedVies = new Action(() =>
                {

                    var eta = WaitForm2.TipoEta._00h;
                    var gefs = WaitForm2.TipoGefs._00h;

                    frm.LimparCache();
                    frm.Eta = eta;
                    frm.Gefs = gefs;
                    frm.Tipo = WaitForm.TipoConjunto.Conjunto;
                    frm.Previsoes2Semanas = true;
                    frm.SalvarDados = true;
                    frm.RemoveLimiteETA = frm.RemoveLimiteGEFS = frm.RemoveViesETA = frm.RemoveViesGEFS = false;

                    var chuvasConjunto = frm.ProcessarConjunto(saveLogFile: System.IO.Path.Combine(searchPath, "conjunto00_EXT_COMVIES.log"));

                    var conjPath = System.IO.Path.Combine(searchPath, "CONJUNTO2W_COMVIES_00");

                    if (!System.IO.Directory.Exists(conjPath)) System.IO.Directory.CreateDirectory(conjPath);

                    foreach (var prec in chuvasConjunto)
                    {
                        PrecipitacaoFactory.SalvarModeloBin(prec.Value,
                            System.IO.Path.Combine(conjPath,
                            "pp" + date.ToString("yyyyMMdd") + "_" + ((prec.Key - date).TotalHours + 12).ToString("0000")
                            )
                        );
                    }
                });



                if (!System.IO.File.Exists(System.IO.Path.Combine(searchPath, "conjunto00.log")))
                {
                    frm = WaitForm2.CreateInstance(date);
                    if (frm.TemEta00 && frm.TemGefs00)
                    {
                        logF.WriteLine("CONJUNTO 00");
                        // funcLogs("00");


                        funcLogsExtended();

                    }
                    else if (frm.TemGefs00 && !System.IO.Directory.Exists(System.IO.Path.Combine(searchPath, "GEFSVIES2W00")))
                    {
                        funcLogsExtended();
                    }

                }
                // else logF.WriteLine("CONJUNTO 00 OK");

                if (!System.IO.File.Exists(System.IO.Path.Combine(searchPath, "conjunto00_EXT.log")))
                {
                    frm = WaitForm2.CreateInstance(date);
                    if (frm.TemEta00 && frm.TemGefs00)
                    {
                        funcLogsExtended();
                    }
                }


                if (!System.IO.File.Exists(System.IO.Path.Combine(searchPath, "conjunto00_EXT_COMVIES.log")))
                {
                    frm = WaitForm2.CreateInstance(date);
                    if (frm.TemEta00 && frm.TemGefs00)
                    {
                        funcLogsExtendedVies();
                    }
                }

            }
            catch (Exception ex)
            {
                logF.WriteLine(ex.Message);
            }
            #endregion

            #region Conversão ECMWF_ONS em GIFS
            //ECMWF_ONS
            try
            {
                // if (!System.IO.Directory.Exists(System.IO.Path.Combine(searchPath, "ECMWF_ONS")))
                //{
                logF.WriteLine("ECMWF_ONS");

                cptec.DownloadECMWF(date, logF, "00");
                // }
                //else
                logF.WriteLine("ECMWF_ONS 00 OK");
            }
            catch (Exception ex)
            {
                logF.WriteLine(ex.ToString());
            }
            #endregion
            // Converte Bin para Dat arquivos do GFS00 e do ECMWF00
            try
            {
                Convert_BinDat("ECMWF");
            }
            catch (Exception ex)
            {
                logF.WriteLine(ex.ToString());
            }
        }

        public static void Copy_GIFS_NOAA()
        {
            var dt = DateTime.Today;
            var caminho_p = Path.Combine(@"C:\Files\Trading\Acompanhamento Metereologico Semanal\spiderman", dt.ToString("yyyy_MM_dd"));
            var oneDrivePath_ori = Environment.GetEnvironmentVariable("OneDriveCommercial");
            
            var oneDrive = Path.Combine(oneDrivePath_ori, @"Compass\Pedro\NOAA\");
            
            if (!Directory.Exists(oneDrive))
            {
                oneDrive = Path.Combine(oneDrivePath_ori.Replace(oneDrivePath_ori.Split('\\').Last(), @"MinhaTI\Alex Freires Marques - Compass\Pedro\NOAA\"));
            }

            var oneDrivePath_Atual = Path.Combine(oneDrive, dt.ToString("yyyy"), dt.ToString("MM"), dt.ToString("dd"));

            if (Directory.Exists(oneDrivePath_Atual))
            {
                var dirs_noaa = Directory.GetDirectories(oneDrivePath_Atual);
            
                foreach(var dir in dirs_noaa)
                {
                    DirectoryInfo dir_info = new DirectoryInfo(dir);
                    var data_modif = dir_info.LastWriteTime;

                    var arqs_noaa = Directory.GetFiles(dir, "*.gif");
                    string[] arqs_P = null;
                    var dir_files = Path.Combine(caminho_p, dir.Split('\\').Last() + "_NOAA");
                    
                        
                    
                    
                        var name_path = dir.Split('\\').Last();
                        
                        if (name_path.Contains("GEFS") || name_path.Contains("GFS"))
                        {
                            if (!Directory.Exists(dir_files) && arqs_noaa.Count() > 0)
                            {
                                Directory.CreateDirectory(dir_files);
                            }
                            

                            if (arqs_noaa.Count() > 0){
                                arqs_P = Directory.GetFiles(dir_files, "*.gif");
                                if (arqs_P.Count() != arqs_noaa.Count())
                                {
                                    foreach (var arq_noaa in arqs_noaa)
                                    {
                                        var dir_file = Path.Combine(dir_files, arq_noaa.Split('\\').Last());
                                        File.Copy(arq_noaa, dir_file, true);
                                    }
                                }
                            }

                            

                            
                        }
                    
                }
                

            }
        }

        internal static async void AutoExec(DateTime date, System.IO.TextWriter logF, bool encad = false)
        {
            ///Rodada automática
            /// 
            var pastaSaida = "";
            var data_verifica = DateTime.Today;



            if (DateTime.Now > DateTime.Today.AddHours(7))
            {
                var funceme = Directory.GetFiles(Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Previsao_Numerica\Modelo_R\funceme\", data_verifica.ToString("yyyyMM"), data_verifica.ToString("dd")));
                var funcemeFolder = Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Previsao_Numerica\Modelo_R\funceme\", data_verifica.ToString("yyyyMM"), data_verifica.ToString("dd"));
                var ETA40 = Directory.GetFiles(Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Previsao_Numerica\", data_verifica.ToString("yyyyMM"), data_verifica.ToString("dd")), "ETA40_*");

                var frmMain = new FrmMain(true, encad);

                var runRev = ChuvaVazaoTools.Tools.Tools.GetNextRev(date);

                //Verifica se já existe Acomph para o dia
                if (!File.Exists(Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de vazões\ACOMPH\1_historico", data_verifica.ToString("yyyy"), data_verifica.ToString("MM_yyyy"), "ACOMPH_" + data_verifica.ToString("dd-MM-yyyy") + ".xls")))
                {   // Verifica se Funceme e ETA40 já estão disponiveis
                    if (funceme.Length != 0 && ETA40.Length > 1)
                    {
                        pastaSaida = @"C:\Files\Middle - Preço\16_Chuva_Vazao\" + runRev.revDate.ToString("yyyy_MM") + @"\RV" + runRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\Mapas Acomph d-1\";
                        if (!Directory.Exists(pastaSaida))
                        {

                            logF.WriteLine("Executando Mapas R Acomph d-1");
                            Directory.CreateDirectory(pastaSaida);

                            Conjunto_R(pastaSaida, date, logF);


                            //Salvar mapas de saída do modelo R como Img
                            frmMain.Salvar_Img(Path.Combine(pastaSaida, "Arq_Saida"), true);
                        }
                    }
                    else
                    {
                        if (DateTime.Now > DateTime.Today.AddMinutes(450))
                        {
                            if (!Directory.Exists(funcemeFolder))
                            {
                                Directory.CreateDirectory(funcemeFolder);
                            }
                            var euroOntem = Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Previsao_Numerica\Modelo_R\ECMWF00", DateTime.Today.ToString("yyyyMM"), DateTime.Today.AddDays(-1).ToString("dd"));
                            var euroBin_Ctl = Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Previsao_Numerica", DateTime.Today.ToString("yyyyMM"), DateTime.Today.AddDays(-1).ToString("dd"), "ECMWF00");
                            var obs_Funceme = Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Observado_Funceme", DateTime.Today.ToString("yyyy"), DateTime.Today.ToString("MM"));

                            var arqEuro = "pp" + DateTime.Today.AddDays(-1).ToString("yyyyMMdd") + "_0036";
                            var nomeFunceme = "funceme_p" + DateTime.Today.ToString("ddMMyy") + "a" + DateTime.Today.ToString("ddMMyy") + ".dat";
                            var funcemeBin_ctl = "funceme_" + DateTime.Today.ToString("yyyyMMdd");

                            File.Copy(Path.Combine(euroOntem, arqEuro + ".dat"), Path.Combine(funcemeFolder, nomeFunceme));
                            File.Copy(Path.Combine(euroBin_Ctl, arqEuro + ".bin"), Path.Combine(obs_Funceme, funcemeBin_ctl + ".bin"));
                            File.Copy(Path.Combine(euroBin_Ctl, arqEuro + ".ctl"), Path.Combine(obs_Funceme, funcemeBin_ctl + ".ctl"));

                            logF.WriteLine("Arquivos funceme não encontrados, subistituindo por euro");
                            var retornoEmail = Tools.Tools.SendMail("", "Arquivos funceme não encontrados, substituindo por euro", "Funceme não encontrado [AUTO]", "preco");
                            retornoEmail.Wait();
                            AutoExec(date, logF, encad);
                        }
                        else
                        {
                            logF.WriteLine("Arquivos não encontrados");
                            RunAutoRoutines("download");
                        }

                    }

                }
                else
                {

                    pastaSaida = @"C:\Files\Middle - Preço\16_Chuva_Vazao\" + runRev.revDate.ToString("yyyy_MM") + @"\RV" + runRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\Mapas Acomph";
                    if (!Directory.Exists(pastaSaida))
                    {
                        logF.WriteLine("Executando Mapas R Acomph");
                        Directory.CreateDirectory(pastaSaida);

                        Conjunto_R(pastaSaida, date, logF);

                        //Salvar mapas de saída do modelo R como Img
                        frmMain.Salvar_Img(Path.Combine(pastaSaida, "Arq_Saida"), true);
                    }


                }


                //frmMain.Run(logF, out _);
                //return;
                if (File.Exists(Path.Combine(pastaSaida, "logC.txt")) && !File.Exists(Path.Combine(pastaSaida, "error.log")))
                {
                    try
                    {
                        if (date.DayOfWeek != DayOfWeek.Thursday)
                        {
                            //rodada com offset na remoção de viés.
                            frmMain.modelosChVz.Clear();
                            frmMain.RunExecProcess(logF, out _, FrmMain.EnumRemo.RemocaoUmaSemanaEuro);
                        }
                    }
                    catch (Exception ex)
                    {
                        logF.WriteLine(ex.ToString());
                    }
                    try
                    {
                        if (date.DayOfWeek != DayOfWeek.Thursday)
                        {
                            //rodada com offset na remoção de viés.

                            frmMain.modelosChVz.Clear();
                            frmMain.RunExecProcess(logF, out _, FrmMain.EnumRemo.RemocaoUmaSemanaEuro_op);
                        }
                    }
                    catch (Exception ex)
                    {
                        logF.WriteLine(ex.ToString());
                    }
                    try
                    {
                        frmMain.modelosChVz.Clear();
                        frmMain.RunExecProcess(logF, out var runIdT);
                    }
                    catch (Exception ex)
                    {
                        logF.WriteLine(ex.ToString());
                    }
                    try
                    {
                        if (date.DayOfWeek != DayOfWeek.Thursday)
                        {
                            frmMain.modelosChVz.Clear();
                            //rodada com offset na remoção de viés.
                            frmMain.RunExecProcess(logF, out _, FrmMain.EnumRemo.RemocaoUmaSemana);
                        }
                    }
                    catch (Exception ex)
                    {
                        logF.WriteLine(ex.ToString());
                    }
                    try
                    {

                        if (date.DayOfWeek != DayOfWeek.Thursday)
                        {
                            frmMain.modelosChVz.Clear();
                            frmMain.RunExecProcess(logF, out _, FrmMain.EnumRemo.RemocaoUmaSemanaGFS);
                        }
                    }
                    catch (Exception ex)
                    {
                        logF.WriteLine(ex.ToString());
                    }
                    try
                    {
                        frmMain.modelosChVz.Clear();
                        frmMain.RunExecProcess(logF, out _, FrmMain.EnumRemo.RemocaoDuasSemanasGEFS);
                    }
                    catch (Exception ex)
                    {
                        logF.WriteLine(ex.ToString());
                    }
                    try
                    {
                        frmMain.modelosChVz.Clear();
                        frmMain.RunExecProcess(logF, out _, FrmMain.EnumRemo.RemocaoDuasSemanasEuro);
                    }
                    catch (Exception ex)
                    {
                        logF.WriteLine(ex.ToString());
                    }
                    try
                    {
                        frmMain.modelosChVz.Clear();
                        frmMain.RunExecProcess(logF, out _, FrmMain.EnumRemo.RemocaoDuasSemanasEuro_op);
                    }
                    catch (Exception ex)
                    {
                        logF.WriteLine(ex.ToString());
                    }
                    try
                    {
                        frmMain.modelosChVz.Clear();
                        frmMain.RunExecProcess(logF, out _, FrmMain.EnumRemo.RemocaoDuasSemanasGFS);
                    }
                    catch (Exception ex)
                    {
                        logF.WriteLine(ex.ToString());
                    }
                }
                else
                {
                    Directory.Delete(pastaSaida, true);
                }
            }
        }






        //Remocação de vies antigo
        internal static void AutoRun(DateTime date, System.IO.TextWriter logF)
        {
            ///Rodada automática
            /// 
            var frmMain = new FrmMain(true);

            //frmMain.Run(logF, out _);
            //return;

            try
            {
                if (date.DayOfWeek != DayOfWeek.Thursday)
                {
                    //rodada com offset na remoção de viés.
                    frmMain.Run(logF, out _, FrmMain.EnumRemo.RemocaoUmaSemanaEuro);
                }
            }
            catch (Exception ex)
            {
                logF.WriteLine(ex.ToString());
            }
            try
            {

                frmMain.Run(logF, out var runIdT);
            }
            catch (Exception ex)
            {
                logF.WriteLine(ex.ToString());
            }
            try
            {
                if (date.DayOfWeek != DayOfWeek.Thursday)
                {
                    //rodada com offset na remoção de viés.
                    frmMain.Run(logF, out _, FrmMain.EnumRemo.RemocaoUmaSemana);
                }
            }
            catch (Exception ex)
            {
                logF.WriteLine(ex.ToString());
            }
            try
            {
                frmMain.Run(logF, out _, FrmMain.EnumRemo.RemocaoDuasSemanasGEFS);
            }
            catch (Exception ex)
            {
                logF.WriteLine(ex.ToString());
            }
            try
            {
                frmMain.Run(logF, out _, FrmMain.EnumRemo.RemocaoDuasSemanasEuro);
            }
            catch (Exception ex)
            {
                logF.WriteLine(ex.ToString());
            }
            try
            {
                frmMain.Run(logF, out _, FrmMain.EnumRemo.RemocaoDuasSemanasGFS);
            }
            catch (Exception ex)
            {
                logF.WriteLine(ex.ToString());
            }
            try
            {
                frmMain.Run(logF, out _, FrmMain.EnumRemo.RemocaoDuasSemanasGFS2x);
            }
            catch (Exception ex)
            {
                logF.WriteLine(ex.ToString());
            }
        }

        internal static void AutoReport(DateTime date, System.IO.TextWriter logF)
        {
            //condicões para relatorios: Downloads OK / (Rodadas CV do dia ok || 8:40 da manhã)
            var nextRev = Tools.Tools.GetNextRev(date);
            var runRev = ChuvaVazaoTools.Tools.Tools.GetNextRev(date);

            // var verPastaPre = @"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\16_Chuva_Vazao\" + runRev.revDate.ToString("yyyy_MM") + @"\RV" + runRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\Mapas Acomph d-1";
            //var verPastaDef = @"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\16_Chuva_Vazao\" + runRev.revDate.ToString("yyyy_MM") + @"\RV" + runRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\Mapas Acomph";
            var verPastaPre = @"C:\Files\Middle - Preço\16_Chuva_Vazao\" + runRev.revDate.ToString("yyyy_MM") + @"\RV" + runRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\Mapas Acomph d-1";
            var verPastaDef = @"C:\Files\Middle - Preço\16_Chuva_Vazao\" + runRev.revDate.ToString("yyyy_MM") + @"\RV" + runRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\Mapas Acomph";

            var previsoesPath0 = Path.Combine(@"C:\Files\Middle - Preço\16_Chuva_Vazao", nextRev.revDate.ToString("yyyy_MM"), $"RV{nextRev.rev}", date.ToString("yy-MM-dd"));

            var dirA = (Path.Combine(previsoesPath0, "CV_ACOMPH_FUNC"));
            var dirB = (Path.Combine(previsoesPath0, "CV_ACOMPH_FUNC_d-1"));//CV_SOMBRA_ACOMPH_FUNC
            var dirS = (Path.Combine(previsoesPath0, "CV_SOMBRA_ACOMPH_FUNC"));//CV_SOMBRA_ACOMPH_FUNC

            var camRelPrev = @"C:\Files\Relatorios\Relatorio Final\Relatorios\" + "Relatorio_de_Previsoes_" + DateTime.Today.ToString("dd_MM_yyyy") + "_Preliminar.pdf";
            var camRelPrevDef = @"C:\Files\Relatorios\Relatorio Final\Relatorios\" + "Relatorio_de_Previsoes_" + DateTime.Today.ToString("dd_MM_yyyy") + ".pdf";
            var camRelPrevCompleto = @"C:\Files\Relatorios\Relatorio Final\Relatorios\" + "Relatorio_de_Previsoes_" + DateTime.Today.ToString("dd_MM_yyyy") + "_Completo.pdf";

            var horaRel = @"C:\Files\Relatorios\Relatorio Final\Relatorios";
            var relprevPre = "Relatorio_de_Previsoes_" + DateTime.Today.ToString("dd_MM_yyyy") + "_Preliminar.pdf";
            var relprevDef = "Relatorio_de_Previsoes_" + DateTime.Today.ToString("dd_MM_yyyy") + ".pdf";

            var relComPre = "Relatorio_Compass_" + date.ToString("dd_MM_yyyy") + "_(" + 0.ToString() + " hrs)_Preliminar.pdf";
            var relComDef = "Relatorio_Compass_" + date.ToString("dd_MM_yyyy") + "_(" + 0.ToString() + " hrs).pdf";

            var pastPrev00 = Path.Combine(@"C:\Files\Trading\Acompanhamento Metereologico Semanal\spiderman", DateTime.Now.ToString("yyyy_MM_dd"), "CONJUNTO00PREV");

            try
            {
                try
                {
                    if (!File.Exists(camRelPrev) && DateTime.Now >= DateTime.Today.AddSeconds(27000) && Directory.Exists(pastPrev00) || !File.Exists(camRelPrev) && DateTime.Now >= DateTime.Today.AddHours(9))// File.Exists(Path.Combine(verPastaPre, "logC.txt")))
                    {
                        logF.WriteLine("Relatorio Preliminar");
                        Tools.Tools.addHistory("C:\\Sistemas\\ChuvaVazao\\Log\\report.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "- tentativa de gerar relatório de previsoes via auto report");

                        var h = Tools.Tools.readHistory(Path.Combine(horaRel, "RelatoriosPrev_log.txt")).Last();

                        var d = Convert.ToDateTime(h);

                        if (d <= DateTime.Now.AddMinutes(-10))
                        {
                            Tools.Tools.addHistory(Path.Combine(horaRel, "RelatoriosPrev_log.txt"), DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));

                            Report.Program.CriarRelatorioPrevs(date, camRelPrev, true);

                            if (File.Exists(camRelPrev))
                            {
                                var retornoEmail = Tools.Tools.SendMail(camRelPrev, "Relatório preliminar de previsões disponível", "Relatório de previsões [AUTO]", "precoSergioVini");
                                retornoEmail.Wait(300000);
                                Tools.Tools.addHistory(Path.Combine(horaRel, "RelatoriosPrev_log.txt"), DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
                            }

                        }
                    }



                    if (File.Exists(camRelPrev) && !File.Exists(camRelPrevDef))
                    {
                        logF.WriteLine("Relatorio Definitivo");
                        var h = Tools.Tools.readHistory(Path.Combine(horaRel, "RelatoriosPrev_log.txt")).Last();

                        var d = Convert.ToDateTime(h);

                        if (d <= DateTime.Now.AddMinutes(-10))
                        {
                            Report.Program.CriarRelatorioPrevs(date, camRelPrevDef);
                            if (File.Exists(camRelPrevDef))
                            {
                                var retornoEmail = Tools.Tools.SendMail(camRelPrevDef, "Relatório de previsões disponível", "Relatório de previsões [AUTO]", "precoSergioVini");
                                retornoEmail.Wait(300000);
                                Tools.Tools.addHistory(Path.Combine(horaRel, "RelatoriosPrev_log.txt"), DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
                            }


                        }

                    }

                    if (File.Exists(camRelPrev) && File.Exists(camRelPrevDef) && !File.Exists(camRelPrevCompleto))
                    {
                        logF.WriteLine("Relatorio Definitivo 2");

                        var h = Tools.Tools.readHistory(Path.Combine(horaRel, "RelatoriosPrev_log.txt")).Last();

                        var d = Convert.ToDateTime(h);

                        if (d <= DateTime.Now.AddMinutes(-10))
                        {
                            Report.Program.CriarRelatorioPrevs2(date, camRelPrevCompleto);
                            if (File.Exists(camRelPrevCompleto))
                            {
                                var retornoEmail = Tools.Tools.SendMail(camRelPrevCompleto, "Relatório de previsões disponível", "Relatório de previsões [AUTO]", "precoSergioVini");
                                retornoEmail.Wait(300000);
                                Tools.Tools.addHistory(Path.Combine(horaRel, "RelatoriosPrev_log.txt"), DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
                            }


                        }

                    }
                }
                catch (Exception ex)
                {
                    logF.WriteLine("Erro ao Gerar Relatorio: " + ex.Message);
                    Tools.Tools.addHistory("C:\\Sistemas\\ChuvaVazao\\Log\\report.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "- catch acionado na tentativa de gerar relatório de previsoes " + ex.Message);

                }
                //caminho = @"C:\Files\Relatorios\Relatorio Final\" + "Relatorio_Compass_" + data.ToString("dd_MM_yyyy") + "_(" + hora.ToString() + " hrs).pdf";
             /*   if ((Directory.Exists(dirB) && File.Exists(Path.Combine(dirB, "enadiaria.log"))) || DateTime.Now >= DateTime.Today.AddHours(8))
                {
                    var caminhoRel = @"C:\Files\Relatorios\Relatorio Final\Relatorios\" + "Relatorio_Compass_" + date.ToString("dd_MM_yyyy") + "_(" + 0.ToString() + " hrs)_Preliminar.pdf";

                    if (!File.Exists(caminhoRel))
                    {
                        Tools.Tools.addHistory("C:\\Sistemas\\ChuvaVazao\\Log\\report.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "- tentativa de gerar relatório via pârametro [preliminar == true]");
                        if (!File.Exists(Path.Combine(horaRel, "RelatoriosCompass_log.txt")))
                        {
                            Tools.Tools.addHistory(Path.Combine(horaRel, "RelatoriosCompass_log.txt"), DateTime.Now.AddMinutes(-20).ToString("dd-MM-yyyy HH:mm:ss"));

                        }

                        var h = Tools.Tools.readHistory(Path.Combine(horaRel, "RelatoriosCompass_log.txt")).Last();

                        var d = Convert.ToDateTime(h);

                        if (d <= DateTime.Now.AddMinutes(-10))
                        {
                            Tools.Tools.addHistory(Path.Combine(horaRel, "RelatoriosCompass_log.txt"), DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
                            try
                            {
                                logF.WriteLine("Gerando relatorio:");
                                logF.WriteLine(caminhoRel);
                                Report.Program.CriarRelatorio2(date, caminhoRel, true);

                                if (File.Exists(caminhoRel))
                                {
                                    var retornoEmail = Tools.Tools.SendMail(caminhoRel, "Relatório de acompanhamento disponível", "Relatório de acompanhamento [AUTO]", "precoSergio");
                                    //sendNotification("Relatório de acompanhamento disponível", "bruno.araujo@cpas.com.br,natalia.biondo@cpas.com.br,diana.lima@cpas.com.br,pedro.modesto@cpas.com.br", caminhoRel);
                                    retornoEmail.Wait(300000);
                                    Tools.Tools.addHistory(Path.Combine(horaRel, "RelatoriosCompass_log.txt"), DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));

                                }

                            }
                            catch
                            {
                                if (File.Exists(caminhoRel))
                                {
                                    File.Delete(caminhoRel);
                                }
                            }
                        }
                    }
                    else
                    {
                        logF.WriteLine("Relatório já existente:");
                        logF.WriteLine(caminhoRel);
                    }
                    // relatorio 00 h preliminar
                }

                if ((Directory.Exists(dirA) && File.Exists(Path.Combine(dirA, "enadiaria.log")))|| (Directory.Exists(dirS) && File.Exists(Path.Combine(dirS, "enadiaria.log"))))
                {
                    var caminhoRel = @"C:\Files\Relatorios\Relatorio Final\Relatorios\" + "Relatorio_Compass_" + date.ToString("dd_MM_yyyy") + "_(" + 0.ToString() + " hrs).pdf";

                    if (!File.Exists(caminhoRel))
                    {
                        Tools.Tools.addHistory("C:\\Sistemas\\ChuvaVazao\\Log\\report.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "- tentativa de gerar relatório via pârametro [preliminar == false]");

                        if (!File.Exists(Path.Combine(horaRel, "RelatoriosCompass_log.txt")))
                        {
                            Tools.Tools.addHistory(Path.Combine(horaRel, "RelatoriosCompass_log.txt"), DateTime.Now.AddMinutes(-20).ToString("dd-MM-yyyy HH:mm:ss"));

                        }
                        var h = Tools.Tools.readHistory(Path.Combine(horaRel, "RelatoriosCompass_log.txt")).Last();

                        var d = Convert.ToDateTime(h);

                        if (d <= DateTime.Now.AddMinutes(-10))
                        {
                            Tools.Tools.addHistory(Path.Combine(horaRel, "RelatoriosCompass_log.txt"), DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
                            try
                            {
                                logF.WriteLine("Gerando relatorio:");
                                logF.WriteLine(caminhoRel);
                                Report.Program.CriarRelatorio2(date, caminhoRel, false);

                                if (File.Exists(caminhoRel))
                                {
                                    var retornoEmail = Tools.Tools.SendMail(caminhoRel, "Relatório de acompanhamento disponível", "Relatório de acompanhamento [AUTO]", "precoSergio");

                                    retornoEmail.Wait(300000);
                                    //sendNotification("Relatório de acompanhamento disponível", "bruno.araujo@cpas.com.br,natalia.biondo@cpas.com.br,diana.lima@cpas.com.br,pedro.modesto@cpas.com.br", caminhoRel);
                                    Tools.Tools.addHistory(Path.Combine(horaRel, "RelatoriosCompass_log.txt"), DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));

                                }

                            }
                            catch
                            {
                                if (File.Exists(caminhoRel))
                                {
                                    File.Delete(caminhoRel);
                                }
                            }

                        }
                    }
                    else
                    {
                        logF.WriteLine("Relatório já existente:");
                        logF.WriteLine(caminhoRel);
                    }
                }*/
            }
            catch (Exception ex)
            {
                Tools.Tools.addHistory("C:\\Sistemas\\ChuvaVazao\\Log\\report.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "- catch acionado na tentativa de gerar relatório via pârametro: " + ex.Message);
                logF.WriteLine(ex.ToString());
            }
        }


        internal static void AutoRun_R(DateTime date, System.IO.TextWriter logF, bool encad = false)
        {

            // Verifica se já existe mapas para rodada

            var pastaSaida = "";
            var data_verifica = DateTime.Today;



            if (DateTime.Now > DateTime.Today.AddHours(7))
            {
                var funceme = Directory.GetFiles(Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Previsao_Numerica\Modelo_R\funceme\", data_verifica.ToString("yyyyMM"), data_verifica.ToString("dd")));
                var funcemeFolder = Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Previsao_Numerica\Modelo_R\funceme\", data_verifica.ToString("yyyyMM"), data_verifica.ToString("dd"));
                var ETA40 = Directory.GetFiles(Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Previsao_Numerica\", data_verifica.ToString("yyyyMM"), data_verifica.ToString("dd")), "ETA40_*");

                var frmMain = new FrmMain(true, encad);

                var runRev = ChuvaVazaoTools.Tools.Tools.GetNextRev(date);

                //Verifica se já existe Acomph para o dia
                if (!File.Exists(Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de vazões\ACOMPH\1_historico", data_verifica.ToString("yyyy"), data_verifica.ToString("MM_yyyy"), "ACOMPH_" + data_verifica.ToString("dd-MM-yyyy") + ".xls")))
                {   // Verifica se Funceme e ETA40 já estão disponiveis
                    if (funceme.Length != 0 && ETA40.Length > 1)
                    {
                        pastaSaida = @"C:\Files\Middle - Preço\16_Chuva_Vazao\" + runRev.revDate.ToString("yyyy_MM") + @"\RV" + runRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\Mapas Acomph d-1\";
                        if (!Directory.Exists(pastaSaida))
                        {

                            logF.WriteLine("Executando Mapas R Acomph d-1");
                           // Directory.CreateDirectory(pastaSaida);

                            Conjunto_R(pastaSaida, date, logF);

                            //Salvar mapas de saída do modelo R como Img
                            frmMain.Salvar_Img(Path.Combine(pastaSaida, "Arq_Saida"), true);
                        }
                    }
                    else
                    {
                        if (DateTime.Now > DateTime.Today.AddMinutes(450))
                        {
                            if (!Directory.Exists(funcemeFolder))
                            {
                                Directory.CreateDirectory(funcemeFolder);
                            }
                            var euroOntem = Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Previsao_Numerica\Modelo_R\ECMWF00", DateTime.Today.ToString("yyyyMM"), DateTime.Today.AddDays(-1).ToString("dd"));
                            var euroBin_Ctl = Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Previsao_Numerica", DateTime.Today.ToString("yyyyMM"), DateTime.Today.AddDays(-1).ToString("dd"), "ECMWF00");
                            var obs_Funceme = Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Observado_Funceme", DateTime.Today.ToString("yyyy"), DateTime.Today.ToString("MM"));

                            var arqEuro = "pp" + DateTime.Today.AddDays(-1).ToString("yyyyMMdd") + "_0036";
                            var nomeFunceme = "funceme_p" + DateTime.Today.ToString("ddMMyy") + "a" + DateTime.Today.ToString("ddMMyy") + ".dat";
                            var funcemeBin_ctl = "funceme_" + DateTime.Today.ToString("yyyyMMdd");

                            File.Copy(Path.Combine(euroOntem, arqEuro + ".dat"), Path.Combine(funcemeFolder, nomeFunceme));
                            File.Copy(Path.Combine(euroBin_Ctl, arqEuro + ".bin"), Path.Combine(obs_Funceme, funcemeBin_ctl + ".bin"));
                            File.Copy(Path.Combine(euroBin_Ctl, arqEuro + ".ctl"), Path.Combine(obs_Funceme, funcemeBin_ctl + ".ctl"));

                            logF.WriteLine("Arquivos funceme não encontrados, subistituindo por euro");
                            var retornoEmail = Tools.Tools.SendMail("", "Arquivos funceme não encontrados, substituindo por euro", "Funceme não encontrado [AUTO]", "preco");
                            retornoEmail.Wait();
                            AutoRun_R(date, logF, encad);
                        }
                        else
                        {
                            logF.WriteLine("Arquivos não encontrados");
                            RunAutoRoutines("download");
                        }

                    }

                }
                else
                {

                    pastaSaida = @"C:\Files\Middle - Preço\16_Chuva_Vazao\" + runRev.revDate.ToString("yyyy_MM") + @"\RV" + runRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\Mapas Acomph";
                    if (!Directory.Exists(pastaSaida))
                    {
                        logF.WriteLine("Executando Mapas R Acomph");
                        //Directory.CreateDirectory(pastaSaida);

                        Conjunto_R(pastaSaida, date, logF);

                        //Salvar mapas de saída do modelo R como Img
                        frmMain.Salvar_Img(Path.Combine(pastaSaida, "Arq_Saida"), true);
                    }


                }



                if (File.Exists(Path.Combine(pastaSaida, "logC.txt")) && !File.Exists(Path.Combine(pastaSaida, "error.log")))
                {
                    try
                    {
                        if (date.DayOfWeek != DayOfWeek.Thursday)
                        {
                            //rodada com offset na remoção de viés.

                            frmMain.modelosChVz.Clear();
                            frmMain.Run_R(logF, out _, FrmMain.EnumRemo.RemocaoUmaSemanaEuro);
                        }
                    }
                    catch (Exception ex)
                    {
                        logF.WriteLine(ex.ToString());
                    }
                    try
                    {
                        if (date.DayOfWeek != DayOfWeek.Thursday)
                        {
                            //rodada com offset na remoção de viés.

                            frmMain.modelosChVz.Clear();
                            frmMain.Run_R(logF, out _, FrmMain.EnumRemo.RemocaoUmaSemanaEuro_op);
                        }
                    }
                    catch (Exception ex)
                    {
                        logF.WriteLine(ex.ToString());
                    }
                    try
                    {
                        frmMain.modelosChVz.Clear();
                        frmMain.Run_R(logF, out var runIdT);
                    }
                    catch (Exception ex)
                    {
                        logF.WriteLine(ex.ToString());
                    }
                    try
                    {
                        if (date.DayOfWeek != DayOfWeek.Thursday)
                        {
                            frmMain.modelosChVz.Clear();
                            //rodada com offset na remoção de viés.
                            frmMain.Run_R(logF, out _, FrmMain.EnumRemo.RemocaoUmaSemana);
                        }
                    }
                    catch (Exception ex)
                    {
                        logF.WriteLine(ex.ToString());
                    }
                    try
                    {

                        if (date.DayOfWeek != DayOfWeek.Thursday)
                        {
                            frmMain.modelosChVz.Clear();
                            frmMain.Run_R(logF, out _, FrmMain.EnumRemo.RemocaoUmaSemanaGFS);
                        }
                    }
                    catch (Exception ex)
                    {
                        logF.WriteLine(ex.ToString());
                    }
                    try
                    {
                        frmMain.modelosChVz.Clear();
                        frmMain.Run_R(logF, out _, FrmMain.EnumRemo.RemocaoDuasSemanasEuro);
                    }
                    catch (Exception ex)
                    {
                        logF.WriteLine(ex.ToString());
                    }
                    try
                    {
                        frmMain.modelosChVz.Clear();
                        frmMain.Run_R(logF, out _, FrmMain.EnumRemo.RemocaoDuasSemanasEuro_op);
                    }
                    catch (Exception ex)
                    {
                        logF.WriteLine(ex.ToString());
                    }
                    try
                    {
                        frmMain.modelosChVz.Clear();
                        frmMain.Run_R(logF, out _, FrmMain.EnumRemo.RemocaoDuasSemanasGEFS);
                    }
                    catch (Exception ex)
                    {
                        logF.WriteLine(ex.ToString());
                    }
                    try
                    {
                        frmMain.modelosChVz.Clear();
                        frmMain.Run_R(logF, out _, FrmMain.EnumRemo.RemocaoDuasSemanasGFS);
                    }
                    catch (Exception ex)
                    {
                        logF.WriteLine(ex.ToString());
                    }
                    try
                    {

                        if (date.DayOfWeek == DayOfWeek.Tuesday || date.DayOfWeek == DayOfWeek.Friday)
                        {
                            frmMain.modelosChVz.Clear();
                            frmMain.Run_R(logF, out _, FrmMain.EnumRemo.RemocaoTresSemanasEuro);
                        }
                    }
                    catch (Exception ex)
                    {
                        logF.WriteLine(ex.ToString());
                    }
                    try
                    {
                        frmMain.modelosChVz.Clear();
                        frmMain.Run_R(logF, out _, FrmMain.EnumRemo.RemocaoTresSemanasGEFS);
                    }
                    catch (Exception ex)
                    {
                        logF.WriteLine(ex.ToString());
                    }

                    try
                    {

                        if (date.DayOfWeek == DayOfWeek.Tuesday || date.DayOfWeek == DayOfWeek.Friday)
                        {
                            frmMain.modelosChVz.Clear();
                            frmMain.Run_R(logF, out _, FrmMain.EnumRemo.RemocaoQuatroSemanasEuro);
                        }
                    }
                    catch (Exception ex)
                    {
                        logF.WriteLine(ex.ToString());
                    }
                    try
                    {
                        frmMain.modelosChVz.Clear();
                        frmMain.Run_R(logF, out _, FrmMain.EnumRemo.RemocaoQuatroSemanasGEFS);
                    }
                    catch (Exception ex)
                    {
                        logF.WriteLine(ex.ToString());
                    }
                }
                else if(File.Exists(Path.Combine(pastaSaida, "error.log")))
                {
                    Directory.Delete(pastaSaida, true);
                }
            }
        }


        internal static void executar_R(string path, DateTime date)
        {

            //Conjunto_R(path, date);//Middle - Preço\16_Chuva_Vazao\Conjunto-PastasEArquivos
            //Junta_Mapas("ECMWF");

            //string executar = @"/C " + "H: & cd " + txtCaminho.Text + "& bat.bat";

            string executar = @"/C " + "N: & cd " + path + @" & powershell.exe .\Gerar_mapas.ps1";


            System.Diagnostics.Process.Start("CMD.EXE", executar).WaitForExit();
            path = Path.Combine(path, "logC.txt");
            File.Create(path);


        }

        internal static void Conjunto_R(string path, DateTime date, TextWriter LogF)
        {
            var data = date;
            var localPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Conjunto_R_" + DateTime.Now.ToString("HHmmss"));
            var Conj_Zip = Path.Combine(@"C:\Files\Middle - Preço\16_Chuva_Vazao\Conjunto-PastasEArquivos.zip");

            var path_final = Path.Combine(path);


            if (!Directory.Exists(localPath))
            {
                Directory.CreateDirectory(localPath);
            }

            File.Copy(Conj_Zip, Path.Combine(localPath, "Conjunto - PastasEArquivos.zip"));

            System.IO.Compression.ZipFile.ExtractToDirectory(Path.Combine(localPath, "Conjunto - PastasEArquivos.zip"), Path.Combine(localPath, "Conjunto - PastasEArquivos"));

            ChuvaVazaoTools.Gerar_Mapas_R.Gerar_R(Path.Combine(localPath, "Conjunto - PastasEArquivos"), LogF);

            var dirs = Directory.GetDirectories(Path.Combine(localPath, "Conjunto - PastasEArquivos"));

            var dirs_CV = dirs.Where(x => x.Split('\\').Last().StartsWith("CV"));

            var dirs_Resto = dirs.Where(x => !x.Split('\\').Last().StartsWith("CV"));

            var files = Directory.GetFiles(Path.Combine(localPath, "Conjunto - PastasEArquivos"));

            

            if (dirs_CV.Count() > 0)
            {
                Directory.CreateDirectory(path_final);

                foreach (var dir in dirs_CV)
                {
                    DirectoryCopy(dir, Path.Combine(path_final, dir.Split('\\').Last()), true);

                    

                }

                var log_C = Path.Combine(path_final, "logC.txt");

                var log = File.Create(log_C);

                log.Close();

                foreach (var dir in dirs_Resto)
                {
                    DirectoryCopy(dir, Path.Combine(path_final, dir.Split('\\').Last()), true);
                }

                foreach (var file in files)
                {
                    File.Copy(file, Path.Combine(path_final, file.Split('\\').Last()));
                }


            }
            else
            {
                foreach (var file in files)
                {
                    File.Copy(file, Path.Combine(path_final, file.Split('\\').Last()));
                }
            }

            var path_Z = path_final.Replace("C:\\Files\\Middle - Preço\\16_Chuva_Vazao", "Z:\\16_Chuva_Vazao");
            DirectoryCopy(path_final, path_Z, true);
            Directory.Delete(Path.Combine(localPath), true);
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


        internal static void Convert_BinDat
            (string metodo)
        {
            DateTime data_inicial = DateTime.Today.AddDays(-5);

            if (metodo == "funceme")
            {
                data_inicial = DateTime.Today.AddDays(-3);

            }
            DateTime data_final = DateTime.Today;
            var frmMain = new FrmMain();


            var localPath = Config.CaminhoPrevisao;

            var localModelo = Config.CaminhoModelo;


            var p1 = System.IO.Path.Combine(localPath, data_final.ToString("yyyyMM"), data_final.ToString("dd"));

            var p2 = System.IO.Path.Combine(localModelo, metodo + "00", data_inicial.ToString("yyyyMM"), data_inicial.ToString("dd"));

            if (metodo == "funceme")
            {
                localPath = Config.CaminhoFunceme;

                p1 = System.IO.Path.Combine(localPath, data_inicial.ToString("yyyy"), data_inicial.ToString("MM"));

            }
            else if (metodo == "merge")
            {
                localPath = Config.CaminhoMerge;

                p1 = System.IO.Path.Combine(localPath, data_inicial.ToString("yyyy"), data_inicial.ToString("MM"));


            }




            while (System.IO.Directory.Exists(p1) && (data_inicial <= data_final))
            {


                p2 = System.IO.Path.Combine(localModelo, metodo + "00", data_inicial.ToString("yyyyMM"), data_inicial.ToString("dd"));
                if (metodo == "funceme")
                {
                    p2 = System.IO.Path.Combine(localModelo, metodo, data_inicial.ToString("yyyyMM"), data_inicial.ToString("dd"));
                }
                else if (metodo == "merge")
                {
                    p2 = System.IO.Path.Combine(localModelo, metodo, data_inicial.ToString("yyyyMM"), data_inicial.ToString("dd"));
                }
                if (!System.IO.Directory.Exists(p2) || metodo == "funceme" || metodo == "merge")
                {


                    var localfilePath = System.IO.Path.Combine(localPath, data_inicial.ToString("yyyyMM"), data_inicial.ToString("dd"), metodo + "_00h");
                    if (!System.IO.Directory.Exists(localfilePath))
                    {
                        localfilePath = System.IO.Path.Combine(localPath, data_inicial.ToString("yyyyMM"), data_inicial.ToString("dd"), metodo + "00");
                    }
                    if (metodo == "funceme" || metodo == "merge")
                    {
                        localfilePath = System.IO.Path.Combine(localPath, data_inicial.ToString("yyyy"), data_inicial.ToString("MM"));

                    }

                    if (System.IO.Directory.Exists(localfilePath))
                    {
                        string[] arquivos = Directory.GetFiles(localfilePath, "*.ctl");

                        if (arquivos != null)
                        {
                            foreach (var file in arquivos)
                            {
                                if (System.IO.Path.GetExtension(file).ToUpperInvariant() == ".CTL")
                                {
                                    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"pp(\d{4})(\d{2})(\d{2})_(\d+)");
                                    if (metodo == "funceme" || metodo == "merge")
                                    {
                                        r = new System.Text.RegularExpressions.Regex(@"(\d{4})(\d{2})(\d{2})");
                                    }

                                    var fMatch = r.Match(file);
                                    if (fMatch.Success)
                                    {
                                        var data = new DateTime(
                                            int.Parse(fMatch.Groups[1].Value),
                                            int.Parse(fMatch.Groups[2].Value),
                                            int.Parse(fMatch.Groups[3].Value))
                                            ;
                                        var dataPrev = data;
                                        if (metodo != "funceme" && metodo != "merge")
                                        {
                                            var horas = int.Parse(fMatch.Groups[4].Value);

                                            dataPrev = data.AddHours(horas).Date;
                                        }

                                        frmMain.chuvas[dataPrev] = PrecipitacaoFactory.BuildFromMergeFile(file);
                                        frmMain.chuvas[dataPrev].Descricao = System.IO.Path
                                            .GetFileName(file);

                                        frmMain.chuvas[dataPrev].Data = dataPrev;

                                    }
                                }
                            }
                            var CaminhoArq = System.IO.Path.Combine(localModelo, metodo + "00", data_inicial.ToString("yyyy"));
                            if (metodo == "funceme" || metodo == "merge")
                            {
                                if (!System.IO.Directory.Exists(System.IO.Path.Combine(localModelo, metodo, data_inicial.ToString("yyyyMM"))))
                                {
                                    System.IO.Directory.CreateDirectory(System.IO.Path.Combine(localModelo, metodo, data_inicial.ToString("yyyyMM")));
                                }
                                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(localModelo, metodo, data_inicial.ToString("yyyyMM"), data_inicial.ToString("dd")));
                                CaminhoArq = System.IO.Path.Combine(localModelo, metodo, data_inicial.ToString("yyyyMM"), data_inicial.ToString("dd"));

                            }
                            else
                            {
                                if (!System.IO.Directory.Exists(System.IO.Path.Combine(localModelo, metodo + "00", data_inicial.ToString("yyyyMM"))))
                                {
                                    System.IO.Directory.CreateDirectory(System.IO.Path.Combine(localModelo, metodo + "00", data_inicial.ToString("yyyyMM")));
                                }
                                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(localModelo, metodo + "00", data_inicial.ToString("yyyyMM"), data_inicial.ToString("dd")));
                                CaminhoArq = System.IO.Path.Combine(localModelo, metodo + "00", data_inicial.ToString("yyyyMM"), data_inicial.ToString("dd"));

                            }
                            if (metodo == "funceme" || metodo == "merge")
                            {

                                foreach (var file in arquivos)
                                {
                                    if (System.IO.Path.GetExtension(file).ToUpperInvariant() == ".CTL")
                                    {
                                        System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"pp(\d{4})(\d{2})(\d{2})_(\d+)");
                                        if (metodo == "funceme" || metodo == "merge")
                                        {
                                            r = new System.Text.RegularExpressions.Regex(@"(\d{4})(\d{2})(\d{2})");
                                        }

                                        var fMatch = r.Match(file);
                                        if (fMatch.Success)
                                        {
                                            var data = new DateTime(
                                                int.Parse(fMatch.Groups[1].Value),
                                                int.Parse(fMatch.Groups[2].Value),
                                                int.Parse(fMatch.Groups[3].Value))
                                                ;
                                            var dataPrev = data;
                                            if (metodo != "funceme" && metodo != "merge")
                                            {
                                                var horas = int.Parse(fMatch.Groups[4].Value);

                                                dataPrev = data.AddHours(horas).Date;
                                            }

                                            frmMain.chuvas[dataPrev] = PrecipitacaoFactory.BuildFromMergeFile(file);
                                            frmMain.chuvas[dataPrev].Descricao = System.IO.Path
                                                .GetFileName(file);

                                            frmMain.chuvas[dataPrev].Data = dataPrev;
                                            //
                                            var name_prec = frmMain.chuvas[dataPrev].Descricao.ToString().Split('_');
                                            var name_file = "";
                                            if (name_prec.Count() > 2)
                                            {
                                                name_file = name_prec[0] + "_" + name_prec[1];
                                            }
                                            else
                                            {
                                                name_file = metodo;
                                            }
                                            var raiznome = name_file + "_p" + data.ToString("ddMMyy") + "a" + frmMain.chuvas[dataPrev].Data.ToString("ddMMyy") + ".dat";

                                            if (dataPrev == data_inicial)
                                            {
                                                frmMain.chuvas[dataPrev].SalvarModeloEta(System.IO.Path.Combine(CaminhoArq, raiznome));
                                            }
                                        }
                                    }
                                }

                                //foreach (var prec in frmMain.chuvas.Where(x => x.Key == data_inicial))
                                //{
                                //    System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"(\d{4})(\d{2})(\d{2})");
                                //    string ajuste_data = prec.Value.Descricao.ToString().Split('.').First();
                                //    var fMatch = r.Match(ajuste_data);
                                //    if (fMatch.Success)
                                //    {
                                //        var data = new DateTime(
                                //            int.Parse(fMatch.Groups[1].Value),
                                //            int.Parse(fMatch.Groups[2].Value),
                                //            int.Parse(fMatch.Groups[3].Value))
                                //            ;
                                //        var name_prec = prec.Value.Descricao.ToString().Split('_');
                                //        var name_file = "";
                                //        if (name_prec.Count() > 2)
                                //        {
                                //            name_file = name_prec[0] + "_" + name_prec[1];
                                //        }
                                //        else
                                //        {
                                //            name_file = metodo;
                                //        }
                                //        var raiznome = name_file + "_p" + data.ToString("ddMMyy") + "a" + prec.Value.Data.ToString("ddMMyy") + ".dat";
                                //        prec.Value.SalvarModeloEta(System.IO.Path.Combine(CaminhoArq, raiznome));


                                //    }
                                //    // var raiznome = prec.Value.Descricao.ToString().Split('.').First() + ".dat";
                                //    // raiznome = metodo + "_p" + data.ToString("ddMMyy") + "a" + prec.Value.Data.ToString("ddMMyy") + ".dat";
                                //    // prec.Value.SalvarModeloEta(System.IO.Path.Combine(CaminhoArq, raiznome));
                                //}
                            }
                            else
                            {
                                foreach (var prec in frmMain.chuvas.Where(x => x.Key >= data_inicial))
                                {

                                    if (metodo == "GEFS" || metodo == "GFS")
                                    {
                                        System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"pp(\d{4})(\d{2})(\d{2})_(\d+)");
                                        string ajuste_data = prec.Value.Descricao.ToString().Split('.').First();
                                        var fMatch = r.Match(ajuste_data);
                                        if (fMatch.Success)
                                        {
                                            var data = new DateTime(
                                                int.Parse(fMatch.Groups[1].Value),
                                                int.Parse(fMatch.Groups[2].Value),
                                                int.Parse(fMatch.Groups[3].Value))
                                                ;

                                            var raiznome = metodo + "_p" + data.ToString("ddMMyy") + "a" + prec.Value.Data.ToString("ddMMyy") + ".dat";
                                            prec.Value.SalvarModeloDAT(System.IO.Path.Combine(CaminhoArq, raiznome), metodo);
                                            //prec.Value.SalvarModeloEta(System.IO.Path.Combine(CaminhoArq, raiznome));
                                        }
                                    }
                                    else
                                    {
                                        var raiznome = prec.Value.Descricao.ToString().Split('.').First() + ".dat";
                                        prec.Value.SalvarModeloEta(System.IO.Path.Combine(CaminhoArq, raiznome));
                                        //prec.Value.SalvarModeloEta(System.IO.Path.Combine(CaminhoArq, raiznome));

                                    }
                                }
                            }
                        }


                    }
                    data_inicial = data_inicial.AddDays(1);
                    frmMain.chuvas.Clear();

                }
                else
                {
                    data_inicial = data_inicial.AddDays(1);
                }
            }
            if (metodo == "ECMWF")
            {
                Convert_BinDat("GFS");
            }
            if (metodo == "GFS")
            {
                Convert_BinDat("GEFS");
            }
            if (metodo == "GEFS")
            {
                Convert_BinDat("funceme");
            }
            //if (metodo == "funceme")
            //{
            //    Convert_BinDat("merge");
            //}



        }


        // public static void sendNotification(string message, string tos, string attachment = null)
        //{

        //    /*if (!tos.Contains("@"))
        //        tos = ChuvaVazaoTools.Config.ReceiversGroup;*/

        //    System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();

        //    msg.Body = " Acompanhamento de Precipitação \r\n\r\n";

        //    msg.Body = msg.Body + message + "\r\n";

        //    msg.Subject = "Acompanhamento de Precipitação";

        //    msg.Sender = msg.From = new System.Net.Mail.MailAddress("cpas.robot@gmail.com");


        //    msg.ReplyToList.Add(new System.Net.Mail.MailAddress("bruno.araujo@cpas.com.br"));


        //    foreach (var to in tos.Split(new char[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
        //    {
        //        msg.To.Add(to);
        //    }

        //    if (!string.IsNullOrWhiteSpace(attachment) && System.IO.File.Exists(attachment))
        //    {
        //        msg.Attachments.Add(
        //            new System.Net.Mail.Attachment(attachment)
        //            );
        //    }

        //    System.Net.Mail.SmtpClient cli = new System.Net.Mail.SmtpClient();

        //    cli.Host = "smtp.gmail.com";
        //    cli.Port = 587;
        //    cli.Credentials = new System.Net.NetworkCredential("cpas.robot@gmail.com", "cp@s9876");

        //    cli.EnableSsl = true;

        //    cli.Send(msg);  //.SendMailAsync(msg);
        //}
    }

    public class LogFile : System.IO.TextWriter
    {
        string file = "";

        string computerName = "";




        public LogFile(string filePath)
        {

            computerName = System.Environment.MachineName;
            file = filePath;
        }

        public override void WriteLine(string value)
        {
            try
            {
                System.IO.File.AppendAllText(file, $"{DateTime.Now.ToString()} - {computerName} - {value}{System.Environment.NewLine}");
                base.WriteLine(value);
            }
            finally { }
        }

        public override Encoding Encoding { get { return System.Text.Encoding.UTF8; } }
    }

    public static class Helper
    {
        public static Microsoft.Office.Interop.Excel.Application StartExcel()
        {
            Microsoft.Office.Interop.Excel.Application instance = null;
            try
            {
                instance = (Microsoft.Office.Interop.Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                instance = new Microsoft.Office.Interop.Excel.Application();
            }
            instance.Visible = true;

            return instance;
        }

        public static void Release(object o)
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(o);
        }
    }

}
