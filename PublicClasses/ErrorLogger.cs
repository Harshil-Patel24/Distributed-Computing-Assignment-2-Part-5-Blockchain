using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PublicClasses
{
    public class ErrorLogger
    {
        public static Mutex mut = new Mutex();
        private const string ERROR_DIR = "D:\\Harshil\\Uni\\Units\\DC\\Assignment_2\\LogFiles";

        public static void ClearFiles()
        {
            DirectoryInfo di = new DirectoryInfo(ERROR_DIR);
            foreach (FileInfo file in di.GetFiles())
            {
                if(file.Name.Contains("ErrorLogPart5"))
                {
                    file.Delete();
                }
            }
        }

        public static void WriteError(string Error)
        {
            mut.WaitOne();
            string ErrorFileName = ERROR_DIR + "\\ErrorLogPart5" + "_" + Process.GetCurrentProcess().Id + ".txt";
            using (StreamWriter sw = File.AppendText(ErrorFileName))
            {
                sw.WriteLine(Error);
            }
            mut.ReleaseMutex();
        }
    }
}
