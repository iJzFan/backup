using Ass;
using CHIS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CHIS.Models.ViewModel;

namespace CHIS.Controllers
{
    public class DoctorCBL : BaseCBL
    {
        public DoctorCBL(BaseController c) : base(c) { }




        /// <summary>
        /// 默认上班情况，做假数据
        /// </summary>
        public List<Models.vwCHIS_Doctor_OnOffDutyData> InitalEmployeeOnDutyInfo(int doctorId, int departId, DateTime dateFrom, DateTime dateTo)
        {
            var rtn = new List<Models.vwCHIS_Doctor_OnOffDutyData>();
            int[] slots = new int[] { 1, 2 };
            int days = (dateTo - dateFrom).Days;
            for (int i = 0; i <= days; i++)
            {
                DateTime date = dateFrom.AddDays(i).Date;
                foreach (var slotNum in slots)
                {
                    var slotInfo = InitalEmployeeOnDutyInfo(doctorId, departId, date, slotNum);
                    if (date.Date == DateTime.Today)
                    {
                        if (DateTime.Now.TimeOfDay.Add(new TimeSpan(0, 10, 0)) > slotInfo.ToTime) continue;
                    }
                    rtn.Add(slotInfo);
                }
            }
            return rtn;
        }


        #region 开药品
        public IQueryable<Models.vwCHIS_DrugStock_Monitor> query_GetDrugInfos(string term, ref IEnumerable<string> drugFrom, string drugType = "")
        {
            term = Ass.P.PStr(term).ToLower();
            var u = controller.UserSelf;
            //首先筛选本工作站门诊房 或者 剑客网上药品
            if (drugFrom == null) drugFrom = new List<string> { "0-0" };//默认本地
            List<int?> f0 = new List<int?>(), f1 = new List<int?>(), f2 = new List<int?>();
            foreach (var item in drugFrom)
            {
                try
                {
                    var ss = item.Split('-');
                    var a = int.Parse(ss[0]); var bb = int.Parse(ss[1]);
                    if (a == 0) f0.Add(bb);
                    if (a == 1) f1.Add(bb);
                    if (a == 2) f2.Add(bb);
                }
                catch { }
            }

            //组织sql语句实现快速搜索
            StringBuilder b = new StringBuilder(), ba = new StringBuilder();
            b.AppendFormat("select * from vwCHIS_DrugStock_Monitor where (StationId={0} {1}) and StockDrugIsEnable=1", u.DrugStoreStationId, f1.Count() > 0 ? " or StationId=-1" : "");
            if (drugType.IsNotEmpty()) b.AppendFormat(" and MedialMainKindCode='{0}'", drugType);

            //搜索范围
            if (f0.Count > 0) ba.AppendFormat("or (SourceFrom={0} and SupplierId in({1}))", 0, string.Join(",", f0));
            if (f1.Count > 0) ba.AppendFormat("or (SourceFrom={0} and SupplierId in({1}))", 1, string.Join(",", f1));
            if (f2.Count > 0) ba.AppendFormat("or (SourceFrom={0} and SupplierId in({1}))", 2, string.Join(",", f2));
            if (ba.Length > 10) b.AppendFormat(" and ({0})", ba.Remove(0, 2));

            //搜索数量大于零
            b.Append(" and DrugStockNum>0 ");

            //筛选内容

            long drugId = 0;
            if (long.TryParse(term, out drugId))
            {
                if (term.Length > 8)
                {
                    b.AppendFormat(" and BarCode={0}", term);
                }
                else
                {
                    var b0 = b.ToString() + $" and DrugId={drugId}";
                    var b1 = b.AppendFormat(" and charindex('{0}',codedock)>0 ", term).ToString();
                    b = new StringBuilder();
                    b.Append(b0); b.Append(" union "); b.Append(b1);
                }
            }
            else
            {
                b.AppendFormat(" and charindex('{0}',codedock)>0 ", term);
            }      

            return _db.vwCHIS_DrugStock_Monitor.FromSql(b.ToString());
        }
        public Models.vwCHIS_DrugStock_Monitor query_GetDrugInfos(int drugId)
        {
            var u = controller.UserSelf;
            return _db.vwCHIS_DrugStock_Monitor.FirstOrDefault(m => m.DrugId == drugId);
        }




