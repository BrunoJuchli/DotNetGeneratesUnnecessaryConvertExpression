using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Text;

namespace ConvertExpression;

public class ExpressionTreePrinter : ExpressionVisitor
{
    readonly StringBuilder sb = new StringBuilder();
    int indent;

    void Indent()
    {
        indent++;
    }

    void Dedent()
    {
        indent--;
    }

    void AppendLine(string text)
    {
        sb.Append(new string(' ', indent * 2)); // 2 spaces per indent level
        sb.AppendLine(text);
    }

    void AppendNodeInfo(Expression node)
    {
        AppendLine($"Node Type: {node.NodeType} (C# Type: {node.GetType().Name}) (Return Type: {node.Type.Name})");
    }

    public string Print(Expression expression)
    {
        sb.Clear();
        Visit(expression);
        return sb.ToString();
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        AppendLine($"--- BinaryExpression ---");
        AppendNodeInfo(node);
        AppendLine($"Operation: {node.NodeType}");
        Indent();
        AppendLine("Left:");
        Visit(node.Left);
        AppendLine("Right:");
        Visit(node.Right);
        if (node.Method != null)
        {
            AppendLine($"Method: {node.Method.DeclaringType?.Name}.{node.Method.Name}");
        }
        if (node.Conversion != null)
        {
            AppendLine($"Conversion: {node.Conversion.ReturnType.Name} {node.Conversion.Name}");
        }
        Dedent();
        return base.VisitBinary(node);
    }

    protected override Expression VisitBlock(BlockExpression node)
    {
        AppendLine($"--- BlockExpression ---");
        AppendNodeInfo(node);
        AppendLine($"Result Type: {node.Type.Name}");
        Indent();
        if (node.Variables.Any())
        {
            AppendLine("Variables:");
            foreach (var variable in node.Variables)
            {
                Visit(variable);
            }
        }
        AppendLine("Expressions:");
        foreach (var expr in node.Expressions)
        {
            Visit(expr);
        }
        Dedent();
        return base.VisitBlock(node);
    }

    protected override CatchBlock VisitCatchBlock(CatchBlock node)
    {
        AppendLine($"--- CatchBlock ---");
        AppendLine($"Type: {node.Test?.Name ?? "General Exception"}");
        Indent();
        if (node.Variable != null)
        {
            AppendLine("Variable:");
            Visit(node.Variable);
        }
        if (node.Filter != null)
        {
            AppendLine("Filter:");
            Visit(node.Filter);
        }
        AppendLine("Body:");
        Visit(node.Body);
        Dedent();
        return base.VisitCatchBlock(node);
    }

    protected override Expression VisitConditional(ConditionalExpression node)
    {
        AppendLine($"--- ConditionalExpression ---");
        AppendNodeInfo(node);
        Indent();
        AppendLine("Test:");
        Visit(node.Test);
        AppendLine("If True (ValueIfTrue):");
        Visit(node.IfTrue);
        AppendLine("If False (ValueIfFalse):");
        Visit(node.IfFalse);
        Dedent();
        return base.VisitConditional(node);
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        AppendLine($"--- ConstantExpression ---");
        AppendNodeInfo(node);
        AppendLine($"Value: {node.Value ?? "null"}");
        return base.VisitConstant(node);
    }

    protected override Expression VisitDebugInfo(DebugInfoExpression node)
    {
        AppendLine($"--- DebugInfoExpression ---");
        AppendNodeInfo(node);
        AppendLine($"Start Line: {node.StartLine}, Start Column: {node.StartColumn}");
        AppendLine($"End Line: {node.EndLine}, End Column: {node.EndColumn}");
        AppendLine($"Document: {node.Document.FileName}");
        return base.VisitDebugInfo(node);
    }

    protected override Expression VisitDefault(DefaultExpression node)
    {
        AppendLine($"--- DefaultExpression ---");
        AppendNodeInfo(node);
        AppendLine($"Default Value Type: {node.Type.Name}");
        return base.VisitDefault(node);
    }

