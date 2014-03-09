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
            string text = "var nTimes : int := 0;\n" +
                          "print \"How many times?\";\n" +
                          "read nTimes;\n" + 
                          "var x : int;\n" +
                          "for x in 0..nTimes-1 do\n" +
                          "     print x;\n" +
                          "     print \" : Hello, World!\n\";\n" +
                          "end for;\n" +
                          "assert (x = nTimes);";

            MiniPL miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();

            Parser.ParseTree pt =  ps.Parse(sc.Tokenize(text));
            miniPL.TrimParseTree(pt);
            Console.ReadLine();
        }
    }
}
