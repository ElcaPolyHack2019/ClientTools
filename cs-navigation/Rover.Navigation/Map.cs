using System;
using System.Collections.Generic;
using System.Linq;

namespace Rover.Navigation
{



    public class Map
    {

		// constructs a map with squares of the given size in the given area.
	    public Map(Point topLeft, Point bottomRight, float squareLength)
	    {
			var topRight = new Point(bottomRight.X, topLeft.Y);
			var x = topLeft.CalculateDistanceInXandYPlane(topRight);
			var bottomLeft = new Point(topLeft.X, bottomRight.Y);
			var y = topLeft.CalculateDistanceInXandYPlane(bottomLeft);

			var numberOfRectanglesX = (int) Math.Ceiling(x / squareLength);
			var numberOfRectanglesY = (int) Math.Ceiling(y / squareLength);

			Generate(topLeft, numberOfRectanglesX, numberOfRectanglesY, squareLength);

		}

	    private void Generate(Point topLeft, int squaresX, int squaresY, float squareLength)
	    {
		    var currentX = topLeft.X;
		    var currentY = topLeft.Y;
		    for (int i = 0; i < squaresX; i++)
		    {
			    for (int j = 0; j < squaresY; j++)
			    {
				    var toAdd = new Point(currentX + (squareLength / 2), currentY - (squareLength / 2));
				    AddNode(GetNodeId(i, j), toAdd);
				    currentY -= squareLength;
			    }

			    currentX += squareLength;
			    currentY = topLeft.Y;
		    }

		    for (int i = 0; i < squaresX; i++)
		    {
			    for (int j = 0; j < squaresY; j++)
			    {
				    // add node on the bottom
					if (j < squaresY - 1)
				    {
					    AddEdge(GetNodeId(i, j), GetNodeId(i, j + 1), squareLength);
					}
					// add node on the right
					if (i < squaresX - 1)
					{
						AddEdge(GetNodeId(i, j), GetNodeId(i + 1, j), squareLength);
					}
				}
		    }
	    }

	    private static string GetNodeId(int i, int j)
	    {
		    return $"Node_{i}_{j}";
	    }

	    private void AddEdge(string node1, string node2, float cost)
        {
            var sourceNode = Nodes.First(x => x.Name == node1);
            var targetNode = Nodes.First(x => x.Name == node2);
            sourceNode.Connections.Add(new Edge() {ConnectedNode = targetNode, Cost = cost, Length = cost});
            targetNode.Connections.Add(new Edge() {ConnectedNode = sourceNode, Cost = cost, Length = cost});
        }

        private void AddNode(string name, Point point)
        {
            var node = new Node() {Name = name, Connections = new List<Edge>(), Id = Guid.NewGuid(), Point = point};
            Nodes.Add(node);
        }

        public List<Node> Nodes { get; set; } = new List<Node>();

        public Node StartNode { get; set; }

        public Node EndNode { get; set; }

        public List<Node> ShortestPath { get; set; } = new List<Node>();

        public void ApplySuspectUnreachable(List<Node> unreachableNodes)
        {
	        foreach (var node in unreachableNodes)
	        {
		        foreach (var connection in node.Connections)
		        {
			        var backConnection = 
				        connection.ConnectedNode.Connections.Find(x => x.ConnectedNode.Name == node.Name);
			        backConnection.Cost = backConnection.Cost + 1;
			        connection.Cost = connection.Cost + 1;
		        }
	        }
        }

        public List<Node> FindKnownLocationsInRange(Point point, float nodeDistanceLimit)
        {
	        var result = new List<Node>();
	        foreach (var node in Nodes)
	        {
		        if (point.CalculateDistanceInXandYPlane(node.Point) <
		            nodeDistanceLimit)
		        {
			        result.Add(node);
		        }
	        }

	        return result;
        }

		public void ApplySuspectUnreachable(List<Point> points, float nodeDistanceLimit)
		{
			var result = new List<Node>();
			// find all the points with a distance of < nodeDistanceLimit and mark them as
			// suspect unreachable.
			foreach (var point in points)
			{
				result.AddRange(FindKnownLocationsInRange(point, nodeDistanceLimit));
			}
			ApplySuspectUnreachable(result);
		}
    }

    public class Node
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Point Point { get; set; }
        public List<Edge> Connections { get; set; } = new List<Edge>();

        public double? MinCostToStart { get; set; }
        public Node NearestToStart { get; set; }
        public bool Visited { get; set; }
        public double StraightLineDistanceToEnd { get; set; }


        internal void ConnectClosestNodes(List<Node> nodes, int branching, Random rnd, bool randomWeight)
        {
            var connections = new List<Edge>();
            foreach (var node in nodes)
            {
                if (node.Id == this.Id)
                    continue;

                var dist = Math.Sqrt(Math.Pow(Point.X - node.Point.X, 2) + Math.Pow(Point.Y - node.Point.Y, 2));
                connections.Add(new Edge
                {
                    ConnectedNode = node,
                    Length = dist,
                    Cost = randomWeight ? rnd.NextDouble() : dist,
                });
            }

            connections = connections.OrderBy(x => x.Length).ToList();
            var count = 0;
            foreach (var cnn in connections)
            {
                //Connect three closes nodes that are not connected.
                if (!Connections.Any(c => c.ConnectedNode == cnn.ConnectedNode))
                    Connections.Add(cnn);
                count++;

                //Make it a two way connection if not already connected
                if (!cnn.ConnectedNode.Connections.Any(cc => cc.ConnectedNode == this))
                {
                    var backConnection = new Edge {ConnectedNode = this, Length = cnn.Length};
                    cnn.ConnectedNode.Connections.Add(backConnection);
                }

                if (count == branching)
                    return;
            }
        }

        public double StraightLineDistanceTo(Node end)
        {
            return Math.Sqrt(Math.Pow(Point.X - end.Point.X, 2) + Math.Pow(Point.Y - end.Point.Y, 2));
        }

        internal bool ToCloseToAny(List<Node> nodes)
        {
            foreach (var node in nodes)
            {
                var d = Math.Sqrt(Math.Pow(Point.X - node.Point.X, 2) + Math.Pow(Point.Y - node.Point.Y, 2));
                if (d < 0.01)
                    return true;
            }

            return false;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class Edge
    {
        public double Length { get; set; }
        public double Cost { get; set; }
        public Node ConnectedNode { get; set; }

        public override string ToString()
        {
            return "-> " + ConnectedNode.ToString();
        }
    }

}
 