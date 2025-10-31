using FluentEmail.Core;
using FluentEmail.Core.Interfaces;
using FluentEmail.Liquid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Crystal.FluentEmail;

public class CrystalFluentEmailFactory(IServiceProvider serviceProvider, ILogger<CrystalFluentEmailFactory> logger)
{
    private static readonly Lazy<LiquidRenderer> LazyRenderer =
        new(() => new LiquidRenderer(new OptionsWrapper<LiquidRendererOptions>(new LiquidRendererOptions())));
    
    public IFluentEmail? CreateEmail()
    {
        var sender = serviceProvider.GetService<ISender>();
        if (sender is null)
        {
            logger.LogError("ISender not registered. Cannot create email client");
            return null;
        }

        return new Email(LazyRenderer.Value, sender);
    }
}