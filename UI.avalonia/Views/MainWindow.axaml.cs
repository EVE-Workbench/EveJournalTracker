using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SharedLibrary.Models;
using SharedLibrary.Services;
using UI.avalonia.ViewModels;
using UI.avalonia.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UI.avalonia.Views
{
    public partial class MainWindow : Window
    {
        private readonly FileWatcherService _fileWatcherService;
        private readonly GlobalHotkeyService _globalHotkeyService;
        private readonly ShortcutService _shortcutService;
        private bool _isWatching = true;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<int, DpsOverlayWindow> _characterDpsOverlays = new();

        public MainWindow(MainWindowViewModel viewModel, FileWatcherService fileWatcherService, GlobalHotkeyService globalHotkeyService, ShortcutService shortcutService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            DataContext = viewModel;

            _fileWatcherService = fileWatcherService;
            _globalHotkeyService = globalHotkeyService;
            _shortcutService = shortcutService;
            _serviceProvider = serviceProvider;
            viewModel.CurrentView = _serviceProvider.GetService<DefaultView>();

            _fileWatcherService.OnISKUpdated += (sender, e) =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    viewModel.UpdateCurrentBountyRunIsk(e.LastBounty, e.Character));
            };

            Opened += (s, e) =>
            {
                _fileWatcherService.StartWatching();
                _globalHotkeyService.Initialize(this);
                _shortcutService.RegisterGlobals();
            };

            Closing += (s, e) =>
            {
                _globalHotkeyService.Dispose();
                // Releases the file watcher, background services and host, then exits.
                (App.Current as App)?.Shutdown();
            };

            _shortcutService.Triggered += OnShortcutTriggered;

            // In-app shortcuts: keyboard via KeyDown, mouse buttons via bubbling PointerPressed
            // (bubble so the Settings capture handlers can intercept first while recording).
            KeyDown += Window_KeyDown;
            AddHandler(PointerPressedEvent, OnWindowPointerPressed, RoutingStrategies.Bubble);
        }

        private void OnShortcutTriggered(string commandId)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                switch (commandId)
                {
                    case UI.avalonia.Input.ShortcutCommands.NewBountyRun:
                        StartNewBountyRunDirect();
                        break;

                    case UI.avalonia.Input.ShortcutCommands.OpenEveJournal:
                        OpenEveJournal_Click(this, new RoutedEventArgs());
                        break;
                }
            });
        }

        private void OnWindowPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var button = ShortcutService.ToMouseButton(e.GetCurrentPoint(this).Properties.PointerUpdateKind);
            if (_shortcutService.HandlePointer(e.KeyModifiers, button))
                e.Handled = true;
        }

        private void StartNewBountyRunDirect()
        {
            var mainViewModel = (MainWindowViewModel)DataContext;

            // Generate default name with current time
            var runCount = mainViewModel.BountyRuns.Count + 1;
            var currentTime = DateTime.Now.ToString("h:mm tt");
            var runName = $"Run #{runCount}, {currentTime}";

            var bountyRun = new SharedLibrary.Models.BountyRun
            {
                Id = DateTime.Now.Ticks.GetHashCode(),
                Name = runName,
                StartTime = DateTime.Now,
                TotalIsk = 0,
                IsCompleted = false
            };

            mainViewModel.SetCurrentBountyRun(bountyRun);
        }

        private void Window_KeyDown(object? sender, KeyEventArgs e)
        {
            // Alt+F4 closes the borderless window (no native chrome to handle it).
            if (e.Key == Key.F4 && e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                e.Handled = true;
                Close();
                return;
            }

            if (_shortcutService.HandleKeyDown(e.KeyModifiers, e.Key))
                e.Handled = true;
        }

        public void CloseModal()
        {
            ModalOverlay.IsVisible = false;
            ModalContent.Content = null;
        }

        private void BountyRunButton_Click(object sender, RoutedEventArgs e)
        {
            var mainViewModel = (MainWindowViewModel)DataContext;

            if (mainViewModel.IsBountyRunActive)
            {
                // Stop current bounty run
                mainViewModel.StopCurrentBountyRun();
            }
            else
            {
                // Start new bounty run
                var view = _serviceProvider.GetService<BountyRunView>();
                ModalContent.Content = view;
                ModalOverlay.IsVisible = true;
            }
        }

        private void ModalOverlay_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            var point = e.GetPosition(ModalContent);
            var bounds = ModalContent.Bounds;

            // Check if click is outside modal content
            if (point.X < 0 || point.Y < 0 || point.X > bounds.Width || point.Y > bounds.Height)
            {
                CloseModal();
            }
        }

        private void StopWatching_Click(object sender, RoutedEventArgs e)
        {
            if (_isWatching)
            {
                _fileWatcherService.StopWatching();
                ((Button)sender).Content = "Start Watching";
                // Note: In Avalonia, use a proper dialog system instead of MessageBox
                // For now, we'll just update the button
            }
            else
            {
                _fileWatcherService.StartWatching();
                ((Button)sender).Content = "Stop Watching";
            }

            _isWatching = !_isWatching;
        }

        // Minimize Window
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // Maximize or Restore Window
        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        // Borderless windows have no OS maximize, and WindowState.Maximized would cover the
        // taskbar/panel. Maximize manually to the screen's working area instead so it stays
        // correct on Windows, Linux and macOS.
        private bool _isMaximized;
        private PixelPoint _restorePosition;
        private Size _restoreSize;

        private void ToggleMaximize()
        {
            if (_isMaximized)
                RestoreFromMaximize();
            else
                Maximize();
        }

        private void Maximize()
        {
            var screen = Screens?.ScreenFromWindow(this) ?? Screens?.Primary;
            if (screen is null)
                return;

            _restorePosition = Position;
            _restoreSize = Bounds.Size;

            var area = screen.WorkingArea;
            var scale = screen.Scaling;

            Position = area.Position;
            Width = area.Width / scale;
            Height = area.Height / scale;
            _isMaximized = true;
        }

        private void RestoreFromMaximize()
        {
            Position = _restorePosition;
            Width = _restoreSize.Width;
            Height = _restoreSize.Height;
            _isMaximized = false;
        }

        // Close Window
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #region Manual window resize (borderless, cross-platform)

        private bool _resizing;
        private (bool West, bool East, bool North, bool South) _resizeEdge;
        private PixelPoint _resizeStartPointer;
        private PixelPoint _resizeStartPosition;
        private Size _resizeStartSize;

        private void ResizePressed(object? sender, PointerPressedEventArgs e)
        {
            if (_isMaximized) return;
            if (sender is not Control control || control.Tag is not string tag) return;
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;

            _resizeEdge = EdgeFromTag(tag);
            _resizing = true;
            _resizeStartPointer = this.PointToScreen(e.GetPosition(this));
            _resizeStartPosition = Position;
            _resizeStartSize = Bounds.Size;
            e.Pointer.Capture(control);
            e.Handled = true;
        }

        private void ResizeMoved(object? sender, PointerEventArgs e)
        {
            if (!_resizing) return;

            var current = this.PointToScreen(e.GetPosition(this));
            double dx = current.X - _resizeStartPointer.X;
            double dy = current.Y - _resizeStartPointer.Y;

            double x = _resizeStartPosition.X;
            double y = _resizeStartPosition.Y;
            double width = _resizeStartSize.Width;
            double height = _resizeStartSize.Height;

            if (_resizeEdge.East)
                width = Math.Max(MinWidth, _resizeStartSize.Width + dx);

            if (_resizeEdge.South)
                height = Math.Max(MinHeight, _resizeStartSize.Height + dy);

            if (_resizeEdge.West)
            {
                width = Math.Max(MinWidth, _resizeStartSize.Width - dx);
                x = _resizeStartPosition.X + (_resizeStartSize.Width - width);
            }

            if (_resizeEdge.North)
            {
                height = Math.Max(MinHeight, _resizeStartSize.Height - dy);
                y = _resizeStartPosition.Y + (_resizeStartSize.Height - height);
            }

            Position = new PixelPoint((int)Math.Round(x), (int)Math.Round(y));
            Width = width;
            Height = height;
            e.Handled = true;
        }

        private void ResizeReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (!_resizing) return;
            _resizing = false;
            e.Pointer.Capture(null);
            e.Handled = true;
        }

        private static (bool West, bool East, bool North, bool South) EdgeFromTag(string tag) => tag switch
        {
            "West" => (true, false, false, false),
            "East" => (false, true, false, false),
            "North" => (false, false, true, false),
            "South" => (false, false, false, true),
            "NorthWest" => (true, false, true, false),
            "NorthEast" => (false, true, true, false),
            "SouthWest" => (true, false, false, true),
            "SouthEast" => (false, true, false, true),
            _ => (false, false, false, false)
        };

        #endregion

        // Handle Pointer Press on Title Bar
        private void TitleBar_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleMaximize();
            }
            else if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                // Dragging a maximized window restores it first, then moves.
                if (_isMaximized)
                    RestoreFromMaximize();
                BeginMoveDrag(e);
            }
        }

        private void CharacterDps_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Control { DataContext: Character character })
                return;

            // One overlay per character: re-focus an existing one instead of stacking.
            if (_characterDpsOverlays.TryGetValue(character.CharacterId, out var existing))
            {
                existing.Activate();
                return;
            }

            var vm = new DpsChartViewModel { CharacterId = character.CharacterId };

            void OnLog(object? s, LogEvent ev) => vm.ProcessLog(ev);
            _fileWatcherService.OnNewLogEvent += OnLog;

            var overlay = new DpsOverlayWindow(persistGeometry: false, headerText: $"DPS · {character.Name}")
            {
                DataContext = vm
            };

            overlay.Closed += (_, _) =>
            {
                _fileWatcherService.OnNewLogEvent -= OnLog;
                vm.Dispose();
                _characterDpsOverlays.Remove(character.CharacterId);
            };

            _characterDpsOverlays[character.CharacterId] = overlay;
            overlay.Show();
        }

        private void OpenEveJournal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var url = _serviceProvider.GetService<IConfiguration>().GetValue<string>("EveJournalUrl");
                if (!string.IsNullOrEmpty(url))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error opening EVE Journal: {ex.Message}");
            }
        }
    }
}
