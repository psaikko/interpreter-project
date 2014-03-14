using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterProject.LexicalAnalysis;

namespace InterpreterProject.SyntaxAnalysis
{
    public class Terminal : ISymbol
    {
        public static readonly Terminal EPSILON = new Terminal("");
        public static readonly Terminal EOF = new Terminal(TokenType.EOF);

        TokenType matchedTokenType = null;
        public TokenType MatchedTokenType
        {
            get { return matchedTokenType; }
        }

        string matchedString = null;
        public string MatchedString
        {
            get { return matchedString; }
        } 

        public Terminal(TokenType tokenType)
        {
            this.matchedTokenType = tokenType;
        }

        public Terminal(string lexeme)
        {
            this.matchedString = lexeme;
        }

        public bool Matches(Token t)
        {
            if (matchedTokenType != null)
                return t.Type == matchedTokenType;
            else
                return t.Lexeme == matchedString;
        }

        public override string ToString()
        {
            if (matchedTokenType != null)
                return "<" + matchedTokenType.Name + ">";
            else
                return "\"" + matchedString + "\"";
        }
    }
}
