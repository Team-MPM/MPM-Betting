using MPM_Betting.Blazor;
using MPM_Betting.Blazor.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddMpmDbContext();

builder.AddRedisOutputCache("redis");

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<WeatherApiClient>(client => client.BaseAddress = new("http://api"));

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapAuthEndpoints();

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