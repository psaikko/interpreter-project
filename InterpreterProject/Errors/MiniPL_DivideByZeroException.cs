using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.Errors
{
    class MiniPL_DivideByZeroException : Exception
    {
        public RuntimeError err;
        public MiniPL_DivideByZeroException(RuntimeError err) : base(err.GetMessage())
        {
            this.err = err;
        }
    }
}
