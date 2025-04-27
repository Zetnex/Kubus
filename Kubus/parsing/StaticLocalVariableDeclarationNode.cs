namespace Kubus.parsing;

public class StaticLocalVariableDeclarationNode : StatementNode
{
    public string Name { get; }
    public ExpressionNode Initializer { get; }

    public StaticLocalVariableDeclarationNode(string name, ExpressionNode initializer)
        : base("StaticLocalVariableDeclaration")
    {
        Name = name;
        Initializer = initializer;
    }
}