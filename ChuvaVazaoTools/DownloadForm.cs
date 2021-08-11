using GradsHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChuvaVazaoTools
{
    public partial class DownloadForm : Form
    {
        private Dictionary<DateTime, Precipitacao> chuvas;

        public DownloadForm(DateTime? data = null)
        {
            InitializeComponent();

            dateTimePicker1.Value = data ?? DateTime.Today;
            this.DialogResult = DialogResult.Cancel;
        }

        public DownloadForm(DateTime? data = null, Dictionary<DateTime, Precipitacao> chuvas = null) : this(data)
        {
            this.chuvas = chuvas;
        }

        public bool TemEta00 { set { button9.BackColor = value ? SystemColors.Control : Color.Red; } }
        public bool TemEta12 { set { button7.BackColor = value ? SystemColors.Control : Color.Red; } }
        public bool TemGefs00 { set { button1.BackColor = value ? SystemColors.Control : Color.Red; } }
        public bool TemGefs06 { set { button2.BackColor = value ? SystemColors.Control : Color.Red; } }
        public bool TemGefs12 { set { button3.BackColor = value ? SystemColors.Control : Color.Red; } }
        public bool TemGfs00 { set { button6.BackColor = value ? SystemColors.Control : Color.Red; } }
        public bool TemGfs06 { set { button5.BackColor = value ? SystemColors.Control : Color.Red; } }
        public bool TemGfs12 { set { button4.BackColor = value ? SystemColors.Control : Color.Red; } }

        public bool TemConjunto00 { set { button10.BackColor = value ? SystemColors.Control : Color.Red; } }
        public bool TemConjunto12 { set { button8.BackColor = value ? SystemColors.Control : Color.Red; } }

        private void DownloadForm_Load(object sender, EventArgs e)
        {
            LoadStatus(dateTimePicker1.Value);
        }

        private void LoadStatus(DateTime date)
        {

            TemEta00 =
            TemEta12 =
            TemGefs00 =
            TemGefs06 =
            TemGefs12 =
            TemGfs00 =
            TemGfs06 =
            TemGfs12 =
            TemConjunto00 =
            TemConjunto12 = true;

            var searchPath = System.IO.Path.Combine(Config.CaminhoPrevisao, date.ToString("yyyyMM"), date.ToString("dd"));
            for (int i = 1; i <= 10; i++)
            {
                var dataPrev = date.AddDays(i);

                if (!File.Exists(System.IO.Path.Combine(searchPath, "ETA00", "pp" + date.ToString("yyyyMMdd") + "_" + (i * 24 + 12).ToString("0000") + ".ctl"))) TemEta00 = false;
                if (!File.Exists(System.IO.Path.Combine(searchPath, "ETA12", "pp" + date.ToString("yyyyMMdd") + "_" + (i * 24).ToString("0000") + ".ctl"))) TemEta12 = false;

                if (!File.Exists(System.IO.Path.Combine(searchPath, "GEFS00", "pp" + date.ToString("yyyyMMdd") + "_" + ((i * 24) + 12).ToString("0000") + ".ctl"))) TemGefs00 = false;
                if (!File.Exists(System.IO.Path.Combine(searchPath, "GEFS06", "pp" + date.ToString("yyyyMMdd") + "_" + ((i * 24) + 6).ToString("0000") + ".ctl"))) TemGefs06 = false;
                if (!File.Exists(System.IO.Path.Combine(searchPath, "GEFS12", "pp" + date.ToString("yyyyMMdd") + "_" + (i * 24).ToString("0000") + ".ctl"))) TemGefs12 = false;

                if (!File.Exists(System.IO.Path.Combine(searchPath, "GFS00", "pp" + date.ToString("yyyyMMdd") + "_" + ((i * 24) + 12).ToString("0000") + ".ctl"))) TemGfs00 = false;
                if (!File.Exists(System.IO.Path.Combine(searchPath, "GFS06", "pp" + date.ToString("yyyyMMdd") + "_" + ((i * 24) + 6).ToString("0000") + ".ctl"))) TemGfs06 = false;
                if (!File.Exists(System.IO.Path.Combine(searchPath, "GFS12", "pp" + date.ToString("yyyyMMdd") + "_" + (i * 24).ToString("0000") + ".ctl"))) TemGfs12 = false;

                if (!File.Exists(System.IO.Path.Combine(searchPath, "CONJUNTO00", "pp" + date.ToString("yyyyMMdd") + "_" + ((i * 24) + 12).ToString("0000") + ".ctl"))) TemConjunto00 = false;
                if (!File.Exists(System.IO.Path.Combine(searchPath, "CONJUNTO12", "pp" + date.ToString("yyyyMMdd") + "_" + (i * 24).ToString("0000") + ".ctl"))) TemConjunto12 = false;
            }



        }

        public Func<TextWriter, Task> Acao { get; private set; }

        private void button_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;


            var btn = sender as Button;

            var data = dateTimePicker1.Value;
            var searchPath = System.IO.Path.Combine(Config.CaminhoPrevisao, data.ToString("yyyyMM"), data.ToString("dd"));
            var funcLogsExtended = new Action(() =>
            {
                var frm = WaitForm2.CreateInstance(data);
                var eta = WaitForm2.TipoEta._00h;
                var gefs = WaitForm2.TipoGefs._00h;

                frm.LimparCache();
                frm.Eta = eta;
                frm.Gefs = gefs;
                frm.Tipo = WaitForm.TipoConjunto.Conjunto;
                frm.Previsoes2Semanas = true;
                frm.SalvarDados = true;

                var chuvasConjunto = frm.ProcessarConjunto();

                var conjPath = System.IO.Path.Combine(searchPath, "CONJUNTO2W00");

                if (!System.IO.Directory.Exists(conjPath)) System.IO.Directory.CreateDirectory(conjPath);

                foreach (var prec in chuvasConjunto)
                {
                    PrecipitacaoFactory.SalvarModeloBin(prec.Value,
                        System.IO.Path.Combine(conjPath,
                        "pp" + data.ToString("yyyyMMdd") + "_" + ((prec.Key - data).TotalHours + 12).ToString("0000")
                        )
                    );
                }
            });
            var funcConjunto = new Action<string, TextWriter>((hora, log) =>
            {
                log.WriteLine("Processando CONJUNTO " + hora + "h");

                var frm = WaitForm2.CreateInstance(data);

                var eta = hora == "00" ? WaitForm2.TipoEta._00h : WaitForm2.TipoEta._12h;
                var gefs = hora == "00" ? WaitForm2.TipoGefs._00h : WaitForm2.TipoGefs._12h;

                frm.LimparCache();
                frm.Eta = eta;
                frm.Gefs = gefs;
                frm.Tipo = WaitForm.TipoConjunto.Conjunto;
                frm.SalvarDados = true;

                var chuvasConjunto = frm.ProcessarConjunto(saveLogFile: System.IO.Path.Combine(searchPath, "conjunto" + hora + ".log"));

                var conjPath = System.IO.Path.Combine(searchPath, "CONJUNTO" + hora);

                if (!System.IO.Directory.Exists(conjPath)) System.IO.Directory.CreateDirectory(conjPath);

                foreach (var prec in chuvasConjunto)
                {

                    var biname = "pp" + data.ToString("yyyyMMdd") + "_" + ((prec.Key - data).TotalHours + (hora == "00" ? 12 : 0)).ToString("0000");
                    log.WriteLine(biname);

                    PrecipitacaoFactory.SalvarModeloBin(prec.Value,
                        System.IO.Path.Combine(conjPath, biname)
                    );
                }

                cptec.ProcessaConjunto(data, hora, log);

                if (hora == "00") funcLogsExtended();

            });



            var funcGEFS = new Action<string, string, TextWriter>((modelo, hora, log) =>
            {
                log.WriteLine("Processando " + modelo + " " + hora + "h");

                //if (modelo == "GEFS") log.WriteLine(cptec.DownloadGEFSNoaa(data, log, modelo, hora));
                //else if (modelo == "GFS") log.WriteLine(cptec.DownloadGFSNoaa(data, log, modelo, hora));
                if (modelo == "GEFS") log.WriteLine(cptec.DownloadNoaaImgs(data, log, modelo, hora));
                else if (modelo == "GFS") log.WriteLine(cptec.DownloadNoaaImgs(data, log, modelo, hora));
                //else if (modelo == "ETA") log.WriteLine(cptec.DownloadETA40(data, log, hora));
                else return;


                if ("00;12".Contains(hora))
                {
                    log.WriteLine("Criando " + modelo.ToLower() + hora + ".log");

                    var frm = WaitForm2.CreateInstance(data);


                    var eta = hora == "00" ? WaitForm2.TipoEta._00h : WaitForm2.TipoEta._12h;
                    var gefs = modelo == "GEFS" ? (hora == "00" ? WaitForm2.TipoGefs._00h : WaitForm2.TipoGefs._12h) : (hora == "00" ? WaitForm2.TipoGefs._ctl_00h : WaitForm2.TipoGefs._ctl_12h);
                    var tipoConj = modelo == "ETA" ? WaitForm.TipoConjunto.Eta40 : WaitForm.TipoConjunto.Gefs;

                    frm.LimparCache();
                    frm.Eta = eta;
                    frm.Gefs = gefs;
                    frm.Tipo = tipoConj;
                    frm.RemoveLimiteETA = false;
                    frm.RemoveViesETA = false;
                    frm.SalvarDados = true;
                    frm.ProcessarConjunto(saveLogFile: System.IO.Path.Combine(searchPath, modelo.ToLower() + hora + ".log"));

                }
            });
            var funcETA = new Action<string, string, TextWriter>((modelo, hora, log) =>
            {
                log.WriteLine("Processando " + modelo + " " + hora + "h");

                //if (modelo == "GEFS") log.WriteLine(cptec.DownloadGEFSNoaa(data, log, modelo, hora));
                //else if (modelo == "GFS") log.WriteLine(cptec.DownloadGFSNoaa(data, log, modelo, hora));
                //else if (modelo == "ETA") log.WriteLine(cptec.DownloadETA40(data, log, hora));
                //else return;
                log.WriteLine(cptec.DownloadETA40_Atual(data, log, hora));


                if ("00;12".Contains(hora))
                {
                    log.WriteLine("Criando " + modelo.ToLower() + hora + ".log");

                    var frm = WaitForm2.CreateInstance(data);


                    var eta = hora == "00" ? WaitForm2.TipoEta._00h : WaitForm2.TipoEta._12h;
                    var gefs = modelo == "GEFS" ? (hora == "00" ? WaitForm2.TipoGefs._00h : WaitForm2.TipoGefs._12h) : (hora == "00" ? WaitForm2.TipoGefs._ctl_00h : WaitForm2.TipoGefs._ctl_12h);
                    var tipoConj = modelo == "ETA" ? WaitForm.TipoConjunto.Eta40 : WaitForm.TipoConjunto.Gefs;

                    frm.LimparCache();
                    frm.Eta = eta;
                    frm.Gefs = gefs;
                    frm.Tipo = tipoConj;
                    frm.RemoveLimiteETA = false;
                    frm.RemoveViesETA = false;
                    frm.SalvarDados = true;
                    frm.ProcessarConjunto(saveLogFile: System.IO.Path.Combine(searchPath, modelo.ToLower() + hora + ".log"));

                }
            });

            var funcECMWF = new Action<string, string, TextWriter>((modelo, hora, log) =>
            {
                if (log != null) log.WriteLine("Processando " + modelo + " " + hora + "h");

                if (hora != "00")
                    data = data.AddHours(int.Parse(hora));

                //if (modelo == "GEFS") log.WriteLine(cptec.DownloadGEFSNoaa(data, log, modelo, hora));
                //else if (modelo == "GFS") log.WriteLine(cptec.DownloadGFSNoaa(data, log, modelo, hora));
                if (modelo == "ECMWF")
                {
                    try
                    {
                        var response = cptec.DownloadMeteologixImgs(data, log, out List<Precipitacao> precs);
                        if (log != null) log.WriteLine(response);
                        foreach (var p in precs)
                        {
                            chuvas[p.Data] = p;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (log != null) log.WriteLine(ex.Message);
                    }
                }
                else return;

            });

            var funcMerge = new Action<TextWriter>((log) =>
            {
                var resp = cptec.ListNewMerge(log);

                if (!resp.Equals("Nada novo", StringComparison.OrdinalIgnoreCase))
                {
                    var config = Config.ConfigConjunto;
                    //var data = dtAtual.Value.Date;
                    var chuvasMerge = new Dictionary<DateTime, Precipitacao>();

                    var localPath = System.IO.Path.GetTempPath() + "MERGE\\";
                    if (!System.IO.Directory.Exists(localPath)) System.IO.Directory.CreateDirectory(localPath);

                    for (DateTime dt = data.AddDays(-9); dt <= data.Date; dt = dt.AddDays(1))
                    {
                        var mergeCtlFile = System.IO.Directory.GetFiles(Path.Combine(Config.CaminhoMerge, dt.ToString("yyyy")), "prec_" + dt.ToString("yyyyMMdd") + ".ctl", System.IO.SearchOption.AllDirectories);

                        if (mergeCtlFile.Length == 1)
                        {
                            var prec = PrecipitacaoFactory.BuildFromMergeFile(mergeCtlFile[0]);
                            prec.Descricao = "MERGE - " + System.IO.Path.GetFileNameWithoutExtension(mergeCtlFile[0]);
                            prec.Data = dt;

                            chuvasMerge[dt] = prec;//.ChangeDefinition(0.4);
                            var fanem = System.IO.Path.Combine(localPath, "merge_" + dt.ToString("yyyyMMdd"));
                            prec.SalvarModeloBin(fanem);
                            Grads.ConvertCtlToImg(fanem, "MERGE", "Precipacao observada entre " + dt.AddDays(-1).ToString("dd/MM") + " e " + dt.ToString("dd/MM"), "merge.gif", System.IO.Path.Combine(Config.CaminhoAuxiliar, "CREATE_GIF.gs"));
                            
                            var directoryToSaveGif = @"C:\Files\Trading\Acompanhamento Metereologico Semanal\spiderman\" + dt.ToString("yyyy_MM_dd") + @"\OBSERVADO";
                            cptec.CopyGifs(localPath, directoryToSaveGif);
                        }
                    }

                    var remo = new PrecipitacaoConjunto(config);
                    var chuvaMEDIA = remo.ConjuntoLivre(chuvasMerge, null);

                    foreach (var c in chuvaMEDIA)
                    {
                        var fanem = System.IO.Path.Combine(localPath, "mergemed_" + c.Key.ToString("yyyyMMdd"));
                        c.Value.SalvarModeloBin(fanem);
                        Grads.ConvertCtlToImg(fanem, "MERGE Medio", "Precipacao observada entre " + c.Key.AddDays(-1).ToString("dd/MM") + " e " + c.Key.ToString("dd/MM"), "mergemed.gif", System.IO.Path.Combine(Config.CaminhoAuxiliar, "CREATE_GIF.gs"));
                        cptec.CopyGifs(localPath, @"C:\Files\Trading\Acompanhamento Metereologico Semanal\spiderman\" + c.Key.ToString("yyyy_MM_dd") + @"\OBSERVADO");
                    }

                    foreach (var pCo in remo.RegioesConjunto.Where(x => x.Modelo == "CONJ"))
                    {
                        for (int i = 0; i < chuvaMEDIA.Keys.Count; i++)
                        {
                            PrecipitacaoRepository.SaveAverage(chuvaMEDIA.Keys.ToArray()[i], pCo.Agrupamento.Nome, pCo.Nome, pCo.precMedia[i], "MERGE");
                        }

                    }

                    remo = new PrecipitacaoConjunto(config);
                    var chuvaMediaBacia = remo.MediaBacias(chuvasMerge);
                    foreach (var pCo in remo.RegioesConjunto.Where(x => x.Modelo == "CONJ").GroupBy(x => x.Agrupamento))
                    {
                        for (int i = 0; i < chuvaMediaBacia.Keys.Count; i++)
                        {
                            PrecipitacaoRepository.SaveAverage(chuvaMediaBacia.Keys.ToArray()[i], pCo.Key.Nome, "", pCo.First().precMedia[i], "MERGE");
                        }
                    }                 

                    System.IO.Directory.Delete(localPath, true);
                }
            });

            switch (btn.Text)
            {
                case "MERGE":
                    this.Acao = (TextWriter log) => { return Task.Factory.StartNew(() => funcMerge(log)); };
                    break;
                case "CONJUNTO 00":
                case "CONJUNTO 12":
                {
                    var h = btn.Text.Split(' ')[1];
                    this.Acao = (TextWriter log) => { return Task.Factory.StartNew(() => { funcConjunto(h, log); /*Logs( cptec.ProcessaConjunto(dateTimePicker1.Value, "00");*/ }); };
                    break;
                }
                case "GEFS 00":
                case "GEFS 06":
                case "GEFS 12":
                case "GFS 00":
                case "GFS 06":
                case "GFS 12":
                {
                    var m = btn.Text.Split(' ')[0];
                    var h = btn.Text.Split(' ')[1];
                    this.Acao = (TextWriter log) => { return Task.Factory.StartNew(() => { funcGEFS(m, h, log); /*Logs( cptec.ProcessaConjunto(dateTimePicker1.Value, "00");*/ }); };
                    break;
                }
                case "ETA 00":
                case "ETA 12":
                {
                    var m = btn.Text.Split(' ')[0];
                    var h = btn.Text.Split(' ')[1];
                    this.Acao = (TextWriter log) => { return Task.Factory.StartNew(() => { funcETA(m, h, log); /*Logs( cptec.ProcessaConjunto(dateTimePicker1.Value, "00");*/ }); };
                    break;
                }
                case "ECMWF 00":
                {
                    var m = btn.Text.Split(' ')[0];
                    var h = btn.Text.Split(' ')[1];

                    //funcECMWF(m, h, null);
                    this.Acao = (TextWriter log) => { return Task.Factory.StartNew(() => { funcECMWF(m, h, log); /*Logs( cptec.ProcessaConjunto(dateTimePicker1.Value, "00");*/ }); };
                    break;
                }
                case "ECMWF 06":
                {
                    var m = btn.Text.Split(' ')[0];
                    var h = btn.Text.Split(' ')[1];

                    //funcECMWF(m, h, null);
                    this.Acao = (TextWriter log) => { return Task.Factory.StartNew(() => { funcECMWF(m, h, log); /*Logs( cptec.ProcessaConjunto(dateTimePicker1.Value, "00");*/ }); };
                    break;
                }
                case "ECMWF 12":
                {
                    var m = btn.Text.Split(' ')[0];
                    var h = btn.Text.Split(' ')[1];

                    //funcECMWF(m, h, null);
                    this.Acao = (TextWriter log) => { return Task.Factory.StartNew(() => { funcECMWF(m, h, log); /*Logs( cptec.ProcessaConjunto(dateTimePicker1.Value, "00");*/ }); };
                    break;
                }
                default:
                {
                    MessageBox.Show("Não configurado");
                    return;
                }
            }
            this.Close();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            LoadStatus(dateTimePicker1.Value);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            var data = dateTimePicker1.Value;
            var searchPath = System.IO.Path.Combine(Config.CaminhoPrevisao, data.ToString("yyyyMM"), data.ToString("dd"));

            var hora = "00";


            if (cptec.DownloadGFSNoaa2(data, null, "GFS", hora) != "CANCELADO")
                if ("00;12".Contains(hora))
                {
                    var frm = WaitForm2.CreateInstance(data);


                    var eta = hora == "00" ? WaitForm2.TipoEta._00h : WaitForm2.TipoEta._12h;
                    var gefs = (hora == "00" ? WaitForm2.TipoGefs._ctl_00h : WaitForm2.TipoGefs._ctl_12h);
                    var tipoConj = WaitForm.TipoConjunto.Gefs;

                    frm.LimparCache();
                    frm.Eta = eta;
                    frm.Gefs = gefs;
                    frm.Tipo = tipoConj;
                    frm.RemoveLimiteETA = false;
                    frm.RemoveViesETA = false;
                    frm.SalvarDados = true;
                    frm.ProcessarConjunto(saveLogFile: System.IO.Path.Combine(searchPath, "gfs" + hora + ".log"));

                }

            hora = "06";

            cptec.DownloadGFSNoaa2(data, null, "GFS", hora);

            hora = "12";

            if (cptec.DownloadGFSNoaa2(data, null, "GFS", hora) != "CANCELADO")
                if ("00;12".Contains(hora))
                {
                    var frm = WaitForm2.CreateInstance(data);


                    var eta = hora == "00" ? WaitForm2.TipoEta._00h : WaitForm2.TipoEta._12h;
                    var gefs = (hora == "00" ? WaitForm2.TipoGefs._ctl_00h : WaitForm2.TipoGefs._ctl_12h);
                    var tipoConj = WaitForm.TipoConjunto.Gefs;

                    frm.LimparCache();
                    frm.Eta = eta;
                    frm.Gefs = gefs;
                    frm.Tipo = tipoConj;
                    frm.RemoveLimiteETA = false;
                    frm.RemoveViesETA = false;
                    frm.SalvarDados = true;
                    frm.ProcessarConjunto(saveLogFile: System.IO.Path.Combine(searchPath, "gfs" + hora + ".log"));

                }
        }
    }
}
