using Kubus.lexicalization;
using System.Collections.Generic;

namespace Kubus.parsing;

public class Parser
{
     private readonly Token[] _tokens;
    private int _position;

    public Parser(Token[] tokens)
    {
        _tokens = tokens;
        _position = 0;
    }

    public Ast Parse()
    {
        var ast = new Ast();

        // Parse mandatory namespace
        ast.Namespace = ParseNamespace().QualifiedName;

        // Parse optional imports
        if (Match("use"))
        {
            ast.Imports.AddRange(ParseImportList());
            Consume("semicolon", "Expected ';' after imports");
        }

        // Parse class definitions
        while (!IsAtEnd())
        {
            ast.Classes.Add(ParseClass());
        }

        return ast;
    }

    private NamespaceNode ParseNamespace()
    {
        Consume("namespace", "Expected 'ns' keyword");
        var namespaceDefinition = ParseNamespaceDefinition();
        Consume("semicolon", "Expected ';' after namespace");
        return new NamespaceNode(namespaceDefinition);
    }
    
    private string ParseNamespaceDefinition()
    {
        return Consume("namespace_definition", "Expected namespace definition").Value;
    }

    private List<string> ParseImportList()
    {
        var imports = new List<string>();
        do {
            imports.Add(ParseNamespaceDefinition());
        } while (Match("comma"));
        return imports;
    }

    private string ParseQualifiedName()
    {
        return Consume("identifier", "Expected identifier").Value;
    }
    
    private List<string> ParseQualifiedNameList()
    {
        var names = new List<string>();
        do {
            names.Add(ParseQualifiedName());
        } while (Match("comma"));
        return names;
    }

    private ClassNode ParseClass()
    {
        var name = ParseQualifiedName();
        string parent = Match("colon") ? ParseQualifiedName() : string.Empty;

        string? @interface = null;
        if (Match("exclamation"))
        {
            @interface = ParseQualifiedName();
        }

        Consume("open_brace", "Expected '{' before class body");

        var classNode = new ClassNode(name, parent, @interface);

        while (!Check("close_brace") && !IsAtEnd())
        {
            var member = ParseMember();
            if (member != null)
            {
                classNode.AddMember(member);
            }
        }

        Consume("close_brace", "Expected '}' after class body");
        return classNode;
    }
    private AstNode? ParseMember()
    {
        if (Match("open_paren"))
        {
            return ParseConstructor();
        }
        
        if (CheckVisibility())
        {
            string visibility = ConsumeVisibility();
            bool isStatic = false;
            bool isMethodModifier = false;

            if (Match("static"))
            {
                isStatic = true;
                if (Check("variable_sign")) return ParseProperty(visibility, isStatic);
            }
            else if (Match("magic"))
            {
                isMethodModifier = true;
            }

            if (Check("variable_sign") || CheckDatatype())
            {
                return ParseProperty(visibility, isStatic);
            }

            return ParseMethod(visibility, isStatic, isMethodModifier);
        }

        if (Check("identifier") && Peek(1).Type == "open_paren")
        {
            throw new Exception("Missing visibility on function definition");
        }

        // TraitUsageStatement
        if (Check("use"))
        {
            Consume("use");
            var traitNames = ParseQualifiedNameList();
            Consume("semicolon", "Expected ';' after trait usage");
            return new TraitUsageStatementNode(traitNames);
        }

        throw new Exception("Unknown class member");
        
        return null;
    }
    
    private bool CheckDatatype()
    {
        return Check("int") || Check("float") || Check("string") || Check("bool") || Check("void") || Check("char") ||
               (Check("identifier") && Peek(1).Type != "open_paren");
    }

    private ConstructorNode ParseConstructor()
    {
        var parameters = ParseParamList();
        Consume("close_paren", "Expected ')' after parameters");
        var block = ParseBlock();
        return new ConstructorNode(parameters, block);
    }

    private PropertyNode ParseProperty(string visibility, bool isStatic)
    {
        Token type;
        if (CheckDatatype())
        {
            type = Consume();
        }
        else
        {
            throw new Exception("Datatypes are required on properties");
        }
        
        Consume("variable_sign", "Expected '$' before property name");
        string name = Consume("identifier", "Expected property name").Value;
        ExpressionNode? initialValue = null;
        
        if (Match("assign"))
        {
            initialValue = ParseExpr();
        }
        
        Consume("semicolon", "Expected ';' after property");
        return new PropertyNode(visibility, type, isStatic, name, initialValue);
    }

