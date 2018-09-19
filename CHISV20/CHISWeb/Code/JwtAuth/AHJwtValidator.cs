using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace CHIS.Code.JwtAuth
{

    /// <summary>
    /// 自定义Jwt验证
    /// </summary>
    public class AHJwtValidator : ISecurityTokenValidator
    {
        private JwtSecurityTokenHandler _tokenHandler;

        public AHJwtValidator()
        {
            _tokenHandler = new JwtSecurityTokenHandler();
        }

        public bool CanValidateToken
        {
            get
            {
                return true;
            }
        }

        public int MaximumTokenSizeInBytes { get; set; } = TokenValidationParameters.DefaultMaximumTokenSizeInBytes;

        public bool CanReadToken(string securityToken)
        {
            return _tokenHandler.CanReadToken(securityToken);
        }

        public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            

            var principal = _tokenHandler.ValidateToken(securityToken, validationParameters, out validatedToken);

            return principal;
        }
    }
}
