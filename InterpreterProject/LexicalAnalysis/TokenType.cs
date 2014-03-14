﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.LexicalAnalysis
{
    public class TokenType
    {
        public enum Priority { Whitespace, Default, Keyword };

        public static readonly TokenType ERROR = new TokenType("ERROR", Regex.None());
        public static readonly TokenType EOF = new TokenType("EOF", Regex.Char(TokenAutomaton.EOF));

        String name;
        public String Name
        {
            get { return name; }
        }

        Priority tokenPriority;
        public Priority TokenPriority
        {
            get { return tokenPriority; }
        } 

        Regex regex;

        public TokenType(String name, Regex regex, Priority priority = Priority.Default)
        {
            this.name = name;
            this.regex = regex;
            this.tokenPriority = priority;
            regex.DefineTokenClass(this);
        }

        public Token CreateToken(String lexeme, int row, int col)
        {
            return new Token(this, lexeme, row, col);
        }

        public TokenAutomaton Automaton()
        {
            return regex.ConstructAutomaton();
        }

        public static TokenAutomaton CombinedAutomaton(params TokenType[] ts)
        {
            Regex[] rs = new Regex[ts.Length];
            for (int i = 0; i < ts.Length; i++)
                rs[i] = ts[i].regex;
            Regex combined = Regex.Union(rs);
            return combined.ConstructAutomaton();
        }
    }
}
