using System.Linq.Expressions;

namespace TraderView.Infrastructure.Repositories;

public partial class GenericRepository<T> where T : class
{
    /// <summary>
    /// Helper class to translate simple expression trees into SQL and extract parameters.
    /// Supports binary operators (==, !=, &&, ||, >, <, >=, <=) and basic string Contains.
    /// This is intentionally small and can be extended as needed.
    /// </summary>
    private class ExpressionToSqlTranslator
    {
        private int _paramIndex = 0;
        private readonly List<KeyValuePair<string, object?>> _parameters = new();

        public (string sql, List<KeyValuePair<string, object?>> parameters) Translate(Expression expression)
        {
            _paramIndex = 0;
            _parameters.Clear();
            var sql = Visit(expression);
            return (sql, new List<KeyValuePair<string, object?>>(_parameters));
        }

        private string Visit(Expression? expr)
        {
            if (expr == null) return string.Empty;

            switch (expr)
            {
                case BinaryExpression b:
                    return VisitBinary(b);
                case MemberExpression m:
                    return VisitMember(m);
                case ConstantExpression c:
                    return VisitConstant(c);
                case MethodCallExpression mc:
                    return VisitMethodCall(mc);
                case UnaryExpression u:
                    return Visit(u.Operand);
                default:
                    throw new NotSupportedException($"Expression type '{expr.NodeType}' is not supported for SQL translation");
            }
        }

        private string VisitBinary(BinaryExpression b)
        {
            if (b.NodeType == ExpressionType.AndAlso || b.NodeType == ExpressionType.OrElse)
            {
                var leftSql = Visit(b.Left);
                var rightSql = Visit(b.Right);
                var op = b.NodeType == ExpressionType.AndAlso ? "AND" : "OR";
                return $"({leftSql} {op} {rightSql})";
            }

            var leftExpr = b.Left as MemberExpression ?? (b.Left is UnaryExpression lu ? ((UnaryExpression)b.Left).Operand as MemberExpression : null);
            var rightConst = b.Right as ConstantExpression ?? (b.Right is UnaryExpression ru ? ((UnaryExpression)b.Right).Operand as ConstantExpression : null);

            var left = Visit(b.Left);

            // Handle null comparisons
            if (b.NodeType == ExpressionType.Equal && (IsNullConstant(b.Right) || IsNullConstant(b.Left)))
            {
                var resolvedName = leftExpr != null ? leftExpr.Member.Name : left;
                return $"[{resolvedName}] IS NULL";
            }
            if (b.NodeType == ExpressionType.NotEqual && (IsNullConstant(b.Right) || IsNullConstant(b.Left)))
            {
                var resolvedName = leftExpr != null ? leftExpr.Member.Name : left;
                return $"[{resolvedName}] IS NOT NULL";
            }

            string sqlOp = b.NodeType switch
            {
                ExpressionType.Equal => "=",
                ExpressionType.NotEqual => "<>",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                _ => throw new NotSupportedException($"Binary operator '{b.NodeType}' is not supported")
            };

            object? value = null;
            if (rightConst != null)
            {
                value = rightConst.Value;
            }
            else
            {
                try
                {
                    var lambda = Expression.Lambda(b.Right);
                    var compiled = lambda.Compile();
                    value = compiled.DynamicInvoke();
                }
                catch
                {
                    throw new NotSupportedException("Only constant values are supported on the right side of binary expressions");
                }
            }

            var paramName = GetNextParamName();
            _parameters.Add(new KeyValuePair<string, object?>(paramName, value));

            var resolvedName2 = leftExpr != null ? leftExpr.Member.Name : left;

            return $"[{resolvedName2}] {sqlOp} {paramName}";
        }

        private static bool IsNullConstant(Expression expr)
        {
            if (expr is ConstantExpression c)
                return c.Value == null;
            if (expr is UnaryExpression u && u.Operand is ConstantExpression cu)
                return cu.Value == null;
            return false;
        }

        private string VisitMember(MemberExpression m)
        {
            if (m.Expression is ParameterExpression)
            {
                return m.Member.Name;
            }

            var value = GetValue(m);
            var paramName = GetNextParamName();
            _parameters.Add(new KeyValuePair<string, object?>(paramName, value));
            return paramName;
        }

        private string VisitConstant(ConstantExpression c)
        {
            var paramName = GetNextParamName();
            _parameters.Add(new KeyValuePair<string, object?>(paramName, c.Value));
            return paramName;
        }

        private string VisitMethodCall(MethodCallExpression mc)
        {
            if (mc.Method.Name == "Contains")
            {
                if (mc.Object != null && mc.Object is MemberExpression member && mc.Arguments.Count == 1)
                {
                    var memberName = member.Member.Name;
                    object? value = null;
                    var arg = mc.Arguments[0];
                    if (arg is ConstantExpression ce) value = ce.Value;
                    else
                    {
                        try
                        {
                            var lambda = Expression.Lambda(arg);
                            var compiled = lambda.Compile();
                            value = compiled.DynamicInvoke();
                        }
                        catch
                        {
                            throw new NotSupportedException("Unsupported argument to Contains");
                        }
                    }

                    var paramName = GetNextParamName();
                    _parameters.Add(new KeyValuePair<string, object?>(paramName, $"%{value}%"));
                    return $"[{memberName}] LIKE {paramName}";
                }
            }

            throw new NotSupportedException($"Method call '{mc.Method.Name}' is not supported for SQL translation");
        }

        private string GetNextParamName()
        {
            var name = "@p" + _paramIndex++;
            return name;
        }

        private static object? GetValue(MemberExpression member)
        {
            try
            {
                var objectMember = Expression.Convert(member, typeof(object));
                var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                var getter = getterLambda.Compile();
                return getter();
            }
            catch
            {
                return null;
            }
        }
    }
}
