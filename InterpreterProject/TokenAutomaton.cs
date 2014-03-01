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
            if (!IsFailState())
            {
                accumulator += c;
                if (currentState.HasTransition(c))
                {                    
                    currentState = currentState.GetNext(c);

                    if (currentState.IsAcceptingState())
                    {
                        lastAccepting = currentState;
                        lastToken = currentState.recognizedTokenType.CreateToken(accumulator);
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
            public TokenType recognizedTokenType = null;

            public bool IsAcceptingState()
            {
                return (recognizedTokenType != null);
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
    }


}
