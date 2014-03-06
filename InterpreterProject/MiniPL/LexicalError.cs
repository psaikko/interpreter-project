using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    public class LexicalError : IError
    {
        public Token t;

        public LexicalError(Token t)
        {
            this.t = t;
        }
    }
}
