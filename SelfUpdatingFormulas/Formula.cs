using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
                VisitArrayItems(node);
                VisitRegularItems(node);
                VisitObservableCollection(node);
                return base.Visit(node);
            }

            private void VisitRegularItems(Expression node)
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
            }

            private void VisitObservableCollection(Expression node)
            {
                if (node is MemberExpression me)
                {
                    var type = me.Type;
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ObservableCollection<>))
                    {
                        var objectMember = Expression.Convert(me, typeof(object));
                        var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                        var getter = getterLambda.Compile();
                        var o = getter();
                        if (o is INotifyCollectionChanged iNotifyCollectionChanged)
                        {
                            if (_subscribe)
                            {
                                iNotifyCollectionChanged.CollectionChanged += ObservableCollectionChanged;
                            }
                            else
                            {
                                iNotifyCollectionChanged.CollectionChanged += ObservableCollectionChanged;
                            }
                        }
                        if (o is IEnumerable ienumerable)
                        {
                            foreach (var variable in ienumerable.OfType<IMutableVariable>())
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
                    }
                }
            }

            private void ObservableCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.OldItems != null)
                {
                    foreach (var oldItem in e.OldItems.OfType<IMutableVariable>())
                    {
                        oldItem.Changed -= _changed;
                    }
                }

                if (e.NewItems != null)
                {
                    foreach (var newItem in e.NewItems.OfType<IMutableVariable>())
                    {
                        newItem.Changed += _changed;
                    }
                }
                _changed?.Invoke(this, EventArgs.Empty);
            }

            private void VisitArrayItems(Expression node)
            {
                if (node is MemberExpression me)
                {
                    var type = me.Type;
                    if (type.IsArray && type.GetElementType().GetInterfaces().Contains(typeof(IMutableVariable)))
                    {
                        var objectMember = Expression.Convert(me, typeof(object));
                        var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                        var getter = getterLambda.Compile();
                        var o = getter();
                        if (o is IEnumerable ienumerable)
                        {
                            foreach (var variable in ienumerable.OfType<IMutableVariable>())
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
                    }
                }
            }
        }
    }
}