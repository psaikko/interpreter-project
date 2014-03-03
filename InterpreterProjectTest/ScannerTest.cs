﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using InterpreterProject;

namespace InterpreterProjectTest
{
    [TestClass]
    public class ScannerTest
    {
        private List<Token> GetTokens(TokenAutomaton automaton, string text)
        {
            Scanner sc = new Scanner(automaton);
            IEnumerable<Token> tokens = sc.Tokenize(text);
            return new List<Token>(tokens);
        }

        [TestMethod]
        public void Scanner_ErrorTest()
        {
            Regex a = Regex.Character('a');
            TokenType aToken = new TokenType("a", a);
            TokenAutomaton automaton = aToken.Automaton();

            List<Token> tokens = GetTokens(automaton, "ccaacaacc");

            string[] expectedTokens = {"c","c","a","a","c","a","a","c","c"};
            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
            {
                Assert.AreEqual(expectedTokens[i], tokens[i].lexeme);
                if (tokens[i].lexeme == "a")
                    Assert.AreEqual(aToken, tokens[i].tokenType);
                else
                    Assert.AreEqual(TokenType.ERROR, tokens[i].tokenType);
            }
        }

        [TestMethod]
        public void Scanner_LineCommentTest()
        {
            Regex a = Regex.Character('a').Star();
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
                Assert.AreEqual(expectedTokens[i], tokens[i].lexeme);
            }
        }

        [TestMethod]
        public void Scanner_BlockCommentTest()
        {
            Regex a = Regex.Character('a').Star();
            Regex b = Regex.Character('b').Star();
            // block comment regex /\*([^*]|(\*+[^*/]))\*+/
            Regex blockComment = Regex.Concat("/*")
                                    .Concat(Regex.Not('*')
                                        .Union(Regex.Character('*').Plus()
                                                .Concat(Regex.Not('*','/')))
                                        .Star())
                                    .Concat(Regex.Character('*').Plus().Concat(Regex.Character('/')));          
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

            List<Token> tokens = GetTokens(automaton, text);

            string[] expectedTokens = { "bbb", "bbb", "bbb", "bbb", "bbb", "bbb", "bbb" };
            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
            {
                Assert.AreEqual(expectedTokens[i], tokens[i].lexeme);
                Assert.AreEqual(ttB, tokens[i].tokenType);
            }
        }

        [TestMethod]
        public void Scanner_IntegerTest()
        {
            Regex whitespace = Regex.Union(" \t\n").Star();
            Regex integer = Regex.Character('-').Maybe()
                .Concat(Regex.Range('1', '9'))
                .Concat(Regex.Range('0', '9').Star())
                .Union(Regex.Character('0'));
            TokenType ttWhitespace = new TokenType("Whitespace", whitespace);
            TokenType ttInteger = new TokenType("integer", integer);
            TokenAutomaton automaton = TokenType.CombinedAutomaton(ttInteger, ttWhitespace);
            List<Token> tokens = GetTokens(automaton, "1234 0 -99 -1");

            string[] expectedTokens = { "1234", " ", "0", " ", "-99", " ", "-1" };

            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
                Assert.AreEqual(expectedTokens[i], tokens[i].lexeme);
        }

        [TestMethod]
        public void Scanner_WhitespaceTest()
        {
            Regex whitespace = Regex.Union(" \t\n").Star();
            Regex a = Regex.Character('a');
            TokenType ttWhitespace = new TokenType("Whitespace", whitespace);
            TokenType ttA = new TokenType("a", a);
            TokenAutomaton automaton = TokenType.CombinedAutomaton(ttA, ttWhitespace);
            List<Token> tokens = GetTokens(automaton, "a   a  \t\na  \n  a   \t\t   a     \n\t a ");
            string[] expectedTokens = { "a", "   ", "a", "  \t\n", "a", "  \n  ", "a", "   \t\t   ", "a", "     \n\t ", "a", " " };

            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
                Assert.AreEqual(expectedTokens[i], tokens[i].lexeme);
        }

