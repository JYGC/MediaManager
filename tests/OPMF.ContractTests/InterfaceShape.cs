using System;
using System.Collections.Generic;
using System.Linq;

namespace OPMF.ContractTests
{
    internal static class InterfaceShape
    {
        public static List<string> MethodSignaturesOf(Type interfaceType)
        {
            return interfaceType.GetMethods()
                .Select(method => $"{NameOfType(method.ReturnType)} {method.Name}({string.Join(", ", method.GetParameters().Select(parameter => NameOfType(parameter.ParameterType)))})")
                .OrderBy(signature => signature, StringComparer.Ordinal)
                .ToList();
        }

        public static List<string> PropertyDeclarationsOf(Type interfaceType)
        {
            return interfaceType.GetProperties()
                .Select(property => $"{NameOfType(property.PropertyType)} {property.Name} {{ {(property.CanRead ? "get; " : "")}{(property.CanWrite ? "set; " : "")}}}")
                .OrderBy(declaration => declaration, StringComparer.Ordinal)
                .ToList();
        }

        private static string NameOfType(Type type)
        {
            if (!type.IsGenericType)
            {
                return type.Name;
            }
            var nameWithoutArity = type.Name.Substring(0, type.Name.IndexOf('`'));
            return nameWithoutArity + "<" + string.Join(", ", type.GetGenericArguments().Select(NameOfType)) + ">";
        }
    }
}
