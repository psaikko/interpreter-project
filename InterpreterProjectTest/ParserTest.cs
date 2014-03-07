﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using InterpreterProject.LexicalAnalysis;
using InterpreterProject.SyntaxAnalysis;
using InterpreterProject.Languages;

namespace InterpreterProjectTest
{
    [TestClass]
    public class ParserTest
    {
        [TestMethod]
        public void Parser_MiniPL_LL1ParseTableGenerationTest()
        {
            CFG grammar = MiniPL.GetInstance().GetGrammar();
            ParseTable parseTable = grammar.CreateLL1ParseTable();
            Assert.AreNotEqual(null, parseTable);
        }

        [TestMethod]
        public void Parser_MiniPL_LL1FirstSetsTest()
        {
            MiniPL miniPL = MiniPL.GetInstance();
            CFG grammar = miniPL.GetGrammar();
            Dictionary<string, Nonterminal> vars = miniPL.GetGrammarNonterminals();
            Dictionary<string, Terminal> terms = miniPL.GetGrammarTerminals();

            ISet<Terminal> firstSet = grammar.First(vars["program"]);
            Assert.IsTrue(firstSet.SetEquals(new Terminal[] { 
                terms["var"], terms["identifier"], terms["for"], terms["read"], terms["print"], terms["assert"]}));

            firstSet = grammar.First(vars["statements"]);
            Assert.IsTrue(firstSet.SetEquals(new Terminal[] { 
                terms["var"], terms["identifier"], terms["for"], terms["read"], terms["print"], terms["assert"] }));

            firstSet = grammar.First(vars["statements_head"]);
            Assert.IsTrue(firstSet.SetEquals(new Terminal[] { 
                terms["var"], terms["identifier"], terms["for"], terms["read"], terms["print"], terms["assert"] }));

            firstSet = grammar.First(vars["statements_tail"]);
            Assert.IsTrue(firstSet.SetEquals(new Terminal[] { 
                terms["var"], terms["identifier"], terms["for"], terms["read"], terms["print"], terms["assert"], Terminal.EPSILON }));

            firstSet = grammar.First(vars["statement"]);
            Assert.IsTrue(firstSet.SetEquals(new Terminal[] { 
                terms["var"], terms["identifier"], terms["for"], terms["read"], terms["print"], terms["assert"] }));

            firstSet = grammar.First(vars["declaration"]);
            Assert.IsTrue(firstSet.SetEquals(new Terminal[] { 
                terms["var"] }));

            firstSet = grammar.First(vars["declaration_assignment"]);
            Assert.IsTrue(firstSet.SetEquals(new Terminal[] { 
                terms[":="], Terminal.EPSILON }));

            firstSet = grammar.First(vars["expression"]);
            Assert.IsTrue(firstSet.SetEquals(new Terminal[] { 
                terms["unary_operator"], terms["integer"], terms["string"], terms["identifier"], terms["("] }));

            firstSet = grammar.First(vars["unary_operation"]);
            Assert.IsTrue(firstSet.SetEquals(new Terminal[] { 
                terms["unary_operator"] }));

            firstSet = grammar.First(vars["binary_operation"]);
            Assert.IsTrue(firstSet.SetEquals(new Terminal[] { 
                terms["binary_operator"], Terminal.EPSILON }));

            firstSet = grammar.First(vars["operand"]);
            Assert.IsTrue(firstSet.SetEquals(new Terminal[] { 
                terms["integer"], terms["string"], terms["identifier"], terms["("] }));
        }

        [TestMethod]
        public void Parser_MiniPL_LL1FollowSetsTest()
        {
            MiniPL miniPL = MiniPL.GetInstance();
            CFG grammar = miniPL.GetGrammar();
            Dictionary<string, Nonterminal> vars = miniPL.GetGrammarNonterminals();
            Dictionary<string, Terminal> terms = miniPL.GetGrammarTerminals();

            ISet<Terminal> followSet = grammar.Follow(vars["program"]);
            Assert.IsTrue(followSet.SetEquals(new Terminal[] { 
                Terminal.EOF } ));

            followSet = grammar.Follow(vars["statements"]);
            Assert.IsTrue(followSet.SetEquals(new Terminal[] { 
                terms["end"], Terminal.EOF }));

            followSet = grammar.Follow(vars["statements_head"]);
            Assert.IsTrue(followSet.SetEquals(new Terminal[] { 
                terms["end"], Terminal.EOF, terms["var"], terms["assert"], terms["read"], terms["print"], terms["for"], terms["identifier"]}));

            followSet = grammar.Follow(vars["statements_tail"]);
            Assert.IsTrue(followSet.SetEquals(new Terminal[] { 
                terms["end"], Terminal.EOF}));

            followSet = grammar.Follow(vars["statement"]);
            Assert.IsTrue(followSet.SetEquals(new Terminal[] { 
                terms[";"]}));

            followSet = grammar.Follow(vars["declaration"]);
            Assert.IsTrue(followSet.SetEquals(new Terminal[] { 
                terms[";"]}));

            followSet = grammar.Follow(vars["declaration_assignment"]);
            Assert.IsTrue(followSet.SetEquals(new Terminal[] { 
                terms[";"]}));

            followSet = grammar.Follow(vars["expression"]);
            Assert.IsTrue(followSet.SetEquals(new Terminal[] { 
                terms[".."], terms["do"], terms[")"], terms[";"]}));

            followSet = grammar.Follow(vars["unary_operation"]);
            Assert.IsTrue(followSet.SetEquals(new Terminal[] { 
                terms[".."], terms["do"], terms[")"], terms[";"] }));

            followSet = grammar.Follow(vars["binary_operation"]);
            Assert.IsTrue(followSet.SetEquals(new Terminal[] { 
                terms[".."], terms["do"], terms[")"], terms[";"] }));

            followSet = grammar.Follow(vars["operand"]);
            Assert.IsTrue(followSet.SetEquals(new Terminal[] { 
                terms[".."], terms["do"], terms[")"], terms[";"], terms["binary_operator"] }));           
        }

