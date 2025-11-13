using Crystal.Core.Abstractions;
using Crystal.Core.Services;
using Crystal.Core.Services.EmailSender;
using FluentEmail.Core.Defaults;
using FluentEmail.Core.Interfaces;
using FluentEmail.MailKitSmtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Crystal.FluentEmail;

public static class CrystalServiceBuilderExtensions
{
    public static CrystalServiceBuilder<TUser, TKey> AddFluentEmail<TUser, TKey>(
        this CrystalServiceBuilder<TUser, TKey> builder,
        bool registerMailKitSender = true)
        where TKey : IEquatable<TKey>
        where TUser : IdentityUser<TKey>
        , ICrystalUser<TKey>, new()
    {
        if (builder.Configuration.GetSection(FluentEmailOptions.SectionName).Exists())
        {
            builder.Services.Configure<FluentEmailOptions>(builder.Configuration.GetSection(FluentEmailOptions.SectionName));
        }
        else
        {
            builder.Services.Configure<FluentEmailOptions>(_ => { });
        }

        var saveEmailsOnDisk = builder.Configuration.GetValue<string?>($"{FluentEmailOptions.SectionName}:SaveEmailsOnDisk");
        if (saveEmailsOnDisk is not null)
        {
            Directory.CreateDirectory(saveEmailsOnDisk);
            builder.Services.TryAdd(ServiceDescriptor.Scoped<ISender>(_ => new SaveToDiskSender(saveEmailsOnDisk)));
        }
        else if (registerMailKitSender)
        {
            builder.Services.TryAdd(ServiceDescriptor.Scoped<ISender>(
                sp =>
                {
                    var opts = sp.GetRequiredService<IOptions<FluentEmailOptions>>();
                    var smtpClientOptions = new SmtpClientOptions
                    {
                        Server = opts.Value.SmtpHost,
                        Port = opts.Value.SmtpPort,
                        User = opts.Value.SmtpUsername,
                        Password = opts.Value.SmtpPassword,
                    };
                    return new MailKitSender(smtpClientOptions);
                }));
        }

        builder.Services.AddScoped<CrystalFluentEmailFactory>();
        builder.Services.AddTransient<ICrystalEmailConfirmationEmailSender<TUser, TKey>, EmailConfirmationEmailSender<TUser, TKey>>();
        builder.Services.AddTransient<ICrystalPasswordResetEmailSender<TUser, TKey>, PasswordResetEmailSender<TUser, TKey>>();

        return builder;
    }
}