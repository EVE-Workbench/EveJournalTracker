using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EWB_Tracker.Commands;
using SharedLibrary.Models;
using Microsoft.Extensions.DependencyInjection;

namespace EWB_Tracker.ViewModels;

public class BountyRunViewModel : INotifyPropertyChanged
{
    private readonly IServiceProvider _serviceProvider;
    private string _runName;

    public BountyRunViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
        // Generate default name with current time
        var mainViewModel = _serviceProvider.GetService<MainWindowViewModel>();
        var runCount = mainViewModel?.BountyRuns.Count + 1 ?? 1;
        var currentTime = DateTime.Now.ToString("h:mm tt");
        RunName = $"Run #{runCount}, {currentTime}";
        
        StartRunCommand = new RelayCommand(StartRun);
    }

    public string RunName
    {
        get => _runName;
        set
        {
            _runName = value;
            OnPropertyChanged();
        }
    }

    public ICommand StartRunCommand { get; }

    private void StartRun()
    {
        if (string.IsNullOrWhiteSpace(RunName))
            return;

        var bountyRun = new BountyRun
        {
            Id = DateTime.Now.Ticks.GetHashCode(), // Simple ID generation for in-memory
            Name = RunName,
            StartTime = DateTime.Now,
            TotalIsk = 0,
            IsCompleted = false
        };

        // Get MainWindowViewModel and set current run
        var mainViewModel = _serviceProvider.GetService<MainWindowViewModel>();
        // DEBUG: Check if we got the ViewModel
        if (mainViewModel == null)
        {
            System.Diagnostics.Debug.WriteLine("❌ MainWindowViewModel is null!");
            return;
        }
    
        System.Diagnostics.Debug.WriteLine($"✅ Adding bounty run: {bountyRun.Name}");
        System.Diagnostics.Debug.WriteLine($"✅ BountyRuns count before: {mainViewModel.BountyRuns.Count}");
    
        mainViewModel.SetCurrentBountyRun(bountyRun);
    
        System.Diagnostics.Debug.WriteLine($"✅ BountyRuns count after: {mainViewModel.BountyRuns.Count}");
        System.Diagnostics.Debug.WriteLine($"✅ IsBountyRunActive: {mainViewModel.IsBountyRunActive}");

        OnRunStarted?.Invoke();
    }

    public event Action OnRunStarted;
    public event PropertyChangedEventHandler PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}