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
        foreach (var constant in node.Constants)
        {
            output.AppendLine(_indentation.ApplyIndent(TranspileConstant(constant)));
        }
        
        foreach (var member in node.Members)
        {
            output.AppendLine(_indentation.ApplyIndent(TranspileMember(member)));
        }
        _indentation.DecreaseIndent();
        
        output.AppendLine("}");
        
        return output.ToString();
    }

    private string TranspileConstant(ConstantNode constant)
    {
        var visibility = constant.Visibility.ToLower() + " ";
        var name = constant.Name;
        var value = $" = {TranspileExpression(constant.Value)}";

        return $"{visibility}const {name}{value};";
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

    private string TranspileBody(BlockNode memberBody)
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
            case "ReturnStatement":
                return TranspileReturnStatement((ReturnStatementNode)statement);
            case "ExpressionStatement":
                return TranspileExpression(((ExpressionStatementNode)statement).Expression) + ";";
            case "IfStatement":
                return TranspileIfStatement((IfStatementNode)statement);
            default:
                return statement.NodeType + " is not implemented yet.";
        }
    }

    private string TranspileIfStatement(IfStatementNode statement)
    {
        var condition = TranspileExpression(statement.Condition);
        var output = new StringBuilder();
        output.AppendLine($"if ({condition}) {{");
    
        _indentation.IncreaseIndent();
        var thenBlock = TranspileBody(statement.ThenBlock);
        if (thenBlock != "")
        {
            output.AppendLine(thenBlock);
        }
        _indentation.DecreaseIndent();
    
        output.Append(_indentation.ApplyIndent("}"));

        if (statement.ElseBlock != null)
        {
            output.AppendLine(" else {");
            _indentation.IncreaseIndent();
            var elseBlock = TranspileBody(statement.ElseBlock);
            if (elseBlock != "")
            {
                output.AppendLine(elseBlock);
            }
            _indentation.DecreaseIndent();
            output.AppendLine(_indentation.ApplyIndent("}"));
        }

        return output.ToString().TrimEnd();
    }

    private string TranspileReturnStatement(ReturnStatementNode statement)
    {
        var returnValue = TranspileExpression(statement.ReturnValue);
        return $"return {returnValue};";
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
                var literal = (LiteralExpressionNode)expression;
                return literal.Value.Type switch
                {
                    "string" => $"{literal.Value.Value}",
                    "boolean" => literal.Value.Value.ToLower(),
                    _ => literal.Value.Value
                };
            case "LocalVariable":
                return "$" + ((LocalVariableNode)expression).Name;
            case "ObjectProperty":
                return "$this->" + ((ObjectProprertyNode)expression).PropertyName;
            case "StaticProperty":
                return "self::$" + ((StaticPropertyNode)expression).PropertyName;
            case "FunctionCall":
                return TranspileFunctionCall((FunctionCallNode)expression);
            case "ConstantAccess":
                return "self::" + ((ConstantAccessNode)expression).Name;
            case "ClassName":
                return ((ClassNameNode)expression).Name;
            case "This":
                return "$this";
            case "MethodCallChain":
                return TranspileMethodCallChain((MethodCallChainNode)expression);
            default:
                throw new Exception($"Unknown expression node type: {expression.NodeType}");
        }
    }

    private string TranspileMethodCallChain(MethodCallChainNode expression)
    {
        var output = new StringBuilder();
        
        // Transpile the receiver
        if (expression.Receiver is ThisNode)
        {
            output.Append("$this");
        }
        else if (expression.Receiver is ClassNameNode className)
        {
            output.Append(className.Name);
        }
        else if (expression.Receiver is LocalVariableNode variable)
        {
            output.Append("$" + variable.Name);
        }
        else
        {
            output.Append(TranspileExpression(expression.Receiver));
        }

        // Transpile each method call
        bool isFirstCall = true;
        foreach (var (methodName, arguments) in expression.Calls)
        {
            if (expression.Receiver is ClassNameNode && isFirstCall)
            {
                output.Append("::"); // Static call, e.g., Logger::getInstance
            }
            else
            {
                output.Append("->"); // Instance call, e.g., ->info
            }
            
            output.Append(methodName);
            output.Append("(");
            var argStrings = arguments.Select(arg => TranspileExpression(arg));
            output.Append(string.Join(", ", argStrings));
            output.Append(")");
            
            isFirstCall = false;
        }

        return output.ToString();
    }

    private string TranspileStaticFunctionCall(StaticFunctionCallNode expression)
    {
        var functionName = expression.FunctionName;
        var arguments = new List<string>();
        
        foreach (var argument in expression.Arguments)
        {
            arguments.Add(TranspileExpression(argument));
        }

        return $"self::{functionName}({string.Join(", ", arguments)})";
    }

    private string TranspileFunctionCall(FunctionCallNode functionCall)
    {
        var output = new StringBuilder();
        output.Append(functionCall.FunctionName);
        output.Append("(");
        var argStrings = functionCall.Arguments.Select(arg => TranspileExpression(arg));
        output.Append(string.Join(", ", argStrings));
        output.Append(")");
        return output.ToString();
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