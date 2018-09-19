using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CHIS.Code.JwtAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CHIS.Api
{
    [Route("api/[controller]")]
    public class OauthController : Controller
    {
        private OAuthService _jwtAuth;

        public OauthController(OAuthService jwtAuth)
        {
            _jwtAuth = jwtAuth;
        }

        [HttpGet]
        public IActionResult Get(string appId, string secretKey)
        {
            if (appId == ThirthPartTestUser.AppId && secretKey == ThirthPartTestUser.SecretKey)
            {
                var token = _jwtAuth.GenerateToken(appId, ThirthPartTestUser.Name);

                return Ok(new {State = "success", Headers = "angel-auth", Token = token});
            }

            return Ok(new { State = "fail"});

        }

        [HttpGet("[action]")]
        [Authorize("ThirdPartAuth")]
        public IActionResult Test()
        {
            return Ok(User.Identity.Name);
        }

        private class ThirthPartTestUser
        {
            public static string AppId = "123456";

            public static string SecretKey = "123456";

            public static string Name = "XX体检中心";
        }

    }
}
