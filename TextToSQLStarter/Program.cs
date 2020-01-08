using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextToSQLStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\HB Factory Programs\\TextToSql"))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\HB Factory Programs\\TextToSql");
            }
            File.Copy(@"\\joi\eu\Public\EE Process Test\Software\TextToSQL\TextToSQL.exe", Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\HB Factory Programs\\TextToSql\\TextToSQL.exe", true);
            Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\HB Factory Programs\\TextToSql\\TextToSQL.exe");

        }
    }
}
