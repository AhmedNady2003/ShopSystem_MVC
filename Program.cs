using Microsoft.EntityFrameworkCore;
using ShopWebMVC.Context;
using ShopWebMVC.Models;

namespace ShopWebMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add DbContext with SQL Server connection string
            builder.Services.AddDbContext<ShopDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ShopCon")));

            

            // Configure Session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout duration
                options.Cookie.HttpOnly = true;  // Set cookie to be HttpOnly for security
                options.Cookie.IsEssential = true; // Set session cookie to be essential for the app
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                // Use the exception handler page in non-development environments
                app.UseExceptionHandler("/Home/Error");
                // Enforce HTTPS
                app.UseHsts();
            }

            // Ensure HTTPS redirection is enabled for all requests
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Set up routing for controllers and views
            app.UseRouting();

            // Use Session middleware
            app.UseSession();

            // Use Authorization middleware
            app.UseAuthorization();

            // Map default route to Home controller and Index action
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
