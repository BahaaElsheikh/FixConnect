using FixConnect.BLL.Services;
using FixConnect.DAL.Context;
using FixConnect.DAL.Data.Seed;
using FixConnect.DAL.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ DI: Register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ DI: Register Repositories & Services
builder.Services.AddScoped(typeof(GenericRepository<>));
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<AuthService>();

// ✅ Fix Correlation failed
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.None;
    options.Secure = CookieSecurePolicy.Always;
});

// ✅ DI: Worker Phase Services
builder.Services.AddScoped<WorkerService>();
builder.Services.AddScoped<PortfolioService>();

// ✅ Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    // ✅ Fix SameSite for Google OAuth callback
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    options.SaveTokens = true;
    // ✅ Fix Correlation — store state in cookie not session
    options.CorrelationCookie.SameSite = SameSiteMode.None;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();



if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();