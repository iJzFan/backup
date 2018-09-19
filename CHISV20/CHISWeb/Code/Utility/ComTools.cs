using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CHIS.Code.Utility
{
    public class ComTools
    {
        /// <summary>
        /// 生成随机数
        /// </summary>
        private static char[] constant =
        {
        '0','1','2','3','4','5','6','7','8','9',
        'a','b','c','d','e','f','g','h','j','k','m','n','p','r','s','t','u','v','w','x','y','z',
        'A','B','C','D','E','F','G','H','J','K','L','M','N','P','Q','R','S','T','U','V','W','X','Y','Z'
        };
        public static string GenerateRandomNumber(int Length,bool bChars=false)
        {
            System.Text.StringBuilder newRandom = new System.Text.StringBuilder(62);
            Random rd = new Random();
            int num = bChars ? 56 : 10;
            for (int i = 0; i < Length; i++)
            {
                newRandom.Append(constant[rd.Next(num)]);
            }
            return newRandom.ToString();
        }
        #region 身份证验证

        /// <summary>
        /// 根据身份证获取身份证信息
        /// 18位身份证
        /// 0地区代码(1~6位,其中1、2位数为各省级政府的代码，3、4位数为地、市级政府的代码，5、6位数为县、区级政府代码)
        /// 1出生年月日(7~14位)
        /// 2顺序号(15~17位单数为男性分配码，双数为女性分配码)
        /// 3性别
        /// 
        /// 15位身份证
        /// 0地区代码 
        /// 1出生年份(7~8位年,9~10位为出生月份，11~12位为出生日期 
        /// 2顺序号(13~15位)，并能够判断性别，奇数为男，偶数为女
        /// 3性别
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public static string[] GetCardIdInfo(string cardId)
        {
            string[] info = new string[4];
            System.Text.RegularExpressions.Regex regex = null;
            if (cardId.Length == 18)
            {
                regex = new System.Text.RegularExpressions.Regex(@"^\d{17}(\d|x|X)$");
                if (regex.IsMatch(cardId))
                {
                    info.SetValue(cardId.Substring(0, 6), 0);
                    info.SetValue(cardId.Substring(6, 8), 1);//生日
                    info.SetValue(cardId.Substring(14, 3), 2);
                    info.SetValue(Convert.ToInt32(info[2]) % 2 != 0 ? "男" : "女", 3);
                }
            }
            else if (cardId.Length == 15)
            {
                regex = new System.Text.RegularExpressions.Regex(@"^\d{15}");
                if (regex.IsMatch(cardId))
                {
                    info.SetValue(cardId.Substring(0, 6), 0);
                    info.SetValue("19" + cardId.Substring(6, 6), 1); //生日
                    info.SetValue(cardId.Substring(12, 3), 2);
                    info.SetValue(Convert.ToInt32(info[2]) % 2 != 0 ? "男" : "女", 3);
                }
            }
            else throw new Exception("身份证号码位数错误");
            if (info[1].Length == 8)
            {
                var birthday = info[1];
                string nbirthday = birthday.Substring(0, 4) + "-" + birthday.Substring(4, 2) + "-" + birthday.Substring(6, 2);
                info.SetValue(nbirthday, 1);
            }
            return info;

        }
       /// 身份证号码的合法性
           public static bool IsIDCard(string idNuber)
        {
            if (idNuber.Length == 18)
            {               
                return CheckIDCard18(idNuber);
            }
            else if (idNuber.Length == 15)
            {
                return CheckIDCard15(idNuber);
            }
            else
            {

              throw new Exception("不合法身份号码");
            }        
         }
        /// <summary> 
        /// 18位身份证号码验证 
        /// </summary> 
        public static bool CheckIDCard18(string idNumber)
        {
            long n = 0;
            if (long.TryParse(idNumber.Remove(17), out n) == false
                || n < Math.Pow(10, 16) || long.TryParse(idNumber.Replace('x', '0').Replace('X', '0'), out n) == false)
            {
                return false;//数字验证 
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(idNumber.Remove(2)) == -1)
            {
                return false;//省份验证 
            }
            string birth = idNumber.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证 
            }
            string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            char[] Ai = idNumber.Remove(17).ToCharArray();
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
            }
            int y = -1;
            y=sum % 11;
            if (arrVarifyCode[y] != idNumber.Substring(17, 1).ToLower())
            {
                return false;//校验码验证 
            }
            return true;//符合GB11643-1999标准 
        }

        /// <summary> 
        /// 16位身份证号码验证 
        /// </summary> 
        public static bool CheckIDCard15(string idNumber)
        {
            long n = 0;
            if (long.TryParse(idNumber, out n) == false || n < Math.Pow(10, 14))
            {
                return false;//数字验证 
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(idNumber.Remove(2)) == -1)
            {
                return false;//省份验证 
            }
            string birth = idNumber.Substring(6, 6).Insert(4, "-").Insert(2, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证 
            }
            return true;
        }

        #endregion

        #region  
        //验证手机的合法性
        public static bool IsTelephone(string strTelephone)

        {
            if (string.IsNullOrEmpty(strTelephone)) return false;
            if (strTelephone.Length != 11) throw new Exception("不合法手机号码");          
            return System.Text.RegularExpressions.Regex.IsMatch(strTelephone, @"^[1]+[3,4,5,7,8,9]+\d{9}");
        }
        #endregion

        #region
        /// <summary>
        /// 判断输入的字符串是否是一个合法的Email地址
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsEmail(string input)
        {

            return true;
        }
        #endregion
        # region 获取发票号码
        /// <summary>
        /// 获取发票号码
        /// </summary>
        /// <param name="InvoiceNO"></param>
        /// <returns></returns>
        public static String GetInvoice(string InvoiceNO)
        {
            string StartNO = (Convert.ToInt32(InvoiceNO) + 1).ToString();
            int len = InvoiceNO.Length - StartNO.Length;
            string Zero = "";
            for (int i = 0; i < len; i++)
            {
                Zero += "0";
            }
            return Zero + StartNO;
        }
        #endregion
        public static string GetAreaName(string areaMergerName, int index)
        {
            try
            {
                string[] s = areaMergerName.Split(' ');
                return s[index];
            }
            catch { return ""; }
        }
    }

}