        [TestMethod]
        public void Parser_ParseTreeTest()
        {
            string text = "var X : int := 4 + (6 * 2);\n" +
                          "print X;";

            MiniPL miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();
            Parser.ParseTree ptree = ps.Parse(sc.Tokenize(text));         

            // compare contents of tree to hand-drawn parse tree

            Assert.AreNotEqual(null, ptree);
            // 16 "real terminals", 2 epsilon
            Assert.AreEqual(18, ptree.SymbolCount(s => s is Terminal));
            Assert.AreEqual(2, ptree.SymbolCount(s => (s is Terminal) && (s as Terminal) == Terminal.EPSILON));
            Assert.AreEqual(3, ptree.SymbolCount(s => (s is Terminal) && (s as Terminal).tokenType == miniPL.GetTokenTypes()["integer"]));
            Assert.AreEqual(2, ptree.SymbolCount(s => (s is Terminal) && (s as Terminal).tokenType == miniPL.GetTokenTypes()["binary_op"]));
            Assert.AreEqual(21, ptree.SymbolCount(s => s is Nonterminal));
            // two actual binary operations, one -> epsilon
            Assert.AreEqual(3, ptree.SymbolCount(s => (s is Nonterminal) && (s as Nonterminal) == miniPL.GetGrammarNonterminals()["binary_operation"]));
            Assert.AreEqual(2, ptree.SymbolCount(s => (s is Nonterminal) && (s as Nonterminal) == miniPL.GetGrammarNonterminals()["statement"]));
            Assert.IsTrue(ptree.DepthContains(12, s => (s is Terminal) && (s as Terminal).tokenType == miniPL.GetTokenTypes()["integer"]));
            Assert.IsTrue(ptree.DepthContains(11, s => (s is Terminal) && (s as Terminal).tokenType == miniPL.GetTokenTypes()["integer"]));
            Assert.IsTrue(ptree.DepthContains(8, s => (s is Terminal) && (s as Terminal).tokenType == miniPL.GetTokenTypes()["integer"]));
        }

        [TestMethod]
        public void Parser_MiniPLTest1()
        {
            string text = "var X : int := 4 + (6 * 2);\n" +
                          "print X;";

            MiniPL miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();

            Parser.ParseTree ptree = ps.Parse(sc.Tokenize(text));          
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

            MiniPL miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();

            Assert.AreNotEqual(null, ps.Parse(sc.Tokenize(text)));

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

            MiniPL miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();

            Assert.AreNotEqual(null, ps.Parse(sc.Tokenize(text)));
        }

        [TestMethod]
        public void Parser_LexicalErrorTest()
        {
            // check that the parser can disregard error tokens and log them as errors

            string text = "]var X :[ int := 4 +' (6 * 2);\n" +
                          "print X###;";

            MiniPL miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();
            Parser.ParseTree ptree = ps.Parse(sc.Tokenize(text));

            Assert.AreEqual(6, ptree.errors.Count);

            Assert.AreNotEqual(null, ptree);
            Assert.AreEqual(18, ptree.SymbolCount(s => s is Terminal));
            Assert.AreEqual(2, ptree.SymbolCount(s => (s is Terminal) && (s as Terminal) == Terminal.EPSILON));
            Assert.AreEqual(3, ptree.SymbolCount(s => (s is Terminal) && (s as Terminal).tokenType == miniPL.GetTokenTypes()["integer"]));
            Assert.AreEqual(2, ptree.SymbolCount(s => (s is Terminal) && (s as Terminal).tokenType == miniPL.GetTokenTypes()["binary_op"]));
            Assert.AreEqual(21, ptree.SymbolCount(s => s is Nonterminal));
            Assert.AreEqual(3, ptree.SymbolCount(s => (s is Nonterminal) && (s as Nonterminal) == miniPL.GetGrammarNonterminals()["binary_operation"]));
            Assert.AreEqual(2, ptree.SymbolCount(s => (s is Nonterminal) && (s as Nonterminal) == miniPL.GetGrammarNonterminals()["statement"]));
            Assert.IsTrue(ptree.DepthContains(12, s => (s is Terminal) && (s as Terminal).tokenType == miniPL.GetTokenTypes()["integer"]));
            Assert.IsTrue(ptree.DepthContains(11, s => (s is Terminal) && (s as Terminal).tokenType == miniPL.GetTokenTypes()["integer"]));
            Assert.IsTrue(ptree.DepthContains(8, s => (s is Terminal) && (s as Terminal).tokenType == miniPL.GetTokenTypes()["integer"]));
        }

        [TestMethod]
        public void Parser_SyntaxErrorTest()
        {

        }
    }
}
