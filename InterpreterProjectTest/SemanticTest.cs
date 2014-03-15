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
        private List<Error> GetErrors(string program)
        {
            MiniPL miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.Scanner;
            Parser ps = miniPL.Parser;
            ParseTree ptree = ps.Parse(sc.Tokenize(program));
            MiniPL.Runnable prog = miniPL.ProcessParseTree(ptree, ps.Errors);

            return prog.errors;
        }

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
            Scanner sc = miniPL.Scanner;
            Parser ps = miniPL.Parser;
            ParseTree ptree = ps.Parse(sc.Tokenize(text));
            MiniPL.Runnable prog = miniPL.ProcessParseTree(ptree, ps.Errors);

            Assert.AreEqual(3, prog.declarations.Count);
        }

        [TestMethod]
        public void Semantics_MultipleDeclarationTest()
        {
            string text = "var n : int;\n" +
                          "var n : bool;\n" +
                          "var n : int := 0;\n";

            Assert.AreEqual(2, GetErrors(text).Count);
        }

        [TestMethod]
        public void Semantics_IdentifierUseBeforeDeclarationTest()
        {
            string text = "n := 0;\n" +
                          "var n : int;\n";

            Assert.AreEqual(1, GetErrors(text).Count);
        }

        [TestMethod]
        public void Semantics_ForLoopTest1()
        {
            string text = "var i : int; for i in 0..9 do i := i + 1; end for;";

            List<Error> errors = GetErrors(text);
            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors[0] is SemanticError);
        }

        [TestMethod]
        public void Semantics_ForLoopTest2()
        {
            string text = "var i : int; var j : int; for i in 0..9 do for j in 0..9 do i := i + 1; end for; end for;";

            List<Error> errors = GetErrors(text);
            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors[0] is SemanticError);
        }

        [TestMethod]
        public void Semantics_TypeCheck_ForTest()
        {
            string text = "var s : string; for s in \"hello\"..(2 = 2) do print 1; end for;";

            Assert.AreEqual(3, GetErrors(text).Count);
        }

        [TestMethod]
        public void Semantics_TypeCheck_ReadTest()
        {
            string text = "var b : bool; read b;";

            List<Error> errors = GetErrors(text);
            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors[0] is SemanticError);
        }

        [TestMethod]
        public void Semantics_TypeCheck_PrintTest()
        {
            string text = "var b : bool; print b;";

            List<Error> errors = GetErrors(text);
            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors[0] is SemanticError);
        }

        [TestMethod]
        public void Semantics_TypeCheck_AssertTest()
        {
            string text = "var s : string; assert(s);";

            List<Error> errors = GetErrors(text);
            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors[0] is SemanticError);
        }

        [TestMethod]
        public void Semantics_TypeCheck_AssignmentTest()
        {
            string text = "var b : bool; b := \"nope\";";

            List<Error> errors = GetErrors(text);
            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors[0] is SemanticError);
        }

        [TestMethod]
        public void Semantics_TypeCheck_DeclarationTest()
        {
            string text = "var b : bool := \"nope\";";

            List<Error> errors = GetErrors(text);
            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors[0] is SemanticError);
        }

        [TestMethod]
        public void Semantics_TypeCheck_AdditionTest()
        {
            string text = "var a : int := (1 + \"lol\"); var b : int := (\"lol\" + 1);";

            List<Error> errors = GetErrors(text);
            // 2 for bad assignments, extra for assigning inferred string type to int variable
            Assert.AreEqual(3, errors.Count);
            Assert.IsTrue(errors[0] is SemanticError);
            Assert.IsTrue(errors[1] is SemanticError);
            Assert.IsTrue(errors[2] is SemanticError);
        }

        [TestMethod]
        public void Semantics_TypeCheck_SubtractionTest()
        {
            string text = "var a : int := (1 - \"lol\");\n var b : int := (\"lol\" - 1);\n var c : int := (1 - (1 = 1));";

            List<Error> errors = GetErrors(text);

            Assert.AreEqual(3, errors.Count);
            Assert.IsTrue(errors[0] is SemanticError);
            Assert.IsTrue(errors[1] is SemanticError);
            Assert.IsTrue(errors[2] is SemanticError);
        }

        [TestMethod]
        public void Semantics_TypeCheck_MultiplicationTest()
        {
            string text = "var a : int := (1 * \"lol\");\n var b : int := (\"lol\" * 1);\n var c : int := (1 * (1 = 1));";

            List<Error> errors = GetErrors(text);

            Assert.AreEqual(3, errors.Count);
            Assert.IsTrue(errors[0] is SemanticError);
            Assert.IsTrue(errors[1] is SemanticError);
            Assert.IsTrue(errors[2] is SemanticError);
        }

        [TestMethod]
        public void Semantics_TypeCheck_DivisionTest()
        {
            string text = "var a : int := (1 / \"lol\");\n var b : int := (\"lol\" / 1);\n var c : int := (1 / (1 = 1));";

            List<Error> errors = GetErrors(text);

            Assert.AreEqual(3, errors.Count);
            Assert.IsTrue(errors[0] is SemanticError);
            Assert.IsTrue(errors[1] is SemanticError);
            Assert.IsTrue(errors[2] is SemanticError);
        }

        [TestMethod]
        public void Semantics_TypeCheck_EqualityTest()
        {
            string text = "assert(1 = \"nope\"); assert(\"nope\" = 1);";

            List<Error> errors = GetErrors(text);

            Assert.AreEqual(2, errors.Count);
            Assert.IsTrue(errors[0] is SemanticError);
            Assert.IsTrue(errors[1] is SemanticError);
        }

        [TestMethod]
        public void Semantics_TypeCheck_LessThanTest()
        {
            string text = "assert(1 < \"nope\"); assert(\"nope\" < 1); assert(\"nope\" < (1=1)); assert((1=1) < \"nope\");";

            List<Error> errors = GetErrors(text);

            Assert.AreEqual(4, errors.Count);
            Assert.IsTrue(errors[0] is SemanticError);
            Assert.IsTrue(errors[3] is SemanticError);
            Assert.IsTrue(errors[2] is SemanticError);
            Assert.IsTrue(errors[1] is SemanticError);
        }

        [TestMethod]
        public void Semantics_TypeCheck_AndTest()
        {
            string text = "assert(1 & \"nope\"); assert(\"nope\" & 1); assert(\"nope\" & (1=1)); assert((1=1) & \"nope\");";

            List<Error> errors = GetErrors(text);

            Assert.AreEqual(4, errors.Count);
            Assert.IsTrue(errors[0] is SemanticError);
            Assert.IsTrue(errors[3] is SemanticError);
            Assert.IsTrue(errors[2] is SemanticError);
            Assert.IsTrue(errors[1] is SemanticError);
        }

        [TestMethod]
        public void Semantics_TypeCheck_NotTest()
        {
            string text = "assert(!\"nope\");";

            List<Error> errors = GetErrors(text);

            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors[0] is SemanticError);
        }

        [TestMethod]
        public void Semantics_TypeCheck_NestedExpressionsTest()
        {
            // should determine '+' to yield a string type, give type error on &
            string text = "assert((\"a\"+\"b\") & (1=2));";

            List<Error> errors = GetErrors(text);

            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors[0] is SemanticError);
        }
    }
}
