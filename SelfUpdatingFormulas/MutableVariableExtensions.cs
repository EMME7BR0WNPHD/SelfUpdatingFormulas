using System;
using System.Linq.Expressions;

namespace SelfUpdatingFormulas
{
    public static class MutableVariableExtensions
    {
        public static Formula<T> SetCalculationFormula<T>(this MutableVariable<T> variable, Expression<Func<T>> expression)
        {
            return new Formula<T>(variable, expression);
        }
    }
}