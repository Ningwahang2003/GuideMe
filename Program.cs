using GuideMe.Models;
using GuideMe.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;

namespace GuideMe
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(@"C:\keys"))
                .SetApplicationName("GuideMe")
                .SetDefaultKeyLifetime(TimeSpan.FromDays(90));


         
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpClient();
            builder.Services.AddDbContext<GuideMeContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DbConn")));
            builder.Services.AddDataProtection();


            builder.Services.AddSingleton<DataSecurity>();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o => o.LoginPath = "/Authentication/LogIn");

            builder.Services.AddSession(a =>
            {
                a.IdleTimeout = TimeSpan.FromMinutes(1);
                a.Cookie.HttpOnly = true;
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "weather",
                pattern: "Geographic/Weather/{location?}",
                defaults: new { controller = "Geographic", action = "Weather" }
            );

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Static}/{action=Index}/{id?}");

            app.Run();
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddControllersWithViews();
            services.AddHttpClient();
        }
    }
}
