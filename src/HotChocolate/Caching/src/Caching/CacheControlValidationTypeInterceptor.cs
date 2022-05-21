using System.Collections.Generic;
using System.Linq;
using HotChocolate.Configuration;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors.Definitions;

namespace HotChocolate.Caching;

internal sealed class CacheControlValidationTypeInterceptor : TypeInterceptor
{
    public override void OnValidateType(ITypeSystemObjectContext validationContext,
        DefinitionBase? definition, IDictionary<string, object?> contextData)
    {
        if (validationContext.IsIntrospectionType)
        {
            return;
        }

        if (validationContext.Type is ObjectType objectType)
        {
            var isQueryType = validationContext is ITypeCompletionContext completionContext &&
                completionContext.IsQueryType == true;

            ValidateCacheControlOnType(validationContext, objectType);

            foreach (ObjectField field in objectType.Fields)
            {
                ValidateCacheControlOnField(validationContext, field, objectType,
                    isQueryType);
            }
        }
        else if (validationContext.Type is InterfaceType interfaceType)
        {
            ValidateCacheControlOnType(validationContext, interfaceType);

            foreach (InterfaceField field in interfaceType.Fields)
            {
                ValidateCacheControlOnField(validationContext, field, interfaceType,
                    false);
            }
        }
        else if (validationContext.Type is UnionType unionType)
        {
            ValidateCacheControlOnType(validationContext, unionType);
        }
    }

    private static void ValidateCacheControlOnType(
        ITypeSystemObjectContext validationContext,
        IHasDirectives type)
    {
        CacheControlDirective? directive = type.Directives
                    .FirstOrDefault(d => d.Name == CacheControlDirectiveType.DirectiveName)
                    ?.ToObject<CacheControlDirective>();

        if (directive is null)
        {
            return;
        }

        if (directive.InheritMaxAge == true
            && type is ITypeSystemObject typeSystemObject)
        {
            ISchemaError error = ErrorHelper.CacheControlInheritMaxAgeOnType(typeSystemObject);

            validationContext.ReportError(error);
        }
    }

    private static void ValidateCacheControlOnField(
        ITypeSystemObjectContext validationContext,
        IField field, ITypeSystemObject obj,
        bool isQueryTypeField)
    {
        CacheControlDirective? directive = field.Directives
                    .FirstOrDefault(d => d.Name == CacheControlDirectiveType.DirectiveName)
                    ?.ToObject<CacheControlDirective>();

        if (directive is null)
        {
            return;
        }

        if (field is InterfaceField interfaceField)
        {
            ISchemaError error = ErrorHelper
                    .CacheControlOnInterfaceField(obj, field);

            validationContext.ReportError(error);

            return;
        }

        var inheritMaxAge = directive.InheritMaxAge == true;

        if (isQueryTypeField && inheritMaxAge)
        {
            ISchemaError error = ErrorHelper
                    .CacheControlInheritMaxAgeOnQueryTypeField(obj, field);

            validationContext.ReportError(error);
        }

        if (directive.MaxAge.HasValue)
        {
            if (directive.MaxAge.Value < 0)
            {
                ISchemaError error = ErrorHelper
                    .CacheControlNegativeMaxAge(obj, field);

                validationContext.ReportError(error);
            }

            if (inheritMaxAge)
            {
                ISchemaError error = ErrorHelper
                    .CacheControlBothMaxAgeAndInheritMaxAge(obj, field);

                validationContext.ReportError(error);
            }
        }
    }
}
