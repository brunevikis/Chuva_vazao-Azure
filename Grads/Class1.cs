using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradsHelper
{
    public static class Grads
    {
        public static void ConvertEta12ToImg(DateTime dt, string hora, string tempPath, string gradsScript)
        {
            //if (!System.IO.Directory.Exists(tempPath)) System.IO.Directory.CreateDirectory(tempPath);
            //var gradsScript = System.IO.Path.Combine(Config.CaminhoAuxiliar, "CREATE_GIF.gs");

            int horaI = int.Parse(hora);


            List<string> acumulados = new List<string>();

            for (int i = 1; i <= 10; i++)
            {
                var fileIdx = (12 - horaI) + (i) * 24;
                var openfile = "'open pp" + dt.ToString("yyyyMMdd") + "_" + fileIdx.ToString("0000") + ".ctl'";

                acumulados.Add(openfile);

                var gs = System.IO.File.ReadAllText(gradsScript);
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, "gradsScript.gs"),
                    gs
                    .Replace("%FILECOUNT%", "1")
                    .Replace("%OPENFILES%", openfile)
                    .Replace("%VARIABLE%", "prec")
                    .Replace("%HEADER_MODELO%", "Modelo Regional / Brasil")
                    .Replace("%HEADER_TITULO%", "Precipitacao(mm) acumulada entre 12Z " + dt.AddDays(i - 1).ToString("dd/MM") + " ate 12Z " + dt.AddDays(i).ToString("dd/MM"))
                    //.Replace("%HEADER_DATA%", "Previsao das " + hora + "Z dia " + dt.ToString("dd/MM"))
                    .Replace("%HEADER_DATA%", "Previsao das " + "00" + "Z dia " + dt.ToString("dd/MM"))
                    .Replace("%GIFFILE%", "prev" + i.ToString() + ".gif")
                    .Replace("%BINFILE%", "")
                );


                System.Diagnostics.Process pr = new System.Diagnostics.Process();


                var prInfo = new System.Diagnostics.ProcessStartInfo();
                prInfo.FileName = @"grads.exe";
                prInfo.CreateNoWindow = true;
                prInfo.UseShellExecute = true;
                prInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                prInfo.WorkingDirectory = tempPath;
                prInfo.Arguments = "-p -b -c gradsScript.gs";
                pr.StartInfo = prInfo;
                pr.Start();
                var verifica = pr.WaitForExit(180000);
                if (!verifica)
                {
                    pr.Kill();
                }
            }

            ///criar acumulado
            ///
            {
                var gs = System.IO.File.ReadAllText(gradsScript);
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, "gradsScript.gs"),
                    gs
                    .Replace("%FILECOUNT%", acumulados.Count.ToString())
                    .Replace("%OPENFILES%", string.Join("\r\n", acumulados))
                    .Replace("%VARIABLE%", "prec")
                    .Replace("%HEADER_MODELO%", "Modelo Regional / Brasil")
                    .Replace("%HEADER_TITULO%", "Precipitacao(mm) acumulada entre 12Z " + dt.ToString("dd/MM") + " ate 12Z " + dt.AddDays(10).ToString("dd/MM"))
                    .Replace("%HEADER_DATA%", "Previsao das " + "00" + "Z dia " + dt.ToString("dd/MM"))
                    .Replace("%GIFFILE%", "prev" + "_acumulado_10d" + ".gif")
                    .Replace("%BINFILE%", "")
                );

                System.Diagnostics.Process pr = new System.Diagnostics.Process();


                var prInfo = new System.Diagnostics.ProcessStartInfo();
                prInfo.FileName = @"grads.exe";
                prInfo.CreateNoWindow = true;
                prInfo.UseShellExecute = true;
                prInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                prInfo.WorkingDirectory = tempPath;
                prInfo.Arguments = "-p -b -c gradsScript.gs";
                pr.StartInfo = prInfo;
                pr.Start();
                var verifica = pr.WaitForExit(180000);
                if (!verifica)
                {
                    pr.Kill();
                }
            }

            //acumulado parcial
            {
                int a = 0;
                int b = 10;
                if (hora == "00") b = 9;
                else if (hora == "12") a = 1;
                else return;



                var gs = System.IO.File.ReadAllText(gradsScript);
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, "gradsScript.gs"),
                    gs
                    .Replace("%FILECOUNT%", acumulados.Skip(a).Take(b).Count().ToString())
                    .Replace("%OPENFILES%", string.Join("\r\n", acumulados.Skip(a).Take(b)))
                    .Replace("%VARIABLE%", "prec")
                    .Replace("%HEADER_MODELO%", "Modelo Regional / Brasil")
                    .Replace("%HEADER_TITULO%", "Precipitacao(mm) acumulada entre 12Z " + dt.AddDays(a).ToString("dd/MM") + " ate 12Z " + dt.AddDays(b).ToString("dd/MM"))
                    .Replace("%HEADER_DATA%", "Previsao das " + "00" + "Z dia " + dt.ToString("dd/MM"))
                    .Replace("%GIFFILE%", "prev" + "_acumulado_" + a.ToString() + "_" + b.ToString() + "d.gif")
                    .Replace("%BINFILE%", "")
                );

                System.Diagnostics.Process pr = new System.Diagnostics.Process();


                var prInfo = new System.Diagnostics.ProcessStartInfo();
                prInfo.FileName = @"grads.exe";
                prInfo.CreateNoWindow = true;
                prInfo.UseShellExecute = true;
                prInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                prInfo.WorkingDirectory = tempPath;
                prInfo.Arguments = "-p -b -c gradsScript.gs";
                pr.StartInfo = prInfo;
                pr.Start();
                var verifica = pr.WaitForExit(180000);
                if (!verifica)
                {
                    pr.Kill();
                }
                //System.Threading.Thread.Sleep(30000);
            }
        }

        public static void ConvertECMWFToImg(DateTime dt, string hora, string tempPath, string gradsScript,int contagem)
        {
            //if (!System.IO.Directory.Exists(tempPath)) System.IO.Directory.CreateDirectory(tempPath);
            //var gradsScript = System.IO.Path.Combine(Config.CaminhoAuxiliar, "CREATE_GIF.gs");

            int horaI = int.Parse(hora);


            List<string> acumulados = new List<string>();

            for (int i = 1; i <= contagem; i++)
            {
                var fileIdx = (12 - horaI) + (i) * 24;
                var openfile = "'open pp" + dt.ToString("yyyyMMdd") + "_" + fileIdx.ToString("0000") + ".ctl'";

                acumulados.Add(openfile);

                var gs = System.IO.File.ReadAllText(gradsScript);
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, "gradsScript.gs"),
                    gs
                    .Replace("%FILECOUNT%", "1")
                    .Replace("%OPENFILES%", openfile)
                    .Replace("%VARIABLE%", "prec")
                    .Replace("%HEADER_MODELO%", "ECMWF")
                    .Replace("%HEADER_TITULO%", "Precipitacao(mm) acumulada entre 12Z " + dt.AddDays(i - 1).ToString("dd/MM") + " ate 12Z " + dt.AddDays(i).ToString("dd/MM"))
                    //.Replace("%HEADER_DATA%", "Previsao das " + hora + "Z dia " + dt.ToString("dd/MM"))
                    .Replace("%HEADER_DATA%", "Previsao das " + "00" + "Z dia " + dt.ToString("dd/MM"))
                    .Replace("%GIFFILE%", "prev" + i.ToString() + ".gif")
                    .Replace("%BINFILE%", "")
                );


                System.Diagnostics.Process pr = new System.Diagnostics.Process();


                var prInfo = new System.Diagnostics.ProcessStartInfo();
                prInfo.FileName = @"grads.exe";
                prInfo.CreateNoWindow = true;
                prInfo.UseShellExecute = true;
                prInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                prInfo.WorkingDirectory = tempPath;
                prInfo.Arguments = "-p -b -c gradsScript.gs";
                pr.StartInfo = prInfo;
                pr.Start();
                var verifica = pr.WaitForExit(180000);
                if (!verifica)
                {
                    pr.Kill();
                }
            }

            ///criar acumulado
            ///
            {
                var gs = System.IO.File.ReadAllText(gradsScript);
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, "gradsScript.gs"),
                    gs
                    .Replace("%FILECOUNT%", acumulados.Count.ToString())
                    .Replace("%OPENFILES%", string.Join("\r\n", acumulados))
                    .Replace("%VARIABLE%", "prec")
                    .Replace("%HEADER_MODELO%", "ECMWF")
                    .Replace("%HEADER_TITULO%", "Precipitacao(mm) acumulada entre 12Z " + dt.ToString("dd/MM") + " ate 12Z " + dt.AddDays(contagem).ToString("dd/MM"))
                    .Replace("%HEADER_DATA%", "Previsao das " + "00" + "Z dia " + dt.ToString("dd/MM"))
                    .Replace("%GIFFILE%", "prev" + "_acumulado_" + contagem + "d" + ".gif")
                    .Replace("%BINFILE%", "")
                );

                System.Diagnostics.Process pr = new System.Diagnostics.Process();


                var prInfo = new System.Diagnostics.ProcessStartInfo();
                prInfo.FileName = @"grads.exe";
                prInfo.CreateNoWindow = true;
                prInfo.UseShellExecute = true;
                prInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                prInfo.WorkingDirectory = tempPath;
                prInfo.Arguments = "-p -b -c gradsScript.gs";
                pr.StartInfo = prInfo;
                pr.Start();
                var verifica = pr.WaitForExit(180000);
                if (!verifica)
                {
                    pr.Kill();
                }
            }

            //acumulado parcial
            {
                int a = 0;
                int b = contagem;
                if (hora == "00") b = contagem -1;
                else if (hora == "12") a = 1;
                else return;



                var gs = System.IO.File.ReadAllText(gradsScript);
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, "gradsScript.gs"),
                    gs
                    .Replace("%FILECOUNT%", acumulados.Skip(a).Take(b).Count().ToString())
                    .Replace("%OPENFILES%", string.Join("\r\n", acumulados.Skip(a).Take(b)))
                    .Replace("%VARIABLE%", "prec")
                    .Replace("%HEADER_MODELO%", "Modelo Regional / Brasil")
                    .Replace("%HEADER_TITULO%", "Precipitacao(mm) acumulada entre 12Z " + dt.AddDays(a).ToString("dd/MM") + " ate 12Z " + dt.AddDays(b).ToString("dd/MM"))
                    .Replace("%HEADER_DATA%", "Previsao das " + "00" + "Z dia " + dt.ToString("dd/MM"))
                    .Replace("%GIFFILE%", "prev" + "_acumulado_" + a.ToString() + "_" + b.ToString() + "d.gif")
                    .Replace("%BINFILE%", "")
                );

                System.Diagnostics.Process pr = new System.Diagnostics.Process();


                var prInfo = new System.Diagnostics.ProcessStartInfo();
                prInfo.FileName = @"grads.exe";
                prInfo.CreateNoWindow = true;
                prInfo.UseShellExecute = true;
                prInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                prInfo.WorkingDirectory = tempPath;
                prInfo.Arguments = "-p -b -c gradsScript.gs";
                pr.StartInfo = prInfo;
                pr.Start();
                var verifica = pr.WaitForExit(180000);
                if (!verifica)
                {
                    pr.Kill();
                }
                //System.Threading.Thread.Sleep(30000);
            }
        }

        public static void ConvertConjToImg(DateTime dt, string hora, string tempPath, string gradsScript, string nomemodelo = "Modelo Por Conjunto")
        {
            //var gradsScript = System.IO.Path.Combine(Config.CaminhoAuxiliar, "CREATE_GIF.gs");

            int horaI = int.Parse(hora);

            List<string> acumulados = new List<string>();

            for (int i = 1; i <= 100; i++)
            {
                var fileIdx = (12 - horaI) + (i) * 24;
                var filectl = "pp" + dt.ToString("yyyyMMdd") + "_" + fileIdx.ToString("0000") + ".ctl";

                if (!File.Exists(Path.Combine(tempPath, filectl))) break;

                var openfile = "'open " + filectl + "'";

                acumulados.Add(openfile);

                var gs = System.IO.File.ReadAllText(gradsScript);
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, "gradsScript.gs"),
                    gs
                    .Replace("%FILECOUNT%", "1")
                    .Replace("%OPENFILES%", openfile)
                    .Replace("%VARIABLE%", "prec")
                    .Replace("%HEADER_MODELO%", nomemodelo)
                    .Replace("%HEADER_TITULO%", "Precipitacao(mm) acumulada entre 12Z " + dt.AddDays(i - 1).ToString("dd/MM") + " ate 12Z " + dt.AddDays(i).ToString("dd/MM"))
                    .Replace("%HEADER_DATA%", "Previsao das " + hora + "Z dia " + dt.ToString("dd/MM"))
                    .Replace("%GIFFILE%", "prev" + i.ToString() + ".gif")
                    .Replace("%BINFILE%", "")
                );

                System.Diagnostics.Process pr = new System.Diagnostics.Process();


                var prInfo = new System.Diagnostics.ProcessStartInfo();
                prInfo.FileName = @"grads.exe";
                prInfo.CreateNoWindow = true;
                prInfo.UseShellExecute = true;
                prInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                prInfo.WorkingDirectory = tempPath;
                prInfo.Arguments = "-p -b -c gradsScript.gs";
                pr.StartInfo = prInfo;
                pr.Start();
                var verifica = pr.WaitForExit(180000);
                if (!verifica)
                {
                    pr.Kill();
                }
            }

            ///criar acumulado
            ///
            {
                var gs = System.IO.File.ReadAllText(gradsScript);
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, "gradsScript.gs"),
                    gs
                    .Replace("%FILECOUNT%", acumulados.Count.ToString())
                    .Replace("%OPENFILES%", string.Join("\r\n", acumulados))
                    .Replace("%VARIABLE%", "prec")
                    .Replace("%HEADER_MODELO%", nomemodelo)
                    .Replace("%HEADER_TITULO%", "Precipitacao(mm) acumulada entre 12Z " + dt.ToString("dd/MM") + " ate 12Z " + dt.AddDays(acumulados.Count).ToString("dd/MM"))
                    .Replace("%HEADER_DATA%", "Previsao das " + hora + "Z dia " + dt.ToString("dd/MM"))
                    .Replace("%GIFFILE%", "prev" + "_acumulado_10d" + ".gif")
                    .Replace("%BINFILE%", "")
                );

                System.Diagnostics.Process pr = new System.Diagnostics.Process();


                var prInfo = new System.Diagnostics.ProcessStartInfo();
                prInfo.FileName = @"grads.exe";
                prInfo.CreateNoWindow = true;
                prInfo.UseShellExecute = true;
                prInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                prInfo.WorkingDirectory = tempPath;
                prInfo.Arguments = "-p -b -c gradsScript.gs";
                pr.StartInfo = prInfo;
                pr.Start();
                var verifica = pr.WaitForExit(180000);
                if (!verifica)
                {
                    pr.Kill();
                }
            }

            //acumulado parcial
            {
                int a = 0;
                int b = 10;
                if (hora == "00") b = 9;
                else if (hora == "12") a = 1;
                else return;



                var gs = System.IO.File.ReadAllText(gradsScript);
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, "gradsScript.gs"),
                    gs
                    .Replace("%FILECOUNT%", acumulados.Skip(a).Take(b).Count().ToString())
                    .Replace("%OPENFILES%", string.Join("\r\n", acumulados.Skip(a).Take(b)))
                    .Replace("%VARIABLE%", "prec")
                    .Replace("%HEADER_MODELO%", nomemodelo)
                    .Replace("%HEADER_TITULO%", "Precipitacao(mm) acumulada entre 12Z " + dt.AddDays(a).ToString("dd/MM") + " ate 12Z " + dt.AddDays(b).ToString("dd/MM"))
                    .Replace("%HEADER_DATA%", "Previsao das " + hora + "Z dia " + dt.ToString("dd/MM"))
                    .Replace("%GIFFILE%", "prev" + "_acumulado_" + a.ToString() + "_" + b.ToString() + "d.gif")
                    .Replace("%BINFILE%", "")
                );

                System.Diagnostics.Process pr = new System.Diagnostics.Process();


                var prInfo = new System.Diagnostics.ProcessStartInfo();
                prInfo.FileName = @"grads.exe";
                prInfo.CreateNoWindow = true;
                prInfo.UseShellExecute = true;
                prInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                prInfo.WorkingDirectory = tempPath;
                prInfo.Arguments = "-p -b -c gradsScript.gs";
                pr.StartInfo = prInfo;
                pr.Start();
                var verifica = pr.WaitForExit(180000);
                if (!verifica)
                {
                    pr.Kill();
                }
            }
        }

        public static void ConvertCtlToImg(string ctlFilePath, string header, string subheader, string outImgName, string gradsScript)
        {
            //var gradsScript = System.IO.Path.Combine(Config.CaminhoAuxiliar, "CREATE_GIF.gs");

            var fname = System.IO.Path.GetFileNameWithoutExtension(ctlFilePath);
            var tempPath = System.IO.Path.GetDirectoryName(ctlFilePath);
            string outDatName = outImgName.Split('.').First()+".dat";
            var openfile = "'open " + ctlFilePath + ".ctl'";



            var gs = System.IO.File.ReadAllText(gradsScript);
            System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, "gradsScript.gs"),
                gs
                .Replace("%FILECOUNT%", "1")
                .Replace("%OPENFILES%", openfile)
                .Replace("%VARIABLE%", "prec")
                .Replace("%HEADER_MODELO%", header)
                .Replace("%HEADER_TITULO%", "Precipitacao(mm) acumulada em 24h")
                .Replace("%HEADER_DATA%", subheader)
                .Replace("%GIFFILE%", outImgName)
                .Replace("%BINFILE%", "")
                .Replace("%DATFILE%", outDatName)
            );

            System.Diagnostics.Process pr = new System.Diagnostics.Process();


            var prInfo = new System.Diagnostics.ProcessStartInfo();
            prInfo.FileName = @"grads.exe";
            prInfo.UseShellExecute = true;
            prInfo.CreateNoWindow = true;
            prInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            prInfo.WorkingDirectory = tempPath;
            prInfo.Arguments = "-p -b -c gradsScript.gs";
            pr.StartInfo = prInfo;
            pr.Start();
            var verifica = pr.WaitForExit(180000);
            if (!verifica)
            {
                pr.Kill();
            }
        }

        public static void ConvertCtlToImgGEFS(string ctlFilePath, string header, string subheader, string outImgName, string gradsScript, string complemento ="")
        {
            //var gradsScript = System.IO.Path.Combine(Config.CaminhoAuxiliar, "CREATE_GIF.gs");

            var fname = System.IO.Path.GetFileNameWithoutExtension(ctlFilePath);
            var tempPath = System.IO.Path.GetDirectoryName(ctlFilePath);

            var openfile = "'open " + ctlFilePath+"'";



            var gs = System.IO.File.ReadAllText(gradsScript);
            System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, "gradsScript.gs"),
                gs
                .Replace("%FILECOUNT%", "1")
                .Replace("%OPENFILES%", openfile)
                .Replace("%VARIABLE%", "prec")
                .Replace("%HEADER_MODELO%", header)
                .Replace("%HEADER_TITULO%", "Precipitacao(mm)" + complemento)
                .Replace("%HEADER_DATA%", subheader)
                .Replace("%GIFFILE%", outImgName)
                .Replace("%BINFILE%", "")
            );

            System.Diagnostics.Process pr = new System.Diagnostics.Process();


            var prInfo = new System.Diagnostics.ProcessStartInfo();
            prInfo.FileName = @"grads.exe";
            prInfo.CreateNoWindow = true;
            prInfo.UseShellExecute = true;
            prInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            prInfo.WorkingDirectory = tempPath;
            prInfo.Arguments = "-p -b -c gradsScript.gs";
            pr.StartInfo = prInfo;
            pr.Start();
            var verifica = pr.WaitForExit(180000);
            if (!verifica)
            {
                pr.Kill();
            }
        }



        public static void ConvertNoaaTropsToImg(DateTime dt, string hora, string tempPath, string modelo, string gradsScript)
        {
            //var gradsScript = System.IO.Path.Combine(Config.CaminhoAuxiliar, "CREATE_GIF.gs");

            int horaI = int.Parse(hora);

            List<string> acumulados = new List<string>();
            // 00 - i=18
            for (int i = 1; i <= 15; i++)
            {
                var fileIdx = (12 - horaI) + (i) * 24;
                var fname = $"pp{dt.ToString("yyyyMMdd")}_{fileIdx.ToString("0000")}.ctl";

                if (!File.Exists(Path.Combine(tempPath, fname))) continue;
                var openfile = "'open " + fname + "'";

                var gs = System.IO.File.ReadAllText(gradsScript);
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, "gradsScript.gs"),
                    gs
                    .Replace("%FILECOUNT%", "1")
                    .Replace("%OPENFILES%", openfile)
                    .Replace("%VARIABLE%", "prec")
                    .Replace("%HEADER_MODELO%", modelo)
                    .Replace("%HEADER_TITULO%", "Precipitacao(mm) acumulada entre 12Z " + dt.AddDays(i - 1).ToString("dd/MM") + " ate 12Z " + dt.AddDays(i).ToString("dd/MM"))
                    .Replace("%HEADER_DATA%", "Previsao das " + hora + "Z dia " + dt.ToString("dd/MM"))
                    .Replace("%GIFFILE%", "prev" + i.ToString() + ".gif")
                    .Replace("%BINFILE%", "") // pp20180906_036
                    );

                System.Diagnostics.Process pr = new System.Diagnostics.Process();

                var prInfo = new System.Diagnostics.ProcessStartInfo();
                prInfo.FileName = @"grads.exe";
                prInfo.CreateNoWindow = true;
                prInfo.UseShellExecute = true;
                prInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                prInfo.WorkingDirectory = tempPath;
                prInfo.Arguments = "-p -b -c gradsScript.gs";
                pr.StartInfo = prInfo;
                pr.Start();
                var verifica = pr.WaitForExit(180000);
                if (!verifica)
                {
                    pr.Kill();
                }

                if (i <= 10) acumulados.Add(openfile);
            }

            ///criar acumulado
            ///
            {
                var gs = System.IO.File.ReadAllText(gradsScript);
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, "gradsScript.gs"),
                    gs
                    .Replace("%FILECOUNT%", acumulados.Count.ToString())
                    .Replace("%OPENFILES%", string.Join("\r\n", acumulados))
                    .Replace("%VARIABLE%", "prec")
                    .Replace("%HEADER_MODELO%", modelo)
                    .Replace("%HEADER_TITULO%", "Precipitacao(mm) acumulada entre 12Z " + dt.ToString("dd/MM") + " ate 12Z " + dt.AddDays(10).ToString("dd/MM"))
                    .Replace("%HEADER_DATA%", "Previsao das " + hora + "Z dia " + dt.ToString("dd/MM"))
                    .Replace("%GIFFILE%", "prev" + "_acumulado_10d" + ".gif")
                    .Replace("%BINFILE%", "")
                );

                System.Diagnostics.Process pr = new System.Diagnostics.Process();


                var prInfo = new System.Diagnostics.ProcessStartInfo();
                prInfo.FileName = @"grads.exe";
                prInfo.CreateNoWindow = true;
                prInfo.UseShellExecute = true;
                prInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                prInfo.WorkingDirectory = tempPath;
                prInfo.Arguments = "-p -b -c gradsScript.gs";
                pr.StartInfo = prInfo;
                pr.Start();
                var verifica = pr.WaitForExit(180000);
                if (!verifica)
                {
                    pr.Kill();
                }
            }

            //acumulado parcial
            {
                int a = 0;
                int b = 10;
                if (hora == "00") b = 9;
                else if (hora == "12") a = 1;
                else return;



                var gs = System.IO.File.ReadAllText(gradsScript);
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, "gradsScript.gs"),
                    gs
                    .Replace("%FILECOUNT%", acumulados.Skip(a).Take(b).Count().ToString())
                    .Replace("%OPENFILES%", string.Join("\r\n", acumulados.Skip(a).Take(b)))
                    .Replace("%VARIABLE%", "prec")
                    .Replace("%HEADER_MODELO%", modelo)
                    .Replace("%HEADER_TITULO%", "Precipitacao(mm) acumulada entre 12Z " + dt.AddDays(a).ToString("dd/MM") + " ate 12Z " + dt.AddDays(b).ToString("dd/MM"))
                    .Replace("%HEADER_DATA%", "Previsao das " + hora + "Z dia " + dt.ToString("dd/MM"))
                    .Replace("%GIFFILE%", "prev" + "_acumulado_" + a.ToString() + "_" + b.ToString() + "d.gif")
                    .Replace("%BINFILE%", "")
                );

                System.Diagnostics.Process pr = new System.Diagnostics.Process();


                var prInfo = new System.Diagnostics.ProcessStartInfo();
                prInfo.FileName = @"grads.exe";
                prInfo.CreateNoWindow = true;
                prInfo.UseShellExecute = true;
                prInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                prInfo.WorkingDirectory = tempPath;
                prInfo.Arguments = "-p -b -c gradsScript.gs";
                pr.StartInfo = prInfo;
                pr.Start();
                var verifica = pr.WaitForExit(180000);
                if (!verifica)
                {
                    pr.Kill();
                }
            }
        }

        public static void ConvertNoaaGEFSToImg(DateTime dt, string hora, string tempPath, string modelo, string gradsScript)
        {
            //var gradsScript = System.IO.Path.Combine(Config.CaminhoAuxiliar, "CREATE_GIF.gs");

            int horaI = int.Parse(hora);

            var fileRadical = modelo == "GEFS" ? "geavg.t" : "gec00.t";


            List<string> acumulados = new List<string>();
            // 00 - i=18
            for (int i = 1; i <= 15; i++)
            {
                var fileIdx = (18 - horaI) + (i - 1) * 24;

                var openfiles = new string[] {
                "'open " + fileRadical + hora + "z.pgrb2af" + (fileIdx ).ToString("00") + ".ctl'",
                "'open " + fileRadical + hora + "z.pgrb2af" + (fileIdx+6).ToString("00") + ".ctl'",
                "'open " + fileRadical + hora + "z.pgrb2af" + (fileIdx+12).ToString("00") + ".ctl'",
                "'open " + fileRadical + hora + "z.pgrb2af" + (fileIdx+18).ToString("00") + ".ctl'"
                };

                var binname = "pp" + dt.ToString("yyyyMMdd") + "_" + ((i * 24) + (12 - horaI)).ToString("0000");

                var gs = System.IO.File.ReadAllText(gradsScript);
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, "gradsScript.gs"),
                    gs
                    .Replace("%FILECOUNT%", "4")
                    .Replace("%OPENFILES%", string.Join("\r\n", openfiles))
                    .Replace("%VARIABLE%", "apcpsfc")
                    .Replace("%HEADER_MODELO%", modelo)
                    .Replace("%HEADER_TITULO%", "Precipitacao(mm) acumulada entre 12Z " + dt.AddDays(i - 1).ToString("dd/MM") + " ate 12Z " + dt.AddDays(i).ToString("dd/MM"))
                    .Replace("%HEADER_DATA%", "Previsao das " + hora + "Z dia " + dt.ToString("dd/MM"))
                    .Replace("%GIFFILE%", "prev" + i.ToString() + ".gif")
                    .Replace("%BINFILE%", binname + ".bin") // pp20180906_036
                    );

                System.Diagnostics.Process pr = new System.Diagnostics.Process();

                var prInfo = new System.Diagnostics.ProcessStartInfo();
                prInfo.FileName = @"grads.exe";
                prInfo.CreateNoWindow = true;
                prInfo.UseShellExecute = true;
                prInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                prInfo.WorkingDirectory = tempPath;
                prInfo.Arguments = "-p -b -c gradsScript.gs";
                pr.StartInfo = prInfo;
                pr.Start();
                var verifica = pr.WaitForExit(180000);
                if (!verifica)
                {
                    pr.Kill();
                }

                var ctlContent =
    @"DSET ^" + binname + ".bin" + @"
