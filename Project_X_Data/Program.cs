using Microsoft.EntityFrameworkCore;
using Project_X_Data.Data;
using Project_X_Data.Middleware.Auth;
using Project_X_Data.Services.Auth;
using Project_X_Data.Services.Kdf;
using Project_X_Data.Services.MailKit;
using Project_X_Data.Services.Storage;
using System;
var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IKdfService, PbKdf1Service>();

//builder.Services.AddDbContext<DataContext>(options => -- 20/04/2026
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("DefaultConnection"),
//        sqlOptions =>
//        {
//            sqlOptions.EnableRetryOnFailure(
//                maxRetryCount: 5,
//                maxRetryDelay: TimeSpan.FromSeconds(10),
//                errorNumbersToAdd: null
//            );
//        }));
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, SessionAuthService>();
builder.Services.AddSingleton<IStorageService, DiskStorageService>();
builder.Services.AddScoped<DataAccessor>();

builder.Services.AddSingleton<IEmailSender, Project_X_Data.Services.MailKit.EmailSender>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader();
    });
});

var app = builder.Build();

// 20/04/2026
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DataContext>();
    context.Database.EnsureCreated();
}

app.UseStaticFiles(); 

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();

app.UseAuthorization();
app.UseSession();
app.UseSessionAuth();
app.UseJwtAuth();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=index}/{id?}");

app.Run();

