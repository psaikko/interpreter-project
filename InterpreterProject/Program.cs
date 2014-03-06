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
            string text1 = "var X : int := 4 + (6 * 2);\n" +
                           "print X;";

            string text2 = "var nTimes : int := 0;\n" +
                           "print \"How many times?\";\n" +
                           "read nTimes;\n" +
                           "var x : int;\n" +
                           "for x in 0..nTimes-1 do\n" +
                           "     print x;\n" +
                           "     print \" : Hello, World!\n\";\n" +
                           "end for;\n" +
                           "assert (x = nTimes);";

            string text3 = "print \"Give a number\";\n" +
                           "var n : int;\n" +
                           "read n;\n" +
                           "var f : int := 1;\n" +
                           "var i : int;\n" +
                           "for i in 1..n do\n" +
                           "    f := f * i;\n" +
                           "end for;\n" +
                           "print \"The result is: \";\n" +
                           "print f;";

            MiniPL miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();
            Parser.Tree parseTree = ps.Parse(sc.Tokenize(text2));
            miniPL.TrimParseTree(parseTree);
            Console.ReadLine();
        }
    }
}