    private MethodNode ParseMethod(string visibility, bool isStatic, bool isMethod)
    {
        string name = Consume("identifier", "Expected method name").Value;
        Consume("open_paren", "Expected '(' after method name");
        var parameters = ParseParamList();
        Consume("close_paren", "Expected ')' after parameters");

        string? returnType = null;
        if (CheckDatatype())
        {
            returnType = Consume().Value;
        }

        var block = ParseBlock();
        return new MethodNode(visibility, isStatic, isMethod, name, parameters, returnType, block);
    }

    private List<ParameterNode> ParseParamList()
    {
        var parameters = new List<ParameterNode>();
        if (!Check("close_paren"))
        {
            do {
                parameters.Add(ParseParameter());
            } while (Match("comma"));
        }
        return parameters;
    }

    private ParameterNode ParseParameter()
    {
        Consume("variable_sign", "Expected '$' before parameter name");
        string name = Consume("identifier", "Expected parameter name").Value;
        
        if(!CheckDatatype()) throw new Exception("Expected datatype for parameter");
        
        string type = Consume().Value;
        return new ParameterNode(name, type);
    }

    private string ConsumeVisibility()
    {
        if (Match("public")) return "public";
        if (Match("private")) return "private";
        if (Match("protected")) return "protected";
        throw new Exception("Expected visibility modifier");
    }

    private bool CheckVisibility()
    {
        return Check("public") || Check("private") || Check("protected");
    }

    private ExpressionNode ParseExpr()
    {
        return ParseEquality();
    }

    private ExpressionNode ParseEquality()
    {
        var expr = ParseComparison();

        while (Match("equal_equal") || Match("bang_equal"))
        {
            var operatorToken = Previous();
            var right = ParseComparison();
            expr = new BinaryExpressionNode(expr, operatorToken, right);
        }

        return expr;
    }

    private ExpressionNode ParseComparison()
    {
        var expr = ParseTerm();

        while (Match("greater") || Match("greater_equal") || Match("less") || Match("less_equal"))
        {
            var operatorToken = Previous();
            var right = ParseTerm();
            expr = new BinaryExpressionNode(expr, operatorToken, right);
        }

        return expr;
    }

    private ExpressionNode ParseTerm()
    {
        var expr = ParseFactor();

        while (Match("plus") || Match("minus"))
        {
            var operatorToken = Previous();
            var right = ParseFactor();
            expr = new BinaryExpressionNode(expr, operatorToken, right);
        }

        return expr;
    }

    private ExpressionNode ParseFactor()
    {
        var expr = ParseUnary();

        while (Match("star") || Match("slash"))
        {
            var operatorToken = Previous();
            var right = ParseUnary();
            expr = new BinaryExpressionNode(expr, operatorToken, right);
        }

        return expr;
    }

    private ExpressionNode ParseUnary()
    {
        if (Match("bang") || Match("minus"))
        {
            var operatorToken = Previous();
            var right = ParseUnary();
            return new UnaryExpressionNode(operatorToken, right);
        }

        return ParsePrimary();
    }

    private ExpressionNode ParsePrimary()
    {
        if (Match("variable_sign"))
        {
            string variableName = Consume("identifier", "Expected variable name after '$'").Value;
            return new LocalVariableNode(variableName);
        }

        if (Match("local_variable_sign"))
        {
            string variableName = Consume("identifier", "Expected variable name after '$'").Value;
            return new ObjectProprertyNode(variableName);
        }
        
        if (Match("static_local_variable_sign"))
        {
            string variableName = Consume("identifier", "Expected variable name after '$'").Value;
            return new StaticPropertyNode(variableName);
        }

        if (Match("integer"))
        {
            return new LiteralExpressionNode(Previous());
        }
        
        if (Match("float"))
        {
            return new LiteralExpressionNode(Previous());
        }
        
        if (Match("string"))
        {
            return new LiteralExpressionNode(Previous());
        }
        
        if (Match("char"))
        {
            return new LiteralExpressionNode(Previous());
        }

        if (Match("open_paren"))
        {
            var expr = ParseExpr();
            Consume("close_paren", "Expected ')' after expression");
            return new GroupingExpressionNode(expr);
        }

        throw new Exception("Expected expression");
    }

