using Crystal.Core.Abstractions;
﻿namespace Crystal.Core.Services.EmailSender;

/// <summary>
/// Represents an email sender for Crystal
/// </summary>
/// <typeparam name="TUser"></typeparam>
public interface ICrystalPasswordResetEmailSender<TUser> where TUser: ICrystalUser
{
    /// <summary>
    /// Sends a password reset email to the user
    /// </summary>
    /// <param name="user"></param>
    /// <param name="resetLink"></param>
    /// <returns></returns>
    Task SendAsync(TUser user, string resetLink);
}