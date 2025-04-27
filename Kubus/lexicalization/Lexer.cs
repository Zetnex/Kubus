using System.Text.RegularExpressions;

namespace Kubus.lexicalization;

public class Lexer
{
    public const string EoF = "__0End__";
    public const string Whitespace = "Whitespace";
    
    private List<TokenDefinition> _definitions = new();
    private static Regex _whiteSpace = new Regex(@"\s+", RegexOptions.Compiled); // Simplified to match any whitespace

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
        int column = 1; // Changed to 1-based for consistency with common error reporting

        while (index < source.Length)
        {
            TokenDefinition matchedDefinition = null;
            int matchLength = 0;

            // Find the longest matching token at the current index
            foreach (var rule in _definitions)
            {
                var match = rule.Regex.Match(source, index);
                if (match.Success && match.Index == index && match.Length > matchLength)
                {
                    matchedDefinition = rule;
                    matchLength = match.Length;
                }
            }

            if (matchedDefinition == null)
                throw new UnrecognizedTokenException(source[index], new TokenPosition(index, line, column), 
                    $"Unrecognized symbol '{source[index]}' at index {index} (line {line}, column {column})");

            var value = source.Substring(index, matchLength);

            // Yield the token if not ignored
            if (!matchedDefinition.IsIgnored)
                yield return new Token(matchedDefinition.Type, value, new TokenPosition(index, line, column));

            // Update position for the token
            int newlinesInToken = value.Count(c => c == '\n');
            if (newlinesInToken > 0)
            {
                line += newlinesInToken;
                // Set column to the position after the last newline
                int lastNewlineIndex = value.LastIndexOf('\n');
                column = matchLength - (lastNewlineIndex + 1);
            }
            else
            {
                column += matchLength;
            }

            index += matchLength;

            // Handle whitespace
            var whitespaceMatch = _whiteSpace.Match(source, index);
            if (whitespaceMatch.Success && whitespaceMatch.Index == index)
            {
                var whitespaceValue = whitespaceMatch.Value;
                int whitespaceLength = whitespaceMatch.Length;

                if (!ignoreWhitespace)
                    yield return new Token("Whitespace", whitespaceValue, new TokenPosition(index, line, column));

                // Update position for whitespace
                int newlinesInWhitespace = whitespaceValue.Count(c => c == '\n');
                if (newlinesInWhitespace > 0)
                {
                    line += newlinesInWhitespace;
                    int lastNewlineIndex = whitespaceValue.LastIndexOf('\n');
                    column = whitespaceLength - (lastNewlineIndex + 1);
                }
                else
                {
                    column += whitespaceLength;
                }

                index += whitespaceLength;
            }
        }

        yield return new Token(EoF, null, new TokenPosition(index, line, column));
    }
}