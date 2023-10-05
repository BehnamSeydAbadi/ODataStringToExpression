using System;
using System.Linq;

namespace ODataStringToExpression
{
    internal static class Extensions
    {
        internal static bool IsBinaryExpression(this string @operator)
        {
            switch (@operator)
            {
                case "gt":
                case "eq":
                case "lt":
                case "ge":
                case "le":
                case "ne":
                    return true;
                default: return false;
            }
        }

        internal static bool IsMethodCallExpression(this string @operator) => @operator == "in";

        internal static string[] Split(this string value, string separator)
        {
            return value.Split(new[] { separator }, StringSplitOptions.None)
                   .Select(s => s.Trim()).Where(s => string.IsNullOrWhiteSpace(s) is false)
                   .ToArray();
        }
    }
}
