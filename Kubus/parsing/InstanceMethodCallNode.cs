namespace Kubus.parsing;

public class InstanceMethodCallNode : ExpressionNode
{
    public ExpressionNode Receiver { get; }
    public string MethodName { get; }
    public List<ExpressionNode> Arguments { get; }

    public InstanceMethodCallNode(ExpressionNode receiver, string methodName, List<ExpressionNode> arguments) : base("InstanceMethodCall")
    {
        Receiver = receiver;
        MethodName = methodName;
        Arguments = arguments;
    }
}