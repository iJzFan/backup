using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace CHIS.Models.DataModel
{
    public class Doctor
    {
        public Models.CHIS_Code_Customer BaseInfo { get; set; }
        public Models.CHIS_Code_Doctor DoctorInfo { get; set; }
    }

    public class DoctorLogin
    {
        public DoctorLogin()
        {
            Login = new CHIS_Sys_Login();
            Doctor = new vwCHIS_Code_Doctor();
        }
        public Models.CHIS_Sys_Login Login { get; set; }
        public Models.vwCHIS_Code_Doctor Doctor { get; set; }


        public string LoginPwd
        {
            get { return Login.LoginPassword; }
            set { Login.LoginPassword = value; }
        }

        [Compare("LoginPwd", ErrorMessage = "密码与输入密码相等")]
        public string CheckLoginPwd { get; set; }


        public bool IsLoginToDoctor { get; set; } = true;
    }

    /// <summary>
    /// 诊断
    /// </summary>
    public class DiagnosisModel
    {
    

        public string DiagnoisisName { get; set; }

 	

        public string DiagnoisisValue { get; set; }
        public string TypeCode { get; set; } = "USERDEFINED";
 
    }
    public class vwDoctor
    {


        /// <summary>
        /// 医生的基本信息
        /// </summary>
        public Models.vwCHIS_Code_Doctor DoctorBase { get; set; }


        /// <summary>
        /// 医生的证件 职业证书
        /// </summary>
        public IEnumerable<Models.vwCHIS_Code_DoctorCertbook> MyCertificates { get; set; }


        /// <summary>
        /// 医生职业的门诊(所有，包括网络医生站)
        /// </summary>
        public IEnumerable<Models.vwCHIS_Code_Rel_DoctorDeparts> DoctorAllowedDeparts { get; set; }


        /// <summary>
        /// 医生职业的一般门诊(不包括网络医生站)
        /// </summary>
        public IEnumerable<Models.vwCHIS_Code_Rel_DoctorDeparts> DoctorAllowedDepartsNormal
        {
            get
            {
                return DoctorAllowedDeparts?.Where(m => m.StationID != MPS.NetStationId);
            }
        }



        private bool? isNetDoctor = null;
        /// <summary>
        /// 是否是网上医生
        /// </summary>
        public bool IsNetDoctor
        {
            get
            {
                if (isNetDoctor == null)
                {
                    if (DoctorAllowedDeparts != null)
                    {
                        isNetDoctor = DoctorAllowedDeparts.Any(m => m.StationID == MPS.NetStationId);
                        return isNetDoctor.Value;
                    }
                    else return false;
                }
                else
                    return isNetDoctor.Value;
            }
            set => isNetDoctor = value;

        }
        /// <summary>
        /// 我的网上门诊
        /// </summary>
        public IEnumerable<Models.vwCHIS_Code_Rel_DoctorDeparts> MyNetDepartments
        {
            get
            {
                return DoctorAllowedDeparts?.Where(m => m.StationID == MPS.NetStationId);
            }
        }
        /// <summary>
        /// 所有的网上门诊部门
        /// </summary>
        public IEnumerable<vwCHIS_Code_Department> NetDepartments { get; set; }


    }


    /// <summary>
    /// 医生基础类00
    /// </summary>
    public class DoctorSEntityV00
    {
        public int DoctorOpId { get { return CustomerId; } }
        public int CustomerId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorGender { get; set; }
        public string PostTitleName { get; set; }
        public string DoctorSkillRmk { get; set; }
        public string DoctorPhotoUrl { get; set; }
         
        /// <summary>
        /// 医生App端的Id
        /// </summary>
        public string DoctorAppId { get; set; }
    }

    /// <summary>
    /// 医生扩展类01
    /// </summary>
    public class DoctorSEntityV01:DoctorSEntityV00
    {        
        /// <summary>
        /// 默认科室
        /// </summary>
        public int? DefDepartmentId { get; set; } 
    }

    public class DoctorSEntityV02 : DoctorSEntityV00
    {
        public bool IsRxDefault { get; set; }
        public int StationId { get; set; }
    }

   /// <summary>
   /// 用于统一工作站医生和其他处方咨询医生
   /// </summary>
    public class DoctorSEntityV03 : DoctorSEntityV01
    {
        /// <summary>
        /// 是否是处方医生
        /// </summary>
        public bool IsRxDoctor { get{ return IsRxDefault.HasValue; } }
        /// <summary>
        /// 是否是默认处方医生
        /// </summary>
        public bool? IsRxDefault { get; set; }
        /// <summary>
        /// 处方医生所在的工作站
        /// </summary>
        public int? StationId { get; set; }
    }

}
