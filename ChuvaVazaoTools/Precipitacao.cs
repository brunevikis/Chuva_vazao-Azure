using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ChuvaVazaoTools
{

    public class PrecipitacaoRepository
    {
        public static void SaveAverage(DateTime date, string bacia, string subbacia, float value, string source)
        {

            var entity = new PrecipitacaoMdl() { Data = date, Bacia = bacia, SubBacia = subbacia, DataAtualizacao = DateTime.Now, Fonte = source, Precipitacao1 = value };

            using (var ctx = new IPDOEntities1())
            {

                var exists = ctx.Precipitacoes.Any(x => x.Data == entity.Data && x.Bacia == entity.Bacia && x.SubBacia == entity.SubBacia && x.Fonte == entity.Fonte);


                ctx.Precipitacoes.Attach(entity);

                var e = ctx.Entry(entity);

                if (exists)
                {
                    e.State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    e.State = System.Data.Entity.EntityState.Added;
                }

                ctx.SaveChanges();

            }
        }
    }

    public static class PrecipitacaoFactory
    {
        //BuildFromEta
        public static Precipitacao BuildFromEtaFile(string etaFile)
        {

            var res = new Precipitacao();

            var data = System.Text.RegularExpressions.Regex.Match(etaFile, @"a(\d{2})(\d{2})(\d{2})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (data.Success)
            {
                res.Data = new DateTime(
                    int.Parse(data.Groups[3].Value) + 2000,
                    int.Parse(data.Groups[2].Value),
                    int.Parse(data.Groups[1].Value));
            }
            else
            {
                System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"pp(\d{4})(\d{2})(\d{2})_(\d+)");

                var fMatch = r.Match(etaFile);
                if (fMatch.Success)
                {
                    var horas = int.Parse(fMatch.Groups[4].Value);

                    res.Data = new DateTime(
                        int.Parse(fMatch.Groups[1].Value),
                        int.Parse(fMatch.Groups[2].Value),
                        int.Parse(fMatch.Groups[3].Value)).AddHours(horas).Date
                        ;
                }
            }

            using (var tr = System.IO.File.OpenText(etaFile))
            {

                res.Prec = new Dictionary<Tuple<decimal, decimal>, float>();
                while (!tr.EndOfStream)
                {
                    var l = tr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (l.Length < 3)
                    {
                        break;
                    }

                    var lon = decimal.Parse(l[0], System.Globalization.NumberFormatInfo.InvariantInfo);
                    var lat = decimal.Parse(l[1], System.Globalization.NumberFormatInfo.InvariantInfo);
                    var val = float.Parse(l[2], System.Globalization.NumberFormatInfo.InvariantInfo);
                    if (lon >= Precipitacao.lonmin && lon <= Precipitacao.lonmax && lat >= Precipitacao.latmin && lat <= Precipitacao.latmax)
                    {
                        res.Prec[new Tuple<decimal, decimal>(lat, lon)] = val;
                    }
                }
            }

            return res;

        }

        //BuildFromECMWF
        public static Precipitacao BuildFromECMWFFile(string ecmwfFile)
        {

            var res = new Precipitacao();

            var data = System.Text.RegularExpressions.Regex.Match(ecmwfFile, @"a(\d{2})(\d{2})(\d{2})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (data.Success)
            {
                res.Data = new DateTime(
                    int.Parse(data.Groups[3].Value) + 2000,
                    int.Parse(data.Groups[2].Value),
                    int.Parse(data.Groups[1].Value));
            }
            else
            {
                System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"pp(\d{4})(\d{2})(\d{2})_(\d+)");

                var fMatch = r.Match(ecmwfFile);
                if (fMatch.Success)
                {
                    var horas = int.Parse(fMatch.Groups[4].Value);

                    res.Data = new DateTime(
                        int.Parse(fMatch.Groups[1].Value),
                        int.Parse(fMatch.Groups[2].Value),
                        int.Parse(fMatch.Groups[3].Value)).AddHours(horas).Date
                        ;
                }
            }

            using (var tr = System.IO.File.OpenText(ecmwfFile))
            {

                res.Prec = new Dictionary<Tuple<decimal, decimal>, float>();
                while (!tr.EndOfStream)
                {
                    var l = tr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (l.Length < 3)
                    {
                        break;
                    }

                    var lon = decimal.Parse(l[0], System.Globalization.NumberFormatInfo.InvariantInfo);
                    var lat = decimal.Parse(l[1], System.Globalization.NumberFormatInfo.InvariantInfo);
                    var val = float.Parse(l[2], System.Globalization.NumberFormatInfo.InvariantInfo);
                    if (lon >= Precipitacao.lonminECMWF && lon <= Precipitacao.lonmaxECMWF && lat >= Precipitacao.latminECMWF && lat <= Precipitacao.latmaxECMWF)
                    {
                        res.Prec[new Tuple<decimal, decimal>(lat, lon)] = val;
                    }
                }
            }

            return res;

        }

        //BuildFromGFSNOAA
        
        public static Precipitacao BuildFromGFSNOAA(string GFSFile)
        {

            var res = new Precipitacao();

            var data = System.Text.RegularExpressions.Regex.Match(GFSFile, @"a(\d{2})(\d{2})(\d{2})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (data.Success)
            {
                res.Data = new DateTime(
                    int.Parse(data.Groups[3].Value) + 2000,
                    int.Parse(data.Groups[2].Value),
                    int.Parse(data.Groups[1].Value));
            }
            else
            {
                System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"pp(\d{4})(\d{2})(\d{2})_(\d+)");

                var fMatch = r.Match(GFSFile);
                if (fMatch.Success)
                {
                    var horas = int.Parse(fMatch.Groups[4].Value);

                    res.Data = new DateTime(
                        int.Parse(fMatch.Groups[1].Value),
                        int.Parse(fMatch.Groups[2].Value),
                        int.Parse(fMatch.Groups[3].Value)).AddHours(horas).Date
                        ;
                }
            }

            using (var tr = System.IO.File.OpenText(GFSFile))
            {

                res.Prec = new Dictionary<Tuple<decimal, decimal>, float>();
                while (!tr.EndOfStream)
                {
                    var l = tr.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (l.Length < 3)
                    {
                        break;
                    }

                    var lon = decimal.Parse(l[0], System.Globalization.NumberFormatInfo.InvariantInfo);
                    var lat = decimal.Parse(l[1], System.Globalization.NumberFormatInfo.InvariantInfo);
                    var val = float.Parse(l[2], System.Globalization.NumberFormatInfo.InvariantInfo);
                    if (lon >= Precipitacao.lonminGFS25 && lon <= Precipitacao.lonmaxGFS25 && lat >= Precipitacao.latminGFS25 && lat <= Precipitacao.latmaxGFS25)
                    {
                        res.Prec[new Tuple<decimal, decimal>(lat, lon)] = val;
                    }
                }
            }

            return res;

        }
        //

        //BuildFromMerge
        public static Precipitacao BuildFromMergeFile(string ctlFile)
        {

            var res = new Precipitacao();

            var data = System.Text.RegularExpressions.Regex.Match(ctlFile, @"prec_(\d{4})(\d{2})(\d{2})\.ctl", System.Text.RegularExpressions.RegexOptions.IgnoreCase);


            if (data.Success)
            {
                res.Data = new DateTime(
                    int.Parse(data.Groups[1].Value),
                    int.Parse(data.Groups[2].Value),
                    int.Parse(data.Groups[3].Value));
            }
            else
            {
                System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"pp(\d{4})(\d{2})(\d{2})_(\d+)");

                var fMatch = r.Match(ctlFile);
                if (fMatch.Success)
                {
                    var horas = int.Parse(fMatch.Groups[4].Value);

                    res.Data = new DateTime(
                        int.Parse(fMatch.Groups[1].Value),
                        int.Parse(fMatch.Groups[2].Value),
                        int.Parse(fMatch.Groups[3].Value)).AddHours(horas).Date
                        ;
                }
            }


            var ctl = new ConvertMERGE.Ctl(ctlFile);
            var v = new ConvertMERGE.Ctl.Var() { Name = "prec" };


            res.Prec = new Dictionary<Tuple<decimal, decimal>, float>();

            for (int x = 1; x < ctl.Xdef.Lenght; x += 1)
            {
                var lon = ctl.Xdef.Start + ctl.Xdef.Increment * x;
                for (int y = 0; y < ctl.Ydef.Lenght; y += 1)
                {
                    var lat = ctl.Ydef.Start + ctl.Ydef.Increment * y;
                    if (lon > Precipitacao.lonmin && lon < Precipitacao.lonmax && lat > Precipitacao.latmin && lat < Precipitacao.latmax)
                    {
                        res.Prec[new Tuple<decimal, decimal>(lat, lon)] = ctl.Bin[v, lat, lon];
                    }
                }
            }
            return res;
        }

        public static Precipitacao BuildFromJsonData(Dictionary<Tuple<decimal, decimal>, float> data, DateTime datePrev)
        {
            var res = new Precipitacao();
            res.Data = datePrev;

            res.Prec = new Dictionary<Tuple<decimal, decimal>, float>();

            for (decimal lon = Precipitacao.lonmin; lon < Precipitacao.lonmax; lon += 0.2m)
            {
                for (decimal lat = Precipitacao.latmin; lat < Precipitacao.latmax; lat += 0.2m)
                {
                    var ponto = data.Select(x => new { Dist = Math.Pow((double)(x.Key.Item1 - lat), 2) + Math.Pow((double)(x.Key.Item2 - lon), 2), Posto = x })
                        .OrderByDescending(x => x.Dist).Where(x => x.Dist <= 0.8);

                    float val = 0;

                    if (ponto.Count() > 0)
                    {
                        val = (float)(ponto.Sum(x => x.Posto.Value / (double)x.Dist) / ponto.Sum(x => 1d / (double)x.Dist));

                        ;
                    }
                    res.Prec[new Tuple<decimal, decimal>(lat, lon)] = val;
                }
            }

            res.Descricao = "FUNCEME 24h - " + datePrev.ToShortDateString();

            return res;



        }

        public static Precipitacao BuildFromImage(string imageFile)
        {
            var res = new Precipitacao();

            var data = System.Text.RegularExpressions.Regex.Match(imageFile, @"(\d{4})_(\d{2})_(\d{2})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (data.Success)
            {
                res.Data = new DateTime(
                    int.Parse(data.Groups[1].Value),
                    int.Parse(data.Groups[2].Value),
                    int.Parse(data.Groups[3].Value));
            }

            var prImg = new PrecipitacaoImg(imageFile);

            res.Prec = new Dictionary<Tuple<decimal, decimal>, float>();

            decimal passo = 0.1m;

            for (decimal lon = Precipitacao.lonmin; lon < Precipitacao.lonmax; lon += passo)
            {
                for (decimal lat = Precipitacao.latmin; lat < Precipitacao.latmax; lat += passo)
                {
                    res.Prec[new Tuple<decimal, decimal>(lat, lon)] = prImg.GetVal(lat, lon);
                }
            }

            return res;
        }
        public static Precipitacao BuildFromImage0(string imageFile)
        {
            var res = new Precipitacao();

            var data = System.Text.RegularExpressions.Regex.Match(imageFile, @"(\d{4})_(\d{2})_(\d{2})", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (data.Success)
            {
                res.Data = new DateTime(
                    int.Parse(data.Groups[1].Value),
                    int.Parse(data.Groups[2].Value),
                    int.Parse(data.Groups[3].Value));
            }

            var prImg = new PrecipitacaoImg0(imageFile);

            res.Prec = new Dictionary<Tuple<decimal, decimal>, float>();

            decimal passo = 0.1m;

            for (decimal lon = Precipitacao.lonmin; lon < Precipitacao.lonmax; lon += passo)
            {
                for (decimal lat = Precipitacao.latmin; lat < Precipitacao.latmax; lat += passo)
                {
                    res.Prec[new Tuple<decimal, decimal>(lat, lon)] = prImg.GetVal(lat, lon);
                }
            }

            return res;
        }

        public static Precipitacao BuildFromImage2(string imageFile, DateTime data)
        {
            var res = new Precipitacao();

            res.Data = data;

            var prImg = new PrecipitacaoImg2(imageFile);

            res.Prec = new Dictionary<Tuple<decimal, decimal>, float>();

            var passo = 1m;

            for (decimal lon = -83m; lon < Precipitacao.lonmax; lon += passo)
            {
                for (decimal lat = -50m; lat < Precipitacao.latmax; lat += passo)
                {
                    res.Prec[new Tuple<decimal, decimal>(lat, lon)] = prImg.GetVal(lat, lon, passo);
                }
            }

            return res;
        }

        public static Precipitacao BuildFromImage3(string imageFile, DateTime data)
        {
            var res = new Precipitacao();

            res.Data = data;

            var prImg = new PrecipitacaoImg2(imageFile);

            res.Prec = new Dictionary<Tuple<decimal, decimal>, float>();

            var passo = 0.4m;

            for (decimal lon = -83m; lon < Precipitacao.lonmax; lon += passo)
            {
                for (decimal lat = -50.2m; lat < Precipitacao.latmax; lat += passo)
                {
                    res.Prec[new Tuple<decimal, decimal>(lat, lon)] = prImg.GetVal(lat, lon, passo);
                }
            }

            return res;
        }

        //meteologix
        public static Precipitacao BuildFromImage4(string imageFile, DateTime data)
        {
            var res = new Precipitacao();

            res.Data = data;

            var prImg = new PrecipitacaoImg4(imageFile);

            res.Prec = new Dictionary<Tuple<decimal, decimal>, float>();

            var passo = 0.4m;

            for (decimal lon = -83m; lon < Precipitacao.lonmax; lon += passo)
            {
                for (decimal lat = -50.2m; lat < Precipitacao.latmax; lat += passo)
                {
                    res.Prec[new Tuple<decimal, decimal>(lat, lon)] = prImg.GetVal(lat, lon, passo);
                }
            }

            return res;
        }

        public static void SalvarModeloEta(this Precipitacao precipitacao, string p)
        {
            SalvarModeloEta(precipitacao, p, Precipitacao.latmin, Precipitacao.latmax, Precipitacao.lonmin, Precipitacao.lonmax);
        }
        public static void SalvarModeloEta(this Precipitacao precipitacao, string p, decimal latmin, decimal latmax, decimal lonmin, decimal lonmax)
        {


            decimal thisLatMax = precipitacao.Prec.Keys.Select(x => x.Item1).Max();
            decimal thisLatMin = precipitacao.Prec.Keys.Select(x => x.Item1).Min();
            decimal thisLonMax = precipitacao.Prec.Keys.Select(x => x.Item2).Max();
            decimal thisLonMin = precipitacao.Prec.Keys.Select(x => x.Item2).Min();

            using (var sw = System.IO.File.CreateText(p))
            {

                float precipitation;

                for (decimal lon = lonmin; lon <= lonmax; lon += Precipitacao.etaResolution)
                {
                    for (decimal lat = latmin; lat <= latmax; lat += Precipitacao.etaResolution)
                    {

                        var key = new Tuple<decimal, decimal>(lat, lon);
                        precipitation = 0;

                        if (precipitacao.Prec.ContainsKey(key))
                        {
                            precipitation = precipitacao.Prec[key];
                        }
                        else if (lat >= thisLatMin && lat <= thisLatMax && lon >= thisLonMin && lon <= thisLonMax)
                        {

                            var lt1 = lat - 2 * Precipitacao.etaResolution;
                            var ln1 = lon - 2 * Precipitacao.etaResolution;


                            var keyL = precipitacao.Prec.Keys
                                .Where(x => x.Item1 <= lat && x.Item1 >= lt1)
                                .Where(x => x.Item2 <= lon && x.Item2 >= ln1)
                                .LastOrDefault();
                            //if (keyL.Count() > 0) 
                            if (keyL != null) precipitation = precipitacao.Prec[keyL];
                        }

                        sw.WriteLine(
                                string.Join(" ",
                                lon.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo).PadLeft(6),
                                lat.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo).PadLeft(6),
                                precipitation.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo).PadLeft(6)
                                ));
                    }
                }
            }
        }

        public static void SalvarModeloDAT(this Precipitacao precipitacao, string p, string metodo)
        {
            if (metodo == "GEFS" || metodo == "GFS")
            {
                SalvarModeloDAT(precipitacao, p, metodo, Precipitacao.latminGEFS, Precipitacao.latmaxGEFS, Precipitacao.lonminGEFS, Precipitacao.lonmaxGEFS);
            }

            else if (metodo =="ECMWF")
            {
                SalvarModeloDAT(precipitacao, p, metodo, Precipitacao.latminECMWF, Precipitacao.latmaxECMWF, Precipitacao.lonminECMWF, Precipitacao.lonmaxECMWF);
            }

            else
            {
                SalvarModeloDAT(precipitacao, p, metodo, Precipitacao.latmin, Precipitacao.latmax, Precipitacao.lonmin, Precipitacao.lonmax);
            }
        }
        public static void SalvarModeloDAT(this Precipitacao precipitacao, string p, string metodo, decimal latmin, decimal latmax, decimal lonmin, decimal lonmax)
        {


            decimal thisLatMax = precipitacao.Prec.Keys.Select(x => x.Item1).Max();
            decimal thisLatMin = precipitacao.Prec.Keys.Select(x => x.Item1).Min();
            decimal thisLonMax = precipitacao.Prec.Keys.Select(x => x.Item2).Max();
            decimal thisLonMin = precipitacao.Prec.Keys.Select(x => x.Item2).Min();

            using (var sw = System.IO.File.CreateText(p))
            {

                float precipitation;


                if (metodo == "GEFS" || metodo == "GFS")
                {
                    //GEFS

                    for (decimal lat = latmin; lat <= latmax; lat += Precipitacao.GEFSResolution)
                    {
                        for (decimal lon = lonmin; lon <= lonmax; lon += Precipitacao.GEFSResolution)
                        {
                            var key = new Tuple<decimal, decimal>(lat, lon);
                            precipitation = 0;

                            if (precipitacao.Prec.ContainsKey(key))
                            {
                                precipitation = precipitacao.Prec[key];
                            }
                            else if (lat >= thisLatMin && lat <= thisLatMax && lon >= thisLonMin && lon <= thisLonMax)
                            {

                                //GEFS
                                var keyL = precipitacao.Prec.Keys
                                    .Where(x => x.Item1 == lat)
                                    .Where(x => x.Item2 == lon)
                                    .LastOrDefault();

                                if (keyL != null) precipitation = precipitacao.Prec[keyL];
                            }
                            sw.WriteLine(
                                    string.Join(" ",
                                    lon.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo).PadLeft(6),
                                    lat.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo).PadLeft(6),
                                    precipitation.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo).PadLeft(6)
                                    ));
                        }
                    }
                }
                else
                {
                    for (decimal lon = lonmin; lon <= lonmax; lon += Precipitacao.etaResolution)
                    {
                        for (decimal lat = latmin; lat <= latmax; lat += Precipitacao.etaResolution)

                        {

                            var key = new Tuple<decimal, decimal>(lat, lon);
                            precipitation = 0;

                            if (precipitacao.Prec.ContainsKey(key))
                            {
                                precipitation = precipitacao.Prec[key];
                            }
                            else if (lat >= thisLatMin && lat <= thisLatMax && lon >= thisLonMin && lon <= thisLonMax)
                            {

                                var lt1 = lat - 2 * Precipitacao.etaResolution;
                                var ln1 = lon - 2 * Precipitacao.etaResolution;


                                var keyL = precipitacao.Prec.Keys
                                    .Where(x => x.Item1 <= lat && x.Item1 >= lt1)
                                    .Where(x => x.Item2 <= lon && x.Item2 >= ln1)
                                    .LastOrDefault();


                                //if (keyL.Count() > 0) 
                                if (keyL != null) precipitation = precipitacao.Prec[keyL];
                            }

                            sw.WriteLine(
                                    string.Join(" ",
                                    lon.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo).PadLeft(6),
                                    lat.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo).PadLeft(6),
                                    precipitation.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo).PadLeft(6)
                                    ));
                        }
                    }
                }
            }
        }





        public static void SalvarModeloBin(this Precipitacao precipitacao, string p)
        {
            SalvarModeloBin(precipitacao, p, Precipitacao.latmin, Precipitacao.latmax, Precipitacao.lonmin, Precipitacao.lonmax);
        }

        public static void SalvarModeloBin(this Precipitacao precipitacao, string p, decimal latmin, decimal latmax, decimal lonmin, decimal lonmax)
        {
            ConvertMERGE.Ctl ctl = new ConvertMERGE.Ctl(System.IO.Path.GetFileNameWithoutExtension(p) + ".bin", precipitacao.Data.ToString(), precipitacao);
            ctl.FilePath = System.IO.Path.ChangeExtension(p, ".ctl");
            ctl.SaveFile();
        }

        public static void Salvar(this Precipitacao precipitacao, string p)
        {


            decimal thisLatMax = precipitacao.Prec.Keys.Select(x => x.Item1).Max();
            decimal thisLatMin = precipitacao.Prec.Keys.Select(x => x.Item1).Min();
            decimal thisLonMax = precipitacao.Prec.Keys.Select(x => x.Item2).Max();
            decimal thisLonMin = precipitacao.Prec.Keys.Select(x => x.Item2).Min();

            using (var sw = System.IO.File.CreateText(p))
            {

                foreach (var key in precipitacao.Prec.Keys)
                {
                    sw.WriteLine(
                        string.Join(" ",
                        key.Item2.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo).PadLeft(6),
                        key.Item1.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo).PadLeft(6),
                        precipitacao.Prec[key].ToString("0.0", System.Globalization.NumberFormatInfo.InvariantInfo).PadLeft(5)
                    ));
                }
            }
        }
    }
    public class PrecipitacaoNula : Precipitacao
    {

        public override float this[decimal lat, decimal lon]
        {
            get
            {
                return 0f;
            }
        }

        public override float? this[string codigoPosto]
        {
            get
            {
                return 0f;
            }
        }
    }
    public class PrecipitacaoImg
    {

        float? color2Value(string name)
        {

            switch (name)
            {
                case "ffffffff":
                case "ffe1ffff": return 0f;
                case "ffb4f0fa": return 3f;
                case "ff96d2fa": return 7.5f;
                case "ff2882f0": return 12.5f;
                case "ff1464d2": return 17.5f;
                case "ff67fe85": return 22.5f;
                case "ff18d706": return 27.5f;
                case "ff1eb41e": return 35f;
                case "ffffe878": return 45f;
                case "ffffc03c": return 62.5f;
                case "ffff6000": return 87.5f;
                case "ffe11400": return 125f;
                case "fffb5e6b": return 175f;
                case "ffb818fc": return 200f;
                default:
                    return null;
            }

        }

        public float GetVal(decimal lat, decimal lon, decimal passo)
        {
            if (lat < latMin || lat > latMax || lon < lonMin || lon > lonMax) return 0f;

            var i = (int)(((lon - lonMin) / (lonMax - lonMin)) * (maxIpxl - minIpxl)) + minIpxl;
            var j = (int)(((latMax - lat) / (latMax - latMin)) * (maxJpxl - minJpxl)) + minJpxl;

            var vals = new List<Tuple<float?, double>>();
            for (int it = i - (int)(pxlPorGrauX * passo / 2); it <= i + (int)(pxlPorGrauX * passo / 2); it++)
            {
                for (int jt = j - (int)(pxlPorGrauY * passo / 2); jt <= j + (int)(pxlPorGrauY * passo / 2); jt++)
                {
                    var pxl = img.GetPixel(it, jt);
                    vals.Add(new Tuple<float?, double>(color2Value(pxl.Name), Math.Pow((double)(jt - j), 2) + Math.Pow((double)(it - i), 2) + 0.001));
                }
            }

            vals = vals.Where(x => x.Item1.HasValue).ToList();
            if (vals.Count > 0) return (float)(vals.Sum(x => x.Item1.Value / x.Item2) / vals.Sum(x => 1d / x.Item2));
            else return 0;


        }


        public float GetVal(decimal lat, decimal lon)
        {


            if (lat < latMin || lat > latMax || lon < lonMin || lon > lonMax) return 0f;

            var i = (int)(((lon - lonMin) / (lonMax - lonMin)) * (maxIpxl - minIpxl)) + minIpxl;
            var j = (int)(((latMax - lat) / (latMax - latMin)) * (maxJpxl - minJpxl)) + minJpxl;

            var pxl = img.GetPixel(i, j);

            var val = color2Value(pxl.Name);

            if (!val.HasValue) //unknown
            {
                var vs = new float?[] {
                 color2Value(img.GetPixel(i - 2, j - 2).Name),
                 color2Value(img.GetPixel(i + 2, j + 2).Name),
                 color2Value(img.GetPixel(i + 2, j - 2).Name),
                 color2Value(img.GetPixel(i - 2, j + 2).Name),
                 color2Value(img.GetPixel(i - 1, j - 1).Name),
                 color2Value(img.GetPixel(i + 1, j + 1).Name),
                 color2Value(img.GetPixel(i + 1, j - 1).Name),
                 color2Value(img.GetPixel(i - 1, j + 1).Name),
                };

                val = vs.Where(x => x.HasValue).Average();
            }

            return val ?? 0;

        }

        Bitmap img;
        int minIpxl = 0;
        int maxIpxl = 0;
        int minJpxl = 0;
        int maxJpxl = 0;

        decimal latMin = -35m; decimal lonMin = -75m; // lat,lon
        decimal latMax = 5m; decimal lonMax = -30m; // lat,lon
        int pxlPorGrauX;
        int pxlPorGrauY;

        public PrecipitacaoImg(string file)
        {

            img = new Bitmap(file);


            #region limits

            var jm = img.Height / 2;
            var im = img.Width / 2;
            var oks = 0;


            for (int i = 0; i < img.Width; i++)
            {
                Color pixel = img.GetPixel(i, jm);

                if (pixel.Name == "ff000000") oks++;
                else oks = 0;

                if (oks == 2
                    && img.GetPixel(i, jm + 5).Name == "ff000000"
                    && img.GetPixel(i, jm - 5).Name == "ff000000"
                    && img.GetPixel(i - 1, jm + 5).Name == "ff000000"
                    && img.GetPixel(i - 1, jm - 5).Name == "ff000000")
                {
                    minIpxl = i;
                    oks = 0;
                    break;
                }

            }
            for (int i = img.Width - 1; i >= 0; i--)
            {
                Color pixel = img.GetPixel(i, jm);

                if (pixel.Name == "ff000000") oks++;
                else oks = 0;

                if (oks == 2
                    && img.GetPixel(i, jm + 3).Name == "ff000000"
                    && img.GetPixel(i, jm - 5).Name == "ff000000"
                    && img.GetPixel(i + 1, jm + 5).Name == "ff000000"
                    && img.GetPixel(i + 1, jm - 2).Name == "ff000000")
                {
                    maxIpxl = i;
                    oks = 0;
                    break;
                }

            }
            for (int j = 0; j < img.Height; j++)
            {
                Color pixel = img.GetPixel(im, j);

                if (pixel.Name == "ff000000") oks++;
                else oks = 0;

                if (oks == 2
                    && img.GetPixel(im + 3, j).Name == "ff000000"
                    && img.GetPixel(im - 5, j).Name == "ff000000"
                    && img.GetPixel(im + 5, j - 1).Name == "ff000000"
                    && img.GetPixel(im - 2, j - 1).Name == "ff000000")
                {
                    minJpxl = j;
                    oks = 0;
                    break;
                }

            }
            for (int j = img.Height - 1; j >= 0; j--)
            {
                Color pixel = img.GetPixel(im, j);

                if (pixel.Name == "ff000000") oks++;
                else oks = 0;

                if (oks == 2
                    && img.GetPixel(im + 5, j).Name == "ff000000"
                    && img.GetPixel(im - 5, j).Name == "ff000000"
                    && img.GetPixel(im + 5, j + 1).Name == "ff000000"
                    && img.GetPixel(im - 5, j + 1).Name == "ff000000")
                {
                    maxJpxl = j;
                    oks = 0;
                    break;
                }

            }

            #endregion
            pxlPorGrauX = (int)((maxIpxl - minIpxl) / (lonMax - lonMin));
            pxlPorGrauY = (int)((maxJpxl - minJpxl) / (latMax - latMin));


        }
    }
    public class PrecipitacaoImg0
    {

        float? color2Value(string name)
        {

            switch (name)
            {
                case "ffffffff":
                case "ffe1ffff": return 0f;
                case "ffb4f0fa": return 3f;
                case "ff96d2fa": return 7.5f;
                case "ff2882f0": return 12.5f;
                case "ff1464d2": return 17.5f;
                case "ff67fe85": return 22.5f;
                case "ff18d706": return 27.5f;
                case "ff1eb41e": return 35f;
                case "ffffe878": return 45f;
                case "ffffc03c": return 62.5f;
                case "ffff6000": return 87.5f;
                case "ffe11400": return 125f;
                case "fffb5e6b": return 175f;
                case "ffb818fc": return 200f;
                default:
                    return null;
            }

        }

        public float GetVal(decimal lat, decimal lon, decimal passo)
        {
            if (lat < latMin || lat > latMax || lon < lonMin || lon > lonMax) return 0f;

            var i = (int)(((lon - lonMin) / (lonMax - lonMin)) * (maxIpxl - minIpxl)) + minIpxl;
            var j = (int)(((latMax - lat) / (latMax - latMin)) * (maxJpxl - minJpxl)) + minJpxl;

            var vals = new List<Tuple<float?, double>>();
            for (int it = i - (int)(pxlPorGrauX * passo / 2); it <= i + (int)(pxlPorGrauX * passo / 2); it++)
            {
                for (int jt = j - (int)(pxlPorGrauY * passo / 2); jt <= j + (int)(pxlPorGrauY * passo / 2); jt++)
                {
                    var pxl = img.GetPixel(it, jt);
                    vals.Add(new Tuple<float?, double>(color2Value(pxl.Name), Math.Pow((double)(jt - j), 2) + Math.Pow((double)(it - i), 2) + 0.001));
                }
            }

            vals = vals.Where(x => x.Item1.HasValue).ToList();
            if (vals.Count > 0) return (float)(vals.Sum(x => x.Item1.Value / x.Item2) / vals.Sum(x => 1d / x.Item2));
            else return 0;


        }


        public float GetVal(decimal lat, decimal lon)
        {


            if (lat < latMin || lat > latMax || lon < lonMin || lon > lonMax) return 0f;

            var i = (int)(((lon - lonMin) / (lonMax - lonMin)) * (maxIpxl - minIpxl)) + minIpxl;
            var j = (int)(((latMax - lat) / (latMax - latMin)) * (maxJpxl - minJpxl)) + minJpxl;

            var pxl = img.GetPixel(i, j);

            var val = color2Value(pxl.Name);

            if (!val.HasValue) //unknown
            {
                var vs = new float?[] {
                 color2Value(img.GetPixel(i - 2, j - 2).Name),
                 color2Value(img.GetPixel(i + 2, j + 2).Name),
                 color2Value(img.GetPixel(i + 2, j - 2).Name),
                 color2Value(img.GetPixel(i - 2, j + 2).Name),
                 color2Value(img.GetPixel(i - 1, j - 1).Name),
                 color2Value(img.GetPixel(i + 1, j + 1).Name),
                 color2Value(img.GetPixel(i + 1, j - 1).Name),
                 color2Value(img.GetPixel(i - 1, j + 1).Name),
                };

                val = vs.Where(x => x.HasValue).Average();
            }

            return val ?? 0;

        }

        Bitmap img;
        int minIpxl = 36;
        int maxIpxl = 580;
        int minJpxl = 82;
        int maxJpxl = 718;

        decimal latMin = -35m; decimal lonMin = -75m; // lat,lon
        decimal latMax = 5m; decimal lonMax = -34m; // lat,lon
        int pxlPorGrauX;
        int pxlPorGrauY;

        public PrecipitacaoImg0(string file)
        {

            img = new Bitmap(file);



            pxlPorGrauX = (int)((maxIpxl - minIpxl) / (lonMax - lonMin));
            pxlPorGrauY = (int)((maxJpxl - minJpxl) / (latMax - latMin));


        }
    }
    public class PrecipitacaoImg2
    {
        float? color2Value(string name)
        {
            switch (name)
            {
                case "fffef192": return 550f;
                case "fffde385": return 450f;
                case "fffbd479": return 375f;
                case "fffac66c": return 325f;
                case "ffde9357": return 275f;
                case "ffc16e4e": return 237.5f;
                case "ffa54945": return 212.5f;
                case "ff87253b": return 187.5f;
                case "ff6c0033": return 162.5f;
                case "ff682f67": return 137.5f;
                case "ff7a4779": return 112.5f;
                case "ff8e5f8d": return 95f;
                case "ffa0779f": return 85f;
                case "ffb38fb2": return 75f;
                case "ffc5a7c5": return 65f;
                case "ffd9bed8": return 55f;
                case "ffebd5eb": return 47.5f;
                case "ffa9dbf2": return 42.5f;
                case "ff8bc4de": return 37.5f;
                case "ff6dacc8": return 32.5f;
                case "ff5094b5": return 27.5f;
                case "ff337e9f": return 22.5f;
                case "ff14678c": return 17.5f;
                case "ff14673c": return 13.5f;
                case "ff2e7e54": return 10.5f;
                case "ff48936d": return 7.5f;
                case "ff62aa85": return 4.5f;
                case "ff7dc19e": return 2.5f;
                case "ff97d8b7": return 1.5f;
                case "ffb1edcf": return 0.6f;
                case "ffffffff": return 0f;
                default:
                    return null;
            }
            //switch (name)
            //{
            //    case "fffef192": return 500f;
            //    case "fffde385": return 400f;
            //    case "fffbd479": return 350f;
            //    case "fffac66c": return 300f;
            //    case "ffde9357": return 250f;
            //    case "ffc16e4e": return 225f;
            //    case "ffa54945": return 200f;
            //    case "ff87253b": return 175f;
            //    case "ff6c0033": return 150f;
            //    case "ff682f67": return 125f;
            //    case "ff7a4779": return 100f;
            //    case "ff8e5f8d": return 90f;
            //    case "ffa0779f": return 80f;
            //    case "ffb38fb2": return 70f;
            //    case "ffc5a7c5": return 60f;
            //    case "ffd9bed8": return 50f;
            //    case "ffebd5eb": return 45f;
            //    case "ffa9dbf2": return 40f;
            //    case "ff8bc4de": return 35f;
            //    case "ff6dacc8": return 30f;
            //    case "ff5094b5": return 25f;
            //    case "ff337e9f": return 20f;
            //    case "ff14678c": return 15f;
            //    case "ff14673c": return 12f;
            //    case "ff2e7e54": return 9f;
            //    case "ff48936d": return 6f;
            //    case "ff62aa85": return 3f;
            //    case "ff7dc19e": return 2f;
            //    case "ff97d8b7": return 1f;
            //    case "ffb1edcf": return 0.2f;
            //    case "ffffffff": return 0f;
            //    default:
            //        return null;
            //}

        }
        public float GetVal(decimal lat, decimal lon, decimal passo)
        {
            if (lat < latMin || lat > latMax || lon < lonMin || lon > lonMax) return 0f;

            var i = (int)(((lon - lonMin) / (lonMax - lonMin)) * (maxIpxl - minIpxl)) + minIpxl;
            var j = (int)(((latMax - lat) / (latMax - latMin)) * (maxJpxl - minJpxl)) + minJpxl;

            var vals = new List<Tuple<float?, double>>();
            for (int it = i - (int)(pxlPorGrauX * passo / 2); it <= i + (int)(pxlPorGrauX * passo / 2); it++)
            {
                for (int jt = j - (int)(pxlPorGrauY * passo / 2); jt <= j + (int)(pxlPorGrauY * passo / 2); jt++)
                {
                    var pxl = img.GetPixel(it, jt);
                    vals.Add(new Tuple<float?, double>(color2Value(pxl.Name), Math.Pow((double)(jt - j), 2) + Math.Pow((double)(it - i), 2) + 0.001));
                }
            }

            vals = vals.Where(x => x.Item1.HasValue).ToList();
            if (vals.Count > 0) return (float)(vals.Sum(x => x.Item1.Value / x.Item2) / vals.Sum(x => 1d / x.Item2));
            else return 0;


        }

        public float GetVal(decimal lat, decimal lon)
        {


            if (lat < latMin || lat > latMax || lon < lonMin || lon > lonMax) return 0f;

            var i = (int)(((lon - lonMin) / (lonMax - lonMin)) * (maxIpxl - minIpxl)) + minIpxl;
            var j = (int)(((latMax - lat) / (latMax - latMin)) * (maxJpxl - minJpxl)) + minJpxl;

            var pxl = img.GetPixel(i, j);

            var val = color2Value(pxl.Name);

            if (!val.HasValue) //unknown
            {
                var vs = new float?[] {
                 color2Value(img.GetPixel(i - 2, j - 2).Name),
                 color2Value(img.GetPixel(i + 2, j + 2).Name),
                 color2Value(img.GetPixel(i + 2, j - 2).Name),
                 color2Value(img.GetPixel(i - 2, j + 2).Name),
                 color2Value(img.GetPixel(i - 1, j - 1).Name),
                 color2Value(img.GetPixel(i + 1, j + 1).Name),
                 color2Value(img.GetPixel(i + 1, j - 1).Name),
                 color2Value(img.GetPixel(i - 1, j + 1).Name),
                };

                val = vs.Where(x => x.HasValue).Average();
            }

            return val ?? 0;
        }

        Bitmap img;
        int minIpxl = 139;
        int maxIpxl = 969;
        int minJpxl = 47;
        int maxJpxl = 611;

        decimal latMin = -40m; decimal lonMin = -80m; // lat,lon
        decimal latMax = 10m; decimal lonMax = -0m; // lat,lon

        int pxlPorGrauX = 1;
        int pxlPorGrauY = 1;

        public PrecipitacaoImg2(string file)
        {
            img = new Bitmap(file);

            pxlPorGrauX = (int)((maxIpxl - minIpxl) / (lonMax - lonMin));
            pxlPorGrauY = (int)((maxJpxl - minJpxl) / (latMax - latMin));
        }
    }
    //meteologix
    public class PrecipitacaoImg4
    {
        float? color2Value(string name)
        {
            switch (name)
            {
                case "fff0f0f0": return 0f;
                case "ffb4d7ff": return 0.2f;
                case "ff75baff": return 0.7f;
                case "ff359aff": return 1.5f;
                case "ff0482ff": return 2.5f;
                case "ff0069d2": return 4f;
                case "ff00367f": return 6f;
                case "ff148f1b": return 8.5f;
                case "ff1acf05": return 12.5f;
                case "ff63ed07": return 17.5f;
                case "fffff42b": return 22.5f;
                case "ffe8dc00": return 27.5f;
                case "fff06000": return 32.5f;
                case "ffff7f27": return 37.5f;
                case "ffffa66a": return 42.5f;
                case "fff84e78": return 47.5f;
                case "fff71e54": return 55f;
                case "ffbf0000": return 65f;
                case "ff880000": return 75f;
                case "ff64007f": return 85f;
                case "ffc200fb": return 95f;
                case "ffdd66ff": return 112.5f;
                case "ffeba6ff": return 137.5f;
                case "fff9e6ff": return 175f;
                case "ffd4d4d4": return 250f;
                case "ff969696": return 300f;

                default:
                    return null;
            }

        }
        public float GetVal(decimal lat, decimal lon, decimal passo)
        {
            if (lat < latMin || lat > latMax || lon < lonMin || lon > lonMax) return 0f;

            var i = (int)(((lon - lonMin) / (lonMax - lonMin)) * (maxIpxl - minIpxl)) + minIpxl;
            var j = (int)(((latMax - lat) / (latMax - latMin)) * (maxJpxl - minJpxl)) + minJpxl;

            //var vals = new List<Tuple<float?, double>>();
            var xys = new List<Tuple<int, int>>();

            for (int it = i - (int)(pxlPorGrauX * passo); it <= i + (int)(pxlPorGrauX * passo); it++)
            {
                for (int jt = j - (int)(pxlPorGrauY * passo); jt <= j + (int)(pxlPorGrauY * passo); jt++)
                {
                    xys.Add(new Tuple<int, int>(it, jt));
                }
            }
            var vals =
            xys.Distinct().Select(xy =>
            {
                var pxl = img.GetPixel(xy.Item1, xy.Item2);
                return new Tuple<float?, double>(color2Value(pxl.Name), Math.Pow((double)(xy.Item2 - j), 2) + Math.Pow((double)(xy.Item1 - i), 2) + 0.001);
            }).Where(x => x.Item1.HasValue).ToList();

            //vals = vals.Where(x => x.Item1.HasValue).ToList();
            if (vals.Count > 0) return (float)(vals.Sum(x => x.Item1.Value / x.Item2) / vals.Sum(x => 1d / x.Item2));
            else return 0;




        }

        public float GetVal(decimal lat, decimal lon)
        {


            if (lat < latMin || lat > latMax || lon < lonMin || lon > lonMax) return 0f;

            var i = (int)(((lon - lonMin) / (lonMax - lonMin)) * (maxIpxl - minIpxl)) + minIpxl;
            var j = (int)(((latMax - lat) / (latMax - latMin)) * (maxJpxl - minJpxl)) + minJpxl;

            var pxl = img.GetPixel(i, j);

            var val = color2Value(pxl.Name);

            if (!val.HasValue) //unknown
            {
                var vs = new float?[] {
                 color2Value(img.GetPixel(i - 2, j - 2).Name),
                 color2Value(img.GetPixel(i + 2, j + 2).Name),
                 color2Value(img.GetPixel(i + 2, j - 2).Name),
                 color2Value(img.GetPixel(i - 2, j + 2).Name),
                 color2Value(img.GetPixel(i - 1, j - 1).Name),
                 color2Value(img.GetPixel(i + 1, j + 1).Name),
                 color2Value(img.GetPixel(i + 1, j - 1).Name),
                 color2Value(img.GetPixel(i - 1, j + 1).Name),
                };

                val = vs.Where(x => x.HasValue).Average();
            }

            return val ?? 0;
        }

        Bitmap img;
        int minIpxl = 53;
        int maxIpxl = 673;
        int minJpxl = 15;
        int maxJpxl = 589;

        decimal latMin = -33.76m; decimal lonMin = -80.96m; // lat,lon
        decimal latMax = 5.65m; decimal lonMax = -34.85m; // lat,lon

        int pxlPorGrauX = 1;
        int pxlPorGrauY = 1;

        public PrecipitacaoImg4(string file)
        {
            img = new Bitmap(file);

            pxlPorGrauX = (int)((maxIpxl - minIpxl) / (lonMax - lonMin));
            pxlPorGrauY = (int)((maxJpxl - minJpxl) / (latMax - latMin));
        }
    }
    public class Precipitacao
    {

        public static Precipitacao Nula
        {
            get
            {

                return new PrecipitacaoNula();

            }
        }
        //public static decimal latmin = -35, latmax = 5, lonmin = -75, lonmax = -35;

        public static decimal latmin = -50.2m, latmax = 12.2m, lonmin = -83m, lonmax = -25.8m;
        public static decimal etaResolution = 0.4m;

        //GEFS
        public static decimal latminGEFS = -60.0m, latmaxGEFS = 20.0m, lonminGEFS = -99m, lonmaxGEFS = -20.0m;
        public static decimal GEFSResolution = 1;

        //ECMWF
        public static decimal latminECMWF = -60.0m, latmaxECMWF = 15.0m, lonminECMWF = -90.0m, lonmaxECMWF = -30.0m;
        public static decimal ECMWFResolution = 0.2m;

        //GFSNOAA 0.25
        public static decimal latminGFS25 = -35.0m, latmaxGFS25 = 5.0m, lonminGFS25 = -75.0m, lonmaxGFS25 = -34.0m;
        public static decimal GFS25Resolution = 0.25m;

        static object lockLoad = new object();
        public Precipitacao()
        {
            //put lock
            if (_pluviometr == null)
                lock (lockLoad)
                {
                    if (_pluviometr == null)
                        ReadPostos();
                }
        }

        public string Descricao { get; set; }



        public DateTime Data { get; set; }

        public Dictionary<Tuple<decimal, decimal>, float> Prec { get; set; }

        public virtual float this[decimal lat, decimal lon]
        {
            get
            {
                return this.Prec[new Tuple<decimal, decimal>(lat, lon)];
            }
        }

        public virtual float this[Tuple<decimal, decimal> coord]
        {
            get
            {
                if (this.Prec.ContainsKey(coord))
                {
                    return this.Prec[coord];
                }
                else
                {
                    var lt1 = coord.Item1;
                    var ln1 = coord.Item2;

                    var keyL = this.Prec.Keys
                        .Where(x => x.Item1 >= lt1 - 2 * etaResolution && x.Item1 <= lt1 + etaResolution)
                        .Where(x => x.Item2 >= ln1 - 2 * etaResolution && x.Item2 <= ln1 + etaResolution)
                        .LastOrDefault();
                    //if (keyL.Count() > 0) 
                    if (keyL != null) return this.Prec[keyL];
                    throw new KeyNotFoundException(coord.ToString());
                }

                //return this.Prec[coord];
            }
            set
            {

                this.Prec[coord] = value;
            }
        }

        public virtual float? this[string codigoPosto]
        {
            get
            {

                if (!_pluviometr.ContainsKey(codigoPosto))
                {
                    throw new Exception("Posto pluviométrico não identificado no arquivo de configurações: " + codigoPosto);
                }

                var key = _pluviometr[codigoPosto];

                if (this.Prec.ContainsKey(key))
                {
                    return this.Prec[key];
                }
                else
                {
                    var lt1 = key.Item1;
                    var ln1 = key.Item2;

                    var keyL = this.Prec.Keys
                        .Where(x => x.Item1 >= lt1 - 2 * etaResolution && x.Item1 <= lt1 + etaResolution)
                        .Where(x => x.Item2 >= ln1 - 2 * etaResolution && x.Item2 <= ln1 + etaResolution)
                        .LastOrDefault();
                    //if (keyL.Count() > 0) 
                    if (keyL != null) return this.Prec[keyL];
                    else return null;
                }

                throw new Exception("Não foi possivel encontrar a precipitação para o posto plu " + codigoPosto);
            }
        }

        static Dictionary<string, Tuple<decimal, decimal>> _pluviometr = null;

        static void ReadPostos()
        {

            var f = Config.ConfigPostosPlu;

            _pluviometr = new Dictionary<string, Tuple<decimal, decimal>>();

            using (var sr = System.IO.File.OpenText(f))
                while (!sr.EndOfStream)
                {

                    var l = sr.ReadLine();

                    if (l.StartsWith("#"))
                    {
                        continue;
                    }
                    else if (string.IsNullOrWhiteSpace(l)) break;

                    var lArr = l.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    _pluviometr[lArr[0]] = new Tuple<decimal, decimal>(
                                decimal.Parse(lArr[1], System.Globalization.NumberFormatInfo.InvariantInfo),
                                decimal.Parse(lArr[2], System.Globalization.NumberFormatInfo.InvariantInfo)
                                );
                }
        }

        public Precipitacao Duplicar()
        {

            Precipitacao nova = new Precipitacao() { Data = this.Data, Descricao = this.Descricao, Prec = new Dictionary<Tuple<decimal, decimal>, float>() };

            foreach (var k in this.Prec.Keys)
            {
                nova.Prec[k] = this.Prec[k];
            }

            return nova;
        }

        public Dictionary<Tuple<decimal, decimal>, float> CreateBlankPrecDictionary()
        {
            Dictionary<Tuple<decimal, decimal>, float> returnPrec;
            returnPrec = this.Prec.Keys.ToDictionary(x => x, x => 0f);
            return returnPrec;
        }


        internal Precipitacao ChangeDefinition(decimal resolusaoNova)
        {
            var lats = Prec.Keys.Select(x => x.Item1).Distinct().OrderBy(x => x).ToArray();
            var rOriginal = lats[1] - lats[0];

            Precipitacao nova;
            if (resolusaoNova > rOriginal)
            {
                nova = new Precipitacao() { Data = this.Data, Descricao = this.Descricao, Prec = new Dictionary<Tuple<decimal, decimal>, float>() };

                var latmin = lats.First();
                var latmax = lats.Last();

                for (decimal lat = latmin + resolusaoNova / 2; lat < latmax; lat += resolusaoNova)
                {
                    var latMult = Math.Floor(lat / rOriginal);
                    var latBase = latmin + latMult * latMult;




                }



            }
            else if (resolusaoNova < rOriginal)
            {
                nova = new Precipitacao() { Data = this.Data, Descricao = this.Descricao, Prec = new Dictionary<Tuple<decimal, decimal>, float>() };
            }
            else
            {
                nova = Duplicar();
            }

            return nova;

        }
    }
    public class PrecipitacaoConjunto
    {
        public List<RegiaoVies> Regioes { get; set; }
        public List<RegiaoConj> RegioesConjunto { get; set; }
        public List<Agrupamento> Agrupamentos { get; set; }

        public PrecipitacaoConjunto(string arquivoParametros)
        {

            Agrupamentos = new List<Agrupamento>();
            Regioes = new List<RegiaoVies>();
            RegioesConjunto = new List<RegiaoConj>();

            var fileLines = System.IO.File.ReadAllLines(arquivoParametros);

            var readGrades = false;
            var readParame = false;
            var readLimite = false;
            var readCorrel = false;
            var readConjun = false;

            foreach (var line in fileLines)
            {


                if (line.StartsWith("&") || string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.StartsWith("#"))
                {
                    readGrades = readParame = readLimite = readCorrel = readConjun = false;
                }

                var lineArr = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (line.StartsWith("#GRADES"))
                {
                    readGrades = true;
                }
                else if (line.StartsWith("#PARAMETROS"))
                {
                    readParame = true;
                }
                else if (line.StartsWith("#LIMITES"))
                {
                    readLimite = true;
                }
                else if (line.StartsWith("#CONJUNTO"))
                {
                    readConjun = true;
                }
                else if (line.StartsWith("#CORRELACAO"))
                {
                    readCorrel = true;
                }
                else if (readGrades)
                {
                    var reg = new RegiaoVies();

                    reg.Nome = lineArr[0];
                    reg.Modelo = lineArr[1];

                    reg.Coordenadas.AddRange(
                    lineArr.Skip(2).ToList().Select(coord =>
                    {
                        var lat = decimal.Parse(coord.Split(';')[0]);
                        var lon = decimal.Parse(coord.Split(';')[1]);
                        return new Tuple<decimal, decimal>(lat, lon);
                    }));
                    Regioes.Add(reg);
                }
                else if (readParame)
                {
                    if (Regioes.Any(x => x.Nome == lineArr[0] && x.Modelo == lineArr[1]))
                    {
                        var reg = Regioes.First(x => x.Nome == lineArr[0] && x.Modelo == lineArr[1]);

                        for (int i = 1; i <= 12; i++)
                        {
                            reg.A[i] = float.Parse(lineArr.ToArray()[i + 1]);
                            reg.B[i] = float.Parse(lineArr.ToArray()[i + 13]);
                            reg.LimVies[i] = float.Parse(lineArr.ToArray()[i + 25]);
                        }
                    }
                    else { }
                }
                else if (readLimite)
                {

                    if (Regioes.Any(x => x.Nome == lineArr[0] && x.Modelo == lineArr[1]))
                    {
                        var reg = Regioes.First(x => x.Nome == lineArr[0] && x.Modelo == lineArr[1]);
                        reg.LimDiario = float.Parse(lineArr[2]);
                        for (int i = 1; i <= 12; i++)
                        {
                            reg.Lim10Dias[i] = float.Parse(lineArr.ToArray()[i + 2]);
                        }
                    }
                    else { }
                }
                else if (readCorrel)
                {
                    var agrup = new Agrupamento();
                    agrup.Nome = lineArr[0];
                    agrup.Modelo = lineArr[1];

                    for (int i = 1; i <= 10; i++)
                    {
                        agrup.Correlacao[i] = float.Parse(lineArr.ToArray()[i + 1]);
                    }

                    Agrupamentos.Add(agrup);
                }
                else if (readConjun)
                {
                    var reg = new RegiaoConj();

                    var agrup = Agrupamentos.First(x => x.Nome == lineArr[0] && x.Modelo == lineArr[2]);

                    reg.Agrupamento = agrup;
                    reg.Nome = lineArr[1];
                    reg.Modelo = lineArr[2];

                    reg.Coordenadas.AddRange(
                    lineArr.Skip(3).ToList().Select(coord =>
                    {
                        var lat = decimal.Parse(coord.Split(';')[0]);
                        var lon = decimal.Parse(coord.Split(';')[1]);
                        return new Tuple<decimal, decimal>(lat, lon);
                    }));


                    RegioesConjunto.Add(reg);
                }
            }
        }

        public Dictionary<DateTime, Precipitacao> Remover(string modelo, Dictionary<DateTime, Precipitacao> previsoes, bool remVies, bool remLim)
        {

            //Limpar regioes não atendidas
            var coord = Regioes.Where(x => x.Modelo == modelo).SelectMany(x => x.Coordenadas).Distinct();
            var precipConjunto = previsoes.ToDictionary(x => x.Key, x => x.Value.Duplicar()); // new Precipitacao() { Prec = new Dictionary<Tuple<decimal, decimal>, float>() });


            Console.WriteLine(modelo + "    Remocao de vies");
            foreach (var reg in Regioes.Where(x => x.Modelo == modelo))
            {

                reg.RemoverVies(previsoes, precipConjunto, remVies, remLim);

                Console.WriteLine();

            }

            return precipConjunto;
        }

        public Dictionary<DateTime, Precipitacao> Conjunto(Dictionary<DateTime, Precipitacao> chuvasParaTratamentoETA,
            Dictionary<DateTime, Precipitacao> chuvasParaTratamentoGEFS,
            ChuvaVazaoTools.WaitForm.TipoConjunto tipo, bool trucarMapa = true
            )
        {
            var precipConjunto = new Dictionary<DateTime, Precipitacao>();
            var dataIni = chuvasParaTratamentoETA != null ? chuvasParaTratamentoETA.Keys.Min() : chuvasParaTratamentoGEFS.Keys.Min();

            foreach (var regGrp in RegioesConjunto.Where(x => x.Modelo == "ETA40").ToList())
            {
                RegioesConjunto.Add(regGrp.Copy("CONJ"));
            }

            var iMax = Math.Max(chuvasParaTratamentoETA == null ? 0 : chuvasParaTratamentoETA.Count, chuvasParaTratamentoGEFS == null ? 0 : chuvasParaTratamentoGEFS.Count);

            RegioesConjunto.ForEach(x => x.precMedia = new float[iMax]);


            for (int i = 0; i < iMax; i++)
            {
                var precRef = (chuvasParaTratamentoETA != null && chuvasParaTratamentoETA.ContainsKey(dataIni.AddDays(i))) ?
                        chuvasParaTratamentoETA[dataIni.AddDays(i)] :
                        chuvasParaTratamentoGEFS[dataIni.AddDays(i)];


                precipConjunto[dataIni.AddDays(i)] = new Precipitacao()
                {
                    Data = dataIni.AddDays(i),
                    Descricao = "CONJ_" + dataIni.AddDays(i).ToString("yyyyMMdd"),
                    Prec = trucarMapa ? precRef.CreateBlankPrecDictionary() : precRef.Duplicar().Prec
                };

                foreach (var regGrp in RegioesConjunto.GroupBy(x => x.Nome))
                {
                    var regeta = regGrp.First(x => x.Modelo == "ETA40");
                    var reggefs = regGrp.First(x => x.Modelo == "GEFS");
                    var regconj = regGrp.First(x => x.Modelo == "CONJ");

                    var pmEta = 0f;
                    var pmGefs = 0f;

                    float pConj = 0;
                    if (tipo == WaitForm.TipoConjunto.Conjunto && chuvasParaTratamentoETA.ContainsKey(dataIni.AddDays(i)) && chuvasParaTratamentoGEFS.ContainsKey(dataIni.AddDays(i)))
                    {
                        pmEta = regeta.
                        Coordenadas.Select(x =>
                               chuvasParaTratamentoETA[dataIni.AddDays(i)][x]
                        ).Sum() / regeta.Coordenadas.Count;

                        pmGefs = reggefs.
                        Coordenadas.Select(x =>
                               chuvasParaTratamentoGEFS[dataIni.AddDays(i)][x]
                        ).Sum() / reggefs.Coordenadas.Count;

                        if (i < 10)
                        {
                            pConj = regeta.Agrupamento.Correlacao[i + 1] * pmEta + reggefs.Agrupamento.Correlacao[i + 1] * pmGefs;
                        }
                        else
                        {
                            pConj = regeta.Agrupamento.Correlacao[10] * pmEta + reggefs.Agrupamento.Correlacao[10] * pmGefs;
                        }
                    }
                    else if (chuvasParaTratamentoETA.ContainsKey(dataIni.AddDays(i)) && (tipo == WaitForm.TipoConjunto.Eta40 || tipo == WaitForm.TipoConjunto.Conjunto))
                    {
                        pmEta = regeta.
                            Coordenadas.Select(x =>
                            chuvasParaTratamentoETA[dataIni.AddDays(i)][x]
                            ).Sum() / regeta.Coordenadas.Count;
                        pConj = pmEta;
                    }
                    else if (chuvasParaTratamentoGEFS.ContainsKey(dataIni.AddDays(i)) && (tipo == WaitForm.TipoConjunto.Gefs || tipo == WaitForm.TipoConjunto.Conjunto))
                    {
                        pmGefs = reggefs.
                       Coordenadas.Select(x =>
                              chuvasParaTratamentoGEFS[dataIni.AddDays(i)][x]
                       ).Sum() / reggefs.Coordenadas.Count;
                        pConj = pmGefs;
                    }




                    reggefs.precMedia[i] = pmGefs < 0 ? 0 : pmGefs;
                    regeta.precMedia[i] = pmEta < 0 ? 0 : pmEta;
                    regconj.precMedia[i] = pConj < 0 ? 0 : pConj;

                    regeta.Coordenadas.ForEach(x => precipConjunto[dataIni.AddDays(i)][x] = pConj);
                }
            }

            return precipConjunto;
        }

        public Dictionary<DateTime, Precipitacao> ConjuntoLivre(Dictionary<DateTime, Precipitacao> chuvasParaTratamentoETA,
            Dictionary<DateTime, Precipitacao> chuvasParaTratamentoGEFS)
        {
            var precipConjunto = new Dictionary<DateTime, Precipitacao>();
            var dataIni = chuvasParaTratamentoETA != null ? chuvasParaTratamentoETA.Keys.Min() : chuvasParaTratamentoGEFS.Keys.Min();

            foreach (var regGrp in RegioesConjunto.Where(x => x.Modelo == "ETA40").ToList())
            {
                RegioesConjunto.Add(regGrp.Copy("CONJ"));
            }
            var iMax = Math.Max(chuvasParaTratamentoETA == null ? 0 : chuvasParaTratamentoETA.Count, chuvasParaTratamentoGEFS == null ? 0 : chuvasParaTratamentoGEFS.Count);

            RegioesConjunto.ForEach(x => x.precMedia = new float[iMax]);

            for (int i = 0; true; i++)
            {
                bool etaOK = chuvasParaTratamentoETA != null && chuvasParaTratamentoETA.ContainsKey(dataIni.AddDays(i));
                bool gefsOK = chuvasParaTratamentoGEFS != null && chuvasParaTratamentoGEFS.ContainsKey(dataIni.AddDays(i));

                if (!etaOK && !gefsOK) break;


                precipConjunto[dataIni.AddDays(i)] = new Precipitacao()
                {
                    Data = dataIni.AddDays(i),
                    Descricao = "CONJ_" + dataIni.AddDays(i).ToString("yyyyMMdd"),
                    Prec = new Dictionary<Tuple<decimal, decimal>, float>()
                };

                foreach (var regGrp in RegioesConjunto.GroupBy(x => x.Nome))
                {
                    var regeta = regGrp.First(x => x.Modelo == "ETA40");
                    var reggefs = regGrp.First(x => x.Modelo == "GEFS");
                    var regconj = regGrp.First(x => x.Modelo == "CONJ");

                    var pmEta = 0f;
                    var pmGefs = 0f;

                    var corrEta = 1f;
                    var corrGefs = 1f;

                    float pConj = 0;


                    if (etaOK)
                    {
                        pmEta = regeta.
                        Coordenadas.Select(x =>
                               chuvasParaTratamentoETA[dataIni.AddDays(i)][x]
                        ).Sum() / regeta.Coordenadas.Count;


                        corrEta = gefsOK ? regeta.Agrupamento.Correlacao[i + 1] : 1f;
                    }

                    if (gefsOK)
                    {
                        pmGefs = reggefs.
                        Coordenadas.Select(x =>
                               chuvasParaTratamentoGEFS[dataIni.AddDays(i)][x]
                        ).Sum() / reggefs.Coordenadas.Count;
                        corrGefs = etaOK ? reggefs.Agrupamento.Correlacao[i + 1] : 1f;
                    }


                    pConj = corrEta * pmEta + corrGefs * pmGefs;


                    reggefs.precMedia[i] = pmGefs;
                    regeta.precMedia[i] = pmEta;

                    regconj.precMedia[i] = pConj;

                    regeta.Coordenadas.ForEach(x => precipConjunto[dataIni.AddDays(i)][x] = pConj);
                }
            }
            return precipConjunto;
        }

        internal Dictionary<DateTime, Precipitacao> MediaBacias(Dictionary<DateTime, Precipitacao> chuvas)
        {
            var precipConjunto = new Dictionary<DateTime, Precipitacao>();
            if (chuvas.Count == 0) return null;

            var dataIni = chuvas.Keys.Min();

            foreach (var regGrp in RegioesConjunto.Where(x => x.Modelo == "ETA40").ToList())
            {
                RegioesConjunto.Add(regGrp.Copy("CONJ"));
            }
            var iMax = chuvas.Count;

            RegioesConjunto.ForEach(x => x.precMedia = new float[iMax]);

            for (int i = 0; i < iMax; i++)
            {

                precipConjunto[dataIni.AddDays(i)] = new Precipitacao()
                {
                    Data = dataIni.AddDays(i),
                    Descricao = "CONJ_" + dataIni.AddDays(i).ToString("yyyyMMdd"),
                    Prec = new Dictionary<Tuple<decimal, decimal>, float>()
                };

                foreach (var regGrp in RegioesConjunto.GroupBy(x => x.Agrupamento))
                {
                    var regeta = regGrp.Where(x => x.Modelo == "ETA40").SelectMany(x => x.Coordenadas);
                    var regconj = regGrp.Where(x => x.Modelo == "CONJ");

                    var pmEta = 0f;

                    float pConj = 0;

                    pmEta = regeta.Select(x =>
                           chuvas[dataIni.AddDays(i)][x]
                    ).Sum() / regeta.Count();

                    pConj = pmEta;

                    regconj.ToList().ForEach(x => x.precMedia[i] = pConj);

                    regeta.ToList().ForEach(x => precipConjunto[dataIni.AddDays(i)][x] = pConj);
                }
            }

            return precipConjunto;
        }

        internal Precipitacao MLT(int mes)
        {
            var mltVals = new Dictionary<string, int[]>();

            mltVals.Add("Paraguai", new int[] { 235, 184, 168, 103, 67, 29, 26, 34, 62, 107, 164, 211 });
            mltVals.Add("GRANDE", new int[] { 266, 201, 172, 81, 57, 29, 24, 28, 64, 138, 178, 264 });
            mltVals.Add("IGUAÇU", new int[] { 160, 165, 136, 121, 139, 125, 115, 117, 150, 144, 144, 165 });
            mltVals.Add("URUGUAI", new int[] { 144, 150, 132, 136, 134, 143, 141, 143, 163, 160, 139, 145 });
            mltVals.Add("PARANAÍBA", new int[] { 283, 207, 193, 102, 44, 15, 14, 18, 52, 150, 213, 280 });
            mltVals.Add("PARANAPANEMA", new int[] { 177, 160, 135, 105, 116, 95, 66, 69, 109, 156, 144, 187 });
            mltVals.Add("Jacui", new int[] { 116, 126, 120, 105, 109, 136, 141, 143, 151, 131, 116, 116 });
            mltVals.Add("TIETÊ", new int[] { 225, 191, 151, 76, 72, 47, 38, 38, 72, 131, 143, 216 });
            mltVals.Add("Paraiba do SUl", new int[] { 241, 179, 159, 98, 64, 39, 38, 37, 71, 117, 177, 240 });
            mltVals.Add("Doce", new int[] { 221, 130, 140, 71, 37, 20, 21, 24, 42, 117, 197, 231 });
            mltVals.Add("SÃO FRANCISCO", new int[] { 175, 134, 139, 86, 33, 21, 19, 14, 24, 82, 140, 184 });
            mltVals.Add("TOCANTINS", new int[] { 270, 237, 233, 132, 38, 9, 6, 15, 53, 150, 209, 270 });
            mltVals.Add("Parnaiba", new int[] { 170, 184, 213, 162, 50, 14, 10, 6, 21, 62, 97, 141 });
            mltVals.Add("PARANÁ", new int[] { 214, 179, 158, 108, 91, 66, 55, 60, 93, 139, 166, 211 });
            mltVals.Add("ITAIPU", new int[] { 166, 164, 136, 116, 131, 115, 98, 101, 136, 148, 144, 172 });

            var prec =
            new Precipitacao()
            {
                Descricao = $"MLT_{mes:00}",
                Prec = new Dictionary<Tuple<decimal, decimal>, float>()
            };

            foreach (var regeta in RegioesConjunto.Where(x => x.Modelo == "ETA40"))
            {
                if (mltVals.ContainsKey(regeta.Agrupamento.Nome))
                {
                    var precipval = mltVals[regeta.Agrupamento.Nome][mes - 1];
                    regeta.Coordenadas.ForEach(x => prec[x] = precipval);
                }
            }

            return prec;
        }


        public class RegiaoVies
        {
            public string Nome { get; set; }
            public string Modelo { get; set; }
            public Dictionary<int, float> A { get; set; }
            public Dictionary<int, float> B { get; set; }
            public Dictionary<int, float> LimVies { get; set; }
            public float LimDiario { get; set; }
            public Dictionary<int, float> Lim10Dias { get; set; }


            public float[] precMedia = new float[10];
            public float[] ppr = new float[10];
            public float pTot;
            public float limVies;

            public float a;
            public float b;

            public float lim10 = 1000;
            public float limDia = 1000;


            public float pTotpr;


            public List<Tuple<decimal, decimal>> Coordenadas { get; set; }

            public RegiaoVies()
            {
                A = new Dictionary<int, float>();
                B = new Dictionary<int, float>();
                LimVies = new Dictionary<int, float>();
                Coordenadas = new List<Tuple<decimal, decimal>>();
                Lim10Dias = new Dictionary<int, float>();
            }

            public void RemoverVies(Dictionary<DateTime, Precipitacao> previsoes, Dictionary<DateTime, Precipitacao> previsoesOUT, bool remVies, bool remLim)
            {

                Console.Write(Nome + "\t");

                var dataIni = previsoes.Keys.Min();

                var df = dataIni.AddDays(9);
                var fms = 0;// (df.Month != dataIni.Month ? df.Day : 0) / 10.00f;
                var fma = 1.00f - fms;

                //to do: deixar pronto tambem o modelo com parametros ponderados
                a = A[dataIni.Month] * fma + A[df.Month] * fms;
                b = B[dataIni.Month] * fma + B[df.Month] * fms;
                limVies = LimVies[dataIni.Month] * fma + LimVies[df.Month] * fms;

                limDia = LimDiario;
                lim10 = Lim10Dias[dataIni.Month] * fma + Lim10Dias[df.Month] * fms;

                pTot = 0;

                precMedia = new float[previsoes.Count];

                for (int i = 0; i < 10; i++)
                {
                    precMedia[i] =
                        Coordenadas.Select(x =>
                            previsoes[dataIni.AddDays(i)][x]
                        ).Sum() / Coordenadas.Count;


                    pTot += precMedia[i];
                }

                for (int i = 10; i < previsoes.Count; i++)
                {
                    precMedia[i] =
                       Coordenadas.Select(x =>
                           previsoes[dataIni.AddDays(i)][x]
                       ).Sum() / Coordenadas.Count;
                }



                if (remVies)
                {
                    pTotpr = pTot >= limVies ? limVies :
                    (float)Math.Pow(pTot, 2) * a + pTot * b;
                }
                else
                    pTotpr = pTot;

                if (remLim) pTotpr = pTotpr >= lim10 ? lim10 : pTotpr;

                var alpha = pTot > 0 ? pTotpr / pTot : 0;


                ppr = new float[previsoes.Count];


                for (int i = 0; i < previsoes.Count; i++)
                {
                    ppr[i] = alpha * precMedia[i];

                    if (remLim) ppr[i] = ppr[i] >= limDia ? limDia : ppr[i];

                    Console.Write(ppr[i].ToString("N2") + "\t");

                    var beta = precMedia[i] > 0 ? ppr[i] / precMedia[i] : 0;

                    Coordenadas.ForEach(c =>
                    {
                        previsoesOUT[dataIni.AddDays(i)][c] = previsoes[dataIni.AddDays(i)][c] * beta;

                    });
                }
            }

        }

        public class RegiaoConj
        {

            public List<Tuple<decimal, decimal>> Coordenadas { get; set; }

            public string Nome { get; set; }
            public string Modelo { get; set; }
            public Agrupamento Agrupamento { get; set; }


            public float[] precMedia = new float[10];


            public RegiaoConj()
            {
                Coordenadas = new List<Tuple<decimal, decimal>>();
            }

            internal RegiaoConj Copy(string modelo = "")
            {
                return new RegiaoConj
                {
                    Coordenadas = this.Coordenadas.ToList(),
                    Nome = this.Nome,
                    Modelo = modelo,
                    Agrupamento = this.Agrupamento,
                };
            }
        }

        public class Agrupamento
        {
            public string Nome { get; set; }
            public string Modelo { get; set; }
            public Dictionary<int, float> Correlacao { get; set; }

            public Agrupamento()
            {
                Correlacao = new Dictionary<int, float>();
            }
        }
    }
}

