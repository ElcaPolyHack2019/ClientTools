using System.Collections.Generic;
using System.Linq;

namespace Rover.Navigation
{
    public class PathFinder
    {
	    private Map _map;

	    public PathFinder(Map map)
	    {
		    _map = map;
	    }

        public Trajectory FindTrajectoryForOrder(Point startPoint, Point targetPoint, DuckieBot duckieBot)
        {
            var deliveryTrajectory = new Trajectory();
            targetPoint = FindNearestKnownLocation(targetPoint); // ensure that we have a point from the map as target.            

            if (new Point(duckieBot.CurrentX, duckieBot.CurrentY)
                    .CalculateDistanceInXandYPlane(startPoint) > 0.2f)
            {
                GoToStart(duckieBot, startPoint, deliveryTrajectory);
            }

            deliveryTrajectory.AddPoint(startPoint); // land at start
            BuildTrajectory(startPoint, targetPoint, deliveryTrajectory);
            deliveryTrajectory.AddPoint(new Point(targetPoint.X, targetPoint.Y)); // land at destination
            return deliveryTrajectory;
        }

        private void GoToStart(DuckieBot duckieBot, Point startPoint, Trajectory deliveryTrajectory)
        {
            var currentPoint = FindNearestKnownLocation(duckieBot);
            // go to nearest known location and from there to base
            var toBase = BuildWayPoints(new Point(currentPoint.X, currentPoint.Y), new Point(startPoint.X, startPoint.Y));
            foreach (var point in toBase)
            {
                deliveryTrajectory.AddPoint(point);
            }
        }

        private Point FindNearestKnownLocation(DuckieBot duckieBot)
        {
            return FindNearestKnownLocation(new Point(duckieBot.CurrentX, duckieBot.CurrentY));
        }

        private Point FindNearestKnownLocation(Point currentLocation)
        {
            var droneLocation = new Point(currentLocation.X, currentLocation.Y);
            var result = _map.Nodes.First().Point;            
            foreach (var node in _map.Nodes)
            {
                if (droneLocation.CalculateDistanceInXandYPlane(node.Point) <
                    droneLocation.CalculateDistanceInXandYPlane(result))
                {
                    result = node.Point;
                }
            }
            return new Point(result.X, result.Y);
        }

        private void BuildTrajectory(Point startPoint, Point destinationPoint, Trajectory deliveryTrajectory)
        {
            var navPoints = BuildWayPoints(new Point(startPoint.X, startPoint.Y), new Point(destinationPoint.X, destinationPoint.Y));
            foreach (var navPoint in navPoints)
            {
                deliveryTrajectory.AddPoint(navPoint);
            }

            var target = navPoints.Last();
        }

        private List<Point> BuildWayPoints(Point start, Point target)
        {
            var result = new List<Point>();
            var searchEngine = new SearchEngine(_map);
            var navPoints = searchEngine.GetShortestPathDijikstra();

            foreach (var navPoint in navPoints)
            {
                result.Add(new Point(navPoint.Point.X, navPoint.Point.Y));
            }

            return result;
        }
    }
}
