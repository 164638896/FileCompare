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
            FileCompare fileCom = new FileCompare();
            fileCom.CompareFile(@"./md5", @"./data/");
        }   
    }
}
