using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WhenTheFireFades.Data;
using WhenTheFireFades.Data.Repositories;
using WhenTheFireFades.Domain.Helpers;
using WhenTheFireFades.Domain.Services;
using WhenTheFireFades.Hubs;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(6);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<SessionHelper>();

builder.Services.AddScoped<GameService>();

builder.Services.AddScoped<IGameRepository, GameRepository>();

builder.Services.AddScoped<IGamePlayerRepository, GamePlayerRepository>();

builder.Services.AddScoped<IRoundRepository, RoundRepository>();



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

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<GameLobbyHub>("/gameLobbyHub");
app.MapHub<GamePlayHub>("/gamePlayHub");

app.Run();
