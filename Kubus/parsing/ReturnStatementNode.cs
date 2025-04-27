namespace Kubus.parsing;

public class ReturnStatementNode : StatementNode
{
    public ExpressionNode ReturnValue { get; }

    public ReturnStatementNode(ExpressionNode returnValue) : base("ReturnStatement")
    {
        ReturnValue = returnValue;
    }
}