using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report
{
    public static class PdfExtensions
    {

        //criando a variavel para paragrafo
        static Paragraph titulo = new Paragraph("", new iTextSharp.text.Font(iTextSharp.text.Font.NORMAL, 16));
        static Paragraph subtitulo = new Paragraph("", new iTextSharp.text.Font(iTextSharp.text.Font.NORMAL, 14));
        static Paragraph paragrafo = new Paragraph("", new iTextSharp.text.Font(iTextSharp.text.Font.NORMAL, 10));
        static Paragraph espaço = new Paragraph("", new iTextSharp.text.Font(iTextSharp.text.Font.NORMAL, 10));
        static Paragraph halfespaço = new Paragraph("", new iTextSharp.text.Font(iTextSharp.text.Font.NORMAL, 7));

        public static Document NovoPdf2(string caminho, DateTime data, int hora)
        {
            var pgSize = new iTextSharp.text.Rectangle(595, 860);
            var doc = new iTextSharp.text.Document(pgSize, 30, 30, 0, 0);

            //Document doc = new Document(iTextSharp.text.PageSize.A4);//criando e estipulando o tipo da folha usada
            //Document doc = new Document(iTextSharp.text.PageSize.A4);//criando e estipulando o tipo da folha usada
            //doc.SetMargins(30, 30, 0, 0); //estibulando o espaçamento das margens que queremos
            doc.AddCreationDate(); //adicionando as configuracoes
                                   //criando o arquivo pdf embranco, passando como parametro a variavel doc criada acima e a variavel caminho tambén criada acima.
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(caminho, FileMode.Create));
            doc.Open();

            iTextSharp.text.Image ImagemPdf0 = iTextSharp.text.Image.GetInstance(Path.Combine(Program.caminhoBase, "logo_comercializacao.png"));
            ImagemPdf0.ScalePercent((iTextSharp.text.PageSize.A4.Width / ImagemPdf0.Width) * 100);
            ImagemPdf0.Alignment = 1; //0=Left, 1=Centre, 2=Right
            doc.Add(ImagemPdf0);
            //etipulando o alinhamneto
            titulo.Alignment = Element.ALIGN_CENTER; //Alinhamento Justificado
            subtitulo.Alignment = Element.ALIGN_LEFT; //Alinhamento Justificado
            paragrafo.Alignment = Element.ALIGN_JUSTIFIED; //Alinhamento Justificado
            espaço.Alignment = Element.ALIGN_JUSTIFIED; //Alinhamento Justificado

            //Adicionando a variavel do tipo "Font"
            titulo.Font = new iTextSharp.text.Font(iTextSharp.text.Font.NORMAL, 16, (int)System.Drawing.FontStyle.Bold);
            titulo.Font.SetFamily("Arial");
            subtitulo.Font = new iTextSharp.text.Font(iTextSharp.text.Font.NORMAL, 14, (int)System.Drawing.FontStyle.Bold);
            paragrafo.Font = new iTextSharp.text.Font(iTextSharp.text.Font.NORMAL, 10, (int)System.Drawing.FontStyle.Regular);
            espaço.Font = new iTextSharp.text.Font(iTextSharp.text.Font.NORMAL, 10, (int)System.Drawing.FontStyle.Regular);

            //string esp0 = " ";
            //espaço.Add(esp0);
            //doc.Add(espaço);

            string tit = "RELATÓRIO ENERCORE - " + data.ToString("dd/MM/yyyy") + " (" + hora.ToString() + " hrs)";
            titulo.Add(tit);
            doc.Add(titulo);

            string esp1 = " ";
            espaço.Add(esp1);
            doc.Add(espaço);
            
            return doc;
        }

        public static Document NovoPdfPrevs(string caminho, DateTime data, int hora)
        {
            

           // Document doc = new Document(iTextSharp.text.PageSize.A4.Rotate());//criando e estipulando o tipo da folha usada
            Document doc = new Document(iTextSharp.text.PageSize.A4);//criando e estipulando o tipo da folha usada                     
                        
            doc.SetMargins(30, 30, 0, 0); //estibulando o espaçamento das margens que queremos
            doc.AddCreationDate(); //adicionando as configuracoes
                                   //criando o arquivo pdf embranco, passando como parametro a variavel doc criada acima e a variavel caminho tambén criada acima.
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(caminho, FileMode.Create));
            doc.Open();

            iTextSharp.text.Image ImagemPdf0 = iTextSharp.text.Image.GetInstance(Path.Combine(Program.caminhoBase, "logo_comercializacao.png"));//Logo2.png
            ImagemPdf0.ScalePercent((iTextSharp.text.PageSize.A4.Width / ImagemPdf0.Width) * 100);
            ImagemPdf0.Alignment = 1; //0=Left, 1=Centre, 2=Right
            doc.Add(ImagemPdf0);
            //etipulando o alinhamneto
            titulo.Alignment = Element.ALIGN_CENTER; //Alinhamento Justificado
            subtitulo.Alignment = Element.ALIGN_LEFT; //Alinhamento Justificado
            paragrafo.Alignment = Element.ALIGN_JUSTIFIED; //Alinhamento Justificado
            espaço.Alignment = Element.ALIGN_JUSTIFIED; //Alinhamento Justificado

            //Adicionando a variavel do tipo "Font"
            titulo.Font = new iTextSharp.text.Font(iTextSharp.text.Font.NORMAL, 16, (int)System.Drawing.FontStyle.Bold);
            titulo.Font.SetFamily("Arial");
            subtitulo.Font = new iTextSharp.text.Font(iTextSharp.text.Font.NORMAL, 14, (int)System.Drawing.FontStyle.Bold);
            paragrafo.Font = new iTextSharp.text.Font(iTextSharp.text.Font.NORMAL, 10, (int)System.Drawing.FontStyle.Regular);
            espaço.Font = new iTextSharp.text.Font(iTextSharp.text.Font.NORMAL, 10, (int)System.Drawing.FontStyle.Regular);

            //string esp0 = " ";
            //espaço.Add(esp0);
            //doc.Add(espaço);

            string tit = "RELATÓRIO ENERCORE - " + data.ToString("dd/MM/yyyy") + " (" + hora.ToString() + " hrs)";
            titulo.Add(tit);
            doc.Add(titulo);

            string esp1 = " ";
            espaço.Add(esp1);
            doc.Add(espaço);

            return doc;
        }

        public static void InserirTexto(this Document doc, string texto)
        {

            paragrafo = new Paragraph();
            paragrafo.Alignment = Element.ALIGN_JUSTIFIED; //Alinhamento Justificado
            paragrafo.Font = new iTextSharp.text.Font(iTextSharp.text.Font.NORMAL, 10, (int)System.Drawing.FontStyle.Regular);
            paragrafo.Font.SetFamily("Arial");
            paragrafo.Add(texto);
            doc.Add(paragrafo);
        }

        public static void InserirSubSubtitulo(this Document doc, string texto)
        {
            paragrafo = new Paragraph();
            paragrafo.Font = new iTextSharp.text.Font(iTextSharp.text.Font.NORMAL, 10, (int)System.Drawing.FontStyle.Regular);
            subtitulo.Font.SetFamily("Arial");
            paragrafo.Add($"    {subparteNum:0}.{subtituloNum:0}.{++subsubtituloNum:0} {texto}");
            doc.Add(paragrafo);
            doc.Add(espaço);

        }

        static int subparteNum = 0;
        static int subtituloNum = 0;
        static int subsubtituloNum = 0;

        public static void InserirEspaco(this Document doc)
        {
            doc.Add(espaço);
        }
        public static void InserirMeioEspaco(this Document doc)
        {
            doc.Add(halfespaço);
        }

        public static void InserirSubtitulo(this Document doc, string texto)
        {
            //doc.Add(espaço);
            subtitulo = new Paragraph();
            subtitulo.Font = new iTextSharp.text.Font(iTextSharp.text.Font.NORMAL, 12, (int)System.Drawing.FontStyle.Bold);
            subtitulo.Font.SetFamily("Arial");
            subtitulo.Add($"{subparteNum:0}.{++subtituloNum:0} {texto}");
            doc.Add(subtitulo);
            doc.Add(espaço);
            subsubtituloNum = 0;
        }
        public static void InserirSubtitulo2(this Document doc, string texto)
        {
            //doc.Add(espaço);
            subtitulo = new Paragraph();
            subtitulo.Font = new iTextSharp.text.Font(iTextSharp.text.Font.NORMAL, 6, (int)System.Drawing.FontStyle.Bold);
            subtitulo.Font.SetFamily("Arial");
            subtitulo.Add($"{subparteNum:0}.{++subtituloNum:0} {texto}");
            doc.Add(subtitulo);
            //doc.Add(halfespaço);
            doc.Add(espaço);
            subsubtituloNum = 0;
        }

        public static void InserirParte(this Document doc, string texto)
        {
            //doc.Add(espaço);
            subtitulo = new Paragraph();            
            subtitulo.Font = new iTextSharp.text.Font(iTextSharp.text.Font.NORMAL, 14, (int)System.Drawing.FontStyle.Bold);
            subtitulo.Font.SetFamily("Arial");
            subtitulo.Add($"PARTE {++subparteNum:0}: {texto}");
            doc.Add(subtitulo);
            doc.Add(espaço);
            subtituloNum = 0;
            subsubtituloNum = 0;
        }
        public static void InserirParte2(this Document doc, string texto)
        {
            //doc.Add(espaço);
            subtitulo = new Paragraph();
            subtitulo.Font = new iTextSharp.text.Font(iTextSharp.text.Font.NORMAL, 8, (int)System.Drawing.FontStyle.Bold);
            subtitulo.Font.SetFamily("Arial");
            subtitulo.Add($"PARTE {++subparteNum:0}: {texto}");
            doc.Add(subtitulo);
           // doc.Add(halfespaço);
            doc.Add(espaço);
            subtituloNum = 0;
            subsubtituloNum = 0;
        }



        public static void NovaPagina2(this Document doc)
        {
            doc.NewPage();

            iTextSharp.text.Image ImagemPdf0 = iTextSharp.text.Image.GetInstance(Path.Combine(Program.caminhoBase, "logo_comercializacao.png"));//Logo4.png
            ImagemPdf0.ScalePercent((iTextSharp.text.PageSize.A4.Width / ImagemPdf0.Width) * 100);
            ImagemPdf0.Alignment = 1; //0=Left, 1=Centre, 2=Right
            doc.Add(ImagemPdf0);
        }

        public static void InserirImagens(this Document doc, float redutor = 1, params string[] caminhos)
        {
            float maxHeigth = (redutor > 1) ? 0.4f : 0.24f;
            redutor = Math.Min(redutor, 1);

            PdfPTable table = new PdfPTable(caminhos.Length);
            table.WidthPercentage = redutor * 100;

            foreach (var i in caminhos)
            {
                if (i != null)
                {
                    Image imagem = CreateImagemX(doc, i, caminhos.Length, redutor, maxHeigth);
                    imagem.Alignment = 1;
                    //  p.Add(new Chunk(imagem, 0, 0));
                    PdfPCell cell = new PdfPCell(imagem);
                    cell.HorizontalAlignment = 1;
                    cell.Border = 0;
                    table.AddCell(cell);
                }
                
            }

            doc.Add(table);
        }

        public static void InserirImagens2(this Document doc, float redutor = 1, params string[] caminhos)
        {
            float maxHeigth = (redutor > 1) ? 0.4f : 0.24f;
           float redutor1 = Math.Min(redutor, 1);
            //redutor = Math.Min(redutor, 1);

            PdfPTable table = new PdfPTable(caminhos.Length);
            table.WidthPercentage = redutor1 * 100;

            foreach (var i in caminhos)
            {
                if (i != null)
                {
                    Image imagem = CreateImagemX(doc, i, caminhos.Length, redutor, maxHeigth);
                    imagem.Alignment = 0;
                    //  p.Add(new Chunk(imagem, 0, 0));
                    PdfPCell cell = new PdfPCell(imagem);
                    cell.HorizontalAlignment = 1;
                    cell.Border = 0;
                    table.AddCell(cell);
                }

            }

            doc.Add(table);
        }
        private static Image CreateImagemX(this Document doc, string caminho, int imgnumber, float redutor = 1, float maxHeigth = 0.24f)
        {
            iTextSharp.text.Image imagem;
            if (System.IO.File.Exists(caminho))
            {
                imagem = iTextSharp.text.Image.GetInstance(caminho);
            }
            else
            {
                imagem = iTextSharp.text.Image.GetInstance(Path.Combine(Program.caminhoBase, "branco.gif"));
            }

            var pageW = doc.PageSize.Width - doc.RightMargin - doc.LeftMargin;
            var pageH = doc.PageSize.Height - doc.TopMargin - doc.BottomMargin;

            imagem.ScaleToFit((pageW / imgnumber) * redutor, pageH * maxHeigth);

            imagem.Border = iTextSharp.text.Rectangle.BOX;
            imagem.BorderColor = iTextSharp.text.BaseColor.BLACK;
            imagem.BorderWidth = 0f;

            return imagem;
        }
    }
}
