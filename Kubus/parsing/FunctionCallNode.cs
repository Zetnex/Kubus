namespace Kubus.parsing;

public class FunctionCallNode : ExpressionNode
{
    public string FunctionName { get; }
    public List<ExpressionNode> Arguments { get; }

    public FunctionCallNode(string functionName, List<ExpressionNode> arguments) : base("FunctionCall")
    {
        FunctionName = functionName;
        Arguments = arguments;
    }
}