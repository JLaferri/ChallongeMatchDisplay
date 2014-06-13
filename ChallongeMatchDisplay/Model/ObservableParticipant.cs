using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fizzi.Libraries.ChallongeApiWrapper;
using Fizzi.Applications.ChallongeVisualization.Common;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Fizzi.Applications.ChallongeVisualization.Model
{
    class ObservableParticipant : INotifyPropertyChanged
    {
        private static System.Reflection.PropertyInfo[] participantProperties = typeof(Participant).GetProperties();

        private Participant source;

        #region Externalize Source Properties
        public int Id { get { return source.Id; } }
        public string Name { get { return source.Name; } }

        public int Seed { get { return source.Seed; } }
        public string Misc { get { return source.Misc; } }
        #endregion

        public TournamentContext OwningContext { get; private set; }

        #region Convenience Properties
        private ParticipantMiscProperties miscProperties;
        private DateTime lastManualMiscChange = DateTime.MinValue;

        public bool IsMissing
        {
            get { return miscProperties.IsMissing; }
            set { miscPropertySetter("IsMissing", value, IsMissing, newVal => miscProperties.IsMissing = newVal); }
        }

        private void miscPropertySetter<T>(string property, T newValue, T currentValue, Action<T> setProperty)
        {
            if (!object.Equals(newValue, currentValue))
            {
                setProperty(newValue);

                this.Raise(property, PropertyChanged);
                
                //Commit changes to misc property on challonge
                Observable.Start(() =>  OwningContext.Portal.SetParticipantMisc(OwningContext.Tournament.Id, this.Id, miscProperties.ToString()));
                source.Misc = miscProperties.ToString();

                //Store change time to ensure misc changed wont be raised until one full poll frequency
                lastManualMiscChange = DateTime.UtcNow;
            }
        }
        #endregion

        public ObservableParticipant(Participant participant, TournamentContext context)
        {
            source = participant;
            miscProperties = ParticipantMiscProperties.Parse(Misc);
            OwningContext = context;

            //Listen for when properties changed to that changed events for the convenience properties can also be fired.
            this.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case "Misc":
                        //Handle misc string changes by parsing the new values and raising the properties that have changed
                        var oldMiscProperties = miscProperties;
                        miscProperties = ParticipantMiscProperties.Parse(Misc);

                        //Check for changes from old to new, raise those properties if changed
                        if (oldMiscProperties.IsMissing != miscProperties.IsMissing) this.Raise("IsMissing", PropertyChanged);
                        break;
                }
            };
        }

        public void Update(Participant newData)
        {
            var oldData = source;
            source = newData;

            var recentLocalMiscChange = OwningContext.CurrentPollInterval.HasValue && 
                DateTime.UtcNow - lastManualMiscChange < OwningContext.CurrentPollInterval.Value;

            //Raise notify event for any property that has changed value
            foreach (var property in participantProperties)
            {
                if (recentLocalMiscChange && property.Name == "Misc") continue; //When a local change has recently happened to Misc, do not accept change from server
                if (!object.Equals(property.GetValue(oldData, null), property.GetValue(newData, null))) this.Raise(property.Name, PropertyChanged);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
