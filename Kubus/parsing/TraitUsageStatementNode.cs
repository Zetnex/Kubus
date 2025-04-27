namespace Kubus.parsing;

public class TraitUsageStatementNode : StatementNode
{
    public List<string> Traits { get; }

    public TraitUsageStatementNode(List<string> traits)
        : base("TraitUsage")
    {
        Traits = traits;
    }
}