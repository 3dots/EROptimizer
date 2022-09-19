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
        Task IProgressConsole.WriteLine(string s)
        {
            return Task.Run(() => { Console.WriteLine(s); });
        }
    }

    class DummyProgressConsole : IProgressConsole
    {
        Task IProgressConsole.WriteLine(string s)
        {
            return Task.CompletedTask;
        }
    }
}
