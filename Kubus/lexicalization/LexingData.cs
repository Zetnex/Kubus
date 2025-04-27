namespace Kubus.lexicalization;

public class LexingData
{
    public static void ApplyDefinitions(Lexer lexer)
    {
        // Keywords
        Define("ns", "namespace", TypeGroup.Keyword, lexer);
        Define("use", "use", TypeGroup.Keyword, lexer);
        Define("pu", "public", TypeGroup.Keyword, lexer);
        Define("pr", "private", TypeGroup.Keyword, lexer);
        Define("pt", "protected", TypeGroup.Keyword, lexer);
        Define("st", "static", TypeGroup.Keyword, lexer);
        Define("ab", "abstract", TypeGroup.Keyword, lexer);
        Define("if", "if", TypeGroup.Keyword, lexer);
        Define("el", "else", TypeGroup.Keyword, lexer);
        Define("elf", "elseif", TypeGroup.Keyword, lexer);
        Define("re", "return", TypeGroup.Keyword, lexer);
        Define("new", "new", TypeGroup.Keyword, lexer);
        Define("m", "magic", TypeGroup.Keyword, lexer);
        Define("cst", "const", TypeGroup.Keyword, lexer);
        
        // data types
        Define("int", "int", TypeGroup.Keyword, lexer);
        Define("flt", "float", TypeGroup.Keyword, lexer);
        Define("str", "string", TypeGroup.Keyword, lexer);
        Define("chr", "char", TypeGroup.Keyword, lexer);
        Define("bool", "bool", TypeGroup.Keyword, lexer);
        Define("void", "void", TypeGroup.Keyword, lexer);
        
        // literals
        Define("\"([^\"\\\\]|\\\\.)*\"", "string", TypeGroup.Literal, lexer);
        Define("'([^'\\\\]|\\\\.)*'", "char", TypeGroup.Literal, lexer);
        Define("[0-9]+", "integer", TypeGroup.Literal, lexer);
        Define("[0-9]*\\.?[0-9]+([eE][-+]?[0-9]+)?", "float", TypeGroup.Literal, lexer);
        Define("true|false", "boolean", TypeGroup.Literal, lexer);
        
        // identifiers
        Define("[a-zA-Z_][a-zA-Z0-9_]*", "identifier", TypeGroup.Identifier, lexer);

        // namespace definition
        Define("([a-zA-Z_][a-zA-Z0-9_]*\\\\)+[a-zA-Z_][a-zA-Z0-9_]*", "namespace_definition", TypeGroup.Identifier, lexer);
        
        // operators
        Define("\\+", "plus", TypeGroup.Operator, lexer); 
        Define("-", "minus", TypeGroup.Operator, lexer);
        Define("\\*", "multiply", TypeGroup.Operator, lexer);
        Define("/", "divide", TypeGroup.Operator, lexer);
        Define("%", "modulus", TypeGroup.Operator, lexer);
        Define("=", "assign", TypeGroup.Operator, lexer);
        Define("==", "equal", TypeGroup.Operator, lexer);
        Define("!=", "not_equal", TypeGroup.Operator, lexer);
        Define(">", "greater", TypeGroup.Operator, lexer);
        Define(">=", "greater_equal", TypeGroup.Operator, lexer);
        Define("<", "less", TypeGroup.Operator, lexer);
        Define("<=", "less_equal", TypeGroup.Operator, lexer);
        Define("\\+\\+", "increment", TypeGroup.Operator, lexer);
        Define("--", "decrement", TypeGroup.Operator, lexer);
        
        // delimiters
        Define("\\{", "open_brace", TypeGroup.Delimiter, lexer);
        Define("\\}", "close_brace", TypeGroup.Delimiter, lexer);
        Define("\\(", "open_paren", TypeGroup.Delimiter, lexer);
        Define("\\)", "close_paren", TypeGroup.Delimiter, lexer);
        Define("\\[", "open_bracket", TypeGroup.Delimiter, lexer);
        Define("\\]", "close_bracket", TypeGroup.Delimiter, lexer);
        Define(";", "semicolon", TypeGroup.Delimiter, lexer);
        
        // comments
        Define("//.*?(\r|\n)", "single_line_comment", TypeGroup.Comment, lexer);
        Define("/\\*.*?\\*/", "multi_line_comment", TypeGroup.Comment, lexer);
        
        // whitespace
        Define("\\s+", "whitespace", TypeGroup.Whitespace, lexer);
        
        // punctuation
        Define("\\.", "dot", TypeGroup.Punctuation, lexer);
        Define(",", "comma", TypeGroup.Punctuation, lexer);
        Define(":", "colon", TypeGroup.Punctuation, lexer);
        Define("!", "exclamation", TypeGroup.Punctuation, lexer);
        Define("\\?", "question", TypeGroup.Punctuation, lexer);
        
        // special symbols
        Define("\\$", "variable_sign", TypeGroup.SpecialSymbol, lexer);
        Define("\\$\\$", "local_variable_sign", TypeGroup.SpecialSymbol, lexer);
        Define("\\$\\$\\$", "static_local_variable_sign", TypeGroup.SpecialSymbol, lexer);
    }

    private static void Define(string regex, string type, TypeGroup typeGroup, Lexer lexer)
    {
        lexer.AddDefinition(new TokenDefinition(regex, type, typeGroup));  
    }
}