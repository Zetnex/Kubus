using System.Text;
using Kubus.parsing;

namespace Kubus.transpilation;

public class Transpiler
{
    private const string PhpInitialization = "<?php";
    private const string DeclareTypesStrict = "declare(strict_types=1);";

    private readonly Ast _ast;
    private readonly IndentationHandler _indentation = new();

    public Transpiler(Ast ast)
    {
        _ast = ast;
    }

    public string Transpile()
    {
        var output = new StringBuilder();
        output.AppendLine(FollowedByNewLine(PhpInitialization));
        output.AppendLine(FollowedByNewLine(DeclareTypesStrict));
        
        var @namespace = FollowedByNewLine(TranspileNamespace(_ast.Namespace));
        output.AppendLine(@namespace);
        
        var imports = TranspileUses(_ast.Imports);
        if(imports != "") output.AppendLine(imports);
        
        var classes = FollowedByNewLine(TranspileClasses(_ast.Classes));
        if(classes != "") output.AppendLine(classes);
        
        // Todo: Add more transpilation logic here

        return output.ToString().Trim();
    }
    
    private string TranspileNamespace(string namespaceName)
    {
        return $"namespace {namespaceName};";
    }
    
    private string TranspileUses(IEnumerable<string> imports)
    {
        var output = new StringBuilder();
        
        foreach (var import in imports)
        {
            output.AppendLine($"use {import};");
        }

        return output.ToString();
    }
    
    private string TranspileClasses(IEnumerable<ClassNode> classes)
    {
        var output = new StringBuilder();
        
        foreach (var node in classes)
        {
            output.AppendLine(FollowedByNewLine(TranspileClass(node)));
        }

        return output.ToString();
    }

    private string TranspileClass(ClassNode node)
    {
        var output = new StringBuilder();
        
        bool hasInheritance = node.Parent != "";
        bool hasImplements = node.Interface != null;
        
        output.AppendLine($"class {node.Name}{(hasInheritance ? $" extends {node.Parent}" : "")}{(hasImplements ? $" implements {node.Interface}" : "")} {{");
        
        _indentation.IncreaseIndent();
        foreach (var member in node.Members)
        {
            output.AppendLine(_indentation.ApplyIndent(TranspileMember(member)));
        }
        _indentation.DecreaseIndent();
        
        output.AppendLine("}");
        
        return output.ToString();
    }

    private string TranspileMember(AstNode member)
    {
        var output = new StringBuilder();

        switch (member.NodeType)
        {
            case "Property":
                return TranspileProperty((PropertyNode)member);
            case "Constructor":
                return TranspileConstructor((ConstructorNode)member);
            case "Method":
                return TranspileMethod((MethodNode)member);
            default:
                return member.NodeType + " is not implemented yet.";
        }
    }

    private string TranspileMethod(MethodNode member)
    {
        // visibility function name(params): returnType { body }
        var visibility = member.Visibility.ToLower() + " ";
        var @static = member.IsStatic ? "static " : "";
        var returnType = member.ReturnType != null ? $": {member.ReturnType} " : ": void ";
        var functionDeclaration = $"{visibility}{@static}function {member.Name}(";
        
        var parameters = new List<string>();
        foreach (var param in member.Parameters)
        {
            var type = param.Type != "" ? $"{param.Type} " : "";
            var name = "$" + param.Name;
            parameters.Add($"{type}{name}");
        }
        functionDeclaration += string.Join(", ", parameters) + ")" + returnType + "{";
        var output = new StringBuilder(FollowedByNewLine(functionDeclaration));
        
        _indentation.IncreaseIndent();
        output.AppendLine(TranspileBody(member.Body));
        _indentation.DecreaseIndent();
        
        output.Append(_indentation.ApplyIndent("}"));
        
        return output.ToString();
    }

    private string TranspileConstructor(ConstructorNode member)
    {
        var functionDeclaration = $"public function __construct(";
        var parameters = new List<string>();
        foreach (var param in member.Parameters)
        {
            var type = param.Type != "" ? $"{param.Type} " : "";
            var name = "$" + param.Name;
            parameters.Add($"{type}{name}");
        }
        functionDeclaration += string.Join(", ", parameters) + ") {";
        var output = new StringBuilder(FollowedByNewLine(functionDeclaration));
        
        _indentation.IncreaseIndent();
        output.AppendLine(TranspileBody(member.Body));
        _indentation.DecreaseIndent();
        
        output.Append(_indentation.ApplyIndent("}"));
        return output.ToString();
    }

    private string? TranspileBody(BlockNode memberBody)
    {
        var output = new StringBuilder();
        
        foreach (var statement in memberBody.Statements)
        {
            output.AppendLine(_indentation.ApplyIndent(TranspileStatement(statement)));
        }

        return output.ToString().TrimEnd();
    }

    private string TranspileStatement(StatementNode statement)
    {
        switch (statement.NodeType)
        {
            case "VariableDeclaration":
                return TranspileVariableDeclaration((VariableDeclarationNode)statement);
            default:
                return statement.NodeType + " is not implemented yet.";
        }
    }

    private string TranspileVariableDeclaration(VariableDeclarationNode statement)
    {
        var name = "$" + statement.Name;
        var value = $" = {TranspileExpression(statement.Initializer)}";
        return $"{name}{value};";
    }

    private string TranspileProperty(PropertyNode member)
    {
        var visibility = member.Visibility.ToLower() + " ";
        var @static = member.IsStatic ? "static " : "";
        var type = member.Type.Value + " ";
        var name = "$" + member.Name + " ";
        
        string value = "";
        if (member.InitialValue != null)
        {
            var initialValue = TranspileExpression(member.InitialValue);
            value = $"= {initialValue}";
        }

        return $"{visibility}{@static}{type}{name}{value};";
    }

    private string TranspileExpression(ExpressionNode expression)
    {
        switch (expression.NodeType)
        {
            case "LiteralExpression":
                return TranspileLiteralExpression((LiteralExpressionNode)expression);
            case "BinaryExpression":
                return TranspileBinaryExpression((BinaryExpressionNode)expression);
            case "LocalVariable":
                return "$" + ((LocalVariableNode)expression).Name;
            case "ObjectProperty":
                return "$this->" + ((ObjectProprertyNode)expression).PropertyName;
            case "StaticProperty":
                return "self::$" + ((StaticPropertyNode)expression).PropertyName;
            default:
                return expression.NodeType;
        }
    }

    private string TranspileBinaryExpression(BinaryExpressionNode expression)
    {
        var left = TranspileExpression(expression.Left);
        var @operator = expression.Operator.Value;
        var right = TranspileExpression(expression.Right);

        return $"{left} {@operator} {right}";
    }

    private string TranspileLiteralExpression(LiteralExpressionNode expression)
    {
        return expression.Value.Value;
    }

    private string FollowedByNewLine(string code, int times = 1)
    {
        // Nothing has not to be followed by a new line
        if (code == "") return code;
        
        for (var i = 0; i < times; i++)
        {
            code += "\n";
        }

        return code;
    }
}