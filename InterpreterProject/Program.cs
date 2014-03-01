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

            TokenType whitespaceToken = new TokenType("Whitespace", whitespace);
            TokenType stringToken = new TokenType("string", str);

            Regex combined = whitespace.Union(str);

            

            TokenAutomaton automaton = combined.ConstructAutomaton();

            //Console.ReadLine();

            Scanner sc = new Scanner(automaton);

            string text = "\"asdf\" \"sdfg\"";

            

            List<Token> tokens = sc.Tokenize(text);

            Token.PrintList(tokens);

            Console.WriteLine("Finished");
            //Console.ReadLine();
        }
    }
}