namespace ConvertMERGE
{

    public class Data
    {

        Dictionary<Ctl.Var, float[,]> data = new Dictionary<Ctl.Var, float[,]>();
        Ctl ctl;

        public float this[Ctl.Var var, decimal lat, decimal lon]
        {
            get
            {

                int x = (int)Math.Floor((lon - ctl.Xdef.Start) / ctl.Xdef.Increment);
                int y = (int)Math.Floor((lat - ctl.Ydef.Start) / ctl.Ydef.Increment);

                return data[var][x, y];
            }
        }

        public Data(Ctl ctl)
        {
            this.ctl = ctl;
            var binFile = System.IO.Path.ChangeExtension(ctl.FilePath, "bin");
            var xDef = ctl.Xdef.Lenght;
            var yDef = ctl.Ydef.Lenght;

            var bin = System.IO.File.ReadAllBytes(binFile);

            int offset = 0;
            foreach (var _variable in ctl.Vars)
            {
                data[_variable] = new float[xDef, yDef];

                for (int y = 0; y < yDef; y++)
                {
                    for (int x = 0; x < xDef; x++)
                    {
                        var value = BitConverter.ToSingle(bin, (y * xDef + x) * 4 + offset);

                        data[_variable][x, y] = value;

                    }
                }
                offset += xDef * yDef;
            }
        }

