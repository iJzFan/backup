using CHIS.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Ass;
using Microsoft.EntityFrameworkCore;
using CHIS.Models;
using CHIS.Models.ViewModel;
using CHIS.DbContext;
using CHIS.Models.StatisticsModels;
using CHIS.Models.DataModel;
using Microsoft.EntityFrameworkCore.Internal;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;

namespace CHIS.Services
{
    /// <summary>
    /// 处方管理类服务
    /// </summary>
    public class LoginService : BaseService
    {
        CustomerService _cusSvr;
        DoctorService _docSvr;
        WorkStationService _staSvr;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="db"></param>
        public LoginService(CHISEntitiesSqlServer db
             , CustomerService cusSvr
             , DoctorService docSvr
             , WorkStationService staSvr
            ) : base(db)
        {
            _cusSvr = cusSvr; _docSvr = docSvr; _staSvr = staSvr;
        }

        //===============================================================================



        #region 辅助登录
        /// <summary>
        /// 是否需要辅助登录
        /// </summary>
        /// <param name="loginId">主登录Id</param>
        /// <returns></returns>
        public bool IsNeedLoginExt(long loginId)
        {
            var login = _db.CHIS_Sys_Login.Find(loginId);
            if (login == null) return false;
            return login.NeedLoginExt == true;
        }

        /// <summary>
        /// 获取用户可登陆的工作站
        /// </summary>
        /// <param name="mobileNumber">手机号</param>
        /// <returns></returns>
        public IEnumerable<StationInfo2> GetUserCanLoginStation(string mobileNumber, bool onlyDrugStore = false)
        {
            if (mobileNumber.IsEmpty()) throw new UnvalidComException("没有填写手机号");
            var doctorId = (_db.vwCHIS_Code_Doctor.AsNoTracking().SingleOrDefault(m => m.CustomerMobile == mobileNumber)?.DoctorId) ?? 0;
            var finds = _db.CHIS_Sys_Rel_DoctorStations.AsNoTracking().Where(m => m.DoctorId == doctorId && m.StationIsEnable).Select(m => m.StationId).ToList();
            // var rtn = _staSvr.QueryViewWorkStation().AsNoTracking();

            var rtn = from _a in _db.CHIS_Code_WorkStation.AsNoTracking()
                      join b in _db.CHIS_Code_WorkStation.AsNoTracking() on _a.ParentStationID ?? 0 equals b.StationID into g
                      from _p in g.DefaultIfEmpty()
                      join c in _db.SYS_ChinaArea.AsNoTracking() on _a.AreaID ?? 0 equals c.AreaId into garea
                      from _z in garea.DefaultIfEmpty()
                      join d in _db.CHIS_Code_Dict_Detail.AsNoTracking() on _a.StationTypeId ?? 0 equals d.DetailID into gtype
                      from _d0 in gtype.DefaultIfEmpty()
                      where _a.IsEnable && finds.Contains(_a.StationID)
                      select new StationInfo2
                      {
                          StationId = _a.StationID,
                          StationName = _a.StationName,
                          StationRmk = _a.StationRmk,
                          StationPicHUrl = _a.StationPicH.ahDtUtil().GetStationImg(imgSizeTypes.HorizNormal),
                          StationPicVUrl = _a.StationPic.ahDtUtil().GetStationImg(imgSizeTypes.VerticalNormal),
                          StationAddress = _z.MergerName + _a.Address,
                          StationTypeName = _d0.ItemName,
                          StationTypeId = _a.StationTypeId,
                          Lat = _a.Lat,
                          Lng = _a.Lng
                      };
            if (onlyDrugStore) rtn = rtn.Where(m => m.StationTypeId == CHIS.DictValues.StationType.k_StationType_drugstore2);
            return rtn;
        }

