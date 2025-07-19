using ASP.Data;
using ASP.Middleware.Auth;
using ASP.Services.Email;
using ASP.Services.Identity;
using ASP.Services.Kdf;
using ASP.Services.Random;
using ASP.Services.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace ASP
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddJsonFile("emailsettings.json");

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddSingleton<IRandomService, DefaultRandomService>();
            builder.Services.AddSingleton<ITimeService, MilliSecTimeService>();
            builder.Services.AddSingleton<IIdentityService, DefaultIdentityService>();
            builder.Services.AddSingleton<IKdfService, PbKdfService>();
            builder.Services.AddSingleton<IEmailService, GmailService>();

            builder.Services.AddDbContext<DataContext>(
                options => options.UseSqlServer(builder.Configuration.GetConnectionString("LocalDB")) 
            );

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>  { options.IdleTimeout = TimeSpan.FromSeconds(100);
                options.Cookie.HttpOnly = true; options.Cookie.IsEssential = true; });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthToken();
            app.UseAuthorization();
            app.UseSession();

            app.MapStaticAssets();

            app.UseAuthSession();
            

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            using (var scope = app.Services.CreateScope()) { var db = scope.ServiceProvider.GetRequiredService<DataContext>(); await db.Database.MigrateAsync(); }

            app.Run();
        }
    }
}
