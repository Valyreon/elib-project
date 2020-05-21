using System;

namespace MVVMLibrary.Messaging
{
    /// <summary>
    /// Stores an <see cref="Action" /> without causing a hard reference
    /// to be created to the Action's owner. The owner can be garbage collected at any time.
    /// </summary>
    ////[ClassInfo(typeof(Messenger))]
    public class WeakAction
    {
        private readonly Action _action;

        private WeakReference _reference;

        public WeakAction(object target, Action action)
        {
            _reference = new WeakReference(target);
            _action = action;
        }

        public Action Action
        {
            get
            {
                return _action;
            }
        }

        public bool IsAlive
        {
            get
            {
                if (_reference == null)
                {
                    return false;
                }

                return _reference.IsAlive;
            }
        }

        public object Target
        {
            get
            {
                if (_reference == null)
                {
                    return null;
                }

                return _reference.Target;
            }
        }

        public void Execute()
        {
            if (_action != null
                && IsAlive)
            {
                _action();
            }
        }

        public void MarkForDeletion()
        {
            _reference = null;
        }
    }
}