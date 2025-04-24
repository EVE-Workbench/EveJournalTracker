using System.Windows;
using System.Windows.Controls;
using SharedLibrary.Models;

namespace EWB_Tracker.Views.Components
{
    public partial class CharacterAvatarControl : UserControl
    {
        public CharacterAvatarControl()
        {
            InitializeComponent();
            UpdateSizeProperties();
        }

        public static readonly DependencyProperty CharacterProperty =
            DependencyProperty.Register("Character", typeof(Character), typeof(CharacterAvatarControl), 
                new PropertyMetadata(null));

        public Character Character
        {
            get { return (Character)GetValue(CharacterProperty); }
            set { SetValue(CharacterProperty, value); }
        }

        public static readonly DependencyProperty AvatarSizeProperty =
            DependencyProperty.Register("AvatarSize", typeof(double), typeof(CharacterAvatarControl), 
                new PropertyMetadata(54.0, OnAvatarSizeChanged));

        public double AvatarSize
        {
            get { return (double)GetValue(AvatarSizeProperty); }
            set { SetValue(AvatarSizeProperty, value); }
        }

        private static void OnAvatarSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as CharacterAvatarControl;
            if (control != null)
            {
                control.UpdateSizeProperties();
            }
        }

        public static readonly DependencyProperty AvatarMarginProperty =
            DependencyProperty.Register("AvatarMargin", typeof(Thickness), typeof(CharacterAvatarControl), 
                new PropertyMetadata(new Thickness(5)));

        public Thickness AvatarMargin
        {
            get { return (Thickness)GetValue(AvatarMarginProperty); }
            set { SetValue(AvatarMarginProperty, value); }
        }

        private static readonly DependencyProperty InnerSizeProperty =
            DependencyProperty.Register("InnerSize", typeof(double), typeof(CharacterAvatarControl), 
                new PropertyMetadata(50.0));

        public double InnerSize
        {
            get { return (double)GetValue(InnerSizeProperty); }
            private set { SetValue(InnerSizeProperty, value); }
        }

        private static readonly DependencyProperty CircleRadiusProperty =
            DependencyProperty.Register("CircleRadius", typeof(double), typeof(CharacterAvatarControl), 
                new PropertyMetadata(25.0));

        public double CircleRadius
        {
            get { return (double)GetValue(CircleRadiusProperty); }
            private set { SetValue(CircleRadiusProperty, value); }
        }

        private static readonly DependencyProperty CenterPointProperty =
            DependencyProperty.Register("CenterPoint", typeof(Point), typeof(CharacterAvatarControl), 
                new PropertyMetadata(new Point(25, 25)));

        public Point CenterPoint
        {
            get { return (Point)GetValue(CenterPointProperty); }
            private set { SetValue(CenterPointProperty, value); }
        }

        // Calculating the size of the inner circle and center point
        private void UpdateSizeProperties()
        {
            InnerSize = AvatarSize - 4; // 4 is the margin
            CircleRadius = InnerSize / 2;
            CenterPoint = new Point(CircleRadius, CircleRadius);
        }
    }
}