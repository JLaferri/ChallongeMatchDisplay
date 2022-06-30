using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using log4net;

namespace Fizzi.Applications.ChallongeVisualization
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public App()
        {
            IDisposable disposableViewModel = null;

            //Create and show window while storing datacontext
            this.Startup += (sender, args) =>
            {
                log4net.Config.XmlConfigurator.Configure();
                Log.Info("Creating main window");

                MainWindow = new View.MainWindow();
                disposableViewModel = MainWindow.DataContext as IDisposable;

                MainWindow.Show();
            };

            //Dispose on unhandled exception
            this.DispatcherUnhandledException += (sender, args) =>
            {
                Log.Error("Unhandled Exception", args.Exception);
                if (disposableViewModel != null) disposableViewModel.Dispose();
            };

            //Dispose on exit
            this.Exit += (sender, args) =>
            {
                Log.Info("Exiting");
                if (disposableViewModel != null) disposableViewModel.Dispose();
            };
        }
    }
}