        #endregion



        /// <summary>
        /// 获取雇员（医生）值班情况信息
        /// </summary>
        private Models.vwCHIS_Doctor_OnOffDutyData InitalEmployeeOnDutyInfo(int doctorId, int departId, DateTime date, int slotNum)
        {

            var depart = _db.CHIS_Code_Department.Find(departId);

            var item = _db.vwCHIS_Doctor_OnOffDutyData.FirstOrDefault(m => m.DoctorId == doctorId &&
                                           (m.ScheduleDate - date).Days == 0 && m.Slot == slotNum);
            if (item == null)
            {
                //如果是正常节假日则                
                var doctor = _db.vwCHIS_Code_Doctor.FirstOrDefault(m => m.DoctorId == doctorId);
                if (doctor == null) throw new Exception("没有找到预约医生信息");
                var defSlot = _db.CHIS_Code_DoctorWorkInfo.FirstOrDefault(m => m.DoctorId == doctorId && m.Slot == slotNum);
                if (defSlot == null) //更新医生默认的Slot
                {
                    TimeSpan fromTime = slotNum == 1 ? new TimeSpan(8, 0, 0) : new TimeSpan(14, 0, 0);
                    TimeSpan toTime = slotNum == 1 ? new TimeSpan(12, 0, 0) : new TimeSpan(18, 0, 0);
                    defSlot = _db.Add(new Models.CHIS_Code_DoctorWorkInfo
                    {
                        DoctorId = doctor.DoctorId,
                        Slot = slotNum,
                        DefSlotAllowNum = 100,
                        DefSlotAllowReservateNum = 20,
                        FromTime = fromTime,
                        ToTime = toTime
                    }).Entity;
                    _db.SaveChanges();
                }
                var addItem = _db.Add(new Models.CHIS_Doctor_OnOffDutyData
                {
                    DoctorId = doctorId,
                    DepartmentId = departId,
                    ScheduleDate = date.Date,
                    StationId = depart.StationID,
                    Slot = slotNum,
                    ReservateLimitNum = defSlot.DefSlotAllowReservateNum.Value,
                    ReservatedNum = 0,
                    MaxCount = defSlot.DefSlotAllowNum.Value,
                    FromTime = defSlot.FromTime,
                    IsNextDayFromTime = defSlot.IsNextDayOfFromTime,
                    IsNextDayToTime = defSlot.IsNextDayOfToTime,
                    ToTime = defSlot.ToTime,
                    IsLimitNum = false,
                    IsWorkSlot = true,
                    AddCount = 0
                }).Entity;
                _db.SaveChanges();
                return _db.vwCHIS_Doctor_OnOffDutyData.FirstOrDefault(m => m.OnOffDutyId == addItem.OnOffDutyId);
            }
            return item;
        }

