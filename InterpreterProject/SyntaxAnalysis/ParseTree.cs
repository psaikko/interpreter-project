using InterpreterProject.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.SyntaxAnalysis
{
    public class ParseTree : IParseNode
    {
        public List<IParseNode> children = new List<IParseNode>();
        public Nonterminal var;
        public ParseTree(Nonterminal var) { this.var = var; }

        public override string ToString()
        {
            String s = "";

            Stack<IParseNode> nodeStack = new Stack<IParseNode>();
            Stack<int> depthStack = new Stack<int>();
            nodeStack.Push(this);
            depthStack.Push(0);

            while (nodeStack.Count > 0)
            {
                IParseNode node = nodeStack.Pop();
                int depth = depthStack.Pop();
                string indent = "";
                for (int i = 0; i < depth; i++) indent += "  ";
                if (node is ParseLeaf)
                {
                    s += indent + node.ToString() + '\n';
                }
                else
                {
                    ParseTree t = node as ParseTree;
                    s += indent + t.var.ToString() + '\n';
                    for (int i = t.children.Count - 1; i >= 0; i--)
                    {
                        IParseNode next = t.children[i];
                        nodeStack.Push(next);
                        depthStack.Push(depth + 1);
                    }
                }
            }

            return s;
        }

        public int SymbolCount(Predicate<IParseNode> pred)
        {
            int count = 0;
            Stack<IParseNode> nodeStack = new Stack<IParseNode>();
            nodeStack.Push(this);
            while (nodeStack.Count > 0)
            {
                IParseNode current = nodeStack.Pop();
                if (pred(current))
                    count++;
                if (current is ParseTree)
                {
                    ParseTree subtree = current as ParseTree;
                    foreach (IParseNode child in subtree.children)
                        nodeStack.Push(child);
                }
            }
            return count;
        }

        public bool DepthContains(int depth, Predicate<IParseNode> pred)
        {
            Stack<IParseNode> nodeStack = new Stack<IParseNode>();
            Stack<int> depthStack = new Stack<int>();
            nodeStack.Push(this);
            depthStack.Push(0);
            while (nodeStack.Count > 0)
            {
                IParseNode currentNode = nodeStack.Pop();
                int currentDepth = depthStack.Pop();
                if (currentDepth < depth)
                {
                    if (currentNode is ParseTree)
                    {
                        ParseTree subtree = currentNode as ParseTree;
                        foreach (IParseNode child in subtree.children)
                        {
                            nodeStack.Push(child);
                            depthStack.Push(currentDepth + 1);
                        }
                    }
                }
                else if (currentDepth == depth)
                    if (pred(currentNode))
                        return true;
            }
            return false;
        }

        public void RemoveNodes(Predicate<IParseNode> pred)
        {
            Stack<ParseTree> treeStack = new Stack<ParseTree>();

            treeStack.Push(this);


            while (treeStack.Count > 0)
            {
                ParseTree currentNode = treeStack.Pop();
                List<IParseNode> pruneList = new List<IParseNode>();
                List<List<IParseNode>> replaceList = new List<List<IParseNode>>();

                foreach (IParseNode node in currentNode.children)
                {
                    if (pred(node))
                    {
                        
                        pruneList.Add(node);
                        if (node is ParseTree)
                            replaceList.Add((node as ParseTree).children);
                        else
                            replaceList.Add(null);
                    }
                }

                bool backtrack = false; // if we remove a child that is a subtree

                for (int i = 0; i < pruneList.Count; i++)
                {
                    int index = currentNode.children.IndexOf(pruneList[i]);
                    currentNode.children.RemoveAt(index);
                    if (replaceList[i] != null)
                    {
                        backtrack = true;
                        currentNode.children.InsertRange(index, replaceList[i]);
                    }
                }

                if (backtrack)
                {
                    treeStack.Push(currentNode);
                }
                else
                {
                    foreach (IParseNode node in currentNode.children)
                        if (node is ParseTree)
                            treeStack.Push(node as ParseTree);
                }
            }
        }

        public IEnumerable<IParseNode> Nodes()
        {
            Stack<IParseNode> nodeStack = new Stack<IParseNode>();
            nodeStack.Push(this);
            while (nodeStack.Count > 0)
            {
                IParseNode currentNode = nodeStack.Pop();
                yield return currentNode;
                
                if (currentNode is ParseTree)
                {
                    ParseTree subtree = currentNode as ParseTree;
                    for (int i = subtree.children.Count - 1; i >= 0; i--)
                        nodeStack.Push(subtree.children[i]);
                }                        
            }
        }

        public ISymbol GetSymbol()
        {
            return var;
        }
    }
}
