using Crystal.Core.Abstractions;
using Crystal.Core.Services.EmailSender;
﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Crystal.Core.Endpoints;

public interface IAuthEndpoint : IEndpoint
{
}

public interface IAccountEndpoint : IEndpoint
{
}

public interface IEndpoint
{
    RouteHandlerBuilder Map(IEndpointRouteBuilder builder);
}