using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.LexicalAnalysis
{
    public class Scanner
    {
        TokenAutomaton automaton;
        
        public Scanner(TokenAutomaton automaton)
        {
            this.automaton = automaton;
        }

        public IEnumerable<Token> Tokenize(Stream input, bool yieldEOF = true)
        {
            StreamReader reader = new StreamReader(input);
            Token t;

            while (!reader.EndOfStream)
            {
                char c = (char) reader.Read();
                Console.WriteLine(String.Format("SCANNER: feeding '{0}'", c));
                automaton.FeedCharacter(c);
                t = automaton.GetToken();
                if (t != null && IsRelevant(t, yieldEOF))
                {
                    Console.WriteLine("SCANNER: yield token "+t);
                    yield return t;
                }
            }

            automaton.FeedCharacter(TokenAutomaton.EOF);

            while ((t = automaton.GetToken()) != null && IsRelevant(t, yieldEOF))
            {
                Console.WriteLine("SCANNER: yield token "+t);
                yield return t;
            }
        }

        private bool IsRelevant(Token t, bool EOFisRelevant)
        {
            return (t.tokenType.priority != TokenType.Priority.Whitespace && (t.tokenType != TokenType.EOF || EOFisRelevant));
        }

        public IEnumerable<Token> Tokenize(string text, bool yieldEOF = true)
        {
            MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(text));
            return Tokenize(ms, yieldEOF);
        }
    }
}
