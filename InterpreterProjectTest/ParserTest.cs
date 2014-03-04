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
            Dictionary<string, CFG.Variable> vars = miniPL.GetGrammarNonterminals();
            Dictionary<string, CFG.Terminal> terms = miniPL.GetGrammarTerminals();

            ISet<CFG.Terminal> firstSet = grammar.First(vars["program"]);
            Assert.IsTrue(firstSet.SetEquals(new CFG.Terminal[] { 
                terms["var"], terms["identifier"], terms["for"], terms["read"], terms["print"], terms["assert"]}));

            firstSet = grammar.First(vars["statements"]);
            Assert.IsTrue(firstSet.SetEquals(new CFG.Terminal[] { 
                terms["var"], terms["identifier"], terms["for"], terms["read"], terms["print"], terms["assert"] }));

            firstSet = grammar.First(vars["statements_head"]);
            Assert.IsTrue(firstSet.SetEquals(new CFG.Terminal[] { 
                terms["var"], terms["identifier"], terms["for"], terms["read"], terms["print"], terms["assert"] }));

            firstSet = grammar.First(vars["statements_tail"]);
            Assert.IsTrue(firstSet.SetEquals(new CFG.Terminal[] { 
                terms["var"], terms["identifier"], terms["for"], terms["read"], terms["print"], terms["assert"], CFG.Terminal.epsilon }));

            firstSet = grammar.First(vars["statement"]);
            Assert.IsTrue(firstSet.SetEquals(new CFG.Terminal[] { 
                terms["var"], terms["identifier"], terms["for"], terms["read"], terms["print"], terms["assert"] }));

            firstSet = grammar.First(vars["declaration"]);
            Assert.IsTrue(firstSet.SetEquals(new CFG.Terminal[] { 
                terms["var"] }));

            firstSet = grammar.First(vars["declaration_assignment"]);
            Assert.IsTrue(firstSet.SetEquals(new CFG.Terminal[] { 
                terms[":="], CFG.Terminal.epsilon }));

            firstSet = grammar.First(vars["expression"]);
            Assert.IsTrue(firstSet.SetEquals(new CFG.Terminal[] { 
                terms["unary_operator"], terms["integer"], terms["string"], terms["identifier"], terms["("] }));

            firstSet = grammar.First(vars["unary_operation"]);
            Assert.IsTrue(firstSet.SetEquals(new CFG.Terminal[] { 
                terms["unary_operator"] }));

            firstSet = grammar.First(vars["binary_operation"]);
            Assert.IsTrue(firstSet.SetEquals(new CFG.Terminal[] { 
                terms["binary_operator"], CFG.Terminal.epsilon }));

            firstSet = grammar.First(vars["operand"]);
            Assert.IsTrue(firstSet.SetEquals(new CFG.Terminal[] { 
                terms["integer"], terms["string"], terms["identifier"], terms["("] }));
        }

        [TestMethod]
        public void Parser_MiniPL_LL1FollowSetsTest()
        {
            ILanguage miniPL = MiniPL.GetInstance();
            CFG grammar = miniPL.GetGrammar();
            Dictionary<string, CFG.Variable> vars = miniPL.GetGrammarNonterminals();
            Dictionary<string, CFG.Terminal> terms = miniPL.GetGrammarTerminals();

            foreach (CFG.Variable var in vars.Values)
            {
                Console.WriteLine(var);
                ISet<CFG.Terminal> followSet = grammar.Follow(var);

                foreach (CFG.Terminal t in followSet) Console.Write(t + " ");
                Console.WriteLine("\n");
            }

            Assert.Fail();
        }
    }
}
