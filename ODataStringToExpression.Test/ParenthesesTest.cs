namespace ODataStringToExpression.Test;

public class ParenthesesTest
{
    [Theory]
    [InlineData("x + (2 * y)", new[] { "2 * y" })]
    [InlineData("x + (2 * y) - (3 / o)", new[] { "2 * y", "3 / o" })]
    [InlineData("x + (x - (t % 6))", new[] { "x - (t % 6)" })]
    [InlineData("x + (x - (t % 6)) / (y - 9)", new[] { "x - (t % 6)", "y - 9" })]
    public void Get_between_parentheses(string expression, string[] values)
    {
        var odataToExpression = new ODataToExpression<object>();

        var output = odataToExpression.GetBetweenParentheses(expression);

        foreach (var value in values)
            output.Should().Contain(value);
    }
}
