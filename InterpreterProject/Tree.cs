using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    public class Tree<T> : INode<T>
    {
        public List<INode<T>> children = new List<INode<T>>();
        public T value;
        public Tree(T value) { this.value = value; }
        public T GetValue() { return value; }

        public override string ToString()
        {
            String s = "";

            Stack<INode<T>> nodeStack = new Stack<INode<T>>();
            Stack<int> depthStack = new Stack<int>();
            nodeStack.Push(this);
            depthStack.Push(0);

            while (nodeStack.Count > 0)
            {
                INode<T> node = nodeStack.Pop();
                int depth = depthStack.Pop();
                string indent = "";
                for (int i = 0; i < depth; i++) indent += "  ";
                if (node is Leaf<T>)
                {
                    s += indent + node.ToString() + '\n';
                }
                else
                {
                    Tree<T> t = node as Tree<T>;
                    s += indent + t.value.ToString() + '\n';
                    for (int i = t.children.Count - 1; i >= 0; i--)
                    {
                        INode<T> next = t.children[i];
                        nodeStack.Push(next);
                        depthStack.Push(depth + 1);
                    }
                }
            }

            return s;
        }

        public int SymbolCount(Predicate<T> pred)
        {
            int count = 0;
            Stack<INode<T>> nodeStack = new Stack<INode<T>>();
            nodeStack.Push(this);
            while (nodeStack.Count > 0)
            {
                INode<T> current = nodeStack.Pop();
                if (pred(current.GetValue()))
                    count++;
                if (current is Tree<T>)
                {
                    Tree<T> subtree = current as Tree<T>;
                    foreach (INode<T> child in subtree.children)
                        nodeStack.Push(child);
                }
            }
            return count;
        }

        public bool DepthContains(int depth, Predicate<T> pred)
        {
            Stack<INode<T>> nodeStack = new Stack<INode<T>>();
            Stack<int> depthStack = new Stack<int>();
            nodeStack.Push(this);
            depthStack.Push(0);
            while (nodeStack.Count > 0)
            {
                INode<T> currentNode = nodeStack.Pop();
                int currentDepth = depthStack.Pop();
                if (currentDepth < depth)
                {
                    if (currentNode is Tree<T>)
                    {
                        Tree<T> subtree = currentNode as Tree<T>;
                        foreach (INode<T> child in subtree.children)
                        {
                            nodeStack.Push(child);
                            depthStack.Push(currentDepth + 1);
                        }
                    }
                }
                else if (currentDepth == depth)
                    if (pred(currentNode.GetValue()))
                        return true;
            }
            return false;
        }

        public void RemoveNodes(Predicate<INode<T>> pred)
        {
            Stack<Tree<T>> treeStack = new Stack<Tree<T>>();

            while (treeStack.Count > 0)
            {
                Tree<T> currentNode = treeStack.Pop();
                List<INode<T>> pruneList = new List<INode<T>>();
                List<List<INode<T>>> replaceList = new List<List<INode<T>>>();

                foreach (INode<T> node in currentNode.children)
                {
                    if (pred(node))
                    {
                        pruneList.Add(node);
                        if (node is Tree<T>)
                            replaceList.Add((node as Tree<T>).children);
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
                    foreach (INode<T> node in currentNode.children)
                        if (node is Tree<T>)
                            treeStack.Push(node as Tree<T>);
                }
            }
        }

        public void RemoveNodesByValue(Predicate<T> pred)
        {
            RemoveNodes(n => pred(n.GetValue()));
        }

        public IEnumerable<INode<T>> Nodes()
        {
            Stack<INode<T>> nodeStack = new Stack<INode<T>>();
            nodeStack.Push(this);
            while (nodeStack.Count > 0)
            {
                INode<T> currentNode = nodeStack.Pop();
                yield return currentNode;
                
                if (currentNode is Tree<T>)
                    foreach (INode<T> child in (currentNode as Tree<T>).children)
                        nodeStack.Push(child);
            }
        }
    }
}
