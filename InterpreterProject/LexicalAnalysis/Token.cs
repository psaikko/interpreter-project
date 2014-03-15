using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.LexicalAnalysis
{
    // Represents a some token
    // consists of lexeme, token type, and the position of the lexeme
    // in the scanned text
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
