using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    public class DFA
    {
        public const char EOF = '\0';

        private State initialState;
        private State currentState;

        private State lastAccepting;
        private Token lastToken;
        private String accumulator = "";

        private bool fail = false;

        public DFA(State initialState)
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
            //Console.Write("waiting.. ");
            //Console.ReadLine();
        }

        public bool IsFailState()
        {
            return fail;
        }

        public Token GetToken()
        {
            if (lastToken == null && accumulator == ""+EOF)
                return new Token(TokenClass.EOF, accumulator);
            else
                return lastToken;
        }

        public Token GetErrorToken()
        {
            return new Token(TokenClass.ERROR, "" + accumulator[0]);
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
            public List<TokenClass> recognizedTokens = new List<TokenClass>();

            public bool IsAcceptingState()
            {
                return (recognizedTokens.Count > 0);
            }

            public TokenClass GetTokenClass()
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
    }


}
