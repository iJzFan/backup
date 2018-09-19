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
using ah.DbContext;
using Microsoft.EntityFrameworkCore;
using ah.Services;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.AspNetCore.StaticFiles;
using Senparc.Weixin.Entities;
using Microsoft.Extensions.Options;
using Senparc.Weixin.MP.Containers;
using ah.WeChatService.Utilities;
using Senparc.Weixin.Threads;
using ah.Models.DataModel;
using ah.WeChatService.MessageHandlers.CustomMessageHandler;

namespace ah
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Global.Config = Configuration = builder.Build(); //复制一个全局的配置器好实用，不采用依赖注入
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region 添加数据库配置
            //services.AddSingleton<IConfiguration>(Configuration);
            services.AddDbContext<AHMSEntitiesSqlServer>(
                options => options.UseSqlServer(Configuration.GetConnectionString("SqlConnection")));
            // services.AddScoped<AccessService>();

            services.AddTransient<MongoLogContext>();
            #endregion

            #region Swagger

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = "JK813 API 文档"
                });
                c.IncludeXmlComments(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "ah.CustomerWeb.xml"));//Controller方法注释
                c.IncludeXmlComments(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "ah.Models.xml"));//属性注释
                c.DescribeAllEnumsAsStrings();//将枚举显示为字符串
            }
       );
            #endregion

            //添加各项配置文件
            services.Configure<SenparcWeixinSetting>(Configuration.GetSection("SenparcWeixinSetting"));
            services.Configure<MongoLog>(Configuration.GetSection("MongoLog"));
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
            #endregion

            // Add framework services.
            services.AddMvc()
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

            services.AddTransient<Code.Managers.IUserFrameMgr, Code.Managers.CustomerFrameManager>();//依赖注入一个用户框架获取帮助器
            services.AddTransient<Code.Managers.IMyRazor, Code.Managers.MyRazor>();//依赖注入一个前端工具

            services.AddTransient<BaseService>();
            services.AddTransient<FollowListService>();
            services.AddTransient<GetInfoService>();
            services.AddTransient<GiftService>();
            services.AddTransient<ReservationService>();
            services.AddTransient<weixinService>();
            services.AddTransient<MongoLogService>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();//注入上下文信息

            //跨域请求
            services.AddCors(opt =>
            {
                opt.AddPolicy("chisOrigin", b =>
                {
                    b.WithOrigins(
                        "http://localhost:61450",
                        "http://192.168.99.138:61450",//rex机器
                        "http://192.168.99.199:61450",//晓峰机器
                        "http://chis.jk213.com");
                    b.WithMethods("GET", "POST");
                    b.AllowAnyHeader();
                });
            });

            services.AddMemoryCache();
            services.AddSession();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IOptions<SenparcWeixinSetting> senparcWeixinSetting)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                //app.UseExceptionHandler("/Home/Error");
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
//添加允许的静态文件
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings.Add(".less", "text,css");
            app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
                                 Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
            });

            //使用跨域
            app.UseCors("chisOrigin");


            //增加驗證
            app.UseAuthentication();

            #region 配置Swagger

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "JK813 API文档");
            });

            #endregion

            #region 微信相关

            //Senparc.Weixin SDK 配置
            Senparc.Weixin.Config.IsDebug = true;
            Senparc.Weixin.Config.DefaultSenparcWeixinSetting = senparcWeixinSetting.Value;

            //提供网站根目录
            if (env.ContentRootPath != null)
            {
                Senparc.Weixin.Config.RootDictionaryPath = env.ContentRootPath;
                Server.AppDomainAppPath = env.ContentRootPath;// env.ContentRootPath;
            }
            Server.WebRootPath = env.WebRootPath;// env.ContentRootPath;

            /* 微信配置开始
 * 
 * 建议按照以下顺序进行注册，尤其须将缓存放在第一位！
 */

            ConfigWeixinTraceLog();     //配置微信跟踪日志
            RegisterWeixinThreads();    //激活微信缓存及队列线程（必须）
            RegisterSenparcWeixin();    //注册Demo所用微信公众号的账号信息（按需）

            /* 微信配置结束 */

            #endregion

            app.UseSession();

            app.UseMvc(routes =>
            {
                //业务系统的Area
                routes.MapRoute(
                    name: "business",
                    template: "{area:exists}/{controller}/{action}",
                    defaults: new { controller = "Home", action = "Index" });



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



            });
        }

        ///<summary>
        /// 配置微信跟踪日志
        /// </summary>
        private void ConfigWeixinTraceLog()
        {
            //这里设为Debug状态时，/App_Data/WeixinTraceLog/目录下会生成日志文件记录所有的API请求日志，正式发布版本建议关闭
            Senparc.Weixin.Config.IsDebug = true;
            Senparc.Weixin.WeixinTrace.SendCustomLog("系统日志", "系统启动");//只在Senparc.Weixin.Config.IsDebug = true的情况下生效

            //自定义日志记录回调
            Senparc.Weixin.WeixinTrace.OnLogFunc = () =>
            {
                //加入每次触发Log后需要执行的代码
            };

            //当发生基于WeixinException的异常时触发
            Senparc.Weixin.WeixinTrace.OnWeixinExceptionFunc = ex =>
            {
                //加入每次触发WeixinExceptionLog后需要执行的代码

                //发送模板消息给管理员
                var eventService = new WeChatService.EventService();
                eventService.ConfigOnWeixinExceptionFunc(ex);
            };
        }

        /// <summary>
        /// 激活微信缓存
        /// </summary>
        private void RegisterWeixinThreads()
        {
            ThreadUtility.Register();//如果不注册此线程，则AccessToken、JsTicket等都无法使用SDK自动储存和管理。
        }


        /// <summary>
        /// 注册所用微信公众号的账号信息
        /// </summary>
        private void RegisterSenparcWeixin()
        {
            var senparcWeixinSetting = Senparc.Weixin.Config.DefaultSenparcWeixinSetting;

            //注册公众号
            AccessTokenContainer.Register(
                senparcWeixinSetting.WeixinAppId,
                senparcWeixinSetting.WeixinAppSecret,
                "【健康813】公众号");
        }
    }
}
