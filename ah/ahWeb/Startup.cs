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
            #region 验证配置
            services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme =
                options.DefaultAuthenticateScheme = Global.AUTHENTICATION_SCHEME;// 这两个最好保持一样，否则出错
                options.DefaultSignInScheme = Global.SIGN_IN_SCHEME;

            }).AddCookie(Global.AUTHENTICATION_SCHEME, m => {
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

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

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
            //增加驗證
            app.UseAuthentication();





            app.UseMvc(routes =>
            {
                //业务系统的Area
                routes.MapRoute(
                    name: "business",
                    template: "{area:exists}/{controller}/{action}",
                    defaults: new { controller = "Home", action = "MyReservationList" });



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
    }
}
