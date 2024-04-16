using BlazorApp1.Components.Account;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using MPM_Betting.Blazor;
using MPM_Betting.Blazor.Components;

var builder = WebApplication.CreateBuilder(args);

// Default

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Auth

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies(); // Tell it to use cookies for auth

// Hosting

builder.AddServiceDefaults();
builder.AddMpmDbContext();

builder.AddRedisOutputCache("redis");


// Our own services

builder.Services.AddHttpClient<WeatherApiClient>(client => client.BaseAddress = new("http://api"));







var app = builder.Build();


// Default
app.MapDefaultEndpoints();
app.UseStaticFiles();
app.UseAntiforgery();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Auth
app.MapAuthEndpoints();
app.MapAdditionalIdentityEndpoints();

// Hosting
app.UseOutputCache();

app.Run();