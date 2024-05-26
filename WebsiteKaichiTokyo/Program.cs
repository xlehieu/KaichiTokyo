using AspNetCoreHero.ToastNotification;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using System.Configuration;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using WebsiteKaichiTokyo.Models;
using Microsoft.EntityFrameworkCore;
using WebsiteKaichiTokyo.Repository;
using Microsoft.Extensions.DependencyInjection;
using WebsiteKaichiTokyo.EmailSender;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
var configuration = builder.Configuration;
builder.Services.AddDbContext<CuaHangNhatBanContext>();
builder.Services.AddSingleton<HtmlEncoder>(HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.All}));
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(p => 
//    {
//        p.LoginPath= "/Dang-nhap";
//        p.AccessDeniedPath = "/";        
//    });
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
    .AddCookie(
    p =>
    {
        p.LoginPath = "/Dang-nhap";
        p.AccessDeniedPath = "/";
    }
    )
    .AddGoogle(GoogleDefaults.AuthenticationScheme, googleOption =>
    {
        googleOption.ClientId = configuration.GetSection("GoogleKeys:ClientId").Value;
        googleOption.ClientSecret = configuration.GetSection("GoogleKeys:ClientSecret").Value;

    });
builder.Services.AddNotyf(config => { config.DurationInSeconds = 10;config.IsDismissable = true;config.Position = NotyfPosition.BottomRight; });
builder.Services.AddScoped<CategoryRepositoryInterface,CategoryRepository>();
builder.Services.AddTransient<IEmailSendercs, EmailSender>();
builder.Services.AddApplicationInsightsTelemetry();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    );
});
app.Run();
