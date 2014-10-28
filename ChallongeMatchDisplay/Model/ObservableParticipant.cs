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
        public string Name { get { return source.NameOrUsername; } }

        public int Seed { get { return source.Seed; } }
        public string Misc { get { return source.Misc; } }
        #endregion

        public TournamentContext OwningContext { get; private set; }

        #region Convenience Properties
        public Dirtyable<ParticipantMiscProperties> MiscProperties { get; private set; }

        public bool IsMissing
        {
            get { return UtcTimeMissing.HasValue; }
            set { SetMissing(value); }
        }

        public DateTime? UtcTimeMissing { get { return MiscProperties.Value.UtcTimeMissing; } }

        public TimeSpan? TimeSinceMissing { get { return UtcTimeMissing.HasValue ? DateTime.UtcNow - UtcTimeMissing.Value : default(TimeSpan?); } }

        public DateTime? UtcTimeMatchAssigned { get { return MiscProperties.Value.UtcTimeMatchAssigned; } }

        public TimeSpan? TimeSinceAssigned { get { return UtcTimeMatchAssigned.HasValue ? DateTime.UtcNow - UtcTimeMatchAssigned.Value : default(TimeSpan?); } }

        public string StationAssignment { get { return MiscProperties.Value.StationAssignment; } }

        public bool IsAssignedToStation { get { return UtcTimeMatchAssigned.HasValue; } }

        private void miscPropertySetter<T>(string property, T newValue, T currentValue, Action<T> setProperty)
        {
            if (!object.Equals(newValue, currentValue))
            {
                setProperty(newValue);

                this.Raise(property, PropertyChanged);
            }
        }
        #endregion

        public ObservableParticipant(Participant participant, TournamentContext context)
        {
            source = participant;
            MiscProperties = new Dirtyable<ParticipantMiscProperties>(ParticipantMiscProperties.Parse(Misc));
            OwningContext = context;

            //Check tournament start date, if it is later than missing time, clear the player's missing status. This happens in the case of a bracket reset
            var tournamentStart = context.Tournament.StartedAt;
            if (UtcTimeMissing.HasValue && tournamentStart.HasValue && UtcTimeMissing.Value.ToLocalTime() < tournamentStart.Value)
            {
                SetMissing(false);
            }

            //Do a similar check on station assignment time
            if (UtcTimeMatchAssigned.HasValue && tournamentStart.HasValue && UtcTimeMatchAssigned.Value.ToLocalTime() < tournamentStart.Value)
            {
                ClearStationAssignment();
            }

            //Listen for when properties changed to that changed events for the convenience properties can also be fired.
            this.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case "Misc":
                        //Handle misc string changes by parsing the new values and raising the properties that have changed
                        var oldMiscProperties = MiscProperties.Value;

                        //Suggest new value for misc properties. This will be ignored if dirty
                        MiscProperties.SuggestValue(ParticipantMiscProperties.Parse(Misc));
                        var miscProperties = MiscProperties.Value;

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

        public void SetMissing(bool isMissing)
        {
            DateTime? utcTimeMissing = isMissing ? DateTime.UtcNow : default(DateTime?);

            MiscProperties.Value = new ParticipantMiscProperties(utcTimeMissing, MiscProperties.Value.UtcTimeMatchAssigned, MiscProperties.Value.StationAssignment);

            this.Raise("UtcTimeMissing", PropertyChanged);
        }

        public void AssignStation(string stationName)
        {
            if (string.IsNullOrWhiteSpace(stationName)) throw new ArgumentException("Station name must contain characters.");

            //Free previously assigned station
            Stations.Instance.AttemptFreeStation(MiscProperties.Value.StationAssignment);

            //This will mark the properties as dirty which will cause them to be committed to the server on the next scan
            MiscProperties.Value = new ParticipantMiscProperties(MiscProperties.Value.UtcTimeMissing, DateTime.UtcNow, stationName);

            //Attempt to set station in use
            Stations.Instance.AttemptClaimStation(stationName);

            this.Raise("UtcTimeMatchAssigned", PropertyChanged);
            this.Raise("StationAssignment", PropertyChanged);
        }

        public void ClearStationAssignment()
        {
            //Attempt to set station open
            Stations.Instance.AttemptFreeStation(MiscProperties.Value.StationAssignment);

            //This will mark the properties as dirty which will cause them to be committed to the server on the next scan
            MiscProperties.Value = new ParticipantMiscProperties(MiscProperties.Value.UtcTimeMissing, null, null);

            this.Raise("UtcTimeMatchAssigned", PropertyChanged);
            this.Raise("StationAssignment", PropertyChanged);
        }

        public void Update(Participant newData)
        {
            var oldData = source;
            source = newData;

            //Raise notify event for any property that has changed value
            foreach (var property in participantProperties)
            {
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
