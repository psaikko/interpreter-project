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

        Dictionary<string, Terminal> GetGrammarTerminals();

        Dictionary<string, Nonterminal> GetGrammarNonterminals();

        CFG GetGrammar();

        Scanner GetScanner();

        Parser GetParser();
    }
}
