using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MPM_Betting.DataModel;
using MPM_Betting.DataModel.User;
using MPM_Betting.Services.Account;

namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    public static IHostApplicationBuilder AddMpmDbContext(this IHostApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<MpmDbContext>("MPM-Betting");

        builder.Services.AddIdentityCore<MpmUser>(options =>
            {
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
        
        return builder;
    }
    
    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        //using var db = app.Services.GetRequiredService<MpmDbContext>();
        //db.Database.EnsureCreated();
        //db.Database.Migrate();
        
        return app;
    }
}