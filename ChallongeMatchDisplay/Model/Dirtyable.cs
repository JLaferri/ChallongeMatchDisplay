using System;

namespace Fizzi.Applications.ChallongeVisualization.Model;

internal class Dirtyable<T>
{
	private T _value;

	private object syncLock = new object();

	public bool IsDirty { get; private set; }

	public T Value
	{
		get
		{
			return getValue();
		}
		set
		{
			setValue(value);
		}
	}

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
				commitAction();
				IsDirty = false;
			}
		}
	}
}