        public Data(Ctl ctl, ChuvaVazaoTools.Precipitacao precipitacao)
        {
            this.ctl = ctl;


            foreach (var _variable in ctl.Vars)
            {
                data[_variable] = new float[ctl.Xdef.Lenght, ctl.Ydef.Lenght];

                for (int x = 0; x < ctl.Xdef.Lenght; x++)
                {
                    var lon = x * ctl.Xdef.Increment + ctl.Xdef.Start;
                    for (int y = 0; y < ctl.Ydef.Lenght; y++)
                    {
                        var lat = y * ctl.Ydef.Increment + ctl.Ydef.Start;
                        var key = new Tuple<decimal, decimal>(lat, lon);
                        if (precipitacao.Prec.ContainsKey(key))
                            data[_variable][x, y] = precipitacao[key];
                        else
                            data[_variable][x, y] = 0;
                    }
                }
            }
        }

        internal void SaveFile()
        {
            var xDef = ctl.Xdef.Lenght;
            var yDef = ctl.Ydef.Lenght;
            var binFile = System.IO.Path.ChangeExtension(ctl.FilePath, "bin");
            var bin = new byte[ctl.Vars.Count() * ctl.Xdef.Lenght * ctl.Ydef.Lenght * 4];
            int offset = 0;
            foreach (var _variable in ctl.Vars)
            {
                for (int y = 0; y < ctl.Ydef.Lenght; y++)
                {
                    for (int x = 0; x < ctl.Xdef.Lenght; x++)
                    {
                        var value = data[_variable][x, y];// BitConverter.ToSingle(bin, (y * xDef + x) * 4 + offset);
                        BitConverter.GetBytes(value).CopyTo(bin, (y * ctl.Xdef.Lenght + x) * 4 + offset);
                    }
                }
                offset += xDef * yDef;
            }

            System.IO.File.WriteAllBytes(binFile, bin);
        }


    }
    public class Ctl
    {

