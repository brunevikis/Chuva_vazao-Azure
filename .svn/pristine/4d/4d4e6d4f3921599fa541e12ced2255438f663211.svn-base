using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuvaVazaoTools.Classes
{
    public class AddLog
    {
        public AddLog(string texto)
        {
            try
            {
                var path = "C:\\Sistemas\\ChuvaVazao\\Log";
                var file = "Chuva_vazao" + DateTime.Today.ToString("yyyyMMdd") + ".log";
                if (File.Exists(Path.Combine(path, file)))
                {
                    //texto += File.ReadAllText(Path.Combine(path, file));
                    File.WriteAllText(Path.Combine(path, file), texto);
                }
                else
                {
                    
                    File.Create(Path.Combine(path, file));
                    new AddLog(texto);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
