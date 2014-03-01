using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    public class TokenClass
    {
        public static readonly TokenClass ERROR = new TokenClass("ERROR", Regex.None());
        public static readonly TokenClass EOF = new TokenClass("EOF", Regex.Character(DFA.EOF));

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
