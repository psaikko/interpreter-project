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
            TokenType aType = new TokenType("a", a);
            TokenAutomaton automaton = aType.Automaton();

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
            TokenType abType = new TokenType("ab", ab);
            TokenAutomaton automaton = abType.Automaton();

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
            TokenType azType = new TokenType("az", az);
            TokenAutomaton automaton = azType.Automaton();

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
            TokenType aaType = new TokenType("aa", aa);
            TokenType abType = new TokenType("a|b", ab);
            TokenAutomaton automaton = TokenType.CombinedAutomaton(aaType, abType);

            string text = "aababaabaa";

            Scanner sc = new Scanner(automaton);
            List<Token> tokens = sc.Tokenize(text);

            string[] expectedTokens = {"aa","b","a","b","aa","b","aa"};
            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
                Assert.AreEqual(expectedTokens[i], tokens[i].lexeme);
        }

        [TestMethod]
        public void Regex_PlusTest()
        {
            Regex ba = Regex.Character('b').Concat(Regex.Character('a').Plus());
            TokenType baType = new TokenType("ba+", ba);
            TokenAutomaton automaton = baType.Automaton();

            string text = "baababab";

            Scanner sc = new Scanner(automaton);
            List<Token> tokens = sc.Tokenize(text);

            string[] expectedTokens = { "baa","ba","ba","b" };
            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
                Assert.AreEqual(expectedTokens[i], tokens[i].lexeme);
            Assert.AreEqual(TokenType.ERROR, tokens[3].type);
        }

        [TestMethod]
        public void Regex_StarTest()
        {
            Regex ab = Regex.Character('a').Star().Concat(Regex.Character('b'));
            TokenType abType = new TokenType("a*b", ab);
            TokenAutomaton automaton = abType.Automaton();

            string text = "aababb";

            Scanner sc = new Scanner(automaton);
            List<Token> tokens = sc.Tokenize(text);

            string[] expectedTokens = { "aab","ab","b" };
            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
                Assert.AreEqual(expectedTokens[i], tokens[i].lexeme);
        }

        [TestMethod]
        public void Regex_MaybeTest()
        {
            Regex ab = Regex.Character('a').Maybe().Concat(Regex.Character('b'));
            TokenType abType = new TokenType("a?b", ab);
            TokenAutomaton automaton = abType.Automaton();

            string text = "babbab";

            Scanner sc = new Scanner(automaton);
            List<Token> tokens = sc.Tokenize(text);

            string[] expectedTokens = { "b","ab","b","ab" };
            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
                Assert.AreEqual(expectedTokens[i], tokens[i].lexeme);
        }

        [TestMethod]
        public void Regex_NotTest()
        {
            Regex b = Regex.Character('b');
            Regex notb = Regex.Not('b');
            TokenType bType = new TokenType("b", b);
            TokenType notbType = new TokenType("not b", notb);
            TokenAutomaton automaton = TokenType.CombinedAutomaton(bType, notbType);

            string text = "9jQbksjhbQ3b";

            Scanner sc = new Scanner(automaton);

            List<Token> tokens = sc.Tokenize(text);

            string[] expectedTokens = { "9","j","Q","b","k","s","j","h","b","Q","3","b" };
            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
            {
                Assert.AreEqual(expectedTokens[i], tokens[i].lexeme);
                if (expectedTokens[i] == "b")
                    Assert.AreEqual("b", tokens[i].type.name);
                else
                    Assert.AreEqual("not b", tokens[i].type.name);
            }
        }

        [TestMethod]
        public void Regex_AnyTest()
        {
            Regex any = Regex.Any();
            TokenType anyType = new TokenType("any", any);
            TokenAutomaton automaton = anyType.Automaton();

            string text = "9jQbksjhbQ3b";

            Scanner sc = new Scanner(automaton);
            List<Token> tokens = sc.Tokenize(text);

            string[] expectedTokens = { "9", "j", "Q", "b", "k", "s", "j", "h", "b", "Q", "3", "b" };
            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
            {
                Assert.AreEqual(expectedTokens[i], tokens[i].lexeme);
                Assert.AreEqual(anyType, tokens[i].type);
            }
        }

        [TestMethod]
        public void Regex_MatchCountTest()
        {
            Regex heresy = Regex.MatchCount(Regex.Character('a'), Regex.Character('b'));
            Regex a = Regex.Character('a');
            Regex b = Regex.Character('b');
            TokenType abMatch = new TokenType("abMatch", heresy);
            TokenType aToken = new TokenType("a", a);
            TokenType bToken = new TokenType("b", b);
            TokenAutomaton automaton = TokenType.CombinedAutomaton(abMatch, aToken, bToken);

            string text = "abaabbaaabbaababb";

            Scanner sc = new Scanner(automaton);
            List<Token> tokens = sc.Tokenize(text);

            Token.PrintList(tokens);

            string[] expectedTokens = { "ab", "aabb", "a", "aabb", "a", "ab", "ab", "b"};

            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
            {
                Assert.AreEqual(expectedTokens[i], tokens[i].lexeme);
            }
        }
    }
}
