using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    public class Regex
    {
        private Node start;
        private Node end;

        private Regex(Node start, Node end)
        {
            this.start = start;
            this.end = end;
        }

        public static Regex Concat(Regex a, Regex b)
        {
            a.end.epsilonTransitions.Add(b.start);
            return new Regex(a.start, b.end);
        }

        public static Regex Concat(String s)
        {
            Regex re = Any();
            for (int i = 0; i < s.Length; i++)
                re = Regex.Concat(re, Regex.Character(s[i]));
            return re;
        }

        public static Regex Star(Regex r)
        {
            Node start = new Node();
            Node end = new Node();
            r.end.epsilonTransitions.Add(r.start);
            start.epsilonTransitions.Add(r.start);
            start.epsilonTransitions.Add(end);
            r.end.epsilonTransitions.Add(end);
            return new Regex(start, end);
        }

        public static Regex Plus(Regex r)
        {
            // like star, but need to go through r at least once
            Node start = new Node();
            Node end = new Node();
            r.end.epsilonTransitions.Add(r.start);
            start.epsilonTransitions.Add(r.start);
            r.end.epsilonTransitions.Add(end);
            return new Regex(start, end);
        }

        public static Regex Maybe(Regex r)
        {
            // like star, but can't repeat r
            Node start = new Node();
            Node end = new Node();
            start.epsilonTransitions.Add(r.start);
            start.epsilonTransitions.Add(end);
            r.end.epsilonTransitions.Add(end);
            return new Regex(start, end);
        }

        public static Regex Union(Regex a, Regex b)
        {
            Node start = new Node();
            Node end = new Node();
            start.epsilonTransitions.Add(a.start);
            start.epsilonTransitions.Add(b.start);
            a.end.epsilonTransitions.Add(end);
            b.end.epsilonTransitions.Add(end);
            return new Regex(start, end);
        }

        public static Regex Union(String s)
        {
            Regex re = None();
            for (int i = 0; i < s.Length; i++)
                re = Union(re, Character(s[i]));
            return re;
        }

        public static Regex Range(char a, char b)
        {
            Regex re = None();
            for (char c = a; c < b; c++)
                re = Union(re, Character(c));
            return re;
        }

        public static Regex Character(char c)
        {
            Node start = new Node();
            Node end = new Node();
            start.transitions.Add(c, end);
            return new Regex(start, end);
        }

        public static Regex None()
        {
            Node start = new Node();
            Node end = new Node();
            return new Regex(start, end);
        }

        public static Regex Any()
        {
            Node start = new Node();
            Node end = new Node();
            start.epsilonTransitions.Add(end);
            return new Regex(start, end);
        }

        public void DefineTokenClass(TokenClass tokenClass)
        {
            this.end.tokenClass = tokenClass;
        }

        /*
         * Make a DFA-ish automaton from the regex
         */
        public DFA ConstructDFA()
        {
            Dictionary<ISet<Node>, DFA.State> DFAStates = new Dictionary<ISet<Node>, DFA.State>(new SetEqualityComparer<Node>());
            DFA.State DFAStartState = new DFA.State();
            ISet<Node> NFAStartStates = start.EpsilonMove();
            DFAStates.Add(NFAStartStates, DFAStartState);

            Stack<ISet<Node>> s = new Stack<ISet<Node>>();
            s.Push(NFAStartStates);
            
            while (s.Count != 0)
            {
                ISet<Node> currentNFAStates = s.Pop();
                DFA.State currentDFAState = DFAStates[currentNFAStates];

                Console.WriteLine("=================================");
                Console.WriteLine("Existing States:");
                foreach (ISet<Node> nodeset in DFAStates.Keys)
                    Node.PrintSet(nodeset);
                Console.WriteLine("In Stack:");
                foreach (ISet<Node> nodeset in s)
                    Node.PrintSet(nodeset);
                Console.WriteLine("\nCurrent:");
                Node.PrintSet(currentNFAStates);
                
                ISet<char> nextChars = new HashSet<char>();
                

                // find which characters we can move with from NFAStates
                foreach (Node n in NFAStartStates)
                {
                    nextChars.UnionWith(n.transitions.Keys);
                }

                // for each character find states we can move to, make a new DFA state 
                foreach (char c in nextChars)
                {
                    ISet<Node> nextNFAStates = Node.Move(currentNFAStates, c);

                    HashSet<Node> epsilonClosure = new HashSet<Node>();
                    epsilonClosure.UnionWith(nextNFAStates);
                    epsilonClosure.UnionWith(Node.EpsilonMove(nextNFAStates));

                    if (epsilonClosure.Count != 0)
                    {
                        Console.WriteLine("Reachable with " + c + ":");
                        Node.PrintSet(epsilonClosure);
                        
                        DFA.State nextDFAState;

                        if (!DFAStates.TryGetValue(epsilonClosure, out nextDFAState))
                        {
                            nextDFAState = new DFA.State();

                            foreach (Node n in epsilonClosure)
                                if (n.tokenClass != null)
                                    nextDFAState.recognizedTokens.Add(n.tokenClass);

                            Console.WriteLine("Adding:");
                            Node.PrintSet(epsilonClosure);
                            DFAStates.Add(epsilonClosure, nextDFAState);
                            s.Push(epsilonClosure);
                        }
                        else
                        {
                            Console.WriteLine("Exists");
                        }
                        Console.WriteLine();

                        currentDFAState.transitions.Add(c, nextDFAState);
                    }
                }
            }

            return new DFA(DFAStartState);
        }

        /*
         * Class for representing nodes of a NFA-ish automaton
         */
        private class Node
        {
            public static int counter = 0;
            public int id = counter++;

            public Dictionary<char, Node> transitions = new Dictionary<char, Node>();
            public List<Node> epsilonTransitions = new List<Node>();
            public TokenClass tokenClass = null;

            /*
             * Calculates the epsilon closure of a node with BFS from node
             */
            public ISet<Node> EpsilonMove()
            {
                HashSet<Node> reachable = new HashSet<Node>();
                reachable.Add(this);
                Stack<Node> s = new Stack<Node>();
                s.Push(this);
                while (s.Count != 0)
                {
                    Node current = s.Pop();
                    foreach (Node next in current.epsilonTransitions)
                    {
                        if (!reachable.Contains(next))
                        {
                            s.Push(next);
                            reachable.Add(next);
                        }
                    }
                }
                return reachable;
            }

            public static ISet<Node> EpsilonMove(ICollection<Node> nodes)
            {
                HashSet<Node> reachable = new HashSet<Node>();
                foreach (Node node in nodes)
                    reachable.UnionWith(node.EpsilonMove());
                return reachable;
            }

            public static ISet<Node> Move(ICollection<Node> nodes, char c)
            {
                HashSet<Node> reachable = new HashSet<Node>();
                foreach (Node node in nodes)
                {
                    Node next;
                    if (node.transitions.TryGetValue(c, out next))
                    {
                        reachable.Add(next);
                    }
                }
                return reachable;
            }

            public static void PrintSet(ISet<Node> nodes)
            {
                String s = "[ ";

                foreach (Node n in nodes)
                {
                    s += (n.id + " ");
                }

                s += "]";

                Console.WriteLine(s);
            }
        }
    }
}
