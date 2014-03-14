using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using InterpreterProject.LexicalAnalysis;
using InterpreterProject.SyntaxAnalysis;
using InterpreterProject.Languages;

namespace InterpreterProjectTest
{
    [TestClass]
    public class ScannerTest
    {
        private List<Token> GetTokens(TokenAutomaton automaton, string text)
        {
            Scanner sc = new Scanner(automaton);
            IEnumerable<Token> tokens = sc.Tokenize(text, yieldEOF:false);
            return new List<Token>(tokens);
        }

        [TestMethod]
        public void Scanner_ErrorTest()
        {
            Regex a = Regex.Char('a');
            TokenType aToken = new TokenType("a", a);
            TokenAutomaton automaton = aToken.Automaton();

            List<Token> tokens = GetTokens(automaton, "ccaacaacc");

            string[] expectedTokens = {"c","c","a","a","c","a","a","c","c"};
            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
            {
                Assert.AreEqual(expectedTokens[i], tokens[i].Lexeme);
                if (tokens[i].Lexeme == "a")
                    Assert.AreEqual(aToken, tokens[i].Type);
                else
                    Assert.AreEqual(TokenType.ERROR, tokens[i].Type);
            }
        }

        [TestMethod]
        public void Scanner_LineCommentTest()
        {
            Regex a = Regex.Char('a').Star();
            Regex lineComment = Regex.Concat("//").Concat(Regex.Not('\n').Star());
            Regex whitespace = Regex.Union(" \t\n").Star();
            TokenType ttWhitespace = new TokenType("Whitespace", whitespace, priority: TokenType.Priority.Whitespace);
            TokenType ttA = new TokenType("a*", a);
            TokenType ttLineComment = new TokenType("line comment", lineComment, priority: TokenType.Priority.Whitespace);
            TokenAutomaton automaton = TokenType.CombinedAutomaton(ttA, ttLineComment, ttWhitespace);

            string text = "aaa//aaa\n"+
                          "//aaaaaaa\n"+
                          "aaa//aaa";

            List<Token> tokens = GetTokens(automaton, text);

            string[] expectedTokens = { "aaa", "aaa" };
            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
            {
                Assert.AreEqual(expectedTokens[i], tokens[i].Lexeme);
            }
        }

        [TestMethod]
        public void Scanner_BlockCommentTest()
        {
            Regex a = Regex.Char('a').Star();
            Regex b = Regex.Char('b').Star();
            // block comment regex /\*([^*]|(\*+[^*/]))\*+/
            Regex blockComment = Regex.Concat("/*")
                                    .Concat(Regex.Not('*')
                                        .Union(Regex.Char('*').Plus()
                                                .Concat(Regex.Not('*','/')))
                                        .Star())
                                    .Concat(Regex.Char('*').Plus().Concat(Regex.Char('/')));          
            Regex whitespace = Regex.Union(" \t\n").Star();
            TokenType ttWhitespace = new TokenType("Whitespace", whitespace, priority: TokenType.Priority.Whitespace);
            TokenType ttA = new TokenType("a*", a);
            TokenType ttB = new TokenType("b*", b);
            TokenType ttBlockComment = new TokenType("line comment", blockComment, priority: TokenType.Priority.Whitespace);
            TokenAutomaton automaton = TokenType.CombinedAutomaton(ttA, ttB, ttBlockComment, ttWhitespace);

            string text = "/* aaa */   \n" +
                          "bbb         \n" +
                          "/* aaa */   \n" +
                          "bbb         \n" +
                          "/* aaa      \n" +
                          "   aaa */   \n" +
                          "bbb         \n" +
                          "/*          \n" +
                          " * aaa      \n" +
                          " */         \n" +
                          "bbb         \n" +
                          "/***        \n" +
                          " * aaa      \n" +
                          " ***/       \n" +
                          "bbb         \n" +
                          "/*/ aaa /*/ \n" +
                          "/*****/     \n" +
                          "/*///*/     \n" +
                          "bbb         \n" +
                          "/***        \n" +
                          " * aaa      \n" +
                          " */         \n" +
                          "bbb        ";

            Scanner sc = new Scanner(automaton);
            IEnumerable<Token> tokenEnumerable = sc.Tokenize(text, yieldEOF: false);
            List<Token> tokens = new List<Token>(tokenEnumerable);

            string[] expectedTokens = { "bbb", "bbb", "bbb", "bbb", "bbb", "bbb", "bbb" };
            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
            {
                Assert.AreEqual(expectedTokens[i], tokens[i].Lexeme);
                Assert.AreEqual(ttB, tokens[i].Type);
            }
        }

