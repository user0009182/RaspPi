using HttpServer.Areas.Identity;
using HttpServer.Auth;
using HttpServer.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var identity = builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = false);

//in-memory store for users
identity.AddUserStore<MemStore>();
    
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<User>>();
builder.Services.AddSingleton<DeviceClientService>();

var app = builder.Build();

//can be used to generate a new user's hash
//var hash = generatePasswordHash(app, "a@a.com", "abcdefg");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

string generatePasswordHash(WebApplication app, string userId, string password)
{
    using (var serviceScope = app.Services.CreateScope())
    {
        var services = serviceScope.ServiceProvider;
        var userStore = services.GetRequiredService<IUserStore<User>>();
        var user = userStore.FindByIdAsync(userId, new CancellationToken()).Result;
        var hasher = services.GetRequiredService<IPasswordHasher<User>>();
        return hasher.HashPassword(user, password);
    }
}