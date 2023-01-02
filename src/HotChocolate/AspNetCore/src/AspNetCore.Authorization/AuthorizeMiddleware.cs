using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotChocolate.AspNetCore.Authorization.Properties;
using HotChocolate.Resolvers;

namespace HotChocolate.AspNetCore.Authorization;

internal sealed class AuthorizeMiddleware
{
    private readonly FieldDelegate _next;
    private readonly IAuthorizationHandler _handler;
    private readonly AuthorizeDirective _directive;

    public AuthorizeMiddleware(
        FieldDelegate next,
        IAuthorizationHandler authorizationHandler,
        AuthorizeDirective directive)
    {
        _next = next ??
            throw new ArgumentNullException(nameof(next));
        _handler = authorizationHandler ??
            throw new ArgumentNullException(nameof(authorizationHandler));
        _directive = directive ??
            throw new ArgumentNullException(nameof(directive));
    }

    public async Task InvokeAsync(IMiddlewareContext context)
    {
        if (_directive.Apply == ApplyPolicy.AfterResolver)
        {
            await _next(context).ConfigureAwait(false);

            var state = await _handler.AuthorizeAsync(context, _directive).ConfigureAwait(false);

            if (state != AuthorizeResult.Allowed && !IsErrorResult(context))
            {
                SetError(context, state);
            }
        }
        else
        {
            var state = await _handler.AuthorizeAsync(context, _directive).ConfigureAwait(false);

            if (state == AuthorizeResult.Allowed)
            {
                await _next(context).ConfigureAwait(false);
            }
            else
            {
                SetError(context, state);
            }
        }
    }

    private bool IsErrorResult(IMiddlewareContext context)
        => context.Result is IError or IEnumerable<IError>;

    private void SetError(
        IMiddlewareContext context,
        AuthorizeResult state)
        => context.Result = state switch
        {
            AuthorizeResult.NoDefaultPolicy
                => ErrorBuilder.New()
                    .SetMessage(AuthResources.AuthorizeMiddleware_NoDefaultPolicy)
                    .SetCode(ErrorCodes.Authentication.NoDefaultPolicy)
                    .SetPath(context.Path)
                    .AddLocation(context.Selection.SyntaxNode)
                    .Build(),
            AuthorizeResult.PolicyNotFound
                => ErrorBuilder.New()
                    .SetMessage(
                        AuthResources.AuthorizeMiddleware_PolicyNotFound,
                        _directive.Policy!)
                    .SetCode(ErrorCodes.Authentication.PolicyNotFound)
                    .SetPath(context.Path)
                    .AddLocation(context.Selection.SyntaxNode)
                    .Build(),
            _
                => ErrorBuilder.New()
                    .SetMessage(AuthResources.AuthorizeMiddleware_NotAuthorized)
                    .SetCode(state == AuthorizeResult.NotAllowed
                        ? ErrorCodes.Authentication.NotAuthorized
                        : ErrorCodes.Authentication.NotAuthenticated)
                    .SetPath(context.Path)
                    .AddLocation(context.Selection.SyntaxNode)
                    .Build()
        };
}