    protected override Expression VisitDynamic(DynamicExpression node)
    {
        AppendLine($"--- DynamicExpression ---");
        AppendNodeInfo(node);
        AppendLine($"Binder Type: {node.Binder.GetType().Name}");
        Indent();
        AppendLine("Arguments:");
        Visit(node.Arguments);
        Dedent();
        return base.VisitDynamic(node);
    }

    protected override Expression VisitExtension(Expression node)
    {
        // This is for custom expression types that derive from Expression but are not built-in
        AppendLine($"--- ExtensionExpression ---");
        AppendNodeInfo(node);
        AppendLine($"Custom Type: {node.GetType().FullName}");
        return base.VisitExtension(node);
    }

    protected override Expression VisitGoto(GotoExpression node)
    {
        AppendLine($"--- GotoExpression ---");
        AppendNodeInfo(node);
        AppendLine($"Kind: {node.Kind}");
        AppendLine($"Target: {node.Target.Name ?? "Unnamed Target"} (Type: {node.Target.Type?.Name ?? "Void"})");
        Indent();
        if (node.Value != null)
        {
            AppendLine("Value:");
            Visit(node.Value);
        }
        Dedent();
        return base.VisitGoto(node);
    }

    protected override Expression VisitIndex(IndexExpression node)
    {
        AppendLine($"--- IndexExpression ---");
        AppendNodeInfo(node);
        Indent();
        AppendLine("Object:");
        Visit(node.Object);
        AppendLine($"Indexer: {node.Indexer?.Name ?? "Default Indexer"}");
        AppendLine("Arguments:");
        Visit(node.Arguments);
        Dedent();
        return base.VisitIndex(node);
    }

    protected override Expression VisitInvocation(InvocationExpression node)
    {
        AppendLine($"--- InvocationExpression ---");
        AppendNodeInfo(node);
        Indent();
        AppendLine("Expression (Target):");
        Visit(node.Expression);
        AppendLine("Arguments:");
        Visit(node.Arguments);
        Dedent();
        return base.VisitInvocation(node);
    }

    protected override Expression VisitLabel(LabelExpression node)
    {
        AppendLine($"--- LabelExpression ---");
        AppendNodeInfo(node);
        AppendLine($"Target: {node.Target.Name ?? "Unnamed Target"} (Type: {node.Target.Type?.Name ?? "Void"})");
        Indent();
        if (node.DefaultValue != null)
        {
            AppendLine("Default Value:");
            Visit(node.DefaultValue);
        }
        Dedent();
        return base.VisitLabel(node);
    }

    protected override LabelTarget? VisitLabelTarget(LabelTarget? node)
    {
        if (node != null)
        {
            AppendLine($"--- LabelTarget ---");
            AppendLine($"Name: {node.Name ?? "Unnamed"} (Type: {node.Type?.Name ?? "Void"})");
        }

        return base.VisitLabelTarget(node);
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        AppendLine($"--- LambdaExpression ---");
        AppendNodeInfo(node);
        AppendLine($"Delegate Type: {node.Type.Name}");
        Indent();
        AppendLine("Parameters:");
        VisitParameterList(node.Parameters);
        AppendLine("Body:");
        Visit(node.Body);
        Dedent();
        return base.VisitLambda(node);
    }

    protected override Expression VisitListInit(ListInitExpression node)
    {
        AppendLine($"--- ListInitExpression ---");
        AppendNodeInfo(node);
        Indent();
        AppendLine("New Expression:");
        Visit(node.NewExpression);
        AppendLine("Initializers:");
        foreach (var initializer in node.Initializers)
        {
            AppendLine($"- {initializer.AddMethod.Name} (Method: {initializer.AddMethod.DeclaringType?.Name}.{initializer.AddMethod.Name})");
            Indent();
            AppendLine("Arguments:");
            Visit(initializer.Arguments);
            Dedent();
        }
        Dedent();
        return base.VisitListInit(node);
    }