        /// <summary>
        /// 添加医生信息
        /// </summary> 
        public async Task<vwCHIS_Code_Doctor> CreateDoctorAsync(CHIS_Code_Doctor model)
        {
            if (model.CustomerId <= 0) throw new Exception("该用户不存在，不能添加医生信息");
            var docList = base._db.CHIS_Code_Doctor.Where(m => m.CustomerId == model.CustomerId);
            if (docList.Count() > 0) throw new Exception("该医生已存在，不能重复添加");
            model.OpTime = model.DoctorCreateTime = DateTime.Now;
            model.OpID = controller.UserSelf.OpId;
            model.OpMan = controller.UserSelf.OpManFullMsg;

         
            _db.BeginTransaction();

            try
            {
                var add = await _db.CHIS_Code_Doctor.AddAsync(model);
                await _db.SaveChangesAsync();

                //临时添加到工作站内
                var add0 = _db.CHIS_Sys_Rel_DoctorStations.AddAsync(new CHIS_Sys_Rel_DoctorStations
                {
                    DoctorId = add.Entity.DoctorId,
                    StationId = controller.UserSelf.StationId,
                    StationIsEnable = false
                });
                await _db.SaveChangesAsync();
                //添加到工作站内
                var add1 = _db.CHIS_Sys_Rel_DoctorStationRoles.AddAsync(new CHIS_Sys_Rel_DoctorStationRoles
                {
                    DoctorId = add.Entity.DoctorId,
                    StationId = controller.UserSelf.StationId,
                    RoleId = 0,
                    MyRoleIsEnable = false,
                    MyStationIsEnable = false
                });
                await _db.SaveChangesAsync();

                _db.CommitTran();
            }
            catch (Exception ex)
            {
                _db.RollbackTran();
                throw ex;
            }

            return _db.vwCHIS_Code_Doctor.AsNoTracking().First(m => m.DoctorId == model.DoctorId);
        }
        internal async Task<vwCHIS_Code_Doctor> ModifyDoctorAsync(CHIS_Code_Doctor model)
        {
    
            var doc = _db.CHIS_Code_Doctor.Find(model.DoctorId);
            doc.CustomerId = model.CustomerId;
            doc.DoctorPhotoUrl = model.DoctorPhotoUrl;
            doc.PostTitle = model.PostTitle;
            doc.Principalship = model.Principalship;
            doc.IsEnable = model.IsEnable == true;
            doc.StopDate = model.StopDate;
            doc.TreatFee = model.TreatFee;
            doc.DoctorSkillRmk = model.DoctorSkillRmk;
            doc.Remark = model.Remark;
            doc.OpID = controller.UserSelf.OpId;
            doc.OpMan = controller.UserSelf.OpMan;
            doc.OpTime = DateTime.Now;
            doc.IsChecking = false;
            doc.DoctorIsAuthenticated = false;
            await _db.SaveChangesAsync();



            return _db.vwCHIS_Code_Doctor.AsNoTracking().FirstOrDefault(m => m.DoctorId == model.DoctorId);
        }






        public Models.vwCHIS_Doctor_OnOffDutyData GetEmployeeOnDutyInfo(int doctorId, DateTime date, int slotNum)
        {
            return _db.vwCHIS_Doctor_OnOffDutyData.FirstOrDefault(m => m.DoctorId == doctorId &&
                                         (m.ScheduleDate - date).Days == 0 && m.Slot == slotNum);

        }

        public async Task<bool> IsNeedCompleteDoctorInfo(int doctorId)
        {
            var d = await _db.vwCHIS_Code_Doctor.AsNoTracking().FirstOrDefaultAsync(m => m.DoctorId == doctorId);
            if (string.IsNullOrEmpty(d.DoctorName)) return true; // 姓名
            if (d.Gender == null) return true;//性别
            if (d.IDcard.IsEmpty() && d.CustomerIsAuthenticated != true) return true;//身份证
                                                                                     //todo 默认需要完善医生信息
#if DEBUG
            //  return true;
#endif
            return false;
        }

        //是否正在审查中
        internal bool IsCheckingOccupationInfo(int doctorId)
        {
            var d = _db.vwCHIS_Code_Doctor.AsNoTracking().FirstOrDefault(m => m.DoctorId == doctorId);
            if (d.DoctorIsAuthenticated != true) return true;
            if (d.IsChecking == true) return true;
            return false;
        }


