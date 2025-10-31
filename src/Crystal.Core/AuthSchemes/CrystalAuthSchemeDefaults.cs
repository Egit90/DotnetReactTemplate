using Crystal.Core.Abstractions;
using Crystal.Core.Models;
using Crystal.Core.Options;
﻿namespace Crystal.Core.AuthSchemes;

public static class CrystalAuthSchemeDefaults
{
    public const string BearerSignInScheme = "Crystal.BearerSignInCookieScheme";
    public const string BearerTokenScheme = "Crystal.BearerSignInTokenScheme";
    public const string RefreshTokenScheme = "Crystal.RefreshToken";
    public const string SignInExternalPolicyScheme = "Crystal.ExternalSignInDefaultScheme";
    public const string SignInExternalScheme = "Crystal.ExternalSignInScheme";
    public const string SignUpExternalScheme = "Crystal.ExternalSignUpScheme";
    
    public const string RefreshTokenCookieName = "Crystal.RefreshToken";
    public const string AccessTokenCookieName = "Crystal.AccessToken";
}