using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EWB_Tracker.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using SharedLibrary.Models;

namespace EWB_Tracker.Views
{
    public partial class AccountView : UserControl
    {
        private bool _isDragging = false;
        private Character _draggedItem = null;

        public AccountView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<AccountViewModel>();
        }

        private void ListBoxItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1 && sender is ListBoxItem item)
            {
                // Start dragging only on the left mouse button
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    _isDragging = true;
                    _draggedItem = item.DataContext as Character;
                    DragDrop.DoDragDrop(item, _draggedItem, DragDropEffects.Move);
                    _isDragging = false;
                }
            }
        }

        private void ListBoxItem_DragOver(object sender, DragEventArgs e)
        {
            if (!_isDragging || _draggedItem == null)
                return;

            if (sender is ListBoxItem item && item.DataContext is Character targetItem && !ReferenceEquals(_draggedItem, targetItem))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void ListBoxItem_Drop(object sender, DragEventArgs e)
        {
            if (!_isDragging || _draggedItem == null)
                return;

            if (sender is ListBoxItem item && item.DataContext is Character targetItem && !ReferenceEquals(_draggedItem, targetItem))
            {
                var viewModel = DataContext as AccountViewModel;
                viewModel?.OnCharacterDrop(_draggedItem, targetItem);
            }

            _draggedItem = null;
            e.Handled = true;
        }
    }
}