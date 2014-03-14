using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterProject.LexicalAnalysis;
using InterpreterProject.SyntaxAnalysis;
using InterpreterProject.Languages;
using InterpreterProject.Errors;
using System.IO;

namespace InterpreterProject
{
    class Program
    {
        public static bool debug = true;

        static void Main(string[] args)
        {
            string text = "var s : string; for s in \"hello\"..(2 = 2) do print 1; end for;";

            MiniPL miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();
            ParseTree ptree = ps.Parse(sc.Tokenize(text));

            MiniPL.Runnable prog = miniPL.ProcessParseTree(ptree, ps.GetErrors());

            foreach (IError err in prog.errors)
                Console.WriteLine(err.GetMessage());

            Console.ReadLine();
        }
    }
}
