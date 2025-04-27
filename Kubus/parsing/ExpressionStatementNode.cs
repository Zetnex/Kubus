namespace Kubus.parsing;

public class ExpressionStatementNode : StatementNode
{
    public ExpressionNode Expression { get; }

    public ExpressionStatementNode(ExpressionNode expression) 
        : base("ExpressionStatement")
    {
        Expression = expression;
    }
}