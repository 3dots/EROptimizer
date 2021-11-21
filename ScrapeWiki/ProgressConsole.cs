using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapeWiki
{
    public interface IProgressConsole 
    {
        void WriteLine();
        void WriteLine(string s);
        void WriteSeparator();
    }


    class ProgressConsole : IProgressConsole
    {
        private readonly int DASH_COUNT = 10;

        void IProgressConsole.WriteLine()
        {
            Console.WriteLine();
        }

        void IProgressConsole.WriteLine(string s)
        {
            Console.WriteLine(s);
        }

        void IProgressConsole.WriteSeparator()
        {
            for (int i = 0; i < DASH_COUNT; i++) Console.Write("-");
            Console.WriteLine();
        }
    }
}
