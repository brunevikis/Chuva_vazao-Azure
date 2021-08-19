using Excel = Microsoft.Office.Interop.Excel;
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
using System.Threading;
using ChuvaVazaoTools.Classes;
using System.Runtime.Serialization.Json;
using ConvertMERGE;
using System.Runtime.InteropServices;

namespace ChuvaVazaoTools
{
    public partial class FrmMain : Form
    {
        private bool Busy
        {
            set
            {
                panel3.Enabled = panel1.Enabled = panel2.Enabled = btnConsultarVazObserv.Enabled = panel4.Enabled = panel5.Enabled = panel6.Enabled = !value;


                this.Cursor = value ? Cursors.WaitCursor : Cursors.Default;
            }
        }

        public string ArquivoPrevsBase { get { return this.txtPrevs.Text; } set { this.txtPrevs.Text = value; } }

        public string ArquivosDeEntradaPrevivaz { get { return this.txtPrevivaz.Text; } set { this.txtPrevivaz.Text = value; } }

        public string ArquivosDeEntradaModelo { get { return this.txtEntrada.Text; } set { this.txtEntrada.Text = value; } }

        public string ArquivosDeSaida { get { return this.txtCaminho.Text; } set { this.txtCaminho.Text = value; } }

        public DateTime? DataSemanaPrevsBase { get; private set; }

        public List<ModeloChuvaVazao> modelosChVz = new List<ModeloChuvaVazao>();
        public Dictionary<DateTime, Precipitacao> chuvas = new Dictionary<DateTime, Precipitacao>();

        bool runAuto = false;
        TextBoxLogger textLogger = null;


        #region Public Methods

        public FrmMain(bool run, bool verifica = false) : this()
        {
            runAuto = run;
            cbx_Encadear_Previvaz.Checked = verifica;
        }

        public FrmMain()
        {
            InitializeComponent();

            this.Text += " - " + GetRunningVersion().ToString();

            dtAtual.Value = DateTime.Today.Date;
        }

        public void Ler(Boolean manual = false)
        {
            try
            {
                var path = txtCaminho.Text;

                if (!System.IO.Directory.Exists(path))
                {
                    MessageBox.Show("Caminho não existente");
                    return;
                }

                modelosChVz.Clear();

                var modelos = System.IO.Directory.GetDirectories(path);

                foreach (var modelo in modelos)
                {

                    var nomeModelo = modelo.Replace(System.IO.Path.GetDirectoryName(modelo), "").Remove(0, 1);

                    if (nomeModelo.StartsWith("SMAP", StringComparison.OrdinalIgnoreCase))
                    {
                        var bacias = System.IO.Directory.GetDirectories(modelo);
                        foreach (var bacia in bacias)
                        {

                            modelosChVz.Add(new ChuvaVazaoTools.SMAP.ModeloSmap(bacia, manual));

                        }
                    }

                    AddLog("\t" + modelo);
                }


                modelosChVz.ForEach(x => x.ColetarSaida());

                listView1.Items.Clear();

                listView1.Items.AddRange(modelosChVz.Select(x => new ModeloItemView(x)).ToArray());

                dtModelo.Value = modelosChVz.Min(x => x.DataPrevisao);

                AddLog("- Modelos Carregados");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                AddLog("\t" + "Erro no método FrmMain/Ler: " + e.Message);
            }
        }

        private void btnLer_Click(object sender, EventArgs e)
        {
            Ler();
        }

        public class ModeloItemView : ListViewItem
        {

            public ModeloItemView(ModeloChuvaVazao x)
            {
                this.Text = x.GetType().Name + " - " + System.IO.Path.GetDirectoryName(x.Caminho);
            }
        }

        public class PrecipitacaoItemView : ListViewItem
        {

            Precipitacao prec = null;

            public Precipitacao Prec { get { return prec; } }

            public String Descricao { get; set; }
            public DateTime DataChuva { get; set; }


            public PrecipitacaoItemView(Precipitacao prec)
                : base(new string[] { prec.Data.ToString("dd/MM/yyyy"), prec.Descricao })
            {
                this.prec = prec;
            }
        }

        public void CopiarResultados()
        {
            modelosChVz.ForEach(x => x.ColetarSaida());

            var vaz = modelosChVz.SelectMany(x => x.Vazoes).ToList();
            var minData = vaz.Min(x => x.Vazoes.Keys.Min());
            var maxData = vaz.Max(x => x.Vazoes.Keys.Max());

            int rows = (int)(maxData - minData).TotalDays + 1;
            int cols = vaz.Count();

            object[,] results = new object[rows + 1, cols + 1];

            for (int i = 0; i < cols; i++)
            {
                results[0, i + 1] = vaz[i].CaminhoArquivo;
            }

            for (int d = 0; d < rows; d++)
            {
                var dt = minData.AddDays(d);
                results[d + 1, 0] = dt;
                for (int i = 0; i < cols; i++)
                {
                    if (vaz[i].Vazoes.ContainsKey(dt))
                    {
                        results[d + 1, i + 1] = vaz[i].Vazoes[dt];
                    }
                }
            }
            string s = "";

            for (int c = 0; c < results.GetLength(1); c++)
            {
                for (int r = 0; r < results.GetLength(0); r++)
                {
                    if (results[r, c] != null) s += results[r, c].ToString();
                    s += "\t";
                }
                s += Environment.NewLine;
            }

            System.Windows.Forms.Clipboard.SetText(s, TextDataFormat.Text);
            MessageBox.Show("Salvo na área de transferência");
        }

        public enum EnumRemo
        {
            RemocaoAtual,
            RemocaoUmaSemana,
            RemocaoUmaSemanaEuro,
            RemocaoUmaSemanaEuro_op,
            RemocaoUmaSemanaGFS,
            RemocaoDuasSemanasEuro,
            RemocaoDuasSemanasEuro_op,
            RemocaoDuasSemanasGEFS,
            RemocaoDuasSemanasGFS,
            RemocaoDuasSemanasGFS2x,
            RemocaoTresSemanasEuro,
            RemocaoTresSemanasGEFS,
            RemocaoQuatroSemanasEuro,
            RemocaoQuatroSemanasGEFS,
        };

        public void EuroSem(IPrecipitacaoForm frm)
        {
            #region EUROPEU SEM
            frm.LimparCache();
            frm.Eta = WaitForm2.TipoEta._euro_00h;
            //frm.Gefs = WaitForm2.TipoGefs._00h;
            frm.Tipo = WaitForm.TipoConjunto.Eta40;
            frm.RemoveViesETA = false;
            frm.RemoveLimiteETA = false;
            frm.SalvarDados = false;

            var chuvasConjunto = frm.ProcessarConjunto();
            foreach (var c in chuvasConjunto)
            {
                chuvas[c.Key] = c.Value;
            }

            #endregion

            #region GEFS COM
            frm.LimparCache();
            frm.Eta = WaitForm2.TipoEta._00h;
            frm.Gefs = WaitForm2.TipoGefs._00h;
            frm.Tipo = WaitForm.TipoConjunto.Conjunto;

            frm.RemoveViesGEFS = true;
            frm.RemoveLimiteGEFS = true;
            frm.RemoveViesETA = true;
            frm.RemoveLimiteETA = true;

            frm.sobrescreverCB = false;
            frm.SalvarDados = false;

            var dtRemocao = dtAtual.Value;

            if (dtRemocao.DayOfWeek == DayOfWeek.Thursday)
                dtRemocao = dtRemocao.AddDays(+7);
            else
            {
                while (dtRemocao.DayOfWeek != DayOfWeek.Thursday)
                    dtRemocao = dtRemocao.AddDays(+1);
            }

            frm.DateRemocao = dtRemocao;

            chuvasConjunto = frm.ProcessarConjunto();
            foreach (var c in chuvasConjunto)
            {
                chuvas[c.Key] = c.Value;
            }

            RefreshPrecipList();

            #endregion
        }

        public void EuroSemGefsCom(IPrecipitacaoForm frm)
        {
            #region EUROPEU SEM
            frm.LimparCache();
            frm.Eta = WaitForm2.TipoEta._euro_00h;
            //frm.Gefs = WaitForm2.TipoGefs._00h;
            frm.Tipo = WaitForm.TipoConjunto.Eta40;
            frm.RemoveViesETA = false;
            frm.RemoveLimiteETA = false;
            frm.SalvarDados = false;
            frm.Previsoes2Semanas = true;

            var chuvasConjunto = frm.ProcessarConjunto();
            foreach (var c in chuvasConjunto)
            {
                chuvas[c.Key] = c.Value;
            }

            #endregion

            #region GEFS COM
            frm.LimparCache();
            //frm.Eta = WaitForm2.TipoEta._euro_00h;
            frm.Gefs = WaitForm2.TipoGefs._00h;
            frm.Tipo = WaitForm.TipoConjunto.Gefs;
            frm.RemoveViesGEFS = true;
            frm.RemoveLimiteGEFS = true;
            frm.sobrescreverCB = false;
            frm.SalvarDados = false;

            var dtRemocao = dtAtual.Value;

            if (dtRemocao.DayOfWeek == DayOfWeek.Thursday)
                dtRemocao = dtRemocao.AddDays(+7);
            else
            {
                while (dtRemocao.DayOfWeek != DayOfWeek.Thursday)
                    dtRemocao = dtRemocao.AddDays(+1);

                dtRemocao = dtRemocao.AddDays(+7);
            }


            frm.DateRemocao = dtRemocao;


            chuvasConjunto = frm.ProcessarConjunto();
            foreach (var c in chuvasConjunto)
            {
                chuvas[c.Key] = c.Value;
            }

            RefreshPrecipList();

            #endregion
        }

        public void GfsComGfsCom(IPrecipitacaoForm frm)
        {
            #region GFS COM
            frm.LimparCache();
            frm.Gefs = WaitForm2.TipoGefs._ctl_00h;
            frm.Tipo = WaitForm.TipoConjunto.Gefs;
            frm.RemoveViesGEFS = true;
            frm.RemoveLimiteGEFS = true;
            frm.SalvarDados = false;
            frm.DateRemocao = dtAtual.Value;
            frm.Previsoes2Semanas = true;

            var chuvasConjunto = frm.ProcessarConjunto();
            foreach (var c in chuvasConjunto)
            {
                chuvas[c.Key] = c.Value;
            }
            #endregion

            #region GFS COM sem sobrescrever

            frm.LimparCache();
            frm.Gefs = WaitForm2.TipoGefs._ctl_00h;
            frm.Tipo = WaitForm.TipoConjunto.Gefs;
            frm.RemoveViesGEFS = true;
            frm.RemoveLimiteGEFS = true;
            frm.sobrescreverCB = false;
            frm.SalvarDados = false;

            var dtRemocao = dtAtual.Value;

            if (dtRemocao.DayOfWeek == DayOfWeek.Thursday)
                dtRemocao = dtRemocao.AddDays(+7);
            else
            {
                while (dtRemocao.DayOfWeek != DayOfWeek.Thursday)
                    dtRemocao = dtRemocao.AddDays(+1);

                dtRemocao = dtRemocao.AddDays(+7);
            }

            frm.DateRemocao = dtRemocao;
            chuvasConjunto = frm.ProcessarConjunto();

            foreach (var c in chuvasConjunto)
            {
                chuvas[c.Key] = c.Value;
            }

            RefreshPrecipList();

            #endregion
        }

        public void GfsSemGefsCom(IPrecipitacaoForm frm)
        {
            #region GFS COM
            frm.LimparCache();
            //frm.Eta = WaitForm2.TipoEta._euro_00h;
            frm.Gefs = WaitForm2.TipoGefs._ctl_00h;
            frm.Tipo = WaitForm.TipoConjunto.Gefs;
            frm.RemoveViesGEFS = false;
            frm.RemoveLimiteGEFS = false;
            frm.SalvarDados = false;
            frm.DateRemocao = dtAtual.Value;
            frm.Previsoes2Semanas = true;

            var chuvasConjunto = frm.ProcessarConjunto();
            foreach (var c in chuvasConjunto)
            {
                chuvas[c.Key] = c.Value;
            }
            #endregion

            #region GEFS COM
            frm.LimparCache();
            //frm.Eta = WaitForm2.TipoEta._euro_00h;
            frm.Gefs = WaitForm2.TipoGefs._00h;
            frm.Tipo = WaitForm.TipoConjunto.Gefs;
            frm.RemoveViesGEFS = true;
            frm.RemoveLimiteGEFS = true;
            frm.sobrescreverCB = false;
            frm.SalvarDados = false;

            var dtRemocao = dtAtual.Value;

            if (dtRemocao.DayOfWeek == DayOfWeek.Thursday)
                dtRemocao = dtRemocao.AddDays(+7);
            else
            {
                while (dtRemocao.DayOfWeek != DayOfWeek.Thursday)
                    dtRemocao = dtRemocao.AddDays(+1);

                dtRemocao = dtRemocao.AddDays(+7);
            }


            frm.DateRemocao = dtRemocao;


            chuvasConjunto = frm.ProcessarConjunto();
            foreach (var c in chuvasConjunto)
            {
                chuvas[c.Key] = c.Value;
            }

            RefreshPrecipList();

            #endregion

        }
        public void GefsSemGefsCom(IPrecipitacaoForm frm)
        {
            #region GeFS COM
            frm.LimparCache();
            //frm.Eta = WaitForm2.TipoEta._euro_00h;
            frm.Gefs = WaitForm2.TipoGefs._00h;
            frm.Tipo = WaitForm.TipoConjunto.Gefs;
            frm.RemoveViesGEFS = false;
            frm.RemoveLimiteGEFS = false;
            frm.SalvarDados = false;
            frm.DateRemocao = dtAtual.Value;
            frm.Previsoes2Semanas = true;

            var chuvasConjunto = frm.ProcessarConjunto();
            foreach (var c in chuvasConjunto)
            {
                chuvas[c.Key] = c.Value;
            }
            #endregion

            #region GEFS COM
            frm.LimparCache();
            //frm.Eta = WaitForm2.TipoEta._euro_00h;
            frm.Gefs = WaitForm2.TipoGefs._00h;
            frm.Tipo = WaitForm.TipoConjunto.Gefs;
            frm.RemoveViesGEFS = true;
            frm.RemoveLimiteGEFS = true;
            frm.sobrescreverCB = false;
            frm.SalvarDados = false;

            var dtRemocao = dtAtual.Value;

            if (dtRemocao.DayOfWeek == DayOfWeek.Thursday)
                dtRemocao = dtRemocao.AddDays(+7);
            else
            {
                while (dtRemocao.DayOfWeek != DayOfWeek.Thursday)
                    dtRemocao = dtRemocao.AddDays(+1);

                dtRemocao = dtRemocao.AddDays(+7);
            }

            frm.DateRemocao = dtRemocao;

            chuvasConjunto = frm.ProcessarConjunto();
            foreach (var c in chuvasConjunto)
            {
                chuvas[c.Key] = c.Value;
            }

            RefreshPrecipList();

            #endregion

        }

        //runStatusFile : { caso copiado, pronto para execucao, executado, resultado coletado, previvaz pronto, finalizado } int[6]
        class RunStatus
        {
            internal enum statuscode : int
            {
                nonInitialized = 0,
                initialialized = 1,
                completed = 2,
                error = 3,
            }

            string filePath = "";

            internal RunStatus(string folder)
            {
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                filePath = Path.Combine(folder, "status.log");

                if (File.Exists(filePath))
                {
                    statuses = File.ReadAllText(filePath).Split(' ').Select(x => int.Parse(x)).ToArray();
                }
                else
                {
                    statuses = new int[] { 0, 0, 0, 0, 0, 0 };
                    Save();
                }
            }
            void Save()
            {
                File.WriteAllText(filePath, string.Join(" ", statuses));
            }

            int[] statuses;

            internal statuscode Creation { get { return (statuscode)statuses[0]; } set { statuses[0] = (int)value; Save(); } }
            internal statuscode Preparation { get { return (statuscode)statuses[1]; } set { statuses[1] = (int)value; Save(); } }
            internal statuscode Execution { get { return (statuscode)statuses[2]; } set { statuses[2] = (int)value; Save(); } }
            internal statuscode Collect { get { return (statuscode)statuses[3]; } set { statuses[3] = (int)value; Save(); } }
            internal statuscode Previvaz { get { return (statuscode)statuses[4]; } set { statuses[4] = (int)value; Save(); } }
            internal statuscode PostProcessing { get { return (statuscode)statuses[5]; } set { statuses[5] = (int)value; Save(); } }
        }

        public void RunExecProcess(System.IO.TextWriter logF, out string runId, EnumRemo offset = EnumRemo.RemocaoAtual)
        {
            dtAtual.Value = DateTime.Today.Date;
            runId = null;

            DateTime datModel;
            string horaPrev = "";
            var name = "CPM_CV"; //Computational Processing Model
            var currRev = ChuvaVazaoTools.Tools.Tools.GetCurrRev(dtAtual.Value);


            //Verifica a RV da rodada
            var runRev = ChuvaVazaoTools.Tools.Tools.GetNextRev(dtAtual.Value,
                (offset == EnumRemo.RemocaoDuasSemanasEuro || offset == EnumRemo.RemocaoDuasSemanasEuro_op || offset == EnumRemo.RemocaoDuasSemanasGEFS || offset == EnumRemo.RemocaoDuasSemanasGFS || offset == EnumRemo.RemocaoDuasSemanasGFS2x) ? 2 : 1
                );


            if (logF != null) logF.WriteLine("INICIANDO RODADA AUTOMÁTICA");

            IPrecipitacaoForm frm = null;
            frm = WaitForm2.CreateInstance(dtAtual.Value);

            try
            {
                if (frm.TemEta00 && frm.TemGefs00)
                {
                    AddLog("CONJUNTO 00");
                }
                else if (frm.TemGefs00)
                {
                    AddLog("GEFS 00");
                    horaPrev = "_GEFS";
                }
                //else
                //{
                //    throw new FileLoadException("Previões para o dia não encontradas - ENCERRANDO");
                //}
            }
            catch
            {
                if (logF != null) logF.WriteLine("Previões para o dia não encontradas - ENCERRANDO");
                AddLog("Previões para o dia não encontradas");
                return;
            }

            name = name + horaPrev;
            var runRevMapas = ChuvaVazaoTools.Tools.Tools.GetNextRev(dtAtual.Value);

            //var pastaMapa = @"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\16_Chuva_Vazao\" + runRevMapas.revDate.ToString("yyyy_MM") + @"\RV" + runRevMapas.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\Mapas Acomph"; //Mapas Acomph
            // var pastaRaiz = @"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\16_Chuva_Vazao\" + runRevMapas.revDate.ToString("yyyy_MM") + @"\RV" + runRevMapas.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\Mapas Acomph"; // "Mapas Acomph";
            var pastaMapa = @"C:\Files\Middle - Preço\16_Chuva_Vazao\" + runRevMapas.revDate.ToString("yyyy_MM") + @"\RV" + runRevMapas.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\Mapas Acomph"; //Mapas Acomph
            var pastaRaiz = @"C:\Files\Middle - Preço\16_Chuva_Vazao\" + runRevMapas.revDate.ToString("yyyy_MM") + @"\RV" + runRevMapas.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\Mapas Acomph"; // "Mapas Acomph";

            try
            {
                PreencherVazObservada(out DateTime dataModelo, out string fonteVaz);

                CarregarPrecRealMedia(dtAtual.Value.Date, out string modeloPrecReal);

                //modelosChVz

                if (modeloPrecReal.EndsWith("-1"))
                {
                    AddLog("Chuva realizada do dia ainda não está disponível");
                    return;
                }

                name = name + "_" + modeloPrecReal;

                if (dataModelo < dtAtual.Value.Date.AddDays(-1))
                {
                    name = name + "_d-1";
                    pastaRaiz = Path.Combine(pastaMapa + " d-1", "CV", "CV_FUNC");
                    pastaMapa = (pastaMapa + " d-1");
                }
                else
                {
                    pastaRaiz = Path.Combine(pastaMapa,"CV", "CV_FUNC");
                }
                switch (offset)
                {
                    case EnumRemo.RemocaoUmaSemana:
                        name = name + "_VIES_VE";
                        pastaRaiz = Path.Combine(pastaMapa, "CV", "CV_VIES_VE");
                        break;
                    case EnumRemo.RemocaoUmaSemanaEuro:
                        name = name + "_EURO";
                        pastaRaiz = Path.Combine(pastaMapa, "CV", "CV_EURO");
                        break;
                    case EnumRemo.RemocaoUmaSemanaEuro_op:
                        name = name + "_EUROop";
                        pastaRaiz = Path.Combine(pastaMapa, "CV", "CV_EUROop");
                        break;
                    case EnumRemo.RemocaoUmaSemanaGFS:
                        name = name + "_GFS";
                        pastaRaiz = Path.Combine(pastaMapa, "CV", "CV_GFS");
                        break;
                    case EnumRemo.RemocaoDuasSemanasEuro:
                        name = name + "_EURO";
                        // name = name.Replace("_" + modeloPrecReal, "").Replace("CV_", "CV2_");
                        name = name.Replace("CV_", "CV2_");
                        pastaRaiz = Path.Combine(pastaMapa, "CV2", "CV2_EURO");
                        break;
                    case EnumRemo.RemocaoDuasSemanasEuro_op:
                        name = name + "_EUROop";
                        // name = name.Replace("_" + modeloPrecReal, "").Replace("CV_", "CV2_");
                        name = name.Replace("CV_", "CV2_");
                        pastaRaiz = Path.Combine(pastaMapa, "CV2", "CV2_EUROop");
                        break;
                    case EnumRemo.RemocaoDuasSemanasGEFS:
                        name = name + "_GEFS";
                        //name = name.Replace("_" + modeloPrecReal, "").Replace("CV_", "CV2_");
                        name = name.Replace("CV_", "CV2_");
                        pastaRaiz = Path.Combine(pastaMapa, "CV2", "CV2_GEFS");
                        break;
                    case EnumRemo.RemocaoDuasSemanasGFS:
                        name = name + "_GFS";
                        //name = name.Replace("_" + modeloPrecReal, "").Replace("CV_", "CV2_");
                        name = name.Replace("CV_", "CV2_");
                        pastaRaiz = Path.Combine(pastaMapa, "CV2", "CV2_GFS");
                        break;

                }
                datModel = dataModelo;
            }
            catch
            {
                logF.WriteLine("Possivel falha no PreencherVazObservada e CarregarPrecRealMedia");
                return;
            }

            ArquivosDeSaida = @"C:\Files\Middle - Preço\16_Chuva_Vazao\" + runRev.revDate.ToString("yyyy_MM") + @"\RV" + runRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\" + name;
            var pastaBase = @"C:\Files\Middle - Preço\Acompanhamento de vazões\" + currRev.revDate.ToString("MM_yyyy") + @"\Dados_de_Entrada_e_Saida_" + currRev.revDate.ToString("yyyyMM") + "_RV" + currRev.rev.ToString();

            var statusF = new RunStatus(ArquivosDeSaida);

            if (System.IO.Directory.Exists(ArquivosDeSaida) && System.IO.File.Exists(Path.Combine(ArquivosDeSaida, "resumoENA.gif")))
            {
                AddLog("Caso já executado para essa data: " + name);
                if (logF != null) logF.WriteLine("Caso já executado para essa data: " + name);
                runId = "OK - " + name;

                return;
            }

            if (statusF.Creation == RunStatus.statuscode.initialialized
                || statusF.Previvaz == RunStatus.statuscode.initialialized
                || statusF.PostProcessing == RunStatus.statuscode.initialialized
                || statusF.Preparation == RunStatus.statuscode.initialialized
                || statusF.Execution == RunStatus.statuscode.initialialized
                )
            {
                AddLog("Caso em execução: " + name);
                if (logF != null) logF.WriteLine("Caso em execução: " + name);
                return;
            }
            if ((System.IO.Directory.Exists(ArquivosDeSaida) &&
                statusF.PostProcessing == RunStatus.statuscode.completed))
            {
                AddLog("Caso já executado para essa data: " + name);
                if (logF != null) logF.WriteLine("Caso já executado para essa data: " + name);

                runId = "OK - " + name;
                return;
            }
            runId = name;

            if (logF != null) logF.WriteLine("INICIANDO RODADA: " + name);

            if (!Directory.Exists(pastaBase) || !(Directory.Exists(System.IO.Path.Combine(pastaBase, "Modelos_Chuva_Vazao")) && Directory.Exists(System.IO.Path.Combine(pastaBase, "Previvaz", "Arq_Entrada")) && System.IO.Directory.GetFiles(pastaBase, "prevs.*", SearchOption.AllDirectories).Length > 0))
            {
                if (logF != null) logF.WriteLine("Arquivos de entrada nao disponiveis");
                return;
            }

            this.ArquivosDeEntradaModelo = System.IO.Path.Combine(pastaBase, "Modelos_Chuva_Vazao_Shadow");//trocar na quinta dia 26-08 Modelos_Chuva_Vazao
            this.ArquivosDeEntradaPrevivaz = System.IO.Path.Combine(pastaBase, "Previvaz", "Arq_Entrada");
            this.ArquivoPrevsBase = System.IO.Directory.GetFiles(pastaBase, "prevs.*", SearchOption.AllDirectories)[0];
            this.DataSemanaPrevsBase = currRev.revDate;

            if (!System.IO.Directory.Exists(ArquivosDeSaida) || statusF.Creation != RunStatus.statuscode.completed)
            {
                try
                {
                    CriarCaso(statusF);
                    string user = System.Environment.UserName.ToString();
                    Tools.Tools.addHistory(Path.Combine(ArquivosDeSaida, user + ".txt"), System.Environment.UserName.ToString());
                }
                catch
                {
                    logF.WriteLine("Falha na movimentação de pastas do Smap");
                    return;
                }
            }
            if (!System.IO.File.Exists(Path.Combine(ArquivosDeSaida, "chuvamedia.log")) || statusF.Preparation != RunStatus.statuscode.completed)
            {
                statusF.Preparation = RunStatus.statuscode.initialialized;

                Ler();

                CarregarPrecObserv();
                PreencherPrecObserv();

                //btnConsultarVazObserv_Click(sender, e);
                PreencherVazObservada(out _, out _);


                dtAtual.Value = datModel.AddDays(1);
                dtModelo.Value = dtAtual.Value.Date;
                Reiniciar(dtModelo.Value);
                PrecipitacaoPrevista_R(pastaRaiz, ArquivosDeSaida);

                AddLog(" --- ");
                AddLog(" --- Executar Parte B quando pronto --- ");

                PreencherPrecObserv();
                //btnSalvarPrecObserv_Click(null, null);
                SalvarPrecObserv_R();
                SalvarVazObserv();
                SalvarPrecPrev_R();

                statusF.Preparation = RunStatus.statuscode.completed;
            }

            if (statusF.Preparation == RunStatus.statuscode.completed && statusF.Creation == RunStatus.statuscode.completed && statusF.Execution != RunStatus.statuscode.completed)
            {
                statusF.Execution = RunStatus.statuscode.initialialized; //TODO: criar um status para o metodo automatico do executingProcess

                if (modelosChVz.Count == 0)
                    Ler();
                ExecutarTudo(statusF);
                if (File.Exists(Path.Combine(txtCaminho.Text, "error.log")))
                {
                    statusF.Execution = RunStatus.statuscode.error;
                    return;
                }
                if (statusF.Execution == RunStatus.statuscode.error)
                {
                    // Directory.Delete(ArquivosDeSaida, true);
                    logF.WriteLine("Erro no SMAP");
                    return;
                }

            }

            #region Propagacoes sem Excell
            try
            {
                var check = cbx_Encadear_Previvaz.Checked;

                List<Propagacao> propagacoes = null;
                if (statusF.Preparation == RunStatus.statuscode.completed && statusF.Creation == RunStatus.statuscode.completed && statusF.Previvaz != RunStatus.statuscode.completed)
                {
                    statusF.Collect = RunStatus.statuscode.initialialized;
                    if (modelosChVz.Count == 0)
                        Ler();
                    propagacoes = new ExecutingProcess().ProcessResultsPart1(modelosChVz, ArquivosDeSaida, dtAtual.Value);
                    if (propagacoes.Count != 0 || propagacoes != null)
                    {
                        statusF.Execution = RunStatus.statuscode.completed;
                        statusF.Collect = RunStatus.statuscode.completed;
                    }


                    if (propagacoes.Count != 0 || propagacoes != null)
                    {
                        MemoryStream stream1 = new MemoryStream();
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<Propagacao>));

                        ser.WriteObject(stream1, propagacoes);
                        stream1.Position = 0;





                        if (statusF.Execution == RunStatus.statuscode.completed && statusF.Collect == RunStatus.statuscode.completed)
                        {
                            File.WriteAllText(Path.Combine(ArquivosDeSaida, "Propagacoes_Automaticas.txt"), new StreamReader(stream1).ReadToEnd());

                            statusF.Previvaz = RunStatus.statuscode.initialialized;

                            var p = Program.GetPrevivazExPath(Path.Combine(ArquivosDeSaida, "Propagacoes_Automaticas.txt"));

                            if (p != null)
                            {
                                var encad = cbx_Encadear_Previvaz.Checked;
                                AddLog("EXECUCAO PREVIVAZ");
                                if (logF != null) logF.WriteLine("EXECUCAO PREVIVAZ");
                                if (encad)
                                {
                                    var parametro = p.Item2 + "|true";
                                    var pre = System.Diagnostics.Process.Start(p.Item1, parametro);
                                    pre.WaitForExit();
                                }
                                else
                                {
                                    var pr = System.Diagnostics.Process.Start(p.Item1, p.Item2);

                                    pr.WaitForExit();
                                }


                                try
                                {
                                    if (System.IO.File.Exists(Path.Combine(ArquivosDeSaida, "Previvaz2.txt")))
                                    {
                                        // var procId = pr.BasePriority;

                                        if (statusF != null) statusF.Previvaz = RunStatus.statuscode.completed;
                                    }
                                    else
                                    {
                                        statusF.Previvaz = RunStatus.statuscode.error;
                                        return;
                                    }

                                }
                                catch (Exception e)
                                {
                                    e.ToString();
                                    statusF.Previvaz = RunStatus.statuscode.error;
                                    return;
                                }
                                if (statusF?.Previvaz != RunStatus.statuscode.completed)
                                {
                                    statusF.Previvaz = RunStatus.statuscode.error;
                                    return;
                                }

                            }
                            else
                            {
                                if (statusF != null && System.IO.Directory.Exists(Path.Combine(ArquivosDeSaida, "Propagacoes_Automaticas.txt"))) statusF.Previvaz = RunStatus.statuscode.error;
                                return;
                            }
                        }
                    }
                    else
                    {
                        statusF.Execution = RunStatus.statuscode.error;
                        statusF.Collect = RunStatus.statuscode.error;
                        //throw new Exception("As propagações foram enviadas ao método e retornaram vazias ou com erro");
                        return;
                    }
                }


                if (statusF.Creation == RunStatus.statuscode.completed &&
                    statusF.Execution == RunStatus.statuscode.completed &&
                    statusF.Preparation == RunStatus.statuscode.completed &&
                    statusF.Previvaz == RunStatus.statuscode.completed &&
                    statusF.PostProcessing != RunStatus.statuscode.completed
                    )
                {
                    if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.initialialized;

                    if (System.IO.File.Exists(Path.Combine(ArquivosDeSaida, "Previvaz2.txt")))
                    {
                        var Read = System.IO.File.ReadAllText(Path.Combine(ArquivosDeSaida, "Previvaz2.txt"));
                        //testeRead.ReadToEnd();

                        DataContractJsonSerializer desser = new DataContractJsonSerializer(typeof(List<Propagacao>));
                        MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(Read));
                        propagacoes = ((List<Propagacao>)desser.ReadObject(ms)).ToList();

                        var prevs = ExportaPrevs(propagacoes, ArquivosDeSaida, dtAtual.Value, runRev.revDate, runRev.rev);
                        ExportaEnas(propagacoes, ArquivosDeSaida);
                        if (prevs != "")
                        {
                            try
                            {
                                var nomeDoCaso = ArquivosDeSaida.Split('\\').Last();
                                if (nomeDoCaso.StartsWith("CPM_CV_") || nomeDoCaso.StartsWith("CPM_CV2_"))
                                {
                                    var pathDestino = Path.Combine("Z:\\cpas_ctl_common", "auto", DateTime.Today.ToString("yyyyMMdd") + "_" + nomeDoCaso);
                                    if (!System.IO.Directory.Exists(pathDestino))
                                    {
                                        Directory.CreateDirectory(pathDestino);
                                        File.Copy(Path.Combine(ArquivosDeSaida, prevs), Path.Combine(pathDestino, prevs));
                                    }

                                }
                                else
                                {
                                    if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.error;
                                    if (statusF != null) statusF.Previvaz = RunStatus.statuscode.error;
                                    return;
                                }
                            }
                            catch
                            {
                                if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.error;
                                return;
                            }

                        }
                        else
                        {
                            if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.error;
                            return;
                        }

                        if (!File.Exists(Path.Combine(ArquivosDeSaida, "enasemanal.log")) || !File.Exists(Path.Combine(ArquivosDeSaida, "enadiaria.log")))
                        {
                            if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.error;
                            return;
                        }
                        else
                        {
                            //Copia resultados para Storage AZURE

                            string[] arqs_copy = { "Para_STR.txt", "VazoesSemanais.log", "VazoesDiarias.log", "enadiaria.log", "enasemanal.log" };

                            var path_Z = ArquivosDeSaida.Replace("C:\\Files\\Middle - Preço\\16_Chuva_Vazao", "Z:\\16_Chuva_Vazao");
                            foreach (var arq in arqs_copy)
                            {
                                if (File.Exists(Path.Combine(ArquivosDeSaida, arq)))
                                {
                                    if (!Directory.Exists(path_Z))
                                    {
                                        Directory.CreateDirectory(path_Z);
                                    }
                                    if (logF != null) logF.WriteLine("Copiando Arquivo " + arq);
                                    File.Copy(Path.Combine(ArquivosDeSaida, arq), Path.Combine(path_Z, arq), true);
                                }

                            }
                        }
                    }
                    else
                    {
                        if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.error;
                        if (statusF != null) statusF.Previvaz = RunStatus.statuscode.error;
                        return;
                    }

                    try
                    {
                        Salvar_Img(ArquivosDeSaida);
                        if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.completed;

                    }
                    catch (Exception ex)
                    {
                        statusF.PostProcessing = RunStatus.statuscode.error;
                        return;
                    }