        [TestMethod]
        public void Scanner_NestedCommentHackTest1()
        {
            Regex a = Regex.Char('a').Star();
            Regex b = Regex.Char('b').Star();
            // hacky solution that allows for nested comments
            Regex reBlockCommentStart = Regex.Concat("/*");
            Regex reBlockCommentEnd = Regex.Concat("*/");
            Regex whitespace = Regex.Union(" \t\n").Star();
            TokenType ttWhitespace = new TokenType("Whitespace", whitespace, priority: TokenType.Priority.Whitespace);
            TokenType ttA = new TokenType("a*", a);
            TokenType ttB = new TokenType("b*", b);
            TokenType ttBlockCommentStart = new TokenType("block comment start", reBlockCommentStart);
            TokenType ttBlockCommentEnd = new TokenType("block comment end", reBlockCommentEnd);
            TokenAutomaton automaton = TokenType.CombinedAutomaton(ttA, ttB, ttBlockCommentStart, ttBlockCommentEnd, ttWhitespace);

            string text = "/* aaa */   \n" +
                          "bbb         \n" +
                          "/* aaa */   \n" +
                          "bbb         \n" +
                          "/* aaa      \n" +
                          "   aaa */   \n" +
                          "bbb         \n" +
                          "/*          \n" +
                          " * aaa      \n" +
                          " */         \n" +
                          "bbb         \n" +
                          "/***        \n" +
                          " * aaa      \n" +
                          " ***/       \n" +
                          "bbb         \n" +
                          "/*/ aaa */  \n" +
                          "/*****/     \n" +
                          "/*   */     \n" +
                          "bbb         \n" +
                          "/***        \n" +
                          " * aaa      \n" +
                          " */         \n" +
                          "bbb        ";

            Scanner sc = new Scanner(automaton, ttBlockCommentStart, ttBlockCommentEnd);
            IEnumerable<Token> tokenEnumerable = sc.Tokenize(text, yieldEOF: false);
            List<Token> tokens = new List<Token>(tokenEnumerable);

            string[] expectedTokens = { "bbb", "bbb", "bbb", "bbb", "bbb", "bbb", "bbb" };
            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
            {
                Assert.AreEqual(expectedTokens[i], tokens[i].Lexeme);
                Assert.AreEqual(ttB, tokens[i].Type);
            }
        }

        [TestMethod]
        public void Scanner_NestedCommentHackTest2()
        {
            Regex a = Regex.Char('a').Star();
            Regex b = Regex.Char('b').Star();
            // hacky solution that allows for nested comments
            Regex reBlockCommentStart = Regex.Concat("/*");
            Regex reBlockCommentEnd = Regex.Concat("*/");
            Regex whitespace = Regex.Union(" \t\n").Star();
            TokenType ttWhitespace = new TokenType("Whitespace", whitespace, priority: TokenType.Priority.Whitespace);
            TokenType ttA = new TokenType("a*", a);
            TokenType ttB = new TokenType("b*", b);
            TokenType ttBlockCommentStart = new TokenType("block comment start", reBlockCommentStart);
            TokenType ttBlockCommentEnd = new TokenType("block comment end", reBlockCommentEnd);
            TokenAutomaton automaton = TokenType.CombinedAutomaton(ttA, ttB, ttBlockCommentStart, ttBlockCommentEnd, ttWhitespace);

            string text = "bbb /* aaa /* aaa /* aaa */ aaa */ aaa /* aaa */ */ bbb";

            Scanner sc = new Scanner(automaton, ttBlockCommentStart, ttBlockCommentEnd);
            IEnumerable<Token> tokenEnumerable = sc.Tokenize(text, yieldEOF: false);
            List<Token> tokens = new List<Token>(tokenEnumerable);

            string[] expectedTokens = { "bbb", "bbb" };
            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
            {
                Assert.AreEqual(expectedTokens[i], tokens[i].Lexeme);
                Assert.AreEqual(ttB, tokens[i].Type);
            }
        }

