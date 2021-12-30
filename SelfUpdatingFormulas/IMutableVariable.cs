using System;

namespace SelfUpdatingFormulas
{
    /// <summary>
    /// Interface that provides simple change notification
    /// </summary>
    public interface IMutableVariable
    {
        /// <summary>
        /// Event that is fired whenever value of the variable is changed
        /// </summary>
        event EventHandler Changed;
    }

    /// <summary>
    /// Extends <see cref="IMutableVariable"/> with generic property
    /// </summary>
    /// <typeparam name="T">Generic type of a property</typeparam>
    public interface IMutableVariable<T> : IMutableVariable
    {
        /// <summary>
        /// Current value of a variable
        /// </summary>
        T Value { get; set; }
    }
}
