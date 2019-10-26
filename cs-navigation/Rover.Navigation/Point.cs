using System;

namespace Rover.Navigation
{
    public class Point
    {        

        public Point(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X
        {
            get;
        }

        public float Y
        {
            get;
        }


        public override bool Equals(object obj)
        {
            var point = obj as Point;
            return point != null &&
                   X == point.X &&
                   Y == point.Y;
        }
        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"{X} / {Y}";
        }

        public float CalculateDistanceInXandYPlane(Point other)
        {
            return (float) Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        }

        public Point CalculateReflectionPointInXandYPlane(float angleInRadians, float distance)
        {

	        var xResult = X + distance * Math.Cos(angleInRadians);
	        var yResult = Y + distance * Math.Sin(angleInRadians);
	        return new Point((float)xResult, (float)yResult);
        }
	}
}
