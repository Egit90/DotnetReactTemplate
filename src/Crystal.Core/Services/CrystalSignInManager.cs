using Crystal.Core.Abstractions;
using Crystal.Core.Models;
using Crystal.Core.Options;
﻿using System.Security.Claims;
using Crystal.Core.AuthSchemes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crystal.Core.Services;

public class CrystalSignInManager<TUser> : SignInManager<TUser> where TUser : class, ICrystalUser
{
    public bool UseCookie { get; set; }
    
    public CrystalSignInManager(
        UserManager<TUser> userManager, 
        IHttpContextAccessor contextAccessor, 
        IUserClaimsPrincipalFactory<TUser> claimsFactory, 
        IOptions<IdentityOptions> optionsAccessor, 
        ILogger<SignInManager<TUser>> logger, 
        IAuthenticationSchemeProvider schemes, 
        IUserConfirmation<TUser> confirmation) 
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
       AuthenticationScheme = CrystalAuthSchemeDefaults.BearerSignInScheme;
    }

    public override Task SignInWithClaimsAsync(
        TUser user, AuthenticationProperties? authenticationProperties, IEnumerable<Claim> additionalClaims)
    {
        var properties = authenticationProperties ?? new AuthenticationProperties();
        if (UseCookie)
        {
            properties.SetParameter("useCookie", true);
        }
        
        return base.SignInWithClaimsAsync(user, properties, additionalClaims);
    }
}