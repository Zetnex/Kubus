namespace Kubus.parsing;

public class MethodCallChainNode : ExpressionNode
{
    public ExpressionNode Receiver { get; } // e.g., $this, $logger, or a class name for static calls
    public List<(string MethodName, List<ExpressionNode> Arguments)> Calls { get; } // List of method calls in the chain

    public MethodCallChainNode(ExpressionNode receiver, List<(string, List<ExpressionNode>)> calls) : base("MethodCallChain")
    {
        Receiver = receiver;
        Calls = calls;
    }
}