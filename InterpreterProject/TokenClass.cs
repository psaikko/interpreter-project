using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    public class TokenClass
    {
        public readonly String name;
        readonly Regex regex;

        public TokenClass(String name, Regex regex)
        {
            this.name = name;
            this.regex = regex;
            regex.DefineTokenClass(this);
        }

        public Token CreateToken(String lexeme)
        {
            return new Token(this, lexeme);
        }
    }
}
