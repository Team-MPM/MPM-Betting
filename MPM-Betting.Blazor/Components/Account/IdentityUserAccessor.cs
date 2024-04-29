using Microsoft.AspNetCore.Identity;
using MPM_Betting.DataModel.User;

namespace BlazorApp1.Components.Account;

internal sealed class IdentityUserAccessor(
    UserManager<MpmUser> userManager,
    IdentityRedirectManager redirectManager)
{
    public async Task<MpmUser> GetRequiredUserAsync(HttpContext context)
    {
        var user = await userManager.GetUserAsync(context.User);

        if (user is null)
        {
            redirectManager.RedirectToWithStatus("Account/InvalidUser",
                $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);
        }

        return user;
    }
}