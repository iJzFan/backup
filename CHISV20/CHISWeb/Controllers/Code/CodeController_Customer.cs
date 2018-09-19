using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Ass;
using CHIS.Codes.Utility;
using Microsoft.EntityFrameworkCore;
using CHIS.Models;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace CHIS.Controllers
{

    public partial class CodeController
    {
        public IActionResult CustomerDocs()
        {

            return View();
        }
        public IActionResult CustomerDocs2()
        {
            return View();
        }

        public IActionResult LoadCustomerList(string searchText, string timeRange = "Today", bool? isVIP = null, int pageIndex = 1, int pageSize = 20)
        {
            base.initialData_Page(ref pageIndex, ref pageSize);
            DateTime? dt0 = null, dt1 = null;
            base.initialData_TimeRange(ref dt0, ref dt1, timeRange);
            //筛选数据                
            var findList = _db.vwCHIS_Code_Customer.AsNoTracking();
            if (timeRange != "All")
            {
                findList = findList.Where(m => m.CustomerCreateDate >= dt0 && m.CustomerCreateDate < dt1);
            }

            if (!string.IsNullOrEmpty(searchText))
            {
                var t = searchText.GetStringType();
                if (t.IsEmail) findList = findList.Where(m => m.Email == t.String);
                else if (t.IsMobile) findList = findList.Where(m => m.CustomerMobile == t.String);
                else if (t.IsIdCardNumber) findList = findList.Where(m => m.IDcard == t.String);
                else if (t.IsLoginNameLegal) findList = findList.Where(m => m.LoginName == t.String);
                else findList = findList.Where(m => m.CustomerName == searchText);
            }


            if (isVIP.HasValue) findList = findList.Where(m => m.IsVIP == isVIP.Value);


            var total = findList.Count();
            var find = findList.OrderByDescending(m => m.CustomerCreateDate).Skip(pageSize * (pageIndex - 1)).Take(pageSize);

            var model = new Ass.Mvc.PageListInfo<vwCHIS_Code_Customer>
            {
                DataList = find,
                PageIndex = pageIndex,
                PageSize = pageSize,
                RecordTotal = total
            };
            return PartialView("_pvCustomerList", model);
        }

        public IActionResult LoadRowDetailOfCustomer(long cusId)
        {
            var model = _db.vwCHIS_Code_Customer.FirstOrDefault(m => m.CustomerID == cusId);
            return PartialView("_pvCustomerRowDetail", model);
        }

        //员工档案记录查询
        public IActionResult SearchJson_CHIS_Code_Customer(string keyword = null)
        {
            try
            {
                //筛选数据                
                var findList = _db.vwCHIS_Code_Customer.AsNoTracking();
                if (!string.IsNullOrEmpty(keyword)) findList = findList.Where(m => m.Telephone == keyword || m.CustomerMobile == keyword || m.IDcard == keyword || m.Email == keyword || m.CustomerName == keyword);
                //分页返回
                return FindPagedData_jqgrid<Models.vwCHIS_Code_Customer>(findList.OrderByDescending(m => m.CustomerCreateDate),
                   selector: m => new
                   {
                       CustomerId = m.CustomerID,
                       CustomerNo = m.CustomerNo,
                       CustomerName = m.CustomerName,
                       IsVip = m.IsVIP,
                       VipCode = m.VIPcode,
                       Gender = m.Gender,
                       Age = m.Birthday?.ToAgeString(),
                       IDcard = m.IDcard?.ToMarkString(Ass.Models.MaskType.IDCode),
                       Mobile = m.CustomerMobile?.ToMarkString(Ass.Models.MaskType.MobileCode),
                       Telephone = m.Telephone?.ToMarkString(Ass.Models.MaskType.MobileCode),
                       Email = m.Email,
                       MergerName = m.MergerName,
                       PhotoUrlDef = m.PhotoUrlDef,
                       CreatDate = Convert.ToDateTime(m.CustomerCreateDate).ToString("yyyy-MM-dd")
                   }
               );
            }
            catch (Exception ex)
            {
                return View("ErrorBlank", ex);
            }
        }






        //编辑的页面操作 op=NEWF/NEW/MODIFYF/MODIFY/DELETE 
        public async Task<IActionResult> CHIS_Code_Customer_Edit(string op, Models.DataModel.CustomerInfo model, int recId)
        {
            var user = UserSelf;
            //todo
            try
            {
                string editViewName = nameof(CHIS_Code_Customer_Edit);
                //   var sysUser = base.GetCurrentUserInfo();
                ViewBag.OP = op;// 初始化操作类别             
                switch (op.ToUpper())
                {
                    case "NEWF": //新增页面 空白的数据页面
                        var modelnew = new Models.DataModel.CustomerInfo() { };
                        ViewBag.OP = "NEW";
                        return View(editViewName, modelnew);
                    case "NEW": // 更新新增的数据
                        _db.BeginTransaction();

                        try
                        {
                            #region
                            //********************业务处理代码模块******************                         
                            //1.判断手机号码和身份证号码是否合法
                            //2.判断身份证号码和手机号码是否存在

                            //判断身份证号码的合法性
                            if (!model.Customer.IDcard.IsEmpty())
                            {
                                var isIDCard = CHIS.Code.Utility.ComTools.IsIDCard(model.Customer.IDcard);
                                if (!isIDCard) throw new Exception("不合法的身份证号码");
                                var finds = _db.vwCHIS_Sys_Login.Where(m => m.IdCardNumber == model.Customer.IDcard);
                                if (finds.Count() > 0) throw new Exception("该身份证号已存在！");
                            }
                            #endregion

                            //添加用户必须要的基本信息
                            model.Customer.sysLatestActiveTime = DateTime.Now;
                            //model.Customer.CustomerNo = AutoGetCustomerNo(DateTime.Today);//患者编号
                            model.Customer.OpID = user.OpId;
                            model.Customer.OpMan = user.OpManFullMsg;
                            model.Customer.OpTime = DateTime.Now;
                            model.Customer.CustomerCreateDate = DateTime.Today;
                            model.Customer.sysSource = sysSources.CHIS系统.ToString();
                            //添加用户;
                            var c = _db.Add(model.Customer).Entity;
                            _db.SaveChanges();
                            model.Health.CustomerId = c.CustomerID;

                            _db.CHIS_Code_Customer_HealthInfo.Add(model.Health);
                            _db.SaveChanges();
                            _db.CommitTran();
                            return Json(new { state = "success" });
                        }
                        catch (Exception ex)
                        {
                            _db.RollbackTran();
                            return Json(new { state = "error", msg = "添加失败" + ex.Message });
                        }

                    case "MODIFYF": //修改 查找出修改的原始实体数据
                        var modelmodifyCust = _db.CHIS_Code_Customer.Find(recId);
                        if (modelmodifyCust.CustomerID <= 0) throw new Exception("不在该用户");
                        var modelmodifyHeal = _db.CHIS_Code_Customer_HealthInfo.FirstOrDefault(m => m.CustomerId == modelmodifyCust.CustomerID);
                        if (modelmodifyHeal == null) { modelmodifyHeal = _db.CHIS_Code_Customer_HealthInfo.Add(new CHIS_Code_Customer_HealthInfo() { CustomerId = modelmodifyCust.CustomerID }).Entity; _db.SaveChanges(); }
                        var modelmodify = new Models.DataModel.CustomerInfo();
                        modelmodify.Customer = modelmodifyCust;
                        modelmodify.Health = modelmodifyHeal;
                        ViewBag.OP = "MODIFY";
                        return View(editViewName, modelmodify);
                    case "MODIFY": //修改后的数据
                        return await TryCatchFuncAsync(async () =>
                       {
                           await new CustomerCBL(this).UpdateCustomerTrans(model.Customer, model.Health);
                           return null;
                       });
                    case "DELETE": //删除，返回json 
                        return TryCatchFunc(() =>
                        {
                            new CustomerCBL(this).ForceDelete(recId);
                            return null;
                        });
                    case "VIEW":
                        var md = new Models.DataModel.CustomerInfo();
                        var cust = _db.CHIS_Code_Customer.Find(recId);
                        var heal = _db.CHIS_Code_Customer_HealthInfo.First(m => m.CustomerId == cust.CustomerID);
                        md.Customer = cust;
                        md.Health = heal;
                        return View(editViewName, md);
                    default:
                        throw new Exception("错误的命令");
                }
            }
            catch (Exception ex)
            {
                // Loger.WriteError("Code", "CHIS_Code_EmployeeMsg_Edit", ex);
                return View("Error", ex);
            }
        }

        //编辑的页面操作 op=NEWF/NEW/MODIFYF/MODIFY/DELETE 
        public async Task<IActionResult> CustomerEdit(string op, Models.DataModel.CustomerInfo model, int recId)
        {
            var user = UserSelf;
            //todo
            try
            {
                string editViewName = nameof(CustomerEdit);
                //   var sysUser = base.GetCurrentUserInfo();
                ViewBag.OP = op;// 初始化操作类别             
                switch (op.ToUpper())
                {
                    case "NEWF": //新增页面 空白的数据页面
                        var modelnew = new Models.DataModel.CustomerInfo() { };
                        ViewBag.OP = "NEW";
                        return View(editViewName, modelnew);
                    case "NEW": // 更新新增的数据
                        return await TryCatchFuncAsync(async (d) =>
                        {
                            ModelState.Remove("Customer.CustomerID");
                            if (ModelState.IsValid)
                            {
                                #region
                                //********************业务处理代码模块******************                         
                                //1.判断手机号码和身份证号码是否合法
                                //2.判断身份证号码和手机号码是否存在

                                //判断身份证号码的合法性
                                if (!model.Customer.IDcard.IsEmpty())
                                {
                                    var isIDCard = CHIS.Code.Utility.ComTools.IsIDCard(model.Customer.IDcard);
                                    if (!isIDCard) throw new Exception("不合法的身份证号码");
                                    var finds = _db.vwCHIS_Sys_Login.Where(m => m.IdCardNumber == model.Customer.IDcard);
                                    if (finds.Count() > 0) throw new Exception("该身份证号已存在！");
                                }
                                #endregion
                                //添加用户必须要的基本信息                              
                                //model.Customer.CustomerNo = AutoGetCustomerNo(DateTime.Today);//患者编号
                                model.Customer.OpID = user.OpId;
                                model.Customer.OpMan = user.OpManFullMsg;
                                model.Customer.sysSource = sysSources.CHIS系统.ToString();
                                
                                var cus = await _cusSvr.CreateCustomerAsync(model.Customer, model.Health,user.OpId,user.OpMan);
                                d.item = _db.vwCHIS_Code_Customer.AsNoTracking().First(m => m.CustomerID == cus.CustomerID);
                            }
                            else throw new Exception(base.GetErrorOfModelState(ModelState));

                            return null;
                        });
                    case "MODIFYF": //修改 查找出修改的原始实体数据
                        var modelmodifyCust = _db.CHIS_Code_Customer.Find(recId);
                        if (modelmodifyCust.CustomerID <= 0) throw new Exception("不在该用户");
                        var modelmodifyHeal = _db.CHIS_Code_Customer_HealthInfo.FirstOrDefault(m => m.CustomerId == modelmodifyCust.CustomerID);
                        if (modelmodifyHeal == null) { modelmodifyHeal = _db.CHIS_Code_Customer_HealthInfo.Add(new CHIS_Code_Customer_HealthInfo() { CustomerId = modelmodifyCust.CustomerID }).Entity; _db.SaveChanges(); }
                        var modelmodify = new Models.DataModel.CustomerInfo();
                        modelmodify.Customer = modelmodifyCust;
                        modelmodify.Health = modelmodifyHeal;
                        ViewBag.OP = "MODIFY";
                        return View(editViewName, modelmodify);
                    case "MODIFY": //修改后的数据
                        return await TryCatchFuncAsync(async () =>
                        {
                            await new CustomerCBL(this).UpdateCustomerTrans(model.Customer, model.Health);
                            return null;
                        });
                    case "DELETE": //删除，返回json 
                        return TryCatchFunc(() =>
                        {
                            new CustomerCBL(this).ForceDelete(recId);
                            return null;
                        });
                    case "VIEW":
                        var md = new Models.DataModel.CustomerInfo();
                        var cust = _db.CHIS_Code_Customer.Find(recId);
                        var heal = _db.CHIS_Code_Customer_HealthInfo.First(m => m.CustomerId == cust.CustomerID);
                        md.Customer = cust;
                        md.Health = heal;
                        return View(editViewName, md);
                    default:
                        throw new Exception("错误的命令");
                }
            }
            catch (Exception ex)
            {
                // Loger.WriteError("Code", "CHIS_Code_EmployeeMsg_Edit", ex);
                return View("Error", ex);
            }
        }

        #region 用户注册的基本信息

        /// <summary>
        /// 自动获取患者编号
        /// </summary>
        /// <param name="OpDate">操作日期</param>
        /// <returns></returns>
        public string AutoGetCustomerNo(DateTime? OpDate, int? stationId = null)
        {
            string customerNo = "";
            if (OpDate == null) OpDate = DateTime.Today;
            if (!stationId.HasValue)
            {
                var user = UserSelf;
                stationId = user.StationId;
            }

            var para = new SqlParameter[] {
                        new SqlParameter("@StationID", stationId),
                        new SqlParameter("@opDate", OpDate)
                    };

            try
            {
                //患者编号:4位工作站编码 +6位日期 +3位流水码
                customerNo = _db.MySqlFunction("dbo.fn_AutoGetCustomerNo", para).ToString();

            }
            catch (Exception ex)
            {
                var e = ex;
                //Loger.WriteError("Reception", "AutoGetCustomerNo", ex);
            }
            return customerNo;
        }
        /// <summary>
        /// 校验身份证是否输入正确
        /// </summary>
        /// <param name="idCard">身份证号</param>
        /// <param name="custID">患者ID</param>
        /// <returns></returns>

        public IActionResult CheckIDCardValidate(string idCard, int custId)
        {
            bool validate = false; string msg = "";
            string birthday = "", provinceNo = "";
            int sex = 0, province = 0;

            try
            {
                if (string.IsNullOrEmpty(idCard))
                    msg = "请输入身份证号！";
                else if (idCard.Length != 18)
                    msg = "身份证位数错误！";
                else
                {
                    int check = int.Parse(_db.MySqlFunction("dbo.fn_CheckIDCardValidate",
                        new SqlParameter("@IDCard", idCard)).ToString());
                    if (check != 1)
                        msg = "身份证输入错误！";
                    else
                    {
                        //判断是否重复
                        var result = _db.CHIS_Code_Customer.Where(m => m.IDcard == idCard && m.CustomerID != custId);
                        if (result.Count() >= 1)
                            msg = "此身份证人员已存在！";
                        else
                        {
                            validate = true;
                            birthday = idCard.Substring(6, 4) + "-" + idCard.Substring(10, 2) + "-" + idCard.Substring(12, 2);
                            sex = Ass.P.PInt(idCard.Substring(16, 1)) % 2;
                            provinceNo = idCard.Substring(0, 2);
                            // province = Global.DictDetail.FirstOrDefault(m => m.DictKey == "Province" && m.ItemValue == provinceNo).ItemName
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message; validate = false;
                //  Loger.WriteError("Reception", "CheckIDCardValidate", ex);
            }
            return Json(new
            {
                validate = validate,  //是否校验正确
                birthday = birthday,  //生日 
                province = province,  //省份
                sex = sex,            //性别
                msg = msg             //错误说明 
            });
        }
        #endregion


    }
}
