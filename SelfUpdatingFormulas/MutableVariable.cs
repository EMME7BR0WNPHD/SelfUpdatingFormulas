using System;
using System.Diagnostics;

namespace SelfUpdatingFormulas
{
    /// <summary>
    /// Implementation of a <see cref="IMutableVariable"/>
    /// </summary>
    [DebuggerDisplay("{_name} = {Value}")]
    public sealed class MutableVariable<T> : IMutableVariable<T>
    {
        public static implicit operator T(MutableVariable<T> t) => t._value;

        private readonly string _name;
        private T _value;
        private bool _isChanging;

        public MutableVariable(T initialValue, string name = default) : this(name)
        {
            _value = initialValue;
        }

        public MutableVariable(string name)
        {
            _value = default;
            _name = name;
        }

        /// <summary>
        /// Current value of a variable
        /// </summary>
        public T Value
        {
            get => _value;
            set
            {
                if (_value.Equals(value) || _isChanging)
                {
                    return;
                }

                _isChanging = true;
                _value = value;
                NotifyChanged();
                _isChanging = false;
            }
        }

        /// <summary>
        /// Event that is fired whenever value of the variable is changed
        /// </summary>
        public event EventHandler Changed;

        private void NotifyChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}