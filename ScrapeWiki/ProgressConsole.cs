using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScrapeWiki
{
    public interface IProgressConsole 
    {
        Task WriteLine(string s);
    }

    class ProgressConsole : IProgressConsole
    {
        private readonly int DASH_COUNT = 10;

        Task IProgressConsole.WriteLine(string s)
        {
            return Task.Run(() => { Console.WriteLine(s); });
        }
    }
}
