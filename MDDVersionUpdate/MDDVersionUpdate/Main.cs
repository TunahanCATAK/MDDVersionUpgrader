using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDDVersionUpdate
{
    public class Main
    {
        private string Path;
        private string RootPath;
        private string LogPath;
        private string IntegrationPath;
        private string projFilePath;
        public DelAddLine AddLineToRichText;
        public DelUpdateBar UpdateProgressBar;

        static String filePath;
        static List<Updater> updateList = new List<Updater>();
        static List<string> integrationList = new List<string>();
        enum Version
        {
            None,
            Ver11,
            Ver12,
            Ver13,
            Ver13_0,
            Ver13_0_1_0,
            Ver13_1
        };

        public Main(string p, string r, string l, string i, DelAddLine del, DelUpdateBar updDel)
        {
            Path = p;
            RootPath = r;
            LogPath = l;
            IntegrationPath = i;
            AddLineToRichText = del;
            UpdateProgressBar = updDel; 
            projFilePath = RootPath + @"\MddProject.csproj";
        }

        public void Upgrade()
        {
            string[] v14mddFiles = Directory.GetFiles(Path, "*V14.0.0.0.mdd");

            UpdateProgressBar(1);
            for (int i = 0; i < v14mddFiles.Count(); i++)
            {
                CheckV13Mdds(v14mddFiles[v14mddFiles.Count() - i - 1]);  
                UpdateProgressBar((int)((i + 1) * 100 / v14mddFiles.Count()));
            }

            UpdateAllList();
            WriteIntegrationFiles();

            AddLineToRichText("!!!!Opeartion Completed!!!!");
        }


        private void WriteIntegrationFiles()
        {
            StreamWriter logWriter = new StreamWriter(IntegrationPath, true);
            for (int i = 0; i < integrationList.Count(); i++)
            {
                logWriter.WriteLine(integrationList[i]);
            }
            logWriter.Close();
        }

        private void UpdateAllList()
        {
            for (int i = 0; i < updateList.Count; i++)
            {
                Updater tmpObj = updateList[i];
                tmpObj.Update();
                AddNeedIntegrationList(tmpObj);
            }
        }

        private void AddNeedIntegrationList(Updater tmpObj)
        {
            string[] mddName = tmpObj.FileName.Split('\\');
            string[] xmlFiles = Directory.GetFiles(RootPath, "*.xml");
            for (int i = 0; i < xmlFiles.Count(); i++)
            {
                if (CheckNeedIntegration(xmlFiles[i], mddName[mddName.Length - 1]))
                {
                    if (!integrationList.Contains(xmlFiles[i]))
                    {
                        integrationList.Add(xmlFiles[i]);
                    }
                }
            }
        }

        private bool CheckNeedIntegration(string p1, string p2)
        {
            var allLines = File.ReadAllLines(p1);
            for (int i = 0; i < allLines.Count(); i++)
            {
                if (allLines[i].Contains(p2))
                {
                    return true;
                }
            }
            return false;
        }

        private void CheckV13Mdds(string p)
        {
            AddLineToRichText("Checking " + p.ToString() + "  file.");
            string line;
            StreamReader file = new StreamReader(p);
            int lineCounter = 0;
            while (null != (line = file.ReadLine()))
            {
                lineCounter++;
                if (0 == String.Compare(line, 0, "IMPORT", 0, 6, true))
                {
                    Version ver = Version.None;

                    if (line.Contains("V13.0.10.0"))
                    {
                        ver = Version.Ver13_1;
                    }
                    else if (line.Contains("V13.0.0.0"))
                    {
                        ver = Version.Ver13_0;
                    }
                    else if (line.Contains("V13.1"))
                    {
                        ver = Version.Ver13;
                    }
                    else if (line.Contains("_V13.0.1.0"))
                    {
                        ver = Version.Ver13_0_1_0;
                    }
                    else if (line.Contains("V12.0"))
                    {
                        ver = Version.Ver12;
                    }
                    else if (line.Contains("V11.0"))
                    {
                        ver = Version.Ver11;
                    }

                    if (ver != Version.None)
                    {
                        int startIndex = findMDDPrefixIndex(line);
                        String mddName = new String(line.ToCharArray(), startIndex, (line.Length - (startIndex + 1)));
                        //string rootDir = @"D:\WS_TUNAHAN\WM5_WinCC_HW_Work\src\HM\Xdd";
                        //string rootDir = @"C:\Users\tunahanTest\Desktop\Test";

                        findInXDDFolder(RootPath, mddName);
                        string newMddPath = ReplaceFunc(filePath);
                        string newLine = ReplaceFunc(line) + "\"";
                        if (File.Exists(newMddPath))
                        {
                            OperationsForExistFiles(p, lineCounter, newMddPath, newLine);
                        }
                        else
                        {
                            newMddPath = ReplaceFunc(filePath, "_V14.0");
                            if (File.Exists(newMddPath))
                            {
                                newLine = ReplaceFunc(line, "_V14.0") + "\"";
                                OperationsForExistFiles(p, lineCounter, newMddPath, newLine);
                            }
                            else
                            {
                                newMddPath = ReplaceFunc(filePath);
                                newLine = ReplaceFunc(line) + "\"";
                                OperationsForNonExistFiles(p, lineCounter, newMddPath, newLine, filePath);
                            }
                        }
                        CheckV13Mdds(newMddPath);
                    }

                }
            }

            file.Close();
        }

        private int findMDDPrefixIndex(string line)
        {
            char[] prefix = { 'M', 'D', 'D', '_' };
            char[] line_array = line.ToUpper().ToCharArray();
            for (int i = 0; i < line_array.Length - prefix.Length; i++)
            {
                int counter = 0;
                for (int j = 0; j < prefix.Length; j++)
                {
                    if (line_array[i + j] != prefix[j])
                        break;
                    else
                    {
                        counter++;
                    }
                }
                if (counter == prefix.Length)
                    return i;
            }

            return -1;
        }

        private void findInXDDFolder(string rootDir, string mddName)
        {
            string[] files = Directory.GetFiles(rootDir, mddName);
            if (files.Count() > 0)
                filePath = files[0];
            foreach (string d in Directory.GetDirectories(rootDir))
            {
                foreach (string f in Directory.GetFiles(d, mddName))
                {
                    filePath = f;
                }
                findInXDDFolder(d, mddName);
            }
        }

        private string ReplaceFunc(string s, string ver = "_V14.0.0.0")
        {
            char[] tmp_s = s.ToCharArray();
            List<char> tmp = new List<char>();
            List<char> output = new List<char>();
            int len = s.Length;
            bool copyFlag1 = false;
            bool copyFlag = false;
            int counter = 0;

            for (int i = len - 1; i >= 0; i--)
            {
                if (true == copyFlag)
                {
                    tmp.Add(s[i]);
                    counter++;
                }
                else
                {
                    if (true == copyFlag1)
                    {
                        if (s[i] == '_')
                        {
                            copyFlag = true;
                        }
                        copyFlag1 = false;
                    }
                    else
                    {
                        if (s[i] == 'V')
                        {
                            copyFlag1 = true;
                        }
                    }
                }
            }

            for (int i = counter - 1; i >= 0; i--)
            {
                output.Add(tmp[i]);
            }

            char[] out_char = output.ToArray();
            string retString = new string(out_char);
            return (retString + ver + ".mdd");


        }

        private void OperationsForExistFiles(string p, int lineCounter, string newMddPath, string newLine)
        {
            //Trace 
            StreamWriter logWriter = new StreamWriter(LogPath, true);
            logWriter.WriteLine(newMddPath + " exists.\n");
            logWriter.Close();

            UpdateImportLine(lineCounter, p, newLine);
        }

        private void OperationsForNonExistFiles(string p, int lineCounter, string newMddPath, string newLine, string oldMddPath)
        {
            UpdateImportLine(lineCounter, p, newLine);

            //Trace 
            StreamWriter logWriter = new StreamWriter(LogPath, true);
            logWriter.WriteLine(newMddPath + "'ll be created.\n");
            logWriter.Close();

            System.IO.File.Copy(filePath, newMddPath, true);

            AddLineToMddProjectFile(newMddPath, oldMddPath);
        }

        private void AddLineToMddProjectFile(string newMddPath, string oldMddPath)
        {
            var allLines = File.ReadAllLines(projFilePath).ToList();
            string[] oldMddName = oldMddPath.Split('\\');
            int insertLine = -1;

            for (int i = 0; i < allLines.Count; i++)
            {
                if (allLines[i].Contains(oldMddName[oldMddName.Length - 1]))
                {
                    insertLine = i;
                    break;
                }
            }

            if (insertLine != -1)
            {
                string[] newMddName = newMddPath.Split('\\');
                string newlineForMddProj = allLines[insertLine].Replace(oldMddName[oldMddName.Length - 1], newMddName[newMddName.Length - 1]);
                allLines.Insert(insertLine, newlineForMddProj);
                File.WriteAllLines(projFilePath, allLines);
            }
        }

        private void UpdateImportLine(int lineCounter, string path, string newValue)
        {
            Updater tmpObj = new Updater(path, lineCounter, newValue);
            tmpObj.LogPathSetter(LogPath);
            updateList.Add(tmpObj);
        }

    }
}
