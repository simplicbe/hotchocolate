using System.Collections.Generic;
using System.Linq;
using HotChocolate.Configuration;
using HotChocolate.Language;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using HotChocolate.Types.Descriptors.Definitions;

namespace HotChocolate.Caching;

internal sealed class CacheControlTypeInterceptor : TypeInterceptor
{
    private readonly ICacheControlOptions _cacheControlOptions;

    public CacheControlTypeInterceptor(ICacheControlOptionsAccessor accessor)
    {
        _cacheControlOptions = accessor.CacheControl;
    }

    public override void OnBeforeCompleteType(ITypeCompletionContext completionContext,
        DefinitionBase? definition, IDictionary<string, object?> contextData)
    {
        if (!_cacheControlOptions.Enable || !_cacheControlOptions.ApplyDefaults)
        {
            return;
        }

        if (completionContext.IsIntrospectionType ||
            completionContext.IsSubscriptionType == true ||
            completionContext.IsMutationType == true)
        {
            return;
        }

        if (definition is not ObjectTypeDefinition objectDef)
        {
            return;
        }

        foreach (ObjectFieldDefinition field in objectDef.Fields)
        {
            if (field.IsIntrospectionField)
            {
                // Introspection fields do not need to be declared as cachable.
                continue;
            }

            if (HasCacheControlDirective(field))
            {
                // If the field has a @cacheControl directive,
                // we do not need to apply any defaults.
                continue;
            }

            if (completionContext.IsQueryType == true ||
                CostTypeInterceptor.IsDataResolver(field))
            {
                // Each field on the query type or data resolver fields
                // are treated as fields that need to be explicitly cached.
                ApplyCacheControlWithDefaultMaxAge(field);
            }
        }
    }

    private void ApplyCacheControlWithDefaultMaxAge(OutputFieldDefinitionBase field)
    {
        field.Directives.Add(
            new DirectiveDefinition(
                new DirectiveNode(
                    CacheControlDirectiveType.DirectiveName,
                    new ArgumentNode("maxAge", _cacheControlOptions.DefaultMaxAge))));
    }

    private static bool HasCacheControlDirective(ObjectFieldDefinition field)
    {
        return field.Directives.Any(IsCacheControlDirective);
    }

    private static bool IsCacheControlDirective(DirectiveDefinition directive)
    {
        if (directive.Reference is NameDirectiveReference directiveReference &&
            directiveReference.Name.Value == CacheControlDirectiveType.DirectiveName)
        {
            return true;
        }

        if (directive.Reference is ClrTypeDirectiveReference { ClrType: { } type } &&
            type == typeof(CacheControlDirective))
        {
            return true;
        }

        return false;
    }
}