    protected override Expression VisitLoop(LoopExpression node)
    {
        AppendLine($"--- LoopExpression ---");
        AppendNodeInfo(node);
        Indent();
        if (node.BreakLabel != null)
        {
            AppendLine($"Break Label: {node.BreakLabel.Name ?? "Unnamed"}");
        }
        if (node.ContinueLabel != null)
        {
            AppendLine($"Continue Label: {node.ContinueLabel.Name ?? "Unnamed"}");
        }
        AppendLine("Body:");
        Visit(node.Body);
        Dedent();
        return base.VisitLoop(node);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        AppendLine($"--- MemberExpression ---");
        AppendNodeInfo(node);
        AppendLine($"Member: {node.Member.DeclaringType?.Name}.{node.Member.Name} (Member Type: {node.Member.MemberType})");
        Indent();
        if (node.Expression != null)
        {
            AppendLine("Expression (Object/Container):");
            Visit(node.Expression);
        }
        Dedent();
        return base.VisitMember(node);
    }

    protected override MemberBinding VisitMemberBinding(MemberBinding node)
    {
        AppendLine($"--- MemberBinding ---");
        AppendLine($"Binding Type: {node.BindingType}");
        AppendLine($"Member: {node.Member.DeclaringType?.Name}.{node.Member.Name}");
        return base.VisitMemberBinding(node);
    }

    protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
    {
        AppendLine($"--- MemberAssignment ---");
        AppendLine($"Member: {node.Member.DeclaringType?.Name}.{node.Member.Name}");
        Indent();
        AppendLine("Expression (Value):");
        Visit(node.Expression);
        Dedent();
        return base.VisitMemberAssignment(node);
    }

    protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
    {
        AppendLine($"--- MemberListBinding ---");
        AppendLine($"Member: {node.Member.DeclaringType?.Name}.{node.Member.Name}");
        Indent();
        AppendLine("Initializers:");
        foreach (var elementInit in node.Initializers)
        {
            AppendLine($"- {elementInit.AddMethod.Name} (Method: {elementInit.AddMethod.DeclaringType?.Name}.{elementInit.AddMethod.Name})");
            Indent();
            AppendLine("Arguments:");
            Visit(elementInit.Arguments);
            Dedent();
        }
        Dedent();
        return base.VisitMemberListBinding(node);
    }

    protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
    {
        AppendLine($"--- MemberMemberBinding ---");
        AppendLine($"Member: {node.Member.DeclaringType?.Name}.{node.Member.Name}");
        Indent();
        AppendLine("Bindings:");
        foreach (var binding in node.Bindings)
        {
            VisitMemberBinding(binding); // Call specific visitor for nested binding
        }
        Dedent();
        return base.VisitMemberMemberBinding(node);
    }


    protected override Expression VisitMemberInit(MemberInitExpression node)
    {
        AppendLine($"--- MemberInitExpression ---");
        AppendNodeInfo(node);
        Indent();
        AppendLine("New Expression:");
        Visit(node.NewExpression);
        AppendLine("Bindings:");
        foreach (var binding in node.Bindings)
        {
            Indent(); // Indent for the binding itself
            VisitMemberBinding(binding); // Calls the appropriate VisitMemberBinding override
            Dedent();
        }
        Dedent();
        return base.VisitMemberInit(node);
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        AppendLine($"--- MethodCallExpression ---");
        AppendNodeInfo(node);
        AppendLine($"Method: {node.Method.DeclaringType?.Name}.{node.Method.Name}");
        Indent();
        if (node.Object != null)
        {
            AppendLine("Object (Instance):");
            Visit(node.Object);
        }
        if (node.Arguments.Any())
        {
            AppendLine("Arguments:");
            Visit(node.Arguments);
        }
        Dedent();
        return base.VisitMethodCall(node);
    }

    protected override Expression VisitNew(NewExpression node)
    {
        AppendLine($"--- NewExpression ---");
        AppendNodeInfo(node);
        AppendLine($"Type: {node.Type.Name}");
        Indent();
        if (node.Constructor != null)
        {
            AppendLine($"Constructor: {node.Constructor.DeclaringType?.Name}.{node.Constructor.Name}");
        }
        if (node.Arguments.Any())
        {
            AppendLine("Arguments:");
            Visit(node.Arguments);
        }
        if (node.Members != null && node.Members.Any())
        {
            AppendLine("Members (Anonymous Type):");
            foreach (var member in node.Members)
            {
                AppendLine($"- {member.Name}");
            }
        }
        Dedent();
        return base.VisitNew(node);
    }

