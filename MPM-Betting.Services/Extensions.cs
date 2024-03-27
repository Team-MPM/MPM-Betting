using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MPM_Betting.DataModel;
using MPM_Betting.DataModel.User;
using MPM_Betting.Services.Account;

namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    public static IHostApplicationBuilder AddMpmDbContext(this IHostApplicationBuilder builder)
    {
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

        builder.Services.AddDataProtection()
            .SetApplicationName("Mpm-Betting");

        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();
        builder.Services.AddAntiforgery();
        
        return builder;
    }
    
    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();

        return app;
    }
}