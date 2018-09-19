using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CHIS;
using System.Security.Claims;
using CHIS.Models;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Ass;
using CHIS.Models.ViewModel;
using System.Text;

namespace CHIS.Controllers
{
    public partial class MyPanelController
    {
        public IActionResult DoctorInfos()
        {
            var model = _db.vwCHIS_Code_Doctor.AsNoTracking().FirstOrDefault(m => m.DoctorId == this.UserSelf.DoctorId);
            ViewBag.Login = _db.vwCHIS_Sys_Login.AsNoTracking().FirstOrDefault(m => m.LoginId == this.UserSelf.LoginId);
            if (UserSelf.LoginExtId > 0)
            {
                return View("DoctorInfos_DrugStore", model);
            }
            return View(model);
        }

        public IActionResult SaveDoctorInfo(CHIS.Models.vwCHIS_Code_Doctor model)
        {
            try
            {
                var doctor = _db.CHIS_Code_Doctor.FirstOrDefault(m => m.DoctorId == model.DoctorId);
                if (doctor == null) throw new Exception("没有发现医生信息");
                if (doctor.CustomerId != model.CustomerId) throw new Exception("没有发现客户信息");
                var cus = _db.CHIS_Code_Customer.FirstOrDefault(m => m.CustomerID == model.CustomerId);
                var login = _db.CHIS_Sys_Login.FirstOrDefault(m => m.CustomerId == model.CustomerId);




                //用户没有验证，则可以更改姓名，性别
                if (cus.CustomerIsAuthenticated != true)
                {
                    if (model.DoctorName.IsNotEmpty() && model.DoctorName != cus.CustomerName)
                    { cus.CustomerName = model.DoctorName; } //姓名
                    if (cus.Gender != model.Gender) { cus.Gender = model.Gender; }//性别
                    if (cus.Birthday != model.Birthday) { cus.Birthday = model.Birthday; }//生日
                }

               // if (cus.Gender != model.Gender) { cus.Gender = model.Gender; }//性别
               // if (cus.Birthday != model.Birthday) { cus.Birthday = model.Birthday; }//生日


                //修改手机号码
                if (login.MobileIsAuthenticated != true)
                {
                    if (model.CustomerMobile.IsNotEmpty() && model.CustomerMobile != cus.CustomerMobile)
                    {
                        cus.CustomerMobile = model.CustomerMobile;/*修改手机*/
                        login.Mobile = cus.CustomerMobile;
                        login.MobileIsAuthenticated = false;
                        login.MobileAuthenticatedTime = null;
                    }
                }
                //修改邮箱
                if (login.EmailIsAuthenticated != true)
                {
                    if (model.Email.IsNotEmpty() && model.Email != cus.Email)
                    {
                        cus.Email = model.Email;/*修改Email*/
                        login.Email = model.Email;
                        login.EmailAuthenticatedTime = null;
                        login.EmailIsAuthenticated = false;
                    }
                }
                //修改身份证
                if (login.IdCardNumberIsAuthenticated != true)
                {
                    bool needReAuthenticated = false;
                    if (model.IDcard.IsNotEmpty() && model.IDcard != cus.IDcard) { cus.IDcard = model.IDcard;                    /*身份证    */    needReAuthenticated = true; }
                    if (model.IDCardAImg.IsNotEmpty() && model.IDCardAImg != cus.IDCardAImg) { cus.IDCardAImg = model.IDCardAImg;/*身份证A面 */    needReAuthenticated = true; }
                    if (model.IDCardBImg.IsNotEmpty() && model.IDCardBImg != cus.IDCardBImg) { cus.IDCardBImg = model.IDCardBImg;/*身份证B面 */    needReAuthenticated = true; }
                    if (needReAuthenticated)
                    {
                        cus.CustomerIsAuthenticated = false;
                        cus.CustomerAuthenticatedTime = null;
                    }
                }

                doctor.DoctorPhotoUrl = model.DoctorPhotoUrl;
                cus.EduLevel = model.EduLevel;
                _db.SaveChanges();
                return DoctorOccupationInfos();
            }
            catch (Exception ex)
            {
                ViewBag.BackLink = "/MyPanel/DoctorInfos";
                ViewBag.BackLinkName = "返回我的资料";
                return View("Error", ex);
            }
        }

        public IActionResult DoctorOccupationInfos()
        {
            var doctorId = this.UserSelf.DoctorId;
            var model = new CHIS.Models.DataModel.vwDoctor();
            model.DoctorBase = _db.vwCHIS_Code_Doctor.AsNoTracking().FirstOrDefault(m => m.DoctorId == doctorId);
            model.DoctorAllowedDeparts = _db.vwCHIS_Code_Rel_DoctorDeparts.AsNoTracking().Where(m => m.DoctorId == doctorId).ToList().OrderBy(m => m.StationName);
            model.MyCertificates = _db.vwCHIS_Code_DoctorCertbook.AsNoTracking().Where(m => m.DoctorId == doctorId);
            model.NetDepartments = _db.vwCHIS_Code_Department.AsNoTracking().Where(m => m.StationID == MPS.NetStationId);//网上平台的部门
            ViewBag.Login = _db.vwCHIS_Sys_Login.AsNoTracking().FirstOrDefault(m => m.LoginId == this.UserSelf.LoginId);
            return View("DoctorOccupationInfos", model);
        }



        //保存职业信息
        [HttpPost]
        public IActionResult DoctorOccupationInfosSave(CHIS.Models.DataModel.vwDoctor model, List<int> departsId, List<CHIS_Code_DoctorCertbook> certbooks)
        {
            return TryCatchFunc(() =>
            {
                var docId = this.UserSelf.DoctorId;
                var doc = _db.CHIS_Code_Doctor.FirstOrDefault(m => m.DoctorId == docId);
                if (doc.PostTitle != model.DoctorBase.PostTitle) { doc.PostTitle = model.DoctorBase.PostTitle;doc.IsChecking = true; }
                doc.DoctorSkillRmk = model.DoctorBase.DoctorSkillRmk;
                _db.SaveChanges();

                var cbl = new DoctorCBL(this);
                //更新部门信息
                bool rlt = cbl.ChangeDoctorDeparts(departsId, docId);
                //更新证书
                bool rlt2 = cbl.ChangeDoctorCertbooks(certbooks, docId);

                return null;
            });

        }


        [AllowAnonymous]
        public IActionResult CheckingOccupationInfo()
        {
            return View();
        }


    }
}
