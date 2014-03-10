using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterProject.LexicalAnalysis;
using InterpreterProject.SyntaxAnalysis;
using InterpreterProject.Languages;
using InterpreterProject.Errors;

namespace InterpreterProject
{
    class Program
    {
        static void Main(string[] args)
        {
            string text = "var X : int := 4 + (6 * 2);\n" +
                          "print X;";

            MiniPL miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();
            Tree<Parser.IParseValue> ptree = ps.Parse(sc.Tokenize(text));
            List<IError> errors = ps.GetErrors();

            MiniPL.Runnable prog = miniPL.ProcessParseTree(ptree);
            prog.Execute(Console.In, Console.Out);


            Console.ReadLine();
        }
    }
}
