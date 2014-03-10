using InterpreterProject.LexicalAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.Errors
{
    public class RuntimeError : IError
    {
        public Token t;
        public string description;

        public RuntimeError(Token t, string description)
        {
            this.t = t;
            this.description = description;
        }

        public string GetMessage()
        {
            return String.Format("Runtime error: {0} at {1}", description, t.pos);
        }
    }
}
