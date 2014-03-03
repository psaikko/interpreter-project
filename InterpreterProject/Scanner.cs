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
        
        public Scanner(TokenAutomaton automaton)
        {
            this.automaton = automaton;
        }

        public IEnumerable<Token> Tokenize(Stream input)
        {
            StreamReader reader = new StreamReader(input);
            Token t;

            while (!reader.EndOfStream)
            {
                char c = (char) reader.Read();
                Console.WriteLine("SCANNER: feeding " + c);
                automaton.FeedCharacter(c);
                t = automaton.GetToken();
                if (t != null && IsRelevant(t))
                {
                    Console.WriteLine("SCANNER: token, type: <" + t.tokenType.name + "> lexeme: <" + t.lexeme + ">");
                    yield return t;
                }
            }

            automaton.FeedCharacter(TokenAutomaton.EOF);

            while ((t = automaton.GetToken()) != null && IsRelevant(t))
            {
                Console.WriteLine("SCANNER: token, type: <" + t.tokenType.name + "> lexeme: <" + t.lexeme + ">");
                yield return t;
            }
        }

        private bool IsRelevant(Token t)
        {
            return (t.tokenType.priority != TokenType.Priority.Whitespace && t.tokenType != TokenType.EOF);
        }

        public IEnumerable<Token> Tokenize(string text)
        {
            MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(text));
            return Tokenize(ms);
        }
    }
}
