using Archivr.UI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wkhtmltopdf.NetCore;

namespace Archivr.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // Add services to the container.

            string file = "./app_data.db";
            if (Environment.GetEnvironmentVariable("SUPERVISOR_TOKEN") != null)
            {
                builder.Configuration.AddJsonFile("/config/appsettings.json", optional: true, reloadOnChange: true);
                file = "/config/app_data.db";
            }
            
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite($"Data Source={file}"));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddControllersWithViews();
            builder.Services.AddWkhtmltopdf("Lib");
            builder.Services.AddScoped<GeneratePdf>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            using (var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
            {
                db.Database.Migrate();
            }

            //Enable relative path for HASSIO Ingress
            app.Use(async (context, next) =>
            {
                if (context.Request.Headers.ContainsKey("X-Ingress-Path"))
                {
                    context.Request.PathBase = context.Request.Headers["X-Ingress-Path"].ToString();
                }
                await next();
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}