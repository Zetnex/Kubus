namespace Kubus.parsing;

public class MethodNode : AstNode
{
    public string Visibility { get; }
    public bool IsStatic { get; }
    public bool IsMethod { get; }
    public string Name { get; }
    public List<ParameterNode> Parameters { get; }
    public string? ReturnType { get; }
    public BlockNode Body { get; }

    public MethodNode(
        string visibility,
        bool isStatic,
        bool isMethod,
        string name,
        List<ParameterNode> parameters,
        string? returnType,
        BlockNode body
    ) : base("Method")
    {
        Visibility = visibility;
        IsStatic = isStatic;
        IsMethod = isMethod;
        Name = name;
        Parameters = parameters;
        ReturnType = returnType;
        Body = body;
    }
}