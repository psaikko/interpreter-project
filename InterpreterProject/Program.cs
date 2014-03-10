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
            string text = "var name : string; print \"name?! \"; read name; print \"Hello \"; print name; print \"\\n\";";

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
