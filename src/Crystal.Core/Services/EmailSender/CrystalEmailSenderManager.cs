using Crystal.Core.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crystal.Core.Services.EmailSender;

public class CrystalEmailSenderManager<TUser>(
    ICrystalEmailConfirmationEmailSender<TUser>? _confirmationSender,
    ICrystalPasswordResetEmailSender<TUser>? _passwordResetSender,
    IOptions<IdentityOptions> _identityOptions,
    ILogger<CrystalEmailSenderManager<TUser>> logger)
    : ICrystalEmailSenderManager<TUser>
    where TUser : IdentityUser, ICrystalUser
{
    public Task SendEmailConfirmationAsync(TUser user, string confirmationLink)
    {
        if (!_identityOptions.Value.SignIn.RequireConfirmedEmail)
        {
            logger.LogDebug("Email confirmation not required. Skipping email confirmation for {Email}", user.Email);
            return Task.CompletedTask;
        }

        if (user.EmailConfirmed)
        {
            logger.LogInformation("Email already confirmed for {Email}", user.Email);
            return Task.CompletedTask;
        }

        if (_confirmationSender is null)
        {
            logger.LogCritical("Email client is not configured. Cannot send email confirmation for {Email}",
                user.Email);
            return Task.CompletedTask;
        }

        return _confirmationSender.SendAsync(user, confirmationLink);
    }

    public Task SendPasswordResetAsync(TUser user, string resetLink)
    {
        if (_passwordResetSender is null)
        {
            logger.LogCritical("Email client is not configured. Cannot send password reset for {Email}",
                user.Email);
            return Task.CompletedTask;
        }

        return _passwordResetSender.SendAsync(user, resetLink);
    }
}

public interface ICrystalEmailSenderManager<T> where T : ICrystalUser
{
    Task SendEmailConfirmationAsync(T user, string confirmationLink);
    Task SendPasswordResetAsync(T user, string resetLink);
}