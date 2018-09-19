using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ah
{

    public class PayException : Exception
    {
        public int ExceptionId { get; set; }

        public PayException(int exceptionId, string message) : base(message)
        {
            this.ExceptionId = exceptionId;
        }
    }

    public class RefundException : Exception
    {
        public string ExceptionCode { get; set; }

        public RefundException(string exceptionCode, string message) : base(message)
        {
            this.ExceptionCode = exceptionCode;
        }
    }
}
