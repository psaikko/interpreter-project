using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    public class Token
    {       
        public readonly TokenType tokenType;
        public readonly String lexeme;

        public Token(TokenType type, String lexeme)
        {
            this.tokenType = type;
            this.lexeme = lexeme;
        }

        public static void PrintList(List<Token> tokens)
        {
            foreach (Token t in tokens)
            {
                Console.WriteLine(string.Format("Type: {0,-10} Lexeme: '{1}'", t.tokenType.name, t.lexeme));
            }
        }
    }
}
