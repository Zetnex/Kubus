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
            if (member is ConstantNode constant)
            {
                classNode.AddConstant(constant);
            }
            else if (member != null)
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
            
            // Check for constant declaration
            if (Match("const"))
            {
                return ParseConstant(visibility);
            }
            
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

    private ConstantNode ParseConstant(string visibility)
    {
        string name = Consume("identifier", "Expected constant name").Value;
        Consume("assign", "Expected '=' after constant name");
        ExpressionNode value = ParseExpr();
        Consume("semicolon", "Expected ';' after constant declaration");
        return new ConstantNode(visibility, name, value);
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
        return ParseConcat();
    }

    private ExpressionNode ParseConcat()
    {
        var expr = ParseEquality();

        while (Match("dot"))
        {
            var operatorToken = Previous();
            var right = ParseEquality();
            expr = new ConcatExpressionNode(expr, operatorToken, right);
        }

        return expr;
    }
    
    private ExpressionNode ParseEquality()
    {
        var expr = ParseComparison();

        while (Match("equal") || Match("not_equal"))
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
        while (Match("multiply") || Match("divide") || Match("modulus"))
        {
            var operatorToken = Previous();
            var right = ParseUnary();
            expr = new BinaryExpressionNode(expr, operatorToken, right);
        }
        return expr;
    }

    private ExpressionNode ParseUnary()
    {
        if (Match("exclamation") || Match("minus"))
        {
            var operatorToken = Previous();
            var right = ParseUnary();
            return new UnaryExpressionNode(operatorToken, right);
        }
        return ParsePrimary();
    }

    private ExpressionNode ParsePrimary()
    {
        if (Check("identifier"))
        {
            var identifier = Consume("identifier", "Expected identifier").Value;
            if (Peek().Type == "open_paren")
            {
                var functionCall = ParseFunctionCall(identifier);
                if (Match("dot"))
                {
                    // Start of method call chain, e.g., getLogger().info("")
                    var receiver = new ThisNode(); // Assume $this
                    var calls = new List<(string, List<ExpressionNode>)> { (identifier, functionCall.Arguments) };
                    // Parse the immediate method call after the dot
                    if (Check("identifier") && Peek(1).Type == "open_paren")
                    {
                        var methodName = Consume("identifier", "Expected method name").Value;
                        var methodCall = ParseFunctionCall(methodName);
                        calls.Add((methodName, methodCall.Arguments));
                    }
                    else
                    {
                        throw new Exception("Expected method call after '.'");
                    }
                    return ParseMethodCallChain(receiver, calls); // Handle additional dots
                }
                // Standalone method call, e.g., saveDefaultConfig()
                return new MethodCallChainNode(new ThisNode(), new List<(string, List<ExpressionNode>)> { (identifier, functionCall.Arguments) });
            }
            if (Peek().Type == "dot")
            {
                // Static method call, e.g., Logger.getInstance()
                Consume("dot", "Expected '.' after class name");
                var receiver = new ClassNameNode(identifier);
                var calls = new List<(string, List<ExpressionNode>)>();
                if (Check("identifier") && Peek(1).Type == "open_paren")
                {
                    var methodName = Consume("identifier", "Expected method name").Value;
                    var methodCall = ParseFunctionCall(methodName);
                    calls.Add((methodName, methodCall.Arguments));
                }
                else
                {
                    throw new Exception("Expected method call after '.'");
                }
                return ParseMethodCallChain(receiver, calls);
            }
            
            // Constant or standalone identifier, e.g., DEBUG
            return new ConstantAccessNode(identifier);
        }

        if (Match("variable_sign"))
        {
            string variableName = Consume("identifier", "Expected variable name after '$'").Value;
            if (Match("dot"))
            {
                var receiver = new LocalVariableNode(variableName);
                var calls = new List<(string, List<ExpressionNode>)>();
                if (Check("identifier") && Peek(1).Type == "open_paren")
                {
                    string methodName = Consume("identifier", "Expected method name").Value;
                    var methodCall = ParseFunctionCall(methodName);
                    calls.Add((methodName, methodCall.Arguments));
                }
                else
                {
                    throw new Exception("Expected method call after '.'");
                }
                return ParseMethodCallChain(receiver, calls);
            }
            return new LocalVariableNode(variableName);
        }

        if (Match("local_variable_sign"))
        {
            string variableName = Consume("identifier", "Expected variable name after '$'").Value;
            return new ObjectProprertyNode(variableName); // Corrected typo: ObjectProprertyNode -> ObjectPropertyNode
        }
        
        if (Match("static_local_variable_sign"))
        {
            string variableName = Consume("identifier", "Expected variable name after '$'").Value;
            return new StaticPropertyNode(variableName);
        }

        if (Match("integer") || Match("float") || Match("string") || Match("char") || Match("boolean"))
        {
            return new LiteralExpressionNode(Previous());
        }

        if (Match("open_paren"))
        {
            var expr = ParseExpr();
            Consume("close_paren", "Expected ')' after expression");
            return new GroupingExpressionNode(expr);
        }

        // Debugging: Log unexpected token
        Console.WriteLine($"Unexpected token in ParsePrimary: Type={Peek().Type}, Value={Peek().Value}, Line={Peek().Position.Line}, Column={Peek().Position.Column}");
        throw new Exception("Expected expression");
    }

    private MethodCallChainNode ParseMethodCallChain(ExpressionNode receiver, List<(string, List<ExpressionNode>)> calls)
    {
        while (Match("dot"))
        {
            if (Check("identifier") && Peek(1).Type == "open_paren")
            {
                var methodName = Consume("identifier", "Expected method name").Value;
                var functionCall = ParseFunctionCall(methodName);
                calls.Add((methodName, functionCall.Arguments));
            }
            else
            {
                throw new Exception("Expected method call after '.'");
            }
        }
        return new MethodCallChainNode(receiver, calls);
    }

    private FunctionCallNode ParseFunctionCall(string functionName)
    {
        Consume("open_paren", "Expected '(' after function name");
        var arguments = new List<ExpressionNode>();
    
        if (!Check("close_paren"))
        {
            do
            {
                arguments.Add(ParseExpr());
            } while (Match("comma"));
        }
    
        Consume("close_paren", "Expected ')' after function arguments");
        return new FunctionCallNode(functionName, arguments);
    }

    private ExpressionNode ParseStaticFunctionCall()
    {
        Consume("static_local_variable_sign", "Expected '$$$' before function name");
        var name = Consume("identifier", "Expected function name").Value;
        Consume("open_paren", "Expected '(' after function name");
        var arguments = new List<ExpressionNode>();
        
        if (!Check("close_paren"))
        {
            do
            {
                arguments.Add(ParseExpr());
            } while (Match("comma"));
        }
        
        Consume("close_paren", "Expected ')' after function arguments");
        return new StaticFunctionCallNode(name, arguments);
    }

    private StatementNode ParseStatement()
    {
        if (Check("variable_sign") && Peek(1).Type == "identifier" && 
            (Peek(2).Type == "assign" || Peek(2).Type == "increment" || Peek(2).Type == "decrement"))
        {
            Consume("variable_sign"); // Now consume $ after checking
            return ParseVariableDeclaration();
        }

        if (Match("local_variable_sign"))
        {
            return ParseLocalVariableDeclaration();
        }
    
        if (Match("static_local_variable_sign") && !(Peek(1).Type == "identifier" && Peek(2).Type == "open_paren"))
        {
            return ParseStaticLocalVariableDeclaration();
        }

        if (Match("return"))
        {
            return ParseReturn();
        }

        if (Match("if"))
        {
            return ParseIf();
        }

        return ParseExpressionStatement();
    }

    private StatementNode ParseIf()
    {
        Consume("open_paren", "Expected '(' after 'if'");
        var condition = ParseExpr();
        Consume("close_paren", "Expected ')' after condition");
        var thenBlock = ParseBlock();

        BlockNode? elseBlock = null;
        if (Match("else") || Match("el"))
        {
            elseBlock = ParseBlock();
        }

        return new IfStatementNode(condition, thenBlock, elseBlock);
    }

    private StatementNode ParseReturn()
    {
        var returnValue = ParseExpr();
        Consume("semicolon", "Expected ';' after return statement");
        return new ReturnStatementNode(returnValue);
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
    
        if (Check("increment"))
        {
            Consume("increment", "Expected '++' after variable name");
            Consume("semicolon", "Expected ';' after variable declaration");
            return new VariableIncrementNode(name);
        }
    
        if (Check("decrement"))
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

        var currentToken = Peek();
        throw new Exception($"{errorMessage}. Found token: {currentToken.Type} ('{currentToken.Value}') at line {currentToken.Position.Line} column {currentToken.Position.Column}");
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