        [TestMethod]
        public void Scanner_StringTest()
        {
            Regex whitespace = Regex.Union(" \t\n").Star();
            Regex str = Regex.Character('"').Concat(Regex.Not('"').Star()).Concat(Regex.Character('"'));
            TokenType ttWhitespace = new TokenType("Whitespace", whitespace);
            TokenType ttString = new TokenType("string", str);
            TokenAutomaton automaton = TokenType.CombinedAutomaton(ttWhitespace, ttString);
            List<Token> tokens = GetTokens(automaton, "\"asdf\" \"sdfg\"");

            string[] expectedTokens = { "\"asdf\"", " ", "\"sdfg\"" };

            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
                Assert.AreEqual(expectedTokens[i], tokens[i].lexeme);
        }

        [TestMethod]
        public void Scanner_EscapeStringTest()
        {
            Regex whitespace = Regex.Union(" \t\n").Star();
            // construct the regex "(\\.|[^"\\])*"
            Regex strBegin = Regex.Character('"');
            Regex strEnd = Regex.Character('"');
            Regex strBody = Regex.Character('\\').Concat(Regex.Any()).Union(Regex.Not('"', '\\')).Star();
            Regex str = strBegin.Concat(strBody).Concat(strEnd);
            TokenType ttWhitespace = new TokenType("Whitespace", whitespace);
            TokenType ttString = new TokenType("string", str);
            TokenAutomaton automaton = TokenType.CombinedAutomaton(ttWhitespace, ttString);
            List<Token> tokens = GetTokens(automaton, "\"as\\ndf\\\"\" \"sdfg\\\\\" \"\\\\\"\"");
            
            string[] expectedTokens = { "\"as\\ndf\\\"\"", " ", "\"sdfg\\\\\"" , " ", "\"\\\\\"", "\""};

            Assert.AreEqual(expectedTokens.Length, tokens.Count);
            for (int i = 0; i < expectedTokens.Length; i++)
                Assert.AreEqual(expectedTokens[i], tokens[i].lexeme);
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
                Assert.AreEqual(expectedTokens[i], tokens[i].lexeme);
        }

        private TokenAutomaton CreateMiniPLAutomaton()
        {
            Regex whitespace = Regex.Union(" \t\n").Star();
            Regex str = Regex.Character('"')
                        .Concat(Regex.Character('\\')
                            .Concat(Regex.Any())
                            .Union(Regex.Not('"', '\\')).Star())
                        .Concat(Regex.Character('"'));
            Regex binaryOp = Regex.Union("+-*/<=&");
            Regex unaryOp = Regex.Character('!');
            Regex keyword = Regex.Union(
                Regex.Concat("var"), Regex.Concat("for"), Regex.Concat("end"), Regex.Concat("in"),
                Regex.Concat("do"), Regex.Concat("read"), Regex.Concat("print"), Regex.Concat("int"),
                Regex.Concat("string"), Regex.Concat("bool"), Regex.Concat("assert"));
            Regex lParen = Regex.Character(')');
            Regex rParen = Regex.Character('(');
            Regex colon = Regex.Character(':');
            Regex semicolon = Regex.Character(';');
            Regex assign = Regex.Concat(":=");
            Regex dots = Regex.Concat("..");
            Regex ident = Regex.Union(Regex.Range('A', 'Z'), 
                                      Regex.Range('a', 'z'))
                            .Concat(Regex.Union(Regex.Range('A', 'Z'),
                                                Regex.Range('a', 'z'),
                                                Regex.Range('0', '9'),
                                                Regex.Character('_')).Star());
            Regex integer = Regex.Range('0', '9').Plus();

            TokenType ttInteger = new TokenType("integer", integer);
            TokenType ttWhitespace = new TokenType("whitespace", whitespace, priority: TokenType.Priority.Whitespace);
            TokenType ttString = new TokenType("string", str);
            TokenType ttBinaryOp = new TokenType("binary op", binaryOp);
            TokenType ttUnaryOp = new TokenType("unary op", unaryOp);
            TokenType ttKeyword = new TokenType("keyword", keyword, priority: TokenType.Priority.Keyword);
            TokenType ttRightParen = new TokenType("right paren", rParen);
            TokenType ttLeftParen = new TokenType("left paren", lParen);
            TokenType ttColon = new TokenType("colon", colon);
            TokenType ttSemicolon = new TokenType("semicolon", semicolon);
            TokenType ttAssign = new TokenType("assignment", assign);
            TokenType ttDots = new TokenType("dots", dots);
            TokenType ttIdent = new TokenType("identifier", ident);

            return TokenType.CombinedAutomaton(
                ttWhitespace, ttString, ttBinaryOp, ttUnaryOp, ttKeyword, ttRightParen, ttLeftParen,
                ttColon, ttSemicolon, ttAssign, ttDots, ttIdent, ttInteger);
        }