        [TestMethod]
        public void Scanner_NestedCommentHackTest3()
        {
            Regex a = Regex.Char('a').Star();
            Regex b = Regex.Char('b').Star();
            // hacky solution that allows for nested comments
            Regex reBlockCommentStart = Regex.Concat("/*");
            Regex reBlockCommentEnd = Regex.Concat("*/");
            Regex whitespace = Regex.Union(" \t\n").Star();
            TokenType ttWhitespace = new TokenType("Whitespace", whitespace, priority: TokenType.Priority.Whitespace);
            TokenType ttA = new TokenType("a*", a);
            TokenType ttB = new TokenType("b*", b);
            TokenType ttBlockCommentStart = new TokenType("block comment start", reBlockCommentStart);
            TokenType ttBlockCommentEnd = new TokenType("block comment end", reBlockCommentEnd);
            TokenAutomaton automaton = TokenType.CombinedAutomaton(ttA, ttB, ttBlockCommentStart, ttBlockCommentEnd, ttWhitespace);

            string text = "bbb /* aaa";

            Scanner sc = new Scanner(automaton, ttBlockCommentStart, ttBlockCommentEnd);
            IEnumerable<Token> tokenEnumerable = sc.Tokenize(text, yieldEOF: false);
            List<Token> tokens = new List<Token>(tokenEnumerable);

            string[] expectedTokens = { "bbb" };
            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
            {
                Assert.AreEqual(expectedTokens[i], tokens[i].Lexeme);
                Assert.AreEqual(ttB, tokens[i].Type);
            }
        }

        [TestMethod]
        public void Scanner_IntegerTest()
        {
            Regex whitespace = Regex.Union(" \t\n").Star();
            Regex integer = Regex.Char('-').Maybe()
                .Concat(Regex.Range('1', '9'))
                .Concat(Regex.Range('0', '9').Star())
                .Union(Regex.Char('0'));
            TokenType ttWhitespace = new TokenType("Whitespace", whitespace);
            TokenType ttInteger = new TokenType("integer", integer);
            TokenAutomaton automaton = TokenType.CombinedAutomaton(ttInteger, ttWhitespace);
            List<Token> tokens = GetTokens(automaton, "1234 0 -99 -1");

            string[] expectedTokens = { "1234", " ", "0", " ", "-99", " ", "-1" };

            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
                Assert.AreEqual(expectedTokens[i], tokens[i].Lexeme);
        }

        [TestMethod]
        public void Scanner_WhitespaceTest()
        {
            Regex whitespace = Regex.Union(" \t\n").Star();
            Regex a = Regex.Char('a');
            TokenType ttWhitespace = new TokenType("Whitespace", whitespace);
            TokenType ttA = new TokenType("a", a);
            TokenAutomaton automaton = TokenType.CombinedAutomaton(ttA, ttWhitespace);
            List<Token> tokens = GetTokens(automaton, "a   a  \t\na  \n  a   \t\t   a     \n\t a ");
            string[] expectedTokens = { "a", "   ", "a", "  \t\n", "a", "  \n  ", "a", "   \t\t   ", "a", "     \n\t ", "a", " " };

            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
                Assert.AreEqual(expectedTokens[i], tokens[i].Lexeme);
        }

        [TestMethod]
        public void Scanner_StringTest()
        {
            Regex whitespace = Regex.Union(" \t\n").Star();
            Regex str = Regex.Char('"').Concat(Regex.Not('"').Star()).Concat(Regex.Char('"'));
            TokenType ttWhitespace = new TokenType("Whitespace", whitespace);
            TokenType ttString = new TokenType("string", str);
            TokenAutomaton automaton = TokenType.CombinedAutomaton(ttWhitespace, ttString);
            List<Token> tokens = GetTokens(automaton, "\"asdf\" \"sdfg\"");

            string[] expectedTokens = { "\"asdf\"", " ", "\"sdfg\"" };

            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
                Assert.AreEqual(expectedTokens[i], tokens[i].Lexeme);
        }

