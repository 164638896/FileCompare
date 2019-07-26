using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Md5Gen
{
    class Program
    {
        static void Main(string[] args)
        {
            string dataPath = @"./data/";
            if (args.Length > 0) dataPath = args[0];

            FileCompare fileCom = new FileCompare();
            fileCom.CompareFile(dataPath, @"./md5");
        }   
    }
}
