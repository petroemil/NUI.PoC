using System;
using System.Reactive.Linq;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NUI.Touchpad
{
    public sealed partial class MainPage : Page
    {
        private readonly Brush activeBrush = new SolidColorBrush(Colors.Orange);
        private readonly Brush inactiveBrush = new SolidColorBrush(Colors.Black);

        public MainPage()
        {
            this.InitializeComponent();

            var magic = new MagicClass(this);

            magic.MouseMoved
                .Do(deltaMovement =>
                {
                    var left = Canvas.GetLeft(this.VirtualCursor);
                    var top = Canvas.GetTop(this.VirtualCursor);

                    left += deltaMovement.X;
                    top += deltaMovement.Y;

                    Canvas.SetLeft(this.VirtualCursor, left);
                    Canvas.SetTop(this.VirtualCursor, top);
                })
                .Subscribe();

            magic.LeftMouseButtonPressed
                .Do(_ => this.LeftFinger.Fill = this.activeBrush)
                .Subscribe();

            magic.LeftMouseButtonReleased
                .Do(_ => this.LeftFinger.Fill = this.inactiveBrush)
                .Subscribe();

            magic.RightMouseButtonPressed
                .Do(_ => this.RightFinger.Fill = this.activeBrush)
                .Subscribe();

            magic.RightMouseButtonReleased
                .Do(_ => this.RightFinger.Fill = this.inactiveBrush)
                .Subscribe();
        }
    }
}
