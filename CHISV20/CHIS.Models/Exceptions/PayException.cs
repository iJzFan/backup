using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIS
{
    public class SuccessedException : OkComException
    {
        public SuccessedException():base("已经成功过了") { }
        public SuccessedException(string message) : base(message)
        {
           
        }
    }

    /// <summary>
    /// 不需要处理的错误抛出
    /// </summary>
    public class NotProcessException : Exception
    {
        public NotProcessException() { }
        public NotProcessException(string message) : base(message)
        {

        }
    }

    public class PayException : Exception
    {
        public int ExceptionId { get; set; }

        public PayException(int exceptionId, string message) : base(message)
        {
            this.ExceptionId = exceptionId;
        }
    }

    public class CodeException : Exception
    {
        public string ExceptionCode { get; set; }
        public CodeException(string exceptionCode, string message) : base(message)
        {
            this.ExceptionCode = exceptionCode;
        }
        public CodeException() { }
    }
    public class CodeIdException : Exception
    {
        public string ExceptionCode { get; set; }
        public int ExceptionId { get; set; }
        public CodeIdException(string exceptionCode,int exceptionId, string message) : base(message)
        {
            this.ExceptionCode = exceptionCode;
            this.ExceptionId = exceptionId;
        }
        
    }
    /// <summary>
    /// 订单号重复错误
    /// </summary>
    public class PayOrderSameException : Exception {
        public string ExceptionCode { get; set; }
        public PayOrderSameException(string exceptionCode, string message) : base(message)
        {
            this.ExceptionCode = exceptionCode;
        }
        public PayOrderSameException() { }

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
