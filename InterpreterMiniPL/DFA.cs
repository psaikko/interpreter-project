using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    public class DFA
    {
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
                if (currentState.HasTransition(c))
                {
                    accumulator += c;
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
            //Console.WriteLine(accumulator);
        }

        public bool IsFailState()
        {
            return fail;
        }

        public Token GetToken()
        {
            return lastToken;
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
