#if DEBUG
#define DESTRUCTOR_DEBUG
#endif

using System;
using UnityEngine;


namespace FRG.Core
{
    /// <summary>
    /// Allows the one-time destruction of another object.
    /// This object is reusable and will catch failed attempts at using it.
    /// </summary>
    public sealed class Destructor : IRecyclable
    {
        private enum State
        {
            Unattached = 0,
            Attaching = 1,
            Attached = 2,
            Unattaching = 3,
        }

        object _target;
        Action<object> _callback;

        /// <summary>
        /// Debug-only: serial number incremented upon reuse.
        /// </summary>
        uint _currentId = 0;

        public object Target { get { return _target; } }

        private State AttachState { get { return GetAttachState(_currentId); } }

        private static State GetAttachState(uint id)
        {
            return (State)(id & 3);
        }

        public Destructor()
        {
#if DESTRUCTOR_DEBUG
            GC.SuppressFinalize(this);
#endif
        }

        public int Attach<T>(T target, Action<object> callback)
            where T : class
        {
            Debug.Assert(callback == null || target != null, "Must have a target if there is a callback.");
            Debug.Assert(AttachState == State.Unattached);

            _currentId += 1;

            _target = target;
            _callback = callback;

#if DESTRUCTOR_DEBUG
            GC.ReRegisterForFinalize(this);
#endif
            _currentId += 1;
            return (int)_currentId;
        }

        public void Destroy(int destructorId)
        {
            InternalDestroy(destructorId, true);
        }

        /// <summary>
        /// Gets rid of the destructor without calling it.
        /// This is almost always the wrong option.
        /// </summary>
        /// <param name="destructorId">The id of the destructor that is expected.</param>
        public void UnsafeRelease(int destructorId)
        {
            InternalDestroy(destructorId, false);
        }

        private void InternalDestroy(int destructorId, bool destroy)
        {
            Debug.Assert(GetAttachState((uint)destructorId) == State.Attached, "Need a valid id.");

            // Don't assert. Failing assertions and such will mask bigger errors.
            if (_currentId != (uint)destructorId)
            {
                //LogLog.Error(typeof(Destructor), string.Format("Attempt to destroy {0} multiple times.", ToString()));
                return;
            }

            _currentId += 1;

#if DESTRUCTOR_DEBUG
            GC.SuppressFinalize(this);
#endif

            if (destroy)
            {
                Destroy();
            }

            _currentId += 1;
        }

        /// <summary>
        /// Called when the object is manually destroyed.
        /// </summary>
        private void Destroy()
        {
            object target = _target;
            Action<object> callback = _callback;
            _target = null;
            _callback = null;

            if (target != null && callback != null)
            {
                callback(target);
            }
        }

        bool IRecyclable.Recycle()
        {
            if (AttachState != State.Unattached)
            {
                //LogLog.Error(typeof(Destructor), string.Format("Attempt to destroy {0} multiple times.", ToString()));
                return false;
            }

            return true;
        }

#if DESTRUCTOR_DEBUG
        /// <summary>
        /// Finalizer used to check for lost references.
        /// </summary>
        /// <remarks>
        /// Finalizers are somewhat expensive, so we'll compile them out of the final game.
        /// </remarks>
        ~Destructor()
        {
#if !GAME_SERVER
            if (FRG.Core.FocusHandler.IsShuttingDown)
            {
                return;
            }
#endif
            //LogLog.Error(typeof(Destructor), string.Format("{0} was never called.", ToString()));

            // Do not destroy. Finalizers will be called on some weird thread.
        }
#endif

        public override string ToString()
        {
            object debugTarget = Target;

            string format = (debugTarget != null) ? "{0}({1}, {2})" : "{0}({1})";
            return string.Format(format, GetType().CSharpFullName(), AttachState.ToString(), debugTarget);
        }
    }
}
