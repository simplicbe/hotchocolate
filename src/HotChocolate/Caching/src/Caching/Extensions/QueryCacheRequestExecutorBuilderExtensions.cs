using System;
using HotChocolate;
using HotChocolate.Caching;
using HotChocolate.Execution.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class QueryCacheRequestExecutorBuilderExtensions
{
    /// <summary>
    /// Registers the <see cref="QueryCacheMiddleware"/>.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="IRequestExecutorBuilder"/>.
    /// </param>
    public static IRequestExecutorBuilder UseQueryCache(
        this IRequestExecutorBuilder builder) =>
        builder.UseRequest<QueryCacheMiddleware>();

    /// <summary>
    /// Uses the default request pipeline including the
    /// <see cref="QueryCacheMiddleware"/>.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="IRequestExecutorBuilder"/>.
    /// </param>
    public static IRequestExecutorBuilder UseQueryCachePipeline(
        this IRequestExecutorBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        return builder
            .UseInstrumentation()
            .UseExceptions()
            .UseTimeout()
            .UseDocumentCache()
            .UseDocumentParser()
            .UseDocumentValidation()
            .UseQueryCache()
            .UseOperationCache()
            .UseOperationComplexityAnalyzer()
            .UseOperationResolver()
            .UseOperationVariableCoercion()
            .UseOperationExecution();
    }

    /// <summary>
    /// Add CacheControl types and 
    /// </summary>
    /// <param name="builder">
    /// The <see cref="IRequestExecutorBuilder"/>.
    /// </param>
    public static IRequestExecutorBuilder AddCacheControl(
        this IRequestExecutorBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.ConfigureSchemaServices(services =>
        {
            services.AddOptions();
            services.AddSingleton<ICacheControlOptionsAccessor,
                CacheControlOptionsAccessor>();
        });

        return builder.ConfigureSchema(b =>
        {
            b.AddCacheControl();
            b.TryAddTypeInterceptor<CacheControlTypeInterceptor>();
        });
    }

    /// <summary>
    /// Modify the <see cref="CacheControlOptions"/>.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="IRequestExecutorBuilder"/>.
    /// </param>
    /// <param name="modifyOptions">
    /// Configure the <see cref="CacheControlOptions"/>.
    /// </param>
    public static IRequestExecutorBuilder ModifyCacheControlOptions(
        this IRequestExecutorBuilder builder,
        Action<CacheControlOptions> modifyOptions)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (modifyOptions is null)
        {
            throw new ArgumentNullException(nameof(modifyOptions));
        }

        builder.ConfigureSchemaServices(services =>
        {
            services.Configure(modifyOptions);
        });

        return builder;
    }

    internal static IRequestExecutorBuilder AddQueryCache<TCache>(
        this IRequestExecutorBuilder builder)
        where TCache : QueryCache
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.ConfigureSchemaServices(services =>
        {
            services.AddSingleton<QueryCache, TCache>();
        });

        return builder.AddCacheControl();
    }

    internal static IRequestExecutorBuilder AddQueryCache<TCache>(
        this IRequestExecutorBuilder builder,
        Func<IServiceProvider, TCache> cacheFactory)
        where TCache : QueryCache
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.ConfigureSchemaServices(services =>
        {
            services.AddSingleton<QueryCache>(cacheFactory);
        });

        return builder.AddCacheControl();
    }
}
