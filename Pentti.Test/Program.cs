using System;
using Pentti;

namespace Pentti.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            
            var parts = PSubprocess.Exec("ls", "");
            var o = PSubprocess.CheckOutput("ls", "");

        }
    }
}
