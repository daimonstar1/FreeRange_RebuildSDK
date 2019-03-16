using System;
using UnityEngine;

namespace FRG.Core
{
    /// <summary>
    /// Allows attaching a destructor to an object that can be put in a dispose block.
    /// By convention, passing a value of this type implies passing ownership.
    /// You can use the <see cref="Disown"/> method to make that clear.
    /// </summary>
    /// <remarks>
    /// This is a mutable struct, which is usually a bad idea.
    /// Also an attempt to stuff C++-style RAII and move semantics into C# with an <see cref="IDisposable"/> kludge.
    /// Mutable methods on a struct will only work on immediate locals, fields and array elements.
    /// Never make a <see cref="Pooled{T}"/> readonly.
    /// </remarks>
    public struct Pooled<T> : IDisposable
        where T : class
    {
        // readonly fields do not make a struct immutable, they just help keep them consistent
        // Note: the following applies if Pooled<T> itself is readonly, not its fields
        // https://blogs.msdn.microsoft.com/ericlippert/2008/05/14/mutating-readonly-structs/
        readonly int _destructorId;
        readonly Destructor _destructor;

        public bool HasValue { get { return !ReferenceEquals(_destructor, null); } }

        public T Value { get { return (_destructor != null) ? (T)_destructor.Target : null; } }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="value">The value. May be null.</param>
        /// <param name="destructorCallback">
        /// The destructor, which will be called if this object is disposed.
        /// Will be called multiple times if this object is duplicated incorrectly.
        /// </param>
        public Pooled(T value, Action<object> destructorCallback)
            : this()
        {
            Debug.Assert(destructorCallback == null || value != null, "Must have a value if there is a destructor callback.");

            Destructor destructor = RecyclingPool.SpawnRaw<Destructor>();
            int destructorId = destructor.Attach(value, destructorCallback);

            _destructorId = destructorId;
            _destructor = destructor;
        }

        private Pooled(int destructorId, Destructor destructor)
        {
            _destructorId = destructorId;
            _destructor = destructor;
        }

        /// <summary>
        /// Disposal is what triggers the destructor.
        /// It disowns the current value, but be very sure you
        /// are not working with a copy.
        /// </summary>
        public void Dispose()
        {
            Pooled<T> copy = Disown();
            if (copy._destructor != null)
            {
                copy._destructor.Destroy(copy._destructorId);
            }
        }

        /// <summary>
        /// Resets the current field or array element, passing the result upward.
        /// </summary>
        /// <remarks>
        /// May do unexpected things when called while this object is involved in a using block.
        /// </remarks>
        public Pooled<T> Disown()
        {
            Pooled<T> oldSelf = this;
            // Modifies the current value. Does not update every reference for the object.
            this = default(Pooled<T>);
            return oldSelf;
        }

        /// <summary>
        /// Calls <see cref="Disown"/> to reset and casts a copy to a base or derived class (or interface).
        /// </summary>
        /// <remarks>
        /// This sort of cast is impossible to write with an operator.
        /// Other casts are left out intentionally. They are extremely prone to error.
        /// </remarks>
        public Pooled<U> DisownAs<U>()
            where U : class
        {
            Debug.Assert(!HasValue || Value is U);

            Pooled<T> copy = Disown();

            return new Pooled<U>(copy._destructorId, copy._destructor);
        }
        
        /// <summary>
        /// Disowns the specified value without calling the destructor.
        /// This is almost never the right option.
        /// </summary>
        public T UnsafeRelease()
        {
            Pooled<T> copy = Disown();

            T value = copy.Value;
            copy._destructor.UnsafeRelease(copy._destructorId);
            return value;
        }

        public override string ToString()
        {
            return HasValue ? _destructor.Target.ToString() : "";
        }
    }
}
