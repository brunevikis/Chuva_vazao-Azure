using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChuvaVazaoTools
{
    public partial class WaitForm : Form
    {

        Task<WaitForm> waitTask = null;

        bool open = false;

        private WaitForm()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void WaitForm_Load(object sender, EventArgs e)
        {
            open = true;
            waitTask = new Task<WaitForm>(() =>
            {

                while (open) Task.Delay(1000).Wait();

                return this;

            });
        }


        public static Task<WaitForm> ShowAsync( TipoConjunto tipo = TipoConjunto.Conjunto)
        {

            var f = new WaitForm();
            f.Tipo = tipo;

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
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        public TipoConjunto Tipo
        {
            get
            {
                return (TipoConjunto)comboBox1.SelectedIndex;
            }
            set
            {
                switch (value)
                {
                    case TipoConjunto.Eta40:
                    case TipoConjunto.Gefs:
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

        public bool RemoveViesETA { get { return checkBox1.Checked; } }
        public bool RemoveViesGEFS { get { return checkBox2.Checked; } }
        public bool RemoveLimiteETA { get { return checkBox3.Checked; } }
        public bool RemoveLimiteGEFS { get { return checkBox4.Checked; } }

        public enum TipoConjunto : int
        {
            Eta40 = 1,
            Gefs = 2,
            Conjunto = 0
        }

        public bool SalvarDados { get { return chkDados.Checked; } }
    }
}
