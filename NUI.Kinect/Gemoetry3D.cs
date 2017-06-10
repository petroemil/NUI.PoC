using System;
using System.Numerics;
using System.Windows;

namespace NUI.Kinect
{
    public class Gemoetry3D
    {
        private readonly Matrix4x4 inverseTransformMatrix;

        public Gemoetry3D(Size physicalDimensions, Vector3 relativePosition, float rotationInDegree)
        {
            // This is not necessarily the native resolution of the display
            // but the effective resolution after taking the DPI scaling into account
            var effectiveResolution = new Size(
                SystemParameters.PrimaryScreenWidth,
                SystemParameters.PrimaryScreenHeight);

            // Scale: pixel -> cm
            var widthScale = physicalDimensions.Width / effectiveResolution.Width;
            var heightScale = physicalDimensions.Height / effectiveResolution.Height;
            var scale = (widthScale + heightScale) / 2.0; // get the average from the 2 numbers - just in case

            // Rotation (rad)
            var radian = (float)(Math.PI * rotationInDegree / 180.0);

            var translateMatrix = Matrix4x4.CreateTranslation(relativePosition);
            var scaleMatrix = Matrix4x4.CreateScale((float)scale);
            var rotationMatrix = Matrix4x4.CreateRotationX(radian);

            // Kinect scale: cm -> m
            var kinectScaleMatrix = Matrix4x4.CreateScale(0.01f);
            var kinectMirrorMatrix = Matrix4x4.CreateReflection(new Plane(0, 1, 0, 1));
            var kinectAdjustmentMatrix = kinectMirrorMatrix * kinectScaleMatrix;

            var combinedTransformMatrix = scaleMatrix * translateMatrix * rotationMatrix * kinectAdjustmentMatrix;

            // I managed to make the matrix transformation work with these values, in this order, and with the inversion
            // I don't know how to turn things around to make it work without this extra step of inversion
            Matrix4x4.Invert(combinedTransformMatrix, out this.inverseTransformMatrix);
        }

        private Vector3 TranslateRealWorldCoordinate(Vector3 realWorldCoordinate)
        {
            var result = Vector3.Transform(realWorldCoordinate, this.inverseTransformMatrix);
            return result;
        }

        // Intersection
        // Translate the head and hand coordinates to the "Virtual World" and then we can make the intersection logic much simpler
        // we just have to check where pointing vector intersects the X/Y planes, or where will it have Z = 0
        // x = looking from the top -> x=Z, y=X
        // y = looking from the side -> x=Z, y=Y
        public Vector2 Intersect(Vector3 headRealWorldPosition, Vector3 handRealWorldPosition)
        {
            var head = this.TranslateRealWorldCoordinate(headRealWorldPosition);
            var hand = this.TranslateRealWorldCoordinate(handRealWorldPosition);

            var intersectionX = this.IntersectsYAxis(hand.Z, hand.X, head.Z, head.X);
            var intersectionY = this.IntersectsYAxis(hand.Z, hand.Y, head.Z, head.Y);

            return new Vector2(intersectionX, intersectionY);
        }

        // if we assume that there are two points: A(x1, y1) and B(x2, y2)
        // and we are looking for a C point where the line defined by them intersects the Y axis (x = 0) -> C(0, y3)
        // then it's true that
        // (y1 - y2) / (x1 - x2) = (y3 - y1) / (0 - x1)
        // solving this for y3 we get the following:
        private float IntersectsYAxis(float x1, float y1, float x2, float y2)
        {
            var y3 = -x1 * ((y1 - y2) / (x1 - x2)) + y1;
            return y3;
        }
    }
}