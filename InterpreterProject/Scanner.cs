using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    public class Scanner
    {
        TokenAutomaton automaton;

        public Scanner(TokenAutomaton automaton)
        {
            this.automaton = automaton;
        }

        public List<Token> Tokenize(string text)
        {
            List<Token> tokens = new List<Token>();

            text = text + TokenAutomaton.EOF;
            for (int i = 0; i <= text.Length; i++)
            {
                char c = text[i];            
                automaton.FeedCharacter(c);

                if (automaton.IsFailState())
                {
                    
                    Token t = automaton.GetToken();
                    if (t != null)
                    {
                        Console.WriteLine("SCANNER: Recognize token, type: <" + t.type.name + "> lexeme: <" + t.lexeme + ">");
                        // EOF handled internally by scanner, automaton - don't pass it forward
                        if (t.type == TokenType.EOF)
                            break;
                        if (t.type.priority != TokenType.Priority.Whitespace)
                            tokens.Add(t);
                        i -= automaton.Rewind();
                    }
                    else
                    {
                        Console.WriteLine("SCANNER: Invalid token!");
                        // Add error token, skip one character forward in text
                        tokens.Add(automaton.GetErrorToken());
                        i -= (automaton.Rewind() - 1);
                    }
                }
            }
            return tokens;
        }
    }
}
