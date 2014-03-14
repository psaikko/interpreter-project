using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterpreterProject.Errors
{
    class MiniPL_DivideByZeroException : Exception
    {
        RuntimeError error;
        public RuntimeError Error
        {
            get { return error; }
            set { }
        }

        public MiniPL_DivideByZeroException(RuntimeError error)
            : base(error.GetMessage())
        {
            this.error = error;
        }
    }
}
