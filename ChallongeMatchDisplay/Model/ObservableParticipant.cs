using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fizzi.Libraries.ChallongeApiWrapper;
using Fizzi.Applications.ChallongeVisualization.Common;
using System.ComponentModel;

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
        #endregion

        public TournamentContext OwningContext { get; private set; }

        public ObservableParticipant(Participant participant, TournamentContext context)
        {
            source = participant;
            OwningContext = context;
        }

        public void Update(Participant newData)
        {
            var oldData = source;
            source = newData;

            //Raise notify event for any property that has changed value
            foreach (var property in participantProperties)
            {
                if (property.GetValue(oldData, null) != property.GetValue(newData, null)) this.Raise(property.Name, PropertyChanged);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
