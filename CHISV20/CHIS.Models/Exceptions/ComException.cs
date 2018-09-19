using System;
using System.Collections.Generic;
using System.Text;

namespace CHIS
{
    /// <summary>
    /// 通用错误
    /// </summary>
    public class ComException : Exception
    {
        public string ExceptionCode { get; set; }
        public ComException(ExceptionTypes exTypes, string msg,Exception innerException=null) : base(msg,innerException)
        {
            ExceptionCode = exTypes.ToString();
        }


    }
    public class ComExceptionModel
    {
        public ComExceptionModel(Exception ex)
        {
            if (ex is ComException)
            {
                var cex = (ComException)ex;
                ErrorCode = cex.ExceptionCode;
                Msg = cex.Message;
            }
            else
            {
                ErrorCode = ExceptionTypes.Error_System.ToString();
                Msg = ex.Message;
            }
        }
        public bool Rlt { get; set; } = false;
        public string ErrorCode { get; set; }

        public string Msg { get; set; }
    }
    /// <summary>
    /// 系统通用错误
    /// </summary>
    public class SysComException : ComException
    {
        public SysComException(string msg) : base(ExceptionTypes.Error_System, msg) { }
    }
    public class OkComException : ComException
    {
        public OkComException(string msg) : base(ExceptionTypes.Ok_Reprocess, msg) { }
    }
    public class BeThrowComException : ComException
    {
        public BeThrowComException(string msg) : base(ExceptionTypes.Error_BeThorw, msg) { }
    }
    public class UnvalidComException : ComException
    {
        public UnvalidComException(string msg) : base(ExceptionTypes.Error_Unvalid, msg) { }
    }
    public class UnexistedComException : ComException
    {
        public UnexistedComException(string msg) : base(ExceptionTypes.Error_Unexisted, msg) { }
    }
    /// <summary>
    /// 错误类别
    /// </summary>
    public enum ExceptionTypes
    {
        /// <summary>
        /// 系统错误
        /// </summary>
        Error_System = 0,
        /// <summary>
        /// 未通过数据验证
        /// </summary>
        Error_Unvalid = 1,
        /// <summary>
        /// 主动抛错
        /// </summary>
        Error_BeThorw = 2,
        /// <summary>
        /// 没有通过权限验证
        /// </summary>
        Error_Unauthorized=3,
        /// <summary>
        /// 数据不存在
        /// </summary>
        Error_Unexisted=4,
        /// <summary>
        /// 重复处理
        /// </summary>
        Ok_Reprocess = 9
    }
}
