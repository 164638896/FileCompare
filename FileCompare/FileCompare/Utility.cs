using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;


public class Utility
{
    public static void CopyFolder(string srcPath, string dstPath, string[] excludeExtensions = null, bool overwrite = true)
    {
        if (!Directory.Exists(srcPath))
        {
            Console.WriteLine("文件夹不存在：" + srcPath);
            return;
        }

        if (!Directory.Exists(dstPath))
        {
            Directory.CreateDirectory(dstPath);
        }

        var files = Directory.GetFiles(srcPath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(path => excludeExtensions == null || !excludeExtensions.Contains(Path.GetExtension(path)));

        foreach (var file in files)
        {
            File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)), overwrite);
        }

        foreach (var dir in Directory.GetDirectories(srcPath))
        {
            CopyFolder(dir, Path.Combine(dstPath, Path.GetFileName(dir)), excludeExtensions, overwrite);
        }
    }

    public static bool DeleteFolder(string dirSource)
    {
        try
        {
            if (Directory.Exists(dirSource))
            {
                Directory.Delete(dirSource, true);
            }
            return true;
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    //移动文件夹
    public static bool MoveFolder(string dirSource, string dirDestination, bool replace = false)
    {
        try
        {
            if (Directory.Exists(dirSource))
            {
                if (replace && Directory.Exists(dirDestination))
                {
                    Directory.Delete(dirDestination, true);
                }

                Directory.Move(dirSource, dirDestination);
            }
            return true;

        }
        catch (Exception ex) // 异常处理
        {
            Console.WriteLine(ex.Message);
            return false;
        }

    }

    public static bool MoveFile(string fileSrc, string fileDes, bool replace = false)
    {
        try
        {
            if (File.Exists(fileSrc))
            {
                string root = Path.GetDirectoryName(fileDes);
                if (!Directory.Exists(root))
                {
                    Directory.CreateDirectory(root);
                }

                if (replace && File.Exists(fileDes))
                {
                    File.Delete(fileDes);
                }

                File.Move(fileSrc, fileDes);
            }
            return true;
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
        
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
