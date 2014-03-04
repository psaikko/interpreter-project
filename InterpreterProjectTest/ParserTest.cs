﻿using System;
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

            ISet<CFG.Terminal> followSet = grammar.Follow(vars["program"]);
            Assert.IsTrue(followSet.SetEquals(new CFG.Terminal[] { 
                CFG.Terminal.EOF } ));

            followSet = grammar.Follow(vars["statements"]);
            Assert.IsTrue(followSet.SetEquals(new CFG.Terminal[] { 
                terms["end"], CFG.Terminal.EOF }));

            followSet = grammar.Follow(vars["statements_head"]);
            Assert.IsTrue(followSet.SetEquals(new CFG.Terminal[] { 
                terms["end"], CFG.Terminal.EOF, terms["var"], terms["assert"], terms["read"], terms["print"], terms["for"], terms["identifier"]}));

            followSet = grammar.Follow(vars["statements_tail"]);
            Assert.IsTrue(followSet.SetEquals(new CFG.Terminal[] { 
                terms["end"], CFG.Terminal.EOF}));

            followSet = grammar.Follow(vars["statement"]);
            Assert.IsTrue(followSet.SetEquals(new CFG.Terminal[] { 
                terms[";"]}));

            followSet = grammar.Follow(vars["declaration"]);
            Assert.IsTrue(followSet.SetEquals(new CFG.Terminal[] { 
                terms[";"]}));

            followSet = grammar.Follow(vars["declaration_assignment"]);
            Assert.IsTrue(followSet.SetEquals(new CFG.Terminal[] { 
                terms[";"]}));

            followSet = grammar.Follow(vars["expression"]);
            Assert.IsTrue(followSet.SetEquals(new CFG.Terminal[] { 
                terms[".."], terms["do"], terms[")"], terms[";"]}));

            followSet = grammar.Follow(vars["unary_operation"]);
            Assert.IsTrue(followSet.SetEquals(new CFG.Terminal[] { 
                terms[".."], terms["do"], terms[")"], terms[";"] }));

            followSet = grammar.Follow(vars["binary_operation"]);
            Assert.IsTrue(followSet.SetEquals(new CFG.Terminal[] { 
                terms[".."], terms["do"], terms[")"], terms[";"] }));

            followSet = grammar.Follow(vars["operand"]);
            Assert.IsTrue(followSet.SetEquals(new CFG.Terminal[] { 
                terms[".."], terms["do"], terms[")"], terms[";"], terms["binary_operator"] }));           
        }

        [TestMethod]
        public void Parser_MiniPLTest1()
        {
            string text = "var X : int := 4 + (6 * 2);\n" +
                          "print X;";

            ILanguage miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();

            Assert.IsTrue(ps.Parse(sc.Tokenize(text)));
        }

        [TestMethod]
        public void Parser_MiniPLTest2()
        {
            string text = "var nTimes : int := 0;\n" +
                          "print \"How many times?\";\n" +
                          "read nTimes;\n" +
                          "var x : int;\n" +
                          "for x in 0..nTimes-1 do\n" +
                          "     print x;\n" +
                          "     print \" : Hello, World!\n\";\n" +
                          "end for;\n" +
                          "assert (x = nTimes);";

            ILanguage miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();

            Assert.IsTrue(ps.Parse(sc.Tokenize(text)));
        }

        [TestMethod]
        public void Parser_MiniPLTest3()
        {
            string text = "print \"Give a number\";\n" +
                          "var n : int;\n" +
                          "read n;\n" +
                          "var f : int := 1;\n" +
                          "var i : int;\n" +
                          "for i in 1..n do\n" +
                          "    f := f * i;\n" +
                          "end for;\n" +
                          "print \"The result is: \";\n" +
                          "print f;";

            ILanguage miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();

            Assert.IsTrue(ps.Parse(sc.Tokenize(text)));
        }
    }
}