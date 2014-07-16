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
            get { return UtcTimeMissing.HasValue; }
            set { UtcTimeMissing = value ? DateTime.UtcNow : default(DateTime?); }
        }

        public DateTime? UtcTimeMissing
        {
            get { return miscProperties.UtcTimeMissing; }
            set { miscPropertySetter("UtcTimeMissing", value, UtcTimeMissing, newVal => miscProperties.UtcTimeMissing = newVal); }
        }

        public TimeSpan? TimeSinceMissing { get { return UtcTimeMissing.HasValue ? DateTime.UtcNow - UtcTimeMissing.Value : default(TimeSpan?); } }

        public DateTime? UtcTimeMatchAssigned
        {
            get { return miscProperties.UtcTimeMatchAssigned; }
            set { miscPropertySetter("UtcTimeMatchAssigned", value, UtcTimeMatchAssigned, newVal => miscProperties.UtcTimeMatchAssigned = newVal); }
        }

        public TimeSpan? TimeSinceAssigned { get { return UtcTimeMatchAssigned.HasValue ? DateTime.UtcNow - UtcTimeMatchAssigned.Value : default(TimeSpan?); } }

        public string StationAssignment
        {
            get { return miscProperties.StationAssignment; }
            set { miscPropertySetter("StationAssignment", value, StationAssignment, newVal => miscProperties.StationAssignment = newVal); }
        }

        public bool IsAssignedToStation { get { return UtcTimeMatchAssigned.HasValue; } }

        private void miscPropertySetter<T>(string property, T newValue, T currentValue, Action<T> setProperty)
        {
            //TODO: Throttle SetParticipantMisc so that only one call is made for changes that happen in quick succession
            if (!object.Equals(newValue, currentValue))
            {
                setProperty(newValue);

                this.Raise(property, PropertyChanged);
                
                //Commit changes to misc property on challonge
                source.Misc = miscProperties.ToString();
                Observable.Start(() => OwningContext.Portal.SetParticipantMisc(OwningContext.Tournament.Id, this.Id, source.Misc));
                
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

            //Check tournament start date, if it is later than missing time, clear the player's missing status
            var tournamentStart = context.Tournament.StartedAt;
            if (UtcTimeMissing.HasValue && tournamentStart.HasValue && UtcTimeMissing.Value.ToLocalTime() < tournamentStart.Value)
            {
                UtcTimeMissing = null;
            }

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
                        if (!object.Equals(oldMiscProperties.UtcTimeMissing, miscProperties.UtcTimeMissing)) this.Raise("UtcTimeMissing", PropertyChanged);
                        if (!object.Equals(oldMiscProperties.UtcTimeMatchAssigned, miscProperties.UtcTimeMatchAssigned)) this.Raise("UtcTimeMatchAssigned", PropertyChanged);
                        if (!object.Equals(oldMiscProperties.StationAssignment, miscProperties.StationAssignment)) this.Raise("StationAssignment", PropertyChanged);
                        break;
                    case "UtcTimeMissing":
                        this.Raise("IsMissing", PropertyChanged);
                        this.Raise("TimeSinceMissing", PropertyChanged);
                        break;
                    case "UtcTimeMatchAssigned":
                        this.Raise("TimeSinceAssigned", PropertyChanged);
                        this.Raise("IsAssignedToStation", PropertyChanged);
                        break;
                }
            };
        }

        public void ClearStationAssignment()
        {
            UtcTimeMatchAssigned = null;
            StationAssignment = null;
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

            //Always raise the TimeSinceMissing property if UtcTimeMissing is not null
            if (UtcTimeMissing.HasValue)
            {
                this.Raise("TimeSinceMissing", PropertyChanged);
            }

            //Same as above but for match assignment
            if (UtcTimeMatchAssigned.HasValue)
            {
                this.Raise("TimeSinceAssigned", PropertyChanged);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