        /// <summary>
        /// 用户登录到工作站 返回基本信息
        /// </summary>
        /// <param name="mobileNumber"></param>
        /// <param name="code"></param>
        /// <param name="stationId"></param>
        /// <param name="loginType"></param>
        /// <returns></returns>
        internal UserSelfEx UserLoginStation(string mobileNumber, string code, int stationId, string loginType)
        {
            var rtn = new UserSelfEx();
            var login = _db.vwCHIS_Sys_Login.AsNoTracking().SingleOrDefault(m => m.Mobile == mobileNumber);

            //登录系统
            if (loginType == "PWD")
            {
                if (login.LoginPassword != code) throw new ComException(ExceptionTypes.Error_Unauthorized, "密码验证失败");
            }
            if (loginType == "CODE")
            {
                 
            }

            var cus = _db.vwCHIS_Code_Customer.Find(login.CustomerId);
            var dctr = _db.vwCHIS_Code_Doctor.Find(login.DoctorId);
            var sta = _db.vwCHIS_Code_WorkStation.Find(stationId);

            rtn.LoginId = login.LoginId;
            rtn.OpId = cus.CustomerID;
            rtn.OpMan = cus.CustomerName;
            rtn.Gender = cus.Gender;
            rtn.Birthday = cus.Birthday ?? DateTime.Today;
            rtn.StationId = stationId;
            rtn.DoctorId = dctr.DoctorId;
            rtn.PostTitleName = dctr.PostTitleName;
            rtn.PhotoUrlDef = dctr.PhotoUrlDef;
            rtn.DoctorAppId = dctr.DoctorAppId;
            rtn.StationName = sta.StationName;
            rtn.StationTypeId = sta.StationTypeId ?? 0;
            rtn.DrugStoreStationId = sta.DrugStoreStationId ?? stationId;
            rtn.LoginTime = DateTime.Now;
            rtn.IsCanTreat = sta.IsCanTreat;
            rtn.IsManageUnit = sta.IsManageUnit;

            /*
         
            claimsAdd("MyAllowStationIds", dd.MyAllowStationIds);
            claimsAdd("MySonStations", dd.MySonStations);

            claimsAdd("SelectedDepartmentId", dd.SelectedDepartmentId);//选择的部门    
            claimsAdd("SelectedDepartmentName", dd.SelectedDepartmentName);

            claimsAdd("MyRoleIds", dd.MyRoleIds);
            claimsAdd("MyRoleNames", dd.MyRoleNames);


            //辅助登录
            claimsAdd("LoginExtId", dd.LoginExtId);
            claimsAdd("LoginExtMobile", dd.LoginExtMobile);
            claimsAdd("LoginExtName", dd.LoginExtName);
            claimsAdd("LoginExtFuncKeys", dd.LoginExtFuncKeys); 
             
             */

            //增加项目，不在令牌内
            rtn.DoctorPicUrl = dctr.PhotoUrlDef.ahDtUtil().GetDoctorImg(dctr.Gender);

            return rtn;
        }

        /// <summary>
        /// 查找LoginExt
        /// </summary>
        /// <param name="loginExtId"></param>
        /// <returns></returns>
        public CHIS_Sys_LoginExt GetLoginExt(long loginExtId)
        {
            return _db.CHIS_Sys_LoginExt.Find(loginExtId);
        }



        /// <summary>
        /// 查找辅助登录信息
        /// </summary>
        /// <param name="mobile">手机</param>
        /// <param name="parentLoginId">分店登录Id</param>
        /// <returns></returns>
        public CHIS_Sys_LoginExt GetLoginExt(string mobile, long parentLoginId)
        {
            return _db.CHIS_Sys_LoginExt.FirstOrDefault(m => m.LoginExtParentLoginId == parentLoginId && m.LoginExtMobile == mobile);
        }


        /// <summary>
        ///修改辅助登录信息
        /// </summary>
        /// <param name="am"></param>
        public void ModifyLoginExt(CHIS_Sys_LoginExt am)
        {
            var find = _db.CHIS_Sys_LoginExt.Find(am.LoginExtId);
            find.LoginExtEnabled = am.LoginExtEnabled;
            find.LoginExtMobile = am.LoginExtMobile;
            find.LoginExtName = am.LoginExtName;
            if (am.LoginExtPassword.IsNotEmpty()) find.LoginExtPassword = am.LoginExtPassword;
            find.LoginExtRoleKeys = am.LoginExtRoleKeys;
            _db.SaveChanges();
        }
        /// <summary>
        /// 添加辅助登录信息
        /// </summary>
        /// <param name="am"></param>
        public CHIS_Sys_LoginExt AddLoginExt(CHIS_Sys_LoginExt am)
        {
            am.sysCreateTime = DateTime.Now;
            am = _db.CHIS_Sys_LoginExt.Add(am).Entity;
            _db.SaveChanges();
            return am;
        }

        /// <summary>
        /// 设置可用、禁用
        /// </summary>
        /// <param name="loginExtId"></param>
        /// <param name="bEnable"></param>
        /// <returns></returns>
        public bool SetLoginExtEnable(long loginExtId, bool bEnable)
        {
            _db.CHIS_Sys_LoginExt.Find(loginExtId).LoginExtEnabled = bEnable;
            _db.SaveChanges();
            return true;
        }

        /// <summary>
        /// 获取该分店所有的登录账号信息
        /// </summary>
        /// <param name="parentLoginId">分店登录Id</param>
        /// <returns></returns>
        public IQueryable<CHIS_Sys_LoginExt> GetLoginExtsOfThis(long parentLoginId)
        {
            return _db.CHIS_Sys_LoginExt.AsNoTracking().Where(m => m.LoginExtParentLoginId == parentLoginId).OrderByDescending(m => m.sysCreateTime);
        }

        /// <summary>
        /// 获取登录授权的功能项目
        /// </summary>
        /// <param name="loginExitId"></param>
        /// <returns></returns>
        public IQueryable<CHIS_Sys_LoginExt_Rel_RoleFunc> GetLoginExtFuncs(long loginExitId)
        {
            return _db.CHIS_Sys_LoginExt_Rel_RoleFunc.FromSql($"exec sp_Sys_GetLoginExtFuncCodes {loginExitId}");
        }
        /// <summary>
        /// 获取登录授权的功能项目Key表
        /// </summary>
        /// <param name="loginExitId"></param>
        /// <returns></returns>
        public string GetLoginExtFuncKeys(long loginExitId)
        {
            var ss = GetLoginExtFuncs(loginExitId).Select(m => m.LoginExtRoleFuncKey).ToArray();
            return string.Join(',', ss);
        }

