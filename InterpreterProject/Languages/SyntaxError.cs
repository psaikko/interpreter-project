using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterProject.LexicalAnalysis;
using InterpreterProject.SyntaxAnalysis;

namespace InterpreterProject.Languages
{
    public class SyntaxError : IError
    {
        public Terminal actual;
        public List<Terminal> expected;

        public SyntaxError(Terminal term)
        {
            this.actual = term;
        }

        public string GetMessage()
        {
            throw new NotImplementedException();
        }
    }
}
