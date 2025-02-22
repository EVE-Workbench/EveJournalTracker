using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using SharedLibrary.Jobs;
using SharedLibrary.Repositories;
using SharedLibrary.Services;
using SharedLibrary.Utils;

namespace EWB_Tracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private CheckOnlineJob _checkOnlineJob;
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var fileWatcherService = new FileWatcherService(EveUtils.GetDefaultLogFolderLocation());
            ServiceLocator.RegisterService(fileWatcherService);

            _checkOnlineJob = new CheckOnlineJob(5000);
            _checkOnlineJob.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            
            ServiceLocator.GetService<FileWatcherService>().StopWatching();
            _checkOnlineJob.Stop();
            base.OnExit(e);
        }
    }
}