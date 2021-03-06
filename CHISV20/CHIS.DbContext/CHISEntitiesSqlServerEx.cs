﻿


//------------------------------------------------------------------------------        
// <auto-generated>      
//     此代码由T4模板自动生成     
//	   生成时间 2017-10-26 14:33:09 by Rex         
//     对此文件的更改可能会导致不正确的行为，并且如果  
//     重新生成代码，这些更改将会丢失。     
// </auto-generated>   
//------------------------------------------------------------------------------  


using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Ass;
using CHIS.Models;
using CHIS.Models.ViewModels;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace CHIS.DbContext
{

    public partial class CHISEntitiesSqlServer
    {

        public virtual DbSet<_StationId> StationIds { get; set; }
        public virtual DbSet<FeeSumaryViewModel> FeeSumary { get; set; }

        #region 事务嵌套

        /// <summary>
        /// 开始事务
        /// </summary>
        public void BeginTransaction()
        {
            if (this.Database.CurrentTransaction == null)
            {
                this.Database.BeginTransaction();
            }
            this.BeginCounter++;
            this.IsTransaction = true;

        }

        /// <summary>
        /// 提交事务
        /// </summary>
        /// <returns></returns>
        public int CommitTran()
        {
            this.BeginCounter--;
            int result = this.SaveChanges();
            if (this.BeginCounter == 0)
            {
                this.IsTransaction = false;
                var transaction = this.Database.CurrentTransaction;
                if (transaction != null)
                {
                    transaction.Commit();
                    transaction.Dispose();
                    result += 1;
                }
            }
            return result;
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void RollbackTran(Exception ex=null)
        {
            this.BeginCounter--;
            if (this.BeginCounter == 0)
            {
                this.IsTransaction = false;
                var transaction = this.Database.CurrentTransaction;
                if (transaction != null)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                }
            }
            else
            {
                //this.BeginCounter = 1;
                var msg = "嵌套内部事务异常";
                if (ex != null) msg += ex.Message;
                throw new Exception(msg);
            }
        }

        private bool isTransaction = false;

        /// <summary>
        /// 是否是事务性操作
        /// </summary>
        public bool IsTransaction
        {
            get { return isTransaction; }
            set { this.isTransaction = value; }
        }

        private int beginCounter = 0;

        /// <summary>
        /// 事务计数器
        /// </summary>
        public int BeginCounter
        {
            get { return beginCounter; }
            set { this.beginCounter = value; }
        }

        #endregion



        public async Task<DataSet> QuerySqlAsync(string sql)
        {
            DataSet ds = null;
            SqlConnection conn = (SqlConnection)this.Connection;
            //using (SqlConnection conn = (SqlConnection)this.Connection)
            //{
            try
            {
                if (conn.State != ConnectionState.Open) await conn.OpenAsync();
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                ds = new DataSet();
                da.Fill(ds);
            }
            catch (Exception ex) { }
            //}
            return ds;
        }
        public async Task<DataTable> QueryTableAsync(string sql)
        {
            var rlt = await QuerySqlAsync(sql);
            return rlt.Tables[0];
        }

        public async Task<object> ExecuteScalarAsync(string sql)
        {
            object rtn = null;
            SqlConnection conn = (SqlConnection)this.Connection;
            if (conn.State != ConnectionState.Open) await conn.OpenAsync();
            SqlCommand cmd = new SqlCommand(sql, conn);
            rtn = await cmd.ExecuteScalarAsync();
            return rtn;
        }

        /// <summary>
        /// 获取数据库时间
        /// </summary>
        /// <returns></returns>
        public DateTime GetDBTime()
        {
            try
            {
                return (DateTime)(ExecuteScalarAsync("select getdate()").Result);
            }
            catch (Exception ex) { return DateTime.Now; }
        }
    }


    public class _StationId
    {
        [Key]
        public int StationId { get; set; }
    }
}


