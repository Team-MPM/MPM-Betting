using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.Services.Account;

public sealed class IdentityNoOpEmailSender : IEmailSender<MpmUser>
{
    private readonly IEmailSender m_EmailSender = new NoOpEmailSender();

    public Task SendConfirmationLinkAsync(MpmUser user, string email, string confirmationLink) =>
        m_EmailSender.SendEmailAsync(email, "Confirm your email", $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");
 
    public Task SendPasswordResetLinkAsync(MpmUser user, string email, string resetLink) =>
        m_EmailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");
 
    public Task SendPasswordResetCodeAsync(MpmUser user, string email, string resetCode) =>
        m_EmailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password using the following code: {resetCode}");
}
