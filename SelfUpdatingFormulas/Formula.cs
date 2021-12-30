using System;
using System.Linq;
using System.Linq.Expressions;

namespace SelfUpdatingFormulas
{
    /// <summary>
    /// A formula that could consist of several <see cref="IMutableVariable"/>
    /// track their changes, update the value and notify another dependent formulas about the change (if any)
    /// </summary>
    /// <typeparam name="T">Type of a formula's result</typeparam>
    public class Formula<T> : IDisposable
    {
        private readonly Func<T> _func;
        private readonly Expression<Func<T>> _expression;

        #region Ctors

        private readonly IMutableVariable<T> _result;

        public Formula(IMutableVariable<T> result, Expression<Func<T>> expression)
        {
            _result = result;
            _expression = expression;
            _func = expression.Compile();
            MutableVariablesVisitor.SubscribeOnChanged(expression, OnMasterChanged);
            UpdateValue();
        }
        #endregion

        public void Dispose()
        {
            MutableVariablesVisitor.UnsubscribeOnChanged(_expression, OnMasterChanged);
        }

        private void OnMasterChanged(object sender, EventArgs args)
        {
            UpdateValue();      //.. update the formula's result
        }

        private void UpdateValue()
        {
            _result.Value = _func.Invoke();
        }

        private class MutableVariablesVisitor : ExpressionVisitor
        {
            private readonly EventHandler _changed;
            private readonly bool _subscribe;

            public static void SubscribeOnChanged(Expression expression, EventHandler changed)
            {
                var visitor = new MutableVariablesVisitor(changed, true);
                visitor.Visit(expression);
            }

            public static void UnsubscribeOnChanged(Expression expression, EventHandler changed)
            {
                var visitor = new MutableVariablesVisitor(changed, false);
                visitor.Visit(expression);
            }

            private MutableVariablesVisitor(EventHandler changed, bool subscribe)
            {
                _changed = changed;
                _subscribe = subscribe;
            }

            public override Expression Visit(Expression node)
            {
                if (node is MemberExpression member && member.Type.GetInterfaces().Contains(typeof(IMutableVariable)))
                {
                    var objectMember = Expression.Convert(member, typeof(object));
                    var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                    var getter = getterLambda.Compile();
                    if (getter() is IMutableVariable variable)
                    {
                        if (_subscribe)
                        {
                            variable.Changed += _changed;
                        }
                        else
                        {
                            variable.Changed -= _changed;
                        }
                    }
                }
                return base.Visit(node);
            }
        }
    }
}