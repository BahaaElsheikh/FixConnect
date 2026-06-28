using FixConnect.BLL.Services;
using FixConnect.BLL.Settings;
using FixConnect.DAL.Context;
using FixConnect.DAL.Data.Seed;
using FixConnect.DAL.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ DI: Register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

// ✅ DI: Register Repositories & Services
builder.Services.AddScoped(typeof(GenericRepository<>));
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<AuthService>();
// أضف بعد WorkerService
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<RequestService>();
builder.Services.AddScoped<ProposalService>();

builder.Services.AddScoped<JobService>();
builder.Services.AddScoped<WalletService>();
builder.Services.AddScoped<ReviewService>();

builder.Services.AddScoped<NotificationBadgeService>();
builder.Services.AddScoped<CustomerNotificationBadgeService>();


builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<EmailSender>();



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


builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins("https://localhost:7163")  // ← عنوان الـ React
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();                    // ← مهم عشان الـ Cookies
    });
});


builder.Services.ConfigureApplicationCookie(options =>
{
    // لما الـ API مش مـ Authenticated ترجع 401 مش Redirect للـ Login
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    // لما الـ API مش عندها Permission ترجع 403 مش Redirect
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});






var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
    DbSeeder.Seed(context);
}




if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCookiePolicy();

app.UseCors("ReactPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
//pattern: "{controller=Admin}/{action=Users}/{id?}");

app.Run();


// Hello Mo3taz !!!