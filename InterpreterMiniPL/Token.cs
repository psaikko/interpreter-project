using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterMiniPL
{
    class Token
    {
        public readonly TokenClass type;
        public readonly String lexeme;

        public Token(TokenClass type, String lexeme)
        {
            this.type = type;
            this.lexeme = lexeme;
        }
    }
}
