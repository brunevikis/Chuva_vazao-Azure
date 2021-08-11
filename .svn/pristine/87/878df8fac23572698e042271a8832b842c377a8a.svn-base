using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ChuvaVazaoTools
{
    public partial class PrevViewer : Form
    {

        static string mode = "ONS";

        public Bitmap Image { get; private set; }

        public PrevViewer()
        {

            InitializeComponent();
            this.cbxColorMode.SelectedItem = mode;
            this.cbxColorMode.SelectedIndexChanged += new System.EventHandler(this.cbxColorMode_SelectedIndexChanged);
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;

            if (mode == "ONS") pictureBox1.Image = ChuvaVazaoTools.Properties.Resources.ONS;
            else pictureBox1.Image = ChuvaVazaoTools.Properties.Resources.GRAD;

        }

        public PrevViewer(Precipitacao prec, string caption = "", bool dialog = false)
            : this()
        {

            this.prec = prec;
            this.Text = caption;
            this.tempData = new Dictionary<Tuple<decimal, decimal>, float>();
            this.lblData.Text = "Precipitação entre os dias " + prec.Data.AddDays(-1).ToShortDateString() + " a " + prec.Data.ToShortDateString();


            foreach (var k in prec.Prec.Keys)
            {
                this.tempData[k] = prec.Prec[k];
            }

        }

        //static GMap.NET.WindowsForms.GMapOverlay baciasOverlay = null;
        static List<GMapPolygon> baciasPoly = null;
        static decimal latmin = -35, latmax = 5, lonmin = -75, lonmax = -35;

        Precipitacao prec;
        internal Dictionary<Tuple<decimal, decimal>, float> tempData;

        public static ChuvaViewer ShowViewer(IEnumerable<Precipitacao>[] precsArr, IWin32Window mainWindow, Size? viewrSize = null, Point? viewPosition = null, string caption = "")
        {

            ChuvaViewer vwr = new ChuvaViewer();
            vwr.Text = caption;

            foreach (var precs in precsArr)
            {
                vwr.AddView(precs, viewrSize);
            }

            if (viewPosition.HasValue) vwr.Location = viewPosition.Value;
            vwr.Show(mainWindow);

            return vwr;
        }


        public static DialogResult ShowViewer(Precipitacao prec, string caption = "", bool dialog = false)
        {

            var frm = new PrevViewer(prec, caption, dialog);

            if (dialog) frm.ShowDialog(null);
            else frm.Show();

            return frm.DialogResult;
        }


        static Color getColorx(float x)
        {
            int a, r, g, b;

            a = 150;

            r = 255;

            g = (int)(((200f - x) / 200f) * 255f);
            b = Math.Max(0, (int)(((50f - x) / 50f) * 255f));


            return Color.FromArgb(a, r, g, b);
        }
        static Color getColor(float val)
        {

            int a, r, g, b;

            a = 175;

            r = 0;
            g = 0;
            b = 0;

            //if (val > 0 && val <= 20) { r = (int)(255 + (-11.75 * val)); g = (int)(255 + (-7.75 * val)); b = (int)(255 + (-2.25 * val)); } else if (val > 20 && val <= 40) { r = (int)(10 + (0.5 * val)); g = (int)(20 + (4 * val)); b = (int)(390 + (-9 * val)); } else if (val > 40 && val <= 100) { r = (int)(-120 + (3.75 * val)); g = (int)(236 + (-1.4 * val)); b = (int)(50 + (-0.5 * val)); } else if (val > 100 && val <= 150) { r = (int)(255 + (0 * val)); g = (int)(248 + (-1.52 * val)); b = (int)(0 + (0 * val)); } else if (val > 150) { r = (int)Math.Max(Math.Min((267 + (-0.08 * val)), 255), 0); g = (int)Math.Max(Math.Min((-202 + (1.48 * val)), 255), 0); b = (int)Math.Max(Math.Min((-321 + (2.14 * val)), 255), 0); }


            if (val < 17.5) { r = (int)(val * -13.429 + (255)); g = (int)(val * -8.857 + (255)); b = (int)(val * -2.571 + (255)); }
            else if (val < 22.5) { r = (int)(val * 16.6 + (-270.5)); g = (int)(val * 30.8 + (-439)); b = (int)(val * -15.4 + (479.5)); }
            else if (val < 35) { r = (int)(val * -1.84 + (144.4)); g = (int)(val * -5.92 + (387.2)); b = (int)(val * -8.24 + (318.4)); }
            else if (val < 45) { r = (int)(val * 17.5 + (-532.5)); g = (int)(val * 5.2 + (-2)); b = (int)(val * 9 + (-285)); }
            else if (val < 125) { r = (int)(val * -0.375 + (271.875)); g = (int)(val * -2.65 + (351.25)); b = (int)(val * -1.5 + (187.5)); }
            else if (val < 250) { r = (int)(val * -0.328 + (266)); g = (int)(val * 0.032 + (16)); b = (int)(val * 2.016 + (-252)); }
            else { r = 184; g = 24; b = 252; }


            //if (val > 0 && val <= 1) { r = (int)(val * -30.00 + 255.0); g = (int)(val * 0.00 + 255.0); b = (int)(val * 0.00 + 255.0); }
            //else if (val > 1 && val <= 5) { r = (int)(val * -11.25 + 236.3); g = (int)(val * -3.75 + 258.8); b = (int)(val * 0.00 + 255.0); }
            //else if (val > 5 && val <= 10) { r = (int)(val * -6.00 + 210.0); g = (int)(val * -6.00 + 270.0); b = (int)(val * -1.00 + 260.0); }
            //else if (val > 10 && val <= 15) { r = (int)(val * -22.00 + 370.0); g = (int)(val * -16.00 + 370.0); b = (int)(val * -2.00 + 270.0); }
            //else if (val > 15 && val <= 20) { r = (int)(val * -4.00 + 100.0); g = (int)(val * -6.00 + 220.0); b = (int)(val * -6.00 + 330.0); }
            //else if (val > 20 && val <= 25) { r = (int)(val * 16.60 + -312.0); g = (int)(val * 30.80 + -516.0); b = (int)(val * -15.40 + 518.0); }
            //else if (val > 25 && val <= 30) { r = (int)(val * -15.80 + 498.0); g = (int)(val * -7.80 + 449.0); b = (int)(val * -25.40 + 768.0); }
            //else if (val > 30 && val <= 40) { r = (int)(val * 5.60 + -144.0); g = (int)(val * -3.50 + 320.0); b = (int)(val * 2.40 + -66.0); }
            //else if (val > 40 && val <= 50) { r = (int)(val * 17.50 + -620.0); g = (int)(val * 5.20 + -28.0); b = (int)(val * 9.00 + -330.0); }
            //else if (val > 50 && val <= 75) { r = (int)(val * 0.00 + 255.0); g = (int)(val * -1.60 + 312.0); b = (int)(val * -2.40 + 240.0); }
            //else if (val > 75 && val <= 100) { r = (int)(val * 0.00 + 255.0); g = (int)(val * -3.84 + 480.0); b = (int)(val * -2.40 + 240.0); }
            //else if (val > 100 && val <= 150) { r = (int)(val * -0.60 + 315.0); g = (int)(val * -1.52 + 248.0); b = (int)(val * 0.00 + 0.0); }
            //else if (val > 150 && val <= 200) { r = (int)(val * 0.52 + 147.0); g = (int)(val * 1.48 + -202.0); b = (int)(val * 2.14 + -321.0); }
            //else if (val > 200 && val <= 250) { r = (int)(val * -1.34 + 519.0); g = (int)(val * -1.40 + 374.0); b = (int)(val * 2.90 + -473.0); }
            //else { r = 184; g = 24; b = 252; }


            return Color.FromArgb(a, r, g, b);
        }
        static Color getColorONS(float val)
        {

            int a, r, g, b;

            a = 175;

            r = 0;
            g = 0;
            b = 0;

            if (val > 0 && val <= 1) { r = 225; g = 255; b = 255; }
            else if (val > 1 && val <= 5) { r = 180; g = 240; b = 250; }
            else if (val > 5 && val <= 10) { r = 150; g = 210; b = 250; }
            else if (val > 10 && val <= 15) { r = 40; g = 130; b = 240; }
            else if (val > 15 && val <= 20) { r = 20; g = 100; b = 210; }
            else if (val > 20 && val <= 25) { r = 103; g = 254; b = 133; }
            else if (val > 25 && val <= 30) { r = 24; g = 215; b = 6; }
            else if (val > 30 && val <= 40) { r = 80; g = 180; b = 30; }
            else if (val > 40 && val <= 50) { r = 255; g = 232; b = 120; }
            else if (val > 50 && val <= 75) { r = 255; g = 192; b = 60; }
            else if (val > 75 && val <= 100) { r = 255; g = 96; b = 0; }
            else if (val > 100 && val <= 150) { r = 225; g = 20; b = 0; }
            else if (val > 150 && val <= 200) { r = 251; g = 94; b = 107; }
            else { r = 184; g = 24; b = 252; }


            return Color.FromArgb(a, r, g, b);
        }


        public void RenderPrec()
        {

            if (gMapControl1.Overlays.Count > 1) gMapControl1.Overlays.RemoveAt(1);

            decimal res = 0;

            var k1 = tempData.Keys.First();
            var k2 = tempData.Keys.Skip(1).First();

            if (k1.Item1 != k2.Item1) res = Math.Abs(k1.Item1 - k2.Item1);
            else res = Math.Abs(k1.Item2 - k2.Item2);

            var polygons = new GMap.NET.WindowsForms.GMapOverlay("prec");
            gMapControl1.Overlays.Add(polygons);

            foreach (var item in tempData)
            {

                var lat = item.Key.Item1;
                var lon = item.Key.Item2;
                var val = item.Value;

                if (lon >= lonmin && lon <= lonmax && lat >= latmin && lat <= latmax)
                {

                    if (val > 0)
                    {


                        var points = new List<GMap.NET.PointLatLng>();
                        points.Add(new PointLatLng((double)lat, (double)lon));
                        points.Add(new PointLatLng((double)(lat + res), (double)lon));
                        points.Add(new PointLatLng((double)(lat + res), (double)(lon + res)));
                        points.Add(new PointLatLng((double)lat, (double)(lon + res)));
                        var polygon = new GMapPolygon(points, "prec") { IsHitTestVisible = true, Tag = item.Key };


                        polygon.Stroke.Width = 0;
                        polygon.Stroke.Color = Color.FromArgb(0, 0, 0, 0);

                        polygon.Fill = new SolidBrush(
                            (string)cbxColorMode.SelectedItem == "ONS" ? getColorONS(val) : getColor(val)
                            );

                        polygons.Polygons.Add(polygon);
                    }
                }
            }

            Image = new Bitmap(447, 518);
            this.panel1.DrawToBitmap(Image, new Rectangle() { Width = 447, Height = 518 });
        }

        static object _lock = new object();
        private void PrevViewer_Load(object sender, EventArgs e)
        {
            PrevViewerOnLoad();

            if (tempData != null) RenderPrec();
        }

        private void PrevViewerOnLoad()
        {
            gMapControl1.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;

            gMapControl1.SetPositionByKeywords("Brasilia, Brazil");
            gMapControl1.ShowCenter = false;
            gMapControl1.SetZoomToFitRect(new RectLatLng((double)latmax, (double)lonmin, (double)(lonmax - lonmin), (double)(latmax - latmin)));


            if (baciasPoly == null)
            {
                lock (_lock)
                {
                    if (baciasPoly == null)
                    {
                        ReadMAPA();
                    }
                }
            }

            if (baciasPoly != null)
            {

                var overlay = new GMap.NET.WindowsForms.GMapOverlay("bacias");

                baciasPoly.ForEach(pol => overlay.Polygons.Add(pol));

                gMapControl1.Overlays.Add(overlay);
            }



        }

        static void ReadMAPA()
        {

            var f = Config.ConfigMapa;
            var baciaBrush = new System.Drawing.SolidBrush(Color.FromArgb(200, Color.DarkGray));

            baciasPoly = new List<GMapPolygon>();

            string bacia = null;
            List<GMap.NET.PointLatLng> points = null;
            using (var sr = System.IO.File.OpenText(f))
                while (!sr.EndOfStream)
                {

                    var l = sr.ReadLine();

                    if (l.StartsWith("#"))
                    {

                        if (bacia != null && points != null)
                        {
                            baciasPoly.Add(new GMapPolygon(points, bacia) { Fill = baciaBrush, IsHitTestVisible = true });
                        }

                        bacia = l.Substring(1);
                        points = new List<GMap.NET.PointLatLng>();
                    }
                    else if (bacia != null && l.Split(';').Length == 2)
                    {

                        points.Add(new PointLatLng(
                                double.Parse(l.Split(';')[0], System.Globalization.NumberFormatInfo.InvariantInfo),
                                double.Parse(l.Split(';')[1], System.Globalization.NumberFormatInfo.InvariantInfo)
                                ));
                    }
                }

            if (bacia != null && points != null)
            {
                baciasPoly.Add(new GMapPolygon(points, bacia) { Fill = baciaBrush, IsHitTestVisible = true });
            }
        }

        private void gMapControl1_OnPolygonClick(GMapPolygon item, MouseEventArgs e)
        {
            MessageBox.Show("Bacia: " + item.Name);

        }

        private void gMapControl1_MouseClick(object sender, MouseEventArgs e)
        {

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                if (this.checkBox1.Checked || this.checkBox2.Checked || this.checkBox3.Checked)
                {

                    var pclick = gMapControl1.FromLocalToLatLng(
                        e.Location.X - gMapControl1.Location.X,
                        e.Location.Y - gMapControl1.Location.Y
                        );

                    var size = (double)numericUpDown1.Value;
                    var incr = (this.checkBox2.Checked ? -1 : 1) * (float)numericUpDown2.Value;
                    var set = this.checkBox3.Checked;


                    tempData.Where(x =>
                        Math.Pow((double)x.Key.Item1 - pclick.Lat, 2d)
                        + Math.Pow((double)x.Key.Item2 - pclick.Lng, 2d)
                         <= Math.Pow(size, 2d))
                         .ToList().ForEach(x =>
                         {
                             var valor = incr + (set ? 0 : tempData[x.Key]);
                             if (valor < 0) valor = 0;
                             tempData[x.Key] = valor;
                         });

                    RenderPrec();
                }
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            if (sender == this.checkBox1)
            {
                this.checkBox2.Checked = false;
                this.checkBox3.Checked = false;
            }
            else if (sender == this.checkBox2)
            {
                this.checkBox1.Checked = false;
                this.checkBox3.Checked = false;
            }
            else if (sender == this.checkBox3)
            {
                this.checkBox1.Checked = false;
                this.checkBox2.Checked = false;
            }



            if (this.checkBox2.Checked || this.checkBox1.Checked || this.checkBox3.Checked)
            {
                gMapControl1.Cursor = Cursors.Hand;
            }
            else
            {
                gMapControl1.Cursor = Cursors.Default;
            }


        }

        private void gMapControl1_OnPolygonEnter(GMapPolygon item)
        {


            if (!string.IsNullOrWhiteSpace(item.Name))
            {
                //label3.Text = "Bacia: " + item.Name;
            }

            if (item.Tag is Tuple<decimal, decimal>)
            {
                lblPrec.Text = tempData[(Tuple<decimal, decimal>)item.Tag].ToString("0.0 mm");
            }
        }



        private void button1_Click(object sender, EventArgs e)
        {
            foreach (var k in tempData.Keys)
            {
                prec.Prec[k] = tempData[k];
            }
            MessageBox.Show("Alterado");

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            //this.panel1.DrawToBitmap(Image, new Rectangle() { Width = 430, Height = 430 });

            this.Close();

        }


        private void cbxColorMode_SelectedIndexChanged(object sender, EventArgs e)
        {



            RenderPrec();

            mode = (string)cbxColorMode.SelectedItem;
            if (mode == "ONS") pictureBox1.Image = ChuvaVazaoTools.Properties.Resources.ONS;
            else pictureBox1.Image = ChuvaVazaoTools.Properties.Resources.GRAD;

        }

        private void btmMoverBaixo_Click(object sender, EventArgs e)
        {

            var tempCopia = new Dictionary<Tuple<decimal, decimal>, float>();

            foreach (var k in tempData.Keys)
            {
                tempCopia[k] = tempData[k];
            }

            foreach (var key in tempCopia.Keys)
            {
                tempData[new Tuple<decimal, decimal>(key.Item1 - 2m, key.Item2)] = tempCopia[key];
            }

            RenderPrec();
        }
        private void btmMoverCima_Click(object sender, EventArgs e)
        {

            var tempCopia = new Dictionary<Tuple<decimal, decimal>, float>();

            foreach (var k in tempData.Keys)
            {
                tempCopia[k] = tempData[k];
            }

            foreach (var key in tempCopia.Keys)
            {
                tempData[new Tuple<decimal, decimal>(key.Item1 + 2m, key.Item2)] = tempCopia[key];
            }

            RenderPrec();
        }

        private void cbxColorMode_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

        private void btmMoverDireita_Click(object sender, EventArgs e)
        {

            var tempCopia = new Dictionary<Tuple<decimal, decimal>, float>();

            foreach (var k in tempData.Keys)
            {
                tempCopia[k] = tempData[k];
            }

            foreach (var key in tempCopia.Keys)
            {
                tempData[new Tuple<decimal, decimal>(key.Item1, key.Item2 + 2m)] = tempCopia[key];
            }

            RenderPrec();
        }
        private void btmMoverEsquerda_Click(object sender, EventArgs e)
        {

            var tempCopia = new Dictionary<Tuple<decimal, decimal>, float>();

            foreach (var k in tempData.Keys)
            {
                tempCopia[k] = tempData[k];
            }

            foreach (var key in tempCopia.Keys)
            {
                tempData[new Tuple<decimal, decimal>(key.Item1, key.Item2 - 2m)] = tempCopia[key];
            }

            RenderPrec();
        }

        private void PrevViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (gMapControl1 != null)
            {
                gMapControl1.Dispose();
                gMapControl1 = null;
            }
        }

    }
}
