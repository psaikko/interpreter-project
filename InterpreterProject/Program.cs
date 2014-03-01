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

            Regex whitespace = Regex.Union(" \t\n").Star();
            Regex str = Regex.Character('"').Concat(Regex.Not('"').Star()).Concat(Regex.Character('"'));

            TokenClass whitespaceToken = new TokenClass("Whitespace", whitespace);
            TokenClass stringToken = new TokenClass("string", str);

            Regex combined = whitespace.Union(str);

            //Console.ReadLine();

            DFA automaton = combined.ConstructDFA();
            Scanner sc = new Scanner(automaton);

            string text = "\"asdf\" \"sdfg\"";

            

            List<Token> tokens = sc.Tokenize(text);

            Token.PrintList(tokens);

            Console.WriteLine("Finished");
            Console.ReadLine();
        }
    }
}
