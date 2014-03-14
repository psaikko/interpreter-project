using InterpreterProject.LexicalAnalysis;
using InterpreterProject.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.SyntaxAnalysis
{
    public class ParseLeaf : IParseNode
    {
        public Terminal terminal;
        public Token token;

        public ParseLeaf(Terminal terminal) 
        {
            this.terminal = terminal;
        }

        public override string ToString()
        {
            return (token == null) ? terminal.ToString() : token.ToString();
        }

        public ISymbol GetSymbol()
        {
            return terminal;
        }
    }
}
