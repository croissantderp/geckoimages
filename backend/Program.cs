using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;
using System.Management.Automation;

namespace geckoimagesBackend
{
    class Program
    {
        
        static void Main(string[] args)
        {
            /*
            DirectoryInfo folder = new DirectoryInfo(@"C:\Users\yiunf\source\repos\geckoimages\public\geckos");
            
            string toSave = "";

            foreach (FileInfo file in folder.GetFiles().OrderBy(a => a.Name))
            {
                if (file.Name == "placeholder.png" || file.Name == "db.txt")
                {
                    continue;
                }

                toSave += file.Name + ";;;";
            }

            StreamWriter db = new (@"C:\Users\yiunf\source\repos\geckoimages\public\geckos\db.txt");
            db.Write(toSave);
            db.Close
            */

        }

        public void checkDrive()
        {

        }

        private void deploy()
        {
            using (Process process = new Process())
            {
                process.StartInfo.WorkingDirectory = @"C:\Users\yiunf\source\repos\geckoimages";
                process.StartInfo.FileName = @"cmd.exe";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                //process.StartInfo.RedirectStandardOutput = true;

                process.Start();

                process.StandardInput.WriteLine(@"C:\Users\yiunf\source\repos\geckoimages\firebase-tools-instant-win.exe");

                Thread.Sleep(10000);

                process.StandardInput.WriteLine(@"firebase deploy --only hosting:geckos");
            };
        }
    }
}