        [TestMethod]
        public void Scanner_EscapeStringTest()
        {
            Regex whitespace = Regex.Union(" \t\n").Star();
            // construct the regex "(\\.|[^"\\])*"
            Regex strBegin = Regex.Char('"');
            Regex strEnd = Regex.Char('"');
            Regex strBody = Regex.Char('\\').Concat(Regex.Any()).Union(Regex.Not('"', '\\')).Star();
            Regex str = strBegin.Concat(strBody).Concat(strEnd);
            TokenType ttWhitespace = new TokenType("Whitespace", whitespace);
            TokenType ttString = new TokenType("string", str);
            TokenAutomaton automaton = TokenType.CombinedAutomaton(ttWhitespace, ttString);
            List<Token> tokens = GetTokens(automaton, "\"as\\ndf\\\"\" \"sdfg\\\\\" \"\\\\\"\"");
            
            string[] expectedTokens = { "\"as\\ndf\\\"\"", " ", "\"sdfg\\\\\"" , " ", "\"\\\\\"", "\""};

            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
                Assert.AreEqual(expectedTokens[i], tokens[i].Lexeme);
        }

        [TestMethod]
        public void Scanner_TokenPriorityTest()
        {
            // keywords should be matched over regular tokens and whitespace should not be returned
            Regex keywords = Regex.Union(Regex.Concat("int"), Regex.Concat("bool"));
            Regex words = Regex.Range('a', 'z').Plus();
            Regex whitespace = Regex.Union(" \t\n").Star();
            TokenType ttKeyword = new TokenType("keyword", keywords, priority: TokenType.Priority.Keyword);
            TokenType ttWords = new TokenType("words", words);
            TokenType ttWhitespace = new TokenType("Whitespace", whitespace, priority: TokenType.Priority.Whitespace);
            TokenAutomaton automaton = TokenType.CombinedAutomaton(ttWords, ttWhitespace, ttKeyword);
            List<Token> tokens = GetTokens(automaton, "in int ints bool bools boo");

            string[] expectedTokens = { "in", "int", "ints", "bool", "bools", "boo"};

            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
                Assert.AreEqual(expectedTokens[i], tokens[i].Lexeme);
        }

        [TestMethod]
        public void Scanner_MiniPL_ExampleTest1()
        {
            string text = "var X : int := 4 + (6 * 2);\n"+
                          "print X;";

            string[] expectedTokens = { "var", "X", ":", "int", ":=", "4", "+", "(", "6", "*", "2", ")", ";", "print", "X", ";" };
            string[] expectedTypeNames = { "keyword", "identifier", "colon", "type", "assignment", "int", "binary op", "left paren",
                                           "int", "binary op", "int", "right paren", "semicolon", "keyword", "identifier", "semicolon"};

            Scanner sc = MiniPL.GetInstance().Scanner;
            List<Token> tokens = new List<Token>(sc.Tokenize(text, yieldEOF:false));

            for (int i = 0; i < tokens.Count; i++)
            {
                Assert.AreEqual(expectedTokens[i], tokens[i].Lexeme);
                Assert.AreEqual(expectedTypeNames[i], tokens[i].Type.Name);
            }
        }

        [TestMethod]
        public void Scanner_MiniPL_ExampleTest2()
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

            string[] expectedTokens = { "var", "nTimes", ":", "int", ":=", "0", ";",
                                        "print","\"How many times?\"",";",
                                        "read","nTimes",";",
                                        "var","x",":","int",";",
                                        "for","x","in","0","..","nTimes","-","1","do",
                                        "print","x",";",
                                        "print","\" : Hello, World!\n\"",";",
                                        "end","for",";",
                                        "assert","(","x","=","nTimes",")",";" };

            string[] expectedTypeNames = { "keyword", "identifier", "colon", "type", "assignment", "int", "semicolon",
                                           "keyword","string","semicolon",
                                           "keyword","identifier","semicolon",
                                           "keyword","identifier","colon","type", "semicolon",
                                           "keyword","identifier","keyword","int","dots","identifier","binary op","int","keyword",
                                           "keyword","identifier","semicolon",
                                           "keyword","string","semicolon",
                                           "keyword","keyword","semicolon",
                                           "keyword","left paren","identifier","binary op","identifier","right paren","semicolon" };

            Scanner sc = MiniPL.GetInstance().Scanner;
            List<Token> tokens = new List<Token>(sc.Tokenize(text, yieldEOF: false));

