using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    class Program
    {
        static void Main(string[] args)
        {
            ILanguage miniPL = MiniPL.GetInstance();
            Console.ReadLine();
        }
    }
}
