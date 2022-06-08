using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Fizzi.Applications.ChallongeVisualization.Common;
using Fizzi.Libraries.SmashggApiWrapper;

namespace Fizzi.Applications.ChallongeVisualization.Model;

internal class SmashggObservableEntrant : IObservableParticipant, INotifyPropertyChanged
{
	private static PropertyInfo[] entrantProperties = typeof(SmashggEntrant).GetProperties();

	private SmashggEntrant source;

	public int Id => source.Id;

	public string DisplayName => string.Join(", ", source.Participants.Select((SmashggParticipant x) => x.GamerTag).ToArray());

	public SmashggEventPhaseGroupContext OwningContext { get; private set; }

	public string OverlayName => DisplayName;

	public event PropertyChangedEventHandler PropertyChanged;

	private void miscPropertySetter<T>(string property, T newValue, T currentValue, Action<T> setProperty)
	{
		if (!object.Equals(newValue, currentValue))
		{
			setProperty(newValue);
			this.Raise(property, this.PropertyChanged);
		}
	}

	public SmashggObservableEntrant(SmashggEntrant entrant, SmashggEventPhaseGroupContext context)
	{
		source = entrant;
		OwningContext = context;
	}

	public void Update(SmashggEntrant newData)
	{
		SmashggEntrant obj = source;
		source = newData;
		PropertyInfo[] array = entrantProperties;
		foreach (PropertyInfo propertyInfo in array)
		{
			if (!object.Equals(propertyInfo.GetValue(obj, null), propertyInfo.GetValue(newData, null)))
			{
				this.Raise(propertyInfo.Name, this.PropertyChanged);
			}
		}
	}
}