            for (int i = 0; i < tokens.Count; i++)
            {
                Assert.AreEqual(expectedTokens[i], tokens[i].Lexeme);
                Assert.AreEqual(expectedTypeNames[i], tokens[i].Type.Name);
            }
        }

        [TestMethod]
        public void Scanner_MiniPL_ExampleTest3()
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

            string[] expectedTokens = { "print", "\"Give a number\"", ";",
                                        "var", "n", ":", "int", ";",
                                        "read", "n", ";",
                                        "var", "f", ":", "int", ":=", "1", ";",
                                        "var", "i", ":", "int", ";",
                                        "for", "i", "in", "1", "..", "n", "do",
                                        "f", ":=", "f", "*", "i", ";",
                                        "end", "for", ";",
                                        "print", "\"The result is: \"", ";",
                                        "print", "f", ";"};
            string[] expectedTypeNames = { "keyword", "string", "semicolon",
                                           "keyword", "identifier", "colon", "type", "semicolon",
                                           "keyword", "identifier", "semicolon",
                                           "keyword", "identifier", "colon", "type", "assignment", "int", "semicolon",
                                           "keyword", "identifier", "colon", "type", "semicolon",
                                           "keyword", "identifier", "keyword", "int", "dots", "identifier", "keyword",
                                           "identifier", "assignment", "identifier", "binary op", "identifier", "semicolon",
                                           "keyword", "keyword", "semicolon",
                                           "keyword", "string", "semicolon",
                                           "keyword", "identifier", "semicolon"};

            Scanner sc = MiniPL.GetInstance().Scanner;
            List<Token> tokens = new List<Token>(sc.Tokenize(text, yieldEOF: false));

            for (int i = 0; i < tokens.Count; i++)
            {
                Assert.AreEqual(expectedTokens[i], tokens[i].Lexeme);
                Assert.AreEqual(expectedTypeNames[i], tokens[i].Type.Name);
            }
        }

        [TestMethod]
        public void Scanner_MiniPL_TokenPositionTest()
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

            int[] expectedRows = new int[] { 0, 0, 0, 
                1, 1, 1, 1, 1,
                2, 2, 2,
                3, 3, 3, 3, 3, 3, 3,
                4, 4, 4, 4, 4,
                5, 5, 5, 5, 5, 5, 5,
                6, 6, 6, 6, 6, 6,
                7, 7, 7,
                8, 8, 8,
                9, 9, 9
            };
            int[] expectedCols = new int[] { 0, 6, 21,
                0, 4, 6, 8, 11,
                0, 5, 6,
                0, 4, 6, 8, 12, 15, 16,
                0, 4, 6, 8, 11,
                0, 4, 6, 9, 10, 12, 14,
                4, 6, 9, 11, 13, 14,
                0, 4, 7,
                0, 6, 23,
                0, 6, 7
            };

            Scanner sc = MiniPL.GetInstance().Scanner;
            List<Token> tokens = new List<Token>(sc.Tokenize(text, yieldEOF: false));

            for (int i = 0; i < tokens.Count; i++)
            {
                Assert.AreEqual(expectedRows[i], tokens[i].TextPosition.Row);
                Assert.AreEqual(expectedCols[i], tokens[i].TextPosition.Column);
            }
        }

        [TestMethod]
        public void Scanner_MiniPL_NestedCommentTest()
        {
            string text = "print \"Give a number\";\n" +
                          "var n : int; /* \n" +
                          "read n;\n" +
                          "var f : int := 1;\n" +
                          "/* var i : int; */\n" +
                          "/* for i in 1..n do\n" +
                          "       f := f * i;\n" +
                          "*/ end for;\n" +
                          "print \"The result is: \";\n" +
                          "print f;";

            int[] expectedRows = new int[] { 0, 0, 0, 
                1, 1, 1, 1, 1
            };
            int[] expectedCols = new int[] { 0, 6, 21,
                0, 4, 6, 8, 11
            };

            Scanner sc = MiniPL.GetInstance().Scanner;
            List<Token> tokens = new List<Token>(sc.Tokenize(text, yieldEOF: false));

            for (int i = 0; i < tokens.Count; i++)
            {
                Assert.AreEqual(expectedRows[i], tokens[i].TextPosition.Row);
                Assert.AreEqual(expectedCols[i], tokens[i].TextPosition.Column);
            }
        }
    }
}
