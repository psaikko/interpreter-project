using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    public class Scanner
    {
        TokenAutomaton automaton;
        StreamReader reader;

        public Scanner(TokenAutomaton automaton)
        {
            this.automaton = automaton;
        }

        public List<Token> Tokenize(string text)
        {
            List<Token> tokens = new List<Token>();

            text = text + TokenAutomaton.EOF;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                Console.WriteLine("SCANNER: feeding " + c);
                automaton.FeedCharacter(c);
                Token t = automaton.GetToken();
                if (t != null)
                {
                    if (t.tokenType == TokenType.EOF)
                        break;
                    if (t.tokenType.priority != TokenType.Priority.Whitespace)
                    {
                        Console.WriteLine("SCANNER: token, type: <" + t.tokenType.name + "> lexeme: <" + t.lexeme + ">");
                        tokens.Add(t);
                    }
                }
            }
            return tokens;
        }
    }
}