        [TestMethod]
        public void Scanner_MiniPLTest_Example1()
        {
            string text = "var X : int := 4 + (6 * 2);\n"+
                          "print X;";

            string[] expectedTokens = { "var", "X", ":", "int", ":=", "4", "+", "(", "6", "*", "2", ")", ";", "print", "X", ";" };
            string[] expectedTypeNames = { "keyword", "identifier", "colon", "keyword", "assignment", "integer", "binary op", "right paren",
                                           "integer", "binary op", "integer", "left paren", "semicolon", "keyword", "identifier", "semicolon"};

            List<Token> tokens = GetTokens(CreateMiniPLAutomaton(), text);

            for (int i = 0; i < tokens.Count; i++)
            {
                Assert.AreEqual(expectedTokens[i], tokens[i].lexeme);
                Assert.AreEqual(expectedTypeNames[i], tokens[i].tokenType.name);
            }
        }

        [TestMethod]
        public void Scanner_MiniPLTest_Example2()
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

            string[] expectedTypeNames = { "keyword", "identifier", "colon", "keyword", "assignment", "integer", "semicolon",
                                           "keyword","string","semicolon",
                                           "keyword","identifier","semicolon",
                                           "keyword","identifier","colon","keyword", "semicolon",
                                           "keyword","identifier","keyword","integer","dots","identifier","binary op","integer","keyword",
                                           "keyword","identifier","semicolon",
                                           "keyword","string","semicolon",
                                           "keyword","keyword","semicolon",
                                           "keyword","right paren","identifier","binary op","identifier","left paren","semicolon" };

            List<Token> tokens = GetTokens(CreateMiniPLAutomaton(), text);

            for (int i = 0; i < tokens.Count; i++)
            {
                Assert.AreEqual(expectedTokens[i], tokens[i].lexeme);
                Assert.AreEqual(expectedTypeNames[i], tokens[i].tokenType.name);
            }
        }

        [TestMethod]
        public void Scanner_MiniPLTest_Example3()
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
                                           "keyword", "identifier", "colon", "keyword", "semicolon",
                                           "keyword", "identifier", "semicolon",
                                           "keyword", "identifier", "colon", "keyword", "assignment", "integer", "semicolon",
                                           "keyword", "identifier", "colon", "keyword", "semicolon",
                                           "keyword", "identifier", "keyword", "integer", "dots", "identifier", "keyword",
                                           "identifier", "assignment", "identifier", "binary op", "identifier", "semicolon",
                                           "keyword", "keyword", "semicolon",
                                           "keyword", "string", "semicolon",
                                           "keyword", "identifier", "semicolon"};

            List<Token> tokens = GetTokens(CreateMiniPLAutomaton(), text);

            for (int i = 0; i < tokens.Count; i++)
            {
                Assert.AreEqual(expectedTokens[i], tokens[i].lexeme);
                Assert.AreEqual(expectedTypeNames[i], tokens[i].tokenType.name);
            }
        }
    }
}