        /// <summary>
        /// 注册医生基本信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool RegistDoctorBasic(CHIS.Models.ViewModel.CustomerRegistViewModel model)
        {
            if (model.RegistType == "mobile")
            {
                var find = _db.CHIS_DataTemp_SMS.AsNoTracking().Where(m => m.PhoneCode == model.Mobile && m.VCodeProp == null && m.CreatTime > DateTime.Today).OrderByDescending(m => m.CreatTime).FirstOrDefault();
                if (find == null) throw new Exception("没有找到短信验证码");
                if (!string.Equals(find.VCode, model.VCode, StringComparison.CurrentCultureIgnoreCase)) throw new Exception("验证码输入错误");
            }
            if (model.RegistType == "email")
            {
                var find = _db.CHIS_DataTemp_SendMailVCode.AsNoTracking().Where(m => m.EmailAddress == model.Email && m.VCodeProp == null && m.CreatTime > DateTime.Today).OrderByDescending(m => m.CreatTime).FirstOrDefault();
                if (find == null) throw new Exception("没有找到邮件的验证码");
                if (!string.Equals(find.VCode, model.VCode, StringComparison.CurrentCultureIgnoreCase)) throw new Exception("验证码输入错误");
            }

            if (model.RegPaswd != model.RegPaswdConfirm) throw new Exception("注册密码与确认密码不一致");
            if (model.RegistRole != "doctor") throw new Exception("注册非医生");

            _db.BeginTransaction();

            try
            {
                //----获取或者添加会员信息-------
                Models.CHIS_Code_Customer cus = null;
                if (model.RegistType == "mobile")
                    cus = _db.CHIS_Code_Customer.FirstOrDefault(m => m.CustomerMobile == model.Mobile);
                if (model.RegistType == "email")
                    cus = _db.CHIS_Code_Customer.FirstOrDefault(m => m.Email == model.Email);

                if (cus == null)
                {

                    var newcus = new Models.CHIS_Code_Customer
                    {
                        Email = model.Email,
                        CustomerMobile = model.Mobile,
                        CustomerCreateDate = DateTime.Now, //创建时间
                        sysLatestActiveTime = DateTime.Now,
                        sysSource = sysSources.医生注册.ToString()
                    };
                    newcus.NickName = newcus.CustomerName = "用户" + DateTime.Now.ToString("yyMMddHHmmssfff");
                    cus = _db.Add(newcus).Entity;
                    _db.SaveChanges();
                }

                //----获取医生信息 -----------
                Models.CHIS_Code_Doctor doc = null;
                doc = _db.CHIS_Code_Doctor.FirstOrDefault(m => m.CustomerId == cus.CustomerID);
                if (doc == null)
                {
                    var docEntry = _db.Add(new Models.CHIS_Code_Doctor
                    {
                        CustomerId = cus.CustomerID,
                        DoctorCreateTime = DateTime.Now,
                        IsEnable = true
                    });
                    _db.SaveChanges();
                    doc = docEntry.Entity;
                }
                //--------------增加医生与工作站默认基本联系 网上平台测试站-------------
                new CHIS.Api.syshis(controller._db).UpsertDoctorStationRoles(doc.DoctorId, 6, new List<int> { 12 });



                var login = new Models.CHIS_Sys_Login
                {
                    CustomerId = cus.CustomerID,
                    DoctorId = doc.DoctorId,
                    Email = model.Email,
                    Mobile = model.Mobile,
                    LoginPassword = model.RegPaswd,
                    IsLock = false
                };
                if (model.RegistType == "email") { login.EmailIsAuthenticated = true; login.EmailAuthenticatedTime = DateTime.Now; }
                if (model.RegistType == "mobile") { login.MobileIsAuthenticated = true; login.MobileAuthenticatedTime = DateTime.Now; }
                var addEntry = _db.CHIS_Sys_Login.Add(login);
                _db.SaveChanges();
                //   tx.Rollback();
                _db.CommitTran();
                return true;
            }
            catch (Exception ex) { _db.RollbackTran(); throw ex; }

            // return false; 
        }

