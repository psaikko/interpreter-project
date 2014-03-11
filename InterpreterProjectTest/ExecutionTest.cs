using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InterpreterProject.LexicalAnalysis;
using InterpreterProject.SyntaxAnalysis;
using InterpreterProject.Languages;
using InterpreterProject.Errors;
using InterpreterProject;
using System.Collections.Generic;
using System.IO;

namespace InterpreterProjectTest
{
    [TestClass]
    public class ExecutionTest
    {
        private string RunWithInput(string program, string input)
        {
            MiniPL miniPL = MiniPL.GetInstance();
            Scanner sc = miniPL.GetScanner();
            Parser ps = miniPL.GetParser();

            Tree<Parser.IParseValue> ptree = ps.Parse(sc.Tokenize(program));
            MiniPL.Runnable prog = miniPL.ProcessParseTree(ptree, ps.GetErrors());

            StringWriter writer = new StringWriter();
            StringReader reader = new StringReader(input);

            prog.Execute(reader, writer);

            return writer.ToString();
        }

        [TestMethod]
        public void Execute_ReadErrorTest()
        {
            string program = "var x : int; read x; print x + 2;";
            string input = "bob";
            Assert.IsTrue(RunWithInput(program, input).StartsWith("Runtime error"));
        }

        [TestMethod]
        public void Execute_ReadStringTest()
        {
            string program = "var x : string; read x; print x; read x; print x;";
            string input = "hello world";
            Assert.AreEqual("helloworld", RunWithInput(program, input));
        }

        [TestMethod]
        public void Execute_ReadIntTest()
        {
            string program = "var x : int; read x; print x + 2;";
            string input = "4";
            Assert.AreEqual("6", RunWithInput(program, input));
        }

        [TestMethod]
        public void Execute_ForTest()
        {
            string program = "var i : int; for i in 0..10 do print i; end for;";
            string input = "";
            string output = RunWithInput(program, input);
            Console.WriteLine(output);
            Assert.AreEqual("0123456789", output);
        }

        [TestMethod]
        public void Execute_AssertPassTest()
        {
            string program = "assert (1 & 1);";
            string input = "";
            string output = RunWithInput(program, input);
            Assert.AreEqual("", output);
        }

        [TestMethod]
        public void Execute_AssertFailTest()
        {
            string program = "assert (1 = 2);";
            string input = "";
            string output = RunWithInput(program, input);
            Assert.IsTrue(output.StartsWith("Runtime error"));
        }

        [TestMethod]
        public void Execute_StringDeclarationTest()
        {
            string program = "var s : string; print s;";
            string input = "";
            string output = RunWithInput(program, input);
            Assert.AreEqual("", output);
        }

        [TestMethod]
        public void Execute_StringDeclarationAssignmenrTest()
        {
            string program = "var s : string := \"hello world\"; print s;";
            string input = "";
            string output = RunWithInput(program, input);
            Assert.AreEqual("hello world", output);
        }

        [TestMethod]
        public void Execute_IntegerDeclarationTest()
        {
            string program = "var i : int; print i;";
            string input = "";
            string output = RunWithInput(program, input);
            Assert.AreEqual("0", output);
        }

        [TestMethod]
        public void Execute_IntegerDeclarationAssignmenrTest()
        {
            string program = "var i : int := 1337; print i;";
            string input = "";
            string output = RunWithInput(program, input);
            Assert.AreEqual("1337", output);
        }

        [TestMethod]
        public void Execute_BooleanDeclarationTest()
        {
            string program = "var b : bool; assert(b);";
            string input = "";
            string output = RunWithInput(program, input);
            Assert.IsTrue(output.StartsWith("Runtime error"));
        }

        [TestMethod]
        public void Execute_BooleanDeclarationAssignmenrTest()
        {
            string program = "var b : bool := 1; assert(b);";
            string input = "";
            string output = RunWithInput(program, input);
            Assert.AreEqual("", output);
        }

        [TestMethod]
        public void Execute_ArithmeticExpressionTest()
        {
            string program = "print ((10 / 2) - 4) + (2*3);";
            string input = "";
            string output = RunWithInput(program, input);
            Assert.AreEqual("7", output);
        }

        [TestMethod]
        public void Execute_LogicalExpressionTest()
        {
            string program = "assert((4 = (2+2)) & (!(1=0)));";
            string input = "";
            string output = RunWithInput(program, input);
            Assert.AreEqual("", output);
        }
    }
}
