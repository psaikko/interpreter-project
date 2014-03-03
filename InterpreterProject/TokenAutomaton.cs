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

        private Node start;
        private Node position;

        private Queue<Token> tokenBuffer = new Queue<Token>();

        private Token lastToken = null;
        private string charBuffer = "";

        private bool fail = false;

        public TokenAutomaton(Node initNode)
        {
            this.start = initNode;
            this.position = initNode;
        }

        public void FeedCharacter(char c)
        {
            charBuffer += c;
            Console.WriteLine("AUTOMATON accumulated: " + charBuffer);

            if (lastToken == null)
            {
                if (charBuffer[0] == EOF)
                    lastToken = TokenType.EOF.CreateToken(charBuffer);
                else
                    lastToken = TokenType.ERROR.CreateToken(charBuffer);
            }    

            if (position.HasTransition(c))
            {
                position = position.GetNext(c);
                if (position.IsAcceptingState())
                {
                    lastToken = position.acceptedTokenType.CreateToken(charBuffer);
                }           
            }
            else
            {
                int tokenLength = lastToken.lexeme.Length;
                tokenBuffer.Enqueue(lastToken);

                Console.WriteLine("AUTOMATON recognize token, type: <" + lastToken.tokenType.name + "> lexeme: <" + lastToken.lexeme + ">");

                position = start;
                lastToken = null;
                string overflow = charBuffer.Remove(0, tokenLength);
                charBuffer = "";
                foreach (char ch in overflow)
                    FeedCharacter(ch);
            }
        }

        public Token GetToken()
        {
            if (tokenBuffer.Count > 0)
                return tokenBuffer.Dequeue();
            else
                return null;
        }
         
        public class Node
        {
            public Dictionary<char, Node> transitions = new Dictionary<char, Node>();
            public TokenType acceptedTokenType = null;

            public bool IsAcceptingState()
            {
                return acceptedTokenType != null;
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
