using System.Text.RegularExpressions;

namespace Kubus.lexicalization;

public class TokenDefinition
{
    public bool IsIgnored { get; }
    public Regex Regex { get; }
    public string Type { get; }
    public TypeGroup TypeGroup { get; set; }

    public TokenDefinition(string regex, string type, TypeGroup typeGroup)
        : this(new Regex(regex), type, false, typeGroup)
    {
    }


    public TokenDefinition(Regex regex, string type, TypeGroup typeGroup)
        : this(regex, type, false, typeGroup)
    {
    }

    public TokenDefinition(string regex, string type, bool isIgnored, TypeGroup typeGroup)
        : this(new Regex(regex), type, isIgnored, typeGroup)
    {
    }
    
    public TokenDefinition(Regex regex, string type, bool isIgnored, TypeGroup typeGroup)
    {
        TypeGroup = typeGroup;
        Regex = regex ?? throw new ArgumentNullException("regex", "The regex for a TokenDefinition can't be null.");
        Type = type ?? throw new ArgumentNullException("type", "The type for a token can't be null.");
        IsIgnored = isIgnored;
    }
}