using System;
using EWB_Tracker.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Repositories.Interfaces;

namespace EWB_Tracker.Views;

public partial class DungeonView
{
    public DungeonView(IDungeonRepository dungeonRepository, IServiceProvider serviceProvider)
    {
        InitializeComponent();


        DataContext = new DungeonViewModel(dungeonRepository);
    }
}