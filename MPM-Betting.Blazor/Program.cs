using BlazorApp1.Components.Account;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using MPM_Betting.Blazor;
using MPM_Betting.Blazor.Components;
using MPM_Betting.Services;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Blazor

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Auth

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// Application Services

builder.AddServiceDefaults();
builder.AddMpmDbContext();
builder.AddMpmCache();
builder.AddMpmAuth();
builder.AddMpmMail();
builder.AddFootballApi();

// Api clients

builder.Services.AddHttpClient<WeatherApiClient>(client => client.BaseAddress = new("http://api"));


// Blazorise

builder.Services
    .AddBlazorise( options =>
    {
        options.Immediate = true;
    } )
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();

// MudBlazor


builder.Services.AddMudServices();




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