        /// <summary>
        /// 获取工作站和药店的信息
        /// </summary>
        /// <param name="stationId"></param>
        /// <param name="doctorId"></param>
        /// <returns></returns>
        public StationStoreInfo GetStationStoreInfo(int stationId, int doctorId)
        {
            var station = _db.vwCHIS_Code_WorkStation.Find(stationId);
            var doctor = _db.vwCHIS_Code_Doctor.Find(doctorId);

            return new StationStoreInfo
            {
                StationId = station.StationID,
                StationName = station.StationName,
                StationLogPicH = station.StationLogPicH.GetUrlPath(Global.ConfigSettings.StationImagePathRoot),
                StationLogPicV = station.StationLogPicV.GetUrlPath(Global.ConfigSettings.StationImagePathRoot),
                StoreName = doctor.DoctorName,
                DoctorId = doctor.DoctorId
            };
        }

        /// <summary>
        /// 检测登录密码
        /// </summary>
        /// <param name="parentLoginId">分店登录Id</param>
        /// <param name="loginExtMobile">手机号</param>
        /// <param name="loginExtPwd">密码</param>
        /// <returns></returns>
        public bool CheckLoginExtPwd(long parentLoginId, string loginExtMobile, string loginExtPwd, out Exception ex)
        {
            ex = null;
            if (loginExtMobile.IsEmpty()) { ex = new Exception("没有传入用户手机号"); return false; }
            if (loginExtPwd.IsEmpty()) { ex = new Exception("没有输入密码"); return false; }
            var s = GetLoginExt(loginExtMobile, parentLoginId);
            if (s == null) { ex = new Exception("没有发现辅助登录账户"); return false; }
            if (s.LoginExtPassword == loginExtPwd) return true;
            return false;
        }

        /// <summary>
        /// 获取辅助登录的角色
        /// </summary>
        /// <param name="loginRoleKeys"></param>
        /// <returns></returns>
        public IEnumerable<CHIS_Sys_LoginExt_Role> GetLoginExtRols(string loginRoleKeys)
        {

            string[] rolekeys = loginRoleKeys.Split(',');
            return _db.CHIS_Sys_LoginExt_Role.AsNoTracking().Where(m => rolekeys.Contains(m.LoginExtRoleKey));
        }

        /// <summary>
        /// 获取可选择的角色
        /// </summary>
        /// <param name="stationId"></param>
        /// <param name="loginId"></param>
        /// <returns></returns>
        public IEnumerable<CHIS_Sys_LoginExt_Role> GetCanSelectRoles(int stationId, long loginId)
        {
            var lst = _db.CHIS_Sys_LoginExt_Rel_LoginAllowRoles.AsNoTracking().Where(m => m.StationId == stationId && m.RoleIsEnable).ToList();
            var roles0 = lst.Where(m => m.LoginId == null);
            var roles1 = lst.Where(m => m.LoginId == loginId);
            var selrolekeys = roles0.Concat(roles1).Select(m => m.RoleKey);
            return _db.CHIS_Sys_LoginExt_Role.AsNoTracking().Where(m => selrolekeys.Contains(m.LoginExtRoleKey));
        }


        /// <summary>
        /// 获取该角色的详细功能信息
        /// </summary>
        /// <param name="roleKey"></param>
        /// <returns></returns>
        public IEnumerable<CHIS_Sys_LoginExt_Rel_RoleFunc> GetRoleFuncs(string roleKey)
        {
            return _db.CHIS_Sys_LoginExt_Rel_RoleFunc.AsNoTracking().Where(m => m.LoginExtRoleKey == roleKey);
        }

        /// <summary>
        /// 获取审查角色的辅助账户
        /// </summary>
        /// <param name="stationId"></param>
        /// <param name="parentLoginId"></param>
        /// <returns></returns>
        public IEnumerable<CHIS_Sys_LoginExt> GetLoginExtCanCheckOfStore(int stationId, long parentLoginId)
        {
            return GetLoginExtOfStoreByFuncKey(stationId, parentLoginId, "RxCheck");
        }
        /// <summary>
        /// 获取具有对应功能键权限的辅助登录账户
        /// </summary>
        /// <param name="stationId"></param>
        /// <param name="parentLoginId"></param>
        /// <param name="funcKey"></param>
        /// <returns></returns>
        public IEnumerable<CHIS_Sys_LoginExt> GetLoginExtOfStoreByFuncKey(int stationId, long parentLoginId, string funcKey)
        {
            return _db.CHIS_Sys_LoginExt.FromSql(string.Format("exec sp_GetLoginExtByFuncKey {0},{1},'{2}'", stationId, parentLoginId, funcKey));
        }



        #endregion


        #region 登录的工作站信息



        #endregion



    }
}
