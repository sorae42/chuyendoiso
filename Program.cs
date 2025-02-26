using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using chuyendoiso.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using chuyendoiso.Models;
using System;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<chuyendoisoContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("chuyendoisoContext") ?? throw new InvalidOperationException("Connection string 'chuyendoisoContext' not found.")));

// Config cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Home/Index";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<chuyendoisoContext>();

    if (!context.Auth.Any())
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("admin123");
        context.Auth.Add(new Auth { Username = "admin", Password = hashedPassword, Email = "nhoanghai2003@gmail.com", Phone = "123456789" });
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// For dev and showcases only, uncomment before prod
//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();