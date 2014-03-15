using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.LexicalAnalysis
{
    // Compares the contents of sets so they can be used as dictionary keys
    // Used in the NFA->DFA conversion in Regex.cs
    class SetEqualityComparer<T> : IEqualityComparer<ISet<T>>
    {
        public bool Equals(ISet<T> x, ISet<T> y)
        {
            return x.SetEquals(y);
        }

        public int GetHashCode(ISet<T> obj)
        {
            int hash = 0;
            foreach (T t in obj)
            {
                hash = (hash + t.GetHashCode() % 15485867) % 15485867;
            }
            return hash;
        }
    }
}
