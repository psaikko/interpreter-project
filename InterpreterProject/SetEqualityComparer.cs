using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
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
                hash = (hash + t.GetHashCode()) % 7919;
            }
            return hash;
        }    
    }
}
