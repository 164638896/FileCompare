using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FileCompare
{
    private Dictionary<string, Dictionary<string, string>> mOldMd5List = new Dictionary<string, Dictionary<string, string>>();
    private Dictionary<string, string> mCurrMD5Dict = new Dictionary<string, string>();
    private int mCurrProgress = 0;

    public void CompareFile(string md5Path, string dataPath)
    {
        ReadHistoryMD5File(md5Path);
        ReadCurrMD5File(dataPath);
        if (CompareMD5File())
        {
            SaveMD5File(md5Path, mCurrMD5Dict, mOldMd5List.Count);
        }
    }

    private bool ReadHistoryMD5File(string path)
    {
        DirectoryInfo root = new DirectoryInfo(path);
        if (!root.Exists) return false;

        FileInfo[] files = root.GetFiles();
        foreach (FileInfo fileName in files)
        {
            if (!File.Exists(fileName.FullName))
            {
                break;
            }

            Dictionary<string, string> md5Dict = new Dictionary<string, string>();
            string str = "";
            FileStream fileStr = File.OpenRead(fileName.FullName);
            StreamReader sr = new StreamReader(fileStr);

            str = sr.ReadLine();
            while (str != null)
            {
                String[] arrayStr = str.Split(',');
                md5Dict.Add(arrayStr[0], arrayStr[1]);
                str = sr.ReadLine();
            }
            sr.Close();
            String name = System.IO.Path.GetFileNameWithoutExtension(fileName.Name);
            mOldMd5List.Add(name, md5Dict);
            Console.WriteLine("读入旧md5文件");
        }

        return true;
    }

    private bool ReadCurrMD5File(string choosePath)
    {
        // 计算Data 下的md5
        String[] fileNames = Directory.GetFiles(choosePath, "*", SearchOption.AllDirectories);
        //foreach (String fileName in fileNames)
        mCurrProgress = 0;
        for (int fileIndex = 0; fileIndex < fileNames.Length; ++fileIndex)
        {
            string fileName = fileNames[fileIndex];

            if (System.IO.Path.GetExtension(fileName) == ".meta" || System.IO.Path.GetExtension(fileName) == ".manifest")
            {
                continue;
            }

            string fileNameShort = fileName.Substring(choosePath.Length);
            string md5 = Utility.GetMD5HashFromFile(fileName);

            mCurrMD5Dict.Add(fileNameShort, md5);

            int pro = (int)(((float)fileIndex / fileNames.Length) * 100);
            if (pro > mCurrProgress)
            {
                mCurrProgress = pro;
                Console.WriteLine("计算文件md5：{0}%", mCurrProgress);
            }
        }
        return true;
    }

    private bool CompareMD5File()
    {
        if (mOldMd5List.Count > 0)
        {
            Dictionary<string, string> updateFileDic = new Dictionary<string, string>();
            foreach(KeyValuePair<string, Dictionary<string, string>> oldKVP in mOldMd5List.Reverse())
            {
                Dictionary<string, string> oldMd5Dict = oldKVP.Value;

                updateFileDic.Clear();
                foreach (KeyValuePair<string, string> newKVP in mCurrMD5Dict)
                {
                    string oldValue = null;
                    oldMd5Dict.TryGetValue(newKVP.Key, out oldValue);
                    if (oldValue == null || oldValue != newKVP.Value)
                    {
                        // 需要更新的文件
                        updateFileDic.Add(newKVP.Key, newKVP.Value);
                    }
                }

                if (!GenUpdateInfo(updateFileDic, oldKVP.Key, mOldMd5List.Count))
                {
                    Console.WriteLine("文件没有修改");
                    return false;
                }
            }
        }

        return true;
    }

    private bool GenUpdateInfo(Dictionary<string, string> dict, string formIndex, int toIndex)
    {
        if (dict.Count <= 0) return false;

        string toRoot = "update/" + toIndex + "/" + formIndex + "_" + toIndex;
        foreach (KeyValuePair<string, string> kvp in dict)
        {
            string form = "data/" + kvp.Key;
            string to = toRoot + "/" + kvp.Key;

            string path = Path.GetDirectoryName(to);
            Directory.CreateDirectory(path);

            File.Copy(form, to, true);
        }

        string toZIp = "update/" + toIndex + "/" + formIndex + "_" + toIndex + ".zip";
        CreateZipFile(toRoot, toZIp);
        Utility.DelectDir(toRoot);
        return true;
    }

    private void CreateZipFile(string path, string zipFile)
    {
        if (!Directory.Exists(path))
        {
            Console.WriteLine("Cannot find directory '{0}'", path);
            return;
        }

        try
        {
            String[] filenames = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

            using (ZipOutputStream s = new ZipOutputStream(File.Create(zipFile)))
            {
                //s.SetLevel(9); 

                byte[] buffer = new byte[4096];

                foreach (string file in filenames)
                {
                    string filePaht = file.Substring(path.Length);
                    var entry = new ZipEntry(filePaht);
                    entry.DateTime = DateTime.Now;
                    s.PutNextEntry(entry);

                    using (FileStream fs = File.OpenRead(file))
                    {
                        int sourceBytes;
                        do
                        {
                            sourceBytes = fs.Read(buffer, 0, buffer.Length);

                            s.Write(buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);
                    }
                }
                s.Finish();
                s.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception during processing {0}", ex);
        }
    }

    public bool SaveMD5File(string path, Dictionary<string, string> saveDictionary, int index)
    {
        if (saveDictionary.Count <= 0) return false;

        StringBuilder fileInfoes = new StringBuilder();
        foreach (KeyValuePair<string, string> kvp in saveDictionary)
        {
            fileInfoes.Append(kvp.Key).Append(",").Append(kvp.Value).Append("\n");
            //Console.WriteLine("key={0},value={1}", kvp.Key, kvp.Value);
        }

        StringBuilder newMD5File = new StringBuilder();
        String newPath = newMD5File.Append(path).ToString();
        Directory.CreateDirectory(newPath);
        FileStream fs = new FileStream(newMD5File.Append("/").Append(index).Append(".txt").ToString(), FileMode.Create);
        byte[] data = new UTF8Encoding().GetBytes(fileInfoes.ToString());
        fs.Write(data, 0, data.Length);
        fs.Flush();
        fs.Close();

        return true;
    }
}