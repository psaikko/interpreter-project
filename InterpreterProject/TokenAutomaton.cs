using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    public class TokenAutomaton
    {
        public const char EOF = '\0';

        private State initialState;
        private State currentState;

        private State lastAccepting;
        private Token lastToken;
        private String accumulator = "";

        private bool fail = false;

        public TokenAutomaton(State initialState)
        {
            this.initialState = initialState;
            this.currentState = initialState;
        }

        public void FeedCharacter(char c)
        {           
            /*
            if (lineComment != null && 
                c == lineComment[0] && 
                text.Length - i> lineComment.Length)
            {
                Console.WriteLine("Checking for line comment");
                if (text.Substring(i, lineComment.Length) == lineComment)
                {
                    Console.WriteLine("Line comment found");
                    int len = lineCommentLength(text, i, lineComment);
                    Console.WriteLine("Length " + len);

                    Token t = automaton.GetToken();
                    if (t != null)
                        tokens.Add(t);
                    automaton.Rewind();

                    i += len;
                    continue;
                }
            }

            if (blockCommentStart != null && 
                c == blockCommentStart[0] && 
                text.Length - i > blockCommentStart.Length)
            {
                if (text.Substring(i, lineComment.Length) == lineComment)
                {
                    i += blockCommentLength(text, i, blockCommentStart, blockCommentEnd);
                    continue;
                }
            }
             */

            if (!IsFailState())
            {
                accumulator += c;
                if (currentState.HasTransition(c))
                {                    
                    currentState = currentState.GetNext(c);

                    if (currentState.IsAcceptingState())
                    {
                        lastAccepting = currentState;
                        lastToken = currentState.GetTokenClass().CreateToken(accumulator);
                    }
                }
                else
                {
                    fail = true;
                    currentState = null;
                }
            }

            Console.WriteLine("acc: "+accumulator);
        }

        public bool IsFailState()
        {
            return fail;
        }

        public Token GetToken()
        {
            if (lastToken == null && accumulator == ""+EOF)
                return new Token(TokenType.EOF, accumulator);
            else
                return lastToken;
        }

        public Token GetErrorToken()
        {
            return new Token(TokenType.ERROR, "" + accumulator[0]);
        }

        public int Rewind()
        {
            int accLength = accumulator.Length;
            int tokenLength = lastToken == null ? 0 : lastToken.lexeme.Length;

            accumulator = "";
            lastToken = null;
            fail = false;
            currentState = initialState;

            return accLength - tokenLength;
        }

        public class State
        {
            public Dictionary<char, State> transitions = new Dictionary<char, State>();
            public List<TokenType> recognizedTokens = new List<TokenType>();

            public bool IsAcceptingState()
            {
                return (recognizedTokens.Count > 0);
            }

            public TokenType GetTokenClass()
            {
                if (IsAcceptingState())
                    return recognizedTokens[0];
                else
                    return null;
            }

            public State GetNext(char c)
            {
                return transitions[c];
            }

            public bool HasTransition(char c)
            {
                return transitions.ContainsKey(c);
            }
        }

        private int lineCommentLength(string text, int startIndex, string lineComment)
        {
            for (int i = startIndex; i < text.Length; i++)
            {
                Console.WriteLine(text[i]);
                if (text[i] == '\n' || text[i] == TokenAutomaton.EOF)
                    return i - startIndex;
            }
            return text.Length - startIndex;
        }

        private int blockCommentLength(string text, int startIndex,
            string blockCommentStart, string blockCommentEnd)
        {
            int count = 0;
            int i = startIndex;
            do
            {
                if (text.Length - i > blockCommentStart.Length &&
                    text.Substring(i, blockCommentStart.Length) == blockCommentStart)
                {
                    count++;
                    i += 2;
                }
                else if (text.Length - i > blockCommentEnd.Length &&
                    text.Substring(i, blockCommentEnd.Length) == blockCommentEnd)
                {
                    count--;
                    i += 2;
                }
                else if (count > 0)
                {
                    i++;
                }
            }
            while (count > 0 && i < text.Length);

            return i - startIndex;
        }
    }


}
