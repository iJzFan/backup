using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using CHIS.Code.WebSocketsMiddleware;
using Alipay.AopSdk.AspnetCore;
using Alipay.AopSdk.F2FPay.AspnetCore;
using Microsoft.EntityFrameworkCore;
using CHIS.Services;
using NLog.Web;
using NLog.Extensions.Logging;
using NLog;
using AutoMapper;
using CHIS.Code.Mapping;
using Microsoft.AspNetCore.HttpOverrides;
using Senparc.Weixin.MP.Containers;
using Microsoft.Extensions.Options;
using CHIS.Codes.Utility.XPay;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using CHIS.Code.JwtAuth;

namespace CHIS
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Global.Config = Configuration = configuration; //复制一个全局的配置器好实用，不采用依赖注入
            Global.Initial().Wait(Global.WAIT_MSEC);// 初始化数据信息           
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var startupFunc = new StartupFunctions(services, Configuration);
            #region 添加数据库配置
            //services.AddSingleton<IConfiguration>(Configuration);
            services.AddDbContext<DbContext.CHISEntitiesSqlServer>(
                options => options.UseSqlServer(Configuration.GetConnectionString("SqlConnection")));

            #endregion

            #region Config注入

            services.AddAutoMapper(typeof(GiftProfile), typeof(PointsDetailProfile), typeof(GiftOrderProfile));
            services.Configure<WXParams>(Configuration.GetSection("WXParamsJK813"));
            services.Configure<MongoLog>(Configuration.GetSection("MongoLog"));
            services.Configure<Code.JwtAuth.JwtAuthSettings>(Configuration.GetSection("JwtAuthSettings"));
            #endregion

            #region Service注入

            services.AddScoped<DictService>();//全局字典服务
            services.AddScoped<ChangePayService>();
            services.AddScoped<DispensingService>();
            services.AddScoped<DrugService>();
            services.AddScoped<PharmacyService>();
            services.AddScoped<DoctorService>();
            services.AddScoped<CustomerService>();
            services.AddScoped<TreatService>();
            services.AddScoped<ReservationService>();
            services.AddScoped<WorkStationService>();
            services.AddScoped<JKWebNetService>();
            services.AddScoped<RxService>();
            services.AddScoped<LoginService>();
            services.AddScoped<NotifyService>();
            services.AddScoped<AccessService>();
            services.AddScoped<FollowListService>();
            services.AddScoped<MongoLogContext>();
            services.AddScoped<MongoLogService>();
            services.AddScoped<GiftService>();
            services.AddScoped<PointsDetailService>();
            services.AddScoped<GiftOrderService>();
            services.AddScoped<WeChatService>();
            services.AddSingleton<OAuthService>();
            services.AddScoped<Code.Managers.IMyLogger, Code.Managers.MyLogger>();//添加日志写入

            #endregion


            services.AddMemoryCache();//添加缓存IMemoryCache         


            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();//注入上下文信息

            //添加Swagger
            startupFunc.AddSwagger();
             


            #region 验证配置
            services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme =
                options.DefaultAuthenticateScheme = Global.AUTHENTICATION_SCHEME;// 这两个最好保持一样，否则出错
                options.DefaultSignInScheme = Global.SIGN_IN_SCHEME;

            }).AddCookie(Global.AUTHENTICATION_SCHEME, m =>
            {
                m.LoginPath = new PathString(Global.ROUTE_LOGIN);
                m.AccessDeniedPath = new PathString(Global.ROUTE_FORBIDDEN);
                m.LogoutPath = new PathString(Global.ROUTE_LOGOUT);
                m.Cookie.Path = "/";
            });

            //添加jwt验证
            startupFunc.AddJwtAuth();




            #endregion






            //app.UseCookieAuthentication(new CookieAuthenticationOptions()
            //{
            //    AuthenticationScheme = Global.AUTHENTICATION_SCHEME,
            //    CookieHttpOnly = true,
            //    CookieName = Global.AUTHENTICATION_COOKIE_NAME,
            //    LoginPath = new Microsoft.AspNetCore.Http.PathString(Global.ROUTE_LOGIN),
            //    AccessDeniedPath = new PathString(Global.ROUTE_FORBIDDEN),//解決訪問的路徑
            //    AutomaticAuthenticate = true,//如果要跳轉，必須設置
            //    AutomaticChallenge = true,
            //    CookiePath = "/"

            //});
 

            //跨域请求
            services.AddCors(opt =>
            {
                opt.AddPolicy("jk213Origin", b =>
                {
                    b.WithOrigins(
                        "http://localhost:61448",
                        "http://192.168.99.138:61448",//rex机器
                        "http://192.168.99.199:61448",//晓峰机器
                        "http://my.jk213.com");
                    b.WithMethods("GET", "POST");
                    b.AllowAnyHeader();
                });
            });



            // Add framework services.
            services.AddMvc(options =>
            {
                // options.Filters.Add(typeof(FuncAccessFilterAttribute));   
                //添加缓存设置
                options.CacheProfiles.Add("Default", new Microsoft.AspNetCore.Mvc.CacheProfile
                {
                    Duration = 60
                });
                options.CacheProfiles.Add("Never", new Microsoft.AspNetCore.Mvc.CacheProfile
                {
                    Location = Microsoft.AspNetCore.Mvc.ResponseCacheLocation.None,
                    NoStore = true
                });

                options.Conventions.Add(new Code.Swagger.ApiExplorerGroupPerVersionConvention());
            })
                //全局配置Json序列化处理
                .AddJsonOptions(options =>
                {
                    //忽略循环引用
                    //   options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    //不使用驼峰样式的key
                    //   options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    //设置时间格式
                    options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                });

            services.AddScoped<FuncAccessFilterAttribute>();//添加局部的过滤器
                                                            //过滤器的内容参考 http://www.cnblogs.com/niklai/p/5676632.html

            //  services.AddTransient<Code.Managers.IUserFrameMgr, Code.Managers.UserFrameManager>();//依赖注入一个用户框架获取帮助器
            services.AddTransient<Code.Managers.IMyRazor, Code.Managers.MyRazor>();//依赖注入一个前端工具

            services.AddAlipay(options =>
            {
                options.AlipayPublicKey = Configuration["Alipay:AlipayPublicKey"];
                options.AppId = Configuration["Alipay:AppId"];
                options.CharSet = Configuration["Alipay:CharSet"];
                options.Gatewayurl = Configuration["Alipay:Gatewayurl"];
                options.PrivateKey = Configuration["Alipay:PrivateKey"];
                options.SignType = Configuration["Alipay:SignType"];
                options.Uid = Configuration["Alipay:Uid"];
            }).AddAlipayF2F();
            services.AddScoped<Codes.Utility.XPay.AliPay>();//依赖注入AliPay


            services.AddResponseCaching(options =>
            {
                options.UseCaseSensitivePaths = true;
                options.MaximumBodySize = 1024;
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IOptionsSnapshot<WXParams> wxParams)
        {
            env.ConfigureNLog($"nlog.{env.EnvironmentName}.config");


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseCors("jk213Origin");

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            //增加驗證
            app.UseAuthentication();


            //添加WebSocket
            //app.UseWebSockets(new WebSocketOptions()
            //{
            //    KeepAliveInterval = TimeSpan.FromSeconds(120),
            //    ReceiveBufferSize = 4 * 1024
            //});
            //app.Map("/ws-mobile-pay-station-monitor", (appx) => { appx.UseWebSocketPayMonitor(); });
            //app.Map("/ws-mobile-pays-monitor", (appx) => { appx.UseWebSocketPayMonitor(); });            


            app.UseResponseCaching();


            #region 配置Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v2/swagger.json", "用于药店的 API");
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "OPENAPI V1");
                c.DefaultModelsExpandDepth(2);
            });
            //app.UseSwaggerUI(c => {
            //    c.SwaggerEndpoint("/swagger/app/swagger.json", "app接口 V1");
            //});

            #endregion


            #region 配置公众号
            AccessTokenContainer.Register(
                            wxParams.Value.AppId,
                            wxParams.Value.AppSecret,
                            "JK813");
            #endregion

            app.UseMvc(routes =>
            {
                //二维码扫描短地址
                routes.MapRoute(
                    name: "qrScan",
                    template: "qs",
                    defaults: new { controller = "Tools", action = "QrScan" });

                //业务系统的Area
                routes.MapRoute(
                    name: "business",
                    template: "{area:exists}/{controller}/{action}",
                    defaults: new { controller = "Home", action = "My" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                //基础Json数据接口
                routes.MapRoute(
                  name: "basejson",
                  template: "{area:exists}/basejson/{controller}/{action}",
                  defaults: new { controller = "Home", action = "Index" });

                //接口类
                routes.MapRoute(
                   name: "api",
                   template: "api/{controller}/{action}",
                   defaults: new { controller = "Common", action = "Index" });

                //接口类
                routes.MapRoute(
                   name: "openapi",
                   template: "openapi/{controller}/{action}",
                   defaults: new { controller = "Common", action = "Index" });
                //接口类
                routes.MapRoute(
                   name: "openapi_v2",
                   template: "openapi_v2/{controller}/{action}",
                   defaults: new { controller = "Common", action = "Index" });


                //sys系统配置
                routes.MapRoute(
                    name: "sys",
                    template: "sys/{controller}/{action}",
                    defaults: new { controller = "Home", action = "Index" });

            });
        }

    }
}
