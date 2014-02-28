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

            Assert.AreEqual(5, tokens.Count);
            for (int i = 0; i < 5; i++)
                Assert.AreEqual(tokens[0].lexeme, "a");
        }

        [TestMethod]
        public void Regex_ConcatTest()
        {
            Regex ab = Regex.Concat("ab");
            TokenClass abClass = new TokenClass("ab", ab);
            Console.WriteLine(ab);
            DFA automaton = ab.ConstructDFA();

            string text = "ababab";

            Scanner sc = new Scanner(automaton);
            List<Token> tokens = sc.Tokenize(text);

            Assert.AreEqual(3, tokens.Count);
            for (int i = 0; i < 5; i++)
                Assert.AreEqual(tokens[0].lexeme, "ab");
        }
    }
}
