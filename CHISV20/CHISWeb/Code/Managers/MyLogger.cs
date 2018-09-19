using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS.Code.Managers
{
    public interface IMyLogger
    {
        Models.UserSelf UserInfos { get; set; }


        Task WriteLogAsync(string modelName, string procedureName, string logType, string msg, DateTime? logtime = null, int? opid = null, string opfullName = null, string keyId = "");
        void WriteLog(string logType, string msg, DateTime? logtime = null, int? opid = null, string opfullName = null, string keyId = "");
        Task WriteInfoAsync(string modelName, string procedureName, string infomsg, int? opid = null, string opfullName = null);
        void WriteInfo(string infomsg, int? opid = null, string opfullName = null);
        Task WriteSUCCESSAsync(string modelName, string procedureName, string infomsg, int? opid = null, string opfullName = null, string keyId = "");
        void WriteSUCCESS(string infomsg, int? opid = null, string opfullName = null, string keyId = "");
        Task WriteErrorAsync(string modelName, string procedureName, string errormsg, int? opid = null, string opfullName = null);
        void WriteError(string errormsg, int? opid = null, string opfullName = null);
        Task WriteErrorAsync(string modelName, string procedureName, Exception ex, int? opid = null, string opfullName = null);
        void WriteError(Exception ex, int? opid = null, string opfullName = null);
    }

    public class MyLogger : IMyLogger
    {
        DbContext.CHISLogEntitiesSqlServer db = new Code.Utility.DataBaseHelper().GetLogDbContext();

        Models.UserSelf _userInfos = null;
        public Models.UserSelf UserInfos { get { try { return _userInfos; } catch { return null; } } set { _userInfos = value; } }


        public async Task WriteLogAsync(string modelName, string procedureName, string logType, string msg, DateTime? logtime = null, int? opid = null, string opfullName = null, string keyId = "")
        {
            try
            {
                msg = Ass.P.PStr(msg);
                var MAX = msg.Length > 250 ? 250 : msg.Length;
                if (logtime == null) logtime = DateTime.Now;
                try
                {
                    if (opid == null) opid = UserInfos?.OpId;
                    if (opfullName == null) opfullName = UserInfos?.OpManFullMsg;
                }
                catch { }

                await db.CHIS_Sys_Logs.AddAsync(new Models.CHIS_Sys_Logs
                {
                    LogTime = logtime,
                    LogType = logType,
                    ModeName = modelName,
                    ProcedureName = procedureName,
                    Msg = msg.Substring(0, MAX),
                    OpFullName = opfullName,
                    OpId = opid,
                    KeyId = keyId
                });
                await db.SaveChangesAsync();
            }
            catch { }
        }



        public async Task WriteErrorAsync(string modelName, string procedureName, string errormsg, int? opid = null, string opfullName = null)
        {
            await WriteLogAsync(modelName, procedureName, "ERROR", errormsg, DateTime.Now, opid, opfullName);
        }
        public async Task WriteErrorAsync(string modelName, string procedureName, Exception ex, int? opid = null, string opfullName = null)
        {
            await WriteLogAsync(modelName, procedureName, "ERROR", ex.Message, DateTime.Now, opid, opfullName);
        }




        public async Task WriteInfoAsync(string modelName, string procedureName, string infomsg, int? opid = null, string opfullName = null)
        {
            await WriteLogAsync(modelName, procedureName, "INFO", infomsg, DateTime.Now, opid, opfullName);
        }

        public async Task WriteSUCCESSAsync(string modelName, string procedureName, string infomsg, int? opid = null, string opfullName = null, string keyId = "")
        {
            await WriteLogAsync(modelName, procedureName, "SUCCESS", infomsg, DateTime.Now, opid, opfullName);
        }




        private dynamic GetModelNameAndProcedureName(StackTrace trace)
        {
            string type = "UNKNOW", method = "UNKNOW";
            try
            {
                type = trace.GetFrame(0).GetMethod().DeclaringType.ReflectedType.Name.ToUpper().Replace("CONTROLLER", "");
                method = trace.GetFrame(0).GetMethod().DeclaringType.Name;
                int p0 = method.IndexOf('<');
                int p1 = method.IndexOf('>');
                if (p0 >= 0) method = method.Substring(p0 + 1, p1 - p0 - 1);
            }
            catch (Exception ex)
            {
                try
                {
                    type = trace.GetFrame(0).GetMethod().DeclaringType.Name;
                    method = trace.GetFrame(0).GetMethod().ToString();
                }
                catch (Exception ex2)
                {

                }
            }
            return new
            {
                m = type,
                p = method
            };
        }
        public void WriteLog(string logType, string msg, DateTime? logtime = null, int? opid = null, string opfullName = null, string keyId = "")
        {
            var a = GetModelNameAndProcedureName(new StackTrace(new StackFrame(1)));
            Task.Run(async () =>
            {
                await WriteLogAsync(a.m, a.p, logType, msg, logtime, opid, opfullName, keyId);
            });
        }

        public void WriteInfo(string infomsg, int? opid = null, string opfullName = null)
        {
            var a = GetModelNameAndProcedureName(new StackTrace(new StackFrame(1)));
            Task.Run(async () =>
            {
                await WriteInfoAsync(a.m, a.p, infomsg, opid, opfullName);
            });
        }

        public void WriteSUCCESS(string infomsg, int? opid = null, string opfullName = null, string keyId = "")
        {
            var a = GetModelNameAndProcedureName(new StackTrace(new StackFrame(1)));
            Task.Run(async () =>
            {
                await WriteSUCCESSAsync(a.m, a.p, infomsg, opid, opfullName);
            });
        }

        public void WriteError(string errormsg, int? opid = null, string opfullName = null)
        {
            var a = GetModelNameAndProcedureName(new StackTrace(new StackFrame(1)));
            Task.Run(async () =>
            {
                await WriteErrorAsync(a.m, a.p, errormsg, opid, opfullName);
            });
        }

        public void WriteError(Exception ex, int? opid = null, string opfullName = null)
        {
            var a = GetModelNameAndProcedureName(new StackTrace(new StackFrame(1)));
            Task.Run(async () =>
            {
                await WriteErrorAsync(a.m, a.p, ex, opid, opfullName);
            });
        }
    }
}
