using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.LexicalAnalysis
{
    public class Token
    {
        TokenType type;
        public TokenType Type
        {
            get { return type; }
        }

        String lexeme;
        public String Lexeme
        {
            get { return lexeme; }
        }

        Position textPosition;
        public Position TextPosition
        {
            get { return textPosition; }
        } 

        public Token(TokenType type, String lexeme, int row, int col)
        {
            this.type = type;
            this.lexeme = lexeme;
            this.textPosition = new Position(row, col);
        }

        public override string ToString()
        {
            return "{" + type.Name + " : \"" + lexeme + "\"} @" + textPosition;
        }
    }
}
