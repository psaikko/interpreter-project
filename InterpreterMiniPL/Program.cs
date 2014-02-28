using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterMiniPL
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Re aa = Re.Concat("aa");
            Re a = Re.Character('a');
            Re b = Re.Character('b');
            Re aba = Re.Plus(Re.Concat("aba"));
            Re space = Re.Plus(Re.Character(' '));

            TokenClass aa_token = new TokenClass("aa", aa);
            TokenClass a_token = new TokenClass("a", a);
            TokenClass b_token = new TokenClass("b", b);
            TokenClass aba_token = new TokenClass("aba", aba);
            TokenClass space_token = new TokenClass("space", space);

            Re combined = Re.Union(Re.Union(Re.Union(Re.Union(aba, space), a), aa), b);

            Console.WriteLine("Regex created");

            DFA automaton = combined.ConstructDFA();

            Console.WriteLine("Automaton created");

            string text = "bbbabaaba aaabac  aaa";
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                //Console.WriteLine("feed: " + c);
                automaton.FeedCharacter(c);
                if (automaton.IsFailState())
                {
                    if (automaton.GetToken() != null)
                    {
                        Token t = automaton.GetToken();
                        Console.WriteLine("token: "+t.type.name+ " - lexeme: "+t.lexeme);
                        i -= (automaton.Rewind() + 1);
                    }
                    else
                    {
                        Console.WriteLine("Invalid token");
                        i -= automaton.Rewind();
                    }
                }
            }
            Console.WriteLine(automaton.GetToken().lexeme);

            Console.WriteLine("Finished");
            Console.ReadLine();
        }
    }
}