        public string Dset { get; set; }
        public string Title { get; set; }
        public string Zdef { get; set; }
        public Axis Xdef { get; set; }
        public Axis Ydef { get; set; }
        public Var[] Vars { get; set; }
        public string FilePath { get; set; }

        Data _bin = null;
        public Data Bin
        {
            get
            {
                if (_bin == null)
                    LoadBin();

                return _bin;
            }
        }



        public Ctl(string dset, string title, ChuvaVazaoTools.Precipitacao precipitacao)
        {
            this.Dset = dset;
            this.Title = title;

            var lats = precipitacao.Prec.Keys.Select(x => x.Item1).Distinct().OrderBy(x => x);
            var lons = precipitacao.Prec.Keys.Select(x => x.Item2).Distinct().OrderBy(x => x);

            Ydef = new Axis()
            {
                Lenght = lats.Count(),
                Start = lats.First(),
                Increment = (lats.Skip(1).First() - lats.First())
            };

            Xdef = new Axis()
            {
                Lenght = lons.Count(),
                Start = lons.First(),
                Increment = (lons.Skip(1).First() - lons.First())
            };

            Zdef = precipitacao.Data.ToString("ddMMMyyyy", System.Globalization.DateTimeFormatInfo.InvariantInfo);

            Vars = new Var[]
            {
                 new Var(){ Name = "prec"}
            };

            _bin = new Data(this, precipitacao);

        }
        public Ctl(string filePath)
        {


            FilePath = filePath;
            var ctlContent = System.IO.File.ReadAllLines(filePath);

            for (int i = 0; i < ctlContent.Length; i++)
            {
                var l = ctlContent[i];
                var dl = l.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                Func<string, bool> checkKey = k => dl[0].Equals(k, StringComparison.OrdinalIgnoreCase);

                if (checkKey("dset"))
                {
                    Dset = l.Substring(5);
                }
                else if (checkKey("title"))
                {
                    Title = l.Substring(6);
                }
                else if (checkKey("xdef"))
                {
                    Xdef = new Axis()
                    {
                        Lenght = int.Parse(dl[1]),
                        Start = decimal.Parse(dl[3], System.Globalization.CultureInfo.InvariantCulture),
                        Increment = decimal.Parse(dl[4], System.Globalization.CultureInfo.InvariantCulture)
                    };
                }
                else if (checkKey("ydef"))
                {
                    Ydef = new Axis()
                    {
                        Lenght = int.Parse(dl[1]),
                        Start = decimal.Parse(dl[3], System.Globalization.CultureInfo.InvariantCulture),
                        Increment = decimal.Parse(dl[4], System.Globalization.CultureInfo.InvariantCulture)
                    };
                }
                else if (checkKey("vars"))
                {
                    i++;

                    var varNumber = int.Parse(dl[1]);

                    Vars = new Var[varNumber];
                    for (int v = 0; v < varNumber; v++)
                    {
                        var varLine = ctlContent[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        Vars[v] = new Var()
                        {
                            Name = varLine[0]
                        };
                        i++;
                    }
                }
            }
        }

        public void LoadBin()
        {
            _bin = new Data(this);
        }

        internal void SaveFile()
        {

            var ctlContent =
                @"DSET ^" + Dset + @"
UNDEF -9999.
TITLE PRECIP C_V_TOOLS " + Title + @"
XDEF " + Xdef.Lenght.ToString().PadLeft(3) + @"  LINEAR  " + Xdef.Start.ToString("00.00", System.Globalization.NumberFormatInfo.InvariantInfo).PadLeft(6) + Xdef.Increment.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo).PadLeft(7) + @"
YDEF " + Ydef.Lenght.ToString().PadLeft(3) + @"  LINEAR  " + Ydef.Start.ToString("00.00", System.Globalization.NumberFormatInfo.InvariantInfo).PadLeft(6) + Ydef.Increment.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo).PadLeft(7) + @"
ZDEF   1 LEVELS 1000
TDEF   1 LINEAR 12Z" + Zdef + @" 24hr
VARS  1
PREC    0  99     Total  24h Precip.        (m)
ENDVARS
";
            System.IO.File.WriteAllText(System.IO.Path.ChangeExtension(FilePath, ".ctl"), ctlContent);


            this.Bin.SaveFile();

        }

        public struct Var
        {
            public string Name { get; set; }
            //public string Description { get; set; }
            public override bool Equals(object obj)
            {
                return obj == null ? false : this.GetHashCode() == obj.GetHashCode();
            }

            public override int GetHashCode()
            {
                return Name.Trim().ToUpperInvariant().GetHashCode();
            }
        }

        public struct Axis
        {
            public int Lenght { get; set; }
            public decimal Start { get; set; }
            public decimal Increment { get; set; }
        }
    }
}

