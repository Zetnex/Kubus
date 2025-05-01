namespace Kubus.parsing;

public class StaticMethodCallNode : ExpressionNode
{
    public string ClassName { get; }
    public string MethodName { get; }
    public List<ExpressionNode> Arguments { get; }

    public StaticMethodCallNode(string className, string methodName, List<ExpressionNode> arguments) : base("StaticMethodCall")
    {
        ClassName = className;
        MethodName = methodName;
        Arguments = arguments;
    }
}