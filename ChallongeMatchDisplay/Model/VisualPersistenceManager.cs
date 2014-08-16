using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;

namespace Fizzi.Applications.ChallongeVisualization.Model
{
    [DataContract]
    class VisualPersistenceManager
    {
        #region Singleton Pattern Region
        private static volatile VisualPersistenceManager instance;
        private static object syncRoot = new Object();

        private VisualPersistenceManager()
        {
            initialize();
        }

        public static VisualPersistenceManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null) instance = new VisualPersistenceManager();
                    }
                }

                return instance;
            }
        }
        #endregion

        private string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Fizzi", "ChallongeMatchDisplay", "visual.xml");

        [DataMember]
        public double TextSizeRatio { get; set; }

        private void initialize()
        {
            LoadFromStorage();
        }

        public void LoadFromStorage()
        {
            VisualPersistenceManager result = null;

            try
            {
                DataContractSerializer dcs = new DataContractSerializer(typeof(VisualPersistenceManager));

                using (Stream stream = new FileStream(path, FileMode.Open))
                {
                    result = (VisualPersistenceManager)dcs.ReadObject(stream);
                }
            }
            catch (Exception) { }

            if (result != null)
            {
                TextSizeRatio = result.TextSizeRatio;
            }
            else
            {
                TextSizeRatio = 1;
            }
        }

        public void Save()
        {
            var directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            DataContractSerializer dcs = new DataContractSerializer(typeof(VisualPersistenceManager));

            using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                dcs.WriteObject(stream, this);
            }
        }
    }
}
