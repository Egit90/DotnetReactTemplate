using Crystal.Core.Abstractions;
namespace Crystal.Core.Services.EmailSender;

/// <summary>
/// Represents an email sender for Crystal
/// </summary>
/// <typeparam name="TUser"></typeparam>
public interface ICrystalEmailConfirmationEmailSender<in TUser, TKey>
        where TUser : ICrystalUser<TKey>
        where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Sends an email confirmation to the user
    /// </summary>
    /// <param name="user"></param>
    /// <param name="confirmationLink"></param>
    /// <returns></returns>
    Task SendAsync(TUser user, string confirmationLink);
}