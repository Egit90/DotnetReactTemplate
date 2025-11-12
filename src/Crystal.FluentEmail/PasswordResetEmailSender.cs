using System.Reflection;
using Crystal.Core.Abstractions;
using Crystal.Core.Services.EmailSender;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crystal.FluentEmail;

public class PasswordResetEmailSender<TUser, TKey>(
    IOptions<FluentEmailOptions> fluentEmailOptions,
    CrystalFluentEmailFactory emailFactory,
    ILogger<EmailConfirmationEmailSender<TUser, TKey>> logger) : ICrystalPasswordResetEmailSender<TUser, TKey>
    where TKey : IEquatable<TKey>
    where TUser : IdentityUser<TKey>
    , ICrystalUser<TKey>
{
    protected virtual object PrepareForgotPasswordModel(TUser user, string confirmationLink)
    {
        return new
        {
            ResetPasswordLink = confirmationLink,
            user.Email,
            user.UserName,
        };
    }

    public virtual async Task SendAsync(TUser user, string link)
    {
        var email = emailFactory.CreateEmail();
        if (email is null)
        {
            return;
        }

        var emailSettings = fluentEmailOptions.Value.Emails.GetValueOrDefault(EmailType.PasswordReset);

        email
            .SetFrom(fluentEmailOptions.Value.FromEmail, fluentEmailOptions.Value.FromName)
            .To(user.Email)
            .Subject(string.IsNullOrWhiteSpace(emailSettings?.Subject) ? "Password recovery" : emailSettings.Subject);

        if (string.IsNullOrWhiteSpace(emailSettings?.TemplatePath))
        {
            email.UsingTemplateFromEmbedded(
                "Crystal.FluentEmail.Templates.PasswordReset.template.html",
                PrepareForgotPasswordModel(user, link),
                Assembly.GetAssembly(GetType()));
        }
        else
        {
            email.UsingTemplateFromFile(
                emailSettings.TemplatePath,
                PrepareForgotPasswordModel(user, link));
        }

        var res = await email.SendAsync();
        if (res.Successful)
        {
            logger.LogInformation("Password recovery sent to {Email}", user.Email);
        }
        else
        {
            logger.LogError("Password recovery email failed to send to {Email}. Errors: {Errors}", user.Email,
                res.ErrorMessages);
        }
    }
}