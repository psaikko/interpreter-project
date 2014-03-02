using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject
{
    class Decrementer : IFunction
    {
        private Counter c;
        public Decrementer(Counter c)
        {
            this.c = c;
        }

        public void Call()
        {
            Console.WriteLine('-');
            c.value -= 1;
        }
    }
}
