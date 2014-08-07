using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fizzi.Applications.ChallongeVisualization.Model
{
    class Dirtyable<T>
    {
        public bool IsDirty { get; private set; }

        private T _value;
        public T Value { get { return getValue(); } set { setValue(value); } }

        private object syncLock = new object();

        public Dirtyable(T startValue)
        {
            _value = startValue;

            IsDirty = false;
        }

        private T getValue()
        {
            lock (syncLock)
            {
                return _value;
            }
        }

        private void setValue(T value)
        {
            lock (syncLock)
            {
                //If a non-suggested change is made to the variable, mark it dirty
                if (!object.Equals(_value, value))
                {
                    _value = value;

                    IsDirty = true;
                }
            }
        }

        public void SuggestValue(T value)
        {
            lock (syncLock)
            {
                //Only allow a change suggestion if the variable is not dirty
                if (!IsDirty)
                {
                    _value = value;
                }
            }
        }

        public void CommitIfDirty(Action commitAction)
        {
            lock (syncLock)
            {
                if (IsDirty)
                {
                    //Commit dirt misc value to server
                    commitAction();

                    //Set no longer dirty
                    IsDirty = false;
                }
            }
        }
    }
}
