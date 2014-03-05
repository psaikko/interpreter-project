﻿using System;
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
        private int row = 0;
        private int col = 0;
        private int lastRow = 0;
        private int lastCol = 0;

        public TokenAutomaton(Node initNode)
        {
            this.start = initNode;
            this.position = initNode;
        }

        private void StoreToken(TokenType type)
        {
            lastToken = type.CreateToken(charBuffer, row, col);
            lastRow = row;
            lastCol = col;
        }


        private Token TakeToken()
        {
            Token t = lastToken;
            lastToken = null;
            row = lastRow;
            col = lastCol;
            return t;
        }

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

            Console.WriteLine(String.Format("AUTOMATON accumulated: \"{0}\"", charBuffer));

            if (lastToken == null)
            {
                if (charBuffer[0] == EOF)
                    StoreToken(TokenType.EOF);
                else
                    StoreToken(TokenType.ERROR);
            }    

            if (position.HasTransition(c))
            {
                position = position.GetNext(c);
                if (position.IsAcceptingState())
                {
                    StoreToken(position.acceptedTokenType);
                }           
            }
            else
            {
                Token match = TakeToken();
                tokenBuffer.Enqueue(match);

                Console.WriteLine("AUTOMATON recognize token " + match);

                position = start;
                string overflow = charBuffer.Remove(0, match.lexeme.Length);
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
