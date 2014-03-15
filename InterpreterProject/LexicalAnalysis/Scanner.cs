using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.LexicalAnalysis
{
    // Implements a scanner that tokenizes some Stream of text characters
    public class Scanner
    {
        TokenAutomaton automaton;
        TokenType blockCommentStart;
        TokenType blockCommentEnd;

        // Define the scanner with a TokenAutomaton (combined DFA of token types) and
        // optional special token types to recognize as block comment start and end points
        // so that nested comments can be handled
        public Scanner(TokenAutomaton automaton, TokenType blockCommentStart = null, TokenType blockCommentEnd = null)
        {
            this.automaton = automaton;
            this.blockCommentStart = blockCommentStart;
            this.blockCommentEnd = blockCommentEnd;
        }

        // tokenizes an input stream.
        // implemented as a generator so that lexical and syntax analysis can be done in a single pass
        public IEnumerable<Token> Tokenize(Stream input, bool yieldEOF = true)
        {
            automaton.Reset();
            StreamReader reader = new StreamReader(input);
            Token t;
            int commentDepth = 0;

            while (!reader.EndOfStream)
            {
                // feed a character and try to get a token from the automaton
                char c = (char)reader.Read();
                if (Program.debug) Console.WriteLine(String.Format("SCANNER: feeding '{0}'", c));
                automaton.FeedCharacter(c);
                t = automaton.GetToken();
                if (t != null)
                {
                    // keep track of nested comment depth
                    if (t.Type == blockCommentStart)
                    {
                        if (Program.debug) Console.WriteLine("SCANNER: incrementing comment depth");
                        commentDepth++;
                        continue;
                    }
                    if (t.Type == blockCommentEnd)
                    {
                        if (Program.debug) Console.WriteLine("SCANNER: decrementing comment depth");
                        commentDepth--;
                        if (commentDepth >= 0)
                            continue;
                        else
                            commentDepth = 0;
                    }

                    // yield relevant tokens
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

            // feed automaton an EOF character
            if (Program.debug) Console.WriteLine("SCANNER: feeding EOF");
            automaton.FeedCharacter(TokenAutomaton.EOF);

            // and get the remaining tokens
            while ((t = automaton.GetToken()) != null)
            {
                if (t.Type == blockCommentStart)
                {
                    if (Program.debug) Console.WriteLine("SCANNER: incrementing comment depth");
                    commentDepth++;
                    continue;
                }
                if (t.Type == blockCommentEnd)
                {
                    if (Program.debug) Console.WriteLine("SCANNER: decrementing comment depth");
                    commentDepth--;
                    if (commentDepth >= 0)
                        continue;
                    else
                        commentDepth = 0;
                }

                if (IsRelevant(t, yieldEOF) && ((commentDepth == 0) || t.Type == TokenType.EOF))
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

        // just checks if a token should be yielded by the generator
        private bool IsRelevant(Token t, bool EOFisRelevant)
        {
            return (t.Type.TokenPriority != TokenType.Priority.Whitespace && (t.Type != TokenType.EOF || EOFisRelevant));
        }

        // helper method to scan a string instead of a stream, used for testing
        public IEnumerable<Token> Tokenize(string text, bool yieldEOF = true)
        {
            MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(text));
            return Tokenize(ms, yieldEOF);
        }
    }
}
