using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NUI.Kinect
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            this.SetupGeometryHelper();
            this.SetupKinectSensor();
        }

        private Gemoetry3D geometryHelper;

        private void SetupGeometryHelper()
        {
            var pysicalDimensions = new Size(93.1, 52.4);
            var realtivePosition = new Vector3(-36, 6.5f, 0);
            var rotation = 15;

            this.geometryHelper = new Gemoetry3D(pysicalDimensions, realtivePosition, rotation);
        }

        private void SetupKinectSensor()
        {
            var kinectBodyTracker = new KinectBodyTracker();

            var screenCoordinates = kinectBodyTracker.TrackedJoints
                .Select(joints => VirtualPointer.Create(joints, this.geometryHelper))
                .Buffer(10, 1)
                .Select(buffer => this.SmoothVirtualPointer(buffer));

            screenCoordinates
                .Do(pointer => this.ShowPointer(pointer))
                .Subscribe();
        }

        private VirtualPointer SmoothVirtualPointer(IList<VirtualPointer> buffer)
        {
            var xs = buffer.Select(p => p.Position.X).OrderBy(x => x);
            var ys = buffer.Select(p => p.Position.Y).OrderBy(y => y);
            var zs = buffer.Select(p => p.Depth).OrderBy(z => z);

            var medianIndex = buffer.Count / 2;

            var smoothPosition = new Vector2(xs.ElementAt(medianIndex), ys.ElementAt(medianIndex));
            var smoothDepth = zs.ElementAt(medianIndex);

            return new VirtualPointer
            {
                Position = smoothPosition,
                Depth = smoothDepth
            };
        }

        private void ShowPointer(VirtualPointer pointer)
        {
            Canvas.SetLeft(this.FingerPointer, pointer.Position.X);
            Canvas.SetTop(this.FingerPointer, pointer.Position.Y);

            // This is kind of an assumption that you are approximately (2 x physical screen width) far away from your screen
            // but at this point we are already working with pixels instead of centimeters
            var effectiveHorizontalResolution = SystemParameters.PrimaryScreenWidth / 2;
            this.ScaleTransform.ScaleX = this.ScaleTransform.ScaleY = (pointer.Depth / effectiveHorizontalResolution);
        }
    }
}