UNDEF -9999.
TITLE Previsao " + modelo + @"
XDEF  42  LINEAR  -75.00   1.00
YDEF  41  LINEAR  -35.00   1.00
ZDEF   1 LEVELS 1000
TDEF   1 LINEAR 12Z" + dt.AddDays(i).ToString("ddMMMyyyy", System.Globalization.DateTimeFormatInfo.InvariantInfo) + @" 24hr
VARS  1
PREC    0  99     Total  24h Precip.        (m)
ENDVARS
";
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, binname + ".ctl"), ctlContent);

                if (i <= 10) acumulados.Add("'open " + binname + ".ctl'");
            }

            ///criar acumulado
            ///
            {
                var gs = System.IO.File.ReadAllText(gradsScript);
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, "gradsScript.gs"),
                    gs
                    .Replace("%FILECOUNT%", acumulados.Count.ToString())
                    .Replace("%OPENFILES%", string.Join("\r\n", acumulados))
                    .Replace("%VARIABLE%", "prec")
                    .Replace("%HEADER_MODELO%", modelo)
                    .Replace("%HEADER_TITULO%", "Precipitacao(mm) acumulada entre 12Z " + dt.ToString("dd/MM") + " ate 12Z " + dt.AddDays(10).ToString("dd/MM"))
                    .Replace("%HEADER_DATA%", "Previsao das " + hora + "Z dia " + dt.ToString("dd/MM"))
                    .Replace("%GIFFILE%", "prev" + "_acumulado_10d" + ".gif")
                    .Replace("%BINFILE%", "")
                );

                System.Diagnostics.Process pr = new System.Diagnostics.Process();


                var prInfo = new System.Diagnostics.ProcessStartInfo();
                prInfo.FileName = @"grads.exe";
                prInfo.CreateNoWindow = true;
                prInfo.UseShellExecute = true;
                prInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                prInfo.WorkingDirectory = tempPath;
                prInfo.Arguments = "-p -b -c gradsScript.gs";
                pr.StartInfo = prInfo;
                pr.Start();
                var verifica = pr.WaitForExit(180000);
                if (!verifica)
                {
                    pr.Kill();
                }
            }

            //acumulado parcial
            {
                int a = 0;
                int b = 10;
                if (hora == "00") b = 9;
                else if (hora == "12") a = 1;
                else return;



                var gs = System.IO.File.ReadAllText(gradsScript);
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, "gradsScript.gs"),
                    gs
                    .Replace("%FILECOUNT%", acumulados.Skip(a).Take(b).Count().ToString())
                    .Replace("%OPENFILES%", string.Join("\r\n", acumulados.Skip(a).Take(b)))
                    .Replace("%VARIABLE%", "prec")
                    .Replace("%HEADER_MODELO%", modelo)
                    .Replace("%HEADER_TITULO%", "Precipitacao(mm) acumulada entre 12Z " + dt.AddDays(a).ToString("dd/MM") + " ate 12Z " + dt.AddDays(b).ToString("dd/MM"))
                    .Replace("%HEADER_DATA%", "Previsao das " + hora + "Z dia " + dt.ToString("dd/MM"))
                    .Replace("%GIFFILE%", "prev" + "_acumulado_" + a.ToString() + "_" + b.ToString() + "d.gif")
                    .Replace("%BINFILE%", "")
                );

                System.Diagnostics.Process pr = new System.Diagnostics.Process();


                var prInfo = new System.Diagnostics.ProcessStartInfo();
                prInfo.FileName = @"grads.exe";
                prInfo.CreateNoWindow = true;
                prInfo.UseShellExecute = true;
                prInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                prInfo.WorkingDirectory = tempPath;
                prInfo.Arguments = "-p -b -c gradsScript.gs";
                pr.StartInfo = prInfo;
                pr.Start();
                var verifica = pr.WaitForExit(180000);
                if (!verifica)
                {
                    pr.Kill();
                }
            }
        }

        public static void ConvertNoaaGFSToImg(DateTime dt, string hora, string tempPath, string modelo, string gradsScript)
        {
            //var gradsScript = System.IO.Path.Combine(Config.CaminhoAuxiliar, "CREATE_GIF.gs");

            int horaI = int.Parse(hora);

            List<string> acumulados = new List<string>();
            // 00 - i=18
            for (int i = 1; i <= 15; i++)
            {
                var fileIdx = (15 - horaI) + (i - 1) * 24;

                var files = new string[] {
                    "gfs.t" + hora + "z.pgrb2.1p00.f" + (fileIdx ).ToString("000") + ".ctl",
                    "gfs.t" + hora + "z.pgrb2.1p00.f" + (fileIdx+3).ToString("000") + ".ctl",
                    "gfs.t" + hora + "z.pgrb2.1p00.f" + (fileIdx+6).ToString("000") + ".ctl",
                    "gfs.t" + hora + "z.pgrb2.1p00.f" + (fileIdx+9).ToString("000") + ".ctl",
                    "gfs.t" + hora + "z.pgrb2.1p00.f" + (fileIdx+12).ToString("000") + ".ctl",
                    "gfs.t" + hora + "z.pgrb2.1p00.f" + (fileIdx+15).ToString("000") + ".ctl",
                    "gfs.t" + hora + "z.pgrb2.1p00.f" + (fileIdx+18).ToString("000") + ".ctl",
                    "gfs.t" + hora + "z.pgrb2.1p00.f" + (fileIdx+21).ToString("000") + ".ctl"
                };

                var openfiles = files.Where(x => System.IO.File.Exists(System.IO.Path.Combine(tempPath, x))).Select(x => "'open " + x + "'").ToArray();

                ////gfs.t00z.pgrb2.1p00.f384
                //var openfiles = new string[] {
                //        "'open " + "gfs.t" + hora + "z.pgrb2.1p00.f" + (fileIdx ).ToString("000") + ".ctl'",
                //        "'open " + "gfs.t" + hora + "z.pgrb2.1p00.f" + (fileIdx+3).ToString("000") + ".ctl'",
                //        "'open " + "gfs.t" + hora + "z.pgrb2.1p00.f" + (fileIdx+6).ToString("000") + ".ctl'",
                //        "'open " + "gfs.t" + hora + "z.pgrb2.1p00.f" + (fileIdx+9).ToString("000") + ".ctl'",
                //        "'open " + "gfs.t" + hora + "z.pgrb2.1p00.f" + (fileIdx+12).ToString("000") + ".ctl'",
                //        "'open " + "gfs.t" + hora + "z.pgrb2.1p00.f" + (fileIdx+15).ToString("000") + ".ctl'",
                //        "'open " + "gfs.t" + hora + "z.pgrb2.1p00.f" + (fileIdx+18).ToString("000") + ".ctl'",
                //        "'open " + "gfs.t" + hora + "z.pgrb2.1p00.f" + (fileIdx+21).ToString("000") + ".ctl'"
                //        };



                var binname = "pp" + dt.ToString("yyyyMMdd") + "_" + ((i * 24) + (12 - horaI)).ToString("0000");

                var gs = System.IO.File.ReadAllText(gradsScript);
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, "gradsScript.gs"),
                    gs
                    .Replace("%FILECOUNT%", openfiles.Count().ToString())
                    .Replace("%OPENFILES%", string.Join("\r\n", openfiles))
                    .Replace("%VARIABLE%", "apcpsfc")
                    .Replace("%HEADER_MODELO%", "gfs.t" + hora)
                    .Replace("%HEADER_TITULO%", "Precipitacao(mm) acumulada entre 12Z " + dt.AddDays(i - 1).ToString("dd/MM") + " ate 12Z " + dt.AddDays(i).ToString("dd/MM"))
                    .Replace("%HEADER_DATA%", "Previsao das " + hora + "Z dia " + dt.ToString("dd/MM"))
                    .Replace("%GIFFILE%", "prev" + i.ToString() + ".gif")
                    .Replace("%BINFILE%", binname + ".bin") // pp20180906_036
                    );

                System.Diagnostics.Process pr = new System.Diagnostics.Process();

                var prInfo = new System.Diagnostics.ProcessStartInfo();
                prInfo.FileName = @"grads.exe";
                prInfo.CreateNoWindow = true;
                prInfo.UseShellExecute = true;
                prInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                prInfo.WorkingDirectory = tempPath;
                prInfo.Arguments = "-p -b -c gradsScript.gs";
                pr.StartInfo = prInfo;
                pr.Start();
                var verifica = pr.WaitForExit(180000);
                if (!verifica)
                {
                    pr.Kill();
                }

                var ctlContent =
    @"DSET ^" + binname + ".bin" + @"
