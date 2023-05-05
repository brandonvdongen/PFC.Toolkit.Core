using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PFC.Toolkit.Core.Helpers {
    public static class PFCReflectionHelper {
        public static string GetMemberPath<T>(Expression<Func<T, object>> expr) {
            MemberExpression memberExpr = (MemberExpression)expr.Body;

            Stack<string> stack = new Stack<string>();
            stack.Push(memberExpr.Member.Name);

            while (memberExpr.Expression is MemberExpression) {
                memberExpr = (MemberExpression)memberExpr.Expression;
                stack.Push(memberExpr.Member.Name);
            }
            return string.Join(".", stack);
        }
    }
}