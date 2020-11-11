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
        private WeakReference _reference;

        public WeakAction(object target, Action action)
        {
            _reference = new WeakReference(target);
            Action = action;
        }

        public Action Action { get; }

        public bool IsAlive => _reference != null && _reference.IsAlive;

        public object Target => _reference?.Target;

        public void Execute()
        {
            if (Action != null
                && IsAlive)
            {
                Action();
            }
        }

        public void MarkForDeletion()
        {
            _reference = null;
        }
    }
}