    private StatementNode ParseStatement()
    {
        if (Match("variable_sign"))
        {
            return ParseVariableDeclaration();
        }

        if (Match("local_variable_sign"))
        {
            return ParseLocalVariableDeclaration();
        }
        
        if(Match("static_local_variable_sign"))
        {
            return ParseStaticLocalVariableDeclaration();
        }

        return ParseExpressionStatement();
    }
    
    private StatementNode ParseStaticLocalVariableDeclaration()
    {
        var name = Consume("identifier", "Expected variable name").Value;
        
        if(Check("increment"))
        {
            Consume("increment", "Expected '++' after variable name");
            Consume("semicolon", "Expected ';' after variable declaration");
            return new StaticLocalVariableIncrementNode(name);
        }
        
        if(Check("decrement"))
        {
            Consume("decrement", "Expected '--' after variable name");
            Consume("semicolon", "Expected ';' after variable declaration");
            return new StaticLocalVariableDecrementNode(name);
        }
        
        Consume("assign", "Expected '=' after variable name");
        var initializer = ParseExpr();
        Consume("semicolon", "Expected ';' after variable declaration");
        return new StaticLocalVariableDeclarationNode(name, initializer);
    }
    
    private StatementNode ParseLocalVariableDeclaration()
    {
        var name = Consume("identifier", "Expected variable name").Value;
        
        if(Check("increment"))
        {
            Consume("increment", "Expected '++' after variable name");
            Consume("semicolon", "Expected ';' after variable declaration");
            return new LocalVariableIncrementNode(name);
        }
        
        if(Check("decrement"))
        {
            Consume("decrement", "Expected '--' after variable name");
            Consume("semicolon", "Expected ';' after variable declaration");
            return new LocalVariableDecrementNode(name);
        }
        
        Consume("assign", "Expected '=' after variable name");
        var initializer = ParseExpr();
        Consume("semicolon", "Expected ';' after variable declaration");
        return new LocalVariableDeclarationNode(name, initializer);
    }

    private StatementNode ParseVariableDeclaration()
    {
        var name = Consume("identifier", "Expected variable name").Value;
        
        if(Check("increment"))
        {
            Consume("increment", "Expected '++' after variable name");
            Consume("semicolon", "Expected ';' after variable declaration");
            return new VariableIncrementNode(name);
        }
        
        if(Check("decrement"))
        {
            Consume("decrement", "Expected '--' after variable name");
            Consume("semicolon", "Expected ';' after variable declaration");
            return new VariableDecrementNode(name);
        }
        
        Consume("assign", "Expected '=' after variable name");
        var initializer = ParseExpr();
        Consume("semicolon", "Expected ';' after variable declaration");
        return new VariableDeclarationNode(name, initializer);
    }

    private StatementNode ParseExpressionStatement()
    {
        var expr = ParseExpr();
        Consume("semicolon", "Expected ';' after expression");
        return new ExpressionStatementNode(expr);
    }

    private BlockNode ParseBlock()
    {
        Consume("open_brace", "Expected '{'");
        var statements = new List<StatementNode>();
        
        while (!Check("close_brace") && !IsAtEnd())
        {
            statements.Add(ParseStatement());
        }
        
        Consume("close_brace", "Expected '}'");
        return new BlockNode(statements);
    }

    private Token Consume(string? type = null, string? errorMessage = null)
    {
        if (type == null)
        {
            return Advance();
        }
        
        if (Check(type))
        {
            return Advance();
        }

        throw new Exception(errorMessage);
    }

    private bool Match(string type)
    {
        if (Check(type))
        {
            Advance();
            return true;
        }

        return false;
    }

    private bool Check(string type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) _position++;
        return Previous();
    }

    private bool IsAtEnd()
    {
        return _tokens[_position].Type == Lexer.EoF;
    }

    private Token Peek()
    {
        return _tokens[_position];
    }

    public Token Peek(int depth)
    {
        if (_position + depth > _tokens.Length)
        {
            return _tokens[^1];
        }

        return _tokens[_position + depth];
    }

    private Token Previous()
    {
        return _tokens[_position - 1];
    }
}