                    //var email = Tools.Tools.SendMail(Path.Combine(ArquivosDeSaida, "Propagacoes_Automaticas.txt"), "Sucesso ao executar as propagações automáticas!", "Propagações sem Excell [AUTO]", "desenv");
                    //email.Wait();
                }
            }
            catch (Exception exce)
            {
                statusF.Execution = RunStatus.statuscode.error;
                var email = Tools.Tools.SendMail("", "ERRO: " + exce.Message, "Erro nas propagações sem Excell [AUTO]", "desenv");
                email.Wait();
            }
            #endregion

        }

        static List<Regressao> RegressoesA1 = new List<Regressao>();
        static List<Regressao> RegressoesA0 = new List<Regressao>();
        static List<PostoRegre> PostoRegredidos = new List<PostoRegre>();
        public static void CalcularPostRegre(List<Propagacao> Propagacoes, List<DateTime> dias)
        {


            try
            {
                List<int> idRegre = new List<int> {002, 007, 008, 009, 010, 011, 012, 015, 016, 022, 251, 206, 207, 028, 023, 032, 248, 261,
                                           241, 118, 301, 320, 048, 049, 249, 050, 052, 051, 062, 089, 217, 094, 103, 076, 072,
                                            078, 222, 081, 252, 110, 112, 113, 114, 097, 284, 303, 123, 129, 202, 306, 203, 122, 129,
                                            198, 263, 141, 148, 183, 191, 253, 273, 155,
                                            285, 227, 228, 230, 204, 297, 55};//id de postos regredidos

                var regresA1 = System.IO.File.ReadLines("C:\\Sistemas\\ChuvaVazao\\RegressoesA1.txt").ToList();
                foreach (string l in regresA1)
                {
                    Regressao reg = new Regressao();
                    reg.IdPosto = Convert.ToInt32(l.Split(' ').First());
                    var list = l.Split(' ').ToList();

                    reg.Valor_mensal = list.Select(x => double.Parse(x)).ToList();

                    RegressoesA1.Add(reg);
                }

                var regresA0 = System.IO.File.ReadLines("C:\\Sistemas\\ChuvaVazao\\RegressoesA0.txt").ToList();
                foreach (string l in regresA0)
                {
                    Regressao regB = new Regressao();
                    regB.IdPosto = Convert.ToInt32(l.Split(' ').First());
                    var list = l.Split(' ').ToList();

                    regB.Valor_mensal = list.Select(x => double.Parse(x)).ToList();

                    RegressoesA0.Add(regB);
                }
                var listRegre = System.IO.File.ReadLines("C:\\Sistemas\\ChuvaVazao\\Posto_regre_base.txt").ToList();
                foreach (var l in listRegre)
                {
                    PostoRegre post = new PostoRegre();
                    post.Idposto_Regredido = Convert.ToInt32(l.Split(' ').First());
                    post.IdPosto_Base = Convert.ToInt32(l.Split(' ').Last());
                    PostoRegredidos.Add(post);
                }
            }
            catch (Exception e)
            {
                e.ToString();
            }
            //List<DateTime> listDat = Propagacoes.First().medSemanalNatural.Select(x => x.Key).ToList();
            List<DateTime> listDat = dias;

            foreach (var p in Propagacoes)
            {
                try
                {
                    foreach (var regred in PostoRegredidos)
                    {
                        if (regred.Idposto_Regredido == p.IdPosto)
                        {
                            try
                            {
                                foreach (var d in listDat)
                                {
                                    double[] fatorReg = ValorRegrecao(p.IdPosto, d);
                                    Calcular_Regressao(Propagacoes, p.IdPosto, fatorReg, d);
                                }
                            }
                            catch (Exception e)
                            {
                                e.ToString();
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    e.ToString();
                }
            }


        }

        public static void Calcular_Regressao(List<Propagacao> Propagacoes, int idPosto, double[] fator, DateTime dat)
        {

            double diaRegre = 0;
            foreach (var l in PostoRegredidos.Where(x => x.Idposto_Regredido == idPosto))
            {
                var propBase = Propagacoes.Where(v => v.IdPosto == l.IdPosto_Base).First();

                if (propBase.VazaoNatural.ContainsKey(dat))
                {
                    diaRegre = fator[0] + (fator[1] * propBase.VazaoNatural[dat]);  // os postos regredidos utilizam uma função linear p\ calcular as vazões (Y=A0+A1*X) 
                                                                                    //onde A0 e A1 são fatores e X é a vazão do posto base 
                                                                                    //obs: os dados de A0 A1 e quais são os postos regredidos e base, são disponibilizados pelo ONS     

                    var propRegre = Propagacoes.Where(i => i.IdPosto == l.Idposto_Regredido).First();
                    //if (!propRegre.VazaoNatural.ContainsKey(dat))//usa a regressão para as semanas que ainda não possuem dados
                    //{
                    propRegre.VazaoNatural[dat] = diaRegre;

                    break;
                    //}
                }
            }
        }

        public static double[] ValorRegrecao(int id, DateTime data)
        {
            double[] fat = new double[2];
            foreach (var rb in RegressoesA0)
            {
                if (rb.IdPosto == id)
                {
                    double valor = 0;
                    valor = rb.Valor_mensal[data.Month];
                    fat[0] = valor;
                    break;
                }
            }

            foreach (var r in RegressoesA1)
            {
                if (r.IdPosto == id)
                {
                    double valor = 0;
                    valor = r.Valor_mensal[data.Month];

                    fat[1] = valor;
                    break;
                }
            }
            return fat;
        }

        static Dictionary<int, Tuple<int[], int>> postosIncrementais = new Dictionary<int, Tuple<int[], int>>(){ // <num posto, { postos montantes ... }, num posto inc >
                    {34, new Tuple<int[], int>( new int[] {18, 33, 99, 241, 261}, 135)}, //i solteira
                    {243,new Tuple<int[], int>( new int[] {242}, 138)}, // 3 irmaos
                    {245,new Tuple<int[], int>( new int[] {34, 243}, 136)}, // jupia
                    {246,new Tuple<int[], int>( new int[] {245, 154}, 137)}, // p primaveira
                    {266,new Tuple<int[], int>( new int[] {63, 246}, 166 )}, // itaipu
                    {253,new Tuple<int[], int>( new int[] {191}, 308 )},// sao salvador
                    {273,new Tuple<int[], int>( new int[] {257}, 309 )},// lajeado
                    {271,new Tuple<int[], int>( new int[] {273}, 310 )},// estreito
                    {275,new Tuple<int[], int>( new int[] {271}, 311 )},// tucurui
                    //{257,new Tuple<int[], int>( new int[] {253}, 308 )},// peixe angical
                    // { 169, new Tuple<int[], int>( new int[] {156, 158}, 168 )}, // sobradinho
                    // { 239, new int[] {237 } },
                    // { 242, new int[] {239 } },
                };

        public static double GetMediaSemanal(List<Propagacao> Propagacoes, int codigoPost, DateTime data)
        {
            try
            {
                double valor = 0;
                var prop = Propagacoes.Where(x => x.IdPosto == codigoPost).First();
                if (prop.VazaoNatural.ContainsKey(data))
                {
                    valor = prop.VazaoNatural[data];
                }
                else
                {
                    valor = 0;
                }

                return valor;
            }
            catch (Exception e)

            {
                e.ToString();
            }
            return 0;
        }

        public static void CalcularPostCalculados(List<Propagacao> Propagacoes, List<DateTime> dias)
        {
            var verifica = Propagacoes;
            List<DateTime> listSem = dias;
            /*foreach (var pn in postosIncrementais)//soma as vazões dos postos  montantes e incremnetais dos postos que possuem postos incrementais
            {
                var prop = Propagacoes.Where(x => x.IdPosto == pn.Key).FirstOrDefault();
                var propInc = Propagacoes.Where(x => x.IdPosto == pn.Value.Item2).FirstOrDefault();
                for (int i = 0; i < 12; i++)
                {
                    if (!prop.calMedSemanal.ContainsKey(SemanasPrevs[i].Item1))
                    {
                        if (pn.Key == 253)// sao salvador + canabrava
                        {
                            var propCana = Propagacoes.Where(x => x.IdPosto == 191).First();
                            var propSerra = Propagacoes.Where(x => x.IdPosto == 270).First();
                            double fatcana = 0.504;
                            propCana.calMedSemanal[SemanasPrevs[i].Item1] = (propInc.calMedSemanal[SemanasPrevs[i].Item1] * fatcana) + (propSerra.calMedSemanal[SemanasPrevs[i].Item1]);
                            double fatSal = 0.496;
                            prop.calMedSemanal[SemanasPrevs[i].Item1] = (propInc.calMedSemanal[SemanasPrevs[i].Item1] * fatSal) + (propCana.calMedSemanal[SemanasPrevs[i].Item1]);

                        }
                        else if (pn.Key == 273)//lajeado + peixe angical
                        {
                            var propPeixe = Propagacoes.Where(x => x.IdPosto == 257).First();
                            var propSSal = Propagacoes.Where(x => x.IdPosto == 253).First();

                            double fatpeixe = 0.488;
                            propPeixe.calMedSemanal[SemanasPrevs[i].Item1] = (propInc.calMedSemanal[SemanasPrevs[i].Item1] * fatpeixe) + (propSSal.calMedSemanal[SemanasPrevs[i].Item1]);
                            double fatlaj = 0.512;
                            prop.calMedSemanal[SemanasPrevs[i].Item1] = (propInc.calMedSemanal[SemanasPrevs[i].Item1] * fatlaj) + (propPeixe.calMedSemanal[SemanasPrevs[i].Item1]);

                        }
                        else
                        {
                            double vazMontantes = 0;
                            foreach (var p in prop.PostoMontantes)
                            {
                                var propa = Propagacoes.Where(x => x.IdPosto == p.Propaga.IdPosto).FirstOrDefault();
                                vazMontantes += propa.calMedSemanal[SemanasPrevs[i].Item1];
                            }
                            // vazMontantes = prop.PostoMontantes.SelectMany(x => x.Propaga.calMedSemanal).Where(x => x.Key == SemanasPrevs[i].Item1).Select(x => x.Value).ToList();
                            var v = propInc.calMedSemanal[SemanasPrevs[i].Item1] + vazMontantes;

                            prop.calMedSemanal[SemanasPrevs[i].Item1] = v;
                        }

                    }
                }
            }*/


            foreach (var d in listSem)
            {

                foreach (var p in Propagacoes.Where(x => x.IdPosto == 240))
                {
                    var p239 = GetMediaSemanal(Propagacoes, 239, d);
                    var p242 = GetMediaSemanal(Propagacoes, 242, d);


                    p.VazaoNatural[d] = (p242 - p239) * 0.717 + p239;

                    if (p.VazaoNatural[d] <= 0)
                    {
                        p.VazaoNatural[d] = 0;
                    }
                    else if (p.VazaoNatural[d] <= 1)
                    {
                        p.VazaoNatural[d] = 1;
                    }



                }

                foreach (var p in Propagacoes.Where(x => x.IdPosto == 154))
                {
                    var p246 = GetMediaSemanal(Propagacoes, 246, d);
                    var p245 = GetMediaSemanal(Propagacoes, 245, d);


                    p.VazaoNatural[d] = (p246 - p245) * 0.152;

                    if (p.VazaoNatural[d] <= 0)
                    {
                        p.VazaoNatural[d] = 0;
                    }
                    else if (p.VazaoNatural[d] <= 1)
                    {
                        p.VazaoNatural[d] = 1;
                    }



                }

                foreach (var p in Propagacoes.Where(x => x.IdPosto == 238))
                {
                    var p239 = GetMediaSemanal(Propagacoes, 239, d);
                    var p237 = GetMediaSemanal(Propagacoes, 237, d);


                    p.VazaoNatural[d] = (p239 - p237) * 0.342 + p237;
                    if (p.VazaoNatural[d] <= 0)
                    {
                        p.VazaoNatural[d] = 0;
                    }
                    else if (p.VazaoNatural[d] <= 1)
                    {
                        p.VazaoNatural[d] = 1;
                    }


                }

                foreach (var p in Propagacoes.Where(x => x.IdPosto == 244))
                {
                    var p34 = GetMediaSemanal(Propagacoes, 34, d);
                    var p243 = GetMediaSemanal(Propagacoes, 243, d);
                    // if (p.VazaoNatural.ContainsKey(d))
                    // {
                    p.VazaoNatural[d] = p34 + p243;
                    //}

                }

                foreach (var p in Propagacoes.Where(x => x.IdPosto == 317))
                {
                    var p201 = GetMediaSemanal(Propagacoes, 201, d);
                    var p201b = p201 - 25;
                    if (p201b > 0)
                    {
                        p.VazaoNatural[d] = p201b;
                    }
                    else
                    {
                        p.VazaoNatural[d] = 0;
                    }
                }

                foreach (var p in Propagacoes.Where(x => x.IdPosto == 298))
                {
                    var p125 = GetMediaSemanal(Propagacoes, 125, d);
                    if (p125 <= 190)
                    {
                        p.VazaoNatural[d] = (p125 * 119) / 190;
                    }
                    else if (p125 <= 209 && p125 > 190)
                    {
                        p.VazaoNatural[d] = 119;
                    }
                    else if (p125 <= 250 && p125 > 209)
                    {
                        p.VazaoNatural[d] = p125 - 90;
                    }
                    else
                    {
                        p.VazaoNatural[d] = 160;
                    }
                }

                foreach (var p in Propagacoes.Where(x => x.IdPosto == 315))
                {
                    var p203 = GetMediaSemanal(Propagacoes, 203, d);
                    var p201 = GetMediaSemanal(Propagacoes, 201, d);
                    var p317 = GetMediaSemanal(Propagacoes, 317, d);
                    var p298 = GetMediaSemanal(Propagacoes, 298, d);
                    p.VazaoNatural[d] = (p203 - p201) + p317 + p298;
                    if (p.VazaoNatural[d] <= 0)
                    {
                        p.VazaoNatural[d] = 0;
                    }
                    else if (p.VazaoNatural[d] <= 1)
                    {
                        p.VazaoNatural[d] = 1;
                    }
                }

                foreach (var p in Propagacoes.Where(x => x.IdPosto == 316))
                {
                    var p315 = GetMediaSemanal(Propagacoes, 315, d);
                    if (p315 < 190)
                    {
                        p.VazaoNatural[d] = p315;
                    }
                    else
                    {
                        p.VazaoNatural[d] = 190;
                    }
                }

                foreach (var p in Propagacoes.Where(x => x.IdPosto == 304))
                {
                    var p315 = GetMediaSemanal(Propagacoes, 315, d);
                    var p316 = GetMediaSemanal(Propagacoes, 316, d);

                    p.VazaoNatural[d] = p315 - p316;
                    if (p.VazaoNatural[d] <= 0)
                    {
                        p.VazaoNatural[d] = 0;
                    }
                    else if (p.VazaoNatural[d] <= 1)
                    {
                        p.VazaoNatural[d] = 1;
                    }
                }

                foreach (var p in Propagacoes.Where(x => x.IdPosto == 127))
                {
                    var p129 = GetMediaSemanal(Propagacoes, 129, d);
                    var p298 = GetMediaSemanal(Propagacoes, 298, d);
                    var p203 = GetMediaSemanal(Propagacoes, 203, d);
                    var p304 = GetMediaSemanal(Propagacoes, 304, d);

                    p.VazaoNatural[d] = p129 - p298 - p203 - p304;
                    if (p.VazaoNatural[d] <= 0)
                    {
                        p.VazaoNatural[d] = 0;
                    }
                    else if (p.VazaoNatural[d] <= 1)
                    {
                        p.VazaoNatural[d] = 1;
                    }
                }

                foreach (var p in Propagacoes.Where(x => x.IdPosto == 126))
                {
                    var p127 = GetMediaSemanal(Propagacoes, 127, d);
                    var p127b = p127 - 90;
                    if (p127 <= 430)
                    {
                        if (p127b > 0)
                        {
                            p.VazaoNatural[d] = p127b;
                        }
                        else
                        {
                            p.VazaoNatural[d] = 0;
                        }
                    }
                    else
                    {
                        p.VazaoNatural[d] = 340;
                    }
                }

                foreach (var p in Propagacoes.Where(x => x.IdPosto == 118))
                {
                    if (p.VazaoNatural[d] > 1)
                    {
                        continue;
                    }
                    else
                    {
                        var prop = Propagacoes.Where(x => x.IdPosto == 119).First();
                        if (prop.VazaoNatural[d] > 1)
                        {
                            var valor = (prop.VazaoNatural[d] * 0.8103) + 0.185;
                            if (valor > 0)
                            {
                                p.VazaoNatural[d] = valor;
                            }
                            else
                                p.VazaoNatural[d] = 0;

                        }
                    }
                }

                foreach (var p in Propagacoes)
                {


                    if (p.IdPosto == 244)
                    {
                        var p34 = GetMediaSemanal(Propagacoes, 34, d);
                        var p243 = GetMediaSemanal(Propagacoes, 243, d);

                        p.VazaoNatural[d] = p34 + p243;
                    }

                    //else if (p.IdPosto == 21)
                    //{
                    //    var p123 = GetMediaSemanal(Propagacoes, 123, d);
                    //    p.VazaoNatural[d] = p123;
                    //}

                    else if (p.IdPosto == 292)//BELO MONTE PIMNETAL uso do trecho de vazão reduzida (TVR)
                    {
                        var TVR = System.IO.File.ReadLines("C:\\Sistemas\\ChuvaVazao\\TVR_BeloMonte.txt").ToList();
                        var TVRmeses = TVR[0].Split(' ').Skip(1).ToList();
                        var p288 = GetMediaSemanal(Propagacoes, 288, d);

                        double mediaTVR = 0;

                        mediaTVR = Convert.ToDouble(TVRmeses[d.Month - 1]);

                        if (p288 <= mediaTVR)
                        {
                            p.VazaoNatural[d] = 0;
                        }
                        else if (p288 <= (mediaTVR + 13900))//13900 = valor retirado do arquivo REGRAS.DAT do GEVAZP
                        {
                            p.VazaoNatural[d] = p288 - mediaTVR;
                        }
                        else
                        {
                            p.VazaoNatural[d] = 13900;
                        }
                    }

                    else if (p.IdPosto == 293)
                    {

                        var p288 = GetMediaSemanal(Propagacoes, 288, d);
                        var p292 = GetMediaSemanal(Propagacoes, 292, d);


                        p.VazaoNatural[d] = (1.07 * p288) - p292;
                        if (p.VazaoNatural[d] <= 0)
                        {
                            p.VazaoNatural[d] = 0;
                        }
                        else if (p.VazaoNatural[d] <= 1)
                        {
                            p.VazaoNatural[d] = 1;
                        }
                    }

                    else if (p.IdPosto == 299)
                    {
                        var p130 = GetMediaSemanal(Propagacoes, 130, d);
                        var p298 = GetMediaSemanal(Propagacoes, 298, d);
                        var p203 = GetMediaSemanal(Propagacoes, 203, d);
                        var p304 = GetMediaSemanal(Propagacoes, 304, d);

                        p.VazaoNatural[d] = p130 - p298 - p203 + p304;
                        if (p.VazaoNatural[d] <= 0)
                        {
                            p.VazaoNatural[d] = 0;
                        }
                        else if (p.VazaoNatural[d] <= 1)
                        {
                            p.VazaoNatural[d] = 1;
                        }
                    }

                    //else if (p.IdPosto == 169)
                    //{
                    //    var d2 = d.AddDays(-14);
                    //    var p168 = GetMediaSemanal(168, d);
                    //    var p156 = GetMediaSemanal(156, d2);
                    //    var p158 = GetMediaSemanal(158, d2);
                    //    p.calMedSemanal[d] = p168 + p156 + p158;
                    //    if (p.calMedSemanal[d] <= 0)
                    //    {
                    //        p.calMedSemanal[d] = 0;
                    //    }
                    //    else if (p.calMedSemanal[d] <= 1)
                    //    {
                    //        p.calMedSemanal[d] = 1;
                    //    }
                    //}



                    else if (p.IdPosto == 301)
                    {
                        var p118 = GetMediaSemanal(Propagacoes, 118, d);
                        p.VazaoNatural[d] = p118;
                    }

                    else if (p.IdPosto == 302)
                    {
                        var p288 = GetMediaSemanal(Propagacoes, 288, d);
                        var p292 = GetMediaSemanal(Propagacoes, 292, d);
                        p.VazaoNatural[d] = p288 - p292;
                        if (p.VazaoNatural[d] <= 0)
                        {
                            p.VazaoNatural[d] = 0;
                        }
                        else if (p.VazaoNatural[d] <= 1)
                        {
                            p.VazaoNatural[d] = 1;
                        }
                    }

                    else if (p.IdPosto == 252)
                    {
                        var p259 = GetMediaSemanal(Propagacoes, 259, d);
                        p.VazaoNatural[d] = p259;
                    }

                    else if (p.IdPosto == 172)
                    {
                        var p169 = GetMediaSemanal(Propagacoes, 169, d);


                        p.VazaoNatural[d] = p169;

                    }

                    else if (p.IdPosto == 173)
                    {
                        var p172 = GetMediaSemanal(Propagacoes, 172, d);
                        p.VazaoNatural[d] = p172;
                    }

                    else if (p.IdPosto == 175)
                    {
                        var p172 = GetMediaSemanal(Propagacoes, 172, d);
                        if (d < DateTime.Today)
                        {
                            p.VazaoNatural[d] = 0;

                        }
                        else
                        {
                            p.VazaoNatural[d] = p172;
                        }
                    }

                    else if (p.IdPosto == 178)
                    {
                        var p172 = GetMediaSemanal(Propagacoes, 172, d);


                        p.VazaoNatural[d] = p172;

                    }

                    else if (p.IdPosto == 176)
                    {
                        var p173 = GetMediaSemanal(Propagacoes, 173, d);
                        p.VazaoNatural[d] = p173;
                    }

                    else if (p.IdPosto == 164)
                    {
                        var p161 = GetMediaSemanal(Propagacoes, 161, d);
                        var p117 = GetMediaSemanal(Propagacoes, 117, d);
                        var p118 = GetMediaSemanal(Propagacoes, 118, d);

                        p.VazaoNatural[d] = p161 - p117 - p118;
                        if (p.VazaoNatural[d] <= 0)
                        {
                            p.VazaoNatural[d] = 0;
                        }
                        else if (p.VazaoNatural[d] <= 1)
                        {
                            p.VazaoNatural[d] = 1;
                        }
                    }

                    else if (p.IdPosto == 314)
                    {
                        var p199 = GetMediaSemanal(Propagacoes, 199, d);
                        var p298 = GetMediaSemanal(Propagacoes, 298, d);
                        var p203 = GetMediaSemanal(Propagacoes, 203, d);
                        var p304 = GetMediaSemanal(Propagacoes, 304, d);

                        p.VazaoNatural[d] = p199 - p298 - p203 + p304;
                        if (p.VazaoNatural[d] <= 0)
                        {
                            p.VazaoNatural[d] = 0;
                        }
                        else if (p.VazaoNatural[d] <= 1)
                        {
                            p.VazaoNatural[d] = 1;
                        }

                    }

                    else if (p.IdPosto == 104)
                    {
                        var p117 = GetMediaSemanal(Propagacoes, 117, d);
                        var p118 = GetMediaSemanal(Propagacoes, 118, d);
                        p.VazaoNatural[d] = p117 + p118;
                    }

                    else if (p.IdPosto == 132)
                    {
                        var p202 = GetMediaSemanal(Propagacoes, 202, d);
                        var p201 = GetMediaSemanal(Propagacoes, 201, d);

                        if (p201 < 25)
                        {
                            p.VazaoNatural[d] = p202 + p201;
                        }
                        else
                        {
                            p.VazaoNatural[d] = p202 + 25;
                        }
                    }



                    else if (p.IdPosto == 131)
                    {
                        var p316 = GetMediaSemanal(Propagacoes, 316, d);

                        if (p316 < 144)
                        {
                            p.VazaoNatural[d] = p316;
                        }
                        else
                        {
                            p.VazaoNatural[d] = 144;
                        }
                    }

                    else if (p.IdPosto == 303)
                    {
                        var p132 = GetMediaSemanal(Propagacoes, 132, d);
                        var p316 = GetMediaSemanal(Propagacoes, 316, d);
                        var p131 = GetMediaSemanal(Propagacoes, 131, d);
                        var aux = p316 - p131;
                        if (p132 < 17)
                        {
                            if (aux < 34)
                            {
                                p.VazaoNatural[d] = p132 + aux;
                            }
                            else
                            {
                                p.VazaoNatural[d] = p132 + 34;
                            }
                        }
                        else
                        {
                            if (aux < 34)
                            {
                                p.VazaoNatural[d] = 17 + aux;
                            }
                            else
                            {
                                p.VazaoNatural[d] = 17 + 34;
                            }
                        }
                    }

                    else if (p.IdPosto == 306)
                    {
                        var p303 = GetMediaSemanal(Propagacoes, 303, d);
                        var p131 = GetMediaSemanal(Propagacoes, 131, d);

                        p.VazaoNatural[d] = p303 + p131;
                    }

                    else if (p.IdPosto == 109)
                    {
                        var p118 = GetMediaSemanal(Propagacoes, 118, d);
                        p.VazaoNatural[d] = p118;
                    }

                    else if (p.IdPosto == 116)
                    {
                        var p119 = GetMediaSemanal(Propagacoes, 119, d);
                        var p118 = GetMediaSemanal(Propagacoes, 118, d);
                        p.VazaoNatural[d] = p119 - p118;
                        if (p.VazaoNatural[d] <= 0)
                        {
                            p.VazaoNatural[d] = 0;
                        }
                        else if (p.VazaoNatural[d] <= 1)
                        {
                            p.VazaoNatural[d] = 1;
                        }
                    }

                    else if (p.IdPosto == 70)
                    {
                        var p73 = GetMediaSemanal(Propagacoes, 73, d);
                        if (p73 > 0)
                        {
                            var p73b = p73 - 10;
                            if (p73b <= 173.5)
                            {
                                p.VazaoNatural[d] = p73 - p73b;
                            }
                            else
                            {
                                p.VazaoNatural[d] = p73 - 173.5;
                            }
                        }
                        else
                        {
                            p.VazaoNatural[d] = 0;
                        }
                    }

                    else if (p.IdPosto == 75)
                    {
                        var p76 = GetMediaSemanal(Propagacoes, 76, d);
                        if (p76 > 0)
                        {
                            var p73 = GetMediaSemanal(Propagacoes, 73, d) - 10;
                            if (p73 <= 173.5)
                            {
                                p.VazaoNatural[d] = p76 + p73;
                            }
                            else
                            {
                                p.VazaoNatural[d] = p76 + 173.5;
                            }
                        }
                        else
                        {
                            p.VazaoNatural[d] = 0;
                        }

                    }

                    else if (p.IdPosto == 37)
                    {
                        var p237 = GetMediaSemanal(Propagacoes, 237, d);
                        var p161 = GetMediaSemanal(Propagacoes, 161, d);
                        var p117 = GetMediaSemanal(Propagacoes, 117, d);
                        var p118 = GetMediaSemanal(Propagacoes, 118, d);
                        p.VazaoNatural[d] = p237 - 0.1 * (p161 - p117 - p118) - p117 - p118;
                        if (p.VazaoNatural[d] <= 0)
                        {
                            p.VazaoNatural[d] = 0;
                        }
                        else if (p.VazaoNatural[d] <= 1)
                        {
                            p.VazaoNatural[d] = 1;
                        }

                    }

                    else if (p.IdPosto == 38)
                    {
                        var p238 = GetMediaSemanal(Propagacoes, 238, d);
                        var p161 = GetMediaSemanal(Propagacoes, 161, d);
                        var p117 = GetMediaSemanal(Propagacoes, 117, d);
                        var p118 = GetMediaSemanal(Propagacoes, 118, d);
                        p.VazaoNatural[d] = p238 - 0.1 * (p161 - p117 - p118) - p117 - p118;
                        if (p.VazaoNatural[d] <= 0)
                        {
                            p.VazaoNatural[d] = 0;
                        }
                        else if (p.VazaoNatural[d] <= 1)
                        {
                            p.VazaoNatural[d] = 1;
                        }

                    }

                    else if (p.IdPosto == 318)
                    {
                        var p116 = GetMediaSemanal(Propagacoes, 116, d);
                        var p161 = GetMediaSemanal(Propagacoes, 161, d);
                        var p117 = GetMediaSemanal(Propagacoes, 117, d);
                        var p118 = GetMediaSemanal(Propagacoes, 118, d);
                        p.VazaoNatural[d] = p116 + 0.1 * (p161 - p117 - p118) + p117 + p118;
                        if (p.VazaoNatural[d] <= 0)
                        {
                            p.VazaoNatural[d] = 0;
                        }
                        else if (p.VazaoNatural[d] <= 1)
                        {
                            p.VazaoNatural[d] = 1;
                        }

                    }

                    else if (p.IdPosto == 319)
                    {
                        var p117 = GetMediaSemanal(Propagacoes, 117, d);
                        var p118 = GetMediaSemanal(Propagacoes, 118, d);
                        var p161 = GetMediaSemanal(Propagacoes, 161, d);


                        p.VazaoNatural[d] = p117 + p118 + 0.1 * (p161 - p117 - p118);
                        if (p.VazaoNatural[d] <= 0)
                        {
                            p.VazaoNatural[d] = 0;
                        }
                        else if (p.VazaoNatural[d] <= 1)
                        {
                            p.VazaoNatural[d] = 1;
                        }

                    }

                    else if (p.IdPosto == 320)
                    {
                        var p119 = GetMediaSemanal(Propagacoes, 119, d);

                        p.VazaoNatural[d] = p119;

                    }

                    else if (p.IdPosto == 39)
                    {
                        var p239 = GetMediaSemanal(Propagacoes, 239, d);
                        var p161 = GetMediaSemanal(Propagacoes, 161, d);
                        var p117 = GetMediaSemanal(Propagacoes, 117, d);
                        var p118 = GetMediaSemanal(Propagacoes, 118, d);
                        p.VazaoNatural[d] = p239 - 0.1 * (p161 - p117 - p118) - p117 - p118;
                        if (p.VazaoNatural[d] <= 0)
                        {
                            p.VazaoNatural[d] = 0;
                        }
                        else if (p.VazaoNatural[d] <= 1)
                        {
                            p.VazaoNatural[d] = 1;
                        }

                    }

                    else if (p.IdPosto == 40)
                    {
                        var p240 = GetMediaSemanal(Propagacoes, 240, d);
                        var p161 = GetMediaSemanal(Propagacoes, 161, d);
                        var p117 = GetMediaSemanal(Propagacoes, 117, d);
                        var p118 = GetMediaSemanal(Propagacoes, 118, d);
                        p.VazaoNatural[d] = p240 - 0.1 * (p161 - p117 - p118) - p117 - p118;
                        if (p.VazaoNatural[d] <= 0)
                        {
                            p.VazaoNatural[d] = 0;
                        }
                        else if (p.VazaoNatural[d] <= 1)
                        {
                            p.VazaoNatural[d] = 1;
                        }

                    }

                    else if (p.IdPosto == 42)
                    {
                        var p242 = GetMediaSemanal(Propagacoes, 242, d);
                        var p161 = GetMediaSemanal(Propagacoes, 161, d);
                        var p117 = GetMediaSemanal(Propagacoes, 117, d);
                        var p118 = GetMediaSemanal(Propagacoes, 118, d);
                        p.VazaoNatural[d] = p242 - 0.1 * (p161 - p117 - p118) - p117 - p118;
                        if (p.VazaoNatural[d] <= 0)
                        {
                            p.VazaoNatural[d] = 0;
                        }
                        else if (p.VazaoNatural[d] <= 1)
                        {
                            p.VazaoNatural[d] = 1;
                        }

                    }

                    else if (p.IdPosto == 43)
                    {
                        var p243 = GetMediaSemanal(Propagacoes, 243, d);
                        var p161 = GetMediaSemanal(Propagacoes, 161, d);
                        var p117 = GetMediaSemanal(Propagacoes, 117, d);
                        var p118 = GetMediaSemanal(Propagacoes, 118, d);
                        p.VazaoNatural[d] = p243 - 0.1 * (p161 - p117 - p118) - p117 - p118;
                        if (p.VazaoNatural[d] <= 0)
                        {
                            p.VazaoNatural[d] = 0;
                        }
                        else if (p.VazaoNatural[d] <= 1)
                        {
                            p.VazaoNatural[d] = 1;
                        }

                    }

                    else if (p.IdPosto == 44)
                    {
                        var p244 = GetMediaSemanal(Propagacoes, 244, d);
                        var p161 = GetMediaSemanal(Propagacoes, 161, d);
                        var p117 = GetMediaSemanal(Propagacoes, 117, d);
                        var p118 = GetMediaSemanal(Propagacoes, 118, d);
                        p.VazaoNatural[d] = p244 - 0.1 * (p161 - p117 - p118) - p117 - p118;
                        if (p.VazaoNatural[d] <= 0)
                        {
                            p.VazaoNatural[d] = 0;
                        }
                        else if (p.VazaoNatural[d] <= 1)
                        {
                            p.VazaoNatural[d] = 1;
                        }

                    }

                    else if (p.IdPosto == 45)
                    {
                        var p245 = GetMediaSemanal(Propagacoes, 245, d);
                        var p161 = GetMediaSemanal(Propagacoes, 161, d);
                        var p117 = GetMediaSemanal(Propagacoes, 117, d);
                        var p118 = GetMediaSemanal(Propagacoes, 118, d);
                        p.VazaoNatural[d] = p245 - 0.1 * (p161 - p117 - p118) - p117 - p118;
                        if (p.VazaoNatural[d] <= 0)
                        {
                            p.VazaoNatural[d] = 0;
                        }
                        else if (p.VazaoNatural[d] <= 1)
                        {
                            p.VazaoNatural[d] = 1;
                        }

                    }

                    else if (p.IdPosto == 46)
                    {
                        var p246 = GetMediaSemanal(Propagacoes, 246, d);
                        var p161 = GetMediaSemanal(Propagacoes, 161, d);
                        var p117 = GetMediaSemanal(Propagacoes, 117, d);
                        var p118 = GetMediaSemanal(Propagacoes, 118, d);
                        p.VazaoNatural[d] = p246 - 0.1 * (p161 - p117 - p118) - p117 - p118;
                        if (p.VazaoNatural[d] <= 0)
                        {
                            p.VazaoNatural[d] = 0;
                        }
                        else if (p.VazaoNatural[d] <= 1)
                        {
                            p.VazaoNatural[d] = 1;
                        }

                    }

                    else if (p.IdPosto == 66)
                    {
                        var p266 = GetMediaSemanal(Propagacoes, 266, d);
                        var p161 = GetMediaSemanal(Propagacoes, 161, d);
                        var p117 = GetMediaSemanal(Propagacoes, 117, d);
                        var p118 = GetMediaSemanal(Propagacoes, 118, d);
                        p.VazaoNatural[d] = p266 - 0.1 * (p161 - p117 - p118) - p117 - p118;
                        if (p.VazaoNatural[d] <= 0)
                        {
                            p.VazaoNatural[d] = 0;
                        }
                        else if (p.VazaoNatural[d] <= 1)
                        {
                            p.VazaoNatural[d] = 1;
                        }

                    }
                }

            }



        }

        public static void AjustesDiarios(List<Propagacao> Propagacoes, List<DateTime> dias)
        {
            try
            {
                List<int> postos = new List<int> { 23, 209, 24, 206, 207, 28, 31, 32, 33, 48, 49, 249, 50, 52, 61, 62, 63, 34, 243, 245, 246 };

                var p266 = Propagacoes.Where(x => x.IdPosto == 266).First();
                var p166 = Propagacoes.Where(x => x.IdPosto == 166).First();
                var p123 = Propagacoes.Where(x => x.IdPosto == 123).First();
                var p21 = Propagacoes.Where(x => x.IdPosto == 21).First();

                foreach (var dia in dias)
                {
                    if (p266.VazaoIncremental.ContainsKey(dia))
                    {
                        p166.VazaoNatural[dia] = p266.VazaoIncremental[dia];
                    }
                    else
                    {
                        var valor = GetVazao(p166, dia);
                        p166.VazaoNatural[dia] = valor;
                    }
                    p21.VazaoNatural[dia] = p123.VazaoNatural[dia];
                }



                foreach (var p in Propagacoes)
                {
                    if (postos.Any(x => x.Equals(p.IdPosto)))
                    {
                        CalcDiaria(p, dias);
                    }
                }

                var p34 = Propagacoes.Where(x => x.IdPosto == 34).First();
                var p99 = Propagacoes.Where(x => x.IdPosto == 99).First();
                var p241 = Propagacoes.Where(x => x.IdPosto == 241).First();
                var p261 = Propagacoes.Where(x => x.IdPosto == 261).First();
                var p33 = Propagacoes.Where(x => x.IdPosto == 33).First();
                var p18 = Propagacoes.Where(x => x.IdPosto == 18).First();
                var p243 = Propagacoes.Where(x => x.IdPosto == 243).First();
                var p158 = Propagacoes.Where(x => x.IdPosto == 158).First();
                var p245 = Propagacoes.Where(x => x.IdPosto == 245).First();
                var p154 = Propagacoes.Where(x => x.IdPosto == 154).First();
                var p246 = Propagacoes.Where(x => x.IdPosto == 246).First();
                var p271 = Propagacoes.Where(x => x.IdPosto == 271).First();
                var p273 = Propagacoes.Where(x => x.IdPosto == 273).First();

                foreach (var dia in dias)
                {
                    if (p34.VazaoIncremental.ContainsKey(dia))
                    {
                        p34.VazaoNatural[dia] = p34.VazaoIncremental[dia] + p99.VazaoNatural[dia] + p241.VazaoNatural[dia] + p261.VazaoNatural[dia] + p33.VazaoNatural[dia] + p18.VazaoNatural[dia];
                    }
                }
                foreach (var dia in dias)
                {
                    if (p243.VazaoIncremental.ContainsKey(dia))
                    {
                        p243.VazaoNatural[dia] = p243.VazaoIncremental[dia] + p158.VazaoNatural[dia];
                    }
                }
                foreach (var dia in dias)
                {
                    if (p245.VazaoIncremental.ContainsKey(dia))
                    {
                        p245.VazaoNatural[dia] = p245.VazaoIncremental[dia] + p243.VazaoNatural[dia] + p34.VazaoNatural[dia];
                    }
                }
                foreach (var dia in dias)
                {
                    if (p245.VazaoIncremental.ContainsKey(dia))
                    {
                        p246.VazaoNatural[dia] = p246.VazaoIncremental[dia] + p154.VazaoNatural[dia] + p245.VazaoNatural[dia];
                    }
                }
                foreach (var dia in dias)
                {
                    if (p271.VazaoIncremental.ContainsKey(dia))
                    {
                        p271.VazaoNatural[dia] = p271.VazaoIncremental[dia] + p273.VazaoNatural[dia];
                    }
                }

            }
            catch (Exception e)
            {
                e.ToString();
            }


        }

        public static void CalcDiaria(Propagacao prop, List<DateTime> dias)
        {
            try
            {

                foreach (var dia in dias)
                {
                    if (prop.VazaoIncremental.ContainsKey(dia))
                    {
                        if (dia != dias.First())
                        {
                            if (prop.PostoMontantes.Count() > 0 && prop.IdPosto != 287)
                            {
                                var vazao = SomaIncDiaria(prop, dia);
                                prop.VazaoNatural[dia] = vazao;
                            }
                        }

                    }


                }

            }
            catch (Exception exc)
            {

            }

        }

        public static double SomaIncDiaria(Propagacao propaga, DateTime dia)
        {
            try
            {
                double vazaoCalcinc = 0;
                if (propaga.PostoMontantes.Count() > 0)
                {
                    foreach (var prop in propaga.PostoMontantes)
                    {
                        var postoMontante = prop.Propaga;
                        vazaoCalcinc += SomaIncDiaria(postoMontante, dia);
                    }
                }

                if (propaga.IdPosto != 2)//o posto 2 (ITUTINGA) é uma copia  do posto 1(camargos), esse if previne o erro no calculo.
                {
                    vazaoCalcinc += propaga.VazaoIncremental[dia];
                }


                return vazaoCalcinc;
            }
            catch (Exception e)
            {
                e.ToString();
                return 0;
            }

        }

        public static void ExportaEnas(List<Propagacao> propagacoes, string pastaSaida)
        {
            try
            {
                List<string> subMercados = new List<string>() { "SE/CO", "S", "NE", "N" };
                List<string> sudeste = new List<string>() { "TOCANTINS (SE)", "SÃO FRANCISCO (SE)", "JEQUITINHONHA (SE)", "PARAGUAI", "DOCE", "MUCURI", "ITABAPOANA", "PARAÍBA DO SUL", "ALTO TIETÊ" };
                List<string> madeira = new List<string>() { "AMAZONAS (SE)" };//tem calculo (-total de tele pires)
                List<string> parana = new List<string>() { "GRANDE", "PARANAÍBA", "TIETÊ", "ALTO PARANÁ" };//tem calculo
                List<string> paranapanema = new List<string>() { "PARANAPANEMA (SE)" };//Tem calculo
                List<string> sul = new List<string>() { "PARANAPANEMA (S)", "IGUAÇU" };//tem calculo (somar com total do sul)
                List<string> iguacu = new List<string>() { "PARANAPANEMA (S)", "IGUAÇU" };
                List<string> norte = new List<string>() { "TOCANTINS (N)" };
                List<string> Belomonte = new List<string>() { "AMOZONAS - BELO MONTE" };//tem calculo
                List<string> manaus = new List<string>() { "ARAGUARI", "AMAZONAS (N)" };//tem calculo (-valor calculado em belo monte)
                                                                                        //enasemanal
                #region enasemanal

                StringBuilder vazoesSemPosto = new StringBuilder();
                var semanasVazoes = propagacoes.Where(x => x.calMedSemanal.Count() == 12).Select(x => x.calMedSemanal.Keys.ToList()).First();
                vazoesSemPosto.AppendFormat("{0,-5}",
                    "Posto");
                //var dats = enasemanal.Where(x => x.DadoEna.Count() == 12).Select(x => x.DadoEna).First();


                foreach (var item in semanasVazoes)
                {
                    vazoesSemPosto.AppendFormat("{0,15:dd/MM/yyyy}",
                            item.ToString("dd/MM/yyyy"));
                    if (item == semanasVazoes.Last())
                    {
                        vazoesSemPosto.AppendLine();
                    }
                }
                foreach (var p in propagacoes)
                {
                    vazoesSemPosto.AppendFormat("{0,-5}",
                    p.IdPosto);
                    foreach (var item in semanasVazoes)
                    {
                        double resSem = 0;

                        resSem = p.calMedSemanal[item];

                        vazoesSemPosto.AppendFormat("{0,15}",
                                Math.Round(resSem, 5).ToString());
                        if (item == semanasVazoes.Last())
                        {
                            vazoesSemPosto.AppendLine();
                        }
                    }
                }
                File.WriteAllText(Path.Combine(pastaSaida, "VazoesSemanais.log"), vazoesSemPosto.ToString());


                var Prods = new ESTUDO_PVEntities().Postos.Where(x => x.id <= 350).ToList();
                var ProdsGr = Prods.GroupBy(x => x.bacia).ToList();
                StringBuilder enaSem = new StringBuilder();
                List<Enas> enasemanal = new List<Enas>();
                List<Tuple<string, DateTime, double>> ReeDados = new List<Tuple<string, DateTime, double>>();
                foreach (var p in propagacoes)
                {
                    if (Prods.Any(x => x.id == p.IdPosto))
                    {
                        var postoEna = new Enas() { IdPosto = p.IdPosto, bacia = Prods.Where(x => x.id == p.IdPosto).Select(x => x.bacia).First() };
                        postoEna.subMercado = Convert.ToInt32(Prods.Where(x => x.id == p.IdPosto).Select(x => x.submercado).First());
                        var dataSenamal = p.calMedSemanal.OrderBy(x => x.Key).ToList();
                        foreach (var item in dataSenamal)
                        {
                            postoEna.DadoEna[item.Key] = p.calMedSemanal[item.Key] * Convert.ToDouble(Prods.Where(x => x.id == p.IdPosto).Select(x => x.produtibilidade).First());
                        }
                        enasemanal.Add(postoEna);
                    }

                }
                enaSem.AppendFormat("{0,-20}",
                    "ENA (MWmed)");
                var dats = enasemanal.Where(x => x.DadoEna.Count() == 12).Select(x => x.DadoEna).First();
                foreach (var item in dats.Keys.ToList())
                {
                    enaSem.AppendFormat("{0,15:dd/MM a dd/MM}",
                           item.AddDays(-6).ToString("dd/MM") + " a " + item.ToString("dd/MM"));
                    if (item == dats.Keys.Last())
                    {
                        enaSem.AppendLine();
                    }
                }
                for (int s = 0; s < 4; s++)
                {
                    enaSem.AppendFormat("{0,-20}",
                    subMercados[s]);
                    foreach (var item in dats.Keys.ToList())
                    {
                        double resultado = 0;

                        foreach (var post in enasemanal.Where(x => x.subMercado == s + 1))
                        {
                            resultado += post.DadoEna[item];
                        }
                        enaSem.AppendFormat("{0,15}",
                                Math.Round(resultado, 5).ToString());
                        if (item == dats.Keys.Last())
                        {
                            enaSem.AppendLine();
                        }
                    }

                }
                var bacias = enasemanal.Select(x => x.bacia);
                bacias = bacias.Union(enasemanal.Select(x => x.bacia));

                foreach (var bac in bacias)
                {
                    if (bac == "-" || bac == "PARANAPANEMA")
                    {
                        continue;
                    }
                    else
                    {
                        enaSem.AppendFormat("{0,-20}",
                               bac.Replace("XINGU", "AMOZONAS - BELO MONTE").ToString());
                        foreach (var item in dats.Keys.ToList())
                        {
                            double resultado = 0;

                            foreach (var post in enasemanal.Where(x => x.bacia == bac))
                            {
                                resultado += post.DadoEna[item];
                            }
                            Tuple<string, DateTime, double> rDado = new Tuple<string, DateTime, double>(bac.Replace("XINGU", "AMOZONAS - BELO MONTE").ToString(), item, resultado);
                            ReeDados.Add(rDado);

                            enaSem.AppendFormat("{0,15}",
                                    Math.Round(resultado, 5).ToString());
                            if (item == dats.Keys.Last())
                            {
                                enaSem.AppendLine();
                            }
                        }
                    }


                }
                enaSem.AppendFormat("{0,-20}",
                    "ENA (MWmed)"); enaSem.AppendLine();
                enaSem.AppendFormat("{0,-20}",
                "SUDESTE");
                foreach (var item in dats.Keys.ToList())
                {
                    double valor = 0;
                    foreach (var bc in sudeste)
                    {
                        var dados = ReeDados.Where(x => x.Item1 == bc && x.Item2 == item).Select(x => x.Item3).FirstOrDefault();
                        valor += dados;
                    }
                    enaSem.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dats.Keys.Last())
                    {
                        enaSem.AppendLine();
                    }
                }
                enaSem.AppendFormat("{0,-20}",
                "MADEIRA");
                foreach (var item in dats.Keys.ToList())
                {
                    double valor = 0;
                    foreach (var bc in madeira)
                    {
                        var dados = ReeDados.Where(x => x.Item1 == bc && x.Item2 == item).Select(x => x.Item3).FirstOrDefault();
                        valor += dados;
                    }
                    valor -= enasemanal.Where(x => x.IdPosto == 227).Select(x => x.DadoEna[item]).FirstOrDefault();//sinop
                    valor -= enasemanal.Where(x => x.IdPosto == 228).Select(x => x.DadoEna[item]).FirstOrDefault();//colider
                    valor -= enasemanal.Where(x => x.IdPosto == 229).Select(x => x.DadoEna[item]).FirstOrDefault();//teles pires
                    valor -= enasemanal.Where(x => x.IdPosto == 230).Select(x => x.DadoEna[item]).FirstOrDefault();//são manuel
                    enaSem.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dats.Keys.Last())
                    {
                        enaSem.AppendLine();
                    }
                }
                enaSem.AppendFormat("{0,-20}",
                "TELES PIRES");
                foreach (var item in dats.Keys.ToList())
                {
                    double valor = 0;

                    valor += enasemanal.Where(x => x.IdPosto == 227).Select(x => x.DadoEna[item]).FirstOrDefault();//sinop
                    valor += enasemanal.Where(x => x.IdPosto == 228).Select(x => x.DadoEna[item]).FirstOrDefault();//colider
                    valor += enasemanal.Where(x => x.IdPosto == 229).Select(x => x.DadoEna[item]).FirstOrDefault();//teles pires
                    valor += enasemanal.Where(x => x.IdPosto == 230).Select(x => x.DadoEna[item]).FirstOrDefault();//são manuel
                    enaSem.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dats.Keys.Last())
                    {
                        enaSem.AppendLine();
                    }
                }
                enaSem.AppendFormat("{0,-20}",
                "ITAIPÚ");
                foreach (var item in dats.Keys.ToList())
                {
                    double valor = 0;
                    var produti = Convert.ToDouble(Prods.Where(x => x.id == 66).Select(x => x.produtibilidade).First());

                    var dad66 = propagacoes.Where(x => x.IdPosto == 66).Select(x => x.calMedSemanal[item]).FirstOrDefault();
                    var dad44 = propagacoes.Where(x => x.IdPosto == 44).Select(x => x.calMedSemanal[item]).FirstOrDefault();
                    var dad61 = propagacoes.Where(x => x.IdPosto == 61).Select(x => x.calMedSemanal[item]).FirstOrDefault();

                    valor = (dad66 - dad44 - dad61) * produti;//itaipu incremental
                    enaSem.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dats.Keys.Last())
                    {
                        enaSem.AppendLine();
                    }
                }
                enaSem.AppendFormat("{0,-20}",
                "PARANA");
                foreach (var item in dats.Keys.ToList())
                {
                    double valor = 0;
                    foreach (var bc in parana)
                    {
                        var dados = ReeDados.Where(x => x.Item1 == bc && x.Item2 == item).Select(x => x.Item3).FirstOrDefault();
                        valor += dados;
                    }
                    var dad44 = propagacoes.Where(x => x.IdPosto == 44).Select(x => x.calMedSemanal[item]).FirstOrDefault();
                    var produti = Convert.ToDouble(Prods.Where(x => x.id == 66).Select(x => x.produtibilidade).First());

                    valor += (dad44 * produti);//ITAIPU Mont PARANA
                    valor += enasemanal.Where(x => x.IdPosto == 154).Select(x => x.DadoEna[item]).FirstOrDefault();//sao domingos
                    valor += enasemanal.Where(x => x.IdPosto == 46).Select(x => x.DadoEna[item]).FirstOrDefault();//p. primevera
                    enaSem.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dats.Keys.Last())
                    {
                        enaSem.AppendLine();
                    }
                }
                enaSem.AppendFormat("{0,-20}",
                "PARANAPANEMA");
                foreach (var item in dats.Keys.ToList())
                {
                    double valor = 0;
                    foreach (var bc in paranapanema)
                    {
                        var dados = ReeDados.Where(x => x.Item1 == bc && x.Item2 == item).Select(x => x.Item3).FirstOrDefault();
                        valor += dados;
                    }
                    var dad61 = propagacoes.Where(x => x.IdPosto == 61).Select(x => x.calMedSemanal[item]).FirstOrDefault();
                    var produti = Convert.ToDouble(Prods.Where(x => x.id == 66).Select(x => x.produtibilidade).First());

                    valor += (dad61 * produti);//ITAIPU Mont PARANAPANEMA

                    enaSem.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dats.Keys.Last())
                    {
                        enaSem.AppendLine();
                    }
                }
                enaSem.AppendFormat("{0,-20}",
                "SUL");
                foreach (var item in dats.Keys.ToList())
                {
                    double valor = 0;
                    foreach (var post in enasemanal.Where(x => x.subMercado == 2))
                    {
                        valor += post.DadoEna[item];
                    }
                    foreach (var bc in sul)
                    {
                        var dados = ReeDados.Where(x => x.Item1 == bc && x.Item2 == item).Select(x => x.Item3).FirstOrDefault();
                        valor -= dados;
                    }

                    enaSem.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dats.Keys.Last())
                    {
                        enaSem.AppendLine();
                    }
                }
                enaSem.AppendFormat("{0,-20}",
              "IGUAÇU");
                foreach (var item in dats.Keys.ToList())
                {
                    double valor = 0;
                    foreach (var bc in iguacu)
                    {
                        var dados = ReeDados.Where(x => x.Item1 == bc && x.Item2 == item).Select(x => x.Item3).FirstOrDefault();
                        valor += dados;
                    }

                    enaSem.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dats.Keys.Last())
                    {
                        enaSem.AppendLine();
                    }
                }
                enaSem.AppendFormat("{0,-20}",
                "NORDESTE");
                foreach (var item in dats.Keys.ToList())
                {
                    double valor = 0;
                    foreach (var post in enasemanal.Where(x => x.subMercado == 3))
                    {
                        valor += post.DadoEna[item];
                    }

                    enaSem.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dats.Keys.Last())
                    {
                        enaSem.AppendLine();
                    }
                }
                enaSem.AppendFormat("{0,-20}",
                "NORTE");
                foreach (var item in dats.Keys.ToList())
                {
                    double valor = 0;
                    foreach (var bc in norte)
                    {
                        var dados = ReeDados.Where(x => x.Item1 == bc && x.Item2 == item).Select(x => x.Item3).FirstOrDefault();
                        valor += dados;
                    }
                    enaSem.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dats.Keys.Last())
                    {
                        enaSem.AppendLine();
                    }
                }
                enaSem.AppendFormat("{0,-20}",
                "BELO MONTE");
                foreach (var item in dats.Keys.ToList())
                {
                    double valor = 0;
                    foreach (var bc in Belomonte)
                    {
                        var dados = ReeDados.Where(x => x.Item1 == bc && x.Item2 == item).Select(x => x.Item3).FirstOrDefault();
                        valor += dados;
                    }

                    valor += enasemanal.Where(x => x.IdPosto == 277).Select(x => x.DadoEna[item]).FirstOrDefault();//carua-una

                    enaSem.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dats.Keys.Last())
                    {
                        enaSem.AppendLine();
                    }
                }
                enaSem.AppendFormat("{0,-20}",
               "MANAUS");
                foreach (var item in dats.Keys.ToList())
                {
                    double valor = 0;
                    foreach (var bc in manaus)
                    {
                        var dados = ReeDados.Where(x => x.Item1 == bc && x.Item2 == item).Select(x => x.Item3).FirstOrDefault();
                        valor += dados;
                    }

                    valor -= enasemanal.Where(x => x.IdPosto == 277).Select(x => x.DadoEna[item]).FirstOrDefault();//carua-una

                    enaSem.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dats.Keys.Last())
                    {
                        enaSem.AppendLine();
                    }
                }
                File.WriteAllText(Path.Combine(pastaSaida, "enasemanal.log"), enaSem.ToString());
                #endregion

                //enadiaria
                var dataInicio = DateTime.Today;
                if (pastaSaida.Contains("d-1"))
                {
                    dataInicio = dataInicio.AddDays(-1);
                }

                List<DateTime> dias = new List<DateTime>();
                for (DateTime d = dataInicio; d <= dataInicio.AddDays(25); d = d.AddDays(1))
                {
                    dias.Add(d);
                }

                StringBuilder enaDia = new StringBuilder();
                List<Enas> enasDiarias = new List<Enas>();
                List<Tuple<string, DateTime, double>> ReeDadoDia = new List<Tuple<string, DateTime, double>>();

                foreach (var p in propagacoes)
                {
                    foreach (var dia in dias)
                    {
                        if (!p.VazaoNatural.ContainsKey(dia) || p.VazaoNatural[dia] == 0)
                        {
                            var vazao = GetVazao(p, dia);
                            p.VazaoNatural[dia] = vazao;
                        }
                    }
                }

                AjustesDiarios(propagacoes, dias);

                CalcularPostRegre(propagacoes, dias);
                CalcularPostCalculados(propagacoes, dias);

                StringBuilder vazoesDiariasPosto = new StringBuilder();
                vazoesDiariasPosto.AppendFormat("{0,-5}",
                    "Posto");
                //var dats = enasemanal.Where(x => x.DadoEna.Count() == 12).Select(x => x.DadoEna).First();
                foreach (var item in dias)
                {
                    vazoesDiariasPosto.AppendFormat("{0,15:dd/MM/yyyy}",
                            item.ToString("dd/MM/yyyy"));
                    if (item == dias.Last())
                    {
                        vazoesDiariasPosto.AppendLine();
                    }
                }
                foreach (var p in propagacoes)
                {
                    vazoesDiariasPosto.AppendFormat("{0,-5}",
                    p.IdPosto);
                    foreach (var item in dias)
                    {
                        double resDiario = 0;

                        resDiario = p.VazaoNatural[item];

                        vazoesDiariasPosto.AppendFormat("{0,15}",
                                Math.Round(resDiario, 5).ToString());
                        if (item == dias.Last())
                        {
                            vazoesDiariasPosto.AppendLine();
                        }
                    }
                }
                File.WriteAllText(Path.Combine(pastaSaida, "VazoesDiarias.log"), vazoesDiariasPosto.ToString());


                foreach (var p in propagacoes)
                {
                    if (Prods.Any(x => x.id == p.IdPosto))
                    {
                        var postoEna = new Enas() { IdPosto = p.IdPosto, bacia = Prods.Where(x => x.id == p.IdPosto).Select(x => x.bacia).First() };
                        postoEna.subMercado = Convert.ToInt32(Prods.Where(x => x.id == p.IdPosto).Select(x => x.submercado).First());
                        //foreach (var dia in dias)
                        //{
                        //    if (!p.VazaoNatural.ContainsKey(dia)||p.VazaoNatural[dia] == 0)
                        //    {
                        //        var vazao = GetVazao(p, dia);
                        //        p.VazaoNatural[dia] = vazao;
                        //    }
                        //}

                        foreach (var item in dias)
                        {
                            postoEna.DadoEna[item] = p.VazaoNatural[item] * Convert.ToDouble(Prods.Where(x => x.id == p.IdPosto).Select(x => x.produtibilidade).First());
                        }
                        enasDiarias.Add(postoEna);
                    }

                }

                enaDia.AppendFormat("{0,-20}",
                    "ENA (MWmed)");
                //var dats = enasemanal.Where(x => x.DadoEna.Count() == 12).Select(x => x.DadoEna).First();
                foreach (var item in dias)
                {
                    enaDia.AppendFormat("{0,15:dd/MM/yyyy}",
                            item.ToString("dd/MM/yyyy"));
                    if (item == dias.Last())
                    {
                        enaDia.AppendLine();
                    }
                }
                for (int s = 0; s < 4; s++)
                {
                    enaDia.AppendFormat("{0,-20}",
                    subMercados[s]);
                    foreach (var item in dias)
                    {
                        double resultado = 0;

                        foreach (var post in enasDiarias.Where(x => x.subMercado == s + 1))
                        {
                            resultado += post.DadoEna[item];
                        }
                        enaDia.AppendFormat("{0,15}",
                                Math.Round(resultado, 5).ToString());
                        if (item == dias.Last())
                        {
                            enaDia.AppendLine();
                        }
                    }

                }
                var baciasDia = enasDiarias.Select(x => x.bacia);
                baciasDia = baciasDia.Union(enasDiarias.Select(x => x.bacia));

                foreach (var bac in baciasDia)
                {
                    if (bac == "-" || bac == "PARANAPANEMA")
                    {
                        continue;
                    }
                    else
                    {
                        enaDia.AppendFormat("{0,-20}",
                               bac.Replace("XINGU", "AMOZONAS - BELO MONTE").ToString());
                        foreach (var item in dias)
                        {
                            double resultado = 0;

                            foreach (var post in enasDiarias.Where(x => x.bacia == bac))
                            {
                                resultado += post.DadoEna[item];
                            }
                            Tuple<string, DateTime, double> rDado = new Tuple<string, DateTime, double>(bac.Replace("XINGU", "AMOZONAS - BELO MONTE").ToString(), item, resultado);
                            ReeDadoDia.Add(rDado);

                            enaDia.AppendFormat("{0,15}",
                                    Math.Round(resultado, 5).ToString());
                            if (item == dias.Last())
                            {
                                enaDia.AppendLine();
                            }
                        }
                    }


                }
                enaDia.AppendFormat("{0,-20}",
                    "ENA (MWmed)"); enaDia.AppendLine();
                enaDia.AppendFormat("{0,-20}",
                "SUDESTE");
                foreach (var item in dias)
                {
                    double valor = 0;
                    foreach (var bc in sudeste)
                    {
                        var dados = ReeDadoDia.Where(x => x.Item1 == bc && x.Item2 == item).Select(x => x.Item3).FirstOrDefault();
                        valor += dados;
                    }
                    enaDia.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dias.Last())
                    {
                        enaDia.AppendLine();
                    }
                }
                enaDia.AppendFormat("{0,-20}",
                "MADEIRA");
                foreach (var item in dias)
                {
                    double valor = 0;
                    foreach (var bc in madeira)
                    {
                        var dados = ReeDadoDia.Where(x => x.Item1 == bc && x.Item2 == item).Select(x => x.Item3).FirstOrDefault();
                        valor += dados;
                    }
                    valor -= enasDiarias.Where(x => x.IdPosto == 227).Select(x => x.DadoEna[item]).FirstOrDefault();//sinop
                    valor -= enasDiarias.Where(x => x.IdPosto == 228).Select(x => x.DadoEna[item]).FirstOrDefault();//colider
                    valor -= enasDiarias.Where(x => x.IdPosto == 229).Select(x => x.DadoEna[item]).FirstOrDefault();//teles pires
                    valor -= enasDiarias.Where(x => x.IdPosto == 230).Select(x => x.DadoEna[item]).FirstOrDefault();//são manuel
                    enaDia.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dias.Last())
                    {
                        enaDia.AppendLine();
                    }
                }
                enaDia.AppendFormat("{0,-20}",
                "TELES PIRES");
                foreach (var item in dias)
                {
                    double valor = 0;

                    valor += enasDiarias.Where(x => x.IdPosto == 227).Select(x => x.DadoEna[item]).FirstOrDefault();//sinop
                    valor += enasDiarias.Where(x => x.IdPosto == 228).Select(x => x.DadoEna[item]).FirstOrDefault();//colider
                    valor += enasDiarias.Where(x => x.IdPosto == 229).Select(x => x.DadoEna[item]).FirstOrDefault();//teles pires
                    valor += enasDiarias.Where(x => x.IdPosto == 230).Select(x => x.DadoEna[item]).FirstOrDefault();//são manuel
                    enaDia.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dias.Last())
                    {
                        enaDia.AppendLine();
                    }
                }
                enaDia.AppendFormat("{0,-20}",
                "ITAIPÚ");
                foreach (var item in dias)
                {
                    double valor = 0;
                    var produti = Convert.ToDouble(Prods.Where(x => x.id == 66).Select(x => x.produtibilidade).First());

                    var dad66 = propagacoes.Where(x => x.IdPosto == 66).Select(x => x.VazaoNatural[item]).FirstOrDefault();
                    var dad44 = propagacoes.Where(x => x.IdPosto == 44).Select(x => x.VazaoNatural[item]).FirstOrDefault();
                    var dad61 = propagacoes.Where(x => x.IdPosto == 61).Select(x => x.VazaoNatural[item]).FirstOrDefault();

                    valor = (dad66 - dad44 - dad61) * produti;//itaipu incremental
                    enaDia.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dias.Last())
                    {
                        enaDia.AppendLine();
                    }
                }
                enaDia.AppendFormat("{0,-20}",
                "PARANA");
                foreach (var item in dias)
                {
                    double valor = 0;
                    foreach (var bc in parana)
                    {
                        var dados = ReeDadoDia.Where(x => x.Item1 == bc && x.Item2 == item).Select(x => x.Item3).FirstOrDefault();
                        valor += dados;
                    }
                    var dad44 = propagacoes.Where(x => x.IdPosto == 44).Select(x => x.VazaoNatural[item]).FirstOrDefault();
                    var produti = Convert.ToDouble(Prods.Where(x => x.id == 66).Select(x => x.produtibilidade).First());

                    valor += (dad44 * produti);//ITAIPU Mont PARANA
                    valor += enasDiarias.Where(x => x.IdPosto == 154).Select(x => x.DadoEna[item]).FirstOrDefault();//sao domingos
                    valor += enasDiarias.Where(x => x.IdPosto == 46).Select(x => x.DadoEna[item]).FirstOrDefault();//p. primevera
                    enaDia.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dias.Last())
                    {
                        enaDia.AppendLine();
                    }
                }
                enaDia.AppendFormat("{0,-20}",
                "PARANAPANEMA");
                foreach (var item in dias)
                {
                    double valor = 0;
                    foreach (var bc in paranapanema)
                    {
                        var dados = ReeDadoDia.Where(x => x.Item1 == bc && x.Item2 == item).Select(x => x.Item3).FirstOrDefault();
                        valor += dados;
                    }
                    var dad61 = propagacoes.Where(x => x.IdPosto == 61).Select(x => x.VazaoNatural[item]).FirstOrDefault();
                    var produti = Convert.ToDouble(Prods.Where(x => x.id == 66).Select(x => x.produtibilidade).First());

                    valor += (dad61 * produti);//ITAIPU Mont PARANAPANEMA

                    enaDia.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dias.Last())
                    {
                        enaDia.AppendLine();
                    }
                }
                enaDia.AppendFormat("{0,-20}",
                "SUL");
                foreach (var item in dias)
                {
                    double valor = 0;
                    foreach (var post in enasDiarias.Where(x => x.subMercado == 2))
                    {
                        valor += post.DadoEna[item];
                    }
                    foreach (var bc in sul)
                    {
                        var dados = ReeDadoDia.Where(x => x.Item1 == bc && x.Item2 == item).Select(x => x.Item3).FirstOrDefault();
                        valor -= dados;
                    }

                    enaDia.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dias.Last())
                    {
                        enaDia.AppendLine();
                    }
                }
                enaDia.AppendFormat("{0,-20}",
              "IGUAÇU");
                foreach (var item in dias)
                {
                    double valor = 0;
                    foreach (var bc in iguacu)
                    {
                        var dados = ReeDadoDia.Where(x => x.Item1 == bc && x.Item2 == item).Select(x => x.Item3).FirstOrDefault();
                        valor += dados;
                    }

                    enaDia.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dias.Last())
                    {
                        enaDia.AppendLine();
                    }
                }
                enaDia.AppendFormat("{0,-20}",
                "NORDESTE");
                foreach (var item in dias)
                {
                    double valor = 0;
                    foreach (var post in enasDiarias.Where(x => x.subMercado == 3))
                    {
                        valor += post.DadoEna[item];
                    }

                    enaDia.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dias.Last())
                    {
                        enaDia.AppendLine();
                    }
                }
                enaDia.AppendFormat("{0,-20}",
                "NORTE");
                foreach (var item in dias)
                {
                    double valor = 0;
                    foreach (var bc in norte)
                    {
                        var dados = ReeDadoDia.Where(x => x.Item1 == bc && x.Item2 == item).Select(x => x.Item3).FirstOrDefault();
                        valor += dados;
                    }
                    enaDia.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dias.Last())
                    {
                        enaDia.AppendLine();
                    }
                }
                enaDia.AppendFormat("{0,-20}",
                "BELO MONTE");
                foreach (var item in dias)
                {
                    double valor = 0;
                    foreach (var bc in Belomonte)
                    {
                        var dados = ReeDadoDia.Where(x => x.Item1 == bc && x.Item2 == item).Select(x => x.Item3).FirstOrDefault();
                        valor += dados;
                    }

                    valor += enasDiarias.Where(x => x.IdPosto == 277).Select(x => x.DadoEna[item]).FirstOrDefault();//carua-una

                    enaDia.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dias.Last())
                    {
                        enaDia.AppendLine();
                    }
                }
                enaDia.AppendFormat("{0,-20}",
               "MANAUS");
                foreach (var item in dias)
                {
                    double valor = 0;
                    foreach (var bc in manaus)
                    {
                        var dados = ReeDadoDia.Where(x => x.Item1 == bc && x.Item2 == item).Select(x => x.Item3).FirstOrDefault();
                        valor += dados;
                    }

                    valor -= enasDiarias.Where(x => x.IdPosto == 277).Select(x => x.DadoEna[item]).FirstOrDefault();//carua-una

                    enaDia.AppendFormat("{0,15}",
                                Math.Round(valor, 5).ToString());
                    if (item == dias.Last())
                    {
                        enaDia.AppendLine();
                    }
                }//completo
                File.WriteAllText(Path.Combine(pastaSaida, "enadiaria.log"), enaDia.ToString());

                //Copia dados para o Z
                string[] arqs_copy = { "enadiaria.log", "enasemanal.log" };

                var path_Z = pastaSaida.Replace("C:\\Files\\Middle - Preço\\16_Chuva_Vazao", "Z:\\16_Chuva_Vazao");
                foreach (var arq in arqs_copy)
                {
                    if (File.Exists(Path.Combine(pastaSaida, arq)))
                    {
                        if (!Directory.Exists(path_Z))
                        {
                            Directory.CreateDirectory(path_Z);
                        }

                        File.Copy(Path.Combine(pastaSaida, arq), Path.Combine(path_Z, arq), true);
                    }

                }


            }
            catch (Exception e)
            {
                e.ToString();
            }
        }
        public static double GetVazao(Propagacao prop, DateTime dia)
        {
            double valor = 0;
            var dataSenamal = prop.calMedSemanal.OrderBy(x => x.Key).ToList();
            foreach (var sem in dataSenamal)
            {
                if (sem.Key >= dia)
                {
                    valor = prop.calMedSemanal[sem.Key];
                    return valor;
                }
            }

            return 0;
        }
        public static string ExportaPrevs(List<Propagacao> propagacoes, string camSaida, DateTime dataAtual, DateTime revdate, int? revnum = null)
        {
            try
            {
                bool exportaMesAnt = false;
                var currRev = Tools.Tools.GetCurrRev(dataAtual);
                int indice = 0;

                int nextRevNum = 0;

                if (!revnum.HasValue)
                {
                    var nextRev = ChuvaVazaoTools.Tools.Tools.GetNextRev(dataAtual);
                    nextRevNum = nextRev.rev;
                }
                else
                    nextRevNum = revnum.Value;

                DateTime Data = DateTime.Today;
                var rev = ChuvaVazaoTools.Tools.Tools.GetCurrRev(Data);
                DateTime inicioMes = new DateTime(rev.revDate.Year, rev.revDate.Month, 1);//data da Rv0 do mês

                var semanaZero = inicioMes;

                while (semanaZero.DayOfWeek != DayOfWeek.Saturday)
                {
                    semanaZero = semanaZero.AddDays(-1);
                }
                semanaZero = semanaZero.AddDays(6);//termino da semana rv0 do mês

                var numSem = ChuvaVazaoTools.Tools.Tools.GetSemNumberAndYear(semanaZero);
                var SemanasPrevs = ChuvaVazaoTools.Tools.Tools.GetNumDatSem(semanaZero, numSem.Item1);// as doze semanasde previsão após execução do previvaz

                if (nextRevNum == 0 || (nextRevNum == 1 && currRev.rev != 0))
                {
                    DateTime dataRv0Mes2 = new DateTime(revdate.Year, revdate.Month, 1);
                    for (DateTime d = dataRv0Mes2; d <= SemanasPrevs.Last().Item1; d = d.AddDays(1))
                    {
                        if (SemanasPrevs.Select(x => x.Item1).Contains(d))
                        {
                            indice = SemanasPrevs.IndexOf(SemanasPrevs.Where(x => x.Item1.Equals(d)).First());
                            exportaMesAnt = true;
                            break;
                        }
                    }
                }
                else
                {
                    indice = 0;
                }
                var prevsname = "prevs.rv" + nextRevNum.ToString();


                if (File.Exists(Path.Combine(camSaida, prevsname)))
                {
                    File.Delete(Path.Combine(camSaida, prevsname));
                }

                StringBuilder sb = new StringBuilder();

                foreach (var p in propagacoes)
                {
                    List<Double> vazoes = new List<double>();

                    for (int i = indice; i <= indice + 5; i++)
                    {
                        var vaz = Math.Round(p.calMedSemanal[SemanasPrevs[i].Item1]);
                        vazoes.Add(vaz);
                    }
                    sb.AppendFormat("{0,6}{1,5}{2,10}{3,10}{4,10}{5,10}{6,10}{7,10}{8}",
                            p.IdPosto.ToString(),
                            p.IdPosto.ToString(),
                           vazoes[0],
                           vazoes[1],
                           vazoes[2],
                           vazoes[3],
                           vazoes[4],
                           vazoes[5],
                            Environment.NewLine);

                }
                File.WriteAllText(Path.Combine(camSaida, prevsname), sb.ToString());

                if (exportaMesAnt)
                {//exporta o prevs referenta ao mes anterior  o numero 5 é apenas para os usuarios diferenciarem do prevs correto 
                    var prevsAnt = "prevs.rv5";
                    indice = 0;

                    if (File.Exists(Path.Combine(camSaida, prevsAnt)))
                    {
                        File.Delete(Path.Combine(camSaida, prevsAnt));
                    }

                    StringBuilder pa = new StringBuilder();

                    foreach (var p in propagacoes)
                    {
                        List<Double> vazoes = new List<double>();

                        for (int i = indice; i <= indice + 5; i++)
                        {
                            var vaz = Math.Round(p.calMedSemanal[SemanasPrevs[i].Item1]);
                            vazoes.Add(vaz);
                        }
                        pa.AppendFormat("{0,6}{1,5}{2,10}{3,10}{4,10}{5,10}{6,10}{7,10}{8}",
                                p.IdPosto.ToString(),
                                p.IdPosto.ToString(),
                               vazoes[0],
                               vazoes[1],
                               vazoes[2],
                               vazoes[3],
                               vazoes[4],
                               vazoes[5],
                                Environment.NewLine);

                    }
                    File.WriteAllText(Path.Combine(camSaida, prevsAnt), pa.ToString());
                }

                return prevsname;
            }
            catch (Exception e)
            {
                e.ToString();
                return "";
            }


        }

        public static string ExportaPrevsPorPasta(List<Propagacao> propagacoes, string camSaida, DateTime dataAtual, DateTime revdate, int? revnum = null)
        {
            try
            {
                var currRev = Tools.Tools.GetCurrRev(dataAtual);
                int indice = 0;

                int nextRevNum = 0;

                if (!revnum.HasValue)
                {
                    var nextRev = ChuvaVazaoTools.Tools.Tools.GetNextRev(dataAtual);
                    nextRevNum = nextRev.rev;
                }
                else
                    nextRevNum = revnum.Value;

                DateTime Data = DateTime.Today;
                var rev = ChuvaVazaoTools.Tools.Tools.GetCurrRev(Data);
                DateTime inicioMes = new DateTime(rev.revDate.Year, rev.revDate.Month, 1);//data da Rv0 do mês

                var semanaZero = inicioMes;

                while (semanaZero.DayOfWeek != DayOfWeek.Saturday)
                {
                    semanaZero = semanaZero.AddDays(-1);
                }
                semanaZero = semanaZero.AddDays(6);//termino da semana rv0 do mês

                var numSem = ChuvaVazaoTools.Tools.Tools.GetSemNumberAndYear(semanaZero);
                var SemanasPrevs = ChuvaVazaoTools.Tools.Tools.GetNumDatSem(semanaZero, numSem.Item1);// as doze semanasde previsão após execução do previvaz

                if (nextRevNum == 0 || (nextRevNum == 1 && currRev.rev != 0))
                {
                    DateTime dataRv0Mes2 = new DateTime(inicioMes.Year, inicioMes.AddMonths(1).Month, 1);
                    for (DateTime d = dataRv0Mes2; d <= SemanasPrevs.Last().Item1; d = d.AddDays(1))
                    {
                        if (SemanasPrevs.Select(x => x.Item1).Contains(d))
                        {
                            indice = SemanasPrevs.IndexOf(SemanasPrevs.Where(x => x.Item1.Equals(d)).First());
                            break;
                        }
                    }
                }
                else
                {
                    indice = 0;
                }
                var prevsname = "prevs.rv" + nextRevNum.ToString();


                if (File.Exists(Path.Combine(camSaida, prevsname)))
                {
                    File.Delete(Path.Combine(camSaida, prevsname));
                }

                StringBuilder sb = new StringBuilder();

                foreach (var p in propagacoes)
                {
                    List<Double> vazoes = new List<double>();

                    for (int i = indice; i <= indice + 5; i++)
                    {
                        var vaz = Math.Round(p.calMedSemanal[SemanasPrevs[i].Item1]);
                        vazoes.Add(vaz);
                    }
                    sb.AppendFormat("{0,6}{1,5}{2,10}{3,10}{4,10}{5,10}{6,10}{7,10}{8}",
                            p.IdPosto.ToString(),
                            p.IdPosto.ToString(),
                           vazoes[0],
                           vazoes[1],
                           vazoes[2],
                           vazoes[3],
                           vazoes[4],
                           vazoes[5],
                            Environment.NewLine);

                }
                File.WriteAllText(Path.Combine(camSaida, prevsname), sb.ToString());
                return prevsname;
            }
            catch (Exception e)
            {
                e.ToString();
                return "";
            }


        }

        // Remoção Antiga 
        public void Run(System.IO.TextWriter logF, out string runId, EnumRemo offset = EnumRemo.RemocaoAtual)
        {
            dtAtual.Value = DateTime.Today.Date;

            runId = null;
            if (logF != null) logF.WriteLine("INICIANDO RODADA AUTOMÁTICA");

            var name = "CV";


            var runRev = ChuvaVazaoTools.Tools.Tools.GetNextRev(dtAtual.Value,
                (offset == EnumRemo.RemocaoDuasSemanasEuro || offset == EnumRemo.RemocaoDuasSemanasGEFS || offset == EnumRemo.RemocaoDuasSemanasGFS || offset == EnumRemo.RemocaoDuasSemanasGFS2x) ? 2 : 1
                );

            var currRev = ChuvaVazaoTools.Tools.Tools.GetCurrRev(dtAtual.Value);


            IPrecipitacaoForm frm = null;
            frm = WaitForm2.CreateInstance(dtAtual.Value);
            string horaPrev = "";

            try
            {
                if (frm.TemEta00 && frm.TemGefs00)
                {
                    AddLog("CONJUNTO 00");
                }
                else if (frm.TemGefs00)
                {
                    AddLog("GEFS 00");
                    horaPrev = "_GEFS";
                }
                else
                {
                    if (logF != null) logF.WriteLine("Previões para o dia não encontradas - ENCERRANDO");
                    AddLog("Previões para o dia não encontradas");
                    return;
                }
            }
            catch { }

            name = name + horaPrev;

            PreencherVazObservada(out DateTime dataModelo, out string fonteVaz);

            name = name + "_" + fonteVaz.ToUpper();



            //CarregarPrecReal(dtAtual.Value.Date, out string modeloPrecReal);
            CarregarPrecRealMedia(dtAtual.Value.Date, out string modeloPrecReal);

            if (modeloPrecReal.EndsWith("-1"))
            {
                AddLog("Chuva realizada do dia ainda não está disponível");
                return;
            }

            name = name + "_" + modeloPrecReal;

            if (dataModelo < dtAtual.Value.Date.AddDays(-1))
            {
                name = name + "_d-1";
            }

            if (offset == EnumRemo.RemocaoUmaSemana)
            {
                name = name + "_VIES_VE";
            }
            else if (offset == EnumRemo.RemocaoUmaSemanaEuro)
            {
                name = name + "_EURO";
            }
            else if (offset == EnumRemo.RemocaoDuasSemanasEuro)
            {
                name = name + "_EURO";
                name = name.Replace("_" + modeloPrecReal, "").Replace("CV_", "CV2_");
            }
            else if (offset == EnumRemo.RemocaoDuasSemanasGEFS)
            {
                name = name + "_GEFS";
                name = name.Replace("_" + modeloPrecReal, "").Replace("CV_", "CV2_");
            }
            else if (offset == EnumRemo.RemocaoDuasSemanasGFS)
            {
                name = name + "_GFS";
                name = name.Replace("_" + modeloPrecReal, "").Replace("CV_", "CV2_");
            }
            else if (offset == EnumRemo.RemocaoDuasSemanasGFS2x)
            {
                name = name + "_GFS2x";
                name = name.Replace("_" + modeloPrecReal, "").Replace("CV_", "CV2_");
            }

            // var pastaSaida = @"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\16_Chuva_Vazao\" + runRev.revDate.ToString("yyyy_MM") + @"\RV" + runRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\" + name;
            var pastaSaida = @"C:\Files\Middle - Preço\16_Chuva_Vazao\" + runRev.revDate.ToString("yyyy_MM") + @"\RV" + runRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\" + name;
            //var pastaSaida = @"C:\temp\" + runRev.revDate.ToString("yyyy_MM") + @"\RV" + runRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\" + name;

            //  var pastaBase = @"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\Acompanhamento de vazões\" + currRev.revDate.ToString("MM_yyyy") + @"\Dados_de_Entrada_e_Saida_" + currRev.revDate.ToString("yyyyMM") + "_RV" + currRev.rev.ToString();
            var pastaBase = @"C:\Files\Middle - Preço\Acompanhamento de vazões\" + currRev.revDate.ToString("MM_yyyy") + @"\Dados_de_Entrada_e_Saida_" + currRev.revDate.ToString("yyyyMM") + "_RV" + currRev.rev.ToString();

            var statusF = new RunStatus(pastaSaida);
            if (statusF.Creation == RunStatus.statuscode.initialialized
                || statusF.Previvaz == RunStatus.statuscode.initialialized
                || statusF.PostProcessing == RunStatus.statuscode.initialialized
                || statusF.Preparation == RunStatus.statuscode.initialialized
                || statusF.Execution == RunStatus.statuscode.initialialized
                )
            {
                AddLog("Caso em execução: " + name);
                if (logF != null) logF.WriteLine("Caso em execução: " + name);
                return;
            }

            if ((System.IO.Directory.Exists(pastaSaida) && System.IO.File.Exists(Path.Combine(pastaSaida, "resumoENA.gif"))) &&
                statusF.PostProcessing == RunStatus.statuscode.completed)
            {
                AddLog("Caso já executado para essa data: " + name);
                if (logF != null) logF.WriteLine("Caso já executado para essa data: " + name);

                runId = "OK - " + name;
                return;
            }



            runId = name;

            if (logF != null) logF.WriteLine("INICIANDO RODADA: " + name);

            //var pastaBase = @"C:\Files\Middle - Preço\Acompanhamento de vazões\12_2018\Dados_de_Entrada_e_Saida_201812_RV0";


            if (!Directory.Exists(pastaBase) || !(Directory.Exists(System.IO.Path.Combine(pastaBase, "Modelos_Chuva_Vazao")) && Directory.Exists(System.IO.Path.Combine(pastaBase, "Previvaz", "Arq_Entrada")) && System.IO.Directory.GetFiles(pastaBase, "prevs.*", SearchOption.AllDirectories).Length > 0))
            {
                if (logF != null) logF.WriteLine("Arquivos de entrada nao disponiveis");
                return;
            }

            this.ArquivosDeEntradaModelo = System.IO.Path.Combine(pastaBase, "Modelos_Chuva_Vazao_Shadow");
            this.ArquivosDeEntradaPrevivaz = System.IO.Path.Combine(pastaBase, "Previvaz", "Arq_Entrada");
            this.ArquivoPrevsBase = System.IO.Directory.GetFiles(pastaBase, "prevs.*", SearchOption.AllDirectories)[0];
            this.DataSemanaPrevsBase = currRev.revDate;

            ArquivosDeSaida = pastaSaida;


            if (!System.IO.Directory.Exists(pastaSaida) || statusF.Creation != RunStatus.statuscode.completed)
            {
                CriarCaso(statusF);
            }

            if (!System.IO.File.Exists(Path.Combine(pastaSaida, "chuvamedia.log")) || statusF.Preparation != RunStatus.statuscode.completed)
            {
                statusF.Preparation = RunStatus.statuscode.initialialized;

                Ler();

                CarregarPrecObserv();
                PreencherPrecObserv();

                //btnConsultarVazObserv_Click(sender, e);
                PreencherVazObservada(out _, out _);
                //{

                //    if (offset == EnumRemo.RemocaoUmaSemanaEuro) EuroSem(frm);
                //    else if (offset == EnumRemo.RemocaoDuasSemanasEuro) EuroSemGefsCom(frm);
                //    else if (offset == EnumRemo.RemocaoDuasSemanasGFS2x) GfsComGfsCom(frm);
                //    else if (offset == EnumRemo.RemocaoDuasSemanasGFS) GfsSemGefsCom(frm);
                //    else if (offset == EnumRemo.RemocaoDuasSemanasGEFS) GefsSemGefsCom(frm);
                //    else
                //    {
                //        var funcLogs = new Action<string>(hora =>
                //        {
                //            //var eta = hora.Contains("00") ? WaitForm2.TipoEta._00h : WaitForm2.TipoEta._12h;
                //            //var gefs = hora.Contains("00") ? WaitForm2.TipoGefs._00h : WaitForm2.TipoGefs._12h;

                //            frm.LimparCache();
                //            frm.Eta = WaitForm2.TipoEta._00h;
                //            frm.Gefs = WaitForm2.TipoGefs._00h;
                //            frm.Tipo = hora.Contains("GEFS") ? WaitForm.TipoConjunto.Gefs : WaitForm.TipoConjunto.Conjunto;
                //            frm.SalvarDados = false;

                //            if (offset == EnumRemo.RemocaoUmaSemana)
                //            {
                //                var dtRemocao = dtAtual.Value;
                //                while (dtRemocao.DayOfWeek != DayOfWeek.Thursday) dtRemocao = dtRemocao.AddDays(1);

                //                frm.DateRemocao = dtRemocao;
                //                frm.sobrescreverCB = true;
                //            }
                //            else if (dtAtual.Value.DayOfWeek == DayOfWeek.Friday ||
                //            dtAtual.Value.DayOfWeek == DayOfWeek.Saturday ||
                //            dtAtual.Value.DayOfWeek == DayOfWeek.Sunday)
                //            {
                //                frm.TodasAsPrevisoes = true;
                //            }

                //            var chuvasConjunto = frm.ProcessarConjunto();
                //            foreach (var c in chuvasConjunto)
                //            {
                //                chuvas[c.Key] = c.Value;
                //            }

                //            RefreshPrecipList();
                //        });
                //        funcLogs("00");
                //    }
                //}

                dtAtual.Value = dataModelo.AddDays(1);
                dtModelo.Value = dtAtual.Value.Date;
                Reiniciar(dtModelo.Value);

                AddLog(" --- ");
                AddLog(" --- Executar Parte B quando pronto --- ");

                PreencherPrecObserv();
                btnSalvarPrecObserv_Click(null, null);
                SalvarVazObserv();
                SalvarPrecPrev();

                if (!File.Exists(Path.Combine(pastaSaida, "chuvamedia.log")))
                    statusF.Preparation = RunStatus.statuscode.error;
                else
                    statusF.Preparation = RunStatus.statuscode.completed;
            }

            if (statusF.Preparation != RunStatus.statuscode.completed) return;

            if (statusF.Execution != RunStatus.statuscode.completed)
            {
                ExecutarTudo(statusF);
                if (statusF.Execution == RunStatus.statuscode.error)
                {
                    logF.WriteLine("Erro no SMAP");
                    return;
                }
            }

            if (statusF.Execution == RunStatus.statuscode.completed)
            {
                AddLog(" --- ");
                AddLog(" --- Parte B Concluída --- ");

                if (logF != null) logF.WriteLine("EXECUCAO OK - PRECESSANDO RESULTADOS");

                ProcessarResultados(pastaSaida, logF, runRev.rev, statusF);

                if (logF != null) logF.WriteLine("FINALIZADO");

                runId = "OK - " + name;
            }
            else
            {
                if (logF != null) logF.WriteLine("SMAPS NAO EXECUTADOS");
            }
        }

        // Remoção Utilizando Scripts R
        public void Run_R(System.IO.TextWriter logF, out string runId, EnumRemo offset = EnumRemo.RemocaoAtual)
        {
            dtAtual.Value = DateTime.Today.Date;
            cbx_Encadear_Previvaz.Checked = false;

            runId = null;
            if (logF != null) logF.WriteLine("INICIANDO RODADA AUTOMÁTICA MODELO R");


            var name = "CV";

            //Verifica a RV da rodada
            int incremento = 1;
            if ((offset == EnumRemo.RemocaoDuasSemanasEuro || offset == EnumRemo.RemocaoDuasSemanasEuro_op || offset == EnumRemo.RemocaoDuasSemanasGEFS || offset == EnumRemo.RemocaoDuasSemanasGFS || offset == EnumRemo.RemocaoDuasSemanasGFS2x))
            {
                incremento = 2;
            }
            else if ((offset == EnumRemo.RemocaoTresSemanasEuro || offset == EnumRemo.RemocaoTresSemanasGEFS))
            {
                incremento = 3;
            }
            else if ((offset == EnumRemo.RemocaoQuatroSemanasEuro || offset == EnumRemo.RemocaoQuatroSemanasGEFS))
            {
                incremento = 4;
            }

            var runRev = ChuvaVazaoTools.Tools.Tools.GetNextRev(dtAtual.Value, incremento);
            var currRev = ChuvaVazaoTools.Tools.Tools.GetCurrRev(dtAtual.Value);


            IPrecipitacaoForm frm = null;
            frm = WaitForm2.CreateInstance(dtAtual.Value);
            string horaPrev = "";

            /*          try
                      {
                          if (frm.TemEta00 && frm.TemGefs00)
                          {
                              AddLog("CONJUNTO 00");
                          }
                          else if (frm.TemGefs00)
                          {
                              AddLog("GEFS 00");
                              horaPrev = "_GEFS";
                          }
                          else
                          {
                              if (logF != null) logF.WriteLine("Previões para o dia não encontradas - ENCERRANDO");
                              AddLog("Previões para o dia não encontradas");
                              return;
                          }
                      }
                      catch { }*/
            //Nome para Rodada
            name = name + horaPrev;

            PreencherVazObservada(out DateTime dataModelo, out string fonteVaz);

            name = name + "_" + fonteVaz.ToUpper();

            var runRevMapas = ChuvaVazaoTools.Tools.Tools.GetNextRev(dtAtual.Value);

            //CarregarPrecReal(dtAtual.Value.Date, out string modeloPrecReal);
            CarregarPrecRealMedia(dtAtual.Value.Date, out string modeloPrecReal);// runRev.rev.ToString()

            //Pasta Onde os mapas de saída do R estão
            //var pastaMapa = @"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\16_Chuva_Vazao\" + runRevMapas.revDate.ToString("yyyy_MM") + @"\RV" + runRevMapas.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\Mapas Acomph"; //Mapas Acomph
            //var pastaRaiz = @"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\16_Chuva_Vazao\" + runRevMapas.revDate.ToString("yyyy_MM") + @"\RV" + runRevMapas.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\Mapas Acomph"; // "Mapas Acomph";
            var pastaMapa = @"C:\Files\Middle - Preço\16_Chuva_Vazao\" + runRevMapas.revDate.ToString("yyyy_MM") + @"\RV" + runRevMapas.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\Mapas Acomph"; //Mapas Acomph
            var pastaRaiz = @"C:\Files\Middle - Preço\16_Chuva_Vazao\" + runRevMapas.revDate.ToString("yyyy_MM") + @"\RV" + runRevMapas.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\Mapas Acomph"; // "Mapas Acomph";

            if (modeloPrecReal.EndsWith("-1"))
            {
                AddLog("Chuva realizada do dia ainda não está disponível");
                //return;
            }

            name = name + "_" + modeloPrecReal;

            Boolean d1 = false;
            Boolean exist_psat = File.Exists(Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de Precipitação\Observado_Satelite", dtAtual.Value.ToString("yyyy"), dtAtual.Value.ToString("MM"), "psat_" + dtAtual.Value.ToString("ddMMyyyy") + ".txt"));
            //Verifica Acomph
            if (dataModelo < dtAtual.Value.Date.AddDays(-1))
            {
                name = name + "_d-1";

                pastaRaiz = Path.Combine(pastaMapa + " d-1", "CV", "CV_FUNC");
                pastaMapa = (pastaMapa + " d-1");
                d1 = true;
            }
            else
            {
                pastaRaiz = Path.Combine(pastaMapa, "CV", "CV_FUNC");
            }
            // Seleciona o Modelo para Rodada
            if (offset == EnumRemo.RemocaoUmaSemana)
            {
                name = name + "_VIES_VE";
                pastaRaiz = Path.Combine(pastaMapa, "CV", "CV_VIES_VE");
            }
            else if (offset == EnumRemo.RemocaoUmaSemanaEuro)
            {
                name = name + "_EURO";
                pastaRaiz = Path.Combine(pastaMapa, "CV", "CV_EURO");
            }
            else if (offset == EnumRemo.RemocaoUmaSemanaEuro_op)
            {
                name = name + "_EUROop";
                pastaRaiz = Path.Combine(pastaMapa, "CV", "CV_EUROop");
            }
            else if (offset == EnumRemo.RemocaoUmaSemanaGFS)
            {
                name = name + "_GFS";

                pastaRaiz = Path.Combine(pastaMapa, "CV", "CV_GFS");
            }
            else if (offset == EnumRemo.RemocaoDuasSemanasEuro)
            {
                name = name + "_EURO";
                // name = name.Replace("_" + modeloPrecReal, "").Replace("CV_", "CV2_");
                name = name.Replace("CV_", "CV2_");
                pastaRaiz = Path.Combine(pastaMapa, "CV2", "CV2_EURO");
            }
            else if (offset == EnumRemo.RemocaoDuasSemanasEuro_op)
            {
                name = name + "_EUROop";
                // name = name.Replace("_" + modeloPrecReal, "").Replace("CV_", "CV2_");
                name = name.Replace("CV_", "CV2_");
                pastaRaiz = Path.Combine(pastaMapa, "CV2", "CV2_EUROop");
            }
            else if (offset == EnumRemo.RemocaoDuasSemanasGEFS)
            {
                name = name + "_GEFS";
                //  name = name.Replace("_" + modeloPrecReal, "").Replace("CV_", "CV2_");
                name = name.Replace("CV_", "CV2_");
                pastaRaiz = Path.Combine(pastaMapa, "CV2", "CV2_GEFS");
            }
            else if (offset == EnumRemo.RemocaoDuasSemanasGFS)
            {
                name = name + "_GFS";
                // name = name.Replace("_" + modeloPrecReal, "").Replace("CV_", "CV2_");
                name = name.Replace("CV_", "CV2_");
                pastaRaiz = Path.Combine(pastaMapa, "CV2", "CV2_GFS");
            }
            else if (d1 == true || exist_psat == true)
            {
                if (offset == EnumRemo.RemocaoAtual && name == "CV_ACOMPH_FUNC" && exist_psat == true && DateTime.Today.DayOfWeek == DayOfWeek.Thursday)
                {
                    name = name + "_PSAT";
                }
                else if (offset == EnumRemo.RemocaoTresSemanasGEFS)
                {
                    name = name + "_GEFS";
                    //  name = name.Replace("_" + modeloPrecReal, "").Replace("CV_", "CV2_");
                    name = name.Replace("CV_", "CV3_");
                    pastaRaiz = Path.Combine(pastaMapa, "CV3", "CV3_GEFS");

                    if (currRev.revDate.Month != runRev.revDate.Month)
                    {
                        cbx_Encadear_Previvaz.Checked = true;
                    }

                }
                else if (offset == EnumRemo.RemocaoTresSemanasEuro)
                {
                    name = name + "_EURO";
                    //  name = name.Replace("_" + modeloPrecReal, "").Replace("CV_", "CV2_");
                    name = name.Replace("CV_", "CV3_");
                    pastaRaiz = Path.Combine(pastaMapa, "CV3", "CV3_EURO");
                    if (currRev.revDate.Month != runRev.revDate.Month)
                    {
                        cbx_Encadear_Previvaz.Checked = true;
                    }
                }
                else if (offset == EnumRemo.RemocaoQuatroSemanasGEFS)
                {
                    name = name + "_GEFS";
                    //  name = name.Replace("_" + modeloPrecReal, "").Replace("CV_", "CV2_");
                    name = name.Replace("CV_", "CV4_");
                    pastaRaiz = Path.Combine(pastaMapa, "CV4", "CV4_GEFS");
                    if (currRev.revDate.Month != runRev.revDate.Month)
                    {
                        cbx_Encadear_Previvaz.Checked = true;
                    }
                }
                else if (offset == EnumRemo.RemocaoQuatroSemanasEuro)
                {
                    name = name + "_EURO";
                    //  name = name.Replace("_" + modeloPrecReal, "").Replace("CV_", "CV2_");
                    name = name.Replace("CV_", "CV4_");
                    pastaRaiz = Path.Combine(pastaMapa, "CV4", "CV4_EURO");
                    if (currRev.revDate.Month != runRev.revDate.Month)
                    {
                        cbx_Encadear_Previvaz.Checked = true;
                    }
                }
                else if (offset != EnumRemo.RemocaoAtual)
                {
                    return;
                }
                
            }
            else if (offset != EnumRemo.RemocaoAtual )
            {
                return;
            }

                //@"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\
                //Diretorio de destino das Rodadas
                // var pastaSaida = @"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\16_Chuva_Vazao\" + runRev.revDate.ToString("yyyy_MM") + @"\RV" + runRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\" + name;

                var pastaSaida = @"C:\Files\Middle - Preço\16_Chuva_Vazao\" + runRev.revDate.ToString("yyyy_MM") + @"\RV" + runRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\" + name;

            //var pastaBase = @"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\Acompanhamento de vazões\" + currRev.revDate.ToString("MM_yyyy") + @"\Dados_de_Entrada_e_Saida_" + currRev.revDate.ToString("yyyyMM") + "_RV" + currRev.rev.ToString();
            var pastaBase = @"C:\Files\Middle - Preço\Acompanhamento de vazões\" + currRev.revDate.ToString("MM_yyyy") + @"\Dados_de_Entrada_e_Saida_" + currRev.revDate.ToString("yyyyMM") + "_RV" + currRev.rev.ToString();


            var statusF = new RunStatus(pastaSaida);
            if (statusF.Creation == RunStatus.statuscode.initialialized
                || statusF.Previvaz == RunStatus.statuscode.initialialized
                || statusF.PostProcessing == RunStatus.statuscode.initialialized
                || statusF.Preparation == RunStatus.statuscode.initialialized
                || statusF.Execution == RunStatus.statuscode.initialialized
                )
            {
                AddLog("Caso em execução: " + name);
                if (logF != null) logF.WriteLine("Caso em execução: " + name);
                return;
            }

            if ((System.IO.Directory.Exists(pastaSaida) && System.IO.File.Exists(Path.Combine(pastaSaida, "resumoENA.gif"))) &&
                statusF.PostProcessing == RunStatus.statuscode.completed)
            {
                AddLog("Caso já executado para essa data: " + name);
                if (logF != null) logF.WriteLine("Caso já executado para essa data: " + name);

                runId = "OK - " + name;
                return;
            }



            runId = name;

            if (logF != null) logF.WriteLine("INICIANDO RODADA: " + name);

            //var pastaBase = @"C:\Files\Middle - Preço\Acompanhamento de vazões\12_2018\Dados_de_Entrada_e_Saida_201812_RV0";


            if (!Directory.Exists(pastaBase) || !(Directory.Exists(System.IO.Path.Combine(pastaBase, "Modelos_Chuva_Vazao")) && Directory.Exists(System.IO.Path.Combine(pastaBase, "Previvaz", "Arq_Entrada")) && System.IO.Directory.GetFiles(pastaBase, "prevs.*", SearchOption.AllDirectories).Length > 0))
            {
                if (logF != null) logF.WriteLine("Arquivos de entrada nao disponiveis");
                return;
            }

            this.ArquivosDeEntradaModelo = System.IO.Path.Combine(pastaBase, "Modelos_Chuva_Vazao_Shadow");
            this.ArquivosDeEntradaPrevivaz = System.IO.Path.Combine(pastaBase, "Previvaz", "Arq_Entrada");
            this.ArquivoPrevsBase = System.IO.Directory.GetFiles(pastaBase, "prevs.*", SearchOption.AllDirectories)[0];
            this.DataSemanaPrevsBase = currRev.revDate;

            ArquivosDeSaida = pastaSaida;


            if (!System.IO.Directory.Exists(pastaSaida) || statusF.Creation != RunStatus.statuscode.completed)
            {
                CriarCaso(statusF);
                string user = System.Environment.UserName.ToString();
                Tools.Tools.addHistory(Path.Combine(pastaSaida, user + ".txt"), System.Environment.UserName.ToString());
            }

            if (statusF.Preparation != RunStatus.statuscode.completed)
            {
                statusF.Preparation = RunStatus.statuscode.initialialized;

                Ler();

                CarregarPrecObserv();
                PreencherPrecObserv();

                PreencherVazObservada(out DateTime dtVaz, out _);

                dtAtual.Value = dataModelo.AddDays(1);
                dtModelo.Value = dtAtual.Value.Date;
                Reiniciar(dtModelo.Value);

                if (!Directory.Exists(pastaRaiz))
                    statusF.Preparation = RunStatus.statuscode.error;
                else
                {
                    PrecipitacaoPrevista_R(pastaRaiz, pastaSaida);

                    //Chamar Imagens Aqui ######################################

                    AddLog(" --- ");
                    AddLog(" --- Executar Parte B quando pronto --- ");


                    //Renomear_Eta40();
                    PreencherPrecObserv();
                    SalvarPrecObserv_R();
                    // btnSalvarPrecObserv_Click(null, null);
                    SalvarVazObserv();
                    SalvarPrecPrev_R();



                    statusF.Preparation = RunStatus.statuscode.completed;
                }
            }

            if (statusF.Preparation != RunStatus.statuscode.completed) return;

            if (statusF.Execution != RunStatus.statuscode.completed)
            {
                ExecutarTudo(statusF);
                if (statusF.Execution == RunStatus.statuscode.error)
                {
                    logF.WriteLine("Erro no SMAP");
                    return;
                }
                if (File.Exists(Path.Combine(pastaSaida, "error.log")))
                {
                    statusF.Execution = RunStatus.statuscode.error;
                    logF.WriteLine("Erro no SMAP");
                    return;

                }
            }

            if (statusF.Execution == RunStatus.statuscode.completed)
            {
                AddLog(" --- ");
                AddLog(" --- Parte B Concluída --- ");

                if (logF != null) logF.WriteLine("EXECUCAO OK - PRECESSANDO RESULTADOS");

                ProcessarResultados(pastaSaida, logF, runRev.rev, statusF);

                if (logF != null) logF.WriteLine("FINALIZADO");

                runId = "OK - " + name;
            }
            else
            {
                if (logF != null) logF.WriteLine("SMAPS NAO EXECUTADOS");
            }
            try
            {
                Salvar_Img(pastaSaida);
            }
            catch (Exception ex) { }

            foreach (System.Diagnostics.Process proc in System.Diagnostics.Process.GetProcessesByName("Excel"))
            {
                if (proc.MainWindowHandle == this.pointer)
                {
                    proc.Kill();
                }
            }

        }


        public void Run_Manual(bool rodarPrevivaz = true)
        {
            var logF = textLogger;
            var searchPath = "";
            Boolean Falha = false;

            Ookii.Dialogs.VistaFolderBrowserDialog d = new Ookii.Dialogs.VistaFolderBrowserDialog();

            //d.SelectedPath = System.IO.Path.Combine(Config.CaminhoPrevisao, data.ToString("yyyyMM"), data.ToString("dd"));
            //d.SelectedPath = System.IO.Path.Combine(@"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\16_Chuva_Vazao\");
            d.SelectedPath = System.IO.Path.Combine(@"C:\Files\Middle - Preço\16_Chuva_Vazao\");
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                searchPath = d.SelectedPath;

            }
            else
            {
                return;

            }
            var revnum = 0;
            var rev = new Ookii.Dialogs.InputDialog();
            rev.MainInstruction = "Numero da revisão";
            rev.MaxLength = 1;

            if (rev.ShowDialog() == DialogResult.OK)
            {
                revnum = int.Parse(rev.Input);
            }


            var dir_saida = txtCaminho.Text;
            var dirs_mapas = Directory.GetDirectories(searchPath);
            int conta = 0;
            do
            {

                foreach (string dir_mapa in dirs_mapas)
                {
                    var arquivosdats = Directory.GetFiles(dir_mapa, "*.dat");

                    if (arquivosdats.Length > 0)
                    {

                        // dtAtual.Value = DateTime.Today.Date.AddDays(-3);
                        dtAtual.Value = dtAtual.Value;

                        var nome_pasta = dir_mapa.Split('\\').Last();
                        var name = nome_pasta;
                        if (!nome_pasta.Contains("CV"))
                        {
                            name = "CV_" + nome_pasta;
                        }





                        //if (!Directory.Exists(Path.Combine(dir_saida, name)))
                        //{

                        //Verifica a RV da rodada
                        var runRev = ChuvaVazaoTools.Tools.Tools.GetNextRev(dtAtual.Value);

                        var currRev = ChuvaVazaoTools.Tools.Tools.GetCurrRev(dtAtual.Value);


                        IPrecipitacaoForm frm = null;
                        frm = WaitForm2.CreateInstance(dtAtual.Value);





                        PreencherVazObservada(out DateTime dataModelo, out string fonteVaz);



                        var runRevMapas = ChuvaVazaoTools.Tools.Tools.GetNextRev(dtAtual.Value);

                        //CarregarPrecReal(dtAtual.Value.Date, out string modeloPrecReal);
                        CarregarPrecRealMedia(dtAtual.Value.Date, out string modeloPrecReal);// runRev.rev.ToString()

                        //Pasta Onde os mapas de saída do R estão

                        var pastaRaiz = dir_mapa;



                        //Diretorio de destino das Rodadas
                        var pastaSaida = Path.Combine(dir_saida, name);
                        //var pastaSaida = @"C:\temp\" + runRev.revDate.ToString("yyyy_MM") + @"\RV" + runRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\" + name;

                        //var pastaBase = @"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\Acompanhamento de vazões\" + currRev.revDate.ToString("MM_yyyy") + @"\Dados_de_Entrada_e_Saida_" + currRev.revDate.ToString("yyyyMM") + "_RV" + currRev.rev.ToString();
                        var pastaBase = @"C:\Files\Middle - Preço\Acompanhamento de vazões\" + currRev.revDate.ToString("MM_yyyy") + @"\Dados_de_Entrada_e_Saida_" + currRev.revDate.ToString("yyyyMM") + "_RV" + currRev.rev.ToString();



                        //var pastaBase = @"C:\Files\Middle - Preço\Acompanhamento de vazões\12_2018\Dados_de_Entrada_e_Saida_201812_RV0";


                        this.ArquivosDeEntradaModelo = System.IO.Path.Combine(txtEntrada.Text);//Modelos_Chuva_Vazao
                        this.ArquivosDeEntradaPrevivaz = System.IO.Path.Combine(pastaBase, "Previvaz", "Arq_Entrada");
                        this.ArquivoPrevsBase = System.IO.Directory.GetFiles(pastaBase, "prevs.*", SearchOption.AllDirectories)[0];
                        this.DataSemanaPrevsBase = currRev.revDate;

                        ArquivosDeSaida = pastaSaida;

                        var statusF = new RunStatus(pastaSaida);
                        if (statusF.Creation == RunStatus.statuscode.initialialized
                            || statusF.Previvaz == RunStatus.statuscode.initialialized
                            || statusF.PostProcessing == RunStatus.statuscode.initialialized
                            || statusF.Preparation == RunStatus.statuscode.initialialized
                            || statusF.Execution == RunStatus.statuscode.initialialized
                            )
                        {
                            AddLog("Caso em execução: " + name);
                            if (logF != null) logF.WriteLine("Caso em execução: " + name);
                            //return;
                        }
                        else
                        {

                            if ((System.IO.Directory.Exists(pastaSaida) && System.IO.File.Exists(Path.Combine(pastaSaida, "resumoENA.gif"))) &&
                        statusF.PostProcessing == RunStatus.statuscode.completed)
                            {
                                AddLog("Caso já executado para essa data: " + name);
                                logF.WriteLine("Caso já executado para essa data: " + name);


                                //return;
                            }
                            else
                            {
                                logF.WriteLine("Iniciando " + name);

                                if (!System.IO.Directory.Exists(pastaSaida) || statusF.Creation != RunStatus.statuscode.completed)
                                {
                                    statusF.Creation = RunStatus.statuscode.initialialized;
                                    CriarCaso();
                                    statusF.Creation = RunStatus.statuscode.completed;
                                }

                                if (statusF.Preparation != RunStatus.statuscode.completed)
                                {
                                    statusF.Preparation = RunStatus.statuscode.initialialized;

                                    try
                                    {
                                        Ler();

                                        CarregarPrecObserv();
                                        PreencherPrecObserv();

                                        PreencherVazObservada(out DateTime dtVaz, out _);

                                        dtAtual.Value = dataModelo.AddDays(1);
                                        dtModelo.Value = dtAtual.Value.Date;
                                        Reiniciar(dtModelo.Value);


                                        PrecipitacaoPrevista_R(pastaRaiz, pastaSaida);

                                        PreencherPrecObserv();
                                        SalvarPrecObserv_R();
                                        SalvarVazObserv();
                                        SalvarPrecPrev_R();

                                        statusF.Preparation = RunStatus.statuscode.completed;
                                    }
                                    catch
                                    {
                                        statusF.Preparation = RunStatus.statuscode.error;
                                        Falha = true;
                                    }
                                }
                                if (statusF.Execution != RunStatus.statuscode.completed)
                                {
                                    statusF.Execution = RunStatus.statuscode.initialialized;
                                    logF.WriteLine("EXECUTANDO");
                                    try
                                    {
                                        ExecutarTudo_Manual();
                                        if (statusF.Execution == RunStatus.statuscode.error)
                                        {
                                            logF.WriteLine("Erro no SMAP");
                                            return;
                                        }
                                        statusF.Execution = RunStatus.statuscode.completed;
                                    }
                                    catch
                                    {
                                        statusF.Execution = RunStatus.statuscode.error;

                                    }
                                }
                                if (statusF.Execution == RunStatus.statuscode.completed)
                                {


                                    logF.WriteLine("PROCESSANDO RESULTADOS");
                                    try
                                    {
                                        ProcessarResultadosManual(pastaSaida, logF, revnum, statusF, rodarPrevivaz);

                                    }
                                    catch
                                    {


                                    }
                                }
                                else
                                {
                                    if (logF != null) logF.WriteLine("SMAPS NAO EXECUTADOS");
                                }

                                if (statusF.Creation == RunStatus.statuscode.error
                                || statusF.Previvaz == RunStatus.statuscode.error
                                || statusF.PostProcessing == RunStatus.statuscode.error
                                || statusF.Preparation == RunStatus.statuscode.error
                                || statusF.Execution == RunStatus.statuscode.error
                                || statusF.Collect == RunStatus.statuscode.error
                                )
                                {
                                    Falha = true;
                                }
                                else
                                {
                                    logF.WriteLine("FINALIZADO");
                                }



                            }
                        }
                    }

                }
                if (Falha == true)
                {
                    conta++;
                }
            } while (Falha == true && conta < 2);
            logF.WriteLine("Rodadas Finalizadas");
        }

        public void Reiniciar(DateTime dataPrevisao)
        {
            foreach (var modelo in modelosChVz)
            {
                if (modelo.DataPrevisao != dataPrevisao)
                {
                    modelo.DataPrevisao = dataPrevisao;
                    if (modelo is SMAP.ModeloSmap)
                    {
                        ((SMAP.ModeloSmap)modelo).SubBacias.ForEach(x => x.ReiniciarParametros());
                    }
                    modelo.SalvarParametros();
                }
            }
            AddLog("- Modelos Iniciados para o dia: " + dtAtual.Value.Date.ToShortDateString());
        }


        private void btnReinicar_Click(object sender, EventArgs e)
        {
            dtModelo.Value = dtAtual.Value.Date;
            Reiniciar(dtModelo.Value);
        }

        public void CarregarPrecObserv()
        {

            for (DateTime data = dtModelo.Value.Date; data <= dtAtual.Value.Date; data = data.AddDays(1))
            {
                //CarregarPrecReal(data, out _);
                chuvas[data] = CarregarPrecRealMedia(data, out _);

            }

            RefreshPrecipList();

            AddLog("- Precipitação MERGE Carregada");
        }

        public void CarregarPrecReal(DateTime data, out string modelo)
        {
            var mergeCtlFile = System.IO.Directory.GetFiles(Config.CaminhoMerge, "prec_" + data.ToString("yyyyMMdd") + ".ctl", System.IO.SearchOption.AllDirectories);
            var mergeDatFile = System.IO.Directory.GetFiles(Config.CaminhoMerge, "prec_" + data.ToString("yyyyMMdd") + ".dat", System.IO.SearchOption.AllDirectories);
            if (mergeCtlFile.Length > 0)
            {
                AddLog(mergeCtlFile[0]);

                var prec = PrecipitacaoFactory.BuildFromMergeFile(mergeCtlFile[0]);
                prec.Descricao = "MERGE - " + System.IO.Path.GetFileNameWithoutExtension(mergeCtlFile[0]);
                prec.Data = data;


                chuvas[data] = prec;

                modelo = "MERGE";

            }
            else if (mergeDatFile.Length > 0)
            {
                AddLog(mergeDatFile[0]);

                var prec = PrecipitacaoFactory.BuildFromEtaFile(mergeDatFile[0]);
                prec.Descricao = "MERGE - " + System.IO.Path.GetFileNameWithoutExtension(mergeDatFile[0]);
                prec.Data = data;

                chuvas[data] = prec;

                modelo = "MERGE";
            }
            else
            {
                AddLog("\tmerge para a data " + data.ToShortDateString() + " não encontrado");

                var funcfile = System.IO.Path.Combine(Config.CaminhoFunceme, data.Year.ToString("0000"), data.Month.ToString("00"), "funceme_" + data.ToString("yyyyMMdd") + ".ctl");

                if (
                    System.IO.File.Exists(funcfile) && (runAuto ||
                    MessageBox.Show("Merge para a data " + data.ToShortDateString() + " não encontrado.\r\nUsar funceme?", "Precip Observada - Chuva Vazão", MessageBoxButtons.YesNo)
                    == DialogResult.Yes))
                {
                    var prec = PrecipitacaoFactory.BuildFromMergeFile(funcfile);
                    prec.Descricao = System.IO.Path.GetFileNameWithoutExtension(funcfile);
                    prec.Data = data;
                    chuvas[data] = prec;
                    chuvas[data].Data = data;

                    modelo = "FUNCEME";
                }
                else if (runAuto || MessageBox.Show("Merge para a data " + data.ToShortDateString() + " não encontrado.\r\nUsar merge anterior?", "Precip Observada - Chuva Vazão", MessageBoxButtons.YesNo)
                    == DialogResult.Yes)
                {
                    chuvas[data] = chuvas[data.AddDays(-1)].Duplicar();
                    chuvas[data].Data = data;

                    modelo = "MER-1";
                }
                else modelo = "NULO";
            }
        }

        public void SalvarVazObserv()
        {
            modelosChVz.ForEach(x => x.SalvarVazaoObservada());

            AddLog("- Arquivos de Vazao Observada Salva");
        }

        public Precipitacao CarregarPrecRealMedia(DateTime data, out string modelo)
        {
            Precipitacao merge = null;
            Precipitacao funceme = null;
            Precipitacao ons = null;
            DateTime data_Atual = DateTime.Today;
            var runRev = ChuvaVazaoTools.Tools.Tools.GetCurrRev(data);
            //MERGE
            {

                var mergeCtlFile = System.IO.Directory.GetFiles(Config.CaminhoMerge, "prec_" + data.ToString("yyyyMMdd") + ".ctl", System.IO.SearchOption.AllDirectories);
                var mergeDatFile = System.IO.Directory.GetFiles(Config.CaminhoMerge, "prec_" + data.ToString("yyyyMMdd") + ".dat", System.IO.SearchOption.AllDirectories);
                if (mergeCtlFile.Length > 0)
                {
                    AddLog(mergeCtlFile[0]);

                    var prec = PrecipitacaoFactory.BuildFromMergeFile(mergeCtlFile[0]);
                    prec.Descricao = "MERGE - " + System.IO.Path.GetFileNameWithoutExtension(mergeCtlFile[0]);
                    prec.Data = data;


                    merge = prec;


                }
                else if (mergeDatFile.Length > 0)
                {
                    AddLog(mergeDatFile[0]);

                    var prec = PrecipitacaoFactory.BuildFromEtaFile(mergeDatFile[0]);
                    prec.Descricao = "MERGE - " + System.IO.Path.GetFileNameWithoutExtension(mergeDatFile[0]);
                    prec.Data = data;

                    merge = prec;

                }

            }

            //FUNCEME
            {
                var funcfile = System.IO.Path.Combine(Config.CaminhoFunceme, data.Year.ToString("0000"), data.Month.ToString("00"), "funceme_" + data.ToString("yyyyMMdd") + ".ctl");

                if (System.IO.File.Exists(funcfile))
                {
                    var prec = PrecipitacaoFactory.BuildFromMergeFile(funcfile);
                    prec.Descricao = System.IO.Path.GetFileNameWithoutExtension(funcfile);
                    prec.Data = data;
                    funceme = prec;
                    funceme.Data = data;

                }
            }

            //ONS
            {
                try
                {
                    var onsFile = $"C:\\Files\\Trading\\Acompanhamento Metereologico Semanal\\spiderman\\{data.ToString("yyyy_MM_dd")}\\ETA\\observado.gif";

                    if (merge != null && File.Exists(onsFile))
                    {
                        ons = PrecipitacaoFactory.BuildFromImage(onsFile);
                    }
                }
                finally { }
            }

            if (funceme == null && merge == null)
            {
                var ret = CarregarPrecRealMedia(data.AddDays(-1), out modelo);
                modelo += "-1";
                return ret;
            }
            else if (funceme == null)
            {
                if (ons != null)
                {
                    Precipitacao media = merge.Duplicar();

                    foreach (var v in merge.Prec.Keys)
                    {
                        var vf = ons[v];

                        if (vf > 0)
                        {
                            media[v] = (media[v] + vf) / 2;
                        }
                    }

                    media.Descricao = "Media Ons e Merge";
                    modelo = "ONSeMERG";
                    return media;
                }

                modelo = "MERG";
                return merge;
            }

            //else if(File.Exists(Path.Combine(@"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\Acompanhamento de vazões", runRev.revDate.ToString("MM_yyyy"), @"Dados_de_Entrada_e_Saida_"+ runRev.revDate.ToString("yyyyMM")+"_RV"+runRev.rev, "Modelos_Chuva_Vazao_"+ data_Atual.ToString("yyyyMMdd")+".zip")))//"Modelos_Chuva_Vazao_"+ data_Atual.ToString("yyyyMMdd")+".zip"
            // else if(File.Exists(Path.Combine(@"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\Acompanhamento de vazões", runRev.revDate.ToString("MM_yyyy"), @"Dados_de_Entrada_e_Saida_"+ runRev.revDate.ToString("yyyyMM")+"_RV"+runRev.rev, @"Modelos_Chuva_Vazao\CPINS\Arq_Saida", data_Atual.ToString("dd-MM-yyyy")+"_PLANILHA_USB.txt")))
            else if (File.Exists(Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de vazões", runRev.revDate.ToString("MM_yyyy"), @"Dados_de_Entrada_e_Saida_" + runRev.revDate.ToString("yyyyMM") + "_RV" + runRev.rev, @"Modelos_Chuva_Vazao\CPINS\Arq_Saida", data_Atual.ToString("dd-MM-yyyy") + "_PLANILHA_USB.txt")))
            {

                if (merge != null)
                {
                    modelo = "Merge_Atualizado";
                    return merge;
                }
                else
                {
                    if (DateTime.Today.DayOfWeek == DayOfWeek.Sunday)
                    {
                        modelo = "FUNC_Atualizado";
                        
                    }
                    else
                    {
                        modelo = "FUNC";
                    }
                    return funceme;
                }
            }
            else if (merge == null)
            {
                modelo = "FUNC";
                return funceme;
            }
            //else if (merge != null)
            //{
            //    modelo = "MERG";
            //    return merge;
            //}
            else
            {
                modelo = "FUNC";
                return funceme;
            }
            /*  else if (ons == null)
              {
                  Precipitacao media = merge.Duplicar();

                  foreach (var v in merge.Prec.Keys)
                  {
                      var vf = funceme[v];

                      if (vf > 0)
                      {
                          media[v] = (media[v] + vf) / 2;
                      }
                  }

                  media.Descricao = "Media Func e Merge";
                  modelo = "FUNCeMERG";
                  return media;
              }
              else
              {
                  Precipitacao media = merge.Duplicar();

                  foreach (var v in merge.Prec.Keys)
                  {
                      var vf = funceme[v];
                      var vo = ons[v];

                      media[v] = (media[v] + vf + vo) / 3;

                  }

                  media.Descricao = "Media Func, Merge e ONS";
                  modelo = "FUNCeMERGeONS";
                  return media;
              }*/
        }

        public void SelecionarSaida()
        {
            Ookii.Dialogs.VistaFolderBrowserDialog ofd = new Ookii.Dialogs.VistaFolderBrowserDialog();

            var currRevDate = dtAtual.Value;

            var nextRev = ChuvaVazaoTools.Tools.Tools.GetNextRev(currRevDate);
            // ofd.SelectedPath = @"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\16_Chuva_Vazao\" + nextRev.revDate.ToString("yyyy_MM") + @"\RV" + nextRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\"; ;
            ofd.SelectedPath = @"C:\Files\Middle - Preço\16_Chuva_Vazao\" + nextRev.revDate.ToString("yyyy_MM") + @"\RV" + nextRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\"; ;

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ArquivosDeSaida = ofd.SelectedPath;
            }
        }

        void CriarCaso(RunStatus statusF = null)
        {

            if (statusF != null) statusF.Creation = RunStatus.statuscode.initialialized;

            var modelos = new string[] { "SMAP" };

            var dir = System.IO.Directory.GetDirectories(txtEntrada.Text);

            if (!Directory.Exists(txtCaminho.Text)) Directory.CreateDirectory(txtCaminho.Text);

            foreach (var d in dir)
            {
                var name = d.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Last().ToUpperInvariant();

                if (modelos.Contains(name))
                {

                    AddLog("\tCopiando modelo: " + name);

                    if (Directory.Exists(Path.Combine(txtCaminho.Text, name))) Directory.Delete(Path.Combine(txtCaminho.Text, name), true);

                    SMAPDirectoryCopy(d, Path.Combine(txtCaminho.Text, name), true);
                }
            }

            if (statusF != null) statusF.Creation = RunStatus.statuscode.completed;
        }

        public void PreencherPrecObserv()
        {
            var dataAcomph = dtAtual.Value.Date;
            var pastAcomph = Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de vazões\ACOMPH\1_historico", dataAcomph.ToString("yyyy"), dataAcomph.ToString("MM_yyyy"));
            var nomeAcomph = "ACOMPH_" + dataAcomph.ToString("dd-MM-yyyy") + ".xls";

            if (!File.Exists(Path.Combine(pastAcomph, nomeAcomph)))
            {
                dataAcomph = dataAcomph.AddDays(-1);
            }

            foreach (var prec in chuvas.Where(x => x.Key <= dataAcomph))
            {
                var runRev = ChuvaVazaoTools.Tools.Tools.GetNextRev(prec.Key);
                string precipDado = "0.00";
                var pastaSaida = @"C:\Files\Middle - Preço\16_Chuva_Vazao\" + runRev.revDate.ToString("yyyy_MM") + @"\RV" + runRev.rev.ToString() + @"\" + prec.Key.ToString("yy-MM-dd") + @"\Mapas Acomph\madeira\funceme";


                foreach (var postoPlu in modelosChVz.SelectMany(x => x.PostosPlu))
                {
                    if (Directory.Exists(pastaSaida))
                    {
                        var postoPrecip = GetPsatFuncenme(pastaSaida);
                        if (postoPrecip != null)
                        {
                            var nomeposto = postoPlu.Codigo.Substring(1);// trata o nome de postos que começam com 0 no nome 
                            precipDado = postoPrecip.Where(x => x.Item1.Contains(nomeposto)).Select(x => x.Item2).FirstOrDefault();
                        }
                    }

                    if (postoPlu.Preciptacao.Values.Any(x => x.HasValue))
                    {
                        if (prec.Value.Data <= dataAcomph)// testar se da certo com (<=) se nao der tirar esse if e testar, pq com (==) da problema se falta dias
                        {

                            if (!postoPlu.Preciptacao.ContainsKey(prec.Value.Data))
                            {
                                var isWindowns = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                                if (isWindowns)
                                {
                                    //postoPlu.Preciptacao[prec.Key] = prec.Value[postoPlu.Codigo];
                                    postoPlu.Preciptacao[prec.Key] = float.Parse(precipDado.Replace('.', ','));// o replace é para publicação em windowns 
                                }
                                else
                                {
                                    postoPlu.Preciptacao[prec.Key] = float.Parse(precipDado);// sem o replace é para publicação em nuvem 
                                }

                            }
                        }
                    }
                    else postoPlu.Preciptacao[prec.Key] = null;
                }
            }

            AddLog(" - Preciptação Observada Carregada");
        }

        public static List<Tuple<string, string>> GetPsatFuncenme(string pastaSaida)
        {
            try
            {
                var postosPsat = @"C:\Sistemas\ChuvaVazao\POSTOSPSAT_PLU.txt";
                var psatLinhas = File.ReadAllLines(postosPsat);
                List<Tuple<string, string, string>> dadosPsat = new List<Tuple<string, string, string>>();

                foreach (var linha in psatLinhas)
                {
                    var temp = linha.Split('\t').ToList();
                    Tuple<string, string, string> dad = new Tuple<string, string, string>(temp[0], temp[1], temp[2]);//nome lat long
                    dadosPsat.Add(dad);

                }
                var funceme = Directory.GetFiles(pastaSaida).First();
                var dados = File.ReadAllLines(funceme);
                List<Tuple<string, string, string>> dadosfunceme = new List<Tuple<string, string, string>>();
                foreach (var linha in dados)
                {
                    var temp = linha.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    Tuple<string, string, string> dad = new Tuple<string, string, string>(temp[0], temp[1], temp[2]);//long lat precip
                    dadosfunceme.Add(dad);

                }
                List<Tuple<string, string>> postoPrecip = new List<Tuple<string, string>>();
                foreach (var func in dadosfunceme)
                {
                    try
                    {
                        string precip = func.Item3;
                        string nome = dadosPsat.Where(x => x.Item2 == func.Item2 && x.Item3 == func.Item1).Select(x => x.Item1).First();
                        postoPrecip.Add(new Tuple<string, string>(nome, precip));
                    }
                    catch (Exception e)
                    {

                    }
                }
                return postoPrecip;
            }
            catch (Exception e)
            {
                e.ToString();
                return null;
            }

        }

        public void SalvarPrecObserv()
        {
            foreach (var modelo in modelosChVz)
            {
                modelo.SalvarPrecObservada();
            }

            foreach (var prec in chuvas.Where(x => x.Key <= dtAtual.Value.Date))
            {

                var raiznome = this.txtNomeChuvaPrev.Text + "p" + dtAtual.Value.Date.ToString("ddMMyy") + "a" + prec.Key.ToString("ddMMyy") + ".dat";
                prec.Value.SalvarModeloEta(System.IO.Path.Combine(this.ArquivosDeSaida, raiznome));
            }

            var remo = new PrecipitacaoConjunto(Config.ConfigConjunto);
            var chuvaMedia = remo.MediaBacias(chuvas.Where(x => x.Key <= dtAtual.Value.Date).ToDictionary(x => x.Key, x => x.Value));
            //chuvas = chuvaMedia;
            //RefreshPrecipList();
            var dadoslog = new StringBuilder();
            var header = "Precipitacao média";
            dadoslog.AppendLine(header);
            dadoslog.AppendLine("Bacia\t" + string.Join("\t", chuvaMedia.Keys.Select(x => x.ToString("yyyy-MM-dd"))));
            foreach (var pCo in remo.RegioesConjunto.Where(x => x.Modelo == "CONJ").GroupBy(x => x.Agrupamento))
            {
                dadoslog.Append(pCo.Key.Nome + "\t");
                dadoslog.AppendLine(string.Join("\t", pCo.First().precMedia.Select(x => x.ToString("0.00"))));
            }

            File.WriteAllText(System.IO.Path.Combine(this.ArquivosDeSaida, "chuvamediaObservada.log"), dadoslog.ToString());



            AddLog("Arquivos de Preciptação Observada Salvos");
        }

        public void PrecipitacaoPrevista()
        {
            var data = dtAtual.Value.Date;

            var modelo = "*";

            //carregar ou criar nova?

            Ookii.Dialogs.TaskDialog tskD1 = new Ookii.Dialogs.TaskDialog(this.components) { WindowTitle = "Precipitação Prevista" };
            var bN = new Ookii.Dialogs.TaskDialogButton { ButtonType = Ookii.Dialogs.ButtonType.Custom, Text = "Novo" };
            var bE = new Ookii.Dialogs.TaskDialogButton { ButtonType = Ookii.Dialogs.ButtonType.Custom, Text = "Existente" };
            var bR = new Ookii.Dialogs.TaskDialogButton { ButtonType = Ookii.Dialogs.ButtonType.Custom, Text = "Modelo R" };

            tskD1.Buttons.Add(bN);
            tskD1.Buttons.Add(bE);
            tskD1.Buttons.Add(bR);

            var existente = false;
            var modeloR = false;
            tskD1.ButtonClicked += (se, ev) =>
            {
                if (ev.Item == bE) existente = true;
                else if (ev.Item == bN) existente = false;
                else if (ev.Item == bR)
                {
                    PrecipitacaoPrevista_R();
                    modeloR = true;
                }
            };


            tskD1.ShowDialog();


            if (existente)
            {

                Ookii.Dialogs.VistaFolderBrowserDialog d = new Ookii.Dialogs.VistaFolderBrowserDialog();
                d.SelectedPath = System.IO.Path.Combine(Config.CaminhoPrevisao, data.ToString("yyyyMM"), data.ToString("dd"));
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                    var searchPath = d.SelectedPath;
                    bool tryCtl = false;

                    for (int i = 1; i <= 30; i++)
                    {

                        var dataPrev = data.AddDays(i);
                        var raiznome = "p" + data.ToString("ddMMyy") + "a" + dataPrev.ToString("ddMMyy");

                        var prevFiles = System.IO.Directory.GetFiles(searchPath, modelo + raiznome + ".dat", SearchOption.TopDirectoryOnly);
                        string prevFile = null;
                        if (prevFiles.Length == 0 && modelo == "*")
                        {
                            //MessageBox.Show("Nenhuma previsão encontrada");
                            tryCtl = true;
                            break;
                        }
                        else if (prevFiles.Length == 0 && modelo != "*")
                        {

                            AddLog("   Precipitação Prevista não encontrada: " + modelo + raiznome + ".dat");
                            break;

                        }
                        else if (prevFiles.Length > 1 && modelo == "*")
                        {

                            dialogModeloPrev.RadioButtons.Clear();
                            prevFiles.ToList().ForEach(x =>
                                dialogModeloPrev.RadioButtons.Add(new Ookii.Dialogs.TaskDialogRadioButton()
                                {
                                    Text =
                                        x.Substring(x.LastIndexOf('\\') + 1, x.LastIndexOf(raiznome) - x.LastIndexOf('\\') - 1)
                                        + "\r\n" + x
                                }));
                            if (Ookii.Dialogs.TaskDialog.OSSupportsTaskDialogs)
                                dialogModeloPrev.ShowDialog(this);

                            modelo = dialogModeloPrev.RadioButtons.First(x => x.Checked).Text.Split(new string[] { "\r\n" }, StringSplitOptions.None)[0].Trim();
                            prevFile = prevFiles.First(x => x.Contains(modelo + raiznome));
                        }
                        else prevFile = prevFiles[0];

                        if (modelo == "*")
                        {
                            modelo = prevFile.Substring(
                                prevFile.LastIndexOf('\\') + 1,
                                prevFile.LastIndexOf(raiznome) - prevFile.LastIndexOf('\\') - 1
                                );
                        }



                        chuvas[dataPrev] = PrecipitacaoFactory.BuildFromEtaFile(prevFile);
                        chuvas[dataPrev].Descricao = "PREV NUM - " + modelo + raiznome;
                    }

                    if (tryCtl)
                    {
                        var prevFiles = System.IO.Directory.GetFiles(searchPath, "*.ctl", SearchOption.TopDirectoryOnly);

                        if (prevFiles.Length > 0)
                        {
                            foreach (var prevFile in prevFiles)
                            {
                                var precip = PrecipitacaoFactory.BuildFromMergeFile(prevFile);

                                chuvas[precip.Data] = precip;

                                modelo = System.IO.Path.GetDirectoryName(prevFile).Split('\\').Last();
                                var raiznome = System.IO.Path.GetFileName(prevFile);

                                chuvas[precip.Data].Descricao = "PREV NUM - " + modelo + raiznome;
                            }
                        }
                    }
                }
            }
            else
            {
                if (modeloR == false)
                    GerarPrevisaoConjunto();
                //btnPrecConjunto_Click(sender, e);
            }
            RefreshPrecipList();

            AddLog("- Precipitação Prevista Carregada");
        }


        public void PrecipitacaoPrevista_R()
        {
            var data = dtAtual.Value.Date;

            var modelo = "*";

            var existente = false;


            existente = true;

            if (existente)
            {

                Ookii.Dialogs.VistaFolderBrowserDialog d = new Ookii.Dialogs.VistaFolderBrowserDialog();

                //d.SelectedPath = System.IO.Path.Combine(Config.CaminhoPrevisao, data.ToString("yyyyMM"), data.ToString("dd"));
                d.SelectedPath = System.IO.Path.Combine("H:\\Middle - Preço\\16_Chuva_Vazao\\2019_09\\RV3\\19 - 09 - 19\\Teste");
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {


                    var searchPath = d.SelectedPath;

                    for (int i = 1; i <= 50; i++)
                    {

                        var dataPrev = data.AddDays(i);
                        var raiznome = "p" + data.ToString("ddMMyy") + "a" + dataPrev.ToString("ddMMyy");
                        var prevFiles = System.IO.Directory.GetFiles(searchPath, "*" + raiznome + ".dat", SearchOption.TopDirectoryOnly);
                        var prevFiles_R = System.IO.Directory.GetFiles(searchPath, "*.dat", SearchOption.TopDirectoryOnly);
                        string prevFile = null;

                        Mapas_R(prevFiles);

                        if (prevFiles.Length == 0 && modelo == "*")
                        {
                            //MessageBox.Show("Nenhuma previsão encontrada");

                            break;
                        }
                        else if (prevFiles.Length == 0 && modelo != "*")
                        {

                            AddLog("   Precipitação Prevista não encontrada: " + modelo + raiznome + ".dat");
                            break;

                        }

                        else
                        {

                            prevFile = prevFiles[0];

                        }

                        if (modelo == "*")
                        {
                            modelo = "ETA40";

                        }

                        chuvas[dataPrev] = PrecipitacaoFactory.BuildFromEtaFile(prevFile);
                        chuvas[dataPrev].Descricao = "PREV NUM - " + "ETA40_" + raiznome;




                    }
                    RefreshPrecipList();
                    //  Ler();

                    //foreach (var prec in chuvas.Where(x => x.Key > dtAtual.Value.Date))
                    //{
                    //    /*if (prec.Key == DateTime.Today.AddDays(4))
                    //    {

                    //    }*/

                    //    var raiznome1 = this.txtNomeChuvaPrev.Text + "p" + dtAtual.Value.Date.ToString("ddMMyy") + "a" + prec.Key.ToString("ddMMyy") + ".dat";
                    //    prec.Value.SalvarModeloEta(System.IO.Path.Combine(this.ArquivosDeSaida, raiznome1));
                    //}



                    //foreach (var modelo1 in modelosChVz)
                    //{
                    //    modelo1.DataPrevisao = dtAtual.Value.Date;
                    //    modelo1.SalvarPrecPrevista(chuvas);
                    //    modelo1.SalvarParametros();
                    //}
                }


            }
            else
            {

                GerarPrevisaoConjunto();
                //btnPrecConjunto_Click(sender, e);
            }

            MessageBox.Show("Carregados com Sucesso!");



        }

        public void PrecipitacaoPrevista_R(string pastaRaiz, string pastaSaida)
        {
            var data = dtAtual.Value.Date;

            var modelo = "*";

            var existente = false;


            existente = true;

            if (existente)
            {




                for (int i = 1; i <= 365; i++)
                {

                    var dataPrev = data.AddDays(i);
                    var raiznome = "p" + data.ToString("ddMMyy") + "a" + dataPrev.ToString("ddMMyy");
                    var prevFiles = System.IO.Directory.GetFiles(pastaRaiz, "*" + raiznome + ".dat", SearchOption.TopDirectoryOnly);
                    var prevFiles_R = System.IO.Directory.GetFiles(pastaRaiz, "*.dat", SearchOption.TopDirectoryOnly);
                    string prevFile = null;
                    if (prevFiles.Length != 0)
                    {
                        Mapas_R(prevFiles, i, pastaSaida);

                        if (prevFiles.Length == 0 && modelo == "*")
                        {
                            //MessageBox.Show("Nenhuma previsão encontrada");

                            break;
                        }
                        else if (prevFiles.Length == 0 && modelo != "*")
                        {

                            AddLog("   Precipitação Prevista não encontrada: " + modelo + raiznome + ".dat");
                            break;

                        }

                        else
                        {

                            prevFile = prevFiles[0];

                        }

                        if (modelo == "*")
                        {
                            modelo = "ETA40";

                        }

                        chuvas[dataPrev] = PrecipitacaoFactory.BuildFromEtaFile(prevFile);
                        chuvas[dataPrev].Descricao = "PREV NUM - " + "ETA40_" + raiznome;


                    }

                }
                RefreshPrecipList();




            }
            else
            {

                GerarPrevisaoConjunto();
                //btnPrecConjunto_Click(sender, e);
            }

        }

        public void GravarPrec()
        {



            Ookii.Dialogs.TaskDialog tskD1 = new Ookii.Dialogs.TaskDialog(this.components) { WindowTitle = "Conversão para DAT" };
            var bE = new Ookii.Dialogs.TaskDialogButton { ButtonType = Ookii.Dialogs.ButtonType.Custom, Text = "Modelo ETA" };
            var bG = new Ookii.Dialogs.TaskDialogButton { ButtonType = Ookii.Dialogs.ButtonType.Custom, Text = "Modelo GEFS" };
            var bGFS = new Ookii.Dialogs.TaskDialogButton { ButtonType = Ookii.Dialogs.ButtonType.Custom, Text = "Modelo GFS" };


            tskD1.Buttons.Add(bE);
            tskD1.Buttons.Add(bG);
            tskD1.Buttons.Add(bGFS);

            tskD1.ButtonClicked += (se, ev) =>
            {
                if (ev.Item == bE)
                {
                    foreach (ListViewItem lst in listView_PrecPrev.Items)
                    {
                        var Nome_Chuva = lst.SubItems[1].Text.Split(' ').Last().Split('.').First() + ".dat";
                        var caminho = txtCaminho.Text;
                        var Cam_Final = Path.Combine(caminho, Nome_Chuva);

                        var i = lst as PrecipitacaoItemView;

                        i.Prec.SalvarModeloEta(Cam_Final);

                    }
                }
                else if (ev.Item == bG)
                {
                    foreach (ListViewItem lst in listView_PrecPrev.Items)
                    {
                        var Nome_Chuva = lst.SubItems[1].Text.Split(' ').Last().Split('.').First() + ".dat";
                        var caminho = txtCaminho.Text;
                        var Cam_Final = Path.Combine(caminho, Nome_Chuva);

                        var i = lst as PrecipitacaoItemView;

                        i.Prec.SalvarModeloDAT(Cam_Final, "GEFS");

                    }
                }
                else if (ev.Item == bGFS)
                {
                    foreach (ListViewItem lst in listView_PrecPrev.Items)
                    {
                        var Nome_Chuva = lst.SubItems[1].Text.Split(' ').Last().Split('.').First() + ".dat";
                        var caminho = txtCaminho.Text;
                        var Cam_Final = Path.Combine(caminho, Nome_Chuva);

                        var i = lst as PrecipitacaoItemView;

                        i.Prec.SalvarModeloDAT(Cam_Final, "GFS");

                    }
                }
            };
            tskD1.ShowDialog();
            MessageBox.Show("Conversão Realizada!");
            /* Salvar Prec como CTL
            if (listView_PrecPrev.SelectedItems.Count == 1)
            {

                var i = listView_PrecPrev.SelectedItems[0] as PrecipitacaoItemView;

                SaveFileDialog fd = new SaveFileDialog();
                if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                    //i.Prec.Salvar(fd.FileName);

                    i.Prec.SalvarModeloEta(fd.FileName);

                }
                // PrevViewer.ShowViewer(i.Prec, "Previsao " + i.Prec.Data.ToString("dd-MM-yyyy"));

            }
            else
                MessageBox.Show("Selecione uma chuva");
                */
        }

        public void AtualizarAcompHBD()
        {
            PreencherVazObservada(out _, out _);
        }

        public void SalvarPrecPrev()
        {
            if (chuvas.Count == 0)
            {
                AddLog("Selecione as chuvas");
            }

            var img = false;
            if (String.IsNullOrWhiteSpace(this.ArquivosDeSaida) || !System.IO.Directory.Exists(this.ArquivosDeSaida))
            {
                SelecionarSaida();
                img = true;
            }

            if (String.IsNullOrWhiteSpace(this.ArquivosDeSaida) || !System.IO.Directory.Exists(this.ArquivosDeSaida))
            {
                return;
            }

            if (img)
            {
                this.Busy = true;

                foreach (var prec in chuvas.Where(x => x.Key > dtAtual.Value.Date))
                {
                    PrecipitacaoFactory.SalvarModeloBin(prec.Value,
                        System.IO.Path.Combine(this.ArquivosDeSaida,
                        "pp" + dtAtual.Value.ToString("yyyyMMdd") + "_" + ((prec.Key - dtAtual.Value).TotalHours).ToString("0000")
                        )
                    );
                }

                cptec.CreateCustomImages(dtAtual.Value, this.ArquivosDeSaida, this.txtNomeChuvaPrev.Text);

                var remo = new PrecipitacaoConjunto(Config.ConfigConjunto);
                //var dic = new Dictionary<DateTime, Precipitacao>();
                // dic[pr.Data] = pr;
                ///
                var chuvaMEDIA = remo.ConjuntoLivre(chuvas, null);
                /// 
                //var chuvaMedia = remo.MediaBacias(chuvas);

                var dadoslog = new StringBuilder();

                var header = "Precipitacao média";

                dadoslog.AppendLine(header);
                foreach (var pCo in remo.RegioesConjunto.Where(x => x.Modelo == "CONJ"))
                {
                    dadoslog.Append(pCo.Agrupamento.Nome + "\t" + pCo.Nome + "\t");
                    dadoslog.AppendLine(string.Join("\t", pCo.precMedia.Select(x => x.ToString("0.00"))));
                }

                File.WriteAllText(System.IO.Path.Combine(this.ArquivosDeSaida, "chuvamedia.log"), dadoslog.ToString());

                this.ArquivosDeSaida = "";

                this.Busy = false;
            }
            else
            {

                foreach (var prec in chuvas.Where(x => x.Key > dtAtual.Value.Date))
                {
                    /*if (prec.Key == DateTime.Today.AddDays(4))
                    {

                    }*/

                    var raiznome = this.txtNomeChuvaPrev.Text + "p" + dtAtual.Value.Date.ToString("ddMMyy") + "a" + prec.Key.ToString("ddMMyy") + ".dat";
                    prec.Value.SalvarModeloEta(System.IO.Path.Combine(this.ArquivosDeSaida, raiznome));
                }



                foreach (var modelo in modelosChVz)
                {
                    modelo.DataPrevisao = dtAtual.Value.Date;
                    modelo.SalvarPrecPrevista(chuvas);
                    modelo.SalvarParametros();
                }

                var remo = new PrecipitacaoConjunto(Config.ConfigConjunto);
                var chuvaMedia = remo.MediaBacias(chuvas.Where(x => x.Key > dtAtual.Value.Date).ToDictionary(x => x.Key, x => x.Value));
                //chuvas = chuvaMedia;
                //RefreshPrecipList();
                var dadoslog = new StringBuilder();
                var header = "Precipitacao média";
                dadoslog.AppendLine(header);
                dadoslog.AppendLine("Bacia\t" + string.Join("\t", chuvaMedia.Keys.Select(x => x.ToString("yyyy-MM-dd"))));
                foreach (var pCo in remo.RegioesConjunto.Where(x => x.Modelo == "CONJ").GroupBy(x => x.Agrupamento))
                {
                    dadoslog.Append(pCo.Key.Nome + "\t");
                    dadoslog.AppendLine(string.Join("\t", pCo.First().precMedia.Select(x => x.ToString("0.00"))));
                }

                File.WriteAllText(System.IO.Path.Combine(this.ArquivosDeSaida, "chuvamedia.log"), dadoslog.ToString());

            }

            AddLog("- Precipitação Prevista Salva");
        }

        public async void GerarPrevisaoConjunto()
        {
            var data = dtAtual.Value.Date;

            //WaitForm2 form = await WaitForm2.ShowAsync(data);

            WaitForm2 form = await WaitForm2.ShowAsync(data);


            if (form.DialogResult != System.Windows.Forms.DialogResult.OK) return;


            Dictionary<DateTime, Precipitacao> chuvaConjunto = form.ChuvaConjunto;

            foreach (var c in chuvaConjunto)
            {
                /*foreach (var teste in c.Value.Prec)
                {
                    if (teste.Value < 0) c.Value.Prec[teste.Key] = 0;     //  teste.Value = 0;
                }*/

                chuvas[c.Key] = c.Value;
            }

            RefreshPrecipList();
            await Task.Yield();
        }

        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            textLogger = new TextBoxLogger(txtLogPrecip, this);

            Ookii.Dialogs.VistaFolderBrowserDialog ofd = new Ookii.Dialogs.VistaFolderBrowserDialog();

            #region Caminho de Entrada
            var currRev = ChuvaVazaoTools.Tools.Tools.GetCurrRev(DateTime.Today);

            //var pastaBase = @"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\Acompanhamento de vazões\" + currRev.revDate.ToString("MM_yyyy") + @"\Dados_de_Entrada_e_Saida_" + currRev.revDate.ToString("yyyyMM") + "_RV" + currRev.rev.ToString();
            var pastaBase = @"C:\Files\Middle - Preço\Acompanhamento de vazões\" + currRev.revDate.ToString("MM_yyyy") + @"\Dados_de_Entrada_e_Saida_" + currRev.revDate.ToString("yyyyMM") + "_RV" + currRev.rev.ToString();

            if (Directory.Exists(pastaBase))
            {
                ofd.SelectedPath = System.IO.Path.Combine(Config.CaminhoInicialEntrada, currRev.revDate.ToString("MM_yyyy"));

                this.ArquivosDeEntradaModelo = System.IO.Path.Combine(pastaBase, "Modelos_Chuva_Vazao_Shadow");
                this.ArquivosDeEntradaPrevivaz = System.IO.Path.Combine(pastaBase, "Previvaz", "Arq_Entrada");
                this.ArquivoPrevsBase = System.IO.Directory.GetFiles(pastaBase, "prevs.*", SearchOption.AllDirectories)[0];
            }


            #endregion

            #region Caminho de saída


            var nextRev = ChuvaVazaoTools.Tools.Tools.GetNextRev(dtAtual.Value);
            //this.ArquivosDeSaida = @"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\16_Chuva_Vazao\" + nextRev.revDate.ToString("yyyy_MM") + @"\RV" + nextRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\"; ;
            this.ArquivosDeSaida = @"C:\Files\Middle - Preço\16_Chuva_Vazao\" + nextRev.revDate.ToString("yyyy_MM") + @"\RV" + nextRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\"; ;


            #endregion
        }

        private Version GetRunningVersion()
        {

            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {

                var curV = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion;

                System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CheckForUpdateCompleted += (object sender, System.Deployment.Application.CheckForUpdateCompletedEventArgs e) =>
                {
                    if (e.UpdateAvailable)
                        MessageBox.Show("Nova versão disponível (" + e.AvailableVersion.ToString() + "), reinicie o aplicativo para instalar.");
                };
                System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CheckForUpdateAsync();

                return curV;

            }
            else
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

        }

        //ler precipitacao obsr
        private void button7_Click(object sender, EventArgs e)
        {

            var diag = new Ookii.Dialogs.VistaFolderBrowserDialog();



            if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                var path = diag.SelectedPath;

                foreach (var postoPlu in modelosChVz.SelectMany(x => x.PostosPlu))
                {

                    var existingFiles = System.IO.Directory.GetFiles(path,
                   postoPlu.Codigo + "_c.txt",
                    System.IO.SearchOption.AllDirectories);

                    if (existingFiles.Length > 0)
                    {
                        var exFile = existingFiles[0];

                        using (var onsFile = System.IO.File.OpenText(exFile))
                        {
                            while (!onsFile.EndOfStream)
                            {

                                var l = onsFile.ReadLine();
                                DateTime dt;
                                if (l.Split(' ').Length > 1 &&
                                    DateTime.TryParseExact(l.Split(' ')[1], "dd/MM/yyyy", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None, out dt)
                                    )
                                {
                                    postoPlu.Preciptacao[dt] =
                                        float.Parse(l.Split(' ')[3].Replace("-", "0"), System.Globalization.NumberFormatInfo.InvariantInfo);

                                }
                            }
                        }
                    }
                }
            }
        }

        #region Partes
        public void ParteA()
        {
            try
            {
                Busy = true;
                progressoParte.Value = 10;

                ClearLog();
                progressoParte.Value += 10;

                CriarCaso();
                progressoParte.Value += 10;

                Ler();
                progressoParte.Value += 10;

                CarregarPrecObserv();
                progressoParte.Value += 10;

                PreencherPrecObserv();
                progressoParte.Value += 10;

                PreencherVazObservada(out DateTime dtVaz, out _);
                progressoParte.Value += 10;

                PrecipitacaoPrevista();
                progressoParte.Value += 10;

                dtAtual.Value = dtVaz.AddDays(1);
                dtModelo.Value = dtAtual.Value.Date;

                Reiniciar(dtModelo.Value);
                progressoParte.Value = this.progressoParte.Maximum;

                //MessageBox.Show("Executar Parte B quando pronto");
                AddLog(" --- ");
                AddLog(" --- Executar Parte B quando pronto --- ");

                listLogs.SelectedIndex = this.listLogs.Items.Count - 1;

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            finally
            {
                progressoParte.Value = 0;
                Busy = false;
            }
        }

        public async void ParteB()
        {
            try
            {
                this.Busy = true;
                PreencherPrecObserv();
                progressoParte.Value += 20;
                SalvarPrecObserv();
                progressoParte.Value += 20;
                SalvarPrecPrev();
                progressoParte.Value += 20;
                SalvarVazObserv();
                progressoParte.Value += 20;

                await ExecutarTudoAsync();

                //btnColetarResultados_Click(sender, e);

                progressoParte.Value += 10;

                AddLog(" --- ");
                AddLog(" --- Parte B Concluída --- ");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Busy = false;
                progressoParte.Value = 0;
            }
        }

        public void ParteC()
        {
            try
            {
                var app = Helper.StartExcel();
                progressoParte.Value += 10;

                ColetaDeResultados(app, out Microsoft.Office.Interop.Excel.Workbook wb);
                progressoParte.Value += 30;

                wb.Activate();
                progressoParte.Value = 100;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                progressoParte.Value = 0;
            }
        }

        public async void ParteB_R()
        {
            try
            {
                //Renomear_Eta40();
                this.Busy = true;
                PreencherPrecObserv();
                progressoParte.Value += 20;
                SalvarPrecObserv_R();
                progressoParte.Value += 20;
                SalvarPrecPrev_R();
                progressoParte.Value += 20;
                SalvarVazObserv();
                progressoParte.Value += 20;

                await ExecutarTudoAsync();

                //btnColetarResultados_Click(sender, e);

                progressoParte.Value += 10;

                AddLog(" --- ");
                AddLog(" --- Parte B Concluída --- ");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Busy = false;
                progressoParte.Value = 0;
            }
        }

        #endregion

        private void btnCarregarPrecObserv_Click(object sender, EventArgs e)
        {
            CarregarPrecObserv();
        }

        void ExecutarTudo(RunStatus statusF = null)
        {

            if (statusF != null) statusF.Execution = RunStatus.statuscode.initialialized;

            Parallel.ForEach(modelosChVz, x =>
            {
                AddLog("\t Executando: " + x.Caminho);
                x.Executar();

                if (x.ErroNaExecucao == true)
                {
                    AddLog(x.Caminho + " não executado");
                    File.AppendAllText(Path.Combine(txtCaminho.Text, "error.log"), x.Caminho + " não executado\n");
                }
                else
                {
                    x.ColetarSaida();
                    AddLog("\t Finalizado: " + x.Caminho);
                }

            });

            if (statusF != null) statusF.Execution =
                    modelosChVz.All(x => x.ErroNaExecucao == false) ? RunStatus.statuscode.completed : RunStatus.statuscode.error;

            ;

        }

        void ExecutarTudo_Manual(RunStatus statusF = null)
        {


            Parallel.ForEach(modelosChVz, x =>
            {

                x.Executar();

                if (x.ErroNaExecucao == true)
                {
                    File.AppendAllText(Path.Combine(txtCaminho.Text, "error.log"), x.Caminho + " não executado");
                }
                else
                {
                    x.ColetarSaida();
                }
            });

            if (statusF != null) statusF.Execution =
                    modelosChVz.All(x => x.ErroNaExecucao == false) ? RunStatus.statuscode.completed : RunStatus.statuscode.error;

            ;
        }

        private async Task ExecutarTudoAsync()
        {

            //await Task.Delay(5000);
            var execs = modelosChVz.Select(async x =>
            {
                AddLog("\t Executando: " + x.Caminho);
                await x.ExecutarAsync();

                if (x.ErroNaExecucao == true)
                {
                    AddLog(x.Caminho + " não executado");
                    File.AppendAllText(Path.Combine(txtCaminho.Text, "error.log"), x.Caminho + " não executado");
                    MessageBox.Show(x.Caminho + " não executado", "Execução CHUVA VAZÃO", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    x.ColetarSaida();
                    AddLog("\t Finalizado: " + x.Caminho);
                }
            });
            await Task.WhenAll(execs);
        }

        private async void btnExecutarTudo_Click(object sender, EventArgs e)
        {
            try
            {

                this.Busy = true;

                await ExecutarTudoAsync();
                MessageBox.Show("Execução Finalizada");
            }
            catch (Exception ex)
            {
                AddLog(ex.Message);
            }
            finally
            {
                this.Busy = false;
                AddLog("- Execução Finalizada");
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {


            var iniVazao = "INICIALIZACAO_VAZAO.txt";

            var vazInicialConfigs = System.IO.File.ReadLines(iniVazao)
                .Where(x => x.Length >= 115 && x[0] != '#')
                .Select(x => new
                {
                    Arquivo = x.Substring(0, 36).Trim(),
                    ArquivoONS = x.Substring(36, 68).Trim(),
                    Posto = x.Substring(104, 3).Trim(),
                    PostoTipo = x.Substring(111, 3).Trim(),
                });




            var diag = new Ookii.Dialogs.VistaFolderBrowserDialog();

            if (diag.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                var path = diag.SelectedPath;

                var arqus = modelosChVz.SelectMany(x => x.Vazoes).Join(vazInicialConfigs,
                    x => System.IO.Path.GetFileName(x.CaminhoArquivo).ToUpperInvariant(), y => y.Arquivo.ToUpperInvariant(), (x, y) => new { entrada = x, arqONS = y.ArquivoONS });

                foreach (var arqEntrada in arqus)
                {

                    var existingFiles = System.IO.Directory.GetFiles(path,
                    arqEntrada.arqONS,
                    System.IO.SearchOption.AllDirectories);

                    if (existingFiles.Length > 0)
                    {
                        var exFile = existingFiles[0];
                        var arqONS = new VazoesRealizadas(exFile);

                        foreach (var vaz in arqONS.Vazoes)
                        {
                            arqEntrada.entrada.Vazoes[vaz.Key] = arqONS.Vazoes[vaz.Key];
                        }

                        arqEntrada.entrada.SalvarVazoes();
                    }
                }
            }

            MessageBox.Show("OK");
        }


        public object[,] ColetaCPINS()
        {
            try
            {
                var PathModelo = Path.Combine(txtEntrada.Text, "CPINS", "Arq_Saida");
                DateTime dt_CPINS = DateTime.Today;
                var Arquivo = Path.Combine(PathModelo, dt_CPINS.ToString("dd-MM-yyyy") + "_PLANILHA_USB.txt");

                while (!File.Exists(Arquivo))
                {
                    dt_CPINS = dt_CPINS.AddDays(-1);
                    Arquivo = Path.Combine(PathModelo, dt_CPINS.ToString("dd-MM-yyyy") + "_PLANILHA_USB.txt");
                }

                var TxtCpins = File.ReadAllLines(Arquivo);

                var Num_linhas = TxtCpins.Length;

                object[,] results = new object[Num_linhas, 3];

                for (int i = 0; i <= Num_linhas - 1; i++)
                {
                    var Separa = TxtCpins[i].Split(';');
                    results[i, 0] = Separa[0];
                    results[i, 1] = Separa[1];
                    results[i, 2] = Separa[2];
                }

                return results;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public object[,] ColetarResultado()
        {
            try
            {
                modelosChVz.ForEach(x => x.ColetarSaida());

                var vaz = modelosChVz.SelectMany(x => x.Vazoes).ToList();
                var minData = vaz.Min(x => x.Vazoes.Keys.Min());
                var maxData = vaz.Max(x => x.Vazoes.Keys.Max());

                //s += "\t" + string.Join("\t", vaz.Select(x => x.Nome)) + Environment.NewLine;

                int rows = (int)(maxData - minData).TotalDays + 1;
                int cols = vaz.Count();

                object[,] results = new object[rows + 1, cols + 1];

                for (int i = 0; i < cols; i++)
                {
                    results[0, i + 1] = vaz[i].Nome;
                }

                for (int d = 0; d < rows; d++)
                {
                    var dt = minData.AddDays(d);
                    results[d + 1, 0] = dt;
                    for (int i = 0; i < cols; i++)
                    {
                        if (vaz[i].Vazoes.ContainsKey(dt))
                        {
                            results[d + 1, i + 1] = vaz[i].Vazoes[dt];
                        }
                    }
                }
                AddLog("- Resultados coletados");

                return results;
            }
            catch (Exception e)
            {
                AddLog("\t" + "Erro no método FrmMain/ColetarResultado: " + e.Message);
                return null;
            }
        }

        private void btnCopiarResultados_Click(object sender, EventArgs e)
        {
            CopiarResultados();
        }

        private void btnSalvarVazObserv_Click(object sender, EventArgs e)
        {
            SalvarVazObserv();
        }

        private void btnAtualizarAcomphTXT_Click(object sender, EventArgs e)
        {
            var vazInicialConfigs = System.IO.File.ReadLines(Config.IniVazao)
                .Where(x => x.Length >= 115 && x[0] != '#')
                .Select(x => new
                {
                    Arquivo = x.Substring(0, 36).Trim(),
                    ArquivoONS = x.Substring(36, 68).Trim(),
                    Posto = x.Substring(104, 3).Trim(),
                    PostoTipo = x.Substring(111, 3).Trim(),
                    Vazoes = x.Substring(114).Trim()
                });

            foreach (var arqEntrada in modelosChVz.SelectMany(x => x.Vazoes).Join(vazInicialConfigs,
                x => System.IO.Path.GetFileName(x.CaminhoArquivo).ToUpperInvariant(), y => y.Arquivo.ToUpperInvariant(), (x, y) => new { entrada = x, posto = y.Posto, vazoes = y.Vazoes }))
            {
                if (!string.IsNullOrWhiteSpace(arqEntrada.posto))
                {
                    var dataIni = DateTime.ParseExact(
                        arqEntrada.vazoes.Split(' ')[0],
                        "yyyy-MM-dd", System.Globalization.DateTimeFormatInfo.InvariantInfo);

                    var v = arqEntrada.vazoes.Split(' ').Skip(1).ToArray();

                    for (int i = 0; i < v.Length; i++)
                    {
                        var vazArq = float.Parse(v[i]);

                        if (vazArq > 0) arqEntrada.entrada.Vazoes[dataIni.AddDays(i)] = vazArq;
                    }
                }
            }
            AddLog("- Vazoes Passadas Carregadas de ACOMPH/RDH");
        }

        private void ClearLog()
        {
            this.listLogs.Items.Clear();
            progressoParte.Value = 0;
        }


        private static void SMAPDirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (dir.Name.Equals("Arq_Pos_Processamento", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

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
            if (dir.Name.Equals("ARQ_ENTRADA", StringComparison.OrdinalIgnoreCase))
            {
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    if (!file.Extension.EndsWith("dat", StringComparison.OrdinalIgnoreCase))
                    {
                        // Create the path to the new copy of the file.
                        string temppath = Path.Combine(destDirName, file.Name);

                        // Copy the file.
                        file.CopyTo(temppath, true);
                    }
                }
            }

            if (dir.Name.Equals("ARQ_SAIDA", StringComparison.OrdinalIgnoreCase))
            {
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    if (file.Name.EndsWith("_AJUSTE.txt", StringComparison.OrdinalIgnoreCase))
                    {
                        // Create the path to the new copy of the file.
                        string temppath = Path.Combine(destDirName, file.Name);

                        // Copy the file.
                        file.CopyTo(temppath, true);
                    }
                }
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    SMAPDirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
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

        #region Partes de execução

        private void btnParteA_Click(object sender, EventArgs e)
        {
            ParteA();
        }

        private void btnParteB_Click(object sender, EventArgs e)
        {
            ParteB();
        }

        private void btnParteC_Click(object sender, EventArgs e)
        {
            ParteC();
        }

        #endregion

        private void btnSelecionarEntrada_Click(object sender, EventArgs e)
        {
            Ookii.Dialogs.VistaFolderBrowserDialog ofd = new Ookii.Dialogs.VistaFolderBrowserDialog();

            ofd.SelectedPath = Config.CaminhoInicialEntrada;

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.ArquivosDeEntradaModelo = System.IO.Path.Combine(ofd.SelectedPath, "Modelos_Chuva_Vazao");
                this.ArquivosDeEntradaPrevivaz = System.IO.Path.Combine(ofd.SelectedPath, "Previvaz", "Arq_Entrada");
                this.ArquivoPrevsBase = System.IO.Directory.GetFiles(ofd.SelectedPath, "prevs.*", SearchOption.AllDirectories)[0];
            }
        }

        private void btnSelecionarSaida_Click(object sender, EventArgs e)
        {
            SelecionarSaida();

            //Consulta("");
        }



        //private string _error;
        //private void Consulta(string message)
        //{
        //    string _return = string.Empty;


        //    try
        //    {
        //        this._error = "";

        //        //Criando o bind dos parâmentros que serão passados para o API
        //        //e convertendo em ByteArray para populado no corpo do Request 
        //        //para o Server
        //       // string _paramenterText = bindParmeters(pMethod, pParameters);
        //        //_body = Encoding.UTF8.GetBytes(_paramenterText);

        //        //Chamando função que criptografará os parâmentros a serem enviados
        //        _sign = cripParametersSign(_paramenterText);

        //        //Criando Metodo de Request para o Servidor do Mercado Bitcoin
        //        WebRequest request = null;
        //        request = WebRequest.Create(_REQUEST_HOST + _REQUEST_PATH);

        //        request.Method = "POST";
        //        request.Headers.Add("tapi-id", _MB_TAPI_ID);
        //        request.Headers.Add("tapi-mac", _sign);
        //        request.ContentType = "application/x-www-form-urlencoded";
        //        request.ContentLength = _body.Length;
        //        request.Timeout = 360000;

        //        //Escrevendo parâmentros no corpo do Request para serem enviados a API
        //        Stream _req = request.GetRequestStream();
        //        _req.Write(_body, 0, _body.Length);
        //        _req.Close();

        //        //Pegando retorno do servidor
        //        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        //        Stream dataStream = response.GetResponseStream();

        //        //Convertendo Stream de retorno em texto para 
        //        //Texto de retorno será um JSON 
        //        using (StreamReader reader = new StreamReader(dataStream))
        //            _return = reader.ReadToEnd();

        //        //Liberando objetos para o Coletor de Lixo
        //        dataStream.Close();
        //        dataStream.Dispose();
        //        response.Close();
        //        response.Dispose();

        //    }
        //    catch (Exception ex)
        //    {
        //        //this._error = ex.Message;
        //        _return = "";
        //    }
        //}

        public static string ByteToString(byte[] buff)
        {
            string sbinary = "";

            for (int i = 0; i < buff.Length; i++)
            {
                sbinary += buff[i].ToString("X2"); // hex format
            }
            return (sbinary);
        }



        private void AddLog(String text)
        {
            lock (this)
            {
                listLogs.Items.Add(text);
                if (listLogs.Items.Count > 0) listLogs.SelectedIndex = listLogs.Items.Count - 1;
            }
        }

        private void btnbtnCriarCaso_Click(object sender, EventArgs e)
        {
            CriarCaso();
        }

        private void ColetaDeResultados(Excel.Application app, out Excel.Workbook wb)
        {
            if (modelosChVz.Count == 0) Ler(true);

            //new ExecutingProcess().ProcessResults(modelosChVz);


            //wb = null;

            //return;
            //ExecutingProcess proc = new ExecutingProcess();


            var excelFile = Config.XltmResultado;

            wb = app.Workbooks.Add(excelFile);
            while (!app.Ready)
            {
                System.Threading.Thread.Sleep(200);
            }

            var ws = wb.Worksheets["cpins"] as Microsoft.Office.Interop.Excel.Worksheet;

            ws.Select();
            ws.UsedRange.ClearContents();

            var res = ColetarResultado();
            var resCPINS = ColetaCPINS();
            //modelosChVz

            ws.Range[ws.Cells[2, "A"], ws.Cells[61, "C"]].value = resCPINS;

            ws = wb.Worksheets["RESULTADOS"] as Microsoft.Office.Interop.Excel.Worksheet;

            ws.Range[ws.Cells[1, 1], ws.Cells[res.GetLength(0), res.GetLength(1)]].Value = res;

            ws = wb.Worksheets["Aux"] as Microsoft.Office.Interop.Excel.Worksheet;
            (ws.Range[ws.Cells[3, 2], ws.Cells[3, 2]] as Excel.Range).Value2 = this.ArquivoPrevsBase;
            (ws.Range[ws.Cells[4, 2], ws.Cells[4, 2]] as Excel.Range).Value2 = this.ArquivosDeEntradaPrevivaz;
            (ws.Range[ws.Cells[5, 2], ws.Cells[5, 2]] as Excel.Range).Value2 = this.ArquivosDeSaida.Split('\\').LastOrDefault();

            (ws.Range[ws.Cells[1, 2], ws.Cells[1, 2]] as Excel.Range).Value2 = this.DataSemanaPrevsBase ?? this.dtModelo.Value;

            ws = wb.Worksheets["PREVS_SMAP"] as Microsoft.Office.Interop.Excel.Worksheet;
            (ws.Range[ws.Cells[1, 2], ws.Cells[1, 2]] as Excel.Range).Value2 = this.dtModelo.Value;

            foreach (dynamic conn in wb.Connections)
            {
                conn.Refresh();
            }
        }

        #region Precipitação

        private void btnPreencherPrecObserv_Click(object sender, EventArgs e)
        {
            PreencherPrecObserv();
        }

        private void btnSalvarPrecObserv_Click(object sender, EventArgs e)
        {
            SalvarPrecObserv();
        }

        private void PrecipitacaoObesvAlternativa()
        {
            bool sobreescrever = false;

            for (DateTime data = chuvas.Min(x => x.Key); data <= DateTime.Today; data = data.AddDays(1))
            {

                foreach (var postoPlu in modelosChVz.Where(x => x is SMAP.ModeloSmap).SelectMany(x => x.PostosPlu))
                {
                    if (sobreescrever || !postoPlu.Preciptacao.ContainsKey(data)) postoPlu.Preciptacao[data] = 2;
                }
            }

            AddLog("- Preciptação Observada ALTERNATIVA Carregada");

            foreach (var modelo in modelosChVz)
            {
                modelo.SalvarPrecObservada();
            }

            MessageBox.Show("Arquivos de Preciptação Observada Salvos");
        }

        private void btnCarregarPrecPrev_Click(object sender, EventArgs e)
        {
            PrecipitacaoPrevista();
        }

        private void RefreshPrecipList()
        {
            listView_PrecPrev.Items.Clear();
            listView_PrecPrev.Items.AddRange(
                chuvas.Select(c => new PrecipitacaoItemView(c.Value) { DataChuva = c.Key, Descricao = c.Value.Descricao })
                .OrderBy(x => x.DataChuva)
                .ToArray()
                );
        }

        private void btnSalvarPrecPrev_Click(object sender, EventArgs e)
        {
            SalvarPrecPrev();
        }

        private void btnEditarPrecip_Click(object sender, EventArgs e)
        {
            if (listView_PrecPrev.SelectedItems.Count == 1)
            {
                var i = listView_PrecPrev.SelectedItems[0] as PrecipitacaoItemView;
                PrevViewer.ShowViewer(i.Prec, "Previsao " + i.Prec.Data.ToString("dd-MM-yyyy"));
            }
        }

        private void btnGravarPrec_Click(object sender, EventArgs e)
        {
            GravarPrec();
        }

        private void btnDeletarPrec_Click_2(object sender, EventArgs e)
        {
            if (listView_PrecPrev.SelectedItems.Count == 1)
            {
                var i = listView_PrecPrev.SelectedItems[0] as PrecipitacaoItemView;
                chuvas.Remove(i.DataChuva);
                RefreshPrecipList();
            }
            else
                MessageBox.Show("Selecione uma chuva");
        }

        private void btnCopiarPrecip_Click(object sender, EventArgs e)
        {
            if (listView_PrecPrev.SelectedItems.Count == 1)
            {
                var i = listView_PrecPrev.SelectedItems[0] as PrecipitacaoItemView;
                var dataSel = i.DataChuva;
                var dataInicio = chuvas.Keys.Max().AddDays(1);

                for (int d = 0; d < (dataInicio - dataSel).TotalDays; d++)
                {

                    var np = chuvas[dataSel.AddDays(d)].Duplicar();
                    np.Data = dataInicio.AddDays(d);
                    np.Descricao = "Cópia - " + np.Descricao;
                    chuvas.Add(np.Data, np);
                }

                RefreshPrecipList();
            }
            else
                MessageBox.Show("Selecione uma chuva");
        }

        private void btnVerTudoPrecip_Click(object sender, EventArgs e)
        {
            if (listView_PrecPrev.SelectedItems.Count == 1)
            {
                var chuvasimages = chuvas
                //.Where(x => x.Key > this.dtModelo.Value)
                .OrderBy(c => c.Key)
                .Select(c => c.Value)
                .ToList();

                PrevViewer.ShowViewer(new IEnumerable<Precipitacao>[] { chuvasimages }, this);
            }
            else
                MessageBox.Show("Selecione uma chuva");
        }

        #endregion

        private void PreencherVazObservada(out DateTime ultimaDataDisponivel, out string fonte)
        {

            var iniVazao = Config.Postos_Vazaoes;


            var vazInicialConfigs = System.IO.File.ReadLines(iniVazao)
                .Select(x => x.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries))
                .Where(x => x.Length >= 5)
                .Select(x =>
                {
                    var configFlu = new
                    {
                        Arquivo = x[0].Trim(),
                        TipoAtualizacao = x[1].Trim(),
                        Origem = new List<(float Fator, int Posto, string Tipo)>(),
                    };


                    for (int i = 0; i < (x.Length - 2) / 3; i++)
                    {
                        configFlu.Origem.Add((
                            float.Parse(x[2 + i * 3], System.Globalization.NumberFormatInfo.InvariantInfo),
                            int.Parse(x[3 + i * 3]),
                            x[4 + i * 3].Trim()
                            ));
                    }

                    return configFlu;
                }
                ).ToList();



            var vazInicialConfigsVazias = System.IO.File.ReadLines(iniVazao)
                .Select(x => x.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries))
                .Where(x => x.Length == 1)
                .Select(x => new
                {
                    Arquivo = x[0].Trim()
                }).ToList();


            //ajuste de parametros de modelos PARCIAIS
            foreach (var vazConfig in vazInicialConfigs.ToList().Where(x => x.TipoAtualizacao == "COMPOSTA").GroupBy(x => x.Origem[0].Posto))
            {

                var arquivosEntrada = modelosChVz.SelectMany(x => x.Vazoes).Where(x => vazConfig.Select(y => y.Arquivo.ToUpperInvariant()).Contains(System.IO.Path.GetFileName(x.CaminhoArquivo).ToUpperInvariant()));
                var totalDic = arquivosEntrada.SelectMany(x => x.Vazoes).GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.Sum(y => y.Value));



                foreach (var arquEntrada in arquivosEntrada)
                {

                    var f = arquEntrada.Vazoes.Sum(x => x.Value) / totalDic.Sum(x => x.Value);

                    var oldConfig = vazInicialConfigs.Where(x => x.Arquivo == System.IO.Path.GetFileName(arquEntrada.CaminhoArquivo).ToUpperInvariant()).First();
                    oldConfig.Origem.Add((f, oldConfig.Origem[0].Posto, oldConfig.Origem[0].Tipo));
                    oldConfig.Origem.RemoveAt(0);
                }
            }


            var dataI = dtAtual.Value.Date.AddDays(-34);
            ultimaDataDisponivel = dataI;

            List<CONSULTA_VAZAO> dados = null;
            List<Vazoes_Observadas> dados_observados = null;

            if (Config.FonteVazao.Trim().Equals("db", StringComparison.OrdinalIgnoreCase))
            {
                using (var ctx = new IPDOEntities1())
                {
                    dados = ctx.CONSULTA_VAZAO.Where(x => x.data >= dataI).ToList();
                    dados_observados = ctx.Vazoes_Observadas.Where(x => x.Data >= dataI).ToList();
                }
            }
            else
            {
                dados = ReadVazoesPassadas(Config.HistoricoVazao);
            }
            ///
            //Vazoes relatorio Hidro

            var tipo_vazoes = Tipo_Vazaoes();


            foreach (var arqEntrada in modelosChVz.SelectMany(x => x.Vazoes).Join(vazInicialConfigs,
                x => System.IO.Path.GetFileName(x.CaminhoArquivo).ToUpperInvariant(),
                y => y.Arquivo.ToUpperInvariant(), (x, y) => new { posto = x, config = y }))
            {

                for (DateTime dt = dataI.AddDays(3); dt < dtAtual.Value; dt = dt.AddDays(1))
                {
                    if (arqEntrada.config.Origem.All(y => dados_observados.Any(x => x.Data == dt && y.Posto == x.Cod_Posto)))
                    {
                        if (arqEntrada.config.TipoAtualizacao == "TOTAL" || (dtAtual.Value.Date - dt).TotalDays <= 7)
                        {
                            try
                            {
                                //

                                var vazHidr =
                               arqEntrada.config.Origem.Select(ori =>
                               {

                                   var cod_posto = tipo_vazoes.Where(x => x.Item2 == ori.Posto.ToString()).First();

                                   var value = dados_observados.First(x => x.Data == dt && x.Cod_Posto.ToString() == cod_posto.Item2 && x.Tipo_Vazao == cod_posto.Item4).Vazao;
                                   if (cod_posto.Item2 == "34")
                                   {
                                       value = value + dados_observados.First(x => x.Data == dt && x.Cod_Posto.ToString() == "243" && x.Tipo_Vazao == cod_posto.Item4).Vazao;
                                   }
                                   else if (cod_posto.Item2 == "74")
                                   {
                                       var UV_Atual = dados_observados.First(x => x.Data == dt && x.Cod_Posto.ToString() == "65310001" && x.Tipo_Vazao == "VMD").Vazao;
                                       var UV_D1 = dados_observados.First(x => x.Data == dt.AddDays(-1) && x.Cod_Posto.ToString() == "65310001" && x.Tipo_Vazao == "VMD").Vazao;
                                       value = Convert.ToDecimal(Convert.ToDouble(value) - (Convert.ToDouble(UV_Atual) * (24 - 17.4f) + Convert.ToDouble(UV_D1) * (17.4f)) / 24);
                                   }
                                   else if (cod_posto.Item2 == "73")
                                   {
                                       value = value + dados_observados.First(x => x.Data == dt && x.Cod_Posto.ToString() == "76" && x.Tipo_Vazao == cod_posto.Item4).Vazao;
                                       value = value + dados_observados.First(x => x.Data == dt && x.Cod_Posto.ToString() == "72" && x.Tipo_Vazao == cod_posto.Item4).Vazao;
                                   }
                                   else if (cod_posto.Item2 == "222")
                                   {
                                       value = value + dados_observados.First(x => x.Data == dt && x.Cod_Posto.ToString() == "78" && x.Tipo_Vazao == cod_posto.Item4).Vazao;
                                       value = value + dados_observados.First(x => x.Data == dt && x.Cod_Posto.ToString() == "77" && x.Tipo_Vazao == cod_posto.Item4).Vazao;
                                   }
                                   else if (cod_posto.Item2 == "230")
                                   {
                                       value = value + dados_observados.First(x => x.Data == dt && x.Cod_Posto.ToString() == "229" && x.Tipo_Vazao == cod_posto.Item4).Vazao;
                                   }
                                   else if (cod_posto.Item2 == "228")
                                   {
                                       value = value + dados_observados.First(x => x.Data == dt && x.Cod_Posto.ToString() == "227" && x.Tipo_Vazao == cod_posto.Item4).Vazao;
                                   }
                                   else if (cod_posto.Item2 == "52")
                                   {


                                       var vnm3 = dados_observados.First(x => x.Data == dt && x.Cod_Posto.ToString() == "249" && x.Tipo_Vazao == cod_posto.Item4).Vazao;
                                       var vnm2 = dados_observados.First(x => x.Data == dt && x.Cod_Posto.ToString() == "50" && x.Tipo_Vazao == cod_posto.Item4).Vazao;
                                       var vnm1 = dados_observados.First(x => x.Data == dt && x.Cod_Posto.ToString() == "51" && x.Tipo_Vazao == cod_posto.Item4).Vazao;

                                       var vnm3d1 = dados_observados.First(x => x.Data == dt.AddDays(-1) && x.Cod_Posto.ToString() == "249" && x.Tipo_Vazao == cod_posto.Item4).Vazao;
                                       var vnm1d1 = dados_observados.First(x => x.Data == dt.AddDays(-1) && x.Cod_Posto.ToString() == "51" && x.Tipo_Vazao == cod_posto.Item4).Vazao;
                                       var vnm2d1 = dados_observados.First(x => x.Data == dt.AddDays(-1) && x.Cod_Posto.ToString() == "50" && x.Tipo_Vazao == cod_posto.Item4).Vazao;

                                       var vnm3d2 = dados_observados.First(x => x.Data == dt.AddDays(-2) && x.Cod_Posto.ToString() == "249" && x.Tipo_Vazao == cod_posto.Item4).Vazao;
                                       var vnm1d2 = dados_observados.First(x => x.Data == dt.AddDays(-2) && x.Cod_Posto.ToString() == "51" && x.Tipo_Vazao == cod_posto.Item4).Vazao;
                                       var vnm2d2 = dados_observados.First(x => x.Data == dt.AddDays(-2) && x.Cod_Posto.ToString() == "50" && x.Tipo_Vazao == cod_posto.Item4).Vazao;

                                       var vnm3d3 = dados_observados.First(x => x.Data == dt.AddDays(-3) && x.Cod_Posto.ToString() == "249" && x.Tipo_Vazao == cod_posto.Item4).Vazao;

                                       var valor1 = ((vnm3 * (24 - 3) + vnm3d1 * 3) / 24);
                                       var valor1d1 = ((vnm3d1 * (24 - 3) + vnm3d2 * 3) / 24);
                                       var valor1d2 = ((vnm3d2 * (24 - 3) + vnm3d3 * 3) / 24);

                                       var valor2 = ((Convert.ToDouble(vnm2 + valor1) * (24 - 2.8) + Convert.ToDouble(vnm2d1 + valor1d1) * 2.8) / 24);
                                       var valor2d1 = ((Convert.ToDouble(vnm2d1 + valor1d1) * (24 - 2.8) + Convert.ToDouble(vnm2d2 + valor1d2) * 2.8) / 24);

                                       var valor3 = (((Convert.ToDouble(vnm1) + valor2) * (24 - 2.8) + (Convert.ToDouble(vnm1d1) + valor2d1) * 2.8) / 24);


                                       value = Convert.ToDecimal(Convert.ToDouble(value) + valor3);

                                   }
                                   else if (cod_posto.Item2 == "49")
                                   {
                                       value = value + dados_observados.First(x => x.Data == dt && x.Cod_Posto.ToString() == "48" && x.Tipo_Vazao == cod_posto.Item4).Vazao;
                                   }
                                   else if (cod_posto.Item2 == "63")
                                   {
                                       value = value + dados_observados.First(x => x.Data == dt && x.Cod_Posto.ToString() == "62" && x.Tipo_Vazao == cod_posto.Item4).Vazao;
                                   }

                                   return value;
                               }).Sum();
                                arqEntrada.posto.Vazoes[dt] = float.Parse(vazHidr.ToString());
                            }
                            catch (Exception e)
                            {
                            }
                        }
                    }
                }

            }

            //checar disponibilidade de dados:
            for (DateTime dt = dataI; dt < dtAtual.Value; dt = dt.AddDays(1))
                if (!dados.Any(x => x.data == dt))
                {
                    var txt = "  Vazoes Passadas Não encontradas para o dia " + dt.ToString("dd/MM/yyyy");
                    AddLog(txt);
                    if (!runAuto) MessageBox.Show(txt);
                }
                else
                {
                    ultimaDataDisponivel = dt;
                }

            fonte = dados.Where(x => x.posto == 1).OrderByDescending(x => x.data).First().fonte;

            /*   foreach (var arqEntrada in modelosChVz.SelectMany(x => x.Vazoes).Join(vazInicialConfigs,
                   x => System.IO.Path.GetFileName(x.CaminhoArquivo).ToUpperInvariant(),
                   y => y.Arquivo.ToUpperInvariant(), (x, y) => new { posto = x, config = y }))
               {
                   if (arqEntrada.config.Arquivo.ToString() == "UVITORIA.TXT")
                   {
                       var testee = 0;
                   }
                   for (DateTime dt = dataI; dt < dtAtual.Value; dt = dt.AddDays(1))
                   {
                       if (arqEntrada.config.Origem.All(y => dados.Any(x => x.data == dt && y.Posto == x.posto)))
                       {
                           if (arqEntrada.config.TipoAtualizacao == "TOTAL" || (dtAtual.Value.Date - dt).TotalDays <= 7)
                           {

                               var vazAcomph =
                               arqEntrada.config.Origem.Select(ori =>
                               {
                                   var value = ori.Tipo == "NAT" ?
                                       dados.First(x => x.data == dt && x.posto == ori.Posto).qnat
                                       : dados.First(x => x.data == dt && x.posto == ori.Posto).qinc;
                                   value = value < 0 ? 0 : value;
                                   value = (int)(value * ori.Fator);

                                   return value;
                               }).Sum();
                               arqEntrada.posto.Vazoes[dt] = vazAcomph;
                           }
                       }
                   }

                   //arqEntrada.posto.SalvarVazoes();
               }*/

            foreach (var arqEntrada in modelosChVz.SelectMany(x => x.Vazoes).Join(vazInicialConfigsVazias,
                x => System.IO.Path.GetFileName(x.CaminhoArquivo).ToUpperInvariant(),
                y => y.Arquivo.ToUpperInvariant(), (x, y) => new { posto = x, config = y }))
            {
                for (DateTime dt = dataI; dt < dtAtual.Value; dt = dt.AddDays(1))
                {

                    if (!arqEntrada.posto.Vazoes.ContainsKey(dt) && arqEntrada.posto.Vazoes.ContainsKey(dt.AddDays(-1)))
                    {
                        arqEntrada.posto.Vazoes[dt] = arqEntrada.posto.Vazoes[dt.AddDays(-1)];
                    }
                    else if (!arqEntrada.posto.Vazoes.ContainsKey(dt))
                    {
                        arqEntrada.posto.Vazoes[dt] = arqEntrada.posto.Vazoes.Average(x => x.Value);
                    }
                }
            }


            AddLog("- Vazoes Passadas Carregadas de ACOMPH/RDH");
        }

        private void btnAtualizarAcompHBD_Click(object sender, EventArgs e)
        {
            AtualizarAcompHBD();
        }

        private void btnAtualizarRDHBD_Click(object sender, EventArgs e)
        {
            var iniVazao = Config.PostosFlu;

            var vazInicialConfigs = System.IO.File.ReadLines(iniVazao)
                .Select(x => x.Split('\t'))
                .Where(x => x.Length >= 5)
                .Select(x => new
                {
                    Arquivo = x[0].Trim(),
                    Fator = float.Parse(x[1], System.Globalization.NumberFormatInfo.InvariantInfo),
                    Posto = int.Parse(x[2]),
                    PostoTipo = x[3].Trim(),
                    TipoAtualizacao = x[4].Trim()
                }).ToList();


            var dataI = dtAtual.Value.Date.AddDays(-31);

            List<CONSULTA_VAZAO_RDH> dados = null;

            using (var ctx = new IPDOEntities1())
            {
                dados = ctx.CONSULTA_VAZAO_RDH.Where(x => x.data >= dataI).ToList();
            }

            //checar disponibilidade de dados:
            for (DateTime dt = dataI; dt < dtAtual.Value; dt = dt.AddDays(1))
                if (!dados.Any(x => x.data == dt))
                {
                    var txt = "  Vazoes Passadas Não encontradas para o dia " + dt.ToString("dd/MM/yyyy");
                    AddLog(txt);
                    MessageBox.Show(txt);
                }

            //ajuste de parametros de modelos PARCIAIS
            foreach (var vazConfig in vazInicialConfigs.ToList().Where(x => x.TipoAtualizacao == "COMPOSTA").GroupBy(x => x.Posto))
            {
                var arquivosEntrada = modelosChVz.SelectMany(x => x.Vazoes).Where(x => vazConfig.Select(y => y.Arquivo.ToUpperInvariant()).Contains(System.IO.Path.GetFileName(x.CaminhoArquivo).ToUpperInvariant()));
                var totalDic = arquivosEntrada.SelectMany(x => x.Vazoes).GroupBy(x => x.Key).ToDictionary(x => x.Key, x => x.Sum(y => y.Value));


                foreach (var arquEntrada in arquivosEntrada)
                {

                    var f = arquEntrada.Vazoes.Sum(x => x.Value) / totalDic.Sum(x => x.Value);

                    var oldConfig = vazInicialConfigs.Where(x => x.Arquivo == System.IO.Path.GetFileName(arquEntrada.CaminhoArquivo).ToUpperInvariant()).First();

                    vazInicialConfigs.Remove(oldConfig);
                    vazInicialConfigs.Add(new { oldConfig.Arquivo, Fator = f, oldConfig.Posto, oldConfig.PostoTipo, oldConfig.TipoAtualizacao });
                }
            }

            foreach (var arqEntrada in modelosChVz.SelectMany(x => x.Vazoes).Join(vazInicialConfigs,
                x => System.IO.Path.GetFileName(x.CaminhoArquivo).ToUpperInvariant(),
                y => y.Arquivo.ToUpperInvariant(), (x, y) => new { posto = x, config = y }))
            {

                for (DateTime dt = dataI; dt < dtAtual.Value; dt = dt.AddDays(1))
                {
                    if (dados.Any(x => x.data == dt && x.posto == arqEntrada.config.Posto))
                    {
                        if (arqEntrada.config.TipoAtualizacao == "TOTAL" || (dtAtual.Value.Date - dt).TotalDays <= 7)
                        {
                            var value = arqEntrada.config.PostoTipo == "NAT" ?
                                    dados.First(x => x.data == dt && x.posto == arqEntrada.config.Posto).qnat
                                    : dados.First(x => x.data == dt && x.posto == arqEntrada.config.Posto).qinc;
                            value = value < 0 ? 0 : value;
                            arqEntrada.posto.Vazoes[dt] =
                                arqEntrada.config.Fator * float.Parse(value.ToString());
                        }
                    }
                }

                //arqEntrada.posto.SalvarVazoes();
            }


            AddLog("- Vazoes Passadas Carregadas de ACOMPH/RDH");
        }
        private List<Tuple<string, string, string, string>> Tipo_Vazaoes()
        {
            var tipoVazao = @"C:\Sistemas\ChuvaVazao\tipo_vazao.txt";
            var vazaoLinhas = File.ReadAllLines(tipoVazao);
            List<Tuple<string, string, string, string>> dadosTipo_Vazao = new List<Tuple<string, string, string, string>>();

            try
            {
                foreach (var linha in vazaoLinhas)
                {
                    var temp = linha.Split('\t').ToList();



                    var postos = temp[1].Split('/').ToList();
                    if (postos.Count > 1)
                    {
                    }
                    foreach (var posto in postos)
                    {

                        Tuple<string, string, string, string> dad = new Tuple<string, string, string, string>(temp[0], posto.ToString(), temp[2], temp[3]);
                        dadosTipo_Vazao.Add(dad);
                    }




                }
                return dadosTipo_Vazao;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        private List<CONSULTA_VAZAO> ReadVazoesPassadas(string historicoVazao)
        {

            List<CONSULTA_VAZAO> dados;


            var fileLines = System.IO.File.ReadAllLines(historicoVazao);


            dados = fileLines
                .Where(x => !String.IsNullOrWhiteSpace(x))
                .Select(x => x.Split(new char[] { ';', '\t' }))
                .Where(x => x.Length >= 4)
                .Select(x =>
                {
                    int _posto;
                    DateTime _data;
                    int _q_nat;
                    int _q_inc;


                    if (!int.TryParse(x[1], out _posto) || !DateTime.TryParse(x[0], out _data))
                    {
                        return (CONSULTA_VAZAO)null;
                    }


                    int.TryParse(x[2], out _q_nat);
                    int.TryParse(x[3], out _q_inc);

                    return new CONSULTA_VAZAO() { data = _data, posto = _posto, qinc = _q_inc, qnat = _q_nat, fonte = "FILE" };
                }
                )
                .Where(x => x != null).ToList();

            return dados;
        }

        private void btnInserirPrecip_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Multiselect = true;
            ofd.Filter = "(*.dat)|*.dat|(*.ctl)|*.ctl|(*.gif)|*.gif|(*.png)|*.png";


            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                foreach (var file in ofd.FileNames)
                {


                    if (System.IO.Path.GetExtension(file).ToUpperInvariant() == ".DAT")
                    {
                        System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"a(\d{2})(\d{2})(\d{2})");

                        var fMatch = r.Match(file);
                        if (fMatch.Success)
                        {


                            var data = new DateTime(
                                int.Parse(fMatch.Groups[3].Value) + 2000,
                                int.Parse(fMatch.Groups[2].Value),
                                int.Parse(fMatch.Groups[1].Value))
                                ;


                            //if (this.chuvas.ContainsKey(data))
                            //{
                            //    var precToAdd = PrecipitacaoFactory.BuildFromEtaFile(file);
                            //    foreach (var nk in precToAdd.Prec)
                            //   {
                            //        this.chuvas[data][nk.Key] = nk.Value;
                            //    }

                            //}
                            //else
                            //{
                            this.chuvas[data] = PrecipitacaoFactory.BuildFromEtaFile(file);
                            this.chuvas[data].Descricao = "PREV NUM - " + System.IO.Path
                                .GetFileName(file);
                            //}



                            AddLog("- Precipitação carregada: " + file);
                        }

                    }
                    else if (System.IO.Path.GetExtension(file).ToUpperInvariant() == ".CTL")
                    {
                        System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"pp(\d{4})(\d{2})(\d{2})_(\d+)");

                        var fMatch = r.Match(file);
                        if (fMatch.Success)
                        {
                            var data = new DateTime(
                                int.Parse(fMatch.Groups[1].Value),
                                int.Parse(fMatch.Groups[2].Value),
                                int.Parse(fMatch.Groups[3].Value))
                                ;

                            var horas = int.Parse(fMatch.Groups[4].Value);

                            var dataPrev = data.AddHours(horas).Date;


                            this.chuvas[dataPrev] = PrecipitacaoFactory.BuildFromMergeFile(file);
                            this.chuvas[dataPrev].Descricao = "PREV NUM - " + System.IO.Path
                                .GetFileName(file);

                            this.chuvas[dataPrev].Data = dataPrev;

                            AddLog("- Precipitação carregada: " + file);

                        }
                        else
                        {
                            r = new System.Text.RegularExpressions.Regex(@"([^\\]+)_(\d{4})(\d{2})(\d{2})\.");
                            fMatch = r.Match(file);
                            if (fMatch.Success)
                            {
                                var ano = int.Parse(fMatch.Groups[2].Value);




                                var data = new DateTime(
                                    ano < DateTime.Today.Year ? DateTime.Today.Year : ano,
                                    int.Parse(fMatch.Groups[3].Value),
                                    int.Parse(fMatch.Groups[4].Value))
                                    ;


                                this.chuvas[data] = PrecipitacaoFactory.BuildFromMergeFile(file);
                                this.chuvas[data].Descricao = fMatch.Groups[1].Value + " - " + System.IO.Path
                                    .GetFileName(file);

                                this.chuvas[data].Data = data;

                                AddLog("- Precipitação carregada: " + file);

                            }
                            else
                            {
                                MessageBox.Show("Data não indentificada");
                            }
                        }



                    }
                    else if (System.IO.Path.GetFileName(file).ToUpperInvariant() == "OBSERVADO.GIF")
                    {

                        var precO = PrecipitacaoFactory.BuildFromImage(file);

                        var dataPrev = precO.Data;


                        this.chuvas[dataPrev] = precO;
                        this.chuvas[dataPrev].Descricao = "OBSEVADO ONS - " + System.IO.Path.GetFileName(file);

                        this.chuvas[dataPrev].Data = dataPrev;

                    }
                    else if (System.IO.Path.GetExtension(file).ToUpperInvariant() == ".GIF")
                    {

                        var precO = PrecipitacaoFactory.BuildFromImage0(file);

                        var dataPrev = precO.Data;


                        this.chuvas[dataPrev] = precO;
                        this.chuvas[dataPrev].Descricao = System.IO.Path.GetFileName(file);

                        this.chuvas[dataPrev].Data = dataPrev;

                    }
                    else if (System.IO.Path.GetExtension(file).ToUpperInvariant() == ".PNG" && System.IO.Path.GetFileName(file).Contains("gfs"))
                    {

                        var precO = PrecipitacaoFactory.BuildFromImage2(file, DateTime.Today);

                        var dataPrev = precO.Data;


                        this.chuvas[dataPrev] = precO;
                        this.chuvas[dataPrev].Descricao = "PREVISAO GFS - " + System.IO.Path.GetFileName(file);

                        this.chuvas[dataPrev].Data = dataPrev;

                    }

                    RefreshPrecipList();
                }
            }
        }

        private void btnTempView_Click(object sender, EventArgs e)
        {
            string caminhoBase = Config.CaminhoTemperatura;

            DateTime dt = dtAtual.Value.Date;

            var caminho = System.IO.Path.Combine(caminhoBase, dt.ToString("yyyyMM"), dt.ToString("dd"));
            var filePrevd = System.IO.Path.Combine(caminho, "dia.txt");
            var filePrevd_1 = System.IO.Path.Combine(caminho, "dia_1.txt");
            var filePrevd_2 = System.IO.Path.Combine(caminho, "dia_2.txt");
            var filePrevd_3 = System.IO.Path.Combine(caminho, "dia_3.txt");
            var filePrevd_4 = System.IO.Path.Combine(caminho, "dia_4.txt");



            Temperatura.Show(
                filePrevd
                ,
                filePrevd_1,
                filePrevd_2,
                filePrevd_3,
                filePrevd_4
                );



            //"SAO_PAULO, SP, BR"

            //graf frm = new graf(atual.Where(x => x.Cidade == "SAO_PAULO, SP, BR").First().Previsao,
            //    anterior.Where(x => x.Cidade == "SAO_PAULO, SP, BR").First().Previsao);
        }

        private void btnCompararTemp_Click(object sender, EventArgs e)
        {

            string caminhoBase = Config.CaminhoTemperatura;

            DateTime dt = dtAtual.Value.Date;
            var caminho = System.IO.Path.Combine(caminhoBase, dt.ToString("yyyyMM"), dt.ToString("dd"));
            var filePrevd = System.IO.Path.Combine(caminho, "dia.txt");

            DateTime dtAtn = dt_TempAnterior.Value.Date;
            var caminhoAnt = System.IO.Path.Combine(caminhoBase, dtAtn.ToString("yyyyMM"), dtAtn.ToString("dd"));
            var filePrevdAnt = System.IO.Path.Combine(caminhoAnt, "dia.txt");




            Temperatura.ShowCompara(
                filePrevd,
                filePrevdAnt
                );


        }

        private void btnPrecRealizadaMedia_Click(object sender, EventArgs e)
        {
            var config = Config.ConfigConjunto;
            //var data = dtAtual.Value.Date;
            var chuvasMerge = new Dictionary<DateTime, Precipitacao>();

            for (DateTime data = dtAtual.Value.Date.AddDays(-9); data <= dtAtual.Value.Date; data = data.AddDays(1))
            {
                var mergeCtlFile = System.IO.Directory.GetFiles(Config.CaminhoMerge, "prec_" + data.ToString("yyyyMMdd") + ".ctl", System.IO.SearchOption.AllDirectories);
                //var mergeDatFile = System.IO.Directory.GetFiles(Config.CaminhoMerge, "prec_" + data.ToString("yyyyMMdd") + ".dat", System.IO.SearchOption.AllDirectories);

                if (mergeCtlFile.Length == 1)
                {


                    var prec = PrecipitacaoFactory.BuildFromMergeFile(mergeCtlFile[0]);
                    prec.Descricao = "MERGE - " + System.IO.Path.GetFileNameWithoutExtension(mergeCtlFile[0]);
                    prec.Data = data;

                    chuvasMerge[data] = prec;
                }
                else
                {

                    if (MessageBox.Show("Merge para a data " + data.ToShortDateString() + " não encontrado.\r\nUsar dia anterior?", "Precip Observada - Chuva Vazão", MessageBoxButtons.YesNo)
                        == DialogResult.Yes)
                    {
                        chuvasMerge[data] = chuvasMerge[data.AddDays(-1)].Duplicar();
                        chuvasMerge[data].Data = data;
                    }

                }
            }

            var remo = new PrecipitacaoConjunto(config);
            var chuvaMEDIA = remo.Conjunto(chuvasMerge, null, WaitForm.TipoConjunto.Eta40);

            Console.WriteLine("Realizada Media");
            foreach (var pCo in remo.RegioesConjunto.Where(x => x.Modelo == "ETA40"))
            {
                Console.Write(pCo.Agrupamento.Nome + "\t" + pCo.Nome + "\t");
                Console.WriteLine(string.Join("\t", pCo.precMedia));
            }

            var vwr = PrevViewer.ShowViewer(new IEnumerable<Precipitacao>[] { chuvasMerge.Values, chuvaMEDIA.Values }, this, caption: "Merge", viewrSize: new Size(240, 278));


            foreach (var c in chuvaMEDIA)
            {
                chuvas[c.Key] = c.Value;
            }

            RefreshPrecipList();
        }

        private void btnDownloadMerge_Click(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show(cptec.ListNewMerge(), "Chuva-vazão : Download MERGE", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                if (MessageBox.Show(ex.Message, "Chuva-vazão", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error) == DialogResult.Retry) btnDownloadMerge_Click(sender, e);
            }
        }

        private async void btnVerDifEta_Click(object sender, EventArgs e)
        {
            Busy = true;

            try
            {

                var chuvasEta00 = new Dictionary<DateTime, Precipitacao>();
                var chuvasEta12 = new Dictionary<DateTime, Precipitacao>();
                var data = dtAtual.Value.Date;
                var searchPath = System.IO.Path.Combine(Config.CaminhoPrevisao, data.ToString("yyyyMM"), data.ToString("dd"));

                txtLogPrecip.ResetText();


                //txtLogPrecip.strea


                //Precipitacao prec;
                //ETA40_00h
                //ETA40_12h
                for (int i = 1; i <= 10; i++)
                {
                    var dataPrev = data.AddDays(i);
                    var raiznome00 = "pp" + data.ToString("yyyyMMdd") + "_" + ((i * 24) + 12).ToString("0000") + ".ctl";
                    var raiznome12 = "pp" + data.ToString("yyyyMMdd") + "_" + ((i * 24)).ToString("0000") + ".ctl";


                    var f00 = System.IO.Path.Combine(searchPath, "ETA40_00h", raiznome00);
                    var f12 = System.IO.Path.Combine(searchPath, "ETA40_12h", raiznome12);

                    if (!File.Exists(f00) || !File.Exists(f12)) MessageBox.Show(await cptec.DownloadETA40Async(data, textLogger));
                    if (!File.Exists(f00) || !File.Exists(f12)) return;

                    chuvasEta00[dataPrev] = PrecipitacaoFactory.BuildFromMergeFile(f00);
                    chuvasEta12[dataPrev] = PrecipitacaoFactory.BuildFromMergeFile(f12);


                    chuvasEta00[dataPrev].Data =
                    chuvasEta12[dataPrev].Data = dataPrev;

                    chuvasEta00[dataPrev].Descricao = "ETA40 00h - " + dataPrev.ToString("dd/MM/yy");
                    chuvasEta12[dataPrev].Descricao = "ETA40 12h - " + dataPrev.ToString("dd/MM/yy");
                }
                var vwr = PrevViewer.ShowViewer(new IEnumerable<Precipitacao>[] { chuvasEta00.Values, chuvasEta12.Values }, this, caption: "ETA_40 - 00h + 12h", viewrSize: new Size(240, 278));
            }
            finally
            {
                Busy = false;
            }
        }

        private async void btnBaixarDados_Click(object sender, EventArgs e)
        {
            Busy = true;

            try
            {
                var data = dtAtual.Value.Date;

                DownloadForm frm = new DownloadForm(data, chuvas);

                if (frm.ShowDialog() == DialogResult.OK)
                {
                    txtLogPrecip.ResetText();
                    await frm.Acao(textLogger);
                    //await cptec.DownloadGEFSAsync(data, new TextBoxLogger(txtLogPrecip));
                    RefreshPrecipList();
                }
            }
            catch (Exception Ex)
            {

            }
            finally
            {
                Busy = false;
            }
        }

        private void btnGerarPrevisaoConjunto_Click(object sender, EventArgs e)
        {
            GerarPrevisaoConjunto();
        }

        private void button20_Click(object sender, EventArgs e)
        {
            var p = cptec.DownloadFunceme();
            chuvas[p.Data] = p;

            RefreshPrecipList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //var remo = new PrecipitacaoConjunto(Config.ConfigConjunto);

            //foreach (var mes in Enumerable.Range(1, 12))
            //{
            //    var dt = new DateTime(2019, 04, mes);
            //    chuvas[dt] = remo.MLT(mes);
            //    chuvas[dt].Data = dt;
            //}

            //RefreshPrecipList();


            //return;





            if (String.IsNullOrWhiteSpace(this.ArquivosDeSaida) || !System.IO.Directory.Exists(this.ArquivosDeSaida))
            {
                btnSelecionarSaida_Click(this, e);

            }

            if (String.IsNullOrWhiteSpace(this.ArquivosDeSaida) || !System.IO.Directory.Exists(this.ArquivosDeSaida))
            {
                return;
            }

            var prec = chuvas;


            var remo = new PrecipitacaoConjunto(Config.ConfigConjunto);

            var chuvaMedia = remo.MediaBacias(prec);
            chuvas = chuvaMedia;

            RefreshPrecipList();

            var dadoslog = new StringBuilder();

            var header = "Precipitacao média";

            dadoslog.AppendLine(header);
            dadoslog.AppendLine("Bacia\t" + string.Join("\t", chuvas.Keys.Select(x => x.ToString("yyyy-MM-dd"))));
            foreach (var pCo in remo.RegioesConjunto.Where(x => x.Modelo == "CONJ").GroupBy(x => x.Agrupamento))
            {
                dadoslog.Append(pCo.Key.Nome + "\t");
                dadoslog.AppendLine(string.Join("\t", pCo.First().precMedia.Select(x => x.ToString("0.00"))));

                //for (int i = 0; i < chuvas.Keys.Count; i++)
                //{
                //    PrecipitacaoRepository.SaveAverage(chuvas.Keys.ToArray()[i], pCo.Key.Nome, "", pCo.First().precMedia[i], "MERGE");
                //}


            }

            //remo = new PrecipitacaoConjunto(Config.ConfigConjunto);
            //var chuvaMEDIA = remo.ConjuntoLivre(prec, null);

            //foreach (var pCo in remo.RegioesConjunto.Where(x => x.Modelo == "CONJ"))
            //{
            //    for (int i = 0; i < chuvaMEDIA.Keys.Count; i++)
            //    {
            //        PrecipitacaoRepository.SaveAverage(chuvaMEDIA.Keys.ToArray()[i], pCo.Agrupamento.Nome, pCo.Nome, pCo.precMedia[i], "MERGE");
            //    }

            //}

            File.WriteAllText(System.IO.Path.Combine(this.ArquivosDeSaida, "chuvamedia.log"), dadoslog.ToString());

            this.ArquivosDeSaida = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var pastaSaida = ArquivosDeSaida;

            var rev = new Ookii.Dialogs.InputDialog();
            rev.MainInstruction = "Numero da revisão";
            rev.MaxLength = 1;
            if (rev.ShowDialog() == DialogResult.OK)
            {

                if (int.TryParse(rev.Input, out int revnum))
                {
                    ProcessarResultados(pastaSaida, revnum: revnum, statusF: new RunStatus(pastaSaida));
                }

            }
            AddLog("FINALIZADO");
        }
        IntPtr pointer;
        private void ProcessarResultados(string pastaSaida, System.IO.TextWriter logF = null, int? revnum = null, RunStatus statusF = null)
        {
            var check = cbx_Encadear_Previvaz.Checked;
            Excel.Workbook wbCen = null;
            Excel.Workbook wb = null;

            if (this.pointer != null)
            {
                foreach (System.Diagnostics.Process proc in System.Diagnostics.Process.GetProcessesByName("Excel"))
                {
                    if (proc.MainWindowHandle == this.pointer)
                    {
                        proc.Kill();
                    }
                }
            }

            var xlsApp = new Microsoft.Office.Interop.Excel.Application();
            this.pointer = new IntPtr(xlsApp.Hwnd);
            try
            {
                if (modelosChVz.Count == 0)
                    Ler(true);

                int nextRevNum = 0;

                if (!revnum.HasValue)
                {
                    var nextRev = ChuvaVazaoTools.Tools.Tools.GetNextRev(dtAtual.Value);
                    nextRevNum = nextRev.rev;
                }
                else
                    nextRevNum = revnum.Value;

                var currRev = Tools.Tools.GetCurrRev(this.DataSemanaPrevsBase.HasValue ? this.DataSemanaPrevsBase.Value.AddDays(-1) : dtAtual.Value);

                int code = pastaSaida.GetHashCode();

                while (!xlsApp.Ready)
                {
                    System.Threading.Thread.Sleep(200);
                }

                xlsApp.Visible = true;
                xlsApp.ScreenUpdating = true;
                xlsApp.DisplayAlerts = false;

                var pathResult = Path.Combine(pastaSaida, $"CHUVAVAZAO_{code}.xlsm");

                if (!File.Exists(pathResult) || statusF?.Collect != RunStatus.statuscode.completed)
                {
                    if (statusF != null) statusF.Collect = RunStatus.statuscode.initialialized;
                    ColetaDeResultados(xlsApp, out wb);
                    wb.SaveAs(
                        pathResult, wb.FileFormat
                        );
                }
                else
                {
                    wb = xlsApp.Workbooks.Open(pathResult);
                }
                if (statusF != null) statusF.Collect = RunStatus.statuscode.completed;

                var pathCen = Path.Combine(pastaSaida, $"CHUVAVAZAO_CENARIO_{code}.xlsm");

                var prevsname = "prevs.rv" + nextRevNum.ToString();


                if (!File.Exists(Path.Combine(pastaSaida, prevsname)) || !File.Exists(pathCen) || statusF?.Previvaz != RunStatus.statuscode.completed)
                {
                    try
                    {
                        if (statusF != null) statusF.Previvaz = RunStatus.statuscode.initialialized;
                        xlsApp.DisplayAlerts = false;

                        xlsApp.Run($"'CHUVAVAZAO_{code}.xlsm'!CriarCenario");

                        wbCen = xlsApp.ActiveWorkbook;

                        while (!xlsApp.Ready)
                        {
                            System.Threading.Thread.Sleep(2000);
                        }

                        try
                        {
                            foreach (Microsoft.Office.Interop.Excel.Name wbName in wbCen.Names)
                            {
                                if (wbName.Visible && wbName.Name == "_gravarPrevivaz") wbName.RefersToRange.Value = true;
                            }
                        }
                        finally { }

                        wbCen.SaveAs(
                            pathCen, wb.FileFormat
                            );

                    }
                    catch
                    {
                        AddLog("Erro criando planilha de cenarios");
                        if (logF != null) logF.WriteLine("Erro criando planilha de cenarios");
                        if (wbCen != null) wbCen.Close(SaveChanges: false);
                        wb.Close(SaveChanges: false);

                        return;
                    }
                    finally
                    {
                        if (wbCen != null) wbCen.Close(SaveChanges: false);
                        wbCen = null;

                        wb.Close(SaveChanges: false);
                        wb = null;
                    }



                    var p = Program.GetPrevivazExPath(pathCen);

                    if (p != null)
                    {
                        AddLog("EXECUCAO PREVIVAZ");
                        if (logF != null) logF.WriteLine("EXECUCAO PREVIVAZ");
                        if (check)
                        {
                            var teste = p.Item2 + "|true";
                            var pre = System.Diagnostics.Process.Start(p.Item1, p.Item2 + "|true");
                            pre.WaitForExit();
                        }
                        else
                        {
                            var pr = System.Diagnostics.Process.Start(p.Item1, p.Item2);

                            pr.WaitForExit();
                        }
                    }
                    else
                    {
                        if (statusF != null) statusF.Previvaz = RunStatus.statuscode.error;
                        return;
                    }

                    if (statusF != null) statusF.Previvaz = RunStatus.statuscode.completed;
                }

                if (statusF?.Previvaz != RunStatus.statuscode.completed) return;

                if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.initialialized;

                if (!File.Exists(Path.Combine(pastaSaida, prevsname)))
                    try
                    {

                        wbCen = xlsApp.Workbooks.Open(pathCen, ReadOnly: true);


                        if (nextRevNum == 0 || (nextRevNum == 1 && currRev.rev != 0) || ((nextRevNum == 2 && currRev.rev != 0) && (nextRevNum == 2 && currRev.rev != 1)) || ((nextRevNum == 3 && currRev.rev != 0) && (nextRevNum == 3 && currRev.rev != 1) && (nextRevNum == 3 && currRev.rev != 2)))
                        {
                            xlsApp.Run($"'CHUVAVAZAO_CENARIO_{code}.xlsm'!ExportarPrevsM1", pastaSaida);
                        }
                        else
                        {
                            xlsApp.Run($"'CHUVAVAZAO_CENARIO_{code}.xlsm'!ExportarPrevs", pastaSaida);
                        }

                        var fprevs = Path.Combine(pastaSaida, "prevs.prv");

                        if (File.Exists(fprevs))
                        {

                            if (File.Exists(Path.Combine(pastaSaida, prevsname))) File.Delete(Path.Combine(pastaSaida, prevsname));

                            if (File.Exists(Path.Combine(pastaSaida, "prevs.prv")))
                                System.IO.File.Move(Path.Combine(pastaSaida, "prevs.prv"), Path.Combine(pastaSaida, prevsname));

                            var nomeDoCaso = pastaSaida.Split('\\').Last();

                            if (nomeDoCaso.StartsWith("CV_") || nomeDoCaso.StartsWith("CV2_") || nomeDoCaso.StartsWith("CV3_") || nomeDoCaso.StartsWith("CV4_") || nomeDoCaso.StartsWith("CV5_"))
                            {
                                // var pathDestino = Path.Combine("L:\\cpas_ctl_common", "auto", DateTime.Today.ToString("yyyyMMdd") + "_" + nomeDoCaso);
                                var pathDestino = Path.Combine("Z:\\cpas_ctl_common", "auto", DateTime.Today.ToString("yyyyMMdd") + "_" + nomeDoCaso);
                                if (!System.IO.Directory.Exists(pathDestino)) Directory.CreateDirectory(pathDestino);
                                File.Copy(Path.Combine(pastaSaida, prevsname), Path.Combine(pathDestino, prevsname));
                                if (System.IO.File.Exists(Path.Combine(pastaSaida, "resumoENA.gif")))
                                    File.Copy(Path.Combine(pastaSaida, "resumoENA.gif"), Path.Combine(pathDestino, "resumoENA.gif"));
                            }

                            AddLog(Path.Combine(pastaSaida, prevsname));
                            if (logF != null) logF.WriteLine(Path.Combine(pastaSaida, prevsname));

                            if (statusF != null) statusF.Previvaz = RunStatus.statuscode.completed;
                        }
                        else //deu ruim na exportação do Prevs. (provavelmente erro na execução do previvaz)
                        {
                            if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.error;
                            if (statusF != null) statusF.Previvaz = RunStatus.statuscode.error;
                            AddLog("Erro na execução do Previvaz");

                            if (logF != null) logF.WriteLine("Erro na execução do Previvaz");

                            if (wbCen != null)
                            {
                                wbCen.Close(SaveChanges: false);
                                if (File.Exists(pathCen)) File.Delete(pathCen);
                                wbCen = null;
                            }

                            throw new Exception();
                        }
                    }
                    catch
                    {
                        if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.error;
                        if (statusF != null) statusF.Previvaz = RunStatus.statuscode.error;
                        AddLog("Erro na execução do Previvaz");

                        if (logF != null) logF.WriteLine("Erro na execução do Previvaz");
                        if (wbCen != null) { wbCen.Close(SaveChanges: false); }
                    }

                if (statusF?.Previvaz != RunStatus.statuscode.completed) return;

                if (!File.Exists(Path.Combine(pastaSaida, "enasemanal.log")))
                    try
                    {
                        if (wbCen == null) wbCen = xlsApp.Workbooks.Open(pathCen, ReadOnly: true);

                        var valoresSemanais = wbCen.Worksheets["Cen1"].Range["B14", "N61"].Value as object[,];

                        var enaText = "";
                        for (int i = 1; i <= valoresSemanais.GetLength(0); i++)
                        {
                            for (int j = 1; j <= valoresSemanais.GetLength(1); j++)
                            {
                                enaText += valoresSemanais[i, j]?.ToString() + "\t";
                            }
                            enaText += "\r\n";
                        }

                        File.WriteAllText(Path.Combine(pastaSaida, "enasemanal.log"), enaText);

                    }
                    catch (Exception ex)
                    {

                        AddLog("Erro em exportação de imagem");
                        if (logF != null) logF.WriteLine("Erro em exportação de imagem");
                        AddLog(ex.Message);
                        if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.error;
                    }
                ///DIARIO///
                ///

                if (!File.Exists(Path.Combine(pastaSaida, "enadiaria.log")))
                    try
                    {
                        if (wbCen == null)
                            wbCen = xlsApp.Workbooks.Open(pathCen, ReadOnly: true);

                        if (wb == null)
                            wb = xlsApp.Workbooks.Open(pathResult);

                        var cen1 = wbCen.Worksheets["Cen1"] as Excel.Worksheet;
                        var vals = cen1.Range["_cen1"].Value2;

                        var resPr = wb.Worksheets["PREVIVAZ"] as Excel.Worksheet;

                        resPr.Range["A2", "N321"].Value2 = vals;

                        var wsprevs = wbCen.Worksheets["Prevs"] as Excel.Worksheet;
                        var dats = wsprevs.Range["D3", "O3"].Value2;
                        resPr.Range["C1", "N1"].Value2 = dats;
                        wb.Save();

                        wb.Activate();

                        xlsApp.Run($"'CHUVAVAZAO_{code}.xlsm'!CriarCenarioDiario");
                        Excel.Workbook wbCenDiario = xlsApp.ActiveWorkbook;

                        var valoresDiarios = wbCenDiario.Worksheets["CenDiario"].Range["B14", "AB61"].Value as object[,];

                        try
                        {
                            decimal valu;
                            foreach (var valDia in valoresDiarios)
                            {
                                if (valDia != null)
                                {
                                    if (decimal.TryParse(valDia.ToString(), out valu))
                                        if (valu < 0)
                                            throw new Exception("Erro ao criando Ena diaria");
                                }
                                else
                                    continue;
                            }
                        }
                        catch { }

                        var enaText = "";
                        for (int i = 1; i <= valoresDiarios.GetLength(0); i++)
                        {
                            for (int j = 1; j <= valoresDiarios.GetLength(1); j++)
                            {
                                enaText += valoresDiarios[i, j]?.ToString() + "\t";
                            }
                            enaText += "\r\n";
                        }

                        File.WriteAllText(Path.Combine(pastaSaida, "enadiaria.log"), enaText);

                        if (wbCenDiario != null)
                            wbCenDiario.Close(SaveChanges: false);

                        if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.completed;

                    }
                    catch (Exception ex)
                    {
                        AddLog("Erro em processamento de enas diárias");

                        if (logF != null) logF.WriteLine("Erro em processamento de enas diárias");
                        AddLog(ex.Message);
                        if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.error;
                    }

                //Copia resultados para Storage AZURE


                string[] arqs_copy = { "CHUVAVAZAO_" + code + ".xlsm", "CHUVAVAZAO_CENARIO_" + code + ".xlsm", "enadiaria.log", "enasemanal.log" , prevsname };

                var path_Z = pastaSaida.Replace("C:\\Files\\Middle - Preço\\16_Chuva_Vazao", "Z:\\16_Chuva_Vazao");
                foreach (var arq in arqs_copy)
                {
                    if (File.Exists(Path.Combine(pastaSaida, arq)))
                    {
                        if (!Directory.Exists(path_Z))
                        {
                            Directory.CreateDirectory(path_Z);
                        }
                        if (logF != null) logF.WriteLine("Copiando Arquivo " + arq);
                        File.Copy(Path.Combine(pastaSaida, arq), Path.Combine(path_Z, arq), true);
                    }

                }

            }
            finally
            {
                if (wb != null)
                {
                    wb.Saved = true;
                    wb.Close(SaveChanges: false);
                }

                if (wbCen != null)
                {
                    wbCen.Saved = true;
                    wbCen.Close(SaveChanges: false);
                }

                if (xlsApp != null)
                {
                    xlsApp.DisplayAlerts = false;
                    xlsApp.Quit();
                    Helper.Release(xlsApp);
                }
            }

        }

        private void autoExecPorPasta()
        {
            var logF = textLogger;
            var searchPath = "";
            Boolean Falha = false;

            Ookii.Dialogs.VistaFolderBrowserDialog d = new Ookii.Dialogs.VistaFolderBrowserDialog();

            //d.SelectedPath = System.IO.Path.Combine(Config.CaminhoPrevisao, data.ToString("yyyyMM"), data.ToString("dd"));
            //d.SelectedPath = System.IO.Path.Combine(@"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\16_Chuva_Vazao\");
            d.SelectedPath = System.IO.Path.Combine(@"C:\Files\Middle - Preço\16_Chuva_Vazao\");
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                searchPath = d.SelectedPath;

            }
            else
            {
                return;

            }
            var revnum = 0;
            var rev = new Ookii.Dialogs.InputDialog();
            rev.MainInstruction = "Numero da revisão";
            rev.MaxLength = 1;

            if (rev.ShowDialog() == DialogResult.OK)
            {
                revnum = int.Parse(rev.Input);
            }


            var dir_saida = txtCaminho.Text;
            var dirs_mapas = Directory.GetDirectories(searchPath);
            int conta = 0;

            do
            {

                foreach (string dir_mapa in dirs_mapas)
                {
                    var arquivosdats = Directory.GetFiles(dir_mapa, "*.dat");

                    if (arquivosdats.Length > 0)
                    {

                        // dtAtual.Value = DateTime.Today.Date.AddDays(-3);
                        dtAtual.Value = dtAtual.Value;

                        var nome_pasta = dir_mapa.Split('\\').Last();
                        var name = nome_pasta;
                        if (!nome_pasta.Contains("CPM"))
                        {
                            name = "CPM_" + nome_pasta;
                        }

                        var runRev = ChuvaVazaoTools.Tools.Tools.GetNextRev(dtAtual.Value);

                        var currRev = ChuvaVazaoTools.Tools.Tools.GetCurrRev(dtAtual.Value);

                        IPrecipitacaoForm frm = null;
                        frm = WaitForm2.CreateInstance(dtAtual.Value);

                        PreencherVazObservada(out DateTime dataModelo, out string fonteVaz);

                        var runRevMapas = ChuvaVazaoTools.Tools.Tools.GetNextRev(dtAtual.Value);
                        CarregarPrecRealMedia(dtAtual.Value.Date, out string modeloPrecReal);// runRev.rev.ToString()

                        var pastaRaiz = dir_mapa;
                        var pastaSaida = Path.Combine(dir_saida, name);

                        var pastaBase = @"C:\Files\Middle - Preço\Acompanhamento de vazões\" + currRev.revDate.ToString("MM_yyyy") + @"\Dados_de_Entrada_e_Saida_" + currRev.revDate.ToString("yyyyMM") + "_RV" + currRev.rev.ToString();

                        this.ArquivosDeEntradaModelo = System.IO.Path.Combine(txtEntrada.Text);//Modelos_Chuva_Vazao
                        this.ArquivosDeEntradaPrevivaz = System.IO.Path.Combine(pastaBase, "Previvaz", "Arq_Entrada");
                        this.ArquivoPrevsBase = System.IO.Directory.GetFiles(pastaBase, "prevs.*", SearchOption.AllDirectories)[0];
                        this.DataSemanaPrevsBase = currRev.revDate;

                        ArquivosDeSaida = pastaSaida;

                        var statusF = new RunStatus(pastaSaida);
                        if (statusF.Creation == RunStatus.statuscode.initialialized
                            || statusF.Previvaz == RunStatus.statuscode.initialialized
                            || statusF.PostProcessing == RunStatus.statuscode.initialialized
                            || statusF.Preparation == RunStatus.statuscode.initialialized
                            || statusF.Execution == RunStatus.statuscode.initialialized
                            )
                        {
                            AddLog("Caso em execução: " + name);
                            if (logF != null) logF.WriteLine("Caso em execução: " + name);
                            //return;
                        }
                        else
                        {

                            if ((System.IO.Directory.Exists(pastaSaida) && statusF.PostProcessing == RunStatus.statuscode.completed))
                            {
                                AddLog("Caso já executado para essa data: " + name);
                                logF.WriteLine("Caso já executado para essa data: " + name);


                                //return;
                            }
                            else
                            {
                                logF.WriteLine("Iniciando " + name);

                                if (!System.IO.Directory.Exists(pastaSaida) || statusF.Creation != RunStatus.statuscode.completed)
                                {
                                    statusF.Creation = RunStatus.statuscode.initialialized;
                                    CriarCaso();
                                    statusF.Creation = RunStatus.statuscode.completed;
                                }

                                if (statusF.Preparation != RunStatus.statuscode.completed)
                                {
                                    statusF.Preparation = RunStatus.statuscode.initialialized;

                                    try
                                    {
                                        Ler();

                                        CarregarPrecObserv();
                                        PreencherPrecObserv();

                                        PreencherVazObservada(out DateTime dtVaz, out _);

                                        dtAtual.Value = dataModelo.AddDays(1);
                                        dtModelo.Value = dtAtual.Value.Date;
                                        Reiniciar(dtModelo.Value);


                                        PrecipitacaoPrevista_R(pastaRaiz, pastaSaida);

                                        PreencherPrecObserv();
                                        SalvarPrecObserv_R();
                                        SalvarVazObserv();
                                        SalvarPrecPrev_R();

                                        statusF.Preparation = RunStatus.statuscode.completed;
                                    }
                                    catch
                                    {
                                        statusF.Preparation = RunStatus.statuscode.error;
                                        Falha = true;
                                    }
                                }
                                if (statusF.Execution != RunStatus.statuscode.completed)
                                {
                                    statusF.Execution = RunStatus.statuscode.initialialized;
                                    logF.WriteLine("EXECUTANDO");
                                    try
                                    {
                                        ExecutarTudo_Manual();
                                        if (statusF.Execution == RunStatus.statuscode.error)
                                        {
                                            logF.WriteLine("Erro no SMAP");
                                            return;
                                        }
                                        statusF.Execution = RunStatus.statuscode.completed;
                                    }
                                    catch
                                    {
                                        statusF.Execution = RunStatus.statuscode.error;

                                    }
                                }
                                if (statusF.Execution == RunStatus.statuscode.completed)
                                {


                                    logF.WriteLine("PROCESSANDO RESULTADOS");
                                    try
                                    {
                                        #region Propagacoes sem Excell
                                        try
                                        {
                                            var check = cbx_Encadear_Previvaz.Checked;

                                            List<Propagacao> propagacoes = null;
                                            if (statusF.Preparation == RunStatus.statuscode.completed && statusF.Creation == RunStatus.statuscode.completed && statusF.Previvaz != RunStatus.statuscode.completed)
                                            {
                                                statusF.Collect = RunStatus.statuscode.initialialized;
                                                if (modelosChVz.Count == 0)
                                                    Ler();
                                                propagacoes = new ExecutingProcess().ProcessResultsPart1(modelosChVz, ArquivosDeSaida, dtAtual.Value);
                                                if (propagacoes.Count != 0 || propagacoes != null)
                                                {
                                                    statusF.Execution = RunStatus.statuscode.completed;
                                                    statusF.Collect = RunStatus.statuscode.completed;
                                                }


                                                if (propagacoes.Count != 0 || propagacoes != null)
                                                {
                                                    MemoryStream stream1 = new MemoryStream();
                                                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<Propagacao>));

                                                    ser.WriteObject(stream1, propagacoes);
                                                    stream1.Position = 0;





                                                    if (statusF.Execution == RunStatus.statuscode.completed && statusF.Collect == RunStatus.statuscode.completed)
                                                    {
                                                        File.WriteAllText(Path.Combine(ArquivosDeSaida, "Propagacoes_Automaticas.txt"), new StreamReader(stream1).ReadToEnd());

                                                        statusF.Previvaz = RunStatus.statuscode.initialialized;

                                                        var p = Program.GetPrevivazExPath(Path.Combine(ArquivosDeSaida, "Propagacoes_Automaticas.txt"));

                                                        if (p != null)
                                                        {
                                                            var encad = cbx_Encadear_Previvaz.Checked;
                                                            AddLog("EXECUCAO PREVIVAZ");
                                                            if (logF != null) logF.WriteLine("EXECUCAO PREVIVAZ");
                                                            if (encad)
                                                            {
                                                                var parametro = p.Item2 + "|true";
                                                                var pre = System.Diagnostics.Process.Start(p.Item1, parametro);
                                                                pre.WaitForExit();
                                                            }
                                                            else
                                                            {
                                                                var pr = System.Diagnostics.Process.Start(p.Item1, p.Item2);

                                                                pr.WaitForExit();
                                                            }


                                                            try
                                                            {
                                                                if (System.IO.File.Exists(Path.Combine(ArquivosDeSaida, "Previvaz2.txt")))
                                                                {
                                                                    // var procId = pr.BasePriority;

                                                                    if (statusF != null) statusF.Previvaz = RunStatus.statuscode.completed;
                                                                }
                                                                else
                                                                {
                                                                    statusF.Previvaz = RunStatus.statuscode.error;
                                                                    return;
                                                                }

                                                            }
                                                            catch (Exception e)
                                                            {
                                                                e.ToString();
                                                                statusF.Previvaz = RunStatus.statuscode.error;
                                                                return;
                                                            }
                                                            if (statusF?.Previvaz != RunStatus.statuscode.completed)
                                                            {
                                                                statusF.Previvaz = RunStatus.statuscode.error;
                                                                return;
                                                            }

                                                        }
                                                        else
                                                        {
                                                            if (statusF != null && System.IO.Directory.Exists(Path.Combine(ArquivosDeSaida, "Propagacoes_Automaticas.txt"))) statusF.Previvaz = RunStatus.statuscode.error;
                                                            return;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    statusF.Execution = RunStatus.statuscode.error;
                                                    statusF.Collect = RunStatus.statuscode.error;
                                                    //throw new Exception("As propagações foram enviadas ao método e retornaram vazias ou com erro");
                                                    return;
                                                }
                                            }


                                            if (statusF.Creation == RunStatus.statuscode.completed &&
                                                statusF.Execution == RunStatus.statuscode.completed &&
                                                statusF.Preparation == RunStatus.statuscode.completed &&
                                                statusF.Previvaz == RunStatus.statuscode.completed &&
                                                statusF.PostProcessing != RunStatus.statuscode.completed
                                                )
                                            {
                                                if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.initialialized;

                                                if (System.IO.File.Exists(Path.Combine(ArquivosDeSaida, "Previvaz2.txt")))
                                                {
                                                    var Read = System.IO.File.ReadAllText(Path.Combine(ArquivosDeSaida, "Previvaz2.txt"));
                                                    //testeRead.ReadToEnd();

                                                    DataContractJsonSerializer desser = new DataContractJsonSerializer(typeof(List<Propagacao>));
                                                    MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(Read));
                                                    propagacoes = ((List<Propagacao>)desser.ReadObject(ms)).ToList();

                                                    var prevsRv0Mes2 = ExportaPrevsPorPasta(propagacoes, ArquivosDeSaida, dtAtual.Value, runRev.revDate, revnum);
                                                    var prevsMesAtual = ExportaPrevsPorPasta(propagacoes, ArquivosDeSaida, dtAtual.Value, runRev.revDate, 5);
                                                    ExportaEnas(propagacoes, ArquivosDeSaida);
                                                    if (prevsRv0Mes2 != "")
                                                    {
                                                        try
                                                        {
                                                            var nomeDoCaso = ArquivosDeSaida.Split('\\').Last();
                                                            if (nomeDoCaso.StartsWith("CPM_"))
                                                            {
                                                                var pathDestino = Path.Combine("Z:\\cpas_ctl_common", "auto", DateTime.Today.ToString("yyyyMMdd") + "_" + nomeDoCaso);
                                                                if (!System.IO.Directory.Exists(pathDestino))
                                                                {
                                                                    Directory.CreateDirectory(pathDestino);
                                                                    File.Copy(Path.Combine(ArquivosDeSaida, prevsRv0Mes2), Path.Combine(pathDestino, prevsRv0Mes2));
                                                                }

                                                            }
                                                            else
                                                            {
                                                                if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.error;
                                                                if (statusF != null) statusF.Previvaz = RunStatus.statuscode.error;
                                                                return;
                                                            }
                                                        }
                                                        catch
                                                        {
                                                            if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.error;
                                                            return;
                                                        }

                                                    }
                                                    else
                                                    {
                                                        if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.error;
                                                        return;
                                                    }

                                                    if (!File.Exists(Path.Combine(ArquivosDeSaida, "enasemanal.log")) || !File.Exists(Path.Combine(ArquivosDeSaida, "enadiaria.log")))
                                                    {
                                                        if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.error;
                                                        return;
                                                    }
                                                }
                                                else
                                                {
                                                    if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.error;
                                                    if (statusF != null) statusF.Previvaz = RunStatus.statuscode.error;
                                                    return;
                                                }

                                                try
                                                {
                                                    Salvar_Img(ArquivosDeSaida);
                                                    if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.completed;

                                                }
                                                catch (Exception ex)
                                                {
                                                    statusF.PostProcessing = RunStatus.statuscode.error;
                                                    return;
                                                }

                                                //var email = Tools.Tools.SendMail(Path.Combine(ArquivosDeSaida, "Propagacoes_Automaticas.txt"), "Sucesso ao executar as propagações automáticas!", "Propagações sem Excell [AUTO]", "desenv");
                                                //email.Wait();
                                            }
                                        }
                                        catch (Exception exce)
                                        {
                                            statusF.Execution = RunStatus.statuscode.error;
                                            var email = Tools.Tools.SendMail("", "ERRO: " + exce.Message, "Erro nas propagações sem Excell [AUTO]", "desenv");
                                            email.Wait();
                                        }
                                        #endregion

                                    }
                                    catch
                                    {


                                    }
                                }
                                else
                                {
                                    if (logF != null) logF.WriteLine("SMAPS NAO EXECUTADOS");
                                }

                                if (statusF.Creation == RunStatus.statuscode.error
                               || statusF.Previvaz == RunStatus.statuscode.error
                               || statusF.PostProcessing == RunStatus.statuscode.error
                               || statusF.Preparation == RunStatus.statuscode.error
                               || statusF.Execution == RunStatus.statuscode.error
                               || statusF.Collect == RunStatus.statuscode.error
                               )
                                {
                                    Falha = true;
                                }
                                else
                                {
                                    logF.WriteLine("FINALIZADO");
                                }





                            }
                        }
                    }

                }
                if (Falha == true)
                {
                    conta++;
                }
            } while (Falha == true && conta < 2);
            logF.WriteLine("Rodadas Finalizadas");
            //---------------



        }
        private void ProcessarResultadosManual(string pastaSaida, System.IO.TextWriter logF = null, int? revnum = null, RunStatus statusF = null, bool rodarPrevivaz = true)
        {
            Excel.Workbook wbCen = null;
            Excel.Workbook wb = null;

            var xlsApp = new Microsoft.Office.Interop.Excel.Application();

            try
            {
                if (modelosChVz.Count == 0)
                    Ler();

                int nextRevNum = 0;

                if (!revnum.HasValue)
                {
                    var nextRev = ChuvaVazaoTools.Tools.Tools.GetNextRev(dtAtual.Value);
                    nextRevNum = nextRev.rev;
                }
                else
                    nextRevNum = revnum.Value;

                var currRev = Tools.Tools.GetCurrRev(this.DataSemanaPrevsBase.HasValue ? this.DataSemanaPrevsBase.Value.AddDays(-1) : dtAtual.Value);

                int code = pastaSaida.GetHashCode();

                while (!xlsApp.Ready)
                {
                    System.Threading.Thread.Sleep(200);
                }

                xlsApp.Visible = true;
                xlsApp.ScreenUpdating = true;
                xlsApp.DisplayAlerts = false;

                var pathResult = Path.Combine(pastaSaida, $"CHUVAVAZAO_{code}.xlsm");
                if (!File.Exists(pathResult) || statusF?.Collect != RunStatus.statuscode.completed)
                {

                    if (statusF != null) statusF.Collect = RunStatus.statuscode.initialialized;
                    ColetaDeResultados(xlsApp, out wb);
                    wb.SaveAs(
                        pathResult, wb.FileFormat
                        );


                }
                else
                {
                    wb = xlsApp.Workbooks.Open(pathResult);
                }
                if (statusF != null) statusF.Collect = RunStatus.statuscode.completed;


                var pathCen = Path.Combine(pastaSaida, $"CHUVAVAZAO_CENARIO_{code}.xlsm");


                //Colocar RV AQui
                var prevsname = "prevs.rv" + nextRevNum.ToString();


                if (!File.Exists(Path.Combine(pastaSaida, prevsname)) || !File.Exists(pathCen) || statusF?.Previvaz != RunStatus.statuscode.completed)
                {
                    var encad = cbx_Encadear_Previvaz.Checked;
                    try
                    {
                        if (statusF != null) statusF.Previvaz = RunStatus.statuscode.initialialized;
                        xlsApp.DisplayAlerts = false;

                        xlsApp.Run($"'CHUVAVAZAO_{code}.xlsm'!CriarCenario");

                        wbCen = xlsApp.ActiveWorkbook;

                        while (!xlsApp.Ready)
                        {
                            System.Threading.Thread.Sleep(2000);
                        }

                        try
                        {
                            foreach (Microsoft.Office.Interop.Excel.Name wbName in wbCen.Names)
                            {
                                if (wbName.Visible && wbName.Name == "_gravarPrevivaz") wbName.RefersToRange.Value = true;
                            }
                        }
                        finally { }

                        wbCen.SaveAs(
                            pathCen, wb.FileFormat
                            );

                    }
                    catch
                    {
                        AddLog("Erro criando planilha de cenarios");
                        statusF.Previvaz = RunStatus.statuscode.error;
                        if (logF != null) logF.WriteLine("Erro criando planilha de cenarios");
                        if (wbCen != null) wbCen.Close(SaveChanges: false);
                        wb.Close(SaveChanges: false);

                        return;
                    }
                    finally
                    {
                        if (wbCen != null) wbCen.Close(SaveChanges: false);
                        wbCen = null;

                        wb.Close(SaveChanges: false);
                        wb = null;
                    }


                   
                    if (rodarPrevivaz)
                    {
                        var p = Program.GetPrevivazExPath(pathCen);


                        if (p != null)
                        {
                            if (logF != null) logF.WriteLine("EXECUCAO PREVIVAZ");
                            if (encad)
                            {
                                var teste = p.Item2 + "|true";
                                var pre = System.Diagnostics.Process.Start(p.Item1, p.Item2 + "|true");

                                pre.WaitForExit();
                            }
                            else
                            {

                                var pr = System.Diagnostics.Process.Start(p.Item1, p.Item2);

                                pr.WaitForExit();
                            }
                        }
                        else
                        {
                            if (statusF != null) statusF.Previvaz = RunStatus.statuscode.error;
                            return;
                        }
                    }
                    
                    if (statusF != null) statusF.Previvaz = RunStatus.statuscode.completed;
                }

                if (statusF?.Previvaz != RunStatus.statuscode.completed) return;

                if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.initialialized;

                if (!File.Exists(Path.Combine(pastaSaida, prevsname)))
                    try
                    {

                        wbCen = xlsApp.Workbooks.Open(pathCen, ReadOnly: true);


                        if (nextRevNum == 0 || (nextRevNum == 1 && currRev.rev != 0))
                        {
                            xlsApp.Run($"'CHUVAVAZAO_CENARIO_{code}.xlsm'!ExportarPrevsM1", pastaSaida);
                        }
                        else
                        {
                            xlsApp.Run($"'CHUVAVAZAO_CENARIO_{code}.xlsm'!ExportarPrevs", pastaSaida);
                        }

                        var fprevs = Path.Combine(pastaSaida, "prevs.prv");

                        if (File.Exists(fprevs))
                        {

                            if (File.Exists(Path.Combine(pastaSaida, prevsname))) File.Delete(Path.Combine(pastaSaida, prevsname));

                            if (File.Exists(Path.Combine(pastaSaida, "prevs.prv")))
                                System.IO.File.Move(Path.Combine(pastaSaida, "prevs.prv"), Path.Combine(pastaSaida, prevsname));

                            var nomeDoCaso = pastaSaida.Split('\\').Last();

                            if (nomeDoCaso.StartsWith("CV_") || nomeDoCaso.StartsWith("CV2_"))
                            {
                                //var pathDestino = Path.Combine("L:\\cpas_ctl_common", "auto", DateTime.Today.ToString("yyyyMMdd") + "_" + nomeDoCaso);
                                var pathDestino = Path.Combine("Z:\\cpas_ctl_common", "auto", DateTime.Today.ToString("yyyyMMdd") + "_" + nomeDoCaso);
                                if (!System.IO.Directory.Exists(pathDestino)) Directory.CreateDirectory(pathDestino);
                                File.Copy(Path.Combine(pastaSaida, prevsname), Path.Combine(pathDestino, prevsname));
                                if (System.IO.File.Exists(Path.Combine(pastaSaida, "resumoENA.gif")))
                                    File.Copy(Path.Combine(pastaSaida, "resumoENA.gif"), Path.Combine(pathDestino, "resumoENA.gif"));
                            }
                            if (logF != null) logF.WriteLine(Path.Combine(pastaSaida, prevsname));
                            if (statusF != null) statusF.Previvaz = RunStatus.statuscode.completed;

                        }
                        else //deu ruim na exportação do Prevs. (provavelmente erro na execução do previvaz)
                        {
                            if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.error;
                            if (statusF != null) statusF.Previvaz = RunStatus.statuscode.error;

                            if (logF != null) logF.WriteLine("Erro na execução do Previvaz");

                            if (wbCen != null)
                            {
                                wbCen.Close(SaveChanges: false);
                                if (File.Exists(pathCen)) File.Delete(pathCen);
                                wbCen = null;
                            }

                            throw new Exception();
                        }
                    }
                    catch
                    {
                        if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.error;
                        if (statusF != null) statusF.Previvaz = RunStatus.statuscode.error;
                        if (logF != null) logF.WriteLine("Erro na execução do Previvaz");
                        if (wbCen != null) { wbCen.Close(SaveChanges: false); }
                    }
                if (statusF?.Previvaz != RunStatus.statuscode.completed) return;
                try
                {
                    Salvar_Img(pastaSaida);

                }
                catch (Exception ex)
                {
                    statusF.PostProcessing = RunStatus.statuscode.error;

                }
                if (!File.Exists(Path.Combine(pastaSaida, "enasemanal.log")))
                    try
                    {
                        if (wbCen == null) wbCen = xlsApp.Workbooks.Open(pathCen, ReadOnly: true);

                        var valoresSemanais = wbCen.Worksheets["Cen1"].Range["B14", "N61"].Value as object[,];

                        var enaText = "";
                        for (int i = 1; i <= valoresSemanais.GetLength(0); i++)
                        {
                            for (int j = 1; j <= valoresSemanais.GetLength(1); j++)
                            {
                                enaText += valoresSemanais[i, j]?.ToString() + "\t";
                            }
                            enaText += "\r\n";
                        }

                        File.WriteAllText(Path.Combine(pastaSaida, "enasemanal.log"), enaText);

                    }
                    catch (Exception ex)
                    {

                        AddLog("Erro em exportação de imagem");
                        if (logF != null) logF.WriteLine("Erro em exportação de imagem");
                        AddLog(ex.Message);
                        if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.error;
                    }
                ///DIARIO///
                ///

                if (!File.Exists(Path.Combine(pastaSaida, "enadiaria.log")))
                    try
                    {
                        if (wbCen == null)
                            wbCen = xlsApp.Workbooks.Open(pathCen, ReadOnly: true);

                        if (wb == null)
                            wb = xlsApp.Workbooks.Open(pathResult);

                        var cen1 = wbCen.Worksheets["Cen1"] as Excel.Worksheet;
                        var vals = cen1.Range["_cen1"].Value2;

                        var resPr = wb.Worksheets["PREVIVAZ"] as Excel.Worksheet;

                        resPr.Range["A2", "N321"].Value2 = vals;

                        var wsprevs = wbCen.Worksheets["Prevs"] as Excel.Worksheet;
                        var dats = wsprevs.Range["D3", "O3"].Value2;
                        resPr.Range["C1", "N1"].Value2 = dats;
                        wb.Save();

                        wb.Activate();

                        xlsApp.Run($"'CHUVAVAZAO_{code}.xlsm'!CriarCenarioDiario");
                        Excel.Workbook wbCenDiario = xlsApp.ActiveWorkbook;

                        var valoresDiarios = wbCenDiario.Worksheets["CenDiario"].Range["B14", "AB61"].Value as object[,];

                        try
                        {
                            decimal valu;
                            foreach (var valDia in valoresDiarios)
                            {
                                if (valDia != null)
                                {
                                    if (decimal.TryParse(valDia.ToString(), out valu))
                                        if (valu < 0)
                                            throw new Exception("Erro ao criando Ena diaria");
                                }
                                else
                                    continue;
                            }
                        }
                        catch { }

                        var enaText = "";
                        for (int i = 1; i <= valoresDiarios.GetLength(0); i++)
                        {
                            for (int j = 1; j <= valoresDiarios.GetLength(1); j++)
                            {
                                enaText += valoresDiarios[i, j]?.ToString() + "\t";
                            }
                            enaText += "\r\n";
                        }

                        File.WriteAllText(Path.Combine(pastaSaida, "enadiaria.log"), enaText);

                        if (wbCenDiario != null)
                            wbCenDiario.Close(SaveChanges: false);

                        if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.completed;

                    }
                    catch (Exception ex)
                    {
                        AddLog("Erro em processamento de enas diárias");

                        if (logF != null) logF.WriteLine("Erro em processamento de enas diárias");
                        AddLog(ex.Message);
                        if (statusF != null) statusF.PostProcessing = RunStatus.statuscode.error;
                    }
            }
            finally
            {
                if (wb != null)
                {
                    wb.Saved = true;
                    wb.Close(SaveChanges: false);
                }
                try
                {
                    if (wbCen != null)
                    {
                        wbCen.Saved = true;
                        wbCen.Close(SaveChanges: false);
                    }
                }
                catch (Exception e) { }

                if (xlsApp != null)
                {
                    xlsApp.DisplayAlerts = false;
                    xlsApp.Quit();
                    Helper.Release(xlsApp);
                }
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            chuvas.Clear();
            RefreshPrecipList();
        }

        private void button4_Click(object sender, EventArgs e)
        {

            var logF = textLogger;
            logF.WriteLine("Iniciando AutoRoutine");
            var data = dtAtual.Value.Date;
            logF.WriteLine("Iniciando AutoDownload");
            Program.AutoDownload(data, logF);

            logF.WriteLine("Encerrando AutoRoutine");

        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Remoção De Vies Antiga 
            // 
            var logF = textLogger;
            logF.WriteLine("Iniciando AutoRoutine");
            var data = dtAtual.Value.Date;
            logF.WriteLine("Iniciando AutoRun");
            Program.AutoRun(data, logF);


            logF.WriteLine("Encerrando AutoRoutine");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var runRev = ChuvaVazaoTools.Tools.Tools.GetNextRev(dtAtual.Value);
            var preliminar = false;
            //Tools.Tools.addHistory("C:\\Sistemas\\ChuvaVazao\\Log\\report.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "- tentativa de gerar relatório via botão(button6)");
            var camRelPrev = @"C:\Files\Relatorios\Relatorio Final\Relatorios\" + "Relatorio_de_Previsoes_" + DateTime.Today.ToString("dd_MM_yyyy") + "_Preliminar.pdf";
            var camRelPrevDef = @"C:\Files\Relatorios\Relatorio Final\Relatorios\" + "Relatorio_de_Previsoes_" + DateTime.Today.ToString("dd_MM_yyyy") + ".pdf";
            var camRelPrevCompleto = @"C:\Files\Relatorios\Relatorio Final\Relatorios\" + "Relatorio_de_Previsoes_" + DateTime.Today.ToString("dd_MM_yyyy") + "_Completo.pdf";
            // var verPastaPre = @"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\16_Chuva_Vazao\" + runRev.revDate.ToString("yyyy_MM") + @"\RV" + runRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\Mapas Acomph d-1";
            //var verPastaDef = @"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\16_Chuva_Vazao\" + runRev.revDate.ToString("yyyy_MM") + @"\RV" + runRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\Mapas Acomph";
            var verPastaPre = @"C:\Files\Middle - Preço\16_Chuva_Vazao\" + runRev.revDate.ToString("yyyy_MM") + @"\RV" + runRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\Mapas Acomph d-1";
            var verPastaDef = @"C:\Files\Middle - Preço\16_Chuva_Vazao\" + runRev.revDate.ToString("yyyy_MM") + @"\RV" + runRev.rev.ToString() + @"\" + DateTime.Now.ToString("yy-MM-dd") + @"\Mapas Acomph";

            var horaRel = @"C:\Files\Relatorios\Relatorio Final\Relatorios";
            var relprevPre = "Relatorio_de_Previsoes_" + DateTime.Today.ToString("dd_MM_yyyy") + "_Preliminar.pdf";
            var relprevDef = "Relatorio_de_Previsoes_" + DateTime.Today.ToString("dd_MM_yyyy") + ".pdf";

            if (!File.Exists(camRelPrev) && DateTime.Now >= DateTime.Today.AddSeconds(27000) && File.Exists(Path.Combine(verPastaPre, "logC.txt")) || !File.Exists(camRelPrev) && DateTime.Now >= DateTime.Today.AddHours(9))
            {
                var h = Tools.Tools.readHistory(Path.Combine(horaRel, "RelatoriosPrev_log.txt")).Last();

                var d = Convert.ToDateTime(h);

                if (d <= DateTime.Now.AddMinutes(-10))
                {
                    Report.Program.CriarRelatorioPrevs(dtAtual.Value, camRelPrev, true);
                    if (File.Exists(camRelPrev))
                    {
                        var retornoEmail = Tools.Tools.SendMail(camRelPrev, "Relatório preliminar de previsões disponível", "Relatório de previsões [AUTO]", "precoSergioVini");
                        retornoEmail.Wait(150000);
                        Tools.Tools.addHistory(Path.Combine(horaRel, "RelatoriosPrev_log.txt"), DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
                    }

                }

            }
            //-----

           /* if (File.Exists(camRelPrev) && !File.Exists(camRelPrevDef))
            {
                var h = Tools.Tools.readHistory(Path.Combine(horaRel, "RelatoriosPrev_log.txt")).Last();

                var d = Convert.ToDateTime(h);

                if (d <= DateTime.Now.AddMinutes(-10))
                {
                    Report.Program.CriarRelatorioPrevs(dtAtual.Value, camRelPrevDef);

                    if (File.Exists(camRelPrevDef))
                    {
                        var retornoEmail = Tools.Tools.SendMail(camRelPrevDef, "Relatório de previsões disponível", "Relatório de previsões [AUTO]", "precoSergio");
                        retornoEmail.Wait(150000);
                        Tools.Tools.addHistory(Path.Combine(horaRel, "RelatoriosPrev_log.txt"), DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
                    }


                }
            }*/

        /*    if (File.Exists(camRelPrev) && File.Exists(camRelPrevDef) && !File.Exists(camRelPrevCompleto))
            {
                var h = Tools.Tools.readHistory(Path.Combine(horaRel, "RelatoriosPrev_log.txt")).Last();

                var d = Convert.ToDateTime(h);

                if (d <= DateTime.Now.AddMinutes(-10))
                {
                    Report.Program.CriarRelatorioPrevs2(dtAtual.Value, camRelPrevCompleto);

                    if (File.Exists(camRelPrevCompleto))
                    {
                        var retornoEmail = Tools.Tools.SendMail(camRelPrevCompleto, "Relatório de previsões disponível", "Relatório de previsões [AUTO]", "precoSergio");
                        retornoEmail.Wait(150000);
                        Tools.Tools.addHistory(Path.Combine(horaRel, "RelatoriosPrev_log.txt"), DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
                    }


                }
            }*/

        /*    SaveFileDialog svd = new SaveFileDialog();
            if (File.Exists($"Relatorio_Compass_{dtAtual.Value.Date:dd_MM_yyyy}_(0 hrs)_Preliminar.pdf"))
            {
                svd.FileName = $"Relatorio_Compass_{dtAtual.Value.Date:dd_MM_yyyy}_(0 hrs).pdf";
                preliminar = false;
            }
            else
            {
                svd.FileName = $"Relatorio_Compass_{dtAtual.Value.Date:dd_MM_yyyy}_(0 hrs)_Preliminar.pdf";
                preliminar = true;
            }

            svd.OverwritePrompt = true;

            if (svd.ShowDialog() == DialogResult.OK)
            {
                Tools.Tools.addHistory("C:\\Sistemas\\ChuvaVazao\\Log\\report.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "- tentativa de gerar relatório via botão(button6)");
                var reportFile = svd.FileName;
                Report.Program.CriarRelatorio2(dtAtual.Value, reportFile, preliminar);

                if (File.Exists(reportFile))
                {
                    if (MessageBox.Show("Deseja enviar o relatório por email?", "Relatório", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        var resultEmail = ChuvaVazaoTools.Tools.Tools.SendMail(reportFile, "Relatório de acompanhamento disponível", "Relatório de Acompanhamento [AUTO]", "precoSergio");
                        resultEmail.Wait(150000);
                    }
                }

            }*/
        }

        private void Button8_Click(object sender, EventArgs e)
        {
            //if (listView_PrecPrev.SelectedItems.Count > 0)
            //{
            //    Ookii.Dialogs.VistaFolderBrowserDialog svdiag = new Ookii.Dialogs.VistaFolderBrowserDialog();
            //    //svdiag.FileName = "chuvavazao.log";
            //    //svdiag.OverwritePrompt = true;

            //    //if (!string.IsNullOrWhiteSpace(saveLogFile) || svdiag.ShowDialog() == DialogResult.OK)
            //    //{

            //    var caminho = "C:\\Users\\bruno.araujo.CPASS\\Desktop\\Nova pasta (2)";
            //    cptec.CreateCustomImages(DateTime.Today.AddDays(-1), caminho, "teste");






            //    //foreach (var prec in this.ChuvaConjunto)
            //    //{
            //    //    PrecipitacaoFactory.SalvarModeloBin(prec.Value,
            //    //        System.IO.Path.Combine(caminho,
            //    //        "pp" + Date.ToString("yyyyMMdd") + "_" + ((prec.Key - Date).TotalHours).ToString("0000")
            //    //        )
            //    //    );
            //    //}

            //    //var header = "Prec: " + (this.Tipo == WaitForm.TipoConjunto.Conjunto ? "ETA" + this.Eta.ToString() + " GEFS" + this.Gefs.ToString() : (
            //    //        this.Tipo == WaitForm.TipoConjunto.Eta40 ? "ETA" + this.Eta.ToString() : "GEFS" + this.Gefs.ToString()
            //    //     ));
            //    //+
            //    //" flags (vies-limite):" + this.RemoveViesETA.ToString() + " " + this.RemoveViesGEFS + " - " + this.RemoveLimiteETA.ToString() + " " + this.RemoveLimiteGEFS.ToString();


            //    //}
            //}
        }

        private void AutoDev_Click(object sender, EventArgs e)
        {
            var logF = textLogger;
            logF.WriteLine("Iniciando rodada digital");
            var data = dtAtual.Value.Date;
            logF.WriteLine("Iniciando...");
            var encad = cbx_Encadear_Previvaz.Checked;
            Program.AutoExec(data, logF, encad);


            logF.WriteLine("Encerrando rodada digital");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            try
            {

                Program.Convert_BinDat("ECMWF");
                MessageBox.Show("Realizado com Sucesso");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao converter: " + ex.ToString());
            }
        }

        private void bt_execR_Click(object sender, EventArgs e)
        {
            ParteB_R();
        }

        public void Renomear_Eta40()
        {
            var dir = System.IO.Path.Combine(txtCaminho.Text);

            var dirMod = System.IO.Path.Combine(dir, "SMAP");

            string[] dir_Bacias = Directory.GetDirectories(dirMod);

            foreach (string bacias in dir_Bacias)
            {

                if (Directory.Exists(dirMod))
                {

                    string bacia = bacias.Split('\\').Last();
                    string[] arquivos_PMEDIA = Directory.GetFiles(Path.Combine(dirMod, bacia, "ARQ_ENTRADA"), "*_PMEDIA.txt");
                    string[] arquivos_ETA40 = Directory.GetFiles(Path.Combine(dirMod, bacia, "ARQ_ENTRADA"), "*_ETA40.txt");

                    foreach (string arq in arquivos_ETA40)
                    {
                        int t = arq.Split('\\').Last().Split('_').Length;
                        if (arq.Split('\\').Last().Split('_').Length < 3)
                        {

                            if (File.Exists(arq.Replace("ETA40", "PMEDIA")))
                            {
                                File.Delete(arq);
                            }

                        }
                    }

                    foreach (string arq in arquivos_PMEDIA)
                    {
                        File.Move(arq, arq.Replace("PMEDIA", "ETA40"));

                    }


                }
            }




        }

        public void Mapas_R(string[] mapas)
        {
            var dir = System.IO.Path.Combine(txtCaminho.Text);

            var dirMod = System.IO.Path.Combine(dir, "SMAP");


            foreach (string arq_mapas in mapas)
            {

                System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"p(\d{2})(\d{2})(\d{2})a(\d{2})(\d{2})(\d{2})");

                var data_mapa = r.Match(arq_mapas);

                string mapa = data_mapa.ToString() + ".dat";

                if (!File.Exists(Path.Combine(dir, mapa)))
                {

                    File.Copy(arq_mapas, Path.Combine(dir, mapa));
                }
            }

            string[] dir_Bacias = Directory.GetDirectories(dirMod);

            foreach (string bacias in dir_Bacias)
            {

                if (Directory.Exists(dirMod))
                {

                    string bacia = bacias.Split('\\').Last();
                    var dir_Dest = Path.Combine(dirMod, bacia, "ARQ_ENTRADA");


                    foreach (string arq_mapas in mapas)
                    {
                        System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"p(\d{2})(\d{2})(\d{2})a(\d{2})(\d{2})(\d{2})");

                        var data_mapa = r.Match(arq_mapas);

                        string mapa = "PMEDIA_ORIG_" + data_mapa.ToString() + ".dat";

                        if (!File.Exists(Path.Combine(dir_Dest, mapa)))
                        {

                            File.Copy(arq_mapas, Path.Combine(dir_Dest, mapa));
                        }
                    }




                }
            }




        }

        public void Mapas_R(string[] mapas, int count, string pastaSaida)
        {
            var dir = System.IO.Path.Combine(pastaSaida);

            var dirMod = System.IO.Path.Combine(dir, "SMAP");
            string[] dir_Bacias = Directory.GetDirectories(dirMod);

            DateTime data_atual = DateTime.Today;
            var dirAcomp = Path.Combine(@"C:\Files\Middle - Preço\Acompanhamento de vazões\ACOMPH\1_historico", data_atual.ToString("yyyy"), data_atual.ToString("MM_yyyy"));

            var DataAcomp = Path.Combine(dirAcomp, "ACOMPH_" + data_atual.ToString("dd-MM-yyyy") + ".xls");


            foreach (string arq_mapas in mapas)
            {
                System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"p(\d{2})(\d{2})(\d{2})a(\d{2})(\d{2})(\d{2})");

                var data_mapa = r.Match(arq_mapas);

                string mapa = data_mapa.ToString() + ".dat";

                if (!File.Exists(Path.Combine(dir, mapa)))
                {
                    // if (!File.Exists(DataAcomp) && count == 1)
                    //  {
                    //       psat_Dat(Path.Combine(dir));
                    //   }

                    File.Copy(arq_mapas, Path.Combine(dir, mapa));
                }
            }



            foreach (string bacias in dir_Bacias)
            {

                if (Directory.Exists(dirMod))
                {

                    string bacia = bacias.Split('\\').Last();
                    var dir_Dest = Path.Combine(dirMod, bacia, "ARQ_ENTRADA");


                    foreach (string arq_mapas in mapas)
                    {
                        System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"p(\d{2})(\d{2})(\d{2})a(\d{2})(\d{2})(\d{2})");

                        var data_mapa = r.Match(arq_mapas);

                        string mapa = "PMEDIA_ORIG_" + data_mapa.ToString() + ".dat";

                        if (!File.Exists(Path.Combine(dir_Dest, mapa)))
                        {
                            //  if (!File.Exists(DataAcomp) && count == 1)
                            //  {
                            //       psat_Dat(Path.Combine(dir_Dest));
                            //    }

                            File.Copy(arq_mapas, Path.Combine(dir_Dest, mapa));
                        }
                    }




                }
            }
        }

        public void SalvarPrecObserv_R()
        {
            foreach (var modelo in modelosChVz)
            {
                modelo.SalvarPrecObservada();
            }
            /*
            
            var remo = new PrecipitacaoConjunto(Config.ConfigConjunto);
            var chuvaMedia = remo.MediaBacias(chuvas.Where(x => x.Key <= dtAtual.Value.Date).ToDictionary(x => x.Key, x => x.Value));
            //chuvas = chuvaMedia;
            //RefreshPrecipList();
            
            var dadoslog = new StringBuilder();
            var header = "Precipitacao média";
            dadoslog.AppendLine(header);
            dadoslog.AppendLine("Bacia\t" + string.Join("\t", chuvaMedia.Keys.Select(x => x.ToString("yyyy-MM-dd"))));
            foreach (var pCo in remo.RegioesConjunto.Where(x => x.Modelo == "CONJ").GroupBy(x => x.Agrupamento))
            {
                dadoslog.Append(pCo.Key.Nome + "\t");
                dadoslog.AppendLine(string.Join("\t", pCo.First().precMedia.Select(x => x.ToString("0.00"))));
            }

            File.WriteAllText(System.IO.Path.Combine(this.ArquivosDeSaida, "chuvamediaObservada.log"), dadoslog.ToString());
            */


            AddLog("Arquivos de Preciptação Observada Salvos");
        }

        public void SalvarPrecPrev_R()
        {
            if (chuvas.Count == 0)
            {
                AddLog("Selecione as chuvas");
            }

            var img = false;
            if (String.IsNullOrWhiteSpace(this.ArquivosDeSaida) || !System.IO.Directory.Exists(this.ArquivosDeSaida))
            {
                SelecionarSaida();
                img = true;
            }

            if (String.IsNullOrWhiteSpace(this.ArquivosDeSaida) || !System.IO.Directory.Exists(this.ArquivosDeSaida))
            {
                return;
            }

            if (img)
            {
                this.Busy = true;
                /*
                foreach (var prec in chuvas.Where(x => x.Key > dtAtual.Value.Date))
                {
                    PrecipitacaoFactory.SalvarModeloBin(prec.Value,
                        System.IO.Path.Combine(this.ArquivosDeSaida,
                        "pp" + dtAtual.Value.ToString("yyyyMMdd") + "_" + ((prec.Key - dtAtual.Value).TotalHours).ToString("0000")
                        )
                    );
                }
                */
                cptec.CreateCustomImages(dtAtual.Value, this.ArquivosDeSaida, this.txtNomeChuvaPrev.Text);

                var remo = new PrecipitacaoConjunto(Config.ConfigConjunto);
                //var dic = new Dictionary<DateTime, Precipitacao>();
                // dic[pr.Data] = pr;
                ///
                var chuvaMEDIA = remo.ConjuntoLivre(chuvas, null);
                /// 
                //var chuvaMedia = remo.MediaBacias(chuvas);

                var dadoslog = new StringBuilder();

                var header = "Precipitacao média";

                dadoslog.AppendLine(header);
                foreach (var pCo in remo.RegioesConjunto.Where(x => x.Modelo == "CONJ"))
                {
                    dadoslog.Append(pCo.Agrupamento.Nome + "\t" + pCo.Nome + "\t");
                    dadoslog.AppendLine(string.Join("\t", pCo.precMedia.Select(x => x.ToString("0.00"))));
                }

                File.WriteAllText(System.IO.Path.Combine(this.ArquivosDeSaida, "chuvamedia.log"), dadoslog.ToString());

                this.ArquivosDeSaida = "";

                this.Busy = false;
            }
            else
            {


                foreach (var modelo in modelosChVz)
                {
                    modelo.DataPrevisao = dtAtual.Value.Date;
                    modelo.SalvarPrecPrevista_R(chuvas);
                    modelo.SalvarParametros();
                }
                /*
                var remo = new PrecipitacaoConjunto(Config.ConfigConjunto);
                var chuvaMedia = remo.MediaBacias(chuvas.Where(x => x.Key > dtAtual.Value.Date).ToDictionary(x => x.Key, x => x.Value));
                //chuvas = chuvaMedia;
                //RefreshPrecipList();
                var dadoslog = new StringBuilder();
                var header = "Precipitacao média";
                dadoslog.AppendLine(header);
                dadoslog.AppendLine("Bacia\t" + string.Join("\t", chuvaMedia.Keys.Select(x => x.ToString("yyyy-MM-dd"))));
                foreach (var pCo in remo.RegioesConjunto.Where(x => x.Modelo == "CONJ").GroupBy(x => x.Agrupamento))
                {
                    dadoslog.Append(pCo.Key.Nome + "\t");
                    dadoslog.AppendLine(string.Join("\t", pCo.First().precMedia.Select(x => x.ToString("0.00"))));
                }

                File.WriteAllText(System.IO.Path.Combine(this.ArquivosDeSaida, "chuvamedia.log"), dadoslog.ToString());
                */
            }

            AddLog("- Precipitação Prevista Salva");
        }

        private void bt_MapasR_Click(object sender, EventArgs e)
        {
            PrecipitacaoPrevista_R();
        }

        private void bt_RunR_Click(object sender, EventArgs e)
        {
            var encad = cbx_Encadear_Previvaz.Checked;
            var logF = textLogger;
            logF.WriteLine("Iniciando AutoRoutine Modelo R");
            var data = dtAtual.Value.Date;
            logF.WriteLine("Iniciando AutoRun Modelo R");
            Program.AutoRun_R(data, logF);


            logF.WriteLine("Encerrando AutoRoutine Modelo R");

        }



        public void Salvar_Img(string Path_entrada, Boolean relatorio = false)
        {
            //Pastas onde se encontram os .Dat
            // var arq_saida = System.IO.Path.Combine(Path_entrada, "Arq_Saida");
            var arq_saida = System.IO.Path.Combine(Path_entrada);
            //Lendos os Diretorios do Arq_Saida


            //Pasta Temporaria para manipulação dos arquivos
            var localPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Convert_Imgs", DateTime.Now.ToString("HHmmss"));

            if (Directory.Exists(localPath))
            {
                Directory.Delete(localPath, true);
            }
            System.IO.Directory.CreateDirectory(localPath);


            //Lendo Arquivos .dat do diretorio
            //var files = Directory.GetFiles(System.IO.Path.Combine(arq_saida, nomeDir), "*.dat");
            var files = Directory.GetFiles(System.IO.Path.Combine(arq_saida), "*.dat");


            //Convertendo Cada arquivos para um .dat com pontos de grade do GEFS (Existe um modelo Base nos arquivos do Projeto Base_GEFS.dat)
            foreach (string file in files)
            {
                DatGrads(file, localPath);
            }

            //Transformando Arquivos .Dat para .Ctl e em seguida IMG
            datTOctl(localPath, Path_entrada, relatorio);//, nomeDir

            // Apagando pasta temporaria
            System.IO.Directory.Delete(localPath, true);

        }


        public void Salvar_Img_vies(string Path_entrada)
        {
            //Pastas onde se encontram os .Dat
            var arq_saida = System.IO.Path.Combine(Path_entrada, "Arq_Saida");

            //Lendos os Diretorios do Arq_Saida
            //var Dirs = Directory.GetDirectories(arq_saida, "vies*");

            //Pasta Temporaria para manipulação dos arquivos
            var localPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Convert_Imgs");



            // Pegando apenas o nome do Diretorio



            if (Directory.Exists(localPath))
            {
                Directory.Delete(localPath, true);
            }
            System.IO.Directory.CreateDirectory(localPath);


            //Lendo Arquivos .dat do diretorio
            var files = Directory.GetFiles(System.IO.Path.Combine(arq_saida), "*.dat");



            //Convertendo Cada arquivos para um .dat com pontos de grade do GEFS (Existe um modelo Base nos arquivos do Projeto Base_GEFS.dat)
            foreach (string file in files)
            {
                DatGrads(file, localPath);
            }

            //Transformando Arquivos .Dat para .Ctl e em seguida IMG
            datTOctl(localPath, Path_entrada);//, nomeDir

            // Apagando pasta temporaria
            System.IO.Directory.Delete(localPath, true);

        }

        private void datTOctl(string Caminho, string path_saida, Boolean Relatorio = false)//, string nome
        {// chamar do download
            //Cria Pasta temporaria para os CTLs
            var localPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "GIFSCTLS", DateTime.Now.ToString("HHmmss"));
            if (Directory.Exists(localPath))
            {
                Directory.Delete(localPath, true);
            }
            Directory.CreateDirectory(localPath);

            var path = Caminho;
            var pathsaida = Path.Combine(path_saida, "IMAGENS");//, nome
            if (!Directory.Exists(pathsaida))
                Directory.CreateDirectory(pathsaida);

            //Ler os .dat já no ponto de grade do GEFS
            var files = Directory.GetFiles(Path.Combine(path), "*.dat");


            //Carrega as Chuvas no Chuva vazao
            foreach (string file in files)
            {

                System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"a(\d{2})(\d{2})(\d{2})");

                var fMatch = r.Match(file);
                if (fMatch.Success)
                {

                    //var horas = int.Parse(fMatch.Groups[4].Value);
                    var data = new DateTime(
                        int.Parse(fMatch.Groups[3].Value) + 2000,
                        int.Parse(fMatch.Groups[2].Value),
                        //int.Parse(fMatch.Groups[3].Value)).AddHours(horas).Date
                        int.Parse(fMatch.Groups[1].Value))
                        ;

                    this.chuvas[data] = PrecipitacaoFactory.BuildFromEtaFile(file);
                    this.chuvas[data].Descricao = "PREV NUM - " + System.IO.Path
                        .GetFileName(file);

                }

            }

            //Grava o CTL para cada Chuva
            int conta = 1;
            foreach (var prec in this.chuvas.Where(x => x.Key > dtAtual.Value.Date))
            {
                PrecipitacaoFactory.SalvarModeloBin(prec.Value,
                    System.IO.Path.Combine(localPath,
                    "pp" + Convert.ToDateTime(prec.Key).ToString("yyyyMMdd")
                    )
                );


            }

            var ctlfiles = Directory.GetFiles(localPath, "*.ctl");




            foreach (string ctlfile in ctlfiles)
            {
                System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"pp(\d{4})(\d{2})(\d{2})");

                var fMatch = r.Match(ctlfile);
                if (fMatch.Success)
                {

                    //var horas = int.Parse(fMatch.Groups[4].Value);
                    var data = new DateTime(
                        int.Parse(fMatch.Groups[1].Value),
                        int.Parse(fMatch.Groups[2].Value),
                        //int.Parse(fMatch.Groups[3].Value.AddHours(horas).Date
                        int.Parse(fMatch.Groups[3].Value))
                        ;
                    var NomeArq = Path.Combine(localPath, "prev" + conta);
                    //File.Move(ctlfile, NomeArq+".ctl");
                    if (Relatorio == true)
                    {
                        string complemento = " acumulada entre 12Z " + dtAtual.Value.AddDays(conta - 1).ToString("dd/MM") + " ate " + dtAtual.Value.AddDays(conta).ToString("dd/MM");
                        GradsHelper.Grads.ConvertCtlToImgGEFS(ctlfile, "Modelo Por Conjunto", "Previsao das 00Z " + dtAtual.Value.ToString("dd/MM"), NomeArq + ".gif", System.IO.Path.Combine(Config.CaminhoAuxiliar, "CREATE_GIF.gs"), complemento);
                    }

                    else
                        GradsHelper.Grads.ConvertCtlToImgGEFS(ctlfile, "Remocao de Vies", "Previsao das 00Z " + dtAtual.Value.ToString("dd/MM") + " e " + data.ToString("dd/MM"), NomeArq + ".gif", System.IO.Path.Combine(Config.CaminhoAuxiliar, "CREATE_GIF.gs"));
                    conta++;
                }
            }


            // Copia as imagens da pasta temporaria para o destino 
            cptec.CopyGifs(localPath, pathsaida);
            if (Relatorio == true)
            {
                var data = DateTime.Today;
                var oneDrive_equip = Path.Combine(@"B:\Compass\MinhaTI\Preço - Documents\Acompanhamento_de_Precipitacao");
                var oneDrive_Gif = Path.Combine(oneDrive_equip, "Mapas", data.ToString("yyyy"), data.ToString("MM"), data.ToString("dd"), "CONJUNTO00PREV");

                var path_relatorio = Path.Combine(@"C:\Files\Trading\Acompanhamento Metereologico Semanal\spiderman", data.ToString("yyyy_MM_dd"), @"CONJUNTO00PREV");

                DirectoryCopy(pathsaida, path_relatorio, true);
                DirectoryCopy(pathsaida, oneDrive_Gif, true);
            }


            //Apaga a pasta temp
            System.IO.Directory.Delete(localPath, true);

            //Descarrega as chuvas do Chuva Vazão
            this.chuvas.Clear();
        }


        private void DatGrads(string arquivo, string PastTemp)
        {
            //Ler arquivo txt com pontos base
            var DirPontos = Config.ConfigPontosBase;

            //Ler .dat Base GEFS
            var Mapa_GEFS = Config.ConfigMapaGEFS;

            //Separa apenas o nome do arquivo de entrada
            var nome_arquivo = arquivo.Split('\\').Last().Split('.').First();

            //Ler a qtd de linhas de cada arquivo
            var linhas_Pontos = System.IO.File.ReadLines(DirPontos).Count();
            var linhas_Vies = System.IO.File.ReadLines(arquivo).Count();
            var linhas_Final = System.IO.File.ReadLines(Mapa_GEFS).Count();

            string[] pontos = new string[linhas_Pontos];
            float[] val_vies = new float[linhas_Vies];
            string[] Final = new string[linhas_Final];


            //Ler e Grava o Arquivo de Entrada
            using (var tr = System.IO.File.OpenText(arquivo))
            {

                int i = 0;
                while (!tr.EndOfStream)
                {
                    var l = tr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (l.Length < 3)
                    {
                        break;
                    }

                    val_vies[i] = float.Parse(l[2], System.Globalization.NumberFormatInfo.InvariantInfo);
                    i++;

                }
            }

            using (var tr = System.IO.File.OpenText(DirPontos))
            {

                int j = 0;
                while (!tr.EndOfStream)
                {

                    var l = tr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (l.Length < 3)
                    {
                        break;
                    }
                    var lon = decimal.Parse(l[0], System.Globalization.NumberFormatInfo.InvariantInfo);
                    var lat = decimal.Parse(l[1], System.Globalization.NumberFormatInfo.InvariantInfo);
                    var val = int.Parse(l[2], System.Globalization.NumberFormatInfo.InvariantInfo);

                    float resu = val_vies[val - 1];
                    pontos[j] = (lon.ToString() + " " + lat.ToString() + " " + resu.ToString());
                    j++;


                }
            }
            int count = 0;

            using (var tr = System.IO.File.OpenText(Mapa_GEFS))
            {
                while (!tr.EndOfStream)
                {
                    var troca = false;
                    var l = tr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (l.Length < 3)
                    {
                        break;
                    }
                    var lon = decimal.Parse(l[0], System.Globalization.NumberFormatInfo.InvariantInfo);
                    var lat = decimal.Parse(l[1], System.Globalization.NumberFormatInfo.InvariantInfo);
                    var val = float.Parse(l[2], System.Globalization.NumberFormatInfo.InvariantInfo);

                    for (int z = 0; z <= pontos.Length - 1; z++)
                    {
                        var psplit = pontos[z].Split(' ');

                        if (lon.ToString().Contains(psplit[0]) && lat.ToString().Contains(psplit[1]))
                        {

                            Final[count] = lon.ToString().Replace(',', '.') + " " + lat.ToString().Replace(',', '.') + " " + psplit[2].Replace(',', '.');
                            troca = true;
                        }


                    }
                    if (troca == false)
                    {
                        Final[count] = lon.ToString().Replace(',', '.') + " " + lat.ToString().Replace(',', '.') + " " + "0.00";//val.ToString().Replace(',', '.');
                    }
                    count++;
                }
            }
            var arquivo_Saida = Path.Combine(PastTemp, nome_arquivo + ".dat"); ;
            //  var p = @"C:\teste\DatGrads\teste.dat";
            System.IO.File.WriteAllLines(arquivo_Saida, Final);

        }

        private void button11_Click(object sender, EventArgs e)
        {
            Run_Manual();

        }

        private void txtEntrada_TextChanged(object sender, EventArgs e)
        {

        }

        private void Cbx_Encadear_Previvaz_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Button12_Click(object sender, EventArgs e)
        {
            var searchPath = "";
            Boolean Falha = false;

            Ookii.Dialogs.VistaFolderBrowserDialog d = new Ookii.Dialogs.VistaFolderBrowserDialog();

            //d.SelectedPath = System.IO.Path.Combine(Config.CaminhoPrevisao, data.ToString("yyyyMM"), data.ToString("dd"));
            //d.SelectedPath = System.IO.Path.Combine(@"\\cgclsfsr03.comgas.local\SoftsPRD1\Compass\Middle - Preço\16_Chuva_Vazao\");
            d.SelectedPath = System.IO.Path.Combine(@"C:\Files\Middle - Preço\16_Chuva_Vazao\");
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                searchPath = d.SelectedPath;

            }
            else
            {
                return;

            }

            Salvar_Img(searchPath, true);
        }

        private void DtAtual_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button13_Click(object sender, EventArgs e)
        {

            Gerar_Mapas_R.Gerar_R(txtCaminho.Text, textLogger);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            autoExecPorPasta();
        }

        private void listLogs_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btn_PorPastaSemPrevivaz_Click(object sender, EventArgs e)
        {
            bool rodarPrevivaz = false;
            Run_Manual(rodarPrevivaz);
        }
    }

    public class TextBoxLogger : TextWriter
    {
        TextBox textBox = null;
        private Form owner;

        public TextBoxLogger(TextBox output, Form owner)
        {
            textBox = output;
            this.owner = owner;
        }
        public override void Write(char value)
        {
            base.Write(value);
            var a = owner.BeginInvoke(new Action(() =>
            {
                textBox.AppendText(value.ToString());
            }));

            owner.EndInvoke(a);
        }
        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }
}

