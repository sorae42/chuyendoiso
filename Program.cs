using Microsoft.EntityFrameworkCore;
using chuyendoiso.Data;
using chuyendoiso.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using chuyendoiso.Interface;
using chuyendoiso.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<chuyendoisoContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("chuyendoisoContext") ?? throw new InvalidOperationException("Connection string 'chuyendoisoContext' not found.")));

// Config cookie authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            RoleClaimType = ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    context.Token = authHeader.Substring("Bearer ".Length);
                }
                return Task.CompletedTask;
            }
        };
    });

var emailConfig = builder.Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
builder.Services.AddSingleton(emailConfig);
builder.Services.AddScoped<IEmailSender, EmailSender>();

// Add service HttpContextAccessor to take IP, Username from HttpContext
builder.Services.AddHttpContextAccessor();

// Add service LogService to log activity
builder.Services.AddScoped<LogService>();

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

Console.WriteLine("App is building...");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<chuyendoisoContext>();

    // Apply pending EF Core migrations automatically on startup
    context.Database.Migrate();

    if (!context.Auth.Any(u => u.Username == "admin"))
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("admin123");

        context.Auth.Add(new Auth 
        { 
            Username = "admin", 
            Password = hashedPassword, 
            Email = "nhoanghai2003@gmail.com", 
            Phone = "123456789",
            Role = "admin"
        });

        context.SaveChanges();

        Console.WriteLine("Default admin account created.");
    }

    SeedData.Initialize(services);
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

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
