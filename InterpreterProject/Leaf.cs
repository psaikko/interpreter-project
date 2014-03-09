using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    public class Leaf<T> : INode<T>
    {
        private T value;
        public Leaf(T value) { this.value = value; }
        public T GetValue() { return value; }

        public override string ToString()
        {
            return value.ToString();
        }
    }
}
