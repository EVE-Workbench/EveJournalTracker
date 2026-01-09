using Avalonia;
using Avalonia.Controls;
using SharedLibrary.Models;

namespace UI.avalonia.Views.Components
{
    public partial class CharacterAvatarControl : UserControl
    {
        public CharacterAvatarControl()
        {
            InitializeComponent();
            UpdateSizeProperties();
        }

        public static readonly StyledProperty<Character> CharacterProperty =
            AvaloniaProperty.Register<CharacterAvatarControl, Character>(nameof(Character));

        public Character Character
        {
            get => GetValue(CharacterProperty);
            set
            {
                SetValue(CharacterProperty, value);
                if (value != null)
                {
                    System.Console.WriteLine($"[CharacterAvatarControl] Character set: {value.Name} (ID: {value.CharacterId})");
                }
            }
        }

        public static readonly StyledProperty<double> AvatarSizeProperty =
            AvaloniaProperty.Register<CharacterAvatarControl, double>(nameof(AvatarSize), 54.0);

        public double AvatarSize
        {
            get => GetValue(AvatarSizeProperty);
            set
            {
                SetValue(AvatarSizeProperty, value);
                UpdateSizeProperties();
            }
        }

        public static readonly StyledProperty<Thickness> AvatarMarginProperty =
            AvaloniaProperty.Register<CharacterAvatarControl, Thickness>(nameof(AvatarMargin), new Thickness(5));

        public Thickness AvatarMargin
        {
            get => GetValue(AvatarMarginProperty);
            set => SetValue(AvatarMarginProperty, value);
        }

        private static readonly StyledProperty<double> InnerSizeProperty =
            AvaloniaProperty.Register<CharacterAvatarControl, double>(nameof(InnerSize), 50.0);

        public double InnerSize
        {
            get => GetValue(InnerSizeProperty);
            private set => SetValue(InnerSizeProperty, value);
        }

        private static readonly StyledProperty<double> CircleRadiusProperty =
            AvaloniaProperty.Register<CharacterAvatarControl, double>(nameof(CircleRadius), 25.0);

        public double CircleRadius
        {
            get => GetValue(CircleRadiusProperty);
            private set => SetValue(CircleRadiusProperty, value);
        }

        private static readonly StyledProperty<Point> CenterPointProperty =
            AvaloniaProperty.Register<CharacterAvatarControl, Point>(nameof(CenterPoint), new Point(25, 25));

        public Point CenterPoint
        {
            get => GetValue(CenterPointProperty);
            private set => SetValue(CenterPointProperty, value);
        }

        private void UpdateSizeProperties()
        {
            InnerSize = AvatarSize - 4;
            CircleRadius = InnerSize / 2;
            CenterPoint = new Point(CircleRadius, CircleRadius);
        }
    }
}
