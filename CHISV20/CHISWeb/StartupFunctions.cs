
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CHIS
{
    public partial class StartupFunctions
    {
        IServiceCollection services;
        IConfiguration Configuration;
        public StartupFunctions(IServiceCollection services, IConfiguration config)
        {
            this.services = services;
            Configuration = config;
        }

        /// <summary>
        /// 添加JWT验证
        /// </summary>
        public void AddJwtAuth()
        {
            string auConfig = "JwtAuthSettings";
            string headerName = "angel-auth";

            services.Configure<Code.JwtAuth.JwtAuthSettings>(Configuration.GetSection(auConfig));

            var jwtAuthSettings = new Code.JwtAuth.JwtAuthSettings();
            Configuration.Bind(auConfig, jwtAuthSettings);

         
            // Needed for jwt auth.
            // Enable the use of an  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]   attribute on methods and classes to protect.
            services.AddAuthentication().AddJwtBearer(o =>
            {
               // o.SecurityTokenValidators.Clear();//将SecurityTokenValidators清除掉，否则它会在里面拿验证

                o.Events = new JwtBearerEvents
                {
                    //重写OnMessageReceived
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Headers[headerName];
                        context.Token = token.FirstOrDefault();
                        return Task.CompletedTask;
                    }
                };

                o.SecurityTokenValidators.Add(new Code.JwtAuth.AHJwtValidator());

                //o.RequireHttpsMetadata = false;
                //o.SaveToken = true;
                o.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuthSettings.SecurityKey)),
                    ValidAudience = jwtAuthSettings.Audience,
                    ValidIssuer = jwtAuthSettings.Issuer,
                    // When receiving a token, check that we've signed it.
                    ValidateIssuerSigningKey = true,
                    // When receiving a token, check that it is still valid.
                    ValidateLifetime = true,
                    // This defines the maximum allowable clock skew - i.e. provides a tolerance on the token expiry time 
                    // when validating the lifetime. As we're creating the tokens locally and validating them on the same 
                    // machines which should have synchronised time, this can be set to zero. and default value will be 5minutes
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

            });
            #region Policy
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("ThirdPartAuth", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser().RequireRole(jwtAuthSettings.Role).Build());
                auth.AddPolicy("LoginUserAuth", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser().RequireRole(jwtAuthSettings.Role).Build());
            });
#endregion
        }

        /// <summary>
        /// 添加Swagger
        /// </summary>
        public void AddSwagger()
        {
            //添加Swagger.
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v2", new Swashbuckle.AspNetCore.Swagger.Info
                {
                    Version = "v2",
                    Title = "天使药店系统API",
                    Description = "Powered By 天使信息"
                });

                c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
                {
                    Version = "v1",
                    Title = "天使健康云系统 OPEN API",
                    Description = "Powered By 天使信息",

                });




                c.IncludeXmlComments(System.IO.Path.Combine(
                    Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationBasePath,
                    "CHISWeb.xml"));
                c.IncludeXmlComments(System.IO.Path.Combine(
                    Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationBasePath,
                    "CHIS.Models.xml"));

                c.OperationFilter<Code.Swagger.SwaggerTokenFilter>();

                //排序
                c.OrderActionsBy((apiDesc) =>
                {
                    return $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}";
                });
                

            });


        }

    }
}
