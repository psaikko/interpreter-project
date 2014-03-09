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
            List<IError> errors = ps.GetErrors();

            MiniPL.Program prog = miniPL.TrimParseTree(ptree);

            Assert.AreEqual(3, prog.declarations.Count);
        }

        [TestMethod]
        public void Semantics_MultipleDeclarationErrorTest()
        {
            string text = "var n : int;\n" +
                          "var n : bool;\n" +
                          "var n : int := 0;\n";

            MiniPL miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();
            Tree<Parser.IParseValue> ptree = ps.Parse(sc.Tokenize(text));

            MiniPL.Program prog = miniPL.TrimParseTree(ptree);

            foreach (IError err in prog.errors)
                Console.WriteLine(err.GetMessage());

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

            MiniPL.Program prog = miniPL.TrimParseTree(ptree);

            foreach (IError err in prog.errors)
                Console.WriteLine(err.GetMessage());

            Assert.AreEqual(1, prog.errors.Count);
        }


    }
}
