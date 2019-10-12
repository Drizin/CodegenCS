using System;
using System.Collections.Generic;
using System.Text;

namespace CodegenCS.Utils
{
    public class IO
    {
        public static System.IO.DirectoryInfo GetCurrentDirectory()
        {
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
            if (dir.Name == "netcoreapp2.1" || dir.Name == "netstandard2.0")
                dir = dir.Parent;
            if (dir.Name == "Debug" || dir.Name == "Release")
                dir = dir.Parent;
            if (dir.Name == "bin" || dir.Name == "obj")
                dir = dir.Parent;
            return dir;
        }
    }
}
