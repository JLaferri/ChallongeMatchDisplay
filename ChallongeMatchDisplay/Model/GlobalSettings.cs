using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fizzi.Applications.ChallongeVisualization.Model
{
    class GlobalSettings
    {
        #region Singleton Pattern Region
        private static volatile GlobalSettings instance;
        private static object syncRoot = new Object();

        private GlobalSettings()
        {
            initialize();
        }

        public static GlobalSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null) instance = new GlobalSettings();
                    }
                }

                return instance;
            }
        }
        #endregion

        public NewMatchAction SelectedNewMatchAction { get; set; }

        private void initialize()
        {
            SelectedNewMatchAction = NewMatchAction.None;
        }
    }

    enum NewMatchAction
    {
        None,
        AutoAssign,
        Anywhere
    }
}
