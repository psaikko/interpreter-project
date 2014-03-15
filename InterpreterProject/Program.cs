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
            if (args.Length == 2)
                debug = false;

            switch (args.Length)
            {
                case 0:
                    Console.WriteLine("Specify a file path.");
                    break;
                default:
                    FileStream fs;
                    try
                    {
                        fs = System.IO.File.OpenRead(args[0]);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        break;
                    }

                    MiniPL miniPL = MiniPL.GetInstance();
                    Scanner sc = miniPL.Scanner;
                    Parser ps = miniPL.Parser;
                    ParseTree ptree = ps.Parse(sc.Tokenize(fs));
                    MiniPL.Runnable prog = miniPL.ProcessParseTree(ptree, ps.Errors);
                    prog.Execute(Console.In, Console.Out);

                    break;
            }
        }
    }
}
