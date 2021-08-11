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
    public interface IPrecipitacaoForm
    {
        bool TodasAsPrevisoes { get; set; }
        bool Previsoes2Semanas { get; set; }


        DateTime DateRemocao { get; set; }

        WaitForm2.TipoEta Eta { get; set; }
        WaitForm2.TipoGefs Gefs { get; set; }
        bool RemoveLimiteETA { get; set; }
        bool RemoveLimiteGEFS { get; set; }
        bool sobrescreverCB { get; set; }
        bool RemoveViesETA { get; set; }
        bool RemoveViesGEFS { get; set; }
        bool SalvarDados { get; set; }
        bool TemEta00 { get; }
        //bool TemEta12 { get; }
        bool TemEuro00 { get; }
        bool TemGefs00 { get; }
        bool TemGefs06 { get; }
        bool TemGefs12 { get; }
        bool TemGfs00 { get; }
        bool TemGfs06 { get; }
        bool TemGfs12 { get; }
        WaitForm.TipoConjunto Tipo { get; set; }

        void LoadData();
        Dictionary<DateTime, Precipitacao> ProcessarConjunto(bool view = false, string saveLogFile = null);

        void LimparCache();
    }

    public partial class WaitForm2 : Form, IPrecipitacaoForm
    {
        public bool TodasAsPrevisoes { get { return radioButton2.Checked; } set { radioButton2.Checked = value; } }
        public bool Previsoes2Semanas { get { return radioButton3.Checked; } set { radioButton3.Checked = value; } }
        public bool sobrescreverCB { get { return sobrescreverCheckBox.Checked; } set { sobrescreverCheckBox.Checked = value; } }
        //public bool TemEta00 { get => temEta00 || temEta; private set => temEta00 = value; }
        public bool TemEta00 { get => EtaDate == Date; }
        //public bool TemEta12 { get => temEta12; private set => temEta12 = value; }

        public bool TemEuro00 { get => temEuro00; private set => temEuro00 = value; }
        public bool TemGefs00 { get => temGefs00; private set => temGefs00 = value; }
        public bool TemGefs06 { get => temGefs06; private set => temGefs06 = value; }
        public bool TemGefs12 { get => temGefs12; private set => temGefs12 = value; }
        public bool TemGfs00 { get => temGfs00; private set => temGfs00 = value; }
        public bool TemGfs06 { get => temGfs06; private set => temGfs06 = value; }
        public bool TemGfs12 { get => temGfs12; private set => temGfs12 = value; }

        bool temEuro00 = true;
        bool temGefs = true;
        bool temGefs00 = true;
        bool temGefs06 = true;
        bool temGefs12 = true;
        bool temGfs00 = true;
        bool temGfs06 = true;
        bool temGfs12 = true;

        Dictionary<DateTime, Precipitacao> chuvasParaTratamentoETA = null;
        Dictionary<DateTime, Precipitacao> chuvasParaTratamentoGEFS = null;

        string searchPath;
        string searchPathETA;

        DateTime _etaDate;
        DateTime EtaDate
        {
            get { return _etaDate; }
            set { _etaDate = value; }
        }

        DateTime _date;
        DateTime Date
        {
            get { return _date; }
            set
            {
                _date = value;
                dtPrevisao.Value = value;
            }
        }

        public DateTime DateRemocao
        {
            get
            {
                return dtRemocao.Value.Date;
            }
            set
            {
                dtRemocao.Value = value;
            }
        }

        Task<WaitForm2> waitTask = null;

        bool open = false;
        internal Dictionary<DateTime, Precipitacao> ChuvaConjunto;

        private WaitForm2()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 0;
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        public void LoadData()
        {

            EtaDate = Date;
            searchPath = System.IO.Path.Combine(Config.CaminhoPrevisao, Date.ToString("yyyyMM"), Date.ToString("dd"));
            searchPathETA = searchPath;
            for (int i = 1; i <= 10; i++)
            {

                var dataPrev = Date.AddDays(i);
                var raiznome = "p" + Date.ToString("ddMMyy") + "a" + dataPrev.ToString("ddMMyy") + ".dat";

                #region Precipitacao ETA

                var eta40 = System.IO.Path.Combine(searchPath, "ETA40_" + raiznome);
                var eta40Prec = System.IO.Path.Combine(searchPath, "Eta40_precipitacao10d.zip");
                var eta00 = System.IO.Path.Combine(searchPath, "ETA00", "pp" + Date.ToString("yyyyMMdd") + "_" + (i * 24 + 12).ToString("0000") + ".ctl");
                var eta12 = System.IO.Path.Combine(searchPath, "ETA12", "pp" + Date.ToString("yyyyMMdd") + "_" + (i * 24).ToString("0000") + ".ctl");

                string raizNomeETA = raiznome;

                if (!File.Exists(eta40) && File.Exists(eta40Prec))
                {
                    using (var zfile = System.IO.Compression.ZipFile.Open(eta40Prec, System.IO.Compression.ZipArchiveMode.Read))
                    {
                        if (zfile.Entries.Any(x => x.Name == "ETA40_" + raizNomeETA && x.Length > 1024))

                            System.IO.Compression.ZipFile.ExtractToDirectory(eta40Prec, searchPath);
                    }
                }

                for (int j = -1; !File.Exists(eta40) && !File.Exists(eta00) && !File.Exists(eta12); j--)
                {

                    var datePassado = Date.AddDays(j);
                    EtaDate = datePassado;

                    searchPathETA = System.IO.Path.Combine(Config.CaminhoPrevisao, datePassado.ToString("yyyyMM"), (datePassado.AddDays(j + 1)).ToString("dd"));

                    raizNomeETA = "p" + datePassado.ToString("ddMMyy") + "a" + EtaDate.ToString("ddMMyy") + ".dat";

                    eta40 = System.IO.Path.Combine(searchPathETA, "ETA40_" + raizNomeETA);
                    eta00 = System.IO.Path.Combine(searchPathETA, "ETA00", "pp" + datePassado.ToString("yyyyMMdd") + "_" + (i * 24 + 12).ToString("0000") + ".ctl");
                    eta12 = System.IO.Path.Combine(searchPathETA, "ETA12", "pp" + datePassado.ToString("yyyyMMdd") + "_" + (i * 24).ToString("0000") + ".ctl");


                }



                #endregion

                if (!File.Exists(System.IO.Path.Combine(searchPath, "GEFS_" + raiznome)) && File.Exists(System.IO.Path.Combine(searchPath, "GEFS_precipitacao14d.zip")))
                {
                    using (var zfile = System.IO.Compression.ZipFile.Open(System.IO.Path.Combine(searchPath, "GEFS_precipitacao14d.zip"), System.IO.Compression.ZipArchiveMode.Read))
                    {
                        if (zfile.Entries.Any(x => x.Name == "GEFS_" + raiznome))
                            System.IO.Compression.ZipFile.ExtractToDirectory(System.IO.Path.Combine(searchPath, "GEFS_precipitacao14d.zip"), searchPath);
                    }
                }

                /*if (!File.Exists(eta40)) temEta = false;
                if (!File.Exists(eta00)) temEta00 = false;
                if (!File.Exists(eta12)) TemEta12 = false;*/
                if (!File.Exists(System.IO.Path.Combine(searchPath, "ECMWF00", "pp" + Date.ToString("yyyyMMdd") + "_" + ((i * 24) + 12).ToString("0000") + ".ctl"))) TemEuro00 = false;
                if (!File.Exists(System.IO.Path.Combine(searchPath, "GEFS_" + raiznome))) temGefs = false;
                if (!File.Exists(System.IO.Path.Combine(searchPath, "GEFS00", "pp" + Date.ToString("yyyyMMdd") + "_" + ((i * 24) + 12).ToString("0000") + ".ctl"))) TemGefs00 = false;
                if (!File.Exists(System.IO.Path.Combine(searchPath, "GEFS06", "pp" + Date.ToString("yyyyMMdd") + "_" + ((i * 24) + 6).ToString("0000") + ".ctl"))) temGefs06 = false;
                if (!File.Exists(System.IO.Path.Combine(searchPath, "GEFS12", "pp" + Date.ToString("yyyyMMdd") + "_" + (i * 24).ToString("0000") + ".ctl"))) TemGefs12 = false;
                if (!File.Exists(System.IO.Path.Combine(searchPath, "GFS00", "pp" + Date.ToString("yyyyMMdd") + "_" + ((i * 24) + 12).ToString("0000") + ".ctl"))) TemGfs00 = false;
                if (!File.Exists(System.IO.Path.Combine(searchPath, "GFS06", "pp" + Date.ToString("yyyyMMdd") + "_" + ((i * 24) + 6).ToString("0000") + ".ctl"))) temGfs06 = false;
                if (!File.Exists(System.IO.Path.Combine(searchPath, "GFS12", "pp" + Date.ToString("yyyyMMdd") + "_" + (i * 24).ToString("0000") + ".ctl"))) TemGfs12 = false;

            }

            // if (EtaDate != Convert.ToDateTime("01 / 01 / 0001 00:00:00"))
            //   EtaDate = EtaDate.AddDays(-10);


            if (EtaDate != Date && EtaDate != Convert.ToDateTime("01 / 01 / 0001 00:00:00"))
            {
                alertaETA.Text = "ETA do dia:" + EtaDate;
            }
            else
                alertaETA.Text = "";

            if (!temGefs && !TemGefs00 && !temGefs06 && !TemGefs12 && !TemGfs00 && !TemGfs06 && !TemGfs12 && !TemEuro00)
            {
                this.DialogResult = DialogResult.Cancel;
                //this.Close();
                btnCriar.Enabled = false;
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;

                return;
            }
            //else if (!temEta && !TemEta00 && !TemEta12 && !TemEuro00) Tipo = ChuvaVazaoTools.WaitForm.TipoConjunto.Gefs;
            else if (!temGefs && !TemGefs00 && !temGefs06 && !TemGefs12 && !TemGfs00 && !TemGfs06 && !TemGfs12) Tipo = ChuvaVazaoTools.WaitForm.TipoConjunto.Eta40;
            else Tipo = ChuvaVazaoTools.WaitForm.TipoConjunto.Conjunto;

            Eta = (TipoEta)(((int)TipoEta._00h ) + (TemEuro00 ? (int)TipoEta._euro_00h : 0));
            Gefs = (TipoGefs)(((temGefs || TemGefs00) ? (int)TipoGefs._00h : 0) + (temGefs06 ? (int)TipoGefs._06h : 0) + (TemGefs12 ? (int)TipoGefs._12h : 0)
                + (TemGfs00 ? (int)TipoGefs._ctl_00h : 0) + (temGfs06 ? (int)TipoGefs._ctl_06h : 0) + (TemGfs12 ? (int)TipoGefs._ctl_12h : 0));
        }

        private void WaitForm_Load(object sender, EventArgs e)
        {
            open = true;
            waitTask = new Task<WaitForm2>(() =>
            {
                while (open) Task.Delay(200).Wait();
                return this;
            });
        }

        public static IPrecipitacaoForm CreateInstance(DateTime date)
        {
            var f = new WaitForm2();

            f.Date = date;

            f.LoadData();
            //f.Show();
            //f.waitTask.Start();

            return f; //.waitTask;
        }

        public static Task<WaitForm2> ShowAsync(DateTime date)
        {
            var f = CreateInstance(date) as WaitForm2;

            f.Show();
            f.waitTask.Start();

            return f.waitTask;
        }


        private void WaitForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            open = false;
        }

        private void btnCriar_Click(object sender, EventArgs e)
        {
            ProcessarConjunto();
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        public Dictionary<DateTime, Precipitacao> ProcessarConjunto(bool view = false, string saveLogFile = null)
        {
            var config = Config.ConfigConjunto;
            var remo = new PrecipitacaoConjunto(config);

            LerChuvasEntrada();
            ////preview and edit ETA40

            ChuvaViewer vwr = null;// = PrevViewer.ShowViewer(new IEnumerable<Precipitacao>[] { chuvasParaTratamentoETA.Values, chuvasParaTratamentoGEFS.Values }, this, caption: "ETA-40 / GEFS", viewrSize: new Size(240, 240));
            //PrevViewer.ShowViewer(new IEnumerable<Precipitacao>[] { chuvasParaTratamentoGEFS.Values }, this, caption: "GEFS", viewrSize: new Size(240, 240));

            Dictionary<DateTime, Precipitacao> chuvaConjunto = null;
            if (Tipo == ChuvaVazaoTools.WaitForm.TipoConjunto.Conjunto)
            {
                var eta2 = remo.Remover("ETA40", chuvasParaTratamentoETA.Where(x => x.Key > DateRemocao).ToDictionary(x => x.Key, x => x.Value), RemoveViesETA, RemoveLimiteETA);
                var gefs2 = remo.Remover("GEFS", chuvasParaTratamentoGEFS.Where(x => x.Key > DateRemocao).ToDictionary(x => x.Key, x => x.Value), RemoveViesGEFS, RemoveLimiteGEFS);

                if (view) vwr = PrevViewer.ShowViewer(new IEnumerable<Precipitacao>[] { eta2.Values, gefs2.Values }, this, caption: "ETA-40 / GEFS", viewrSize: new Size(240, 278));

                chuvaConjunto = remo.Conjunto(eta2, gefs2, Tipo, !this.Previsoes2Semanas);

            }
            else if (Tipo == ChuvaVazaoTools.WaitForm.TipoConjunto.Eta40)
            {
                var eta2 = remo.Remover("ETA40", chuvasParaTratamentoETA.Where(x => x.Key > DateRemocao).ToDictionary(x => x.Key, x => x.Value), RemoveViesETA, RemoveLimiteETA);
                if (view) vwr = PrevViewer.ShowViewer(new IEnumerable<Precipitacao>[] { eta2.Values }, this, caption: "ETA-40", viewrSize: new Size(240, 278));

                chuvaConjunto = remo.Conjunto(eta2, null, Tipo);
            }
            else if (Tipo == ChuvaVazaoTools.WaitForm.TipoConjunto.Gefs)
            {
                var eta2 = remo.Remover("ETA40", chuvasParaTratamentoETA.Where(x => x.Key > DateRemocao).ToDictionary(x => x.Key, x => x.Value), RemoveViesETA, RemoveLimiteETA);
                var gefs2 = remo.Remover("GEFS", chuvasParaTratamentoGEFS.Where(x => x.Key > DateRemocao).ToDictionary(x => x.Key, x => x.Value), RemoveViesGEFS, RemoveLimiteGEFS);
                if (view) vwr = PrevViewer.ShowViewer(new IEnumerable<Precipitacao>[] { gefs2.Values }, this, caption: "GEFS", viewrSize: new Size(240, 278));
                chuvaConjunto = remo.Conjunto(eta2, gefs2, Tipo);
            }
            else
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                this.Close();
                return null;
            }

            this.ChuvaConjunto = chuvaConjunto;

            if (view) vwr.AddView(chuvaConjunto.Values, new Size(240, 278));

            if (DateRemocao > Date && sobrescreverCB)
            {
                for (DateTime dt = Date.AddDays(1); dt <= DateRemocao; dt = dt.AddDays(1))
                {
                    if (chuvasParaTratamentoGEFS != null && chuvasParaTratamentoGEFS.ContainsKey(dt)) this.ChuvaConjunto[dt] = chuvasParaTratamentoGEFS[dt];
                    else if (chuvasParaTratamentoETA != null && chuvasParaTratamentoETA.ContainsKey(dt)) this.ChuvaConjunto[dt] = chuvasParaTratamentoETA[dt];
                }
            }

            if (SalvarDados)
            {
                Ookii.Dialogs.VistaSaveFileDialog svdiag = new Ookii.Dialogs.VistaSaveFileDialog();
                svdiag.FileName = "chuvavazao.log";
                svdiag.OverwritePrompt = true;

                if (!string.IsNullOrWhiteSpace(saveLogFile) || svdiag.ShowDialog() == DialogResult.OK)
                {

                    var dadoslog = new StringBuilder();

                    var header = "Precipitacao média: " +
                        (this.Tipo == WaitForm.TipoConjunto.Conjunto ? "ETA" + this.Eta.ToString() + " GEFS" + this.Gefs.ToString() : (
                        this.Tipo == WaitForm.TipoConjunto.Eta40 ? "ETA" + this.Eta.ToString() : "GEFS" + this.Gefs.ToString()
                         )) +
                         " flags (vies-limite):" + this.RemoveViesETA.ToString() + " " + this.RemoveViesGEFS + " - " + this.RemoveLimiteETA.ToString() + " " + this.RemoveLimiteGEFS.ToString();

                    dadoslog.AppendLine(header);
                    foreach (var pCo in remo.RegioesConjunto.Where(x => x.Modelo == "CONJ"))
                    {
                        dadoslog.Append(pCo.Agrupamento.Nome + "\t" + pCo.Nome + "\t");
                        dadoslog.AppendLine(string.Join("\t", pCo.precMedia));
                    }

                    File.WriteAllText(saveLogFile ?? svdiag.FileName, dadoslog.ToString());
                }
            }

            if (SalvarImagens)
            {
                Ookii.Dialogs.VistaFolderBrowserDialog svdiag = new Ookii.Dialogs.VistaFolderBrowserDialog();
                //svdiag.FileName = "chuvavazao.log";
                //svdiag.OverwritePrompt = true;

                if (!string.IsNullOrWhiteSpace(saveLogFile) || svdiag.ShowDialog() == DialogResult.OK)
                {
                    var caminho = svdiag.SelectedPath;

                    foreach (var prec in this.ChuvaConjunto)
                    {
                        PrecipitacaoFactory.SalvarModeloBin(prec.Value,
                            System.IO.Path.Combine(caminho,
                            "pp" + Date.ToString("yyyyMMdd") + "_" + ((prec.Key - Date).TotalHours).ToString("0000")
                            )
                        );
                    }

                    var header = "Prec: " + (this.Tipo == WaitForm.TipoConjunto.Conjunto ? "ETA" + this.Eta.ToString() + " GEFS" + this.Gefs.ToString() : (
                            this.Tipo == WaitForm.TipoConjunto.Eta40 ? "ETA" + this.Eta.ToString() : "GEFS" + this.Gefs.ToString()
                         ));
                    //+
                    //" flags (vies-limite):" + this.RemoveViesETA.ToString() + " " + this.RemoveViesGEFS + " - " + this.RemoveLimiteETA.ToString() + " " + this.RemoveLimiteGEFS.ToString();

                    cptec.CreateCustomImages(Date, caminho, header);

                }

            }

            return this.ChuvaConjunto;
        }

        public ChuvaVazaoTools.WaitForm.TipoConjunto Tipo
        {
            get
            {
                return (ChuvaVazaoTools.WaitForm.TipoConjunto)comboBox1.SelectedIndex;
            }
            set
            {
                switch (value)
                {
                    case ChuvaVazaoTools.WaitForm.TipoConjunto.Eta40:
                    case ChuvaVazaoTools.WaitForm.TipoConjunto.Gefs:
                        comboBox1.SelectedIndex = (int)value;
                        comboBox1.Enabled = false;
                        break;
                    default:
                        comboBox1.SelectedIndex = (int)value;
                        comboBox1.Enabled = true;
                        break;
                }
            }
        }
        

        public TipoEta Eta
        {
            get
            {
                return (TipoEta)Enum.Parse(typeof(TipoEta), comboBox2.SelectedItem.ToString());
            }
            set
            {

                comboBox2.Items.Clear();

                if (value.HasFlag(TipoEta._00h)) comboBox2.Items.Add(TipoEta._00h.ToString());
                if (value.HasFlag(TipoEta._12h)) comboBox2.Items.Add(TipoEta._12h.ToString());
                if (value.HasFlag(TipoEta._euro_00h)) comboBox2.Items.Add(TipoEta._euro_00h.ToString());

                if (value != 0)
                {
                    comboBox2.SelectedIndex = 0;
                    comboBox2.Enabled = true;
                }
                else
                {
                    comboBox2.Enabled = false;
                }


            }

        }

        public TipoGefs Gefs
        {
            get
            {
                return (TipoGefs)Enum.Parse(typeof(TipoGefs), comboBox3.SelectedItem.ToString());
            }
            set
            {

                comboBox3.Items.Clear();

                if (value.HasFlag(TipoGefs._00h)) comboBox3.Items.Add(TipoGefs._00h.ToString());
                if (value.HasFlag(TipoGefs._06h)) comboBox3.Items.Add(TipoGefs._06h.ToString());
                if (value.HasFlag(TipoGefs._12h)) comboBox3.Items.Add(TipoGefs._12h.ToString());

                if (value.HasFlag(TipoGefs._ctl_00h)) comboBox3.Items.Add(TipoGefs._ctl_00h.ToString());
                if (value.HasFlag(TipoGefs._ctl_06h)) comboBox3.Items.Add(TipoGefs._ctl_06h.ToString());
                if (value.HasFlag(TipoGefs._ctl_12h)) comboBox3.Items.Add(TipoGefs._ctl_12h.ToString());

                if (value != 0)
                {
                    comboBox3.SelectedIndex = 0;
                    comboBox3.Enabled = true;
                }
                else
                {
                    comboBox3.Enabled = false;
                }

            }

        }

        public bool RemoveViesETA { get { return checkBox1.Checked; } set { checkBox1.Checked = value; } }
        public bool RemoveViesGEFS { get { return checkBox2.Checked; } set { checkBox2.Checked = value; } }
        public bool RemoveLimiteETA { get { return checkBox3.Checked; } set { checkBox3.Checked = value; } }
        public bool RemoveLimiteGEFS { get { return checkBox4.Checked; } set { checkBox4.Checked = value; } }

        //public enum TipoConjunto : int
        //{
        //    Conjunto = 0,
        //    Eta40 = 1,
        //    Gefs = 2
        //}

        [Flags]
        public enum TipoEta : int
        {
            _00h = 1,
            _12h = 2,
            _euro_00h = 4,
        }

        [Flags]
        public enum TipoGefs : int
        {
            _00h = 1,
            _06h = 2,
            _12h = 4,
            _ctl_00h = 8,
            _ctl_06h = 16,
            _ctl_12h = 32,
        }

        public bool SalvarDados { get { return chkDados.Checked; } set { chkDados.Checked = value; } }

        public bool SalvarImagens { get { return chkImages.Checked; } set { chkImages.Checked = value; } }

        private void button1_Click(object sender, EventArgs e)
        {
            LerChuvasEntrada();

            PrevViewer.ShowViewer(new IEnumerable<Precipitacao>[] { chuvasParaTratamentoETA.Values, chuvasParaTratamentoGEFS.Values }, this, caption: "ETA-40 / GEFS", viewrSize: new Size(240, 278));
        }

        void LerChuvasEntrada()
        {

            if (chuvasParaTratamentoETA == null)
            {
                chuvasParaTratamentoETA = new Dictionary<DateTime, Precipitacao>();
                chuvasParaTratamentoGEFS = new Dictionary<DateTime, Precipitacao>();

                var iMax = 10 + (DateRemocao - Date).TotalDays;

                if (TodasAsPrevisoes && iMax < 15) iMax = 15;

                if (Previsoes2Semanas)
                {
                    var datafinal = Date;

                    var friCount = 0;

                    do
                    {
                        datafinal = datafinal.AddDays(1);
                        if (datafinal.DayOfWeek == DayOfWeek.Friday) friCount++;
                    } while (friCount < 3);

                    iMax = Math.Max((datafinal - Date).TotalDays, iMax);
                }


                Precipitacao etaMedio = null;
                Precipitacao gefsMedio = null;

                var getetamedio = new Func<Precipitacao>(() =>
                {

                    if (etaMedio != null)
                    {
                        return etaMedio;
                    }
                    else
                    {


                        etaMedio = chuvasParaTratamentoETA.Last().Value.Duplicar();


                        return etaMedio;
                    }
                });

                var getgefsmedio = new Func<Precipitacao>(() =>
                {

                    if (gefsMedio != null)
                    {
                        return gefsMedio;
                    }
                    else
                    {
                        gefsMedio = chuvasParaTratamentoGEFS.Last().Value.Duplicar();

                        return gefsMedio;
                    }
                });

                var funcLePrecEta = new Action<DateTime, string>((dataPrev, fname) =>
                {
                    if (File.Exists(fname))
                    {
                        chuvasParaTratamentoETA[dataPrev] = PrecipitacaoFactory.BuildFromMergeFile(fname);
                        chuvasParaTratamentoETA[dataPrev].Data = dataPrev;
                        chuvasParaTratamentoETA[dataPrev].Descricao = "PREV NUM - " + Path.GetFileNameWithoutExtension(fname);
                    }
                    else
                    {
                        var gefs40Fname = System.IO.Path.Combine(searchPath, "GEFS40_00", "pp" + Date.ToString("yyyyMMdd") + "_" +
           (((dataPrev - Date).TotalDays * 24) + 12).ToString("0000") + ".ctl");
                        if (File.Exists(gefs40Fname))
                        {
                            chuvasParaTratamentoETA[dataPrev] = PrecipitacaoFactory.BuildFromMergeFile(gefs40Fname);
                            chuvasParaTratamentoETA[dataPrev].Data = dataPrev;
                            chuvasParaTratamentoETA[dataPrev].Descricao = "PREV NUM - " + Path.GetFileNameWithoutExtension(fname);
                        }
                        else
                        {
                            var mlt40 = System.IO.Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Observado_MERGE\MCP\040", "prec_mct1318_" + Date.ToString("MM") + ".ctl");
                            chuvasParaTratamentoETA[dataPrev] = PrecipitacaoFactory.BuildFromMergeFile(mlt40);
                            chuvasParaTratamentoETA[dataPrev].Data = dataPrev;
                            chuvasParaTratamentoETA[dataPrev].Descricao = "PREV NUM Media";
                        }
                    }

                });
                var funcLePrecGEFS = new Action<DateTime, string>((dataPrev, fname) =>
                {
                    if (File.Exists(fname))
                    {
                        chuvasParaTratamentoGEFS[dataPrev] = PrecipitacaoFactory.BuildFromMergeFile(fname);
                        chuvasParaTratamentoGEFS[dataPrev].Data = dataPrev;
                        chuvasParaTratamentoGEFS[dataPrev].Descricao = "PREV NUM - " + Path.GetFileNameWithoutExtension(fname);
                    }
                    else
                    {
                        //get mlt
                        var mlt100 = System.IO.Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Observado_MERGE\MCP\100", "prec_mct1318_" + Date.ToString("MM") + ".ctl");
                        chuvasParaTratamentoGEFS[dataPrev] = PrecipitacaoFactory.BuildFromMergeFile(mlt100);
                        chuvasParaTratamentoGEFS[dataPrev].Data = dataPrev;
                        chuvasParaTratamentoGEFS[dataPrev].Descricao = "PREV NUM Media";
                    }

                });

                for (int i = 1; i <= iMax; i++)
                {
                    var dataPrev = Date.AddDays(i);


                    if ((this.Tipo == ChuvaVazaoTools.WaitForm.TipoConjunto.Conjunto || this.Tipo == ChuvaVazaoTools.WaitForm.TipoConjunto.Gefs) && Gefs == TipoGefs._00h)
                    {
                        var raiznome = "GEFS_p" + Date.ToString("ddMMyy") + "a" + dataPrev.ToString("ddMMyy") + ".dat";
                        if (File.Exists(System.IO.Path.Combine(searchPath, raiznome)))
                        {
                            chuvasParaTratamentoGEFS[dataPrev] = PrecipitacaoFactory.BuildFromEtaFile(System.IO.Path.Combine(searchPath, raiznome));
                            chuvasParaTratamentoGEFS[dataPrev].Descricao = "PREV NUM - " + raiznome;
                        }
                        else
                        {
                            funcLePrecGEFS(dataPrev, System.IO.Path.Combine(searchPath, "GEFS00", "pp" + Date.ToString("yyyyMMdd") + "_" + ((i * 24) + 12).ToString("0000") + ".ctl"));
                        }
                    }
                    if ((this.Tipo == ChuvaVazaoTools.WaitForm.TipoConjunto.Conjunto || this.Tipo == ChuvaVazaoTools.WaitForm.TipoConjunto.Gefs) && Gefs == TipoGefs._06h)
                    {
                        var raiznome = "pp" + Date.ToString("yyyyMMdd") + "_" + ((i * 24) + 6).ToString("0000") + ".ctl";
                        funcLePrecGEFS(dataPrev, System.IO.Path.Combine(searchPath, "GEFS06", raiznome));
                    }
                    if ((this.Tipo == ChuvaVazaoTools.WaitForm.TipoConjunto.Conjunto || this.Tipo == ChuvaVazaoTools.WaitForm.TipoConjunto.Gefs) && Gefs == TipoGefs._12h)
                    {
                        var raiznome = "pp" + Date.ToString("yyyyMMdd") + "_" + (i * 24).ToString("0000") + ".ctl";
                        funcLePrecGEFS(dataPrev, System.IO.Path.Combine(searchPath, "GEFS12", raiznome));
                    }

                    if ((this.Tipo == ChuvaVazaoTools.WaitForm.TipoConjunto.Conjunto || this.Tipo == ChuvaVazaoTools.WaitForm.TipoConjunto.Gefs) && Gefs == TipoGefs._ctl_00h)
                    {
                        var raiznome = "pp" + Date.ToString("yyyyMMdd") + "_" + ((i * 24) + 12).ToString("0000") + ".ctl";
                        funcLePrecGEFS(dataPrev, System.IO.Path.Combine(searchPath, "GFS00", raiznome));

                    }
                    if ((this.Tipo == ChuvaVazaoTools.WaitForm.TipoConjunto.Conjunto || this.Tipo == ChuvaVazaoTools.WaitForm.TipoConjunto.Gefs) && Gefs == TipoGefs._ctl_06h)
                    {
                        var raiznome = "pp" + Date.ToString("yyyyMMdd") + "_" + ((i * 24) + 6).ToString("0000") + ".ctl";
                        funcLePrecGEFS(dataPrev, System.IO.Path.Combine(searchPath, "GFS06", raiznome));
                    }
                    if ((this.Tipo == ChuvaVazaoTools.WaitForm.TipoConjunto.Conjunto || this.Tipo == ChuvaVazaoTools.WaitForm.TipoConjunto.Gefs) && Gefs == TipoGefs._ctl_12h)
                    {
                        var raiznome = "pp" + Date.ToString("yyyyMMdd") + "_" + (i * 24).ToString("0000") + ".ctl";
                        funcLePrecGEFS(dataPrev, System.IO.Path.Combine(searchPath, "GFS12", raiznome));
                    }
                }

                var gap = (Date - EtaDate).Days;

                for (int i = 1 + gap; i <= iMax + gap; i++)
                {
                    var dataPrev = EtaDate.AddDays(i);
                    if (((this.Tipo == ChuvaVazaoTools.WaitForm.TipoConjunto.Conjunto || this.Tipo == ChuvaVazaoTools.WaitForm.TipoConjunto.Eta40) && Eta == TipoEta._00h) || this.Tipo == WaitForm.TipoConjunto.Gefs)
                    {
                        var raiznome = "ETA40_p" + EtaDate.ToString("ddMMyy") + "a" + dataPrev.ToString("ddMMyy") + ".dat";

                        if (File.Exists(System.IO.Path.Combine(searchPathETA, raiznome)))
                        {
                            chuvasParaTratamentoETA[dataPrev] = PrecipitacaoFactory.BuildFromEtaFile(System.IO.Path.Combine(searchPathETA, raiznome));
                            chuvasParaTratamentoETA[dataPrev].Descricao = "PREV NUM - " + raiznome;
                        }
                        else
                        {
                            funcLePrecEta(dataPrev, System.IO.Path.Combine(searchPathETA, "ETA00", "pp" + EtaDate.ToString("yyyyMMdd") + "_" + (i * 24 + 12).ToString("0000") + ".ctl"));
                        }
                    }
                    else if ((this.Tipo == ChuvaVazaoTools.WaitForm.TipoConjunto.Conjunto || this.Tipo == ChuvaVazaoTools.WaitForm.TipoConjunto.Eta40) && Eta == TipoEta._12h)
                    {
                        var raiznome = "pp" + EtaDate.ToString("yyyyMMdd") + "_" + (i * 24).ToString("0000") + ".ctl";
                        funcLePrecEta(dataPrev, System.IO.Path.Combine(searchPathETA, "ETA12", raiznome));
                    }
                    else if ((this.Tipo == ChuvaVazaoTools.WaitForm.TipoConjunto.Conjunto || this.Tipo == ChuvaVazaoTools.WaitForm.TipoConjunto.Eta40) && Eta == TipoEta._euro_00h)
                    {
                        var raiznome = "pp" + EtaDate.ToString("yyyyMMdd") + "_" + (i * 24 + 12).ToString("0000") + ".ctl";
                        funcLePrecEta(dataPrev, System.IO.Path.Combine(searchPath, "ECMWF00", raiznome));
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            LimparCache();
        }

        public void LimparCache()
        {
            //EtaDate = new DateTime();
            //alertaETA.Text = "";
            chuvasParaTratamentoETA = null;
            chuvasParaTratamentoGEFS = null;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ProcessarConjunto(true);
        }

        private void chkDados_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void dtPrevisao_ValueChanged(object sender, EventArgs e)
        {
            //EtaDate = new DateTime();
            this._date = dtPrevisao.Value.Date;
            dtRemocao.Value = this._date;
            LimparCache();
            this.LoadData();
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            LimparCache();
        }

        private void dtRemocao_ValueChanged(object sender, EventArgs e)
        {
            LimparCache();
        }
    }
}
