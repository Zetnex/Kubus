namespace Kubus.parsing;

public class NamespaceNode : AstNode
{
    public string QualifiedName { get; }

    public NamespaceNode(string qualifiedName) : base("Namespace")
    {
        QualifiedName = qualifiedName;
    }
}