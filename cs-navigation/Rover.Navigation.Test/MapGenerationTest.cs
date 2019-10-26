using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Rover.Navigation.Test
{
	[TestFixture]
    public class MapGenerationTest
    {

	    [Test]
	    public void GenerateSimpleMap()
	    {
		    var map = new Map(new Point(-2, -2), new Point(2,2), 2f);
		    AssertNode(map, new Point(-1, -1));
		    AssertNode(map, new Point(1, -1));
		    AssertNode(map, new Point(-1, 1));
		    AssertNode(map, new Point(1, 1));

		    AssertEdge(map, $"Node_0_0", $"Node_1_0");
		    AssertEdge(map, $"Node_0_0", $"Node_0_1");
		    AssertEdge(map, $"Node_1_0", $"Node_1_1");
		    AssertEdge(map, $"Node_0_1", $"Node_1_1");

		    AssertEdge(map, $"Node_1_0", $"Node_0_0");
		    AssertEdge(map, $"Node_0_1", $"Node_0_0");
		    AssertEdge(map, $"Node_1_1", $"Node_1_0");
		    AssertEdge(map, $"Node_1_1", $"Node_0_1");

		}

	    private void AssertEdge(Map map, string nodeSource, string nodeTarget)
	    {
		    var source = map.Nodes.First(x => x.Name == nodeSource);
		    var target = source.Connections.FirstOrDefault(x => x.ConnectedNode.Name == nodeTarget);
		    target.Should().NotBe(null);
	    }

	    private void AssertNode(Map map, Point point)
		{
			map.Nodes.FirstOrDefault(x => x.Point.Equals(point)).Should().NotBe(null);
		}
	}
}
