using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    public class MiniPL : ILanguage
    {
        static MiniPL instance;

        Dictionary<String, TokenType> tts = new Dictionary<string, TokenType>();
        Dictionary<String, CFG.Terminal> terms = new Dictionary<string, CFG.Terminal>();
        Dictionary<String, CFG.Variable> vars = new Dictionary<string, CFG.Variable>();
        CFG grammar;

        public static MiniPL GetInstance()
        {
            if (instance == null)
                instance = new MiniPL();
            return instance;
        }

        private MiniPL()
        {
            // Define regexes for tokens
            Regex reBlockComment = Regex.Concat("/*").Concat(Regex.Not('*').Union(Regex.Char('*').Plus().Concat(Regex.Not('*', '/'))).Star())
                                                     .Concat(Regex.Char('*').Plus().Concat(Regex.Char('/')));
            Regex reLineComment = Regex.Concat("//").Concat(Regex.Not('\n').Star());

            Regex reWhitespace = Regex.Union(" \t\n").Star().Union(reBlockComment).Union(reLineComment);

            Regex reString = Regex.Char('"').Concat(Regex.Char('\\').Concat(Regex.Any()).Union(Regex.Not('"', '\\')).Star()).Concat(Regex.Char('"'));
            Regex reBinaryOperator = Regex.Union("+-*/<=&");
            Regex reUnaryOperator = Regex.Char('!');
            Regex reKeyword = Regex.Union(Regex.Concat("var"), Regex.Concat("for"), Regex.Concat("end"), Regex.Concat("in"),
                                          Regex.Concat("do"), Regex.Concat("read"), Regex.Concat("print"), Regex.Concat("assert"));
            Regex reType = Regex.Union(Regex.Concat("bool"), Regex.Concat("int"), Regex.Concat("string"));

            Regex reParenRight = Regex.Char(')'),
                  reParenLeft = Regex.Char('('),
                  reColon = Regex.Char(':'),
                  reSemicolon = Regex.Char(';'),
                  reAssignment = Regex.Concat(":="),
                  reDots = Regex.Concat("..");

            Regex reIdentifier = Regex.Union(Regex.Range('A', 'Z'), Regex.Range('a', 'z'))
                                      .Concat(Regex.Union(Regex.Range('A', 'Z'), Regex.Range('a', 'z'), Regex.Range('0', '9'), Regex.Char('_')).Star());
            Regex reInteger = Regex.Range('0', '9').Plus();

            // Define token types
            tts["integer"] = new TokenType("integer", reInteger);
            tts["whitespace"] = new TokenType("whitespace", reWhitespace, priority: TokenType.Priority.Whitespace);
            tts["string"] = new TokenType("string", reString);
            tts["binary_op"] = new TokenType("binary op", reBinaryOperator);
            tts["unary_op"] = new TokenType("unary op", reUnaryOperator);
            tts["keyword"] = new TokenType("keyword", reKeyword, priority: TokenType.Priority.Keyword);
            tts["type"] = new TokenType("type", reType, priority: TokenType.Priority.Keyword);
            tts["left_paren"] = new TokenType("left paren", reParenLeft);
            tts["right_paren"] = new TokenType("right paren", reParenRight);
            tts["colon"] = new TokenType("colon", reColon);
            tts["semicolon"] = new TokenType("semicolon", reSemicolon);
            tts["assignment"] = new TokenType("assignment", reAssignment);
            tts["dots"] = new TokenType("dots", reDots);
            tts["identifier"] = new TokenType("identifier", reIdentifier);

            // Define nonterminal variables of CFG
            vars["program"] = new CFG.Variable("PROG");
            vars["statements"] = new CFG.Variable("STMTS");
            vars["statements_head"] = new CFG.Variable("STMTS_HEAD");
            vars["statements_tail"] = new CFG.Variable("STMTS_TAIL");
            vars["statement"] = new CFG.Variable("STMT");
            vars["declaration"] = new CFG.Variable("DECL");
            vars["declaration_assignment"] = new CFG.Variable("DECL_ASSIGN");
            vars["expression"] = new CFG.Variable("EXPR");
            vars["unary_operation"] = new CFG.Variable("UNARY_OP");
            vars["binary_operation"] = new CFG.Variable("BINARY_OP");
            vars["operand"] = new CFG.Variable("OPND");

            // Define terminal variables of CFG
            terms["identifier"] = new CFG.Terminal(tts["identifier"]);
            terms["assert"] = new CFG.Terminal("assert");
            terms["print"] = new CFG.Terminal("print");
            terms["read"] = new CFG.Terminal("read");
            terms["for"] = new CFG.Terminal("for");
            terms["in"] = new CFG.Terminal("in");
            terms["end"] = new CFG.Terminal("end");
            terms["do"] = new CFG.Terminal("do");
            terms["var"] = new CFG.Terminal("var");
            terms["type"] = new CFG.Terminal(tts["type"]);
            terms["string"] = new CFG.Terminal(tts["string"]);
            terms["integer"] = new CFG.Terminal(tts["integer"]);
            terms[")"] = new CFG.Terminal(")");
            terms["("] = new CFG.Terminal("(");
            terms[".."] = new CFG.Terminal("..");
            terms[":="] = new CFG.Terminal(":=");
            terms[":"] = new CFG.Terminal(":");
            terms[";"] = new CFG.Terminal(";");
            terms["binary_operator"] = new CFG.Terminal(tts["binary_op"]);
            terms["unary_operator"] = new CFG.Terminal(tts["unary_op"]);

            grammar = new CFG(vars["program"], terms.Values);

            grammar.AddProductionRule(vars["program"], new CFG.ISymbol[] { vars["statements"] });
            grammar.AddProductionRule(vars["statements"], new CFG.ISymbol[] { vars["statements_head"], vars["statements_tail"] });
            grammar.AddProductionRule(vars["statements_head"], new CFG.ISymbol[] { vars["statement"], terms[";"] });
            grammar.AddProductionRule(vars["statements_tail"], new CFG.ISymbol[] { vars["statements_head"], vars["statements_tail"] });
            grammar.AddProductionRule(vars["statements_tail"], new CFG.ISymbol[] { CFG.Terminal.epsilon });

            grammar.AddProductionRule(vars["statement"], new CFG.ISymbol[] { vars["declaration"] });
            grammar.AddProductionRule(vars["statement"], new CFG.ISymbol[] { terms["identifier"], terms[":="], vars["expression"] });
            grammar.AddProductionRule(vars["statement"], new CFG.ISymbol[] { terms["for"], terms["identifier"], terms["in"], vars["expression"], terms[".."], vars["expression"], terms["do"],
                                                                                   vars["statements"], terms["end"], terms["for"] });
            grammar.AddProductionRule(vars["statement"], new CFG.ISymbol[] { terms["read"], terms["identifier"] });
            grammar.AddProductionRule(vars["statement"], new CFG.ISymbol[] { terms["print"], vars["expression"] });
            grammar.AddProductionRule(vars["statement"], new CFG.ISymbol[] { terms["assert"], terms["("], vars["expression"], terms[")"] });

            grammar.AddProductionRule(vars["declaration"], new CFG.ISymbol[] { terms["var"], terms["identifier"], terms[":"], terms["type"], vars["declaration_assignment"] });
            grammar.AddProductionRule(vars["declaration_assignment"], new CFG.ISymbol[] { terms[":="], vars["expression"] });
            grammar.AddProductionRule(vars["declaration_assignment"], new CFG.ISymbol[] { CFG.Terminal.epsilon });

            grammar.AddProductionRule(vars["expression"], new CFG.ISymbol[] { vars["unary_operation"] });
            grammar.AddProductionRule(vars["expression"], new CFG.ISymbol[] { vars["operand"], vars["binary_operation"] });

            grammar.AddProductionRule(vars["unary_operation"], new CFG.ISymbol[] { terms["unary_operator"], vars["operand"] });

            grammar.AddProductionRule(vars["binary_operation"], new CFG.ISymbol[] { terms["binary_operator"], vars["operand"] });
            grammar.AddProductionRule(vars["binary_operation"], new CFG.ISymbol[] { CFG.Terminal.epsilon });

            grammar.AddProductionRule(vars["operand"], new CFG.ISymbol[] { terms["integer"] });
            grammar.AddProductionRule(vars["operand"], new CFG.ISymbol[] { terms["string"] });
            grammar.AddProductionRule(vars["operand"], new CFG.ISymbol[] { terms["identifier"] });
            grammar.AddProductionRule(vars["operand"], new CFG.ISymbol[] { terms["("], vars["expression"], terms[")"] });
        }

        public Dictionary<string, TokenType> GetTokenTypes()
        {
            return tts;
        }

        public Dictionary<string, CFG.Variable> GetGrammarNonterminals()
        {
            return vars;
        }

        public Dictionary<string, CFG.Terminal> GetGrammarTerminals()
        {
            return terms;
        }

        public Scanner GetScanner()
        {
            TokenAutomaton automaton = TokenType.CombinedAutomaton(tts.Values.ToArray());
            return new Scanner(automaton); 
        }

        public Parser GetParser()
        {
            throw new NotImplementedException();
        }


        public CFG GetGrammar()
        {
            return grammar;
        }
    }
}
