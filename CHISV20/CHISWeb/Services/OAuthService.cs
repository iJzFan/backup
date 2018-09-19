using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using CHIS.Models;
using CHIS.Models.ViewModel;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CHIS.Code.JwtAuth
{
    public class OAuthService
    {
        private JwtAuthSettings _settings;

        public OAuthService(IOptions<JwtAuthSettings> setting)
        {
            _settings = setting.Value;
        }

        /// <summary>
        ///生成jwt
        /// </summary>
        /// <param name="id"></param>
        /// <param name="loginName"></param>
        /// <param name="expires"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        public Token GenerateToken(string id, string loginName, TimeSpan? expires = null)
        {
            var handler = new JwtSecurityTokenHandler();

            ClaimsIdentity identity = new ClaimsIdentity(
                new GenericIdentity(Global.JWT_LOGIN_USER_CLAIMS_IDENTITY),
                new[] { new Claim(ClaimTypes.Sid, id), new Claim(ClaimTypes.Role, _settings.Role) }
            );

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecurityKey));
            var SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _settings.Issuer,
                Audience = _settings.Audience,
                SigningCredentials = SigningCredentials,
                Subject = identity,
                Expires = expires.HasValue ? DateTime.Now.Add(expires.Value) : DateTime.Now.AddHours(_settings.ExpiresHours)
            });
            return new Token { AccessToken = handler.WriteToken(securityToken), ExpiresTime = securityToken.ValidTo };
        }

        /// <summary>
        /// 用户登录Token
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <param name="expires"></param>
        /// <returns></returns>
        public Token GenerateUserLoginToken(UserSelf dd, TimeSpan? expires = null)
        {
            var handler = new JwtSecurityTokenHandler();

            //添加各种验证
            var claims = new List<Claim>();
            Action<string, object> claimsAdd = (key, val) =>
            {
                string v = "";
                if (val is DateTime || val is DateTime?) v = ((DateTime)val).Ticks.ToString();
                else if (val is IEnumerable<int>) v = string.Join(",", (IEnumerable<int>)val);
                else if (val is IEnumerable<string>) v = string.Join(",", (IEnumerable<string>)val);
                else v = Ass.P.PStr(val);
                claims.Add(new Claim(key, v));
            };

            //公共部分
            claimsAdd(ClaimTypes.Role, _settings.Role);


            claims.Add(new Claim(ClaimTypes.NameIdentifier, dd.CustomerId.ToString(), ClaimValueTypes.Integer, Global.AUTHENTICATION_ISSUER));
            claims.Add(new Claim(ClaimTypes.Name, dd.CustomerName ?? "", ClaimValueTypes.String, Global.AUTHENTICATION_ISSUER));
            // claims.Add(new Claim(ClaimTypes.Role, userLoginData.RoleName ?? "", ClaimValueTypes.String, Global.AUTHENTICATION_ISSUER));             

            claimsAdd("LoginId", dd.LoginId);
            claimsAdd("OpId", dd.CustomerId);
            claimsAdd("DoctorId", dd.DoctorId);
            claimsAdd("OpMan", dd.CustomerName);
            claimsAdd("Gender", dd.Gender);
            claimsAdd("Birthday", dd.Birthday);

            claimsAdd("PostTitleName", dd.PostTitleName);
            claimsAdd("PhotoUrlDef", dd.PhotoUrlDef);
            claimsAdd("DoctorAppId", dd.DoctorAppId);//app端的用户Id

            claimsAdd("StationId", dd.StationId);
            claimsAdd("DrugStoreStationId", dd.DrugStoreStationId);//药品药房Id
            claimsAdd("StationName", dd.StationName);//工作站名称
            claimsAdd("StationTypeId", dd.StationTypeId);
            claimsAdd("LoginTime", DateTime.Now);
            claimsAdd("IsCanTreat", dd.IsCanTreat);
            claimsAdd("IsManageUnit", dd.IsManageUnit);
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



            var userIdentity = new ClaimsIdentity(Global.JWT_LOGIN_USER_CLAIMS_IDENTITY);//其他都可以，主要獲取時候方便
            userIdentity.AddClaims(claims);

            //ClaimsIdentity identity = new ClaimsIdentity(
            //    new GenericIdentity(loginInfo.CustomerId),
            //    new[] { new Claim(ClaimTypes.Sid, id), new Claim(ClaimTypes.Role, _settings.Role) }
            //);

            var skey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecurityKey));
            var SigningCredentials = new SigningCredentials(skey, SecurityAlgorithms.HmacSha256);

            var securityToken = handler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _settings.Issuer,
                Audience = _settings.Audience,
                SigningCredentials = SigningCredentials,
                Subject = userIdentity,
                Expires = expires.HasValue ? DateTime.Now.Add(expires.Value) : DateTime.Now.AddHours(_settings.ExpiresHours)
            });
            return new Token { AccessToken = handler.WriteToken(securityToken), ExpiresTime = securityToken.ValidTo };
        }

    }


    public class JwtAuthSettings
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string SecurityKey { get; set; }
        public string Role { get; set; }
        public int ExpiresHours { get; set; }
    }
}
