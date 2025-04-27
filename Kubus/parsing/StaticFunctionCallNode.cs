namespace Kubus.parsing;

public class StaticFunctionCallNode : ExpressionNode
{
    public string FunctionName { get; }
    public List<ExpressionNode> Arguments { get; }

    public StaticFunctionCallNode(string functionName, List<ExpressionNode> arguments)
        : base("StaticFunctionCall")
    {
        FunctionName = functionName;
        Arguments = arguments;
    }
}