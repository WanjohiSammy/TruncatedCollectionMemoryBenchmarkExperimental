using System.Linq.Expressions;
using System.Reflection;

namespace TruncatedCollectionMemoryBenchmark;

internal static class ExpressionHelpers
{
    public static IQueryable Take(IQueryable query, int count, Type type, bool parameterize)
    {
        Expression takeQuery = Take(query.Expression, count, type, parameterize);
        var createMethod = ExpressionHelperMethods.CreateQueryGeneric.MakeGenericMethod(type);

        return createMethod.Invoke(query.Provider, new[] { takeQuery }) as IQueryable;
    }

    public static Expression Take(Expression source, int count, Type elementType, bool parameterize)
    {
        MethodInfo takeMethod;
        if (typeof(IQueryable).IsAssignableFrom(source.Type))
        {
            takeMethod = ExpressionHelperMethods.QueryableTakeGeneric.MakeGenericMethod(elementType);
        }
        else
        {
            takeMethod = ExpressionHelperMethods.EnumerableTakeGeneric.MakeGenericMethod(elementType);
        }

        Expression takeValueExpression = parameterize ? LinqParameterContainer.Parameterize(typeof(int), count) : Expression.Constant(count);
        Expression takeQuery = Expression.Call(null, takeMethod, new[] { source, takeValueExpression });
        return takeQuery;
    }
}
