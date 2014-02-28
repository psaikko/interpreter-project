using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterMiniPL
{
    class TokenClass
    {
        public readonly String name;
        readonly Re regex;

        public TokenClass(String name, Re regex)
        {
            this.name = name;
            this.regex = regex;
            regex.defineTokenClass(this);
        }

        public Token CreateToken(String lexeme)
        {
            return new Token(this, lexeme);
        }
    }
}
