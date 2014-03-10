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

        public readonly TokenType tokenType = null;
        public readonly string matchedString = null;

        public Terminal(TokenType tokenType)
        {
            this.tokenType = tokenType;
        }

        public Terminal(string lexeme)
        {
            this.matchedString = lexeme;
        }

        public bool Matches(Token t)
        {
            if (tokenType != null)
                return t.tokenType == tokenType;
            else
                return t.lexeme == matchedString;
        }

        public override string ToString()
        {
            if (tokenType != null)
                return "<" + tokenType.name + ">";
            else
                return "\"" + matchedString + "\"";
        }
    }
}
