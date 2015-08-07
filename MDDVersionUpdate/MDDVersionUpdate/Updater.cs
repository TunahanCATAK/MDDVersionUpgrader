using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDDVersionUpdate
{
    class Updater
    {
        public string FileName;
        public int lineNumber;
        public string newValue;

        private string LogPath;


        public Updater(string f, int l, string n)
        {
            FileName = f;
            lineNumber = l;
            newValue = n;
        }


        public void LogPathSetter(string p)
        {
            this.LogPath = p;
        }


        public void Update()
        {

            //Trace 
            StreamWriter logWriter = new StreamWriter(LogPath, true);
            logWriter.WriteLine(this.FileName + "-> Line: " + this.lineNumber.ToString() + ": " + this.newValue + "\n");
            logWriter.Close();

            string tmpPath = @"D:\tmp.txt";
            using (var file = File.Open(FileName, FileMode.Open, FileAccess.ReadWrite))
            {
                
                string line = "";
                int line_number = 0;
                var reader = new StreamReader(file);
                var writer = new StreamWriter(tmpPath);
                while ((line = reader.ReadLine()) != null)
                {
                    if (line_number == (lineNumber - 1))
                    {
                        writer.WriteLine(newValue);
                    }
                    else
                    {
                        writer.WriteLine(line);
                    }
                    line_number++;
                }
                file.Close();
                reader.Close();
                writer.Close();
            }

            String str = File.ReadAllText(tmpPath);
            File.WriteAllText(FileName, str);
        }
        

    }
}
