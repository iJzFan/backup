using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ah.Models
{
    public partial class vwCHIS_Code_Doctor
    {
        /// <summary>
        /// 医生的头像图片
        /// </summary>
        public string PhotoUrlDef
        {
            get
            {
                var s = DoctorPhotoUrl;
                if (string.IsNullOrEmpty(s))
                {
                    s = $"_d256_{Gender}.png";
                }
                return s;
            }
        }
    }


    public class PhotoUtils
    {
        /// <summary>
        /// 获取人员的默认头像图片
        /// </summary>
        /// <param name="dburl">数据库内的Url</param>
        /// <param name="gender">性别</param>
        public static string CustomerPhotoUrlDef(string dburl, int? gender)
        {
            var s = dburl;
            if (string.IsNullOrEmpty(s))
            {
                s = $"_u256_{gender}.png";
            }
            return s;
        }
    }

    public partial class vwCHIS_Sys_Login
    {
        /// <summary>
        /// 医生的头像图片
        /// </summary>
        public string DoctorPhotoUrlDef
        {
            get
            {
                var s = DoctorPhotoUrl;
                if (string.IsNullOrEmpty(s))
                {
                    if (DoctorId > 0) return $"_d256_{Gender}.png";
                    else return "";
                }
                return s;
            }
        }
        public string CustomerPhotoUrlDef
        {
            get
            {
                return PhotoUtils.CustomerPhotoUrlDef(CustomerPhoto, Gender);
            }
        }
    }

    public partial class vwCHIS_Code_Customer
    {
        /// <summary>
        /// 头像图片
        /// </summary>
        public string PhotoUrlDef
        {
            get
            {
                return PhotoUtils.CustomerPhotoUrlDef(CustomerPhoto, Gender);
            }
        }
    }

    public partial class CHIS_Code_Customer
    {
        /// <summary>
        /// 头像图片
        /// </summary>
        public string PhotoUrlDef
        {
            get
            {
                string pic = string.IsNullOrEmpty(CustomerPic) ? WXPic : CustomerPic;
                return PhotoUtils.CustomerPhotoUrlDef(pic, Gender);
            }
        }
    }



    public partial class vwCHIS_Register
    {
        /// <summary>
        /// 头像图片
        /// </summary>
        public string PhotoUrlDef
        {
            get
            {                
                return PhotoUtils.CustomerPhotoUrlDef(CustomerPhoto, Gender);
            }
        }
    }

}
