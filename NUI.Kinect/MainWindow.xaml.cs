using System;
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
                .Select(joints => this.geometryHelper.Intersect(joints.Head, joints.Hand))
                .Buffer(10, 1)
                .Select(buffer =>
                {
                    var xs = buffer.Select(x => x.X).OrderBy(x => x);
                    var ys = buffer.Select(x => x.Y).OrderBy(x => x);

                    var medianIndex = buffer.Count / 2;
                    return new Vector2(xs.ElementAt(medianIndex), ys.ElementAt(medianIndex));
                });

            screenCoordinates
                .Do(position =>
                {
                    Canvas.SetLeft(this.FingerPointer, position.X);
                    Canvas.SetTop(this.FingerPointer, position.Y);
                })
                .Subscribe();
        }
    }
}