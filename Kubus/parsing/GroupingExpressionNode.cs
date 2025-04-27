namespace Kubus.parsing;

public class GroupingExpressionNode : ExpressionNode
{
    public ExpressionNode Expression { get; }

    public GroupingExpressionNode(ExpressionNode expression) : base("GroupingExpression")
    {
        Expression = expression;
    }
}