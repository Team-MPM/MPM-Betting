using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using MPM_Betting.DataModel.User;

namespace MPM_Betting.Services.Account;

public sealed class IdentityEmailSender(SmtpClient smtpClient) : IEmailSender<MpmUser>
{
    public Task SendConfirmationLinkAsync(MpmUser user, string email, string confirmationLink) =>
        smtpClient.SendMailAsync(new MailMessage("no-reply@mpm-betting.at", email)
            {
                Subject = "Confirm your email",
                Body = $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.", 
                IsBodyHtml = true
            });
 
    public Task SendPasswordResetLinkAsync(MpmUser user, string email, string resetLink) =>
        smtpClient.SendMailAsync(new MailMessage("no-reply@mpm-betting.at", email)
        {
            Subject = "Reset your password",
            Body = $"Please reset your password by <a href='{resetLink}'>clicking here</a>.", 
            IsBodyHtml = true
        });
 
    public Task SendPasswordResetCodeAsync(MpmUser user, string email, string resetCode) =>
        smtpClient.SendMailAsync(new MailMessage("no-reply@mpm-betting.at", email)
        {
            Subject = "Reset your password",
            Body = $"Please reset your password using the following code: {resetCode}", 
            IsBodyHtml = true
        });
}
