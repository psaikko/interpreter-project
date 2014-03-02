using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    class Counter : IState
    {
        public int value = 0;

        public bool Check()
        {
            Console.WriteLine(value);
            return value == 0;
        }


        public void Reset()
        {
            value = 0;
        }
    }
}
