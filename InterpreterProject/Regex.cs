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

        public static Regex Union(String s)
        {
            Regex re = None();
            for (int i = 0; i < s.Length; i++)
                re = re.Union(Character(s[i]));
            return re;
        }

        public static Regex Range(char a, char b)
        {
            Regex re = None();
            for (char c = a; c <= b; c++)
                re = re.Union(Character(c));
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

        public static Regex Empty()
        {
            Node start = new Node();
            Node end = new Node();
            start.epsilonTransitions.Add(end);
            return new Regex(start, end);
        }

        public static Regex Concat(String s)
        {
            Regex re = Empty();
            for (int i = 0; i < s.Length; i++)
                re = re.Concat(Regex.Character(s[i]));
            return re;
        }

        public static Regex Not(params char[] cs)
        {
            Regex re = None();
            for (char a = (char)1; a < 256; a++)
            {
                if (!cs.Contains(a))
                {
                    re = re.Union(Character(a));
                }
            }
            return re;
        }

        public static Regex Any()
        {
            Regex re = None();
            for (char a = (char)1; a < 256; a++)
            {
                re = re.Union(Character(a));
            }
            return re;
        }

        public Regex Concat(Regex other)
        {
            this.end.epsilonTransitions.Add(other.start);
            return new Regex(this.start, other.end);
        }

        public Regex Star()
        {
            Node start = new Node();
            Node end = new Node();
            this.end.epsilonTransitions.Add(this.start);
            start.epsilonTransitions.Add(this.start);
            start.epsilonTransitions.Add(end);
            this.end.epsilonTransitions.Add(end);
            return new Regex(start, end);
        }

        public Regex Plus()
        {
            // like star, but need to go through r at least once
            Node start = new Node();
            Node end = new Node();
            this.end.epsilonTransitions.Add(this.start);
            start.epsilonTransitions.Add(this.start);
            this.end.epsilonTransitions.Add(end);
            return new Regex(start, end);
        }

        public Regex Maybe()
        {
            // like star, but can't repeat r
            Node start = new Node();
            Node end = new Node();
            start.epsilonTransitions.Add(this.start);
            start.epsilonTransitions.Add(end);
            this.end.epsilonTransitions.Add(end);
            return new Regex(start, end);
        }

        public Regex Union(Regex other)
        {
            Node start = new Node();
            Node end = new Node();
            start.epsilonTransitions.Add(this.start);
            start.epsilonTransitions.Add(other.start);
            this.end.epsilonTransitions.Add(end);
            other.end.epsilonTransitions.Add(end);
            return new Regex(start, end);
        }

        public void DefineTokenClass(TokenClass tokenClass)
        {
            this.end.tokenClass = tokenClass;
        }

        /*
         * Do a BFS of the underlying automaton and print node data
         * For debugging purposes
         */
        public override string ToString()
        {
            String s = "";
            ISet<Node> visited = new HashSet<Node>();
            Queue<Node> q = new Queue<Node>();
            q.Enqueue(start);
            visited.Add(start);
            while (q.Count != 0)
            {
                Node current = q.Dequeue();
                
                s += "ID " + current.id + " Transitions: { ";
                foreach (char c in current.transitions.Keys)
                {
                    Node next = current.transitions[c];
                    s += c + ":" + next.id + " ";
                    if (!visited.Contains(next))
                    {
                        q.Enqueue(next);
                        visited.Add(next);
                    }
                        
                }
                s += "}, Eps: { ";
                foreach (Node next in current.epsilonTransitions)
                {
                    s += next.id + " ";
                    if (!visited.Contains(next))
                    {
                        q.Enqueue(next);
                        visited.Add(next);
                    }
                }
                s += "}";
                if (current.tokenClass != null)
                    s += ", Token: " + current.tokenClass.name;
                s += "\n";
            }
            return s;
        }

        /*
         * Make a DFA-ish automaton from the regex
         */
        public DFA ConstructDFA()
        {
            Dictionary<ISet<Node>, DFA.State> DFAStates = 
                new Dictionary<ISet<Node>, DFA.State>(new SetEqualityComparer<Node>());
            DFA.State DFAStartState = new DFA.State();
            ISet<Node> NFAStartStates = start.EpsilonMove();
            DFAStates.Add(NFAStartStates, DFAStartState);

            Stack<ISet<Node>> s = new Stack<ISet<Node>>();
            s.Push(NFAStartStates);
            
            while (s.Count != 0)
            {
                ISet<Node> currentNFAStates = s.Pop();
                DFA.State currentDFAState = DFAStates[currentNFAStates];
                ISet<char> nextChars = new HashSet<char>();
                
                // find which characters we can move with from currentNFAStates
                foreach (Node n in currentNFAStates)
                    nextChars.UnionWith(n.transitions.Keys);

                foreach (char c in nextChars)
                {
                    ISet<Node> nextNFAStates = Node.Move(currentNFAStates, c);

                    if (nextNFAStates.Count != 0)
                    {
                        HashSet<Node> epsilonClosure = new HashSet<Node>();
                        epsilonClosure.UnionWith(nextNFAStates);
                        epsilonClosure.UnionWith(Node.EpsilonMove(nextNFAStates, cached: true));
                        
                        DFA.State nextDFAState;

                        if (!DFAStates.TryGetValue(epsilonClosure, out nextDFAState))
                        {
                            nextDFAState = new DFA.State();

                            foreach (Node n in epsilonClosure)
                                if (n.tokenClass != null)
                                    nextDFAState.recognizedTokens.Add(n.tokenClass);

                            DFAStates.Add(epsilonClosure, nextDFAState);
                            s.Push(epsilonClosure);
                        }
                   
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
                calls++;
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

            public static int calls = 0;

            /*
             * During the DFA construction process the structure will remain
             * unchanged, but EpsilonMove will get called many times for any 
             * particular node so we cache the result as it is an expensive
             * operation.
             */
            private ISet<Node> cachedEpsilonMove = null;
            public ISet<Node> EpsilonMove(bool cache)
            {
                if (cache)
                {
                    if (cachedEpsilonMove == null)
                        cachedEpsilonMove = EpsilonMove();
                    
                       
                    return cachedEpsilonMove;
                }
                return EpsilonMove();
            }

            public static ISet<Node> EpsilonMove(ICollection<Node> nodes, bool cached = false)
            {
                HashSet<Node> reachable = new HashSet<Node>();
                foreach (Node node in nodes)
                    reachable.UnionWith(node.EpsilonMove(cached));
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
