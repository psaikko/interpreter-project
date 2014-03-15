using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterpreterProject.LexicalAnalysis;
using InterpreterProject.SyntaxAnalysis;

namespace InterpreterProject.Errors
{
    // Interface for different errors that can occur during interpretation process
    public abstract class Error
    {
        protected Token t;

        protected Error(Token t) { this.t = t; }

        abstract public string GetMessage();
    }
}