        /// <summary>
        /// 更新医生的角色
        /// </summary>
        /// <param name="item"></param>
        /// <param name="doctorId"></param>
        /// <returns></returns>
        internal async Task UpsertStationRolesAndDepartsAsync(StationRolesDepartsItem item, int doctorId)
        {
          

            //添加工作站
            var find = _db.CHIS_Sys_Rel_DoctorStations.AsNoTracking().Where(m => m.DoctorId == doctorId).Select(m => m.StationId);
            if (!find.Contains(item.StationId))
            {
                await _db.CHIS_Sys_Rel_DoctorStations.AddAsync(new CHIS_Sys_Rel_DoctorStations
                {
                    DoctorId = doctorId,
                    StationId = item.StationId,
                    StationIsEnable = true
                });
            }
            else
            {
                var fd = await _db.CHIS_Sys_Rel_DoctorStations.FirstOrDefaultAsync(m => m.StationId == item.StationId && m.DoctorId == doctorId);
                fd.StationIsEnable = true;     //设置为可用           
            }

            //添加角色
            var roles = _db.CHIS_Sys_Rel_DoctorStationRoles.AsNoTracking().Where(m => m.StationId == item.StationId && m.DoctorId == doctorId).ToList();
            foreach (var ro in item.Roles)
            {
                if (roles.Any(m => m.RoleId == ro))
                {
                    var rfd = roles.Find(m => m.RoleId == ro);
                    rfd.MyRoleIsEnable = true;
                    rfd.MyStationIsEnable = true;
                    _db.CHIS_Sys_Rel_DoctorStationRoles.Update(rfd);//更新
                }
                else
                {
                    if (ro > 0)
                    {
                        //添加
                        await _db.CHIS_Sys_Rel_DoctorStationRoles.AddAsync(new CHIS_Sys_Rel_DoctorStationRoles
                        {
                            DoctorId = doctorId,
                            StationId = item.StationId,
                            RoleId = ro,
                            MyRoleIsEnable = true,
                            MyStationIsEnable = true
                        });
                    }
                }
            }

            //添加科室
            var deps = _db.CHIS_Code_Rel_DoctorDeparts.AsNoTracking().Where(m => m.DoctorId == doctorId).ToList();
            foreach (var dp in item.Departs)
            {
                if (deps.Any(m => m.DepartId == dp))
                {
                    var fd = deps.Find(m => m.DepartId == dp);
                    fd.IsVerified = true; fd.VerifiedTime = DateTime.Now;
                    _db.CHIS_Code_Rel_DoctorDeparts.Update(fd);//更新
                }
                else
                {
                    if (dp > 0)
                    {
                        await _db.CHIS_Code_Rel_DoctorDeparts.AddAsync(new CHIS_Code_Rel_DoctorDeparts
                        {
                            DepartId = dp,
                            DoctorId = doctorId,
                            IsVerified = true,
                            VerifiedTime = DateTime.Now
                        });
                    }
                }
            }

            await _db.SaveChangesAsync();

        }

