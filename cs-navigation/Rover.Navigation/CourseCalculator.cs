using System;

namespace Rover.Navigation
{
    public class CourseCalculator
    {

        /// <summary>
        /// Calculates the destination point. The angle represents the direction measured from positive x axis.
        /// </summary>
        /// <returns></returns>
        public Point CalculateDestinationInXandYPlane(Point source, float angleDegrees, float distance)
        {
            double angleInRadians = Math.PI * angleDegrees / 180.0;

            var xResult = source.X + distance * Math.Cos(angleInRadians);
            var yResult = source.Y + distance * Math.Sin(angleInRadians);
            return new Point((float)xResult, (float)yResult);
        }

        /// <summary>
        /// Calculate the angle between positive x axis and vector source -> target
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public float CalculateAngleInXandYPlan(Point source, Point target)
        {
            if (source.Equals(target))
            {
                return 0;
            }

            var angleInRadians = Math.Atan2(target.Y - source.Y, target.X - source.X);
            var angleInDegrees = angleInRadians * 180 / Math.PI;
            if (angleInDegrees < 0)
            {
                angleInDegrees += 360;
            }
            return (float)angleInDegrees;
        }

        public Point CalculateIntersection(Point line1Start, Point line1End, Point line2Start, Point line2End)
        {
            
                float s1_x, s1_y, s2_x, s2_y;
                s1_x = line1End.X - line1Start.X; s1_y = line1End.Y - line1Start.Y;
                s2_x = line2End.X - line2Start.X; s2_y = line2End.Y - line2Start.Y;

                float s, t;
                s = (-s1_y * (line1Start.X - line2Start.X) + s1_x * (line1Start.Y - line2Start.Y)) / (-s2_x * s1_y + s1_x * s2_y);
                t = (s2_x * (line1Start.Y - line2Start.Y) - s2_y * (line1Start.X - line2Start.X)) / (-s2_x * s1_y + s1_x * s2_y);

                if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
                {
                    // Collision detected
                    var collisionPoint = new Point(line1Start.X + (t * s1_x), line1Start.Y + (t * s1_y));
                    return collisionPoint;
                }

                return null; // No collision
           
        }

    }
}
