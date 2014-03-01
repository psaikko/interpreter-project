using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    public class Scanner
    {
        DFA automaton;

        public Scanner(DFA automaton)
        {
            this.automaton = automaton;
        }

        public List<Token> Tokenize(String text)
        {
            List<Token> tokens = new List<Token>();

            for (int i = 0; i <= text.Length; i++)
            {
                char c = (i == text.Length) ? DFA.EOF : text[i];

                Console.WriteLine("Tokenizer: i = " + i + "\t c = '" + c + "'");

                automaton.FeedCharacter(c);

                if (automaton.IsFailState())
                {
                    Console.WriteLine("Tokenizer: fail state");
                    Token t = automaton.GetToken();
                    if (t != null)
                    {
                        if (t.type == TokenClass.EOF)
                            break;
                        Console.WriteLine("Tokenizer: adding token");
                        tokens.Add(t);
                        i -= automaton.Rewind();
                    }
                    else
                    {
                        Console.WriteLine("Tokenizer: error");
                        Console.WriteLine();
                        tokens.Add(automaton.GetErrorToken());
                        i -= (automaton.Rewind() - 1);
                    }
                }
            }
            return tokens;
        }
    }
}
