using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MPM_Betting.Blazor;
using MPM_Betting.Blazor.Components;
using MPM_Betting.DataModel;
using MPM_Betting.DataModel.User;
using MPM_Betting.Services.Account;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddRedisOutputCache("redis");

builder.Services.AddDbContext<MpmDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MPM-Betting")));

builder.Services.AddIdentityCore<MpmUser>(options =>
    {
        // TODO Olaf: Identity options go here
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddEntityFrameworkStores<MpmDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<MpmUser>, IdentityNoOpEmailSender>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<WeatherApiClient>(client => client.BaseAddress = new("http://api"));

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseOutputCache();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();