        public async Task DeleteRoleOfDoctorAsync(int doctorId, int stationId, int roleId)
        {
            
            var finds = _db.CHIS_Sys_Rel_DoctorStationRoles.AsNoTracking().Where(m => m.DoctorId == doctorId && m.StationId == stationId && m.RoleId == roleId).ToList();
            foreach (var item in finds)
            {
                item.MyRoleIsEnable = false;
                item.MyRoleStopTime = DateTime.Now;
            }
            _db.CHIS_Sys_Rel_DoctorStationRoles.UpdateRange(finds);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteDepartmentOfDoctorAsync(int doctorId, int departId)
        {
            
            var finds = _db.CHIS_Code_Rel_DoctorDeparts.AsNoTracking().Where(m => m.DoctorId == doctorId && m.DepartId == departId).ToList();
            foreach (var item in finds)
            {
                item.IsVerified = false;
                item.VerifiedTime = null;
            }
            _db.CHIS_Code_Rel_DoctorDeparts.UpdateRange(finds);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteStationOfDoctorAsync(int doctorId, int stationId)
        {
         
            var finds = _db.CHIS_Sys_Rel_DoctorStationRoles.AsNoTracking().Where(m => m.DoctorId == doctorId && m.StationId == stationId).ToList();
            foreach (var item in finds)
            {
                item.MyStationIsEnable = false;
                item.MyStationStopTime = DateTime.Now;
            }
            _db.CHIS_Sys_Rel_DoctorStationRoles.UpdateRange(finds);

            var finds2 = _db.CHIS_Sys_Rel_DoctorStations.AsNoTracking().Where(m => m.DoctorId == doctorId&&m.StationId==stationId).ToList();
            foreach (var item in finds2)
            {
                item.StationIsEnable = false;
                item.StationStopTime = DateTime.Now;
            }
            _db.CHIS_Sys_Rel_DoctorStations.UpdateRange(finds2);

            await _db.SaveChangesAsync();
        }


        /// <summary>
        /// 客户申请医生
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public async Task<bool> CustomerRegistDoctor(CHIS_Code_Customer customer)
        {
            var add = _db.CHIS_Code_Doctor.Add(new CHIS_Code_Doctor
            {
                CustomerId = customer.CustomerID,
                DoctorCreateTime = DateTime.Now,
                IsEnable = true
            }).Entity;
            _db.SaveChanges();
            var login = _db.CHIS_Sys_Login.FirstOrDefault(m => m.CustomerId == customer.CustomerID);
            if (login != null) login.DoctorId = add.DoctorId;
            await _db.SaveChangesAsync();

            //给一个基本的工作站信息
            await _db.CHIS_Sys_Rel_DoctorStations.AddAsync(new CHIS_Sys_Rel_DoctorStations
            {
                DoctorId = add.DoctorId,
                StationId = MPS.TestNetStationId,
                StationIsEnable = true,
            });
            //给全责医生的测试角色
            await _db.CHIS_Sys_Rel_DoctorStationRoles.AddAsync(new CHIS_Sys_Rel_DoctorStationRoles
            {
                DoctorId = add.DoctorId,
                StationId = MPS.TestNetStationId,
                MyRoleIsEnable = true,
                MyStationIsEnable = true,
                RoleId = MPS.RoleTreatAllDoctorId
            });
            await _db.SaveChangesAsync();

            return true;
        }




        #region DoctorTreat接诊

        private Random _radom2 = new Random();






        #endregion



        #region 医生数据信息

        /// <summary>
        /// 更新医生的部门信息
        /// </summary>
        /// <param name="departIds"></param>
        /// <param name="doctorId"></param>
        /// <returns></returns>
        public bool ChangeDoctorDeparts(List<int> departIds, int doctorId)
        {
            bool ischanged = false;// 是否有部门更新
            var finds = _db.CHIS_Code_Rel_DoctorDeparts.AsNoTracking().Where(m => m.DoctorId == doctorId).ToList();
            foreach (var item in finds)
            {
                if (!departIds.Contains(item.DepartId.Value)) { _db.Remove(item); ischanged = true; }
            }
            var dbhas = finds.Select(m => m.DepartId);
            foreach (int did in departIds)
            {
                if (!dbhas.Contains(did))
                {
                    _db.Add(new CHIS_Code_Rel_DoctorDeparts
                    {
                        DepartId = did,
                        DoctorId = doctorId
                    });
                    ischanged = true;
                }
            }
            _db.SaveChanges();
            if (ischanged)
            {
                var doc = _db.CHIS_Code_Doctor.Find(doctorId);
                doc.DoctorIsAuthenticated = null;
                doc.DoctorAuthenticatedTime = null;
                doc.IsChecking = true;
                _db.SaveChanges();
            }
            return true;
        }

        /// <summary>
        /// 更新医生的证书信息
        /// </summary> 
        /// <returns></returns>
        public bool ChangeDoctorCertbooks(List<CHIS_Code_DoctorCertbook> certbooks, int doctorId)
        {

            bool ischanged = false;// 是否有部门更新
            var finds = _db.CHIS_Code_DoctorCertbook.AsNoTracking().Where(m => m.DoctorId == doctorId).ToList();
            //首先过滤需要删除的证书
            foreach (var item in finds)
            {
                //如果没有相同证书
                if (!certbooks.Any(m => m.CertTypeId == item.CertTypeId)) { _db.Remove(item); ischanged = true; }
            }
            var dbhas = finds.Select(m => m.CertImgUrl);
            foreach (var item in certbooks)
            {
                if (finds.Any(m => m.CertTypeId == item.CertTypeId))
                {
                    //修改
                    var mod = _db.CHIS_Code_DoctorCertbook.FirstOrDefault(m => m.DoctorId == doctorId && m.CertTypeId == item.CertTypeId);
                    if (mod.CertImgUrl != item.CertImgUrl) /*只有地址不同时候修改*/
                    {
                        mod.CertImgUrl = item.CertImgUrl;
                        _db.SaveChanges();
                        ischanged = true;
                    }
                }
                else
                {
                    //添加
                    _db.Add(new CHIS_Code_DoctorCertbook
                    {
                        CertImgUrl = item.CertImgUrl,
                        CertTypeId = item.CertTypeId,
                        DoctorId = doctorId
                    });
                    ischanged = true;
                }
            }
            _db.SaveChanges();
            if (ischanged)
            {
                var doc = _db.CHIS_Code_Doctor.Find(doctorId);
                doc.DoctorIsAuthenticated = null;
                doc.DoctorAuthenticatedTime = null;
                doc.IsChecking = true;
                _db.SaveChanges();
            }
            return true;
        }



        #endregion


        /// <summary>
        /// 查询工作站的所有医生
        /// </summary>
        /// <param name="stationId"></param>
        /// <param name="bSubStations"></param>
        /// <returns></returns>
        public IQueryable<vwCHIS_Code_Doctor> queryDoctorsOfMyStation(int stationId, bool bSubStations = true)
        {
            //  return MainDbContext.vwCHIS_Code_Doctor.FromSql("exec sp_Doctor_OfMyStation {0},{1}",stationId,bSubStations?1:0);
            var db = controller._db;
            var stations = bSubStations ? controller.UserMgr.GetStations(stationId) :
                from item in new int[] { stationId } select stationId;

            var finda = from item in db.CHIS_Sys_Rel_DoctorStations where stations.Contains(item.StationId) select item.DoctorId;
            finda = finda.Distinct();

            var findList = from item in db.vwCHIS_Code_Doctor where finda.Contains(item.DoctorId) select item;

            //var findList = (from item in MainDbContext.CHIS_Sys_Rel_DoctorStations
            //                join doctor in MainDbContext.vwCHIS_Code_Doctor on item.DoctorId equals doctor.DoctorId into temp
            //                from tt in temp.DefaultIfEmpty()
            //                where (stations == null) ? (item.StationId == stationId) : (stations.Contains(item.StationId.Value)) && tt != null
            //                select tt).Where(m=>m.DoctorId>0).Distinct();
            return findList;
        }

         

        /// <summary>
        /// 保存医生登录信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        internal async Task<bool> SaveDoctorLoginAsync(CHIS_Sys_Login model)
        {
           

            CHIS_Code_Customer cus = null;
            var fd = await _db.CHIS_Sys_Login.FirstOrDefaultAsync(m => m.LoginId == model.LoginId);
            fd.DoctorId = model.DoctorId;
            if (fd.LoginName != model.LoginName && model.LoginName.IsNotEmpty())
            {
                fd.LoginName = model.LoginName;
                cus = await _db.CHIS_Code_Customer.AsNoTracking().FirstOrDefaultAsync(m => m.CustomerID == fd.CustomerId);
                cus.LoginName = model.LoginName;
            }
            if (model.LoginPassword.IsNotEmpty()) fd.LoginPassword = model.LoginPassword;
            fd.IsLock = model.IsLock;
            fd.LockTime = (model.IsLock == true) ? (model.LockTime ?? DateTime.Now) : (DateTime?)null;
            fd.WhyLock = model.WhyLock;

            if (cus != null) _db.CHIS_Code_Customer.Update(cus);
            if (fd != null) _db.CHIS_Sys_Login.Update(fd);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}

