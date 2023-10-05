namespace ODataStringToExpression
{
    internal class RegularExpressions
    {
        internal const string ForInOperator = @"(\w+)\s+(in)\s+(\(.*?\))";
        internal const string ForNumericAtRight = @"(\w+)\s+(\w+)\s+(\d+)";
        internal const string ForDateTimeAtRight = @"(\w+)\s+(\w+)\s+([\d\-T:.Z]+)";
        internal const string ForBetweenParentheses = @"(?<=\()[^()]+(?=\))";
    }
}
