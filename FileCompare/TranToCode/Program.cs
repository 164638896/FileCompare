using ExcelDataReader;
using LumenWorks.Framework.IO.Csv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TranToCode
{
    class Program
    {
        static void Main(string[] args)
        {
            var t = DateTime.Now;
            string rootPath = "../root/";
            if (args.Length > 0)
            {
                rootPath = args[0];
            }

            List<string> luaFiles = new List<String>();
            var files = Directory.GetFiles(rootPath + "Assets/ResData/Lua", "*.lua.txt", SearchOption.AllDirectories).Where(s => !s.Contains("Assets/ResData/Lua\\Data"));
            //var files = Directory.GetFiles(rootPath + "Assets/ResData/Lua", "*.lua.txt", SearchOption.AllDirectories).Where((s) => { return !s.Contains("Assets/ResData/Lua\\Data"); });
            luaFiles.AddRange(files);

            importTranslateToFile(rootPath + "Assets/ResData/Data/Localization/Localization_lua.csv", luaFiles, @"""(Lua\..+?)""", " --翻译: ");

            List<string> csFiles = new List<String>();
            csFiles.AddRange(Directory.GetFiles(rootPath + "Assets/Scripts", "*.cs", SearchOption.AllDirectories));
            importTranslateToFile(rootPath + "Assets/ResData/Data/Localization/Localization_cs.csv", csFiles, @"""(CS\..+?)""", " //翻译: ");
            Console.WriteLine("TotalSeconds:" + (DateTime.Now - t).TotalSeconds);

            Console.Write("按任意键退出...");
            Console.ReadKey(true);
        }

        public static void importTranslateToFile(string csvPath, List<string> files, string pattern, string tranHead)
        {
            Dictionary<string, string> keyTranMap = new Dictionary<string, string>();

            var t = DateTime.Now;

            //using (var stream = File.Open(csvPath, FileMode.Open, FileAccess.Read))
            //{
            //    using (var reader = ExcelReaderFactory.CreateCsvReader(stream))
            //    {
            //        var result = reader.AsDataSet();
            //        var table = result.Tables[0];
            //        for (int i = 1; i < table.Rows.Count; i++)
            //        {
            //            string key = table.Rows[i][0].ToString();
            //            string tran = table.Rows[i][3].ToString();
            //            keyTranMap.Add(key, tran.Replace("\r\n", "").Replace("\n", ""));
            //        }
            //    }
            //}

            using (var reader = new CsvReader(new StreamReader(csvPath), false))
            {
                var arr = reader.ToArray();
                for (int i = 1; i < arr.Length; i++)
                {
                    keyTranMap.Add(arr[i][0], arr[i][3].Replace("\r\n", "").Replace("\n", ""));
                }
            }
            Console.WriteLine(Path.GetFileNameWithoutExtension(csvPath) + " time:" + (DateTime.Now - t).TotalSeconds);

            string currLine = "";
            foreach (var file in files)
            {
                bool isWrite = false;
                StringBuilder sbNewFile = new StringBuilder();
                StringBuilder sbTranslate = new StringBuilder();

                StreamReader sreader = new StreamReader(File.OpenRead(file));

                currLine = sreader.ReadLine();
                while (currLine != null)
                {
                    MatchCollection Matches = Regex.Matches(currLine, pattern);

                    for(int i =0; i < Matches.Count; ++i)
                    {
                        Match result = Matches[i];
                        if (result.Groups.Count > 1)
                        {
                            var g = result.Groups[1];
                            string key = g.Value;

                            string v = "";
                            keyTranMap.TryGetValue(key, out v);
                            if (v != null)
                            {
                                sbTranslate.Append(tranHead).Append(v);

                                // 删除旧的
                                int index = currLine.IndexOf(tranHead);
                                if (index > 0)
                                {
                                    string oldTranslate = currLine.Substring(index);
                                    currLine = currLine.Replace(oldTranslate, "");
                                }

                                isWrite = true;
                            }
                        }
                    }
                    sbNewFile.Append(currLine);
                    if (Matches.Count > 0)
                    {
                        sbNewFile.Append(sbTranslate.ToString());
                    }
                    sbNewFile.AppendLine();

                    sbTranslate.Clear();
                    currLine = sreader.ReadLine();
                }
                sreader.Close();

                if (isWrite)
                {
                    using (StreamWriter streamWriter = new StreamWriter(file, false, Encoding.UTF8))
                    {
                        streamWriter.Write(sbNewFile.ToString());
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                }
            }
        }
    }
}
