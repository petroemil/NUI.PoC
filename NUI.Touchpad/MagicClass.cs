using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reactive;
using System.Reactive.Linq;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace NUI.Touchpad
{
    public enum TouchEventType
    {
        Pressed,
        Moved,
        Released,
        Idle
    }

    public enum FingerType
    {
        Unknown,
        Left,
        Middle,
        Right
    }

    public class TouchEvent
    {
        public uint Id { get; set; }
        public TouchEventType Type { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 DeltaMovement { get; set; }
        public FingerType Finger { get; set; }
    }

    public class MagicClass
    {
        private readonly UIElement referenceVisual;

        public IObservable<Vector2> MouseMoved { get; private set; }

        public IObservable<Unit> LeftMouseButtonPressed { get; private set; }
        public IObservable<Unit> LeftMouseButtonReleased { get; private set; }

        public IObservable<Unit> RightMouseButtonPressed { get; private set; }
        public IObservable<Unit> RightMouseButtonReleased { get; private set; }

        public MagicClass(UIElement referenceVisual)
        {
            this.referenceVisual = referenceVisual;

            var fingerPressed = Observable
                .FromEventPattern<PointerRoutedEventArgs>(this.referenceVisual, nameof(this.referenceVisual.PointerPressed))
                .Where(e => e.EventArgs.Pointer.PointerDeviceType == PointerDeviceType.Touch)
                .Select(e => this.ConvertFromPointerRoutedEventArgs(e.EventArgs, TouchEventType.Pressed));

            var fingerMoved = Observable
                .FromEventPattern<PointerRoutedEventArgs>(this.referenceVisual, nameof(this.referenceVisual.PointerMoved))
                .Where(e => e.EventArgs.Pointer.PointerDeviceType == PointerDeviceType.Touch)
                .Select(e => this.ConvertFromPointerRoutedEventArgs(e.EventArgs, TouchEventType.Moved));

            var fingerReleased = Observable
                .FromEventPattern<PointerRoutedEventArgs>(this.referenceVisual, nameof(this.referenceVisual.PointerReleased))
                .Where(e => e.EventArgs.Pointer.PointerDeviceType == PointerDeviceType.Touch)
                .Select(e => this.ConvertFromPointerRoutedEventArgs(e.EventArgs, TouchEventType.Released));

            var grouppedFingerEvents = Observable
                .Merge(fingerPressed, fingerMoved, fingerReleased)
                .Scan(new List<TouchEvent>(), this.DoMagic)
                .Publish()
                .RefCount();

            this.MouseMoved = grouppedFingerEvents
                .Select(activeFingers => activeFingers.SingleOrDefault(e => e.Finger == FingerType.Middle && e.Type == TouchEventType.Moved))
                .Where(finger => finger != null)
                .Select(finger => finger.DeltaMovement);

            this.LeftMouseButtonPressed = grouppedFingerEvents
                .Select(activeFingers => activeFingers.SingleOrDefault(e => e.Finger == FingerType.Left && e.Type == TouchEventType.Pressed))
                .Where(finger => finger != null)
                .Select(x => Unit.Default);

            this.LeftMouseButtonReleased = grouppedFingerEvents
                .Select(activeFingers => activeFingers.SingleOrDefault(e => e.Finger == FingerType.Left && e.Type == TouchEventType.Released))
                .Where(finger => finger != null)
                .Select(x => Unit.Default);

            this.RightMouseButtonPressed = grouppedFingerEvents
                .Select(activeFingers => activeFingers.SingleOrDefault(e => e.Finger == FingerType.Right && e.Type == TouchEventType.Pressed))
                .Where(finger => finger != null)
                .Select(x => Unit.Default);

            this.RightMouseButtonReleased = grouppedFingerEvents
                .Select(activeFingers => activeFingers.SingleOrDefault(e => e.Finger == FingerType.Right && e.Type == TouchEventType.Released))
                .Where(finger => finger != null)
                .Select(x => Unit.Default);
        }
        
        private TouchEvent ConvertFromPointerRoutedEventArgs(PointerRoutedEventArgs args, TouchEventType eventType)
        {
            var fingerEvent = new TouchEvent
            {
                Type = eventType,
                Id = args.Pointer.PointerId,
                Position = args.GetCurrentPoint(this.referenceVisual).Position.ToVector2()
            };

            return fingerEvent;
        }

        private List<TouchEvent> DoMagic(List<TouchEvent> activeFingers, TouchEvent newFinger)
        {
            // Remove PointerEvents flagged as "Released"
            activeFingers.RemoveAll(e => e.Type == TouchEventType.Released);

            // Set everything to "Idle"
            // So there is always just one "active" event in the list
            activeFingers.ForEach(e => e.Type = TouchEventType.Idle);

            if (newFinger.Type == TouchEventType.Pressed)
            {
                // Detect finger using the current list of Pointers and the new one that JUST produced the Pressed event
                newFinger.Finger =
                       this.TryDetectFirstFinger(activeFingers, newFinger)
                    ?? this.TryDetectSecondFinger(activeFingers, newFinger)
                    ?? this.TryDetectThirdFinger(activeFingers, newFinger)
                    ?? FingerType.Unknown;

                activeFingers.Add(newFinger);
            }
            else if (newFinger.Type == TouchEventType.Moved)
            {
                var activeFinger = activeFingers.Single(e => e.Id == newFinger.Id);

                // Detect movement
                activeFinger.DeltaMovement = newFinger.Position - activeFinger.Position;

                activeFinger.Position = newFinger.Position;
                activeFinger.Type = TouchEventType.Moved;
            }
            else if (newFinger.Type == TouchEventType.Released)
            {
                var activeFinger = activeFingers.Single(e => e.Id == newFinger.Id);
                activeFinger.Type = TouchEventType.Released;
            }

            return activeFingers;
        }

        private FingerType? TryDetectFirstFinger(List<TouchEvent> activeFingers, TouchEvent newFinger)
        {
            // MIDDLE DOWN
            // ASSUMPTION - The first detected finger is always assumed to be the middle finger
            if (activeFingers.Count == 0)
            {
                return FingerType.Middle;
            }

            return null;
        }

        private FingerType? TryDetectSecondFinger(List<TouchEvent> activeFingers, TouchEvent newFinger)
        {
            if (activeFingers.Count == 1)
            {
                var activeFinger = activeFingers.Single();

                if (activeFinger.Finger == FingerType.Middle)
                {
                    // LEFT DOWN
                    if (newFinger.Position.X < activeFinger.Position.X) return FingerType.Left;

                    // RIGHT DOWN 
                    if (newFinger.Position.X > activeFinger.Position.X) return FingerType.Right;
                }
                else
                {
                    // MIDDLE DOWN
                    // ASSUMPTION - If the Left or Right finger was already down, it is assumed that the second finger is the Middle finger
                    return FingerType.Middle;
                }
            }

            return null;
        }

        private FingerType? TryDetectThirdFinger(List<TouchEvent> activeFingers, TouchEvent newFinger)
        {
            if (activeFingers.Count == 2)
            {
                var activeLeftFinger = activeFingers.SingleOrDefault(x => x.Finger == FingerType.Left);
                var activeMiddleFinger = activeFingers.SingleOrDefault(x => x.Finger == FingerType.Middle);
                var activeRightFinger = activeFingers.SingleOrDefault(x => x.Finger == FingerType.Right);

                // LEFT + MIDDLE -> RIGHT
                if (activeLeftFinger != null && activeMiddleFinger != null) return FingerType.Right;

                // LEFT + RIGHT -> MIDDLE
                if (activeLeftFinger != null && activeRightFinger != null) return FingerType.Middle;

                // MIDDLE + RIGHT -> LEFT
                if (activeMiddleFinger != null && activeRightFinger != null) return FingerType.Left;
            }

            return null;
        }
    }
}