UNDEF -9999.
TITLE Previsao " + modelo + @"
XDEF  42  LINEAR  -75.00   1.00
YDEF  41  LINEAR  -35.00   1.00
ZDEF   1 LEVELS 1000
TDEF   1 LINEAR 12Z" + dt.AddDays(i).ToString("ddMMMyyyy", System.Globalization.DateTimeFormatInfo.InvariantInfo) + @" 24hr
VARS  1
PREC    0  99     Total  24h Precip.        (m)
ENDVARS
";
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, binname + ".ctl"), ctlContent);

                if (i <= 10) acumulados.Add("'open " + binname + ".ctl'");
            }

            ///criar acumulado
            ///
            {
                var gs = System.IO.File.ReadAllText(gradsScript);
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, "gradsScript.gs"),
                    gs
                    .Replace("%FILECOUNT%", acumulados.Count.ToString())
                    .Replace("%OPENFILES%", string.Join("\r\n", acumulados))
                    .Replace("%VARIABLE%", "prec")
                    .Replace("%HEADER_MODELO%", "gfs.t" + hora)
                    .Replace("%HEADER_TITULO%", "Precipitacao(mm) acumulada entre 12Z " + dt.ToString("dd/MM") + " ate 12Z " + dt.AddDays(10).ToString("dd/MM"))
                    .Replace("%HEADER_DATA%", "Previsao das " + hora + "Z dia " + dt.ToString("dd/MM"))
                    .Replace("%GIFFILE%", "prev" + "_acumulado_10d" + ".gif")
                    .Replace("%BINFILE%", "")
                );

                System.Diagnostics.Process pr = new System.Diagnostics.Process();


                var prInfo = new System.Diagnostics.ProcessStartInfo();
                prInfo.FileName = @"grads.exe";
                prInfo.CreateNoWindow = true;
                prInfo.UseShellExecute = true;
                prInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                prInfo.WorkingDirectory = tempPath;
                prInfo.Arguments = "-p -b -c gradsScript.gs";
                pr.StartInfo = prInfo;
                pr.Start();
                var verifica = pr.WaitForExit(180000);
                if (!verifica)
                {
                    pr.Kill();
                }
            }

            //acumulado parcial
            {
                int a = 0;
                int b = 10;
                if (hora == "00") b = 9;
                else if (hora == "12") a = 1;
                else return;



                var gs = System.IO.File.ReadAllText(gradsScript);
                System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, "gradsScript.gs"),
                    gs
                    .Replace("%FILECOUNT%", acumulados.Skip(a).Take(b).Count().ToString())
                    .Replace("%OPENFILES%", string.Join("\r\n", acumulados.Skip(a).Take(b)))
                    .Replace("%VARIABLE%", "prec")
                    .Replace("%HEADER_MODELO%", "gfs.t" + hora)
                    .Replace("%HEADER_TITULO%", "Precipitacao(mm) acumulada entre 12Z " + dt.AddDays(a).ToString("dd/MM") + " ate 12Z " + dt.AddDays(b).ToString("dd/MM"))
                    .Replace("%HEADER_DATA%", "Previsao das " + hora + "Z dia " + dt.ToString("dd/MM"))
                    .Replace("%GIFFILE%", "prev" + "_acumulado_" + a.ToString() + "_" + b.ToString() + "d.gif")
                    .Replace("%BINFILE%", "")
                );

                System.Diagnostics.Process pr = new System.Diagnostics.Process();


                var prInfo = new System.Diagnostics.ProcessStartInfo();
                prInfo.FileName = @"grads.exe";
                prInfo.CreateNoWindow = true;
                prInfo.UseShellExecute = true;
                prInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                prInfo.WorkingDirectory = tempPath;
                prInfo.Arguments = "-p -b -c gradsScript.gs";
                pr.StartInfo = prInfo;
                pr.Start();
                var verifica = pr.WaitForExit(180000);
                if (!verifica)
                {
                    pr.Kill();
                }
            }
        }

        public static void CreateImgFromFiles(IEnumerable<string> ctlFiles, string header, string subheader, string outImgFile, string gradsScriptBase)
        {

            var localPath = System.IO.Path.GetTempPath() + "grads_img2\\";
            localPath += DateTime.Now.ToString("HHmmss");
            if (System.IO.Directory.Exists(localPath)) System.IO.Directory.Delete(localPath, true);
            System.IO.Directory.CreateDirectory(localPath);



            foreach (var f in ctlFiles.Distinct())
            {
                System.IO.File.Copy(f, Path.Combine(localPath, Path.GetFileName(f)));
                System.IO.File.Copy(Path.ChangeExtension(f, ".bin"), Path.ChangeExtension(Path.Combine(localPath, Path.GetFileName(f)), ".bin"));
            }

            var tempPath = localPath;

            var openfile = string.Join("\n", ctlFiles.Select(f => "'open " + Path.GetFileName(f) + "'"));

            var gs = System.IO.File.ReadAllText(gradsScriptBase);

            System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, "gradsScript.gs"),
                gs
                .Replace("%FILECOUNT%", ctlFiles.Count().ToString())
                .Replace("%OPENFILES%", openfile)
                .Replace("%VARIABLE%", "prec")
                .Replace("%HEADER_MODELO%", header)
                .Replace("%HEADER_TITULO%", "Precipitacao(mm) acumulada em " + ctlFiles.Count().ToString() + " dias")
                .Replace("%HEADER_DATA%", subheader)
                .Replace("%GIFFILE%", Path.GetFileName(outImgFile))
                .Replace("%BINFILE%", "")
            );

            System.Diagnostics.Process pr = new System.Diagnostics.Process();


            var prInfo = new System.Diagnostics.ProcessStartInfo();
            prInfo.FileName = @"grads.exe";
            prInfo.CreateNoWindow = true;
            prInfo.UseShellExecute = true;
            prInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            prInfo.WorkingDirectory = tempPath;
            prInfo.Arguments = "-p -b -c gradsScript.gs";
            pr.StartInfo = prInfo;
            pr.Start();

            var verifica = pr.WaitForExit(180000);
            if (!verifica)
            {
                pr.Kill();
            }

            try
            {

                File.Copy(Path.Combine(localPath, Path.GetFileName(outImgFile)), outImgFile, true);
            

            System.IO.Directory.Delete(localPath, true);
            }
            catch { }
        }
        /// <summary>
        /// ( Sum imgsA ) - ( Sum imgsB)
        /// </summary>
        /// <param name="ctlFiles"></param>
        /// <param name="header"></param>
        /// <param name="subheader"></param>
        /// <param name="outImgFile"></param>
        /// <param name="gradsScriptBase"></param>
        public static void CreateImgDiffFromFiles(IEnumerable<string> ctlFilesA, IEnumerable<string> ctlFilesB, string header, string subheader, string outImgFile, string gradsScriptBase)
        {

            var localPath = System.IO.Path.GetTempPath() + "grads_img2\\";
            localPath += DateTime.Now.ToString("HHmmss");
            if (System.IO.Directory.Exists(localPath)) System.IO.Directory.Delete(localPath, true);
            System.IO.Directory.CreateDirectory(localPath);


            var ctlFilesBRen = ctlFilesB.Select(x => new
            {
                Orig = x,
                Nome =
                ctlFilesA.Any(y => Path.GetFileName(y) == Path.GetFileName(x)) ?
                "tmp_" + Path.GetFileName(x)
                : Path.GetFileName(x)

            });

            foreach (var f in ctlFilesA.Distinct()/*.Union(ctlFilesB)*/)
            {
                System.IO.File.Copy(f, Path.Combine(localPath, Path.GetFileName(f)));
                System.IO.File.Copy(Path.ChangeExtension(f, ".bin"), Path.ChangeExtension(Path.Combine(localPath, Path.GetFileName(f)), ".bin"));
            }

            foreach (var f in ctlFilesBRen.Where(x => !File.Exists(Path.Combine(localPath, x.Nome))).Distinct()/*.Union(ctlFilesB)*/)
            {
                var ctltxt = File.ReadAllText(f.Orig);
                ctltxt = ctltxt.Replace(Path.GetFileNameWithoutExtension(f.Orig), Path.GetFileNameWithoutExtension(f.Nome));

                File.WriteAllText(Path.Combine(localPath, f.Nome), ctltxt);
                //System.IO.File.Copy(f.Orig, Path.Combine(localPath, f.Nome));
                System.IO.File.Copy(Path.ChangeExtension(f.Orig, ".bin"), Path.ChangeExtension(Path.Combine(localPath, f.Nome), ".bin"));

            }



            var tempPath = localPath;

            var openfileA = string.Join("\n", ctlFilesA.Select(f => "'open " + Path.GetFileName(f) + "'"));
            var openfileB = string.Join("\n", ctlFilesBRen.Select(f => "'open " + f.Nome + "'"));

            var gs = System.IO.File.ReadAllText(gradsScriptBase);

            System.IO.File.WriteAllText(System.IO.Path.Combine(tempPath, "gradsScript.gs"),
                gs
                .Replace("%FILECOUNTA%", ctlFilesA.Count().ToString())
                .Replace("%OPENFILESA%", openfileA)
                .Replace("%FILECOUNTB%", ctlFilesB.Count().ToString())
                .Replace("%OPENFILESB%", openfileB)
                .Replace("%VARIABLE%", "prec")
                .Replace("%HEADER_MODELO%", header)
                .Replace("%HEADER_TITULO%", "Variacao de Precipitacao(mm) acumulada")
                .Replace("%HEADER_DATA%", subheader)
                .Replace("%GIFFILE%", Path.GetFileName(outImgFile))
                .Replace("%BINFILE%", "")
            );

            System.Diagnostics.Process pr = new System.Diagnostics.Process();


            var prInfo = new System.Diagnostics.ProcessStartInfo();
            prInfo.FileName = @"grads.exe";
            prInfo.CreateNoWindow = true;
            prInfo.UseShellExecute = true;
            prInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            prInfo.WorkingDirectory = tempPath;
            prInfo.Arguments = "-p -b -c gradsScript.gs";
            pr.StartInfo = prInfo;
            pr.Start();
            var verifica = pr.WaitForExit(180000);
            if (!verifica)
            {
                pr.Kill();
            }


            try
            {
                File.Copy(Path.Combine(localPath, Path.GetFileName(outImgFile)), outImgFile, true);

                System.IO.Directory.Delete(localPath, true);
            }
            catch { }
        }
    }
}
