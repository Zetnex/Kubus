using System.Text.RegularExpressions;

namespace Kubus.lexicalization;

public class Lexer
{
    
    public const string EoF = "__0End__";
    public const string Whitespace = "Whitespace";
    
    private List<TokenDefinition> _definitions = new();
    private static Regex _whiteSpace = new Regex($@"((\r|\t|\v|\f| )*(?<NewLine>({Environment.NewLine}|\n)+)?)+", RegexOptions.Compiled);
    
    public Lexer()
    {
    }
    
    public Lexer(IEnumerable<TokenDefinition> definitions)
    {
        foreach (var def in definitions)
            AddDefinition(def);
    }
    
    public void AddDefinition(TokenDefinition definition)
    {
        if (definition == null)
            throw new ArgumentNullException("definition");
        _definitions.Add(definition);
    }
    
    public IEnumerable<Token> Tokenize(string source, bool ignoreWhitespace = true)
    {
        int index = 0;
        int line = 1;
        int column = 0;

        while(index < source.Length)
        {
            TokenDefinition matchedDefinition = null;
            int matchLength = 0;

            foreach(var rule in _definitions)
            {
                var match = rule.Regex.Match(source, index);

                if(match.Success && match.Index - index == 0 && match.Length > matchLength)
                {
                    matchedDefinition = rule;
                    matchLength = match.Length;
                }
            }

            if (matchedDefinition == null)
                throw new UnrecognizedTokenException(source[index], new TokenPosition(index, line, column), 
                    $"Unrecognized symbol '{source[index]}' at index {index} (line {line}, column {column})");

            var value = source.Substring(index, matchLength);

            if (!matchedDefinition.IsIgnored)
                yield return new Token(matchedDefinition.Type, value, new TokenPosition(index, line, column));

            var whitespace = _whiteSpace.Match(source, index + matchLength);

            if (whitespace.Success && whitespace.Length > 0)
            {
                if (!ignoreWhitespace)
                    yield return new Token("Whitespace", whitespace.Value, new TokenPosition(whitespace.Index, line, column + matchLength));

                matchLength += whitespace.Length;
                var newLines = whitespace.Groups["NewLine"];
                if (newLines.Success)
                {
                    line += newLines.Captures.Count;
                    column = whitespace.Length - (whitespace.Value.LastIndexOf(newLines.Value) + 1);
                }
                else
                    column += matchLength;
            }
            else
                column += matchLength;

            index += matchLength;
        }

        yield return new Token(EoF, null, new TokenPosition(index, line, column));
    }
}