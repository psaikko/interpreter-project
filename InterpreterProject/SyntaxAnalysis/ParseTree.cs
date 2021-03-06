﻿using InterpreterProject.SyntaxAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.SyntaxAnalysis
{
    // Implementation of a parse tree internal node
    public class ParseTree : IParseNode
    {
        List<IParseNode> children = new List<IParseNode>();
        public List<IParseNode> Children
        {
            get { return children; }
        }

        Nonterminal nonterminal;
        public Nonterminal Nonterminal
        {
            get { return nonterminal; }
        }

        public ParseTree(Nonterminal var) 
        { 
            this.nonterminal = var; 
        }

        // string representation of the parse tree, for debugging
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
                    s += indent + t.nonterminal.ToString() + '\n';
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

        // counts the number of nodes in the tree that satisfy some condition
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

        // checks if a given depth of the parse tree contains some node
        // satisfying a condition
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

        // Remove each node that satisfies some condition
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

        // Generator that returns every node in the parse tree
        // uses a depth first search
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
            return nonterminal;
        }
    }
}
