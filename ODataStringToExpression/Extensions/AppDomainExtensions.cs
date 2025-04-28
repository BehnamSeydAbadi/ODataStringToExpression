using System;
using System.Linq;

namespace ODataStringToExpression.Extensions;

public static class AppDomainExtensions
{
    public static Type GetType(this AppDomain appDomain, string typeFullName)
    {
        return appDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())
            .FirstOrDefault(type => type.FullName == typeFullName);
    }
}