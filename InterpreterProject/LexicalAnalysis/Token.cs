using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.LexicalAnalysis
{
    public class Token
    {       
        public readonly TokenType tokenType;
        public readonly String lexeme;
        public readonly int row;
        public readonly int col;

        public Token(TokenType type, String lexeme, int row, int col)
        {
            this.tokenType = type;
            this.lexeme = lexeme;
            this.row = row;
            this.col = col;
        }

        public static void PrintList(List<Token> tokens)
        {
            foreach (Token t in tokens)
            {
                Console.WriteLine(t);
            }
        }

        public override string ToString()
        {
            return "{" + tokenType.name + " : \"" + lexeme + "\"} @"+row+","+col;
        }
    }
}
