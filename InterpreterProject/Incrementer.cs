using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    class Incrementer : IFunction
    {
        private Counter c;
        public Incrementer(Counter c)
        {
            this.c = c;
        }

        public void Call()
        {
            Console.WriteLine('+');
            c.value += 1;
        }
    }
}
