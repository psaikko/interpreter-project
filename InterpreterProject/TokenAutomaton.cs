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

        private Node initialNode;
        private Node currentNode;

        private Node lastAccepting;
        private Token lastToken;
        private String accumulator = "";

        private bool fail = false;

        public TokenAutomaton(Node initNode)
        {
            this.initialNode = initNode;
            this.currentNode = initNode;
        }

        public void FeedCharacter(char c)
        {
            if (!IsFailState())
            {
                accumulator += c;
                if (CanTransitionWith(c))
                {
                    TransitionWith(c);
                    UpdateToken();                  
                }
                else
                {
                    SetFailState();
                }
            }

            Console.WriteLine("AUTOMATON accumulated: " + accumulator);
        }

        public bool CanTransitionWith(char c)
        {
            return currentNode.HasTransition(c);
        }

        public void TransitionWith(char c)
        {
            currentNode = currentNode.GetNext(c);
        }

        public void UpdateToken()
        {
            if (currentNode.IsAcceptingState())
            {
                lastAccepting = currentNode;
                lastToken = currentNode.recognizedTokenType.CreateToken(accumulator);
            }
        }

        public void SetFailState()
        {
            fail = true;
            currentNode = null;
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
            currentNode = initialNode;

            return accLength - tokenLength;
        }

        public class Node
        {
            public Dictionary<char, Node> transitions = new Dictionary<char, Node>();
            public TokenType recognizedTokenType = null;

            public bool IsAcceptingState()
            {
                return recognizedTokenType != null;
            }

            public Node GetNext(char c)
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
