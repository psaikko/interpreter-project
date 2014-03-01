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
            
            Regex aa = Regex.Concat("aa");
            Regex a = Regex.Character('a');
            Regex b = Regex.Character('b');
            Regex aba = Regex.Concat("aba").Plus();
            Regex space = Regex.Character(' ').Plus();

            TokenClass aa_token = new TokenClass("aa", aa);
            TokenClass a_token = new TokenClass("a", a);
            TokenClass b_token = new TokenClass("b", b);
            TokenClass aba_token = new TokenClass("aba", aba);
            TokenClass space_token = new TokenClass("space", space);

            Regex combined = aba.Union(aa).Union(a).Union(b).Union(aba).Union(space);

            Console.WriteLine("Regex created");

            DFA automaton = combined.ConstructDFA();

            Console.WriteLine("Automaton created");

            string text = "bbbabaaba aaabac  aaaca";

            Scanner sc = new Scanner(automaton);
            List<Token> tokens = sc.Tokenize(text);
            Token.PrintList(tokens);

            Console.WriteLine("Finished");
            Console.ReadLine();
        }
    }
}
