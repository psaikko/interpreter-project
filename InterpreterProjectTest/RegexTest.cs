﻿using System;
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
                Assert.AreEqual(tokens[i].lexeme, "a");
        }

        [TestMethod]
        public void Regex_ConcatTest()
        {
            Regex ab = Regex.Concat("ab");
            TokenClass abClass = new TokenClass("ab", ab);
            DFA automaton = ab.ConstructDFA();

            string text = "ababab";

            Scanner sc = new Scanner(automaton);
            List<Token> tokens = sc.Tokenize(text);

            Assert.AreEqual(3, tokens.Count);
            for (int i = 0; i < 3; i++)
                Assert.AreEqual(tokens[i].lexeme, "ab");
        }

        [TestMethod]
        public void Regex_RangeTest()
        {
            Regex az = Regex.Range('a', 'z');
            TokenClass azClass = new TokenClass("az", az);
            DFA automaton = az.ConstructDFA();

            string text = "abcdefghijklmnopqrstuvwxyz";

            Scanner sc = new Scanner(automaton);
            List<Token> tokens = sc.Tokenize(text);

            Assert.AreEqual(text.Length, tokens.Count);
            for (int i = 0; i < text.Length; i++)
                Assert.AreEqual(((char)('a'+i)).ToString(), tokens[i].lexeme);
        }

        [TestMethod]
        public void Regex_UnionTest()
        {
            Regex aa = Regex.Concat("aa");
            Regex ab = Regex.Union("ab");
            TokenClass aaClass = new TokenClass("aa", aa);
            TokenClass abClass = new TokenClass("a|b", ab);
            Regex combined = aa.Union(ab);
            DFA automaton = combined.ConstructDFA();

            string text = "aababaabaa";

            Scanner sc = new Scanner(automaton);
            List<Token> tokens = sc.Tokenize(text);

            string[] expectedTokens = {"aa","b","a","b","aa","b","aa"};
            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
                Assert.AreEqual(expectedTokens[i], tokens[i].lexeme);
        }
    }
}
