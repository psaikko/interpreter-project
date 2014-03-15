using InterpreterProject.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.SyntaxAnalysis
{
    // interface for parse tree nodes
    public interface IParseNode
    {
        ISymbol GetSymbol();
    }
}
