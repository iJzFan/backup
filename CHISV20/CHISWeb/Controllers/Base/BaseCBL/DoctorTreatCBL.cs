
using CHIS.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS.Controllers
{
    public class DoctorTreatCBL : BaseCBL
    {
        public DoctorTreatCBL(DbContext.CHISEntitiesSqlServer dbContext, Models.UserSelf sysUserInfos) : base(dbContext, sysUserInfos)
        {
        }
        /// <summary>
        /// 根据挂号，获取一个就诊全信息
        /// </summary> `
        public Models.ViewModels.DoctorTreatViewModel GetNewTreatInfo(long? registerid, long? treatid)
        {

            var registerInfo = _db.CHIS_Register.FirstOrDefault(m => m.RegisterID == registerid);
            var treatmodel = _db.CHIS_DoctorTreat.FirstOrDefault(m => m.TreatId == treatid);
            if (treatmodel == null) treatmodel = _db.CHIS_DoctorTreat.FirstOrDefault(m => m.RegisterID == registerid);//保障接诊和挂号唯一
            //如果必须要挂号，则在这里做判断
            // if (registerInfo == null) throw new Exception("没有发现挂号信息");

            //获取就诊人员信息
            Models.CHIS_Code_Customer cus = null;
            if (treatmodel != null) cus = _db.CHIS_Code_Customer.FirstOrDefault(m => m.CustomerID == treatmodel.CustomerId);//如果已经接诊，则返回的是接诊的人员信息
            else if (registerInfo != null) cus = _db.CHIS_Code_Customer.FirstOrDefault(m => m.CustomerID == registerInfo.CustomerID);
            else throw new Exception("没有找到就诊人员信息");
            //获取人员健康信息
            var cus_h = _db.CHIS_Code_Customer_HealthInfo.FirstOrDefault(m => m.CustomerId == cus.CustomerID);
            if (treatmodel != null && treatmodel.TreatStatus == 1) // 是在诊则重新设置时间
            {
                treatmodel.TreatTime = DateTime.Now;
                _db.SaveChanges();
            }

            //如果接诊信息为空，则初始化接诊信息 包括插入初始化接诊信息到数据库
            if (treatmodel == null) treatmodel = _db.Add(new CHIS_DoctorTreat()
            {
                Height = cus_h?.Height,
                Weight = cus_h?.Weight,
                TreatStatus = 0,
                CustomerId = cus.CustomerID,
                DoctorId = CurrentOper.DoctorId,
                RegisterID = registerid,
                StationId = CurrentOper.StationId,
                TreatTime = DateTime.Now,
                FirstTreatTime = DateTime.Now,
                Department = CurrentOper.SelectedDepartmentId
            }).Entity;

            //设置人员健康的默认值
            if (!treatmodel.Height.HasValue) treatmodel.Height = cus_h?.Height;
            if (!treatmodel.Weight.HasValue) treatmodel.Weight = cus_h?.Weight;

            //一体机信息
            Models.CHIS_DataInput_OneMachine oneMachineData = null;
            if (treatid > 0) oneMachineData = _db.CHIS_DataInput_OneMachine.Where(m => m.TreatId == treatid).AsNoTracking().FirstOrDefault();

            //返回展示模型数据 包括 1挂号信息 2病人信息 3接诊信息
            return new Models.ViewModels.DoctorTreatViewModel()
            {
                CustomerRegist = registerInfo,
                Customer = cus,
                CHIS_DoctorTreat = treatmodel,
                Doctor = _db.CHIS_Code_Doctor.FirstOrDefault(m => m.DoctorId == treatmodel.DoctorId),
                OneMachineData = oneMachineData,
                CustomerHealthInfo = _db.CHIS_Code_Customer_HealthInfo.AsNoTracking().FirstOrDefault(m => m.CustomerId == cus.CustomerID)
            };
        }
    }
}
