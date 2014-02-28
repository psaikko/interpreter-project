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

        const char EOF = '\0';

        public Scanner(DFA automaton)
        {
            this.automaton = automaton;
        }

        public List<Token> Tokenize(String text)
        {
            List<Token> tokens = new List<Token>();

            for (int i = 0; i <= text.Length; i++)
            {
                char c = (i == text.Length) ? EOF : text[i];

                automaton.FeedCharacter(c);

                if (automaton.IsFailState())
                {
                    Token t = automaton.GetToken();
                    if (t != null)
                    {
                        tokens.Add(t);
                        i -= (automaton.Rewind() + 1);
                    }
                    else
                    {
                        if (c == EOF) break;
                        i -= automaton.Rewind();
                    }
                }
            }
            return tokens;
        }
    }
}
