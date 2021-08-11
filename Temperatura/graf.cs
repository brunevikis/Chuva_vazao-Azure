using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Temperatura {
    public partial class graf : Form {
        public graf() {



            InitializeComponent();


        }

        public graf(Dictionary<DateTime, float> A, Dictionary<DateTime, float> B)
            : this() {

            this.chart1.Series[0].Points.Clear();
            this.chart1.Series[1].Points.Clear();
            this.chart1.Series[2].Points.Clear();
            this.chart1.Series[3].Points.Clear();
            this.chart1.Series[4].Points.Clear();

            this.chart1.Series[2].Points.DataBindXY(A.Keys, A.Values);
            this.chart1.Series[3].Points.DataBindXY(B.Keys, B.Values);


            var mediaAtual = A
                .Where(x => x.Key.Hour >= 9 && x.Key.Hour < 18)
                .GroupBy(x => x.Key.Date)
                // .Select(x => new KeyValuePair<DateTime, double>(x.Key, x.Average(y=>y.Value)))
                .ToDictionary(x => x.Key, x => x.Average(y => y.Value));
            this.chart1.Series[1].Points.DataBindXY(mediaAtual.Keys, mediaAtual.Values);

            var mediaD_4 = B
                .Where(x => x.Key.Hour >= 9 && x.Key.Hour < 18)
                .GroupBy(x => x.Key.Date)
                // .Select(x => new KeyValuePair<DateTime, double>(x.Key, x.Average(y=>y.Value)))
                .ToDictionary(x => x.Key, x => x.Average(y => y.Value));
            this.chart1.Series[0].Points.DataBindXY(mediaD_4.Keys, mediaD_4.Values);


            foreach (var k in mediaAtual.Keys.Union(mediaD_4.Keys)) {

                this.chart1.Series[4].Points.AddXY(k,

                    mediaAtual.ContainsKey(k) && mediaD_4.ContainsKey(k) ?
                    (mediaAtual[k] - mediaD_4[k]) * 10 : 0);
            }






        }
    }
}

