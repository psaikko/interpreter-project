using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.LexicalAnalysis
{
    // Implementation of a DFA, augmented with the ability to produce token objects
    // and tracking of position in source text
    public class TokenAutomaton
    {
        public const char EOF = '\0';

        private Node start;
        private Node position;

        private Queue<Token> tokenBuffer = new Queue<Token>();

        private Token lastToken = null;

        private string charBuffer = "";

        // variables to track location in the scanned text
        private int row = 0;
        private int col = 0;
        private int endRow = 0;
        private int endCol = 0;
        private int startRow = 0;
        private int startCol = 0;

        // resets the automaton to its initial state
        public void Reset()
        {
            row = 0;
            col = 0;
            endRow = 0;
            endCol = 0;
            startRow = 0;
            startCol = 0;
            tokenBuffer = new Queue<Token>();
            lastToken = null;
            position = start;
        }

        public TokenAutomaton(Node initNode)
        {
            this.start = initNode;
            this.position = initNode;
        }

        // remember the last valid token
        private void StoreToken(TokenType type)
        {
            lastToken = type.CreateToken(charBuffer, startRow, startCol);
            endRow = row;
            endCol = col;
        }

        // recall the last valid token
        private Token TakeToken()
        {
            Token t = lastToken;
            lastToken = null;
            row = endRow;
            col = endCol;
            startRow = endRow;
            startCol = endCol;
            return t;
        }

        // input a character into the automaton
        public void FeedCharacter(char c)
        {
            charBuffer += c;

            if (c == '\n')
            {
                row++; col = 0;
            }
            else
            {
                col++;
            }

            if (Program.debug) Console.WriteLine(String.Format("AUTOMATON accumulated: \"{0}\"", charBuffer));

            if (lastToken == null)
            {
                if (charBuffer[0] == EOF)
                    StoreToken(TokenType.EOF);
                else
                    StoreToken(TokenType.ERROR);
            }

            if (position.HasTransition(c))
            {
                // character can be a part of some token
                position = position.GetNext(c);
                if (position.IsAcceptingState())
                    StoreToken(position.acceptedTokenType);
            }
            else
            {
                // character cannot be a part of any token
                // so recognize the last seen valid token

                Token match = TakeToken();
                tokenBuffer.Enqueue(match);

                if (Program.debug) Console.WriteLine("AUTOMATON recognize token " + match);

                // and set the automaton state to the initial state
                position = start;
                string overflow = charBuffer.Remove(0, match.Lexeme.Length);
                charBuffer = "";

                // input characters into automaton that weren't a part of the token
                foreach (char ch in overflow)
                    FeedCharacter(ch);
            }
        }

        // gets a token from the buffer if one exists
        public Token GetToken()
        {
            if (tokenBuffer.Count > 0)
                return tokenBuffer.Dequeue();
            else
                return null;
        }

        // representation of a DFA node
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
