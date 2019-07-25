using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;


public class Utility
{
    public static void CopyDirectory(string srcPath, string destPath)
    {
        DirectoryInfo dir = new DirectoryInfo(srcPath);
        if (dir == null || !dir.Exists)
        {
            //Debug.LogError("文件夹不存在：" + srcPath);
            Console.WriteLine("文件夹不存在：" + srcPath);
            return;
        }

        if (!Directory.Exists(destPath))
        {
            Directory.CreateDirectory(destPath);
        }

        FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //获取目录下（不包含子目录）的文件和子目录
        foreach (FileSystemInfo i in fileinfo)
        {
            if (i is DirectoryInfo)     //判断是否文件夹
            {
                if (!Directory.Exists(destPath + "/" + i.Name))
                {
                    Directory.CreateDirectory(destPath + "/" + i.Name);   //目标目录下不存在此文件夹即创建子文件夹
                }
                CopyDirectory(i.FullName, destPath + "/" + i.Name);    //递归调用复制子文件夹
            }
            else
            {
                Console.WriteLine("i.FullName:" + i.FullName + " -> " + destPath + "/" + i.Name);
                File.Copy(i.FullName, destPath + "/" + i.Name, true);      //不是文件夹即复制文件，true表示可以覆盖同名文件
            }
        }
    }

    public static void DelectDir(string srcPath)
    {
        if (!Directory.Exists(srcPath))
        {
            return;
        }
        DirectoryInfo dir = new DirectoryInfo(srcPath);
        dir.Delete(true);
    }

    public static string GetMD5HashFromFile(string fileName)
    {
        using (FileStream file = new FileStream(fileName, FileMode.Open))
        {
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(file);

            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("x2"));
            }
            return result.ToString();
        }
    }

    public static System.Diagnostics.Process CreateShellExProcess(string cmd, string args, string workingDir = "")
    {
        var pStartInfo = new System.Diagnostics.ProcessStartInfo(cmd);
        pStartInfo.Arguments = args;
        pStartInfo.CreateNoWindow = false;
        pStartInfo.UseShellExecute = true;
        pStartInfo.RedirectStandardError = false;
        pStartInfo.RedirectStandardInput = false;
        pStartInfo.RedirectStandardOutput = false;
        if (!string.IsNullOrEmpty(workingDir))
            pStartInfo.WorkingDirectory = workingDir;
        return System.Diagnostics.Process.Start(pStartInfo);
    }

    public static void RunBat(string batfile, string args, string workingDir = "")
    {
        var p = CreateShellExProcess(batfile, args, workingDir);
        p.Close();
    }

    //public static string Post(string jsonstring, Dictionary<string, string> headers = null)
    //{
    //    ServicePointManager.ServerCertificateValidationCallback =
    //        delegate (object s, X509Certificate certificate,
    //                 X509Chain chain, SslPolicyErrors sslPolicyErrors)
    //        { return true; };
    //    WebRequest request = WebRequest.Create(WEB_HOOK);
    //    request.Method = "POST";
    //    request.ContentType = "application/json";
    //    if (headers != null)
    //    {
    //        foreach (var keyValue in headers)
    //        {
    //            if (keyValue.Key == "Content-Type")
    //            {
    //                request.ContentType = keyValue.Value;
    //                continue;
    //            }
    //            request.Headers.Add(keyValue.Key, keyValue.Value);
    //        }
    //    }

    //    if (string.IsNullOrEmpty(jsonstring))
    //    {
    //        request.ContentLength = 0;
    //    }
    //    else
    //    {
    //        byte[] bs = Encoding.UTF8.GetBytes(jsonstring);
    //        request.ContentLength = bs.Length;
    //        Stream newStream = request.GetRequestStream();
    //        newStream.Write(bs, 0, bs.Length);
    //        newStream.Close();
    //    }


    //    WebResponse response = request.GetResponse();
    //    Stream stream = response.GetResponseStream();
    //    Encoding encode = Encoding.UTF8;
    //    StreamReader reader = new StreamReader(stream, encode);
    //    string resultJson = reader.ReadToEnd();
    //    return resultJson;
    //}

    //public static String GetFileName(String path)
    //{
    //    String fileName = System.IO.Path.GetFileName(path);
    //    return fileName;
    //}

    //public static String GetFileNameExt(String path)
    //{
    //    String strExt = System.IO.Path.GetExtension(path);
    //    return strExt;
    //}
}
