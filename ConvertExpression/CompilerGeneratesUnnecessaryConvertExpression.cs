using System.Linq.Expressions;
using AwesomeAssertions;

namespace ConvertExpression;

public class CompilerGeneratesUnnecessaryConvertExpression
{
    [Fact]
    public void FromConcreteClass()
    {
        var expression = ExpressionBuilder<ClassWithIntProperty>.OrderBy(x => x.IntProperty);

        var instance = new ClassWithIntProperty
        {
            IntProperty = 893,
        };

        expression.Compile().Invoke(instance).Should().Be(instance.IntProperty);

        AssertIsMemberExpressionWithParameterExpression(expression);
    }

    [Fact]
    public void FromInterface()
    {
        var expression = ExpressionBuilder<IWithIntProperty>.OrderBy(x => x.IntProperty);

        var instance = new ClassWithIntProperty
        {
            IntProperty = 893,
        };

        expression.Compile().Invoke(instance).Should().Be(instance.IntProperty);

        AssertIsMemberExpressionWithParameterExpression(expression);
    }

    [Fact]
    public void ViaGenericType()
    {
        var expression = CreateExpression<ClassWithIntProperty>();

        var instance = new ClassWithIntProperty
        {
            IntProperty = 893,
        };

        expression.Compile().Invoke(instance).Should().Be(instance.IntProperty);

        AssertIsMemberExpressionWithParameterExpression(expression);
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

    static Expression<Func<T, int>> CreateExpression<T>()
        where T : IWithIntProperty
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