    protected override Expression VisitNewArray(NewArrayExpression node)
    {
        AppendLine($"--- NewArrayExpression ---");
        AppendNodeInfo(node);
        AppendLine($"Array Type: {node.Type.GetElementType()?.Name}[]");
        AppendLine($"Array Kind: {node.NodeType}"); // NewArrayBounds or NewArrayInit
        Indent();
        AppendLine("Expressions (Elements/Bounds):");
        Visit(node.Expressions);
        Dedent();
        return base.VisitNewArray(node);
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        AppendLine($"--- ParameterExpression ---");
        AppendNodeInfo(node);
        AppendLine($"Name: {node.Name ?? "Unnamed Parameter"}");
        return base.VisitParameter(node);
    }

    protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
    {
        AppendLine($"--- RuntimeVariablesExpression ---");
        AppendNodeInfo(node);
        Indent();
        AppendLine("Variables:");
        VisitParameterList(node.Variables);
        Dedent();
        return base.VisitRuntimeVariables(node);
    }

    protected override Expression VisitSwitch(SwitchExpression node)
    {
        AppendLine($"--- SwitchExpression ---");
        AppendNodeInfo(node);
        Indent();
        AppendLine("Switch Value:");
        Visit(node.SwitchValue);
        if (node.Comparison != null)
        {
            AppendLine($"Comparison Method: {node.Comparison.DeclaringType?.Name}.{node.Comparison.Name}");
        }
        AppendLine("Cases:");
        foreach (var @case in node.Cases)
        {
            AppendLine($"- Case:");
            Indent();
            AppendLine("Test Values:");
            Visit(@case.TestValues);
            AppendLine("Body:");
            Visit(@case.Body);
            Dedent();
        }
        if (node.DefaultBody != null)
        {
            AppendLine("Default Body:");
            Visit(node.DefaultBody);
        }
        Dedent();
        return base.VisitSwitch(node);
    }

    protected override Expression VisitTry(TryExpression node)
    {
        AppendLine($"--- TryExpression ---");
        AppendNodeInfo(node);
        Indent();
        AppendLine("Body:");
        Visit(node.Body);
        AppendLine("Catch Blocks:");
        foreach (var catchBlock in node.Handlers)
        {
            VisitCatchBlock(catchBlock); // Call the specific VisitCatchBlock method
        }
        if (node.Finally != null)
        {
            AppendLine("Finally Block:");
            Visit(node.Finally);
        }
        if (node.Fault != null)
        {
            AppendLine("Fault Block:");
            Visit(node.Fault);
        }
        Dedent();
        return base.VisitTry(node);
    }

    protected override Expression VisitTypeBinary(TypeBinaryExpression node)
    {
        AppendLine($"--- TypeBinaryExpression ---");
        AppendNodeInfo(node);
        AppendLine($"Operation: {node.NodeType}"); // e.g., TypeIs, TypeAs
        AppendLine($"Target Type: {node.TypeOperand.Name}");
        Indent();
        AppendLine("Expression:");
        Visit(node.Expression);
        Dedent();
        return base.VisitTypeBinary(node);
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        AppendLine($"--- UnaryExpression ---");
        AppendNodeInfo(node);
        AppendLine($"Operation: {node.NodeType}");
        Indent();
        AppendLine("Operand:");
        Visit(node.Operand);
        if (node.Method != null)
        {
            AppendLine($"Method: {node.Method.DeclaringType?.Name}.{node.Method.Name}");
        }
        Dedent();
        return base.VisitUnary(node);
    }

    protected void VisitParameterList(ReadOnlyCollection<ParameterExpression> nodes)
    {
        if (nodes.Any())
        {
            Indent();
            foreach (var node in nodes)
            {
                VisitParameter(node);
            }
            Dedent();
        }
        else
        {
            AppendLine("(No Parameters)");
        }
    }

    protected void VisitExpressionList(ReadOnlyCollection<Expression> original)
    {
        if (original.Any())
        {
            Indent();
            foreach (var node in original)
            {
                Visit(node); // Recursively visit each expression
            }
            Dedent();
        }
        else
        {
            AppendLine("(No Expressions)");
        }
    }
}