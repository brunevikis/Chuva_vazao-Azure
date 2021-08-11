using ChuvaVazaoTools.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    class Program
    {
        static void Main(string[] args)
        {
            Tools.addHistory("C:\\Sistemas\\ChuvaVazao\\Log\\report.txt", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "- tentativa de gerar relatório via sei lá o que [metodo sem nome]");
            Report.Program.CriarRelatorio2(DateTime.Today.Date, preliminar: true);
        }
    }
}
