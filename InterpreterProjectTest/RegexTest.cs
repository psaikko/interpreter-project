using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InterpreterProject;
using System.Collections.Generic;
using InterpreterProject.LexicalAnalysis;
using InterpreterProject.SyntaxAnalysis;

namespace InterpreterProjectTest
{
    [TestClass]
    public class RegexTest
    {
        private List<Token> GetTokens(TokenAutomaton automaton, string text)
        {
            Scanner sc = new Scanner(automaton);
            IEnumerable<Token> tokens = sc.Tokenize(text, yieldEOF:false);
            return new List<Token>(tokens);
        }

        [TestMethod]
        public void Regex_CharacterTest()
        {
            Regex a = Regex.Char('a');
            TokenType aType = new TokenType("a", a);
            TokenAutomaton automaton = aType.Automaton();
            List<Token> tokens = GetTokens(automaton, "aaaaa");
            
            Assert.AreEqual(5, tokens.Count);
            for (int i = 0; i < 5; i++)
                Assert.AreEqual(tokens[i].Lexeme, "a");
        }

        [TestMethod]
        public void Regex_ConcatTest()
        {
            Regex ab = Regex.Concat("ab");
            TokenType abType = new TokenType("ab", ab);
            TokenAutomaton automaton = abType.Automaton();
            List<Token> tokens = GetTokens(automaton, "ababab");

            Assert.AreEqual(3, tokens.Count);
            for (int i = 0; i < 3; i++)
                Assert.AreEqual(tokens[i].Lexeme, "ab");
        }

        [TestMethod]
        public void Regex_RangeTest()
        {
            Regex az = Regex.Range('a', 'z');
            TokenType azType = new TokenType("az", az);
            TokenAutomaton automaton = azType.Automaton();
            List<Token> tokens = GetTokens(automaton, "abcdefghijklmnopqrstuvwxyz");

            string[] expectedTokens = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
                Assert.AreEqual(expectedTokens[i], tokens[i].Lexeme);
        }

        [TestMethod]
        public void Regex_UnionTest()
        {
            Regex aa = Regex.Concat("aa");
            Regex ab = Regex.Union("ab");
            TokenType aaType = new TokenType("aa", aa);
            TokenType abType = new TokenType("a|b", ab);
            TokenAutomaton automaton = TokenType.CombinedAutomaton(aaType, abType);
            List<Token> tokens = GetTokens(automaton, "aababaabaa");

            string[] expectedTokens = { "aa", "b", "a", "b", "aa", "b", "aa" };
            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
                Assert.AreEqual(expectedTokens[i], tokens[i].Lexeme);
        }

        [TestMethod]
        public void Regex_PlusTest()
        {
            Regex ba = Regex.Char('b').Concat(Regex.Char('a').Plus());
            TokenType baType = new TokenType("ba+", ba);
            TokenAutomaton automaton = baType.Automaton();
            List<Token> tokens = GetTokens(automaton, "baababab");

            string[] expectedTokens = { "baa","ba","ba","b" };
            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
                Assert.AreEqual(expectedTokens[i], tokens[i].Lexeme);
            Assert.AreEqual(TokenType.ERROR, tokens[3].Type);
        }

        [TestMethod]
        public void Regex_StarTest()
        {
            Regex ab = Regex.Char('a').Star().Concat(Regex.Char('b'));
            TokenType abType = new TokenType("a*b", ab);
            TokenAutomaton automaton = abType.Automaton();
            List<Token> tokens = GetTokens(automaton, "aababb");

            string[] expectedTokens = { "aab","ab","b" };
            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
                Assert.AreEqual(expectedTokens[i], tokens[i].Lexeme);
        }

        [TestMethod]
        public void Regex_MaybeTest()
        {
            Regex ab = Regex.Char('a').Maybe().Concat(Regex.Char('b'));
            TokenType abType = new TokenType("a?b", ab);
            TokenAutomaton automaton = abType.Automaton();
            List<Token> tokens = GetTokens(automaton, "babbab");

            string[] expectedTokens = { "b","ab","b","ab" };
            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
                Assert.AreEqual(expectedTokens[i], tokens[i].Lexeme);
        }

        [TestMethod]
        public void Regex_NotTest()
        {
            Regex b = Regex.Char('b');
            Regex notb = Regex.Not('b');
            TokenType bType = new TokenType("b", b);
            TokenType notbType = new TokenType("not b", notb);
            TokenAutomaton automaton = TokenType.CombinedAutomaton(bType, notbType);
            List<Token> tokens = GetTokens(automaton, "9jQbksjhbQ3b");

            string[] expectedTokens = { "9","j","Q","b","k","s","j","h","b","Q","3","b" };
            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
            {
                Assert.AreEqual(expectedTokens[i], tokens[i].Lexeme);
                if (expectedTokens[i] == "b")
                    Assert.AreEqual("b", tokens[i].Type.Name);
                else
                    Assert.AreEqual("not b", tokens[i].Type.Name);
            }
        }

        [TestMethod]
        public void Regex_AnyTest()
        {
            Regex any = Regex.Any();
            TokenType anyType = new TokenType("any", any);
            TokenAutomaton automaton = anyType.Automaton();
            List<Token> tokens = GetTokens(automaton, "9jQbksjhbQ3b");

            string[] expectedTokens = { "9", "j", "Q", "b", "k", "s", "j", "h", "b", "Q", "3", "b" };
            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
            {
                Assert.AreEqual(expectedTokens[i], tokens[i].Lexeme);
                Assert.AreEqual(anyType, tokens[i].Type);
            }
        }
    }
}
