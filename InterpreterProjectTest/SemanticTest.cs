using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InterpreterProject.LexicalAnalysis;
using InterpreterProject.SyntaxAnalysis;
using InterpreterProject.Languages;
using InterpreterProject.Errors;
using InterpreterProject;
using System.Collections.Generic;

namespace InterpreterProjectTest
{
    [TestClass]
    public class SemanticTest
    {
        [TestMethod]
        public void Semantics_DeclarationScanTest()
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
            Tree<Parser.IParseValue> ptree = ps.Parse(sc.Tokenize(text));

            MiniPL.Runnable prog = miniPL.ProcessParseTree(ptree, ps.GetErrors());

            Assert.AreEqual(3, prog.declarations.Count);
        }

        [TestMethod]
        public void Semantics_MultipleDeclarationTest()
        {
            string text = "var n : int;\n" +
                          "var n : bool;\n" +
                          "var n : int := 0;\n";

            MiniPL miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();

            List<Token> tokens = new List<Token>();
            foreach (Token t in sc.Tokenize(text))
            {
                tokens.Add(t);
                Console.WriteLine(t);
            }

            Tree<Parser.IParseValue> ptree = ps.Parse(tokens);

            MiniPL.Runnable prog = miniPL.ProcessParseTree(ptree, ps.GetErrors());

            Assert.AreEqual(2, prog.errors.Count);
        }

        [TestMethod]
        public void Semantics_IdentifierUseBeforeDeclarationTest()
        {
            string text = "n := 0;\n" +
                          "var n : int;\n";

            MiniPL miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();
            Tree<Parser.IParseValue> ptree = ps.Parse(sc.Tokenize(text));

            MiniPL.Runnable prog = miniPL.ProcessParseTree(ptree, ps.GetErrors());

            Assert.AreEqual(1, prog.errors.Count);
        }

        [TestMethod]
        public void Semantics_TypeCheck_ForTest()
        {
            string text = "var s : string; for s in \"hello\"..(2 = 2) do print 1; end for;";

            MiniPL miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();
            Tree<Parser.IParseValue> ptree = ps.Parse(sc.Tokenize(text));

            MiniPL.Runnable prog = miniPL.ProcessParseTree(ptree, ps.GetErrors());

            foreach (IError err in prog.errors)
                Console.WriteLine(err.GetMessage());

            Assert.AreEqual(3, prog.errors.Count);
        }

        [TestMethod]
        public void Semantics_TypeCheck_ReadTest()
        {
            string text = "var b : bool; read b;";

            MiniPL miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();
            Tree<Parser.IParseValue> ptree = ps.Parse(sc.Tokenize(text));

            MiniPL.Runnable prog = miniPL.ProcessParseTree(ptree, ps.GetErrors());

            foreach (IError err in prog.errors)
                Console.WriteLine(err.GetMessage());

            
            Assert.AreEqual(1, prog.errors.Count);
            Assert.IsTrue(prog.errors[0] is SemanticError);
        }

        [TestMethod]
        public void Semantics_TypeCheck_PrintTest()
        {
            string text = "var b : bool; print b;";

            MiniPL miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();
            Tree<Parser.IParseValue> ptree = ps.Parse(sc.Tokenize(text));

            MiniPL.Runnable prog = miniPL.ProcessParseTree(ptree, ps.GetErrors());

            foreach (IError err in prog.errors)
                Console.WriteLine(err.GetMessage());


            Assert.AreEqual(1, prog.errors.Count);
            Assert.IsTrue(prog.errors[0] is SemanticError);
        }

        [TestMethod]
        public void Semantics_TypeCheck_AssertTest()
        {
            string text = "var s : string; assert(s);";

            MiniPL miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();
            Tree<Parser.IParseValue> ptree = ps.Parse(sc.Tokenize(text));

            MiniPL.Runnable prog = miniPL.ProcessParseTree(ptree, ps.GetErrors());

            foreach (IError err in prog.errors)
                Console.WriteLine(err.GetMessage());


            Assert.AreEqual(1, prog.errors.Count);
            Assert.IsTrue(prog.errors[0] is SemanticError);
        }

        [TestMethod]
        public void Semantics_TypeCheck_AssignmentTest()
        {
            string text = "var b : bool; b := \"nope\";";

            MiniPL miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();
            Tree<Parser.IParseValue> ptree = ps.Parse(sc.Tokenize(text));

            MiniPL.Runnable prog = miniPL.ProcessParseTree(ptree, ps.GetErrors());

            foreach (IError err in prog.errors)
                Console.WriteLine(err.GetMessage());


            Assert.AreEqual(1, prog.errors.Count);
            Assert.IsTrue(prog.errors[0] is SemanticError);
        }

        [TestMethod]
        public void Semantics_TypeCheck_DeclarationTest()
        {
            string text = "var b : bool := \"nope\";";

            MiniPL miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();
            Tree<Parser.IParseValue> ptree = ps.Parse(sc.Tokenize(text));

            MiniPL.Runnable prog = miniPL.ProcessParseTree(ptree, ps.GetErrors());

            foreach (IError err in prog.errors)
                Console.WriteLine(err.GetMessage());


            Assert.AreEqual(1, prog.errors.Count);
            Assert.IsTrue(prog.errors[0] is SemanticError);
        }
    }
}
