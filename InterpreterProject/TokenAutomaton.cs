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
        private ICollection<IState> nodeStates = new List<IState>();

        public TokenAutomaton(Node initNode, ICollection<IState> nodeStates = null)
        {
            this.initialNode = initNode;
            this.currentNode = initNode;
            if (nodeStates != null)
                this.nodeStates = nodeStates;
        }

        public void FeedCharacter(char c)
        {
            if (!IsFailState())
            {
                accumulator += c;
                if (CanTransitionWith(c))
                {
                    TransitionWith(c);
                    currentNode.Visit();
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

            foreach (IState state in nodeStates)
                state.Reset();

            return accLength - tokenLength;
        }

        public class Node
        {
            public Dictionary<char, Node> transitions = new Dictionary<char, Node>();
            public TokenType recognizedTokenType = null;
            public List<IFunction> functions = new List<IFunction>();
            public IState state = null;

            public bool IsAcceptingState()
            {
                if (state == null)
                    return recognizedTokenType != null;
                else
                    return state.Check() && recognizedTokenType != null;
            }

            public void Visit()
            {
                foreach (IFunction f in functions)
                    f.Call();
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
