using Kubus.lexicalization;

namespace Kubus.parsing;

public class PropertyNode : AstNode
{
    public string Visibility { get; }
    public bool IsStatic { get; }
    public string Name { get; }
    public Token Type { get; set; }
    public ExpressionNode? InitialValue { get; }

    public PropertyNode(string visibility, Token type, bool isStatic, string name, ExpressionNode? initialValue)
        : base("Property")
    {
        Visibility = visibility;
        IsStatic = isStatic;
        Type = type;
        Name = name;
        InitialValue = initialValue;
    }
}