using System.Linq.Expressions;
using System.Reflection;
using AwesomeAssertions;

namespace ConvertExpression;

public class CompilerGeneratesUnnecessaryConvertExpression
{
    [Fact]
    public void FromClass()
    {
        var expression = ExpressionBuilder<ClassWithIntProperty>.OrderBy(x => x.IntProperty);

        AssertExpressionResolvesPropertyValue(expression);
        AssertIsMemberExpressionWithParameterExpression(expression);
    }

    [Fact]
    public void FromInterface()
    {
        var expression = ExpressionBuilder<IWithIntProperty>.OrderBy(x => x.IntProperty);

        AssertExpressionResolvesPropertyValue(expression);
        AssertIsMemberExpressionWithParameterExpression(expression);
    }

    [Fact]
    public void FromClassViaGenericMethodConstrainedToInterface()
    {
        var expression = CreateExpressionConstrainedToInterface<ClassWithIntProperty>();

        AssertExpressionResolvesPropertyValue(expression);
        AssertIsMemberExpressionWithParameterExpression(expression);
    }

    [Fact]
    public void FromInterfaceViaGenericMethodConstrainedToInterface()
    {
        var expression = CreateExpressionConstrainedToInterface<IWithIntProperty>();

        AssertExpressionResolvesPropertyValue(expression);
        AssertIsMemberExpressionWithParameterExpression(expression);
    }

    [Fact]
    public void FromClassViaGenericMethodConstrainedToClass()
    {
        var expression = CreateExpressionConstrainedToClass<ClassWithIntProperty>();

        AssertExpressionResolvesPropertyValue(expression);
        AssertIsMemberExpressionWithParameterExpression(expression);
    }

    [Fact]
    public void CustomBuiltExpression()
    {
        var expectedExpression = ExpressionBuilder<IWithIntProperty>.OrderBy(x => x.IntProperty);
        var customExpression = CreateExpressionManually();

        AssertExpressionResolvesPropertyValue(expectedExpression);
        AssertExpressionResolvesPropertyValue(customExpression);

        AssertIsMemberExpressionWithParameterExpression(expectedExpression);
        AssertIsMemberExpressionWithParameterExpression(customExpression);

        var expectedTree = new ExpressionTreePrinter().Print(expectedExpression);
        var actualTree = new ExpressionTreePrinter().Print(customExpression);
        actualTree.Should().Be(expectedTree);
    }

    static Expression<Func<IWithIntProperty, int>> CreateExpressionManually()
    {
        // 1. Define the parameter for the input object (e.g., 'f' in f => f.SomeProperty)
        //    This creates a ParameterExpression representing 'f' of type Foo.
        ParameterExpression fooParameter = Expression.Parameter(typeof(IWithIntProperty), "x");

        // // 2. Get the PropertyInfo for 'SomeProperty' from the Foo class using reflection.
        // //    This tells the expression tree which specific property we want to access.
        // PropertyInfo somePropertyInfo = typeof(IWithIntProperty).GetProperty();

        // 3. Create the MemberExpression (property access expression).
        //    This represents 'f.SomeProperty'. It combines the parameter (f) with the property info (SomeProperty).
        MemberExpression propertyAccessExpression = Expression.Property(fooParameter, nameof(IWithIntProperty.IntProperty));

        // 4. Create the LambdaExpression.
        //    This takes the property access expression as the body and the parameter as its argument(s).
        //    The result is an Expression<Func<Foo, int>>.
        return Expression.Lambda<Func<IWithIntProperty, int>>(
            propertyAccessExpression, // The body of the lambda: f.SomeProperty
            fooParameter              // The parameter(s) of the lambda: f
        );
    }

    static void AssertExpressionResolvesPropertyValue<T>(Expression<Func<T, int>> expression)
    where T : IWithIntProperty
    {
        var instance = new ClassWithIntProperty
        {
            IntProperty = 893,
        };

        expression.Compile().Invoke((T)(object)instance).Should().Be(instance.IntProperty);
    }

    static void AssertIsMemberExpressionWithParameterExpression<T>(Expression<Func<T, int>> expression)
    {
        expression.CanReduce.Should().BeFalse();

        var memberExpression = (MemberExpression)expression.Body;
        memberExpression.CanReduce.Should().BeFalse();
        memberExpression.Member.Name.Should().Be(nameof(ClassWithIntProperty.IntProperty));
        memberExpression.Expression!.CanReduce.Should().BeFalse();
        memberExpression.Expression!.NodeType.Should().Be(ExpressionType.Parameter);
    }

    static Expression<Func<T, int>> CreateExpressionConstrainedToInterface<T>()
        where T : IWithIntProperty
    {
        return ExpressionBuilder<T>.OrderBy(x => x.IntProperty);
    }

    static Expression<Func<T, int>> CreateExpressionConstrainedToClass<T>()
        where T : ClassWithIntProperty
    {
        return ExpressionBuilder<T>.OrderBy(x => x.IntProperty);
    }

    static class ExpressionBuilder<TSource>
    {
        public static Expression<Func<TSource, TKey>> OrderBy<TKey>(
            Expression<Func<TSource, TKey>> keySelector)
        {
            return keySelector;
        }
    }

    interface IWithIntProperty
    {
        public int IntProperty { get; }
    }

    class ClassWithIntProperty :
        IWithIntProperty
    {
        public required int IntProperty { get; init; }
    }
}