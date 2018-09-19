using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ah
{

    public class ChkException : Exception
    {
        public string ExceptionCode { get; set; }

        public ChkException(string exceptionCode, string message) : base(message)
        {
            this.ExceptionCode = exceptionCode;
        }
    }

 
}
