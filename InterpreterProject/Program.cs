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
        public static bool debug = false;

        static void Main(string[] args)
        {
            string program = "var i : int; for i in 0..9 do print i; end for;";

            MiniPL miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();

            Tree<Parser.IParseValue> ptree = ps.Parse(sc.Tokenize(program));
            MiniPL.Runnable prog = miniPL.ProcessParseTree(ptree, ps.GetErrors());

            prog.Execute(Console.In, Console.Out);

            Console.ReadLine();
        }
    }
}
