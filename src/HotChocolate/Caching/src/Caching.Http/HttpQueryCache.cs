using System.Threading.Tasks;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Http;
using static HotChocolate.WellKnownContextData;

namespace HotChocolate.Caching.Http;

/// <summary>
/// A <see cref="QueryCache"/> implementation that utilizes the
/// <c>Cache-Control header</c> to cache query results.
/// </summary>
public class HttpQueryCache : QueryCache
{
    private const string _httpContextKey = nameof(HttpContext);
    private const string _cacheControlValueTemplate = "{0}, max-age={1}";
    private const string _cacheControlPrivateScope = "private";
    private const string _cacheControlPublicScope = "public";

    /// <inheritdoc />
    public override ValueTask WriteQueryResultToCacheAsync(IRequestContext context,
        ICacheConstraints constraints, ICacheControlOptions options)
    {
        var cacheType = constraints.Scope switch
        {
            CacheControlScope.Private => _cacheControlPrivateScope,
            CacheControlScope.Public => _cacheControlPublicScope,
            _ => throw ThrowHelper.UnexpectedCacheControlScopeValue(constraints.Scope)
        };

        var headerValue = string.Format(_cacheControlValueTemplate,
            cacheType, constraints.MaxAge);

        var queryResult = context.Result?.ExpectQueryResult();

        if (queryResult is not null)
        {
            context.Result = QueryResultBuilder.FromResult(queryResult)
                .SetContextData(CacheControlHeaderValue, headerValue)
                .Create();
        }

        return ValueTask.CompletedTask;
    }
}
