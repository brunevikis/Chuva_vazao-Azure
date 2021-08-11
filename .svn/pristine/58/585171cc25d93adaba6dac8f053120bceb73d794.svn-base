using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ChuvaVazaoTools {
    public partial class TempViewer : Form {

        public IEnumerable<Temperatura> Temperaturas { get; set; }

        public bool Offset { get; set; }


        public TempViewer() {
            InitializeComponent();
            this.chart1.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.Bright;
        }

        public TempViewer(params Temperatura[] temps)
            : this() {
            var cidades = temps.SelectMany(x => x.Previsoes.Select(y => y.Key)).Distinct().OrderBy(x => x).ToList();
            comboBox1.DataSource = cidades;
            comboBox1.SelectedIndex = 0;

            var sp = cidades.Where(x => x.Contains("SAO_PAULO")).FirstOrDefault();

            if (sp != null) comboBox1.SelectedItem = sp;

            this.Temperaturas = temps.AsEnumerable();

            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e) {
            RefreshView();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
            RefreshView();
        }



        private void TempViewer_Load(object sender, EventArgs e) {
            RefreshView();
        }

        private void RefreshView() {
            var cidade = comboBox1.SelectedItem.ToString();
            var media = radioButton2.Checked;

            this.chart1.Series.Clear();

            this.chart1.Titles.Clear();
            this.chart1.Titles.Add(cidade);


            if (radioButton3.Checked) {
                this.chart1.Series.Add(BuildSeriesV(
                    Temperaturas.Where(x => x.Previsoes.Any(y => y.Key == cidade))
                    .OrderByDescending(x => x.Arquivo)
                    .Select(x => x.Previsoes.First(y => y.Key == cidade).Value).ToArray()
                    ));
            } else {

                foreach (var temp in Temperaturas.Where(x => x.Previsoes.Any(y => y.Key == cidade))
                    .Select(x => new {
                        Arquivo = x.Arquivo,
                        Previsao = x.Previsoes.First(y => y.Key == cidade).Value
                    }).OrderByDescending(x => x.Arquivo)) {

                    this.chart1.Series.Add(

                        media ? BuildSeriesM(temp.Arquivo, temp.Previsao) : BuildSeriesH(temp.Arquivo, temp.Previsao)


                        );
                }
            }
        }

        private Series BuildSeriesH(string p, TemperaturaCidade temps) {
            Series series1 = new Series();
            //series1.ChartArea = "ChartArea3";
            //series1.Legend = "Legend1";
            series1.LegendText = System.IO.Path.GetFileNameWithoutExtension(p);
            //series1.Name = "Series4";
            series1.XValueType = ChartValueType.DateTime;
            series1.YValueType = ChartValueType.Single;
            series1.YValuesPerPoint = 1;
            series1.ChartType = SeriesChartType.Spline;

            series1.Points.DataBindXY(temps.Previsao.Keys, temps.Previsao.Values);

            return series1;
        }

        private Series BuildSeriesM(string p, TemperaturaCidade temps) {


            var media = temps.Previsao
            .Where(x => x.Key.Hour >= 9 && x.Key.Hour < 18)
            .GroupBy(x => x.Key.Date)
                // .Select(x => new KeyValuePair<DateTime, double>(x.Key, x.Average(y=>y.Value)))
            .ToDictionary(x => x.Key, x => x.Average(y => y.Value));


            Series series1 = new Series();
            //series1.ChartArea = "ChartArea3";
            //series1.Legend = "Legend1";
            series1.LegendText = System.IO.Path.GetFileNameWithoutExtension(p);
            //series1.Name = "Series4";
            series1.XValueType = ChartValueType.Date;
            series1.YValueType = ChartValueType.Single;
            series1.YValuesPerPoint = 1;
            series1.ChartType = SeriesChartType.Column;
            series1.BorderWidth = 3;

            series1.Points.DataBindXY(media.Keys, media.Values);

            return series1;
        }

        private Series BuildSeriesV(params TemperaturaCidade[] temps) {

            var tempMedias = new List<Dictionary<DateTime, float>>();

            foreach (var temp in temps) {
                var media = temp.Previsao
                .Where(x => x.Key.Hour >= 9 && x.Key.Hour < 18)
                .GroupBy(x => x.Key.Date)
                    // .Select(x => new KeyValuePair<DateTime, double>(x.Key, x.Average(y=>y.Value)))
                .ToDictionary(x => x.Key, x => x.Average(y => y.Value));

                tempMedias.Add(media);
            }

            //var medias = temps.SelectMany(z => z.Previsao
            //.Where(x => x.Key.Hour >= 9 && x.Key.Hour < 18)
            //.GroupBy(x => x.Key.Date)
            //    // .Select(x => new KeyValuePair<DateTime, double>(x.Key, x.Average(y=>y.Value)))
            //.ToDictionary(x => x.Key, x => x.Average(y => y.Value)));


            var keys = tempMedias.SelectMany(x => x.Keys).Distinct().OrderBy(x => x);


            var data = keys.Select(x => new {
                key = x,

                open = tempMedias.Where(y => y.ContainsKey(x)).First()[x],
                close = tempMedias.Where(y => y.ContainsKey(x)).Last()[x],
                min = tempMedias.Where(y => y.ContainsKey(x)).Min(y => y[x]),
                max = tempMedias.Where(y => y.ContainsKey(x)).Max(y => y[x])
                //close = medias.Where(y => y.Key == x).Last().Value
            }).ToList();

            Series series1 = new Series();
            //series1.ChartArea = "ChartArea3";
            //series1.Legend = "Legend1";
            //series1.Name = "Series4";

            series1.XValueType = ChartValueType.String;
            series1.YValueType = ChartValueType.Single;
            series1.YValuesPerPoint = 4;
            series1.ChartType = SeriesChartType.Candlestick;
            series1.BorderWidth = 3;

            data.ForEach(x => {
                var dp = new DataPoint {

                    Label = (x.close - x.open).ToString("N1"),

                    LabelForeColor = (x.close - x.open) > 0 ? Color.OrangeRed : Color.DarkSeaGreen,

                    XValue = x.key.ToOADate(),
                    YValues = new double[] { x.min, x.max, x.open, x.close },
                    AxisLabel = x.key.ToString("dd - dddd")
                };
                series1.Points.Add(dp);
            });


            return series1;
        }

    }
}
