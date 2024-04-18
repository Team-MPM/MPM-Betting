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
app.UseRouting();




// Auth
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapAdditionalIdentityEndpoints();

app.MapDefaultEndpoints();
app.UseStaticFiles();

// Hosting
app.UseOutputCache();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();