using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InterpreterProject;
using System.Collections.Generic;

namespace InterpreterProjectTest
{
    [TestClass]
    public class ParserTest
    {
        [TestMethod]
        public void Parser_MiniPL_LL1ParseTableGenerationTest()
        {
            CFG grammar = MiniPL.GetInstance().GetGrammar();
            Dictionary<CFG.Variable, Dictionary<CFG.Terminal, CFG.ISymbol[]>> parseTable =
                grammar.CreateLL1ParseTable();
            Assert.AreNotEqual(null, parseTable);
        }

        [TestMethod]
        public void Parser_MiniPL_LL1FirstSetsTest()
        {
            ILanguage miniPL = MiniPL.GetInstance();
            CFG grammar = miniPL.GetGrammar();
            ISet<CFG.Terminal> firstSet = grammar.First(grammar.GetProductionRules(miniPL.GetGrammarNonterminals()["program"])[0]);

            foreach (CFG.Terminal t in firstSet)
                Console.WriteLine(t);

            Assert.Fail();
        }

        [TestMethod]
        public void Parser_MiniPL_LL1FollowSetsTest()
        {
            Assert.Fail();
        }
    }
}
