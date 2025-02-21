using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SharedLibrary.Services;

namespace EWB_Tracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var fileWatcherService = new FileWatcherService(GetDefaultLogFolderLocation());
            ServiceLocator.RegisterService(fileWatcherService);
        }

        public static string GetDefaultLogFolderLocation()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "EVE",
                "logs",
                "Gamelogs"
            );
        }
    }
}