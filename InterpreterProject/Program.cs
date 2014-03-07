using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterProject.LexicalAnalysis;
using InterpreterProject.SyntaxAnalysis;
using InterpreterProject.Languages;

namespace InterpreterProject
{
    class Program
    {
        static void Main(string[] args)
        {
            string text = "var nTimes : int := -0;\n" + // no unary negative
                          "print \"How many times?\";\n" +
                          "read nTimes + 1;\n" + // read expr
                          "x : int;\n" + // declaration without var
                          "for x in 0..nTimes-1 do\n" +
                          "     print x;\n" +
                          "     print \" : Hello, World!\n\";\n" +
                          "end for;\n" +
                          "assert (x = nTimes); - "; // stray -

            MiniPL miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();

            Console.WriteLine(ps.Parse(sc.Tokenize(text)).errors.Count);
            Console.ReadLine();
        }
    }
}
