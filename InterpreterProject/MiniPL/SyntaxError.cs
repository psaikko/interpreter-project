using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    public class SyntaxError : IError
    {
        public Terminal actual;
        public List<Terminal> expected;

        public SyntaxError(Terminal term)
        {
            this.actual = term;
        }
    }
}
