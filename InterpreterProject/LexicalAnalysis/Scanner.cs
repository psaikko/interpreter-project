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
        TokenType blockCommentStart;
        TokenType blockCommentEnd;

        public Scanner(TokenAutomaton automaton, TokenType blockCommentStart = null, TokenType blockCommentEnd = null)
        {
            this.automaton = automaton;
            this.blockCommentStart = blockCommentStart;
            this.blockCommentEnd = blockCommentEnd;
        }

        public IEnumerable<Token> Tokenize(Stream input, bool yieldEOF = true)            
        {
            StreamReader reader = new StreamReader(input);
            Token t;
            int commentDepth = 0;

            while (!reader.EndOfStream)
            {
                char c = (char) reader.Read();
                if (Program.debug) Console.WriteLine(String.Format("SCANNER: feeding '{0}'", c));
                automaton.FeedCharacter(c);
                t = automaton.GetToken();
                if (t != null)
                {
                    if (t.tokenType == blockCommentStart)
                    {
                        if (Program.debug) Console.WriteLine("SCANNER: incrementing comment depth");
                        commentDepth++;
                        continue;
                    }                        
                    if (t.tokenType == blockCommentEnd)
                    {
                        if (Program.debug) Console.WriteLine("SCANNER: decrementing comment depth");
                        commentDepth--;
                        if (commentDepth >= 0)
                            continue;
                        else
                            commentDepth = 0;
                        // intentionally yield a mismatched comment end token.
                    }
                        
                    if (IsRelevant(t, yieldEOF) && commentDepth == 0)
                    {
                        if (Program.debug) Console.WriteLine("SCANNER: yield token " + t);
                        yield return t;
                    }
                    else
                    {
                        if (Program.debug) Console.WriteLine("SCANNER: ignore token " + t);
                    }
                }
                
            }

            if (Program.debug) Console.WriteLine("SCANNER: feeding EOF");
            automaton.FeedCharacter(TokenAutomaton.EOF);

            while ((t = automaton.GetToken()) != null)
            {
                if (t.tokenType == blockCommentStart)
                {
                    if (Program.debug) Console.WriteLine("SCANNER: incrementing comment depth");
                    commentDepth++;
                    continue;
                }
                if (t.tokenType == blockCommentEnd)
                {
                    if (Program.debug) Console.WriteLine("SCANNER: decrementing comment depth");
                    commentDepth--;
                    if (commentDepth >= 0)
                        continue;
                    else
                        commentDepth = 0;
                }

                if (IsRelevant(t, yieldEOF) && ((commentDepth == 0) || t.tokenType == TokenType.EOF))
                {
                    if (Program.debug) Console.WriteLine("SCANNER: yield token " + t);
                    yield return t;
                }
                else
                {
                    if (Program.debug) Console.WriteLine("SCANNER: ignore token " + t);
                }
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
