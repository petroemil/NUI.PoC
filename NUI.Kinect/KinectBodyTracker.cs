using Microsoft.Kinect;
using System;
using System.Linq;
using System.Numerics;
using System.Reactive.Linq;

namespace NUI.Kinect
{
    public class JointPositions
    {
        public Vector3 Head { get; set; }
        public Vector3 Hand { get; set; }
    }

    public class KinectBodyTracker
    {
        private readonly KinectSensor kinectSensor;

        public IObservable<JointPositions> TrackedJoints { get; private set; }

        public KinectBodyTracker()
        {
            this.kinectSensor = KinectSensor.GetDefault();
            this.kinectSensor.Open();

            var bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

            this.TrackedJoints = Observable
                .FromEventPattern<BodyFrameArrivedEventArgs>(bodyFrameReader, nameof(bodyFrameReader.FrameArrived))
                .Select(e => this.ExtractBody(e.EventArgs))
                .Where(body => body != null)
                .Where(body => body.Joints.ContainsKey(JointType.HandTipRight) && body.Joints.ContainsKey(JointType.Head))
                .Select(body => this.ExtractJoints(body));
        }

        private Body ExtractBody(BodyFrameArrivedEventArgs args)
        {
            using (var frame = args.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    var bodies = new Body[frame.BodyCount];
                    frame.GetAndRefreshBodyData(bodies);

                    // ASSUMPTION: We are just tracking 1 body
                    var theOneBody = bodies.FirstOrDefault(x => x.IsTracked);
                    return theOneBody;
                }
            }

            return null;
        }

        private JointPositions ExtractJoints(Body body)
        {
            // ASSUMPTION:
            // When the user is pointing to the screen with their index finger,
            // they actually cover the area from their point of view with the tip of their finger
            // meaning we can get the point on the screen by getting the line defined by the users head and finger
            // and finding its intersection with the plane of the display
            var hand = body.Joints[JointType.HandTipRight];
            var head = body.Joints[JointType.Head];

            // originally it's in meters and Y axis have to be negated
            //var handPosition = new Vector3(hand.Position.X, -hand.Position.Y, hand.Position.Z) * 100;
            //var headPosition = new Vector3(head.Position.X, -head.Position.Y, head.Position.Z) * 100;

            var handPosition = new Vector3(hand.Position.X, hand.Position.Y, hand.Position.Z);
            var headPosition = new Vector3(head.Position.X, head.Position.Y, head.Position.Z);

            return new JointPositions
            {
                Hand = handPosition,
                Head = headPosition
            };
        }
    }
}
