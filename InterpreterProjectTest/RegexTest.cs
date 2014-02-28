using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InterpreterProject;
using System.Collections.Generic;

namespace InterpreterProjectTest
{
    [TestClass]
    public class RegexTest
    {
        [TestMethod]
        public void Regex_CharacterTest()
        {
            Regex a = Regex.Character('a');

            TokenClass aClass = new TokenClass("a", a);

            DFA automaton = a.ConstructDFA();

            string text = "aaaaa";

            Scanner sc = new Scanner(automaton);
            List<Token> tokens = sc.Tokenize(text);

            Assert.AreEqual(tokens.Count, 5);
            for (int i = 0; i < 5; i++)
                Assert.AreEqual(tokens[0].lexeme, "a");
        }
    }
}
