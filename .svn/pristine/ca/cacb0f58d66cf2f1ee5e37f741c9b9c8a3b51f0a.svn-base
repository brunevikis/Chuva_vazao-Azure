using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChuvaVazaoTools {
    public partial class ChuvaViewer : Form {

        PrevViewer frm = new PrevViewer();


        public ChuvaViewer() {
            InitializeComponent();
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e) {

        }

        public void AddView(IEnumerable<Precipitacao> precs, Size? viewrSize = null) {
            frm.Show();
            System.Threading.Thread.Sleep(400);

            FlowLayoutPanel flwPanel = new FlowLayoutPanel() { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };

            foreach (var p in precs) {

                frm.tempData = p.Prec;
                frm.lblData.Text = "Precipitação entre os dias " + p.Data.AddDays(-1).ToShortDateString() + " a " + p.Data.ToShortDateString();
                frm.RenderPrec();

                PictureBox pic = new PictureBox() { SizeMode = PictureBoxSizeMode.StretchImage, Size = viewrSize ?? new Size(360, 417), Image = frm.Image, Tag = p };

                pic.DoubleClick += (object sender, EventArgs e) => {

                    PrevViewer frm2 = new PrevViewer(p);
                    if (frm2.ShowDialog(this) == DialogResult.OK) {
                        ((PictureBox)sender).Image = frm2.Image;
                    }
                };

                flwPanel.Controls.Add(pic);
                //vwr.flowLayoutPanel1.Controls.Add(pic);
            }
            flowLayoutPanel2.Controls.Add(flwPanel);

            frm.Hide();
        }

        private void ChuvaViewer_FormClosing(object sender, FormClosingEventArgs e) {
            if (frm != null) {
                frm.Close();
            }
        }

        internal void Clear() {
            flowLayoutPanel2.Controls.Clear();
        }

        private void flowLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
