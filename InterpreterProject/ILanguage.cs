using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    public interface ILanguage
    {
        Dictionary<string, TokenType> GetTokenTypes();

        Dictionary<string, CFG.ISymbol> GetGrammarTerminals();

        Dictionary<string, CFG.ISymbol> GetGrammarNonterminals();

        CFG GetGrammar();

        Scanner GetScanner();

        Parser GetParser();
    }
}
