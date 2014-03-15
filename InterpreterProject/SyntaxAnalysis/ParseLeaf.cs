using InterpreterProject.LexicalAnalysis;
using InterpreterProject.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.SyntaxAnalysis
{
    // Implementation of a parse tree leaf node
    public class ParseLeaf : IParseNode
    {
        private Terminal terminal;
        public Terminal Terminal
        {
            get { return terminal; }
        }

        private Token token;
        public Token Token
        {
            get { return token; }
            set { this.token = value; }
        }

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
