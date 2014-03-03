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
            CFG miniPLGrammar = MiniPL.GetInstance().GetGrammar();
            Dictionary<CFG.Variable, Dictionary<CFG.Terminal, CFG.ISymbol[]>> parseTable =
                miniPLGrammar.CreateLL1ParseTable();
            Assert.AreNotEqual(null, parseTable);
        }

        [TestMethod]
        public void Parser_MiniPL_LL1FirstSetsTest()
        {

        }

        [TestMethod]
        public void Parser_MiniPL_LL1FollowSetsTest()
        {

        